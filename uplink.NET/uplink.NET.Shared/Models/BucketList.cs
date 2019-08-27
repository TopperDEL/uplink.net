using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class BucketList
    {
        public bool More { get; set; }
        public List<BucketInfo> Items { get; set; }
        public int Length { get; set; }

        //ToDo: Correct SWIG-Mapping

        internal static BucketList FromSWIG(SWIG.BucketList original)
        {
            BucketList ret = new BucketList();
            ret.Length = original.length;
            ret.More = original.more;
            //Todo: Map items

            return ret;
        }
    }
}
