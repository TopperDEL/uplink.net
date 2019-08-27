using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Exceptions;
using uplink.NET.Interfaces;
using uplink.NET.Models;

namespace uplink.NET.Services
{
    public class BucketService : IBucketService
    {
        public BucketInfo CreateBucket(Project project, string bucketName, BucketConfig bucketConfig)
        {
            string error;
            var res = SWIG.storj_uplink.create_bucket(project._projectRef, bucketName, bucketConfig.ToSWIG(), out error);

            if (!string.IsNullOrEmpty(error))
                throw new BucketCreationException(bucketName, error);

            return BucketInfo.FromSWIG(res);
        }

        public void DeleteBucket(Project project, string bucketName)
        {
            string error;

            SWIG.storj_uplink.delete_bucket(project._projectRef, bucketName, out error);

            if (!string.IsNullOrEmpty(error))
                throw new BucketDeletionException(bucketName, error);
        }

        public BucketInfo GetBucketInfo(Project project, string bucketName)
        {
            string error;

            var bucketInfo = SWIG.storj_uplink.get_bucket_info(project._projectRef, bucketName, out error);

            if (!string.IsNullOrEmpty(error))
                throw new BucketNotFoundException(bucketName, error);

            return BucketInfo.FromSWIG(bucketInfo);
        }

        public BucketList ListBuckets(Project project, BucketListOptions bucketListOptions)
        {
            string error;

            var res = SWIG.storj_uplink.list_buckets(project._projectRef, bucketListOptions.ToSWIG(), out error);

            return BucketList.FromSWIG(res);
        }

        public BucketRef OpenBucket(Project project, string bucketName, EncryptionAccess encryptionAccess)
        {
            string error;

            var handle = SWIG.storj_uplink.open_bucket(project._projectRef, bucketName, encryptionAccess.ToBase58(), out error);

            if (!string.IsNullOrEmpty(error))
                throw new BucketNotFoundException(bucketName, error);

            return BucketRef.FromSWIG(handle);
        }

        public void CloseBucket(BucketRef bucketRef)
        {
            string error;

            SWIG.storj_uplink.close_bucket(bucketRef._bucketRef, out error);
        }
    }
}
