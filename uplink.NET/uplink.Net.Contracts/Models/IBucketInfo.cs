using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Contracts.Models
{
    public interface IBucketInfo
    {
        string Name { get; set; }
        DateTime Created { get; set; }
        int PathCipher { get; set; }
        long SegmentSize { get; set; }
        IEncryptionParameters EncryptionParameters { get; set; }
        IRedundancyScheme RedundancyScheme { get; set; }
    }
}
