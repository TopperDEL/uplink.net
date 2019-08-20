using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Models
{
    public class EncryptionAccess:uplink.Net.Contracts.Models.IEncryptionAccess
    {
        public string Key { get; set; }
    }
}
