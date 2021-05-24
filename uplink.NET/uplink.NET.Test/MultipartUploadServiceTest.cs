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
            _multipartUploadService = new MultipartUploadService(_access);
        }

        [DataTestMethod]
        [DataRow(512)]
        [DataRow(10 * 512)]
        [DataRow(1024 * 512)]
        public async Task MultipartUpload_X_BytesInOneTake(long bytes)
        {
            string bucketname = "multipartuploadtest";
            string objectKey = "multipart.txt";

            await _bucketService.CreateBucketAsync(bucketname);
            byte[] bytesToUpload = GetRandomBytes(bytes);

            var multipart = await _multipartUploadService.BeginUploadAsync(bucketname, objectKey, new UploadOptions());
            var partResult = await _multipartUploadService.UploadPartAsync(bucketname, objectKey, multipart.UploadId, 1, bytesToUpload);
            Assert.AreEqual(bytes, partResult.BytesWritten);

            var uploadResult = await _multipartUploadService.CommitUploadAsync(bucketname, objectKey, multipart.UploadId, new CommitUploadOptions());
            Assert.IsNotNull(uploadResult.Object);
            Assert.AreEqual(objectKey, uploadResult.Object.Key);

            await VerifyUploadByDownloadingAndCompareAsync(bucketname, objectKey, bytesToUpload);
        }

        private async Task VerifyUploadByDownloadingAndCompareAsync(string bucketname, string objectKey, byte[] sentBytes)
        {
            var bucket = await _bucketService.GetBucketAsync(bucketname);

            bool progressChangeCounterCalled = false;

            var downloadOperation = await _objectService.DownloadObjectAsync(bucket, objectKey, new DownloadOptions(), false);
            downloadOperation.DownloadOperationProgressChanged += (op) =>
            {
                progressChangeCounterCalled = true;
            };

            await downloadOperation.StartDownloadAsync();

            Assert.IsTrue(progressChangeCounterCalled);
            Assert.IsTrue(downloadOperation.Completed);
            Assert.AreEqual(sentBytes.Length, downloadOperation.BytesReceived);
            int index = 0;
            foreach (var b in downloadOperation.DownloadedBytes)
            {
                Assert.AreEqual(sentBytes[index], b);
                index++;
            }
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
            await DeleteBucketAsync("multipartuploadtest");
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
                await _bucketService.DeleteBucketWithObjectsAsync(bucketName);
            }
            catch
            { }
        }
    }
}
