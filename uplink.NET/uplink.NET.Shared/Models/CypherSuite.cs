using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public enum CipherSuite
    {
        STORJ_ENC_UNSPECIFIED = 0,
        STORJ_ENC_NULL = 1,
        STORJ_ENC_AESGCM = 2,
        STORJ_ENC_SECRET_BOX = 3
    }
}
