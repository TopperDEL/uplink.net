using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Services;

namespace uplink.NET.Test
{
    [TestClass]

    public class ObjectServiceTest
    {
        IBucketService _bucketService;
        IObjectService _objectService;
        Project _project;
        BucketConfig _bucketConfig;

        [TestInitialize]
        public void Init()
        {
            _bucketService = new BucketService();
            _objectService = new ObjectService();
            UplinkConfig config = new UplinkConfig();
            Uplink uplink = new Uplink(config);
            ApiKey apiKey = new ApiKey(TestConstants.VALID_API_KEY);
            ProjectOptions projectOptions = new ProjectOptions();
            _project = new Project(uplink, apiKey, TestConstants.SATELLITE_URL, projectOptions);
            _bucketConfig = new BucketConfig();
        }

        [TestMethod]
        public void UploadObjectTest()
        {
            string bucketname = "uploadtest";

            var result = _bucketService.CreateBucket(_project, bucketname, _bucketConfig);
            var bucket = _bucketService.OpenBucket(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));
            _objectService.UploadObject(bucket, "myfile.txt", new UploadOptions());

            Assert.AreEqual(bucketname, result.Name);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DeleteBucket("uploadtest");
        }

        private void DeleteBucket(string bucketName)
        {
            try
            {
                _bucketService.DeleteBucket(_project, bucketName);
            }
            catch
            { }
        }
    }
}
