using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The upload parts could not be listed
    /// </summary>
    public class UploadPartsListException : Exception
    {
        public UploadPartsListException(string error):base(error)
        {

        }
    }
}
