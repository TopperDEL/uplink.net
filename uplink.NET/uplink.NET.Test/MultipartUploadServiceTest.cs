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
    public class MultipartUploadServiceTest
    {
        Access _access;
        IBucketService _bucketService;
        IObjectService _objectService;
        IMultipartUploadService _multipartUploadService;

        [TestInitialize]
        public void Init()
        {
            Access.SetTempDirectory(System.IO.Path.GetTempPath());
            _access = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET);
            _bucketService = new BucketService(_access);
            _objectService = new ObjectService(_access);
        }

        private async Task Upload_X_Bytes(long bytes)
        {
            string bucketname = "uploadtest";

            var result = await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload = GetRandomBytes(bytes);

            bool progressChangeCounterCalled = false;

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
            uploadOperation.UploadOperationProgressChanged += (op) =>
            {
                progressChangeCounterCalled = true;
            };

            await uploadOperation.StartUploadAsync();

            Assert.IsTrue(progressChangeCounterCalled);
            Assert.IsTrue(uploadOperation.Completed, uploadOperation.ErrorMessage);
            Assert.AreEqual(bytes, uploadOperation.BytesSent);
        }

        public static byte[] GetRandomBytes(long length)
        {
            byte[] bytes = new byte[length];
            Random rand = new Random();
            rand.NextBytes(bytes);

            return bytes;
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            //await DeleteBucketAsync("uploadtest");
        }

        private async Task DeleteBucketAsync(string bucketName)
        {
            try
            {
                var bucket = await _bucketService.GetBucketAsync(bucketName);
                var result = await _objectService.ListObjectsAsync(bucket, new ListObjectsOptions() { Recursive = true });
                foreach (var obj in result.Items)
                {
                    await _objectService.DeleteObjectAsync(bucket, obj.Key);
                }
            }
            catch
            { }
            try
            {
                await _bucketService.DeleteBucketAsync(bucketName);
            }
            catch
            { }
        }
    }
}
