using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The uploads could not be listed
    /// </summary>
    public class UploadsListException : Exception
    {
        public UploadsListException(string error):base(error)
        {

        }
    }
}
