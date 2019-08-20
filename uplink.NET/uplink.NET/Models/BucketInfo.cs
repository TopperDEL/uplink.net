using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Contracts.Models;

namespace uplink.NET.Models
{
    public class BucketInfo:uplink.NET.Contracts.Models.IBucketInfo
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public int PathCipher { get; set; }
        public long SegmentSize { get; set; }
        public IEncryptionParameters EncryptionParameters { get; set; }
        public IRedundancyScheme RedundancyScheme { get; set; }
    }
}
