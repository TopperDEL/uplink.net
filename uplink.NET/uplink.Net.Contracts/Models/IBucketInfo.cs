using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public interface IBucketInfo
    {
        string Name { get; set; }
        DateTime Created { get; set; }
        int PathCipher { get; set; }
        int SegmentSize { get; set; }
        IEncryptionParameters EncryptionParameters { get; set; }
        IRedundancyScheme RedundancyScheme { get; set; }
    }
}
