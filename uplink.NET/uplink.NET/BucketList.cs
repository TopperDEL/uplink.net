using System;
using System.Collections.Generic;
using System.Text;

namespace uplink
{
    public class BucketList
    {
        public bool More { get; set; }
        public List<BucketInfo> Items { get; set; } //ToDo: Correct SWIG-Mapping
        public int Length { get; set; }
    }
}
