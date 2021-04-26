using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class Config
    {
        /// <summary>
        /// The UserAgent to use for Bucket-Attribution
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// DialTimeoutMilliseconds defines how long client should wait for establishing a connection to peers.
        /// </summary>
        public int DialTimeoutMilliseconds { get; set; }
        /// <summary>
        /// Sets the temporary directory to use.
        /// On Android use CacheDir.AbsolutePath. On Windows/UWP use System.IO.Path.GetTempPath().
        /// </summary>
        public string TempDirectory { get; set; }

        internal SWIG.UplinkConfig ToSWIG()
        {
            SWIG.UplinkConfig config = new SWIG.UplinkConfig();
            config.dial_timeout_milliseconds = DialTimeoutMilliseconds;
            config.temp_directory = TempDirectory;
            config.user_agent = UserAgent;
            
            return config;
        }
    }
}
