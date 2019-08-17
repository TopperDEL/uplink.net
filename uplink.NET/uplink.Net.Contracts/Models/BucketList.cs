using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public abstract class BucketList
    {
        public bool More { get; set; }
        public List<BucketInfo> Items { get; set; }
        public int Length { get; set; }
    }
}
