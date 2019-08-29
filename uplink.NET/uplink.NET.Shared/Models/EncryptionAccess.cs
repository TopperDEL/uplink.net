using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class EncryptionAccess
    {
        internal SWIG.EncryptionAccessRef _handle;
        private EncryptionAccess()
        {
        }

        public string ToBase58()
        {
            string error;
            var base58 = SWIG.storj_uplink.serialize_encryption_access(_handle, out error);

            return base58;
        }

        public static EncryptionAccess FromPassphrase(Project project, string secret)
        {
            EncryptionAccess access = new EncryptionAccess();

            string error;

            var key = SWIG.storj_uplink.project_salted_key_from_passphrase(project._projectRef, secret, out error);
            access._handle = SWIG.storj_uplink.new_encryption_access_with_default_key2(key);

            return access;
        }
    }
}
