using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// An EncryptionKey could not be created
    /// </summary>
    public class EncryptionException : Exception
    {
        public EncryptionException(string error):base(error)
        {

        }
    }
}
