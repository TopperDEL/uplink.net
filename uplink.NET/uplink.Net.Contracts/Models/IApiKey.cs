using System;

namespace uplink.Net.Contracts.Models
{
    public interface IApiKey:IDisposable
    {
        string GetApiKey();
    }
}
