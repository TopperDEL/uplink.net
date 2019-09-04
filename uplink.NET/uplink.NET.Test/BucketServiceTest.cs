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
        Project _project;
        BucketConfig _bucketConfig;

        [TestInitialize]
        public void Init()
        {
            _service = new BucketService();
            UplinkConfig config = new UplinkConfig();
            Uplink uplink = new Uplink(config);
            APIKey apiKey = new APIKey(TestConstants.VALID_API_KEY);
            _project = new Project(uplink, apiKey, TestConstants.SATELLITE_URL);
            _bucketConfig = new BucketConfig();
        }

        [TestMethod]
        public async Task CreateBucket_Creates_NewBucket()
        {
            string bucketname = "createbucket-creates-newbucket";

            var result = await _service.CreateBucketAsync(_project, bucketname, _bucketConfig);

            Assert.AreEqual(bucketname, result.Name);
        }

        [TestMethod]
        public async Task CreateBucket_Fails_OnBucketAlreadyExisting()
        {
            string bucketname = "createbucket-fails-onbucketalreadyexisting";

            await _service.CreateBucketAsync(_project, bucketname, _bucketConfig); //Should work

            try
            {
                var resultFailed = await _service.CreateBucketAsync(_project, bucketname, _bucketConfig); //Should fail
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

            await _service.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucketInfo = await _service.GetBucketInfoAsync(_project, bucketname);

            Assert.AreEqual(bucketname, bucketInfo.Name);
        }

        [TestMethod]
        public async Task GetBucketInfo_Fails_OnNotExistingBucket()
        {
            string bucketname = "getbucketinfo-fails-onnotexistingbucket";

            try
            {
                var bucketInfo = await _service.GetBucketInfoAsync(_project, bucketname);
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

            await _service.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucketHandle = await _service.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));

            Assert.IsNotNull(bucketHandle);
        }

        [TestMethod]
        public async Task OpenBucket_Fails_OnNotExistingBucket()
        {
            string bucketname = "openbucket-fails-onnotexistingbucket";

            try
            {
                var bucketHandle = await _service.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));
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

            await _service.CreateBucketAsync(_project, bucketname, _bucketConfig);

            await _service.DeleteBucketAsync(_project, bucketname);

            try
            {
                await _service.GetBucketInfoAsync(_project, bucketname);
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

            await _service.CreateBucketAsync(_project, bucketname1, _bucketConfig);
            await _service.CreateBucketAsync(_project, bucketname2, _bucketConfig);

            var result = await _service.ListBucketsAsync(_project, new BucketListOptions());

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

            await _service.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucketHandle = await _service.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));

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
                await _service.DeleteBucketAsync(_project, bucketName);
            }
            catch
            { }
        }
    }
}
