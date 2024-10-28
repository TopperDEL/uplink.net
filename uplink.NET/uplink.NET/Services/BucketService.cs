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
        static List<SWIG.UplinkListBucketsOptions> _listOptions = new List<SWIG.UplinkListBucketsOptions>(); //ToDo: Temporary until SWIG does not enforce IDisposable on UplinkListBucketsOptions

        private readonly Access _access;

        public BucketService(Access access)
        {
            _access = access;
        }

        public async Task<Bucket> CreateBucketAsync(string bucketName)
        {
            Console.WriteLine("CreateBucketAsync: Creating bucket with name: {0}", bucketName);
            using (SWIG.UplinkBucketResult bucketResult = await Task.Run(() => SWIG.storj_uplink.uplink_create_bucket(_access._project, bucketName)).ConfigureAwait(false))
            {
                if (bucketResult.error != null && !string.IsNullOrEmpty(bucketResult.error.message))
                {
                    Console.WriteLine("CreateBucketAsync: Error creating bucket: {0}", bucketResult.error.message);
                    throw new BucketCreationException(bucketName, bucketResult.error.message);
                }

                var bucket = Bucket.FromSWIG(bucketResult.bucket, _access._project, bucketResult);
                Console.WriteLine("CreateBucketAsync: Successfully created bucket: {0}", bucketName);
                return bucket;
            }
        }

        public async Task<Bucket> EnsureBucketAsync(string bucketName)
        {
            Console.WriteLine("EnsureBucketAsync: Ensuring bucket with name: {0}", bucketName);
            using (SWIG.UplinkBucketResult bucketResult = await Task.Run(() => SWIG.storj_uplink.uplink_ensure_bucket(_access._project, bucketName)).ConfigureAwait(false))
            {
                if (bucketResult.error != null && !string.IsNullOrEmpty(bucketResult.error.message))
                {
                    Console.WriteLine("EnsureBucketAsync: Error ensuring bucket: {0}", bucketResult.error.message);
                    throw new BucketCreationException(bucketName, bucketResult.error.message);
                }

                var bucket = Bucket.FromSWIG(bucketResult.bucket, _access._project, bucketResult);
                Console.WriteLine("EnsureBucketAsync: Successfully ensured bucket: {0}", bucketName);
                return bucket;
            }
        }

        public async Task DeleteBucketAsync(string bucketName)
        {
            Console.WriteLine("DeleteBucketAsync: Deleting bucket with name: {0}", bucketName);
            using (SWIG.UplinkBucketResult bucketResult = await Task.Run(() => SWIG.storj_uplink.uplink_delete_bucket(_access._project, bucketName)).ConfigureAwait(false))
            {
                if (bucketResult.error != null && !string.IsNullOrEmpty(bucketResult.error.message))
                {
                    Console.WriteLine("DeleteBucketAsync: Error deleting bucket: {0}", bucketResult.error.message);
                    throw new BucketDeletionException(bucketName, bucketResult.error.message);
                }
                Console.WriteLine("DeleteBucketAsync: Successfully deleted bucket: {0}", bucketName);
            }
        }

        public async Task DeleteBucketWithObjectsAsync(string bucketName)
        {
            Console.WriteLine("DeleteBucketWithObjectsAsync: Deleting bucket with objects, name: {0}", bucketName);
            using (SWIG.UplinkBucketResult bucketResult = await Task.Run(() => SWIG.storj_uplink.uplink_delete_bucket_with_objects(_access._project, bucketName)).ConfigureAwait(false))
            {
                if (bucketResult.error != null && !string.IsNullOrEmpty(bucketResult.error.message))
                {
                    Console.WriteLine("DeleteBucketWithObjectsAsync: Error deleting bucket with objects: {0}", bucketResult.error.message);
                    throw new BucketDeletionException(bucketName, bucketResult.error.message);
                }
                Console.WriteLine("DeleteBucketWithObjectsAsync: Successfully deleted bucket with objects: {0}", bucketName);
            }
        }

        public async Task<Bucket> GetBucketAsync(string bucketName)
        {
            Console.WriteLine("GetBucketAsync: Retrieving bucket with name: {0}", bucketName);
            using (SWIG.UplinkBucketResult bucketResult = await Task.Run(() => SWIG.storj_uplink.uplink_stat_bucket(_access._project, bucketName)).ConfigureAwait(false))
            {
                if (bucketResult.error != null && !string.IsNullOrEmpty(bucketResult.error.message))
                {
                    Console.WriteLine("GetBucketAsync: Error retrieving bucket: {0}", bucketResult.error.message);
                    throw new BucketNotFoundException(bucketName, bucketResult.error.message);
                }

                var bucket = Bucket.FromSWIG(bucketResult.bucket, _access._project, bucketResult);
                Console.WriteLine("GetBucketAsync: Successfully retrieved bucket: {0}", bucketName);
                return bucket;
            }
        }

        public async Task<BucketList> ListBucketsAsync(ListBucketsOptions listBucketsOptions)
        {
            Console.WriteLine("ListBucketsAsync: Listing buckets with options: {0}", listBucketsOptions);
            var listBucketsOptionsSWIG = listBucketsOptions.ToSWIG();
            _listOptions.Add(listBucketsOptionsSWIG);
            using (SWIG.UplinkBucketIterator bucketIterator = await Task.Run(() => SWIG.storj_uplink.uplink_list_buckets(_access._project, listBucketsOptionsSWIG)).ConfigureAwait(false))
            {
                using (SWIG.UplinkError error = SWIG.storj_uplink.uplink_bucket_iterator_err(bucketIterator))
                {
                    if (error != null && !string.IsNullOrEmpty(error.message))
                    {
                        Console.WriteLine("ListBucketsAsync: Error listing buckets: {0}", error.message);
                        throw new BucketListException(error.message);
                    }
                }

                BucketList bucketList = new BucketList();

                while (SWIG.storj_uplink.uplink_bucket_iterator_next(bucketIterator))
                {
                    var bucket = SWIG.storj_uplink.uplink_bucket_iterator_item(bucketIterator);
                    bucketList.Items.Add(Bucket.FromSWIG(bucket, _access._project));
                }
                Console.WriteLine("ListBucketsAsync: Successfully listed buckets");
                return bucketList;
            }
        }
    }
}
