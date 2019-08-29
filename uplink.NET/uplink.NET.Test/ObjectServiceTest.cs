using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        public async Task UploadObject_Uploads_2500Bytes()
        {
            await Upload_X_Bytes(2500, 3, new List<int>() { 1024, 2048, 2500 }); //Two 1024 bytes packages, one with 452 bytes
        }

        [TestMethod]
        public async Task UploadObject_Uploads_2048Bytes()
        {
            await Upload_X_Bytes(2048, 2, new List<int>() { 1024, 2048 }); //Two 1024 bytes packages
        }

        [TestMethod]
        public async Task UploadObject_Uploads_256Bytes()
        {
            await Upload_X_Bytes(256, 1, new List<int>() { 256 }); //One 256 bytes package
        }

        private async Task Upload_X_Bytes(int bytes, int exptecedProgressCount, List<int> progressValues)
        {
            string bucketname = "uploadtest";

            var result = _bucketService.CreateBucket(_project, bucketname, _bucketConfig);
            var bucket = _bucketService.OpenBucket(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));
            byte[] bytesToUpload = GetRandomBytes(bytes);

            var progressChangeCounter = 0;

            var uploadOperation = _objectService.UploadObject(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
            uploadOperation.UploadOperationProgressChanged += (op) =>
            {
                Assert.AreEqual(uploadOperation.BytesSent, progressValues[progressChangeCounter]);
                progressChangeCounter++;
            };

            await uploadOperation.StartUploadAsync();

            Assert.AreEqual(exptecedProgressCount, progressChangeCounter);
            Assert.IsTrue(uploadOperation.Completed);
            Assert.AreEqual(bytes, uploadOperation.BytesSent);
        }

        private byte[] GetRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            Random rand = new Random();
            rand.NextBytes(bytes);

            return bytes;
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
