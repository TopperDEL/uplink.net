using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
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
            ApiKey apiKey = new ApiKey(TestConstants.VALID_API_KEY);
            ProjectOptions projectOptions = new ProjectOptions();
            _project = new Project(uplink, apiKey, TestConstants.SATELLITE_URL, projectOptions);
            _bucketConfig = new BucketConfig();
        }

        [TestMethod]
        public void CreateBucket_Creates_NewBucket()
        {
            string bucketname = "createbucket-creates-newbucket";

            var result = _service.CreateBucket(_project, bucketname, _bucketConfig);

            Assert.AreEqual(bucketname, result.Name);
        }

        [TestMethod]
        public void CreateBucket_Fails_OnBucketAlreadyExisting()
        {
            string bucketname = "createbucket-fails-onbucketalreadyexisting";

            var resultWorking = _service.CreateBucket(_project, bucketname, _bucketConfig); //Should work

            try
            {
                var resultFailed = _service.CreateBucket(_project, bucketname, _bucketConfig); //Should fail
            }catch(BucketCreationException ex)
            {
                Assert.IsTrue(ex.Message.Contains("already exists"));
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "CreateBucket did not throw exception on already existing bucket");
        }

        [TestMethod]
        public void GetBucketInfo_Retrieves_BucketInfo()
        {
            string bucketname = "getbucketinfo-retrieves-bucketinfo";

            var createdResult = _service.CreateBucket(_project, bucketname, _bucketConfig);
            var bucketInfo = _service.GetBucketInfo(_project, bucketname);

            Assert.AreEqual(bucketname, bucketInfo.Name);
        }

        [TestMethod]
        public void GetBucketInfo_Fails_OnNotExistingBucket()
        {
            string bucketname = "getbucketinfo-fails-onnotexistingbucket";

            try
            {
                var bucketInfo = _service.GetBucketInfo(_project, bucketname);
            }
            catch(BucketNotFoundException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "GetBucketInfo did not throw exception on not existing bucket");
        }

        [TestMethod]
        public void OpenBucket_Returns_BucketHandle()
        {
            string bucketname = "openbucket-returns-buckethandle";

            var createdBucket = _service.CreateBucket(_project, bucketname, _bucketConfig);
            var bucketHandle = _service.OpenBucket(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));

            Assert.IsNotNull(bucketHandle);
        }

        [TestMethod]
        public void OpenBucket_Fails_OnNotExistingBucket()
        {
            string bucketname = "openbucket-fails-onnotexistingbucket";

            try
            {
                var bucketHandle = _service.OpenBucket(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));
            }
            catch (BucketNotFoundException ex)
            {
                Assert.AreEqual(bucketname, ex.BucketName);
                return;
            }

            Assert.IsTrue(false, "OpenBucket did not throw exception on not existing bucket");
        }

        [TestMethod]
        public void DeleteBucket_Deletes_Bucket()
        {
            string bucketname = "deletebucket-deletes-bucket";

            var createdResult = _service.CreateBucket(_project, bucketname, _bucketConfig);

            _service.DeleteBucket(_project, bucketname);

            try
            {
                var bucketInfo = _service.GetBucketInfo(_project, bucketname);
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
        public void ListBuckets_Lists_TwoNewlyCreatedBuckets()
        {
            string bucketname1 = "listbucket-lists-mytwobuckets-bucket1";
            string bucketname2 = "listbucket-lists-mytwobuckets-bucket2";

            _service.CreateBucket(_project, bucketname1, _bucketConfig);
            _service.CreateBucket(_project, bucketname2, _bucketConfig);

            var result = _service.ListBuckets(_project, new BucketListOptions());

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
        public void CloseBucket_Closes_Bucket()
        {
            string bucketname = "closebucket-closes-bucket";

            var createdBucket = _service.CreateBucket(_project, bucketname, _bucketConfig);
            var bucketHandle = _service.OpenBucket(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));

            _service.CloseBucket(bucketHandle);
        }

        [TestMethod]
        public void CloseBucket_Fails_OnNullBucketHandle()
        {
            try
            {
                _service.CloseBucket(null);
            }
            catch (BucketCloseException ex)
            {
                Assert.AreEqual("Bucket already closed", ex.Message);
                return;
            }

            Assert.IsTrue(false, "CloseBucket did not throw exception on null BucketHandle");
        }

        [TestCleanup]
        public void Cleanup()
        {
            DeleteBucket("createbucket-creates-newbucket");
            DeleteBucket("createbucket-fails-onbucketalreadyexisting");
            DeleteBucket("getbucketinfo-retrieves-bucketinfo");
            DeleteBucket("openbucket-returns-buckethandle");
            DeleteBucket("listbucket-lists-mytwobuckets-bucket1");
            DeleteBucket("listbucket-lists-mytwobuckets-bucket2");
            DeleteBucket("closebucket-closes-bucket");
            DeleteBucket("deletebucket-deletes-bucket");
        }

        private void DeleteBucket(string bucketName)
        {
            try
            {
                _service.DeleteBucket(_project, bucketName);
            }
            catch
            { }
        }
    }
}
