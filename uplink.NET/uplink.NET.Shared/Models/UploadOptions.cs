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
        /// The content-type of the object to upload
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// The expiration date
        /// </summary>
        public DateTime Expires { get; set; }

        internal SWIG.UploadOptions ToSWIG()
        {
            SWIG.UploadOptions options = new SWIG.UploadOptions();
            options.content_type = ContentType;
            if (Expires != DateTime.MinValue)
                options.expires = (new DateTimeOffset(Expires)).ToUnixTimeSeconds();

            return options;
        }
    }
}
