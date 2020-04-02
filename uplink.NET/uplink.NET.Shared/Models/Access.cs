using System;
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
        /// On Android use CacheDir.AbsolutePath. On Windows/UWP use System.IO.Path.GetTempPath().
        /// </summary>
        /// <param name="tempDir">The temporary directory</param>
        public static void SetTempDirectory(string tempDir)
        {
            TempDirectory = tempDir;
        }

        internal SWIG.Access _access { get; set; }
        internal SWIG.Project _project { get; set; }
        internal SWIG.Config _config { get; set; }

        private SWIG.AccessResult _accessResult;

        internal Access(SWIG.Access access)
        {
            Init();

            try
            {
                _access = access;

                SWIG.ProjectResult projectResult = SWIG.storj_uplink.config_open_project(_config, _access);
                if (projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                    throw new ArgumentException(projectResult.error.message);

                _project = projectResult.project;
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
        /// <param name="accessString">The serialized access-string</param>
        public Access(string accessString)
        {
            Init();

            try
            {
                _accessResult = SWIG.storj_uplink.parse_access(accessString);
                if (_accessResult.error != null && !string.IsNullOrEmpty(_accessResult.error.message))
                    throw new ArgumentException(_accessResult.error.message);

                _access = _accessResult.access;

                SWIG.ProjectResult projectResult = SWIG.storj_uplink.config_open_project(_config, _access);
                if (projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                    throw new ArgumentException(projectResult.error.message);

                _project = projectResult.project;
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
                _accessResult = SWIG.storj_uplink.request_access_with_passphrase(satelliteAddress, apiKey, secret);
                if (_accessResult.error != null && !string.IsNullOrEmpty(_accessResult.error.message))
                    throw new ArgumentException(_accessResult.error.message);

                _access = _accessResult.access;

                SWIG.ProjectResult projectResult = SWIG.storj_uplink.config_open_project(_config, _access);
                if (projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                    throw new ArgumentException(projectResult.error.message);

                _project = projectResult.project;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new access based on the satellite-adress, the API-key and the secret passphrase and by using a specific config
        /// </summary>
        /// <param name="config">The configuration</param>
        /// <param name="satelliteAddress">The satellite address</param>
        /// <param name="apiKey">The API-key</param>
        /// <param name="secret">The passphrase</param>
        public Access(Config config, string satelliteAddress, string apiKey, string secret)
        {
            Init(config);

            try
            {
                _accessResult = SWIG.storj_uplink.config_request_access_with_passphrase(_config, satelliteAddress, apiKey, secret);
                if (_accessResult.error != null && !string.IsNullOrEmpty(_accessResult.error.message))
                    throw new ArgumentException(_accessResult.error.message);

                _access = _accessResult.access;

                SWIG.ProjectResult projectResult = SWIG.storj_uplink.open_project(_access);
                if (projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                    throw new ArgumentException(projectResult.error.message);

                _project = projectResult.project;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private void Init(Config config = null)
        {
#if !__ANDROID__
            SWIG.DLLInitializer.Init();
#endif

            if (string.IsNullOrEmpty(TempDirectory))
                throw new ArgumentException("TempDir must be set! On Android use CacheDir.AbsolutePath. On Windows/UWP use System.IO.Path.GetTempPath().");

            if (config == null)
            {
                _config = new SWIG.Config();
                _config.temp_directory = TempDirectory;
            }
            else
            {
                _config = config.ToSWIG();
            }
        }


        /// <summary>
        /// Serializes this access into a string
        /// </summary>
        /// <returns>The serialized scope</returns>
        public string Serialize()
        {
            using (SWIG.StringResult serializedAccessResult = SWIG.storj_uplink.access_serialize(_access))
            {
                if (serializedAccessResult.error != null && !string.IsNullOrEmpty(serializedAccessResult.error.message))
                    throw new ArgumentException(serializedAccessResult.error.message);

                string serializedAccess = serializedAccessResult.string_;

                SWIG.storj_uplink.free_string_result(serializedAccessResult);

                return serializedAccess;
            }
        }

        /// <summary>
        /// Shares an access with the given permissions
        /// </summary>
        /// <param name="permission">The permission describes, which actions are allowed</param>
        /// <param name="prefixes">The prefixes declare for which pathes the permissions are meant for</param>
        /// <returns>The restricted scope</returns>
        public Access Share(Permission permission, List<SharePrefix> prefixes)
        {
            SWIG.storj_uplink.prepare_shareprefixes((uint)prefixes.Count);

            foreach (var prefix in prefixes)
                SWIG.storj_uplink.append_shareprefix(prefix.Bucket, prefix.Prefix);

            SWIG.AccessResult accessResult = SWIG.storj_uplink.access_share2(_access, permission.ToSWIG());
            if (accessResult.error != null && !string.IsNullOrEmpty(accessResult.error.message))
                throw new ArgumentException(accessResult.error.message);

            return new Access(accessResult.access);
        }

        public void Dispose()
        {
            if (_project != null)
            {
                SWIG.Error closeError = SWIG.storj_uplink.close_project(_project);
                SWIG.storj_uplink.free_error(closeError);
                _project.Dispose();
                _project = null;
            }
            if(_accessResult != null)
            {
                SWIG.storj_uplink.free_access_result(_accessResult);
                _accessResult.Dispose();
                _accessResult = null;
            }
        }
    }
}
