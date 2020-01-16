using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// EncryptionsRestrictions describe, for which bucket and path-prefix a scope/restricted API-key has access to.
    /// </summary>
    public class EncryptionRestriction
    {
        /// <summary>
        /// The bucket-name for which this EncryptionRestriction is meant for
        /// </summary>
        public string Bucket { get; set; }

        /// <summary>
        /// The path-prefix for which this EncryptionRestriction is meant for
        /// </summary>
        public string PathPrefix { get; set; }
    }
}
