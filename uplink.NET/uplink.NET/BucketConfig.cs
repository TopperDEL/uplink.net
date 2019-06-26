using System;
using System.Collections.Generic;
using System.Text;

namespace uplink
{
    public class BucketConfig
    {
        public int PathCipher { get; set; }
        public EncryptionParameters EncryptionParameters { get; set; }
        public RedundancyScheme RedundancyScheme { get; set; }
    }
}
