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

        internal static BucketList FromSWIG(SWIG.BucketList original)
        {
            BucketList ret = new BucketList();
            ret.Length = original.length;
            ret.More = original.more;
            ret.Items = new List<BucketInfo>();
            for(int i = 0; i< original.length;i++)
            {
                ret.Items.Add(BucketInfo.FromSWIG(SWIG.storj_uplink.get_bucketinfo_at(original, i)));
            }

            //ToDo: find out why this crashes: SWIG.storj_uplink.free_bucket_list(original);

            return ret;
        }
    }
}
