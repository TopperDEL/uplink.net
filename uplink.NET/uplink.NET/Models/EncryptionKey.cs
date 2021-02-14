using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace uplink.NET.Models
{
    public unsafe class EncryptionKey : IDisposable
    {
        internal SWIG.UplinkEncryptionKeyResult _encryptionKeyResulRef;

        /// <summary>
        /// The EncryptionKey to derive a salted encryption key for users when
        /// implementing multitenancy in a single app bucket.
        /// </summary>
        /// <param name="passphrase">The passphrase</param>
        /// <param name="salt">The salt - should be either 16 or 32 bytes</param>
        public EncryptionKey(string passphrase, byte[] salt)
        {
            fixed (byte* arrayPtr = salt)
            {
                _encryptionKeyResulRef = SWIG.storj_uplink.uplink_derive_encryption_key(passphrase, new SWIG.SWIGTYPE_p_void(new IntPtr(arrayPtr), true), (uint)salt.Length);
            }

            if (_encryptionKeyResulRef.error != null && !string.IsNullOrEmpty(_encryptionKeyResulRef.error.message))
                throw new ArgumentException(_encryptionKeyResulRef.error.message);
        }

        public void Dispose()
        {
            if (_encryptionKeyResulRef != null)
            {
                SWIG.storj_uplink.uplink_free_encryption_key_result(_encryptionKeyResulRef);
                _encryptionKeyResulRef.Dispose();
                _encryptionKeyResulRef = null;
            }
        }
    }
}
