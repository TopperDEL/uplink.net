using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.Net.Contracts.Models
{
    public interface IEncryptionAccess
    {
        string Key { get; set; }
    }
}
