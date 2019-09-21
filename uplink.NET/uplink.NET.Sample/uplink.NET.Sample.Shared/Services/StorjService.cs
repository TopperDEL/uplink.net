using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Sample.Shared.Interfaces;

namespace uplink.NET.Sample.Shared.Services
{
    public class StorjService : IStorjService
    {
        public static string TempDir { get; set; }
        public uplink.NET.Models.Uplink Uplink { get; private set; }
        public uplink.NET.Models.Project Project { get; private set; }
        public uplink.NET.Models.APIKey APIKey { get; private set; }
        public bool IsInitialized { get; private set; }
        public uplink.NET.Models.EncryptionAccess EncryptionAccess { get; private set; }
        public async Task<bool> InitializeAsync(string apiKey, string satellite, string secret)
        {
            if (IsInitialized)
                return true;

#if __ANDROID__
            if (string.IsNullOrEmpty(TempDir))
                throw new ArgumentException("TempDir must be set on android - use CacheDir.AbsolutePath.");
#endif
            try
            {
                var uplinkConfig = new NET.Models.UplinkConfig();
                Uplink = new NET.Models.Uplink(uplinkConfig, TempDir);
                APIKey = new NET.Models.APIKey(apiKey);
                Project = new NET.Models.Project(Uplink, APIKey, satellite);
                EncryptionAccess = uplink.NET.Models.EncryptionAccess.FromPassphrase(Project, secret);
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
