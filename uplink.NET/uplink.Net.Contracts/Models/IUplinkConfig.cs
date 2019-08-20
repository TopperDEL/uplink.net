using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public interface IUplinkConfig
    {
        bool Volatile_TLS_SkipPeerCAWhitelist { get; set; }
    }
}
