using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace uplink.NET.Models
{
    /// <summary>
    /// A BucketList holds the buckets within a given project
    /// </summary>
    public class BucketList
    {
        /// <summary>
        /// Is true if there are more entries available not loaded with the last ListBuckets-command
        /// </summary>
        public bool More { get; private set; }
        /// <summary>
        /// The items within the list - contains information about the listed buckets
        /// </summary>
        public List<BucketInfo> Items { get; private set; }
        /// <summary>
        /// The amount of BucketInfo-items in this BucketList
        /// </summary>
        public int Length { get; private set; }

        internal static BucketList FromSWIG(SWIG.BucketList original)
        {
            BucketList ret = new BucketList();
            ret.Length = original.length;
            ret.More = original.more;
            ret.Items = new List<BucketInfo>();
            for (int i = 0; i < original.length; i++)
            {
                ret.Items.Add(BucketInfo.FromSWIG(SWIG.storj_uplink.get_bucketinfo_at(original, i), false));
            }

            ret.Items = ret.Items.OrderBy(i => i.Name).ToList();

            SWIG.storj_uplink.free_bucket_list(original);

            return ret;
        }
    }
}
