using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.LocalModels
{
    public class UplinkConfig:uplink.Net.Contracts.Models.IUplinkConfig
    {
        public bool Volatile_TLS_SkipPeerCAWhitelist { get; set; }
    }
}
