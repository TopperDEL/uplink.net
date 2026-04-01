using System;
using System.Linq;

namespace uplink.NET.Models
{
    public unsafe class ChunkedUploadOperation : IDisposable
    {
        private SWIG.UplinkUpload _upload;
        private SWIG.UplinkUploadResult _uploadResult;
        private IDisposable _transferLifetime;
        private readonly string _objectName;
        private readonly CustomMetadata _customMetadata;
        private string _errorMessage;
        public string ErrorMessage { get { return _errorMessage; } }

        internal ChunkedUploadOperation(SWIG.UplinkUploadResult uploadResult, IDisposable transferLifetime, string objectName, CustomMetadata customMetadata = null)
        {
            _uploadResult = uploadResult;
            _upload = uploadResult.upload;
            _transferLifetime = transferLifetime;
            _objectName = objectName;
            _customMetadata = customMetadata;
        }

        public bool WriteBytes(byte[] buffer)
        {
            uint bytesSent = 0;

            while (bytesSent != buffer.Length)
            {
                var uploadChunk = buffer.Take((int)(buffer.Length - bytesSent)).ToArray();
                fixed (byte* arrayPtr = uploadChunk)
                {
                    using (SWIG.UplinkWriteResult sentResult = SWIG.storj_uplink.uplink_upload_write(_upload, new SWIG.SWIGTYPE_p_void(new IntPtr(arrayPtr), true), (uint)uploadChunk.Length))
                    {
                        if (sentResult.error != null && !string.IsNullOrEmpty(sentResult.error.message))
                        {
                            _errorMessage = sentResult.error.message;
                            CleanupNativeResources();
                            return false;
                        }
                        else
                            bytesSent += sentResult.bytes_written;
                    }
                }
            }

            return true;
        }

        public bool Commit()
        {
            if (_customMetadata != null)
            {
                UploadOperation.customMetadataSemaphore.Wait();
                try
                {
                    _customMetadata.ToSWIG(); //Appends the customMetadata in the go-layer to a global field
                    using (SWIG.UplinkError customMetadataError = SWIG.storj_uplink.upload_set_custom_metadata2(_upload))
                    {
                        if (customMetadataError != null && !string.IsNullOrEmpty(customMetadataError.message))
                        {
                            _errorMessage = customMetadataError.message;
                            CleanupNativeResources();
                            return false;
                        }
                    }
                }
                finally
                {
                    UploadOperation.customMetadataSemaphore.Release();
                }
            }

            using (SWIG.UplinkError commitError = SWIG.storj_uplink.uplink_upload_commit(_upload))
            {
                if (commitError != null && !string.IsNullOrEmpty(commitError.message))
                {
                    _errorMessage = commitError.message;
                    CleanupNativeResources();
                    return false;
                }
            }

            CleanupNativeResources();
            return true;
        }

        public void Dispose()
        {
            if (_upload != null)
            {
                using (var abortError = SWIG.storj_uplink.uplink_upload_abort(_upload))
                {
                }
            }

            CleanupNativeResources();
        }

        private void CleanupNativeResources()
        {
            if (_uploadResult != null)
            {
                _uploadResult.Dispose();
                _uploadResult = null;
            }

            _upload = null;

            if (_transferLifetime != null)
            {
                _transferLifetime.Dispose();
                _transferLifetime = null;
            }
        }
    }
}
