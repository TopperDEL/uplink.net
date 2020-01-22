using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Exceptions;
using uplink.NET.Interfaces;
using uplink.NET.Models;

namespace uplink.NET.Services
{
    public class BucketService : IBucketService
    {
        Scope _scope;

        public BucketService(Scope scope)
        {
            _scope = scope;
        }

        public async Task<BucketInfo> CreateBucketAsync(string bucketName, BucketConfig bucketConfig)
        {
            string error = string.Empty;

            var bucketInfo = await Task.Run<SWIG.BucketInfo>(() => SWIG.storj_uplink.create_bucket(_scope.Project._projectRef, bucketName, bucketConfig.ToSWIG(), out error));

            if (!string.IsNullOrEmpty(error))
                throw new BucketCreationException(bucketName, error);

            return BucketInfo.FromSWIG(bucketInfo);
        }

        public async Task DeleteBucketAsync(string bucketName)
        {
            string error = string.Empty;

            await Task.Run(() => SWIG.storj_uplink.delete_bucket(_scope.Project._projectRef, bucketName, out error));

            if (!string.IsNullOrEmpty(error))
                throw new BucketDeletionException(bucketName, error);
        }

        public async Task<BucketInfo> GetBucketInfoAsync(string bucketName)
        {
            string error = string.Empty;

            var bucketInfo = await Task.Run<SWIG.BucketInfo>(() => SWIG.storj_uplink.get_bucket_info(_scope.Project._projectRef, bucketName, out error));

            if (!string.IsNullOrEmpty(error))
                throw new BucketNotFoundException(bucketName, error);

            return BucketInfo.FromSWIG(bucketInfo);
        }

        public async Task<BucketList> ListBucketsAsync(BucketListOptions bucketListOptions)
        {
            string error = string.Empty;

            var res = await Task.Run<SWIG.BucketList>(() => SWIG.storj_uplink.list_buckets(_scope.Project._projectRef, bucketListOptions.ToSWIG(), out error));

            if (!string.IsNullOrEmpty(error))
                throw new BucketListException(error);

            return BucketList.FromSWIG(res);
        }

        public async Task<BucketRef> OpenBucketAsync(string bucketName)
        {
            return await OpenBucketAsync(bucketName, _scope.EncryptionAccess);
        }

        public async Task<BucketRef> OpenBucketAsync(string bucketName, EncryptionAccess encryptionAccess)
        {
            string error = string.Empty;

            var handle = await Task.Run<SWIG.BucketRef>(() => SWIG.storj_uplink.open_bucket(_scope.Project._projectRef, bucketName, encryptionAccess.ToBase58(), out error));

            if (!string.IsNullOrEmpty(error))
                throw new BucketNotFoundException(bucketName, error);

            return BucketRef.FromSWIG(handle);
        }

        public async Task CloseBucketAsync(BucketRef bucketRef)
        {
            if (bucketRef == null || bucketRef._bucketRef == null)
                throw new BucketCloseException("Bucket already closed");

            string error = string.Empty;

            await Task.Run(() => SWIG.storj_uplink.close_bucket(bucketRef._bucketRef, out error));

            if (!string.IsNullOrEmpty(error))
                throw new BucketCloseException(error);
        }
    }
}
