using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// Holds the passphrase for encryption
    /// </summary>
    public class EncryptionAccess : IDisposable
    {
        internal SWIG.EncryptionAccessRef _handle;
        private EncryptionAccess()
        {
        }

        internal string ToBase58()
        {
            string error;
            var base58 = SWIG.storj_uplink.serialize_encryption_access(_handle, out error);

            return base58;
        }

        /// <summary>
        /// Creates an EncryptionAccess from a given passphrase for a given project
        /// </summary>
        /// <param name="project">The handle to a project</param>
        /// <param name="secret">The secret passphrase</param>
        /// <returns>An EncryptionAccess</returns>
        public static EncryptionAccess FromPassphrase(Project project, string secret)
        {
            EncryptionAccess access = new EncryptionAccess();

            string error;

            var key = SWIG.storj_uplink.project_salted_key_from_passphrase(project._projectRef, secret, out error);
            if (!string.IsNullOrEmpty(error))
                throw new ArgumentException(error);

            access._handle = SWIG.storj_uplink.new_encryption_access_with_default_key2(key);

            return access;
        }

        public void Dispose()
        {
            if(_handle != null)
            {
                SWIG.storj_uplink.free_encryption_access(_handle);
                _handle = null;
            }
        }
    }
}
