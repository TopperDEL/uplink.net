using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Interfaces;

namespace uplink.NET.Models
{
    public class StorjEnvironment: IStorjEnvironment
    {
        private static string TempDirectory { get; set; }
        public static void SetTempDirectory(string tempDir)
        {
            TempDirectory = tempDir;
        }

        public uplink.NET.Models.Uplink Uplink { get; private set; }
        public uplink.NET.Models.Project Project { get; private set; }
        public uplink.NET.Models.APIKey APIKey { get; private set; }
        public bool IsInitialized { get; private set; }
        public uplink.NET.Models.EncryptionAccess EncryptionAccess { get; private set; }
        public async Task<bool> InitializeAsync(string apiKey, string satellite, string secret, UplinkConfig uplinkConfig = null)
        {
            if (IsInitialized)
                return true;

            SWIG.DLLInitializer.Init();

            if (string.IsNullOrEmpty(TempDirectory))
                throw new ArgumentException("TempDir must be set! On Android use CacheDir.AbsolutePath. On Windows/UWP use System.IO.Path.GetTempPath().");
            try
            {
                if (uplinkConfig != null)
                    Uplink = new NET.Models.Uplink(uplinkConfig, TempDirectory);
                else
                    Uplink = new NET.Models.Uplink(new NET.Models.UplinkConfig(), TempDirectory);
                APIKey = new NET.Models.APIKey(apiKey);
                Project = new NET.Models.Project(Uplink, APIKey, satellite);
                EncryptionAccess = uplink.NET.Models.EncryptionAccess.FromPassphrase(Project, secret);
            }
            catch (Exception ex)
            {
                return false;
            }

            IsInitialized = true;
            return true;
        }

        /// <summary>
        /// Gets the version of the underlying libuplinkc-library. It returns the github-tag being used.
        /// </summary>
        /// <returns>The storj-version</returns>
        public static string GetStorjVersion()
        {
            return SWIG.storj_uplink.get_storj_version();
        }
    }
}
