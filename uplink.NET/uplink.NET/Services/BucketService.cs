using System;
using System.Collections.Generic;
using System.Text;
using uplink.Net.Contracts.Interfaces;
using uplink.Net.Contracts.Models;

namespace uplink.Net.Services
{
    public class BucketService : IBucketService
    {
        public void CloseBucket(IBucketRef bucketRef)
        {
            throw new NotImplementedException();
        }

        public IBucketInfo CreateBucket(IProject project, string bucketName, IBucketConfig bucketConfig)
        {
            Models.BucketConfig _bucketConfig = bucketConfig as Models.BucketConfig;
            Models.Project _project = project as Models.Project;
            string error;
            var res = SWIG.storj_uplink.create_bucket(_project._projectRef, bucketName, _bucketConfig.ToSWIG(), out error);

            return null;
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
            string error;
            Models.BucketListOptions _bucketListOptions = bucketListOptions as Models.BucketListOptions;
            Models.Project _project = project as Models.Project;

            var res = SWIG.storj_uplink.list_buckets(_project._projectRef, _bucketListOptions.ToSWIG(), out error);
            var items = res.items;
            throw new NotImplementedException();
        }

        public IBucketRef OpenBucket(IProject project, string bucketName, IEncryptionAccess encryptionAccess)
        {
            throw new NotImplementedException();
        }
    }
}
