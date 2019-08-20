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
    public class BucketInfo : IBucketInfo
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public int PathCipher { get; set; }
        public long SegmentSize { get; set; }
        public IEncryptionParameters EncryptionParameters { get; set; }
        public IRedundancyScheme RedundancyScheme { get; set; }

        internal BucketInfo(IO.Storj.Libuplink.Mobile.BucketInfo bucketInfo)
        {
            Name = bucketInfo.Name;
            //ToDo Created = bucketInfo.Created;
            PathCipher = bucketInfo.PathCipher;
            SegmentSize = bucketInfo.SegmentsSize;
            //ToDo:
            //EncryptionParameters = new
            //RedundancyScheme = new
        }
    }
}