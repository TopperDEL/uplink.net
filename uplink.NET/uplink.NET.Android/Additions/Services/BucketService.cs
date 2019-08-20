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
using uplink.NET.Android.Binding.Additions.Models;
using uplink.NET.Contracts.Models;

namespace uplink.NET.Android.Services
{
    public class BucketService : uplink.NET.Contracts.Interfaces.IBucketService
    {
        public void CloseBucket(IBucketRef bucketRef)
        {
            throw new NotImplementedException();
        }

        public IBucketInfo CreateBucket(IProject project, string bucketName, IBucketConfig bucketConfig)
        {
            IO.Storj.Libuplink.Mobile.Project _project = ((Project)project)._projectRef;
            IO.Storj.Libuplink.Mobile.BucketConfig _bucketConfig = new IO.Storj.Libuplink.Mobile.BucketConfig();

            var result = _project.CreateBucket(bucketName, ((BucketConfig)bucketConfig).ToJava());

            return new BucketInfo(result);
        }

        public void DeleteBucket(IProject project, string bucketName)
        {
            throw new NotImplementedException();
        }

        public IBucketInfo GetBucketInfo(IProject project, string bucketName)
        {
            throw new NotImplementedException();
        }

        public IBucketList ListBuckets(IProject project, IBucketListOptions bucketListOptions)
        {
            throw new NotImplementedException();
        }

        public IBucketRef OpenBucket(IProject project, string bucketName, IEncryptionAccess encryptionAccess)
        {
            throw new NotImplementedException();
        }
    }
}