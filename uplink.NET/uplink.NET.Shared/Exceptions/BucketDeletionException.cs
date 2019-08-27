using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    public class BucketDeletionException : Exception
    {
        public string BucketName { get; set; }
        public BucketDeletionException(string bucketName, string error):base(error)
        {
            BucketName = bucketName;
        }
    }
}
