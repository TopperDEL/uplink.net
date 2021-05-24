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

        internal PartUploadResult(SWIG.UplinkPartUpload partUpload)
        {
            PartUpload = new PartUpload(partUpload);
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
