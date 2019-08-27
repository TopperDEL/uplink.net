using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    public class BucketCloseException : Exception
    {
        public BucketCloseException(string error):base(error)
        {
        }
    }
}
