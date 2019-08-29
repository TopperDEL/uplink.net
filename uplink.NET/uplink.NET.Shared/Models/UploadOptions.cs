using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class UploadOptions
    {
        public string ContentType { get; set; }
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
