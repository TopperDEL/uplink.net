using System;

namespace uplink.Net.Contracts.Models
{
    public abstract class ApiKey:IDisposable
    {
        public ApiKey(string apiKeyString)
        {

        }

        public abstract void Dispose();

        public abstract string GetApiKey();
    }
}
