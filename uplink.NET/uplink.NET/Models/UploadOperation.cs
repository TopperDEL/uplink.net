using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.IO;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;
using System.Buffers;
using uplink.NET.SWIGHelpers;

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
    public unsafe class UploadOperation : IDisposable
    {
        internal static readonly SemaphoreSlim customMetadataSemaphore = new SemaphoreSlim(1);
        private readonly Stream _byteStreamToUpload;
        private SWIG.UplinkUpload _upload;
        private Task _uploadTask;
        private bool _cancelled;
        private readonly CustomMetadata _customMetadata;

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
        internal UploadOperation(Stream stream, SWIG.UplinkUploadResult uploadResult, string objectName, CustomMetadata customMetadata = null)
        {
            _byteStreamToUpload = stream;
            _upload = uploadResult.upload;
            ObjectName = objectName;
            _customMetadata = customMetadata;

            if (uploadResult.error != null && !string.IsNullOrEmpty(uploadResult.error.message))
            {
                _errorMessage = uploadResult.error.message;
                Failed = true;
                Running = false;
            }
        }

        internal UploadOperation(byte[] bytesToUpload, SWIG.UplinkUploadResult uploadResult, string objectName, CustomMetadata customMetadata = null) :
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
            var shared = ArrayPool<byte>.Shared;
            try
            {
                if (_byteStreamToUpload != null)
                {
                    Running = true;
                    int bytesToUploadCount = 0;
                    do
                    {
                        byte[] bytesToUpload = shared.Rent(262144);

                        try
                        {
                            bytesToUploadCount = _byteStreamToUpload.Read(bytesToUpload, 0, 262144);
                            if (bytesToUploadCount > 0)
                            {
                                fixed (byte* arrayPtr = bytesToUpload)
                                {
                                    using (SWIG.UplinkWriteResult sentResult = SWIG.storj_uplink.uplink_upload_write(_upload, new SWIG.SWIGTYPE_p_void(new IntPtr(arrayPtr), true), (uint)bytesToUploadCount))
                                    {
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
                                    }
                                    if (_cancelled)
                                    {
                                        using (SWIG.UplinkError abortError = SWIG.storj_uplink.uplink_upload_abort(_upload))
                                        {
                                            // Clear ownership to prevent double-free when Dispose() is called
                                            DisposalHelper.ClearOwnership(_upload);
                                            
                                            if (abortError != null && !string.IsNullOrEmpty(abortError.message))
                                            {
                                                Failed = true;
                                                _errorMessage = abortError.message;
                                            }
                                            else
                                                Cancelled = true;
                                        }

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

                            }
                        }
                        finally
                        {
                            shared.Return(bytesToUpload);
                        }
                    } while (bytesToUploadCount > 0);

                    if (_customMetadata != null)
                    {
                        customMetadataSemaphore.Wait();
                        try
                        {
                            _customMetadata.ToSWIG(); //Appends the customMetadata in the go-layer to a global field
                            using (SWIG.UplinkError customMetadataError = SWIG.storj_uplink.upload_set_custom_metadata2(_upload))
                            {
                                if (customMetadataError != null && !string.IsNullOrEmpty(customMetadataError.message))
                                {
                                    _errorMessage = customMetadataError.message;
                                    Failed = true;
                                    UploadOperationEnded?.Invoke(this);
                                    return;
                                }
                            }
                        }
                        finally
                        {
                            customMetadataSemaphore.Release();
                        }
                    }

                    using (SWIG.UplinkError commitError = SWIG.storj_uplink.uplink_upload_commit(_upload))
                    {
                        // Clear ownership to prevent double-free when Dispose() is called
                        DisposalHelper.ClearOwnership(_upload);
                        
                        if (commitError != null && !string.IsNullOrEmpty(commitError.message))
                        {
                            _errorMessage = commitError.message;
                            Failed = true;
                            UploadOperationEnded?.Invoke(this);
                            return;
                        }
                    }
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
            }
            finally
            {
                Running = false;
                UploadOperationEnded?.Invoke(this);
            }
        }

        public void Dispose()
        {
            if (_upload != null)
            {
                _upload.Dispose();
                _upload = null;
            }
        }
    }
}
