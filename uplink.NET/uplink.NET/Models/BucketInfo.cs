using System;
using System.Collections.Generic;
using System.Text;
using uplink.Net.Contracts.Models;

namespace uplink.Net.Models
{
    public class BucketInfo:uplink.Net.Contracts.Models.IBucketInfo
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public int PathCipher { get; set; }
        public int SegmentSize { get; set; }
        public IEncryptionParameters EncryptionParameters { get; set; }
        public IRedundancyScheme RedundancyScheme { get; set; }
    }
}
