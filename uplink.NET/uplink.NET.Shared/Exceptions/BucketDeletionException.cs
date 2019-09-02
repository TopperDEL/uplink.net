using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// A bucket could not be deleted
    /// </summary>
    public class BucketDeletionException : Exception
    {
        /// <summary>
        /// The name of the bucket that could not be deleted
        /// </summary>
        public string BucketName { get; set; }
        public BucketDeletionException(string bucketName, string error):base(error)
        {
            BucketName = bucketName;
        }
    }
}
