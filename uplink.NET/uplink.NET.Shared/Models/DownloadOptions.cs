using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class DownloadOptions
    {
        public long Length { get; set; }
        public long Offset { get; set; }

        public DownloadOptions()
        {
            Length = -1;
        }

        internal SWIG.DownloadOptions ToSWIG()
        {
            SWIG.DownloadOptions options = new SWIG.DownloadOptions();
            options.length = Length;
            options.offset = Offset;
            
            return options;
        }
    }
}
