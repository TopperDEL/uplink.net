using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class PartUpload : IDisposable
    {
        internal SWIG.UplinkPartUpload _partUpload { get; set; }

        internal PartUpload(SWIG.UplinkPartUpload partUpload)
        {
            _partUpload = partUpload;
        }

        public void Dispose()
        {
            if (_partUpload != null)
            {
                _partUpload.Dispose();
                _partUpload = null;
            }
        }
    }
}
