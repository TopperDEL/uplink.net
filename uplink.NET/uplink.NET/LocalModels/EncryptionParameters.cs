using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.LocalModels
{
    public class EncryptionParameters:uplink.Net.Contracts.Models.IEncryptionParameters
    {
        public int CipherSuite { get; set; }
        public int BlockSize { get; set; }
    }
}
