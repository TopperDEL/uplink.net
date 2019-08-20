using System;
using System.Collections.Generic;
using System.Text;
using uplink.Net.Contracts.Models;

namespace uplink.Net.LocalModels
{
    public class BucketList:uplink.Net.Contracts.Models.IBucketList
    {
        public bool More { get; set; }
        public List<IBucketInfo> Items { get; set; }
        public int Length { get; set; }

        //ToDo: Correct SWIG-Mapping
    }
}
