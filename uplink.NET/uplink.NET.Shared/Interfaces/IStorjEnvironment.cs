using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace uplink.NET.Interfaces
{
    public interface IStorjEnvironment
    {
        /// <summary>
        /// The uplink-instance
        /// </summary>
        uplink.NET.Models.Uplink Uplink { get; }
        /// <summary>
        /// The project-instance of your project on the satellite
        /// </summary>
        uplink.NET.Models.Project Project { get; }
        /// <summary>
        /// The API-key to use
        /// </summary>
        uplink.NET.Models.APIKey APIKey { get; }
        /// <summary>
        /// True if the environment is successfully initialized
        /// </summary>
        bool IsInitialized { get; }
        /// <summary>
        /// The current EncryptionAccess    
        /// </summary>
        uplink.NET.Models.EncryptionAccess EncryptionAccess { get; }

        /// <summary>
        /// Initializes the current storj-environment
        /// </summary>
        /// <param name="APIKey">The API-key to us</param>
        /// <param name="satellite">The satellite to connect to</param>
        /// <param name="secret">The encryption-secret</param>
        /// <param name="uplinkConfig">The (optional) UplinkConfig to use</param>
        /// <returns>true, if the enviroment could be initialized - false if not</returns>
        bool Initialize(string APIKey, string satellite, string secret, Models.UplinkConfig uplinkConfig = null);

        /// <summary>
        /// Initializes the current storj-environment from a serialized scope
        /// </summary>
        /// <param name="serializedScope">The serialized scope</param>
        /// <param name="uplinkConfig">The (optional) UplinkConfig to use</param>
        /// <returns>true, if the enviroment could be initialized - false if not</returns>
        bool Initialize(string serializedScope, Models.UplinkConfig uplinkConfig = null);
    }
}
