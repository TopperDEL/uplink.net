using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    public class BucketNotFoundException : Exception
    {
        public string BucketName { get; set; }
        public BucketNotFoundException(string bucketName, string error):base(error)
        {
            BucketName = bucketName;
        }
    }
}
