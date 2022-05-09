using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// An access was not successfull
    /// </summary>
    public class AccessException : Exception
    {
        public AccessException(string error):base(error)
        {

        }
    }
}
