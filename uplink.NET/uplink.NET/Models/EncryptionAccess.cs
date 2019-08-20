using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Models
{
    public class EncryptionAccess:uplink.NET.Contracts.Models.IEncryptionAccess
    {
        public string Key { get; set; }
    }
}
