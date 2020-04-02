using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class Config
    {
        public string UserAgent { get; set; }
        public int DialTimeoutMilliseconds { get; set; }
        public string TempDirectory { get; set; }

        internal SWIG.Config ToSWIG()
        {
            SWIG.Config config = new SWIG.Config();
            config.dial_timeout_milliseconds = DialTimeoutMilliseconds;
            config.temp_directory = TempDirectory;
            config.user_agent = UserAgent;

            return config;
        }
    }
}
