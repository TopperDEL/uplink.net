using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class PartUploadResult : IDisposable
    {
        public PartUpload PartUpload { get; internal set; }
        public string Error { get; internal set; }
        public uint BytesWritten { get; internal set; }

        internal PartUploadResult()
        {
        }

        internal PartUploadResult(SWIG.UplinkPartUploadResult partUploadResult, IDisposable transferLifetime)
        {
            PartUpload = new PartUpload(partUploadResult, transferLifetime);
        }

        public void Dispose()
        {
            if (PartUpload != null)
            {
                PartUpload.Dispose();
                PartUpload = null;
            }
        }
    }
}
