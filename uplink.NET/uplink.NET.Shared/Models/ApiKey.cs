using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// An API-key is necessary to connect to a satellite. It needs to be created manually on a specific satellite for a specific project.
    /// </summary>
    public class APIKey : IDisposable
    {
        internal SWIG.APIKeyRef _apiKeyRef = null;

        /// <summary>
        /// Creates an APIKey-Instance by a given APIKey-String.
        /// Throws ArgumentException if the APIKey is not valid or NullReferenceException if the APIKey could not be created.
        /// 
        /// Needs to be disposed after use!
        /// </summary>
        public APIKey(string apiKeyString)
        {
            string error;

            _apiKeyRef = SWIG.storj_uplink.parse_api_key(apiKeyString, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);
            if (_apiKeyRef == null)
                throw new NullReferenceException("No APIKey-reference created");
        }

        private APIKey()
        {
        }

        internal static APIKey FromSWIG(SWIG.APIKeyRef original)
        {
            APIKey apiKey = new APIKey();
            apiKey._apiKeyRef = original;

            return apiKey;
        }

        /// <summary>
        /// Returns the used APIKey as string
        /// </summary>
        /// <returns>The used APIKey</returns>
        public string GetAPIKey()
        {
            string result = string.Empty;
            string error;

            result = SWIG.storj_uplink.serialize_api_key(_apiKeyRef, out error);
            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            return result;
        }

        /// <summary>
        /// Frees memory in use and disposes an APIKey
        /// </summary>
        public void Dispose()
        {
            if (_apiKeyRef != null)
            {
                SWIG.storj_uplink.free_api_key(_apiKeyRef);
                _apiKeyRef = null;
            }
        }
    }
}
