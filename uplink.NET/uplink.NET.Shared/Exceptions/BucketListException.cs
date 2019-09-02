using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The buckets could not be listed
    /// </summary>
    public class BucketListException : Exception
    {
        public BucketListException(string error):base(error)
        {

        }
    }
}
