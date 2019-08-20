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
            LocalModels.BucketConfig _bucketConfig = bucketConfig as LocalModels.BucketConfig;
            LocalModels.Project _project = project as LocalModels.Project;
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
            LocalModels.BucketListOptions _bucketListOptions = bucketListOptions as LocalModels.BucketListOptions;
            LocalModels.Project _project = project as LocalModels.Project;

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
