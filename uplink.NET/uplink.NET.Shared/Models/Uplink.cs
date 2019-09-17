using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The uplink is the environment for connecting to the storj-network.
    /// 
    /// Needs to be disposed after use!
    /// </summary>
    public class Uplink : IDisposable
    {
        internal SWIG.UplinkRef _uplinkRef = null;
        internal SWIG.UplinkConfig _uplinkConfig = null;

        /// <summary>
        /// Creates an Uplink-Instance
        /// </summary>
        /// <param name="uplinkConfig">The UplinkConfig to use</param>
        /// <param name="tempDir">The temp directory to use - must be set on Android!</param>
        public Uplink(UplinkConfig uplinkConfig, string tempDir = "inmemory")
        {
            string error;

            _uplinkConfig = new SWIG.UplinkConfig();
            _uplinkConfig.Volatile.tls.skip_peer_ca_whitelist = uplinkConfig.Volatile_TLS_SkipPeerCAWhitelist;
            _uplinkRef = SWIG.storj_uplink.new_uplink(_uplinkConfig, out error, tempDir);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);
            if (_uplinkRef == null)
                throw new NullReferenceException("No Uplink-reference created");
        }

        public void Dispose()
        {
            if (_uplinkConfig != null)
            {
                _uplinkConfig.Dispose();
                _uplinkConfig = null;
            }
            if (_uplinkRef != null)
            {
                string error;
                SWIG.storj_uplink.close_uplink(_uplinkRef, out error);
                _uplinkRef = null;
            }
        }
    }
}
