using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// A project
    /// 
    /// Needs to be disposed after use!
    /// </summary>
    public class Project : IDisposable
    {
        internal SWIG.ProjectRef _projectRef = null;

        /// <summary>
        /// Creates a new project-handle for a given uplink, API-key and satellite-address
        /// </summary>
        /// <param name="uplink">The handle to the uplink</param>
        /// <param name="apiKey">The API-key to use</param>
        /// <param name="satelliteAddr">The satellite-address (host:port) to connect to</param>
        public Project(Uplink uplink, APIKey apiKey, string satelliteAddr)
        {
            string error;

            _projectRef = SWIG.storj_uplink.open_project(uplink._uplinkRef, satelliteAddr, apiKey._apiKeyRef, out error);

            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);
            if (_projectRef == null)
                throw new NullReferenceException("Could not open project");
        }

        public void Dispose()
        {
            if (_projectRef != null)
            {
                string error;
                SWIG.storj_uplink.close_project(_projectRef, out error);
                _projectRef = null;
            }
        }
    }
}
