using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// An object could not be moved
    /// </summary>
    public class ObjectMoveException : Exception
    {
        public ObjectMoveException(string error):base(error)
        {

        }
    }
}
