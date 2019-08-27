using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class BucketInfo
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public CipherSuite PathCipher { get; set; }
        public ulong SegmentSize { get; set; }
        public EncryptionParameters EncryptionParameters { get; set; }
        public RedundancyScheme RedundancyScheme { get; set; }

        internal static BucketInfo FromSWIG(SWIG.BucketInfo original)
        {
            BucketInfo ret = new BucketInfo();
            ret.Created = DateTimeOffset.FromUnixTimeSeconds(original.created).ToLocalTime().DateTime;
            ret.EncryptionParameters = EncryptionParameters.FromSWIG(original.encryption_parameters);
            ret.Name = original.name;
            ret.PathCipher = CipherSuiteHelper.FromSWIG(original.path_cipher);
            ret.RedundancyScheme = RedundancyScheme.FromSWIG(original.redundancy_scheme);
            ret.SegmentSize = original.segment_size;

            SWIG.storj_uplink.free_bucket_info(original);

            return ret;
        }
    }
}
