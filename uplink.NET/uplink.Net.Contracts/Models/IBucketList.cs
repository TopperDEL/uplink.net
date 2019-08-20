using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public interface IBucketList
    {
        bool More { get; set; }
        List<IBucketInfo> Items { get; set; }
        int Length { get; set; }
    }
}
