using System;
using System.Collections.Generic;
using System.Text;
using uplink.Net.Contracts.Interfaces;
using uplink.Net.Contracts.Models;

namespace uplink.Net.Services
{
    public class BucketService : IBucketService
    {
        public void CloseBucket(BucketRef bucketRef)
        {
            throw new NotImplementedException();
        }

        public BucketInfo CreateBucket(Project project, string bucketName, BucketConfig bucketConfig)
        {
            LocalModels.BucketConfig _bucketConfig = bucketConfig as LocalModels.BucketConfig;
            LocalModels.Project _project = project as LocalModels.Project;
            string error;
            var res = SWIG.storj_uplink.create_bucket(_project._projectRef, bucketName, _bucketConfig.ToSWIG(), out error);

            return null;
        }

        public void DeleteBucket(Project project, string bucketName)
        {
            throw new NotImplementedException();
        }

        public BucketInfo GetBucketInfo(Project project, string bucketName)
        {
            throw new NotImplementedException();
        }

        public BucketList ListBuckets(Project project, BucketListOptions bucketListOptions)
        {
            string error;
            LocalModels.BucketListOptions _bucketListOptions = bucketListOptions as LocalModels.BucketListOptions;
            LocalModels.Project _project = project as LocalModels.Project;

            var res = SWIG.storj_uplink.list_buckets(_project._projectRef, _bucketListOptions.ToSWIG(), out error);
            var items = res.items;
            throw new NotImplementedException();
        }

        public BucketRef OpenBucket(Project project, string bucketName, EncryptionAccess encryptionAccess)
        {
            throw new NotImplementedException();
        }
    }
}
