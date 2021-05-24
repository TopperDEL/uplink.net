using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    public class SetCustomMetadataFailedException : Exception
    {
        public SetCustomMetadataFailedException(string error) : base(error)
        {

        }
    }
}
