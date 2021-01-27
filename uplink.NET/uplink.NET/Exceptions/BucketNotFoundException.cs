using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The mentioned bucket could not be found
    /// </summary>
    public class BucketNotFoundException : Exception
    {
        /// <summary>
        /// The name of the bucket that could not be found
        /// </summary>
        public string BucketName { get; set; }
        public BucketNotFoundException(string bucketName, string error):base(error)
        {
            BucketName = bucketName;
        }
    }
}
