using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace uplink.NET.Sample.Shared.Interfaces
{
    public interface IStorjService
    {
        uplink.NET.Models.Uplink Uplink { get; }
        uplink.NET.Models.Project Project { get; }
        uplink.NET.Models.APIKey APIKey { get; }
        bool IsInitialized { get; }
        uplink.NET.Models.EncryptionAccess EncryptionAccess { get; }

        Task<bool> InitializeAsync(string APIKey, string satellite, string secret);
    }
}
