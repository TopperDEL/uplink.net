using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    public class BucketCreationException : Exception
    {
        public BucketCreationException(string error):base(error)
        {

        }
    }
}
