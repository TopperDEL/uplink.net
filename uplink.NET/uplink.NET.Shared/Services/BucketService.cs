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

        public async Task<Bucket> CreateBucketAsync(string bucketName)
        {
            SWIG.BucketResult bucketResult = await Task.Run(() => SWIG.storj_uplink.create_bucket(_scope.Project, bucketName));

            if (bucketResult.error != null && !string.IsNullOrEmpty(bucketResult.error.message))
                throw new BucketCreationException(bucketName, bucketResult.error.message);

            return Bucket.FromSWIG(bucketResult.bucket);
        }

        public async Task DeleteBucketAsync(string bucketName)
        {
            SWIG.BucketResult bucketResult = await Task.Run(() => SWIG.storj_uplink.delete_bucket(_scope.Project, bucketName));

            if (bucketResult.error != null && !string.IsNullOrEmpty(bucketResult.error.message))
                throw new BucketDeletionException(bucketName, bucketResult.error.message);
        }

        public async Task<Bucket> GetBucketAsync(string bucketName)
        {
            SWIG.BucketResult bucketResult = await Task.Run(() => SWIG.storj_uplink.stat_bucket(_scope.Project, bucketName));

            if (bucketResult.error != null && !string.IsNullOrEmpty(bucketResult.error.message))
                throw new BucketNotFoundException(bucketName, bucketResult.error.message);

            return Bucket.FromSWIG(bucketResult.bucket);
        }

        public async Task<BucketList> ListBucketsAsync(ListBucketsOptions listBucketsOptions)
        {
            SWIG.BucketIterator bucketIterator = await Task.Run(() => SWIG.storj_uplink.list_buckets(_scope.Project, listBucketsOptions.ToSWIG()));

            var error = SWIG.storj_uplink.bucket_iterator_err(bucketIterator);
            if (error != null && !string.IsNullOrEmpty(error.message))
                throw new BucketListException(error.message);

            BucketList bucketList = new BucketList();

            while(SWIG.storj_uplink.bucket_iterator_next(bucketIterator))
            {
                bucketList.Items.Add(Bucket.FromSWIG(SWIG.storj_uplink.bucket_iterator_item(bucketIterator)));
            }
            SWIG.storj_uplink.free_bucket_iterator(bucketIterator);

            return bucketList;
        }
    }
}
