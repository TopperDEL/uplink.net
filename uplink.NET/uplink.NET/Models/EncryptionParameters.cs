using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class EncryptionParameters:uplink.NET.Contracts.Models.IEncryptionParameters
    {
        public int CipherSuite { get; set; }
        public int BlockSize { get; set; }
    }
}
