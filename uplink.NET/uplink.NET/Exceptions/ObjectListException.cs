using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The objects within a bucket could not be listed
    /// </summary>
    public class ObjectListException : Exception
    {
        public ObjectListException(string error):base(error)
        {

        }
    }
}
