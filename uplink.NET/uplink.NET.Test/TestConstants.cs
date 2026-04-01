using System;
using uplink.NET.Models;

namespace uplink.NET.Test
{
    internal static class TestConstants
    {
        private const string DefaultValidApiKey = "your-API-key-here";
        private const string DefaultEncryptionSecret = "i_am_so_secret_you_cant_imagine";
        private const string DefaultInvalidApiKey = "This is not a valid API-Key";
        private const string DefaultSatelliteUrl = "europe-west-1.tardigrade.io:7777";

        public static string VALID_API_KEY => GetEnvironmentVariable("UPLINK_TEST_API_KEY", DefaultValidApiKey);
        public static string ENCRYPTION_SECRET => GetEnvironmentVariable("UPLINK_TEST_ENCRYPTION_SECRET", DefaultEncryptionSecret);
        public static string INVALID_API_KEY => GetEnvironmentVariable("UPLINK_TEST_INVALID_API_KEY", DefaultInvalidApiKey);
        public static string SATELLITE_URL => GetEnvironmentVariable("UPLINK_TEST_SATELLITE_URL", DefaultSatelliteUrl);
        public static string ACCESS_GRANT => GetOptionalEnvironmentVariable("UPLINK_TEST_ACCESS_GRANT");

        public static Access CreateAccess()
        {
            Access.SetTempDirectory(System.IO.Path.GetTempPath());

            if (!string.IsNullOrWhiteSpace(ACCESS_GRANT))
            {
                return new Access(ACCESS_GRANT);
            }

            return new Access(SATELLITE_URL, VALID_API_KEY, ENCRYPTION_SECRET);
        }

        public static Access CreateInvalidAccess()
        {
            Access.SetTempDirectory(System.IO.Path.GetTempPath());

            if (!string.IsNullOrWhiteSpace(ACCESS_GRANT))
            {
                return new Access("invalid-access-grant");
            }

            return new Access(SATELLITE_URL, INVALID_API_KEY, ENCRYPTION_SECRET);
        }

        private static string GetEnvironmentVariable(string name, string fallback)
        {
            return GetOptionalEnvironmentVariable(name) ?? fallback;
        }

        private static string? GetOptionalEnvironmentVariable(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
