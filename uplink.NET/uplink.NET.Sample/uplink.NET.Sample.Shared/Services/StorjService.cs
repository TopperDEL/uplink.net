using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Sample.Shared.Interfaces;

namespace uplink.NET.Sample.Shared.Services
{
    public class StorjService : IStorjService
    {
        public uplink.NET.Models.Uplink Uplink { get; private set; }
        public uplink.NET.Models.Project Project { get; private set; }
        public uplink.NET.Models.APIKey APIKey { get; private set; }
        public bool IsInitialized { get; private set; }
        public async Task<bool> InitializeAsync(string apiKey, string satellite)
        {
            if (IsInitialized)
                return true;

            try
            {
                var uplinkConfig = new NET.Models.UplinkConfig();
                Uplink = new NET.Models.Uplink(uplinkConfig);
                APIKey = new NET.Models.APIKey(apiKey);
                Project = new NET.Models.Project(Uplink, APIKey, satellite);
            }
            catch
            {
                return false;
            }

            IsInitialized = true;
            return true;
        }
    }
}
