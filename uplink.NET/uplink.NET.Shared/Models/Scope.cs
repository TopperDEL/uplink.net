using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class Scope : IDisposable
    {
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
        public Scope(string serializedScope)
        {
            string error;

            _scoperef = SWIG.storj_uplink.parse_scope(serializedScope, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);
            if (_scoperef == null)
                throw new NullReferenceException("Could not parse scope");
        }

        /// <summary>
        /// Creates a new scope based on the satellite-adress, the API-key and the encryption access.
        /// A Scope contains info about the satellite-address, the EncryptionAccess and the API-Key.
        /// </summary>
        /// <param name="satelliteAddress">The satellite address</param>
        /// <param name="apiKey">The API-key</param>
        /// <param name="encAccess">The EncryptionAccess</param>
        public Scope(string satelliteAddress, APIKey apiKey, EncryptionAccess encAccess)
        {
            string error;

            _scoperef = SWIG.storj_uplink.new_scope(satelliteAddress, apiKey._apiKeyRef, encAccess._handle, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);
            if (_scoperef == null)
                throw new NullReferenceException("Could not create scope");
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

            SWIG.storj_uplink.prepare_restrictions(encryptionRestrictions.Count);

            for (int i = 0; i < encryptionRestrictions.Count; i++)
                SWIG.storj_uplink.add_restriction(encryptionRestrictions[i].ToSWIG(), i);

            try
            {
                var restricted = SWIG.storj_uplink.restrict_scope2(_scoperef, caveat.ToSWIG(), (uint)encryptionRestrictions.Count, out error);
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
        }
    }
}
