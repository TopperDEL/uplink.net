using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The Access could not be revoked
    /// </summary>
    public class AccessRevokeException : Exception
    {
        public AccessRevokeException(string error):base(error)
        {

        }
    }
}
