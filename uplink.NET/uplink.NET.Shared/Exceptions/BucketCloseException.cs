using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// A bucket could not be closed
    /// </summary>
    public class BucketCloseException : Exception
    {
        public BucketCloseException(string error):base(error)
        {
        }
    }
}
