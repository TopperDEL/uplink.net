using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// Holds upload-options
    /// </summary>
    public class UploadOptions
    {
        /// <summary>
        /// The expiration date
        /// </summary>
        public DateTime Expires { get; set; }

        internal SWIG.UploadOptions ToSWIG()
        {
            SWIG.UploadOptions options = new SWIG.UploadOptions();
            if (Expires != DateTime.MinValue)
                options.expires = (new DateTimeOffset(Expires)).ToUnixTimeSeconds();
            else
                options.expires = DateTimeOffset.MaxValue.ToUnixTimeSeconds();

            return options;
        }
    }
}
