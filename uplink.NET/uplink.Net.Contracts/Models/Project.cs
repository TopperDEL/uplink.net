using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public abstract class Project : IDisposable
    {
        public Project(Uplink uplink, ApiKey apiKey, string satelliteAddr, ProjectOptions projectOptions)
        {

        }

        public abstract void Dispose();
    }
}
