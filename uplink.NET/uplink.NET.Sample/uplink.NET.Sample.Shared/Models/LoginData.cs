using System;
using System.Collections.Generic;
using System.Text;

namespace uplink.NET.Sample.Shared.Models
{
    public class LoginData
    {
        public string Satellite { get; set; }
        public string APIKey { get; set; }
        public string Secret { get; set; }

        public LoginData()
        {
            Satellite = "europe-west-1.tardigrade.io:7777";
        }
    }
}
