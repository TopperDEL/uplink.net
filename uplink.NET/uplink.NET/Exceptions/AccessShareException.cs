using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The sharing of an access was not successfull
    /// </summary>
    public class AccessShareException : Exception
    {
        public AccessShareException(string error):base(error)
        {

        }
    }
}
