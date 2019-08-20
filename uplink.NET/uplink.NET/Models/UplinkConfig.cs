using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Models
{
    public class UplinkConfig:uplink.Net.Contracts.Models.IUplinkConfig
    {
        public bool Volatile_TLS_SkipPeerCAWhitelist { get; set; }
    }
}
