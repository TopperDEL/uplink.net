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
        BucketConfig _bucketConfig;
        IStorjEnvironment _environment;

        [TestInitialize]
        public void Init()
        {
            StorjEnvironment.SetTempDirectory(System.IO.Path.GetTempPath());
            _environment = new StorjEnvironment();
            _environment.Initialize(TestConstants.VALID_API_KEY, TestConstants.SATELLITE_URL, TestConstants.ENCRYPTION_SECRET);
            _service = new BucketService(_environment);
            _bucketConfig = new BucketConfig();
        }

        [TestMethod]
        public async Task CreateBucket_Creates_NewBucket()
        {
            string bucketname = "createbucket-creates-newbucket";

            var result = await _service.CreateBucketAsync(bucketname, _bucketConfig);

            Assert.AreEqual(bucketname, result.Name);
        }

        [TestMethod]
        public async Task CreateBucket_Fails_OnBucketAlreadyExisting()
        {
            string bucketname = "createbucket-fails-onbucketalreadyexisting";

            await _service.CreateBucketAsync(bucketname, _bucketConfig); //Should work

            try
            {
                var resultFailed = await _service.CreateBucketAsync(bucketname, _bucketConfig); //Should fail
            }catch(BucketCreationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("already exists"));
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "CreateBucket did not throw exception on already existing bucket");
        }

        [TestMethod]
        public async Task GetBucketInfo_Retrieves_BucketInfo()
        {
            string bucketname = "getbucketinfo-retrieves-bucketinfo";

            await _service.CreateBucketAsync(bucketname, _bucketConfig);
            var bucketInfo = await _service.GetBucketInfoAsync(bucketname);

            Assert.AreEqual(bucketname, bucketInfo.Name);
        }

        [TestMethod]
        public async Task GetBucketInfo_Fails_OnNotExistingBucket()
        {
            string bucketname = "getbucketinfo-fails-onnotexistingbucket";

            try
            {
                var bucketInfo = await _service.GetBucketInfoAsync(bucketname);
            }
            catch(BucketNotFoundException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "GetBucketInfo did not throw exception on not existing bucket");
        }

        [TestMethod]
        public async Task OpenBucket_Returns_BucketHandle()
        {
            string bucketname = "openbucket-returns-buckethandle";

            await _service.CreateBucketAsync(bucketname, _bucketConfig);
            var bucketHandle = await _service.OpenBucketAsync(bucketname);

            Assert.IsNotNull(bucketHandle);
        }

        [TestMethod]
        public async Task OpenBucket_Fails_OnNotExistingBucket()
        {
            string bucketname = "openbucket-fails-onnotexistingbucket";

            try
            {
                var bucketHandle = await _service.OpenBucketAsync(bucketname);
            }
            catch (BucketNotFoundException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "OpenBucket did not throw exception on not existing bucket");
        }

        [TestMethod]
        public async Task DeleteBucket_Deletes_Bucket()
        {
            string bucketname = "deletebucket-deletes-bucket";

            await _service.CreateBucketAsync(bucketname, _bucketConfig);

            await _service.DeleteBucketAsync(bucketname);

            try
            {
                await _service.GetBucketInfoAsync(bucketname);
            }
            catch (BucketNotFoundException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "DeleteBucket did not delete Bucket as it seems to still exist");
        }

        //Not implemented on the storj-side - activate if delete_bucket throws exception for not existing bucket
        //[TestMethod]
        //public void DeleteBucket_Fails_OnNotExistingBucket()
        //{
        //    string bucketname = "deletebucket-fails-onnotexistingbucket";

        //    try
        //    {
        //        _service.DeleteBucket(_project, bucketname);
        //    }
        //    catch (BucketNotFoundException ex)
        //    {
        //        Assert.AreEqual(bucketname, ex.BucketName);
        //        return;
        //    }

        //    Assert.IsTrue(false, "DeleteBucket did not throw exception on not existing bucket");
        //}

        [TestMethod]
        public async Task ListBuckets_Lists_TwoNewlyCreatedBuckets()
        {
            string bucketname1 = "listbucket-lists-mytwobuckets-bucket1";
            string bucketname2 = "listbucket-lists-mytwobuckets-bucket2";

            await _service.CreateBucketAsync(bucketname1, _bucketConfig);
            await _service.CreateBucketAsync(bucketname2, _bucketConfig);

            var result = await _service.ListBucketsAsync(new BucketListOptions());

            Assert.IsTrue(result.Length >= 2);
            int foundBuckets = 0;
            foreach(var bucketInfo in result.Items)
            {
                if (bucketInfo.Name == bucketname1 || bucketInfo.Name == bucketname2)
                    foundBuckets++;
            }
            Assert.AreEqual(2, foundBuckets);
        }

        [TestMethod]
        public async Task CloseBucket_Closes_Bucket()
        {
            string bucketname = "closebucket-closes-bucket";

            await _service.CreateBucketAsync(bucketname, _bucketConfig);
            var bucketHandle = await _service.OpenBucketAsync(bucketname);

            await _service.CloseBucketAsync(bucketHandle);
        }

        [TestMethod]
        public async Task CloseBucket_Fails_OnNullBucketHandle()
        {
            try
            {
                await _service.CloseBucketAsync(null);
            }
            catch (BucketCloseException ex)
            {
                Assert.AreEqual("Bucket already closed", ex.Message);
                return;
            }

            Assert.IsTrue(false, "CloseBucket did not throw exception on null BucketHandle");
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            await DeleteBucketAsync("createbucket-creates-newbucket");
            await DeleteBucketAsync("createbucket-fails-onbucketalreadyexisting");
            await DeleteBucketAsync("getbucketinfo-retrieves-bucketinfo");
            await DeleteBucketAsync("openbucket-returns-buckethandle");
            await DeleteBucketAsync("listbucket-lists-mytwobuckets-bucket1");
            await DeleteBucketAsync("listbucket-lists-mytwobuckets-bucket2");
            await DeleteBucketAsync("closebucket-closes-bucket");
            await DeleteBucketAsync("deletebucket-deletes-bucket");
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
