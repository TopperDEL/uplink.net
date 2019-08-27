using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class RedundancyScheme
    {
        public RedundancyAlgorithm Algorithm { get; set; }
        public int ShareSize { get; set; }
        public int RequiredShares { get; set; }
        public int RepairShares { get; set; }
        public int OptimalShares { get; set; }
        public int TotalShares { get; set; }

        internal static RedundancyScheme FromSWIG(SWIG.RedundancyScheme original)
        {
            RedundancyScheme ret = new RedundancyScheme();
            ret.Algorithm = RedundancyAlgorithmHelper.FromSWIG(original.algorithm);
            ret.OptimalShares = original.optimal_shares;
            ret.RepairShares = original.repair_shares;
            ret.RequiredShares = original.required_shares;
            ret.ShareSize = original.share_size;
            ret.TotalShares = original.total_shares;

            return ret;
        }
    }
}
