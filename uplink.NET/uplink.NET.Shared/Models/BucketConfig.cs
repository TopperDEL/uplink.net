using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// A BucketConfig is needed for the creation of a new bucket
    /// </summary>
    public class BucketConfig
    {
        /// <summary>
        /// Defines the encryption cipher to use for the path
        /// </summary>
        public CipherSuite PathCipher { get; set; }
        /// <summary>
        /// The encryption parameters to use for the bucket content
        /// </summary>
        public EncryptionParameters EncryptionParameters { get; set; }
        /// <summary>
        /// The settings for the redundancy (erasure code) for the bucket content
        /// </summary>
        public RedundancyScheme RedundancyScheme { get; set; }

        /// <summary>
        /// Creates a default BucketConfig
        /// </summary>
        public BucketConfig()
        {
            PathCipher = CipherSuite.STORJ_ENC_AESGCM;
            EncryptionParameters = new EncryptionParameters();
            RedundancyScheme = new RedundancyScheme();
        }

        internal SWIG.BucketConfig ToSWIG()
        {
            SWIG.BucketConfig swig = new SWIG.BucketConfig();
            swig.encryption_parameters = Models.EncryptionParameters.ToSWIG(EncryptionParameters);
            swig.path_cipher = CipherSuiteHelper.ToSWIG(PathCipher);
            swig.redundancy_scheme = Models.RedundancyScheme.ToSWIG(RedundancyScheme);

            return swig;
        }
    }
}
