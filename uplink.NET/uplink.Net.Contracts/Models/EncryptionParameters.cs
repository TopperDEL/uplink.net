using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public abstract class EncryptionParameters
    {
        public int CipherSuite { get; set; }
        public int BlockSize { get; set; }
    }
}
