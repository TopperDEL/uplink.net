using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uplink.NET.Models
{
    public unsafe class ChunkedUploadOperation : IDisposable
    {
        private SWIG.UplinkUpload _upload;
        private string _objectName;
        private CustomMetadata _customMetadata;
        private string _errorMessage;
        public string ErrorMessage { get { return _errorMessage; } }

        internal ChunkedUploadOperation(SWIG.UplinkUploadResult uploadResult, string objectName, CustomMetadata customMetadata = null)
        {
            _upload = uploadResult.upload;
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
                if (UploadOperation.customMetadataMutex.WaitOne(1000))
                {
                    try
                    {
                        _customMetadata.ToSWIG(); //Appends the customMetadata in the go-layer to a global field
                        using (SWIG.UplinkError customMetadataError = SWIG.storj_uplink.upload_set_custom_metadata2(_upload))
                        {
                            if (customMetadataError != null && !string.IsNullOrEmpty(customMetadataError.message))
                            {
                                _errorMessage = customMetadataError.message;
                                return false;
                            }
                        }
                    }
                    finally
                    {
                        UploadOperation.customMetadataMutex.ReleaseMutex();
                    }
                }
            }

            using (SWIG.UplinkError commitError = SWIG.storj_uplink.uplink_upload_commit(_upload))
            {
                if (commitError != null && !string.IsNullOrEmpty(commitError.message))
                {
                    _errorMessage = commitError.message;
                    return false;
                }
            }
            return true;
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
