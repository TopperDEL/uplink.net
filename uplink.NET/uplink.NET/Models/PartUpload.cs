using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class PartUpload : IDisposable
    {
        internal SWIG.UplinkPartUpload _partUpload { get; set; }
        internal SWIG.UplinkPartUploadResult _partUploadResult { get; set; }
        private IDisposable _transferLifetime;

        internal PartUpload(SWIG.UplinkPartUploadResult partUploadResult, IDisposable transferLifetime)
        {
            _partUploadResult = partUploadResult;
            _partUpload = partUploadResult.part_upload;
            _transferLifetime = transferLifetime;
        }

        public void Dispose()
        {
            if (_partUploadResult != null)
            {
                _partUploadResult.Dispose();
                _partUploadResult = null;
            }

            _partUpload = null;

            if (_transferLifetime != null)
            {
                _transferLifetime.Dispose();
                _transferLifetime = null;
            }
        }
    }
}
