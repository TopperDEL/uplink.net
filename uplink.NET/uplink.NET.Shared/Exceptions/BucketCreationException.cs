using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// A bucket could not be created.
    /// Bucket-names muss be lower case!
    /// </summary>
    public class BucketCreationException : Exception
    {
        /// <summary>
        /// The name of the bucket that could not be created.
        /// </summary>
        public string BucketName { get; set; }
        public BucketCreationException(string bucketName, string error):base(error)
        {
            BucketName = bucketName;
        }
    }
}
