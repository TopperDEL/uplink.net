using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

namespace uplink.NET.Models
{
    /// <summary>
    /// Gets raised to inform about a change within the upload-operation progress
    /// </summary>
    /// <param name="uploadOperation">The UploadOperation that changed</param>
    public delegate void UploadOperationProgressChanged(UploadOperation uploadOperation);
    /// <summary>
    /// Gets raised to inform about an ended UploadOperation.
    /// </summary>
    /// <param name="uploadOperation">The UploadOperation that ended</param>
    public delegate void UploadOperationEnded(UploadOperation uploadOperation);
    public class UploadOperation : IDisposable
    {
        private static Mutex mut = new Mutex();
        private byte[] _bytesToUpload;
        private SWIG.UploadResult _uploadResult;
        private Task _uploadTask;
        private bool _cancelled;
        private CustomMetadata _customMetadata;

        /// <summary>
        /// The name of the object uploading
        /// </summary>
        public string ObjectName { get; private set; }
        /// <summary>
        /// Informs about upload-operation progress changes
        /// </summary>
        public event UploadOperationProgressChanged UploadOperationProgressChanged;
        /// <summary>
        /// Inform about a UploadOperation that ended (i.e. Completed, Failed or got Cancelled)
        /// </summary>
        public event UploadOperationEnded UploadOperationEnded;
        /// <summary>
        /// The - until now - sent bytes
        /// </summary>
        public long BytesSent { get; private set; }
        /// <summary>
        /// The total bytes to send
        /// </summary>
        public long TotalBytes
        {
            get
            {
                if (_bytesToUpload != null)
                    return _bytesToUpload.Length;
                else
                    return 0;
            }
        }
        /// <summary>
        /// Is the upload completed?
        /// </summary>
        public bool Completed { get; private set; }
        /// <summary>
        /// Did the upload fail? See ErrorMessage for details.
        /// </summary>
        public bool Failed { get; set; }
        /// <summary>
        /// Got the upload cancelled (by the user)?
        /// </summary>
        public bool Cancelled { get; set; }
        /// <summary>
        /// Is the upload currently in progress?
        /// </summary>
        public bool Running { get; set; }
        private string _errorMessage;
        /// <summary>
        /// The possible error - only filled if "Failed" is true.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
        }
        /// <summary>
        /// The percentage of completeness
        /// </summary>
        public float PercentageCompleted
        {
            get
            {
                return (float)BytesSent / (float)TotalBytes * 100f;
            }
        }

        internal UploadOperation(byte[] bytestoUpload, SWIG.UploadResult uploadResult, string objectName, CustomMetadata customMetadata = null)
        {
            _bytesToUpload = bytestoUpload;
            _uploadResult = uploadResult;
            ObjectName = objectName;
            _customMetadata = customMetadata;

            if (uploadResult.error != null && !string.IsNullOrEmpty(uploadResult.error.message))
            {
                _errorMessage = uploadResult.error.message;
                Failed = true;
                Running = false;
            }
        }

        /// <summary>
        /// Starts the upload if it is not yet running, completed, cancelled or failed
        /// </summary>
        /// <returns></returns>
        public Task StartUploadAsync()
        {
            if (Completed || Failed || Cancelled)
            {
                UploadOperationEnded?.Invoke(this);
                return null;
            }

            if (_uploadTask == null)
                _uploadTask = Task.Run(DoUpload);
            return _uploadTask;
        }

        /// <summary>
        /// Cancelles the upload progress
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }

        private void DoUpload()
        {
            try
            {
                if (_bytesToUpload != null)
                {
                    Running = true;
                    while (BytesSent < _bytesToUpload.Length)
                    {
                        var tenth = _bytesToUpload.Length / 10;
                        if (_bytesToUpload.Length - BytesSent > tenth)
                        {
                            //Send next bytes in batch
                            SWIG.WriteResult sentResult = SWIG.storj_uplink.upload_write(_uploadResult.upload, new SWIG.SWIGTYPE_p_void(System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(_bytesToUpload.Skip((int)BytesSent).Take(tenth).ToArray(), 0), true), (uint)tenth);
                            if (sentResult.error != null && !string.IsNullOrEmpty(sentResult.error.message))
                            {
                                _errorMessage = sentResult.error.message;
                                Failed = true;
                                Running = false;
                                UploadOperationEnded?.Invoke(this);
                                return;
                            }
                            else
                                BytesSent += sentResult.bytes_written;
                            SWIG.storj_uplink.free_write_result(sentResult);
                        }
                        else
                        {
                            //Send only the remaining bytes
                            var remaining = _bytesToUpload.Length - BytesSent;
                            SWIG.WriteResult sentResult = SWIG.storj_uplink.upload_write(_uploadResult.upload, new SWIG.SWIGTYPE_p_void(System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(_bytesToUpload.Skip((int)BytesSent).Take((int)remaining).ToArray(), 0), true), (uint)remaining);
                            if (sentResult.error != null && !string.IsNullOrEmpty(sentResult.error.message))
                            {
                                _errorMessage = sentResult.error.message;
                                Failed = true;
                                Running = false;
                                UploadOperationEnded?.Invoke(this);
                                return;
                            }
                            else
                                BytesSent += sentResult.bytes_written;
                            SWIG.storj_uplink.free_write_result(sentResult);
                        }
                        if (_cancelled)
                        {
                            SWIG.Error abortError = SWIG.storj_uplink.upload_abort(_uploadResult.upload);
                            if (abortError != null && !string.IsNullOrEmpty(abortError.message))
                            {
                                Failed = true;
                                _errorMessage = abortError.message;
                            }
                            else
                                Cancelled = true;
                            SWIG.storj_uplink.free_error(abortError);

                            Running = false;
                            UploadOperationEnded?.Invoke(this);
                            return;
                        }
                        UploadOperationProgressChanged?.Invoke(this);
                        if (!string.IsNullOrEmpty(_errorMessage))
                        {
                            Failed = true;
                            Running = false;
                            UploadOperationEnded?.Invoke(this);
                            return;
                        }
                    }

                    if (_customMetadata != null)
                    {
                        if (mut.WaitOne(1000))
                        {
                            try
                            {
                                SWIG.Error customMetadataError = SWIG.storj_uplink.upload_set_custom_metadata(_uploadResult.upload, _customMetadata.ToSWIG());
                                if (customMetadataError != null && !string.IsNullOrEmpty(customMetadataError.message))
                                {
                                    _errorMessage = customMetadataError.message;
                                    Failed = true;
                                    Running = false;
                                    UploadOperationEnded?.Invoke(this);
                                    return;
                                }
                                SWIG.storj_uplink.free_error(customMetadataError);
                            }
                            finally
                            {
                                mut.ReleaseMutex();
                            }
                        }
                    }

                    SWIG.Error commitError = SWIG.storj_uplink.upload_commit(_uploadResult.upload);
                    if (commitError != null && !string.IsNullOrEmpty(commitError.message))
                    {
                        _errorMessage = commitError.message;
                        Failed = true;
                        Running = false;
                        UploadOperationEnded?.Invoke(this);
                        SWIG.storj_uplink.free_error(commitError);
                        return;
                    }
                    SWIG.storj_uplink.free_error(commitError);
                }
                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    Failed = true;
                    Running = false;
                    UploadOperationEnded?.Invoke(this);
                    return;
                }
                Running = false;
                Completed = true;
                UploadOperationEnded?.Invoke(this);
            }
            catch (Exception ex)
            {
                Failed = true;
                Running = false;
                _errorMessage = ex.Message;
                UploadOperationEnded?.Invoke(this);
                return;
            }
        }

        public void Dispose()
        {
            if (_uploadResult != null)
            {
                SWIG.storj_uplink.free_upload_result(_uploadResult);
                _uploadResult.Dispose();
                _uploadResult = null;
            }
            if (mut != null)
            {
                try
                {
                    mut.Dispose();
                    mut = null;
                }
                catch { }
            }
        }
    }
}
