using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Exceptions
{
    /// <summary>
    /// Aborting an upload failed
    /// </summary>
    public class AbortUploadFailedException : Exception
    {
        public AbortUploadFailedException(string error) : base(error)
        {
        }
    }
}
