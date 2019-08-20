using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Contracts.Models
{
    public interface IBucketConfig
    {
        int PathCipher { get; set; }
        IEncryptionParameters EncryptionParameters { get; set; }
        IRedundancyScheme RedundancyScheme { get; set; }
    }
}
