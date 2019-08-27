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
        public void ListBucketsTest()
        {
            var result = _service.ListBuckets(_project, new BucketListOptions());
        }

        [TestCleanup]
        public void Cleanup()
        {
            DeleteBucket("createbucket-creates-newbucket");
            DeleteBucket("createbucket-fails-onbucketalreadyexisting");
            DeleteBucket("getbucketinfo-retrieves-bucketinfo");
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
