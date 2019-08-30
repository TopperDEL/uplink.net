using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class ObjectList
    {
        public string Bucket { get; set; }
        public string Prefix { get; set; }
        public bool More { get; set; }
        public List<ObjectInfo> Items { get; set; }
        public int Length { get; set; }

        internal static ObjectList FromSWIG(SWIG.ObjectList original)
        {
            ObjectList ret = new ObjectList();
            ret.Bucket = original.bucket;
            ret.Prefix = original.prefix;
            ret.More = original.more;
            ret.Length = original.length;
            ret.Items = new List<ObjectInfo>();
            for (int i = 0; i < original.length; i++)
            {
                ret.Items.Add(ObjectInfo.FromSWIG(SWIG.storj_uplink.get_objectinfo_at(original, i)));
            }

            return ret;
        }
    }
}
