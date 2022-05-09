using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// The Metadata of an object could not be updated
    /// </summary>
    public class CouldNotUpdateObjectMetadataException : Exception
    {
        public CouldNotUpdateObjectMetadataException(string error):base(error)
        {

        }
    }
}
