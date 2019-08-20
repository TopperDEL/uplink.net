using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Contracts.Models;

namespace uplink.NET.Models
{
    public class BucketList:uplink.NET.Contracts.Models.IBucketList
    {
        public bool More { get; set; }
        public List<IBucketInfo> Items { get; set; }
        public int Length { get; set; }

        //ToDo: Correct SWIG-Mapping
    }
}
