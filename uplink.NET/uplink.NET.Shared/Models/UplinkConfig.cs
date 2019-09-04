using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// An Uplink-configuration
    /// </summary>
    public class UplinkConfig
    {
        /// <summary>
        /// Skip peer CA-whitelist
        /// </summary>
        public bool Volatile_TLS_SkipPeerCAWhitelist { get; set; }
    }
}
