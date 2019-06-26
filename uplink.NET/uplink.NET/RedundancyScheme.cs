﻿using System;
using System.Collections.Generic;
using System.Text;

namespace uplink
{
    public class RedundancyScheme
    {
        public int Algorithm { get; set; }
        public int ShareSize { get; set; }
        public int RequiredShares { get; set; }
        public int RepairShares { get; set; }
        public int OptimalShares { get; set; }
        public int TotalShares { get; set; }
    }
}
