using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class Part
    {
        public uint PartNumber { get; set; }
        public uint Size { get; set; }
        public DateTime Modified { get; set; }
        public string ETag { get; set; }
        public uint ETagLength { get; set; }

        internal static Part FromSWIG(SWIG.UplinkPart original)
        {
            Part ret = new Part();
            ret.PartNumber = original.part_number;
            ret.Size = original.size;
            //Temporary to fix a calloc-issue. Should already be fixed - so just to be safe.
            try
            {
                ret.Modified = DateTimeOffset.FromUnixTimeSeconds(original.modified).ToLocalTime().DateTime;
            }
            catch
            {
                ret.Modified = DateTime.Now;
            }
            ret.ETag = original.etag;
            ret.ETagLength = original.etag_length;

            return ret;
        }
    }
}
