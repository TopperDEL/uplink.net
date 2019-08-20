using System;

namespace uplink.NET.Contracts.Models
{
    public interface IApiKey:IDisposable
    {
        string GetApiKey();
    }
}
