using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The serialization of an access was not successfull
    /// </summary>
    public class AccessSerializeException : Exception
    {
        public AccessSerializeException(string error):base(error)
        {

        }
    }
}
