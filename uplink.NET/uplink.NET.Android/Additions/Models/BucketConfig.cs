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
    public class BucketConfig : IBucketConfig
    {
        public int PathCipher { get; set; }
        public IEncryptionParameters EncryptionParameters { get; set; }
        public IRedundancyScheme RedundancyScheme { get; set; }

        internal IO.Storj.Libuplink.Mobile.BucketConfig ToJava()
        {
            IO.Storj.Libuplink.Mobile.BucketConfig bucketConfig = new IO.Storj.Libuplink.Mobile.BucketConfig();

            //ToDo: Mapping

            return bucketConfig;
        }
    }
}