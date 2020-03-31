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

        internal SWIG.Access Access { get; set; }
        internal SWIG.Project Project { get; set; }
        //private bool IsInitialized { get; set; }

        internal SWIG.Config Config { get; set; }

        //internal SWIG.ScopeRef _scoperef;

        //internal Scope(SWIG.ScopeRef scopeRef)
        //{
        //    _scoperef = scopeRef;
        //}

        /// <summary>
        /// Creates a new scope from a serialized string.
        /// A Scope contains info about the satellite-address, the EncryptionAccess and the API-Key.
        /// </summary>
        /// <param name="serializedScope">The serializes scope-string</param>
        public Scope(string serializedScope)
        {
            Init();

            try
            {
                var accessResult = SWIG.storj_uplink.parse_access(serializedScope);
                if (accessResult.error != null && !string.IsNullOrEmpty(accessResult.error.message))
                    throw new ArgumentException(accessResult.error.message);

                Access = accessResult.access;

                var projectResult = SWIG.storj_uplink.config_open_project(Config, Access);
                if (projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                    throw new ArgumentException(projectResult.error.message);

                Project = projectResult.project;
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
        public Scope(string apiKey, string satelliteAddress, string secret)
        {
            Init();

            try
            {
                var accessResult = SWIG.storj_uplink.request_access_with_passphrase(satelliteAddress, apiKey, secret);
                if (accessResult.error != null && !string.IsNullOrEmpty(accessResult.error.message))
                    throw new ArgumentException(accessResult.error.message);

                Access = accessResult.access;

                var projectResult = SWIG.storj_uplink.config_open_project(Config, Access);
                if(projectResult.error != null && !string.IsNullOrEmpty(projectResult.error.message))
                    throw new ArgumentException(projectResult.error.message);

                Project = projectResult.project;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        ///// <summary>
        ///// Creates a new scope based on the satellite-adress, the API-key and the encryption access.
        ///// A Scope contains info about the satellite-address, the EncryptionAccess and the API-Key.
        ///// </summary>
        ///// <param name="satelliteAddress">The satellite address</param>
        ///// <param name="apiKey">The API-key</param>
        ///// <param name="encAccess">The EncryptionAccess</param>
        ///// <param name="uplinkConfig">The (optional) Uplink-Configuration</param>
        //public Scope(string satelliteAddress, APIKey apiKey, EncryptionAccess encAccess, UplinkConfig uplinkConfig = null)
        //{
        //    Init(uplinkConfig);

        //    string error;

        //    try
        //    {
        //        APIKey = apiKey;
        //        EncryptionAccess = encAccess;

        //        _scoperef = SWIG.storj_uplink.new_scope(satelliteAddress, apiKey._apiKeyRef, encAccess._handle, out error);

        //        if (!string.IsNullOrEmpty(error))
        //            throw new ArgumentException(error);
        //        if (_scoperef == null)
        //            throw new NullReferenceException("Could not create scope");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ArgumentException(ex.Message);
        //    }
        //}

        private void Init()
        {
#if !__ANDROID__
            SWIG.DLLInitializer.Init();
#endif

            if (string.IsNullOrEmpty(TempDirectory))
                throw new ArgumentException("TempDir must be set! On Android use CacheDir.AbsolutePath. On Windows/UWP use System.IO.Path.GetTempPath().");

            Config = new SWIG.Config();
            Config.temp_directory = TempDirectory;
        }


        /// <summary>
        /// Serializes this scope into a string
        /// </summary>
        /// <returns>The serialized scope</returns>
        public string Serialize()
        {
            var serializedScopeResult = SWIG.storj_uplink.access_serialize(Access);

            if (serializedScopeResult.error != null && !string.IsNullOrEmpty(serializedScopeResult.error.message))
                throw new ArgumentException(serializedScopeResult.error.message);

            return serializedScopeResult.string_;
        }

        ///// <summary>
        ///// Restricts a scope with the given caveat and for the given EncryptionRestrictions
        ///// </summary>
        ///// <param name="caveat">The caveat describes, which actions are allowd</param>
        ///// <param name="encryptionRestrictions">The encryptionRestrictions declare for which buckets and path-prefixes the restrictions are meant for</param>
        ///// <returns>The restricted scope</returns>
        //public Scope Restrict(Caveat caveat, List<EncryptionRestriction> encryptionRestrictions)
        //{
        //    throw new NotImplementedException();
        //    //string error;

        //    //SWIG.storj_uplink.prepare_restrictions((uint)encryptionRestrictions.Count);

        //    //for (int i = 0; i < encryptionRestrictions.Count; i++)
        //    //    SWIG.storj_uplink.append_restriction(encryptionRestrictions[i].Bucket, encryptionRestrictions[i].PathPrefix);

        //    //try
        //    //{
        //    //    var restricted = SWIG.storj_uplink.access_share(Access, .restrict_scope2(_scoperef, caveat.ToSWIG(), out error);

        //    //    if (!string.IsNullOrEmpty(error))
        //    //        throw new ArgumentException(error);
        //    //    return new Scope(restricted);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    return null;
        //    //}
        //}

        public void Dispose()
        {
            if (Project != null)
            {
                Project.Dispose();
                Project = null;
            }
        }
    }
}
