using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    public class BucketListException : Exception
    {
        public BucketListException(string error):base(error)
        {

        }
    }
}
