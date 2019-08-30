using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    public class ObjectListException : Exception
    {
        public ObjectListException(string error):base(error)
        {

        }
    }
}
