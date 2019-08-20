using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class UplinkConfig:uplink.NET.Contracts.Models.IUplinkConfig
    {
        public bool Volatile_TLS_SkipPeerCAWhitelist { get; set; }
    }
}
