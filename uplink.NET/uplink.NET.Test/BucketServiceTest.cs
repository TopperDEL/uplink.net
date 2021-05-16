using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Exceptions;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Services;

namespace uplink.NET.Test
{
    [TestClass]
    public class BucketServiceTest
    {
        IBucketService _service;
        IObjectService _objectService;
        Access _access;

        [TestInitialize]
        public void Init()
        {
            Access.SetTempDirectory(System.IO.Path.GetTempPath());
            _access = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET);
            _service = new BucketService(_access);
            _objectService = new ObjectService(_access);
        }

        [TestMethod]
        public async Task CreateBucket_Creates_NewBucket()
        {
            string bucketname = "createbucket-creates-newbucket";

            var result = await _service.CreateBucketAsync(bucketname);

            Assert.AreEqual(bucketname, result.Name);
        }

        [TestMethod]
        public async Task CreateBucket_Fails_OnBucketAlreadyExisting()
        {
            string bucketname = "createbucket-fails-onbucketalreadyexisting";

            await _service.CreateBucketAsync(bucketname); //Should work

            try
            {
                var resultFailed = await _service.CreateBucketAsync(bucketname); //Should fail
            }catch(BucketCreationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("already exists"));
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "CreateBucket did not throw exception on already existing bucket");
        }

        [TestMethod]
        public async Task EnsureBucket_Creates_NewBucket()
        {
            string bucketname = "ensurebucket-creates-newbucket";

            var result = await _service.EnsureBucketAsync(bucketname);

            Assert.AreEqual(bucketname, result.Name);
        }

        [TestMethod]
        public async Task EnsureBucket_Returns_BucketEvenIfItExistsAlready()
        {
            string bucketname = "ensurebucket-returns-bucketevenifitexistsalready";

            var result = await _service.CreateBucketAsync(bucketname);

            Assert.AreEqual(bucketname, result.Name);

            var resultEnsured = await _service.EnsureBucketAsync(bucketname);

            Assert.AreEqual(bucketname, resultEnsured.Name);
        }

        [TestMethod]
        public async Task GetBucket_Retrieves_Bucket()
        {
            string bucketname = "getbucket-retrieves-bucket";

            await _service.CreateBucketAsync(bucketname);
            var bucket = await _service.GetBucketAsync(bucketname);

            Assert.AreEqual(bucketname, bucket.Name);
        }

        [TestMethod]
        public async Task GetBucket_Fails_OnNotExistingBucket()
        {
            string bucketname = "getbucket-fails-onnotexistingbucket";

            try
            {
                var bucket = await _service.GetBucketAsync(bucketname);
            }
            catch(BucketNotFoundException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "GetBucket did not throw exception on not existing bucket");
        }

        [TestMethod]
        public async Task DeleteBucket_Deletes_Bucket()
        {
            string bucketname = "deletebucket-deletes-bucket";

            await _service.CreateBucketAsync(bucketname);

            await _service.DeleteBucketAsync(bucketname);

            try
            {
                await _service.GetBucketAsync(bucketname);
            }
            catch (BucketNotFoundException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "DeleteBucket did not delete Bucket as it seems to still exist");
        }

        [TestMethod]
        public async Task DeleteBucket_Fails_OnNotExistingBucket()
        {
            string bucketname = "deletebucket-fails-onnotexistingbucket";

            try
            {
                await _service.DeleteBucketAsync(bucketname);
            }
            catch (BucketDeletionException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "DeleteBucket did not throw exception on not existing bucket");
        }

        [TestMethod]
        public async Task DeleteBucketWithObjects_Deletes_Bucket()
        {
            string bucketname = "deletebucketwithobjects-deletes-bucket";

            var bucket = await _service.CreateBucketAsync(bucketname);

            var upload = await _objectService.UploadObjectAsync(bucket, "test.txt", new UploadOptions(), new byte[] { 1, 2, 3, 4 }, false);
            await upload.StartUploadAsync();
            Assert.IsTrue(upload.Completed);

            await _service.DeleteBucketWithObjectsAsync(bucketname);

            try
            {
                await _service.GetBucketAsync(bucketname);
            }
            catch (BucketNotFoundException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "DeleteBucket did not delete Bucket as it seems to still exist");
        }

        [TestMethod]
        public async Task DeleteBucketWithObjects_Fails_OnNotExistingBucket()
        {
            string bucketname = "deletebucketwithobjects-fails-onnotexistingbucket";

            try
            {
                await _service.DeleteBucketWithObjectsAsync(bucketname);
            }
            catch (BucketDeletionException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "DeleteBucketWithObject did not throw exception on not existing bucket");
        }

        [TestMethod]
        public async Task ListBuckets_Lists_TwoNewlyCreatedBuckets()
        {
            string bucketname1 = "listbucket-lists-mytwobuckets-bucket1";
            string bucketname2 = "listbucket-lists-mytwobuckets-bucket2";

            await _service.CreateBucketAsync(bucketname1);
            await _service.CreateBucketAsync(bucketname2);

            var result = await _service.ListBucketsAsync(new ListBucketsOptions());

            Assert.IsTrue(result.Items.Count >= 2);
            int foundBuckets = 0;
            foreach(var bucketInfo in result.Items)
            {
                if (bucketInfo.Name == bucketname1 || bucketInfo.Name == bucketname2)
                    foundBuckets++;
            }
            Assert.AreEqual(2, foundBuckets);
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            await DeleteBucketAsync("createbucket-creates-newbucket");
            await DeleteBucketAsync("ensurebucket-creates-newbucket");
            await DeleteBucketAsync("ensurebucket-returns-bucketevenifitexistsalready");
            await DeleteBucketAsync("createbucket-fails-onbucketalreadyexisting");
            await DeleteBucketAsync("getbucket-retrieves-bucket");
            await DeleteBucketAsync("listbucket-lists-mytwobuckets-bucket1");
            await DeleteBucketAsync("listbucket-lists-mytwobuckets-bucket2");
            await DeleteBucketAsync("deletebucket-deletes-bucket");
            await DeleteBucketAsync("deletebucketwithobjects-deletes-bucket");
        }

        private async Task DeleteBucketAsync(string bucketName)
        {
            try
            {
                await _service.DeleteBucketAsync(bucketName);
            }
            catch
            { }
        }
    }
}
