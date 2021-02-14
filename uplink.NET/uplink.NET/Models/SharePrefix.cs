using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class SharePrefix
    {
        public string Bucket { get; set; }
        public string Prefix { get; set; }

        internal SWIG.UplinkSharePrefix ToSWIG()
        {
            SWIG.UplinkSharePrefix sharePrefix = new SWIG.UplinkSharePrefix();
            sharePrefix.bucket = Bucket;
            sharePrefix.prefix = Prefix;

            return sharePrefix;
        }
    }
}
