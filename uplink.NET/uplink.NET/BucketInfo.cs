using System;
using System.Collections.Generic;
using System.Text;

namespace uplink
{
    public class BucketInfo
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public int PathCipher { get; set; }
        public int SegmentSize { get; set; }
        public EncryptionParameters EncryptionParameters { get; set; }
        public RedundancyScheme RedundancyScheme { get; set; }
    }
}
