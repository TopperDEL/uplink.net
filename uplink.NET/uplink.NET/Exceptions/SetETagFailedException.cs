using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// Setting the ETag on an upload failed
    /// </summary>
    public class SetETagFailedException : Exception
    {
        public SetETagFailedException(string error) : base(error)
        {
        }
    }
}
