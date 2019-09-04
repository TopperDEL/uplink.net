using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The redundancy scheme
    /// </summary>
    public class RedundancyScheme
    {
        /// <summary>
        /// The redundancy algorithm to use
        /// </summary>
        public RedundancyAlgorithm Algorithm { get; set; }
        /// <summary>
        /// The share-size
        /// </summary>
        public int ShareSize { get; set; }
        /// <summary>
        /// The required shares count
        /// </summary>
        public short RequiredShares { get; set; }
        /// <summary>
        /// The repair shares count
        /// </summary>
        public short RepairShares { get; set; }
        /// <summary>
        /// The optimal shares count
        /// </summary>
        public short OptimalShares { get; set; }
        /// <summary>
        /// The total shares count
        /// </summary>
        public short TotalShares { get; set; }

        /// <summary>
        /// Creates a default RedundancyScheme-object
        /// </summary>
        public RedundancyScheme()
        {
            Algorithm = RedundancyAlgorithm.STORJ_REED_SOLOMON;
            ShareSize = 256;
            RequiredShares = 29;
            RepairShares = 35;
            OptimalShares = 80;
            TotalShares = 130;
        }

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

        internal static SWIG.RedundancyScheme ToSWIG(RedundancyScheme original)
        {
            SWIG.RedundancyScheme ret = new SWIG.RedundancyScheme();
            ret.algorithm = RedundancyAlgorithmHelper.ToSWIG(original.Algorithm);
            ret.optimal_shares = original.OptimalShares;
            ret.repair_shares = original.RepairShares;
            ret.required_shares = original.RequiredShares;
            ret.share_size = original.ShareSize;
            ret.total_shares = original.TotalShares;

            return ret;
        }
    }
}
