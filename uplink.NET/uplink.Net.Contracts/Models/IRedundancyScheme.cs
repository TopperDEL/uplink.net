using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Contracts.Models
{
    public interface IRedundancyScheme
    {
        int Algorithm { get; set; }
        int ShareSize { get; set; }
        int RequiredShares { get; set; }
        int RepairShares { get; set; }
        int OptimalShares { get; set; }
        int TotalShares { get; set; }
    }
}
