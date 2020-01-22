using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class Scope : IDisposable
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

        private Uplink Uplink { get; set; }
        internal Project Project { get; set; }
        internal APIKey APIKey { get; set; }
        private bool IsInitialized { get; set; }
        internal EncryptionAccess EncryptionAccess { get; set; }

        internal SWIG.ScopeRef _scoperef;

        internal Scope(SWIG.ScopeRef scopeRef)
        {
            _scoperef = scopeRef;
        }

        /// <summary>
        /// Creates a new scope from a serialized string.
        /// A Scope contains info about the satellite-address, the EncryptionAccess and the API-Key.
        /// </summary>
        /// <param name="serializedScope">The serializes scope-string</param>
        /// <param name="uplinkConfig">The (optional) Uplink-Configuration</param>
        public Scope(string serializedScope, UplinkConfig uplinkConfig = null)
        {
            Init(uplinkConfig);

            string error;
            try
            {
                _scoperef = SWIG.storj_uplink.parse_scope(serializedScope, out error);
                Project = new NET.Models.Project(Uplink, GetAPIKey(), GetSatelliteAddress());
                EncryptionAccess = GetEncryptionAccess();

                if (!string.IsNullOrEmpty(error))
                    throw new ArgumentException(error);
                if (_scoperef == null)
                    throw new NullReferenceException("Could not parse scope");
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new scope based on the satellite-adress, the API-key and the encryption access.
        /// A Scope contains info about the satellite-address, the EncryptionAccess and the API-Key.
        /// </summary>
        /// <param name="apiKey">The API-key</param>
        /// <param name="satelliteAddress">The satellite address</param>
        /// <param name="encAccess">The EncryptionAccess</param>
        /// <param name="uplinkConfig">The (optional) Uplink-Configuration</param>
        public Scope(string apiKey, string satelliteAddress, string secret, UplinkConfig uplinkConfig = null)
        {
            Init(uplinkConfig);

            string error;

            try
            {  
                APIKey = new NET.Models.APIKey(apiKey);
                Project = new NET.Models.Project(Uplink, APIKey, satelliteAddress);
                EncryptionAccess = uplink.NET.Models.EncryptionAccess.FromPassphrase(Project, secret);

                _scoperef = SWIG.storj_uplink.new_scope(satelliteAddress, APIKey._apiKeyRef, EncryptionAccess._handle, out error);

                if (!string.IsNullOrEmpty(error))
                    throw new ArgumentException(error);
                if (_scoperef == null)
                    throw new NullReferenceException("Could not create scope");
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new scope based on the satellite-adress, the API-key and the encryption access.
        /// A Scope contains info about the satellite-address, the EncryptionAccess and the API-Key.
        /// </summary>
        /// <param name="satelliteAddress">The satellite address</param>
        /// <param name="apiKey">The API-key</param>
        /// <param name="encAccess">The EncryptionAccess</param>
        /// <param name="uplinkConfig">The (optional) Uplink-Configuration</param>
        public Scope(string satelliteAddress, APIKey apiKey, EncryptionAccess encAccess, UplinkConfig uplinkConfig = null)
        {
            Init(uplinkConfig);

            string error;

            try
            {
                APIKey = apiKey;
                EncryptionAccess = encAccess;

                _scoperef = SWIG.storj_uplink.new_scope(satelliteAddress, apiKey._apiKeyRef, encAccess._handle, out error);

                if (!string.IsNullOrEmpty(error))
                    throw new ArgumentException(error);
                if (_scoperef == null)
                    throw new NullReferenceException("Could not create scope");
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private void Init(UplinkConfig uplinkConfig)
        {
#if !__ANDROID__
            SWIG.DLLInitializer.Init();
#endif

            if (string.IsNullOrEmpty(TempDirectory))
                throw new ArgumentException("TempDir must be set! On Android use CacheDir.AbsolutePath. On Windows/UWP use System.IO.Path.GetTempPath().");

            if (uplinkConfig != null)
                Uplink = new NET.Models.Uplink(uplinkConfig, TempDirectory);
            else
                Uplink = new NET.Models.Uplink(new NET.Models.UplinkConfig(), TempDirectory);
        }

        /// <summary>
        /// Returns the satellite-address of this scope
        /// </summary>
        /// <returns>The satellite-address</returns>
        public string GetSatelliteAddress()
        {
            string error;

            var satelliteAddress = SWIG.storj_uplink.get_scope_satellite_address(_scoperef, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            return satelliteAddress;
        }

        /// <summary>
        /// Returns the API-key of this scope
        /// </summary>
        /// <returns>The API-key</returns>
        public APIKey GetAPIKey()
        {
            string error;

            var apiKeyRef = SWIG.storj_uplink.get_scope_api_key(_scoperef, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            return APIKey.FromSWIG(apiKeyRef);
        }

        /// <summary>
        /// Returns the EncryptionAccess of this scope
        /// </summary>
        /// <returns>The EncryptionAccess</returns>
        public EncryptionAccess GetEncryptionAccess()
        {
            string error;

            var encAccess = SWIG.storj_uplink.get_scope_enc_access(_scoperef, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            return EncryptionAccess.FromSWIG(encAccess);
        }

        /// <summary>
        /// Serializes this scope into a string
        /// </summary>
        /// <returns>The serialized scope</returns>
        public string Serialize()
        {
            string error;

            var serializedScope = SWIG.storj_uplink.serialize_scope(_scoperef, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            return serializedScope;
        }

        /// <summary>
        /// Restricts a scope with the given caveat and for the given EncryptionRestrictions
        /// </summary>
        /// <param name="caveat">The caveat describes, which actions are allowd</param>
        /// <param name="encryptionRestrictions">The encryptionRestrictions declare for which buckets and path-prefixes the restrictions are meant for</param>
        /// <returns>The restricted scope</returns>
        public Scope Restrict(Caveat caveat, List<EncryptionRestriction> encryptionRestrictions)
        {
            string error;

            SWIG.storj_uplink.prepare_restrictions((uint)encryptionRestrictions.Count);

            for (int i = 0; i < encryptionRestrictions.Count; i++)
                SWIG.storj_uplink.append_restriction(encryptionRestrictions[i].Bucket, encryptionRestrictions[i].PathPrefix);

            try
            {
                var restricted = SWIG.storj_uplink.restrict_scope2(_scoperef, caveat.ToSWIG(), out error);

                if (!string.IsNullOrEmpty(error))
                    throw new ArgumentException(error);
                return new Scope(restricted);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (_scoperef != null)
            {
                SWIG.storj_uplink.free_scope(_scoperef);
                _scoperef = null;
            }

            if (Uplink != null)
            {
                Uplink.Dispose();
                Uplink = null;
            }

            if (Project != null)
            {
                Project.Dispose();
                Project = null;
            }

            if (APIKey != null)
            {
                APIKey.Dispose();
                APIKey = null;
            }

            if (EncryptionAccess != null)
            {
                EncryptionAccess.Dispose();
                EncryptionAccess = null;
            }
        }
    }
}
