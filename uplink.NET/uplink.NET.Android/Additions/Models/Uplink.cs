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
    public class Uplink : IUplink
    {
        internal IO.Storj.Libuplink.Mobile.Uplink _uplinkRef = null;

        public Uplink(UplinkConfig uplinkConfig, string tempPath)
        {
            _uplinkRef = new IO.Storj.Libuplink.Mobile.Uplink(uplinkConfig.ToJava(), tempPath);
        }

        public void Dispose()
        {
            if(_uplinkRef != null)
            {
                _uplinkRef.Dispose();
                _uplinkRef = null;
            }
        }
    }
}