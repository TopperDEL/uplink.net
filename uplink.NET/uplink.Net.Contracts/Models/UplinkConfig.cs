using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public abstract class UplinkConfig
    {
        public bool Volatile_TLS_SkipPeerCAWhitelist { get; set; }
    }
}
