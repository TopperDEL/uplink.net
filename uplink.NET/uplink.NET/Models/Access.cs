﻿using System;
using System.Collections.Generic;
using System.Text;
using uplink.SWIG;

namespace uplink.NET.Models
{
    public class Access : IDisposable
    {
        private static string TempDirectory { get; set; }

        /// <summary>
        /// Sets the temporary directory to use.
        /// Best is to use System.IO.Path.GetTempPath().
        /// </summary>
        /// <param name="tempDir">The temporary directory</param>
        public static void SetTempDirectory(string tempDir)
        {
            TempDirectory = tempDir;
        }

        /// <summary>
        /// Return the current storj/uplink-c-version
        /// </summary>
        /// <returns>The current version</returns>
        public static string GetStorjVersion()
        {
            return SWIG.storj_uplink.get_storj_version();
        }

        #region iOs-Init
        /// <summary>
        /// Init for iOs - this is needed to correctly find the uplink.Net-Binary
        /// during runtime.
        /// You need to provide the Bundle-Path (Foundation.NSBundle.MainBundle.BundlePath)
        /// </summary>
        /// <param name="bundlePath">Provide Foundation.NSBundle.MainBundle.BundlePath</param>
        public static void Init_iOs(string bundlePath)
        {
            var fmw = @"storj_uplink.framework/storj_uplink";
            var filepath = System.IO.Path.Combine(bundlePath, "Frameworks", fmw);

            mono_dllmap_insert(IntPtr.Zero, "storj_uplink", null, filepath, null);
        }

        [System.Runtime.InteropServices.DllImport("__Internal", CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        private static extern void mono_dllmap_insert(IntPtr assembly, string dll, string func, string tdll, string tfunc);
        #endregion

        internal SWIG.UplinkAccess _access { get; set; }
        internal SWIG.UplinkProject _project { get; set; }
        //internal SWIG.UplinkConfig _config { get; set; }

        //private SWIG.UplinkAccessResult _accessResult;
        //private SWIG.UplinkProjectResult _projectResult;

        internal Access(SWIG.UplinkAccess access)
        {
            Init();

            try
            {
                _access = access;

                using (var uplinkConfigSWIG = GetUplinkConfig())
                {
                    using (var projectResult = SWIG.storj_uplink.uplink_config_open_project(uplinkConfigSWIG, _access))
                    {
                        if (projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                            throw new ArgumentException(projectResult.error.message);

                        _project = projectResult.project;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new access from a serialized string.
        /// An access contains info about the satellite-address, the passphrase and the API-Key.
        /// </summary>
        /// <param name="accessGrant">The serialized access grant</param>
        /// <param name="config">The configuration (optional)</param>
        public Access(string accessGrant, Config config = null)
        {
            Init();

            try
            {
                using (var accessResult = SWIG.storj_uplink.uplink_parse_access(accessGrant))
                {
                    if (accessResult.error != null && !string.IsNullOrEmpty(accessResult.error.message))
                        throw new ArgumentException(accessResult.error.message);

                    _access = accessResult.access;

                    using (var uplinkConfigSWIG = GetUplinkConfig(config))
                    {
                        using (var projectResult = SWIG.storj_uplink.uplink_config_open_project(uplinkConfigSWIG, _access))
                        {
                            if (projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                                throw new ArgumentException(projectResult.error.message);

                            _project = projectResult.project;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new access based on the satellite-adress, the API-key and the secret passphrase.
        /// </summary>
        /// <param name="satelliteAddress">The satellite address</param>
        /// <param name="apiKey">The API-key</param>
        /// <param name="secret">The passphrase</param>
        public Access(string satelliteAddress, string apiKey, string secret)
        {
            Init();

            try
            {
                using (var accessResult = SWIG.storj_uplink.uplink_request_access_with_passphrase(satelliteAddress, apiKey, secret))
                {
                    if (accessResult.error != null && !string.IsNullOrEmpty(accessResult.error.message))
                        throw new ArgumentException(accessResult.error.message);

                    _access = accessResult.access;

                    using (var uplinkConfigSWIG = GetUplinkConfig())
                    {
                        using (var projectResult = SWIG.storj_uplink.uplink_config_open_project(uplinkConfigSWIG, _access))
                        {
                            if (projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                                throw new ArgumentException(projectResult.error.message);

                            _project = projectResult.project;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new access based on the satellite-adress, the API-key and the secret passphrase and by using a specific config
        /// </summary>
        /// <param name="satelliteAddress">The satellite address</param>
        /// <param name="apiKey">The API-key</param>
        /// <param name="secret">The passphrase</param>
        /// <param name="config">The configuration</param>
        public Access(string satelliteAddress, string apiKey, string secret, Config config)
        {
            Init();

            try
            {
                using (var uplinkConfigSWIG = GetUplinkConfig())
                {
                    using (var accessResult = SWIG.storj_uplink.uplink_config_request_access_with_passphrase(uplinkConfigSWIG, satelliteAddress, apiKey, secret))
                    {
                        if (accessResult.error != null && !string.IsNullOrEmpty(accessResult.error.message))
                            throw new ArgumentException(accessResult.error.message);

                        _access = accessResult.access;

                        using (var projectResult = SWIG.storj_uplink.uplink_open_project(_access))
                        {
                            if (projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                                throw new ArgumentException(projectResult.error.message);

                            _project = projectResult.project;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private void Init()
        {
            SWIG.DLLInitializer.Init();

            if (string.IsNullOrEmpty(TempDirectory))
                TempDirectory = System.IO.Path.GetTempPath();
        }

        private UplinkConfig GetUplinkConfig(Config config = null)
        {
            UplinkConfig uplinkConfig;
            if (config == null)
            {
                uplinkConfig = new SWIG.UplinkConfig();
                uplinkConfig.temp_directory = TempDirectory;
            }
            else
            {
                uplinkConfig = config.ToSWIG();
                uplinkConfig.temp_directory = TempDirectory;
            }

            return uplinkConfig;
        }


        /// <summary>
        /// Serializes this access into a string
        /// </summary>
        /// <returns>The serialized access</returns>
        public string Serialize()
        {
            using (SWIG.UplinkStringResult serializedAccessResult = SWIG.storj_uplink.uplink_access_serialize(_access))
            {
                if (serializedAccessResult.error != null && !string.IsNullOrEmpty(serializedAccessResult.error.message))
                    throw new ArgumentException(serializedAccessResult.error.message);

                string serializedAccess = serializedAccessResult.string_;

                SWIG.storj_uplink.uplink_free_string_result(serializedAccessResult);

                return serializedAccess;
            }
        }

        /// <summary>
        /// Shares an access with the given permissions
        /// </summary>
        /// <param name="permission">The permission describes, which actions are allowed</param>
        /// <param name="prefixes">The prefixes declare for which pathes the permissions are meant for</param>
        /// <returns>The restricted access</returns>
        public Access Share(Permission permission, List<SharePrefix> prefixes)
        {
            SWIG.storj_uplink.prepare_shareprefixes((uint)prefixes.Count);

            foreach (var prefix in prefixes)
                SWIG.storj_uplink.append_shareprefix(prefix.Bucket, prefix.Prefix);

            using (var permissionSWIG = permission.ToSWIG())
            {
                using (SWIG.UplinkAccessResult accessResult = SWIG.storj_uplink.access_share2(_access, permissionSWIG))
                {
                    if (accessResult.error != null && !string.IsNullOrEmpty(accessResult.error.message))
                        throw new ArgumentException(accessResult.error.message);

                    return new Access(accessResult.access);
                }
            }
        }

        /// <summary>
        /// OverrideEncryptionAccess overrides the root encryption key for the prefix in 
        /// bucket with encryptionKey.
        /// 
        /// This function is useful for overriding the encryption key in user-specific
        /// access grants when implementing multitenancy in a single app bucket.
        /// </summary>
        /// <param name="bucketName">The name of the bucket</param>
        /// <param name="prefix">The prefix where the encryption key should be overwritten</param>
        /// <param name="encryptionKey">The encryption key</param>
        /// <returns>true, if overwriting worked - raises exception on error</returns>
        public bool OverrideEncryptionAccess(string bucketName, string prefix, EncryptionKey encryptionKey)
        {
            using (var error = SWIG.storj_uplink.uplink_access_override_encryption_key(_access, bucketName, prefix, encryptionKey._encryptionKeyResulRef.encryption_key))
            {
                if (error != null && !string.IsNullOrEmpty(error.message))
                    throw new ArgumentException(error.message);

                return true;
            }
        }

        public void Dispose()
        {
            if (_project != null)
            {
                using (SWIG.UplinkError closeError = SWIG.storj_uplink.uplink_close_project(_project))
                {
                    _project.Dispose();
                    _project = null;
                }
            }
        }
    }
}
