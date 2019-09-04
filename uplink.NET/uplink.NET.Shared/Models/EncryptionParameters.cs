using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class EncryptionParameters
    {
        public CipherSuite CipherSuite { get; set; }
        public int BlockSize { get; set; }

        public EncryptionParameters()
        {
            CipherSuite = CipherSuite.STORJ_ENC_AESGCM;
        }

        internal static EncryptionParameters FromSWIG(SWIG.EncryptionParameters original)
        {
            EncryptionParameters ret = new EncryptionParameters();
            ret.BlockSize = original.block_size;
            ret.CipherSuite = CipherSuiteHelper.FromSWIG(original.cipher_suite);

            return ret;
        }

        internal static SWIG.EncryptionParameters ToSWIG(EncryptionParameters original)
        {
            SWIG.EncryptionParameters ret = new SWIG.EncryptionParameters();
            ret.block_size = original.BlockSize;
            ret.cipher_suite = CipherSuiteHelper.ToSWIG(original.CipherSuite);

            return ret;
        }
    }
}
