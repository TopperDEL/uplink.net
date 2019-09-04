using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    /// <summary>
    /// The cipher suite
    /// </summary>
    public enum CipherSuite
    {
        STORJ_ENC_UNSPECIFIED = 0,
        STORJ_ENC_NULL = 1,
        STORJ_ENC_AESGCM = 2,
        STORJ_ENC_SECRET_BOX = 3
    }

    internal static class CipherSuiteHelper
    {
        internal static CipherSuite FromSWIG(SWIG.CipherSuite original)
        {
           return (CipherSuite)Enum.Parse(typeof(CipherSuite), original.ToString());
        }

        internal static SWIG.CipherSuite ToSWIG(CipherSuite original)
        {
            return (SWIG.CipherSuite)Enum.Parse(typeof(SWIG.CipherSuite), original.ToString());
        }
    }
}
