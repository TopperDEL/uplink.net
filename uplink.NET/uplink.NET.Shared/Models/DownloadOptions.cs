using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// DownloadOptions contains additional options for downloading.
    /// </summary>
    public class DownloadOptions
    {
        /// <summary>
        /// When Length is negative it will read until the end of the blob. This is the default.
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// The offset where to start the download. Default is 0.
        /// </summary>
        public long Offset { get; set; }

        public DownloadOptions()
        {
            Length = -1;
            Offset = 0;
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
