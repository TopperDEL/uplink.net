using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Contracts.Models
{
    public interface IEncryptionParameters
    {
        int CipherSuite { get; set; }
        int BlockSize { get; set; }
    }
}
