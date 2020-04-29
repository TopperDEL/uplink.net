using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.IO;

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
        private Stream _byteStreamToUpload;
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
                try
                {
                    if (_byteStreamToUpload != null)
                        return _byteStreamToUpload.Length;
                    else
                        return 0;
                }
                catch
                {
                    return 0; //Fallback
                }
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
        internal UploadOperation(Stream stream, SWIG.UploadResult uploadResult, string objectName, CustomMetadata customMetadata = null)
        {
            _byteStreamToUpload = stream;
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

        internal UploadOperation(byte[] bytesToUpload, SWIG.UploadResult uploadResult, string objectName, CustomMetadata customMetadata = null) :
            this(new MemoryStream(bytesToUpload), uploadResult, objectName, customMetadata)
        {
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
            {
                _uploadTask = Task.Run(DoUpload);
                Running = true;
            }
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
                if (_byteStreamToUpload != null)
                {
                    Running = true;
                    int bytesToUploadCount = 0;
                    do
                    {
                        byte[] bytesToUpload = new byte[262144];

                        bytesToUploadCount = _byteStreamToUpload.Read(bytesToUpload, 0, 262144);
                        if (bytesToUploadCount > 0)
                        {
                            SWIG.WriteResult sentResult = SWIG.storj_uplink.upload_write(_uploadResult.upload, new SWIG.SWIGTYPE_p_void(System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(bytesToUpload.Take((int)bytesToUploadCount).ToArray(), 0), true), (uint)bytesToUploadCount);
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
                                UploadOperationEnded?.Invoke(this);
                                return;
                            }
                        }
                    } while (bytesToUploadCount > 0);

                    if (_customMetadata != null)
                    {
                        if (mut.WaitOne(1000))
                        {
                            try
                            {
                                _customMetadata.ToSWIG(); //Appends the customMetadata in the go-layer to a global field
                                SWIG.Error customMetadataError = SWIG.storj_uplink.upload_set_custom_metadata2(_uploadResult.upload);
                                if (customMetadataError != null && !string.IsNullOrEmpty(customMetadataError.message))
                                {
                                    _errorMessage = customMetadataError.message;
                                    Failed = true;
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
                        UploadOperationEnded?.Invoke(this);
                        SWIG.storj_uplink.free_error(commitError);
                        return;
                    }
                    SWIG.storj_uplink.free_error(commitError);
                }
                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    Failed = true;
                    UploadOperationEnded?.Invoke(this);
                    return;
                }
                Completed = true;
                UploadOperationEnded?.Invoke(this);
            }
            catch (Exception ex)
            {
                Failed = true;
                _errorMessage = ex.Message;
                return;
            }
            finally
            {
                Running = false;
                UploadOperationEnded?.Invoke(this);
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
        }
    }
}
