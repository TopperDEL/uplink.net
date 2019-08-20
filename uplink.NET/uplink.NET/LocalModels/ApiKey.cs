using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.LocalModels
{
    public class ApiKey : uplink.Net.Contracts.Models.IApiKey
    {
        internal SWIG.APIKeyRef _apiKeyRef = null;

        /// <summary>
        /// Creates an ApiKey-Instance by a given ApiKey-String.
        /// Throws ArgumentException if the ApiKey is not valid
        /// </summary>
        public ApiKey(string apiKeyString)
        {
            string error;

            _apiKeyRef = SWIG.storj_uplink.parse_api_key(apiKeyString, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);
            if (_apiKeyRef == null)
                throw new NullReferenceException("No ApiKey-reference created");
        }

        /// <summary>
        /// Returns the used ApiKey as string
        /// </summary>
        /// <returns>The used ApiKey</returns>
        public string GetApiKey()
        {
            string result = string.Empty;
            string error;

            result = SWIG.storj_uplink.serialize_api_key(_apiKeyRef, out error);

            return result;
        }

        public void Dispose()
        {
            if(_apiKeyRef != null)
            {
                SWIG.storj_uplink.free_api_key(_apiKeyRef);
                _apiKeyRef = null;
            }
        }
    }
}
