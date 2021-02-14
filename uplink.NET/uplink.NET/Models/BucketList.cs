using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace uplink.NET.Models
{
    /// <summary>
    /// A BucketList holds the buckets within a given access
    /// </summary>
    public class BucketList
    {
        /// <summary>
        /// The items within the list - contains the listed buckets
        /// </summary>
        public List<Bucket> Items { get; private set; }

        public BucketList()
        {
            Items = new List<Bucket>();
        }
    }
}
