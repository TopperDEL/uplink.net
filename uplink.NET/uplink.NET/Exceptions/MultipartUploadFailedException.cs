using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The multpart-upload failed
    /// </summary>
    public class MultipartUploadFailedException : Exception
    {
        /// <summary>
        /// The key of the object that could not be uploaded
        /// </summary>
        public string ObjectKey { get; set; }

        public MultipartUploadFailedException(string objectKey, string error) : base(error)
        {
            ObjectKey = objectKey;
        }
    }
}
