using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// A BucketInfo contains information about a specific bucket
    /// </summary>
    public class BucketInfo
    {
        /// <summary>
        /// The name of the bucket
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The date the bucket got created
        /// </summary>
        public DateTime Created { get; private set; }
        /// <summary>
        /// The cipher used for the paths within this bucket
        /// </summary>
        public CipherSuite PathCipher { get; private set; }
        /// <summary>
        /// The segment-size
        /// </summary>
        public ulong SegmentSize { get; private set; }
        /// <summary>
        /// The ecnryption parameters for the contents of the bucket
        /// </summary>
        public EncryptionParameters EncryptionParameters { get; private set; }
        /// <summary>
        /// The redundancy setting (ersaure codes) for the contents of the bucket
        /// </summary>
        public RedundancyScheme RedundancyScheme { get; private set; }

        internal static BucketInfo FromSWIG(SWIG.BucketInfo original, bool disposeBucketInfo = true)
        {
            BucketInfo ret = new BucketInfo();
            ret.Created = DateTimeOffset.FromUnixTimeSeconds(original.created).ToLocalTime().DateTime;
            ret.EncryptionParameters = EncryptionParameters.FromSWIG(original.encryption_parameters);
            ret.Name = original.name;
            ret.PathCipher = CipherSuiteHelper.FromSWIG(original.path_cipher);
            ret.RedundancyScheme = RedundancyScheme.FromSWIG(original.redundancy_scheme);
            ret.SegmentSize = original.segment_size;

            if (disposeBucketInfo)
                SWIG.storj_uplink.free_bucket_info(original);

            return ret;
        }
    }
}
