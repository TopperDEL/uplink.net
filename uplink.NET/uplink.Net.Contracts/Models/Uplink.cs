using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public abstract class Uplink : IDisposable
    {
        public Uplink(UplinkConfig uplinkConfig)
        {
        }

        public abstract void Dispose();
    }
}
