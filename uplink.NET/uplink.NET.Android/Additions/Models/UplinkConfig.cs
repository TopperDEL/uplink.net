using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using uplink.NET.Contracts.Models;

namespace uplink.NET.Android.Binding.Additions.Models
{
    public class UplinkConfig : IUplinkConfig
    {
        public bool Volatile_TLS_SkipPeerCAWhitelist { get; set; }

        internal IO.Storj.Libuplink.Mobile.Config ToJava()
        {
            IO.Storj.Libuplink.Mobile.Config config = new IO.Storj.Libuplink.Mobile.Config();

            //ToDo: Mapping

            return config;
        }
    }
}