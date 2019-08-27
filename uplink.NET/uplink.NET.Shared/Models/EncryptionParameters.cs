using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class EncryptionParameters
    {
        public CipherSuite CipherSuite { get; set; }
        public int BlockSize { get; set; }

        internal static EncryptionParameters FromSWIG(SWIG.EncryptionParameters original)
        {
            EncryptionParameters ret = new EncryptionParameters();
            ret.BlockSize = original.block_size;
            ret.CipherSuite = CipherSuiteHelper.FromSWIG(original.cipher_suite);

            return ret;
        }
    }
}
