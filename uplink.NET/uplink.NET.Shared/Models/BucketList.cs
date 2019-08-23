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
    }
}
