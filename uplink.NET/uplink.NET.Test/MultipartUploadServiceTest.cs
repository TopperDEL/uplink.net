using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Services;
using System.Linq;

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
            if (bytes == 0)
                return;

            string bucketname = "multipartuploadtestsingle";
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

        [DataTestMethod]
        [DataRow(512, 1, 5 * 1024 * 1024)]
        [DataRow(7 * 1024 * 1024, 2, 5 * 1024 * 1024)] //7MB
        public async Task MultipartUpload_X_BytesByMultipleParts(long bytes, int partCount, int partByteSize)
        {
            if (bytes == 0 || partCount == 0)
                return;

            string bucketname = "multipartuploadtest";
            string objectKey = "multipart.txt";

            await _bucketService.CreateBucketAsync(bucketname);
            byte[] bytesToUpload = GetRandomBytes(bytes);

            var multipart = await _multipartUploadService.BeginUploadAsync(bucketname, objectKey, new UploadOptions());

            for (int i = 0; i < partCount; i++)
            {
                var partBytes = bytesToUpload.Skip(i * partByteSize).Take(partByteSize).ToArray();
                if (i == partCount - 1)
                {
                    partBytes = bytesToUpload.Skip(i * partByteSize).ToArray();
                }
                var partResult = await _multipartUploadService.UploadPartAsync(bucketname, objectKey, multipart.UploadId, (uint)i, partBytes);
                Assert.AreEqual(partBytes.Length, (int)partResult.BytesWritten);
            }

            var uploadResult = await _multipartUploadService.CommitUploadAsync(bucketname, objectKey, multipart.UploadId, new CommitUploadOptions());
            Assert.IsNotNull(uploadResult.Object, uploadResult.Error);
            Assert.AreEqual(objectKey, uploadResult.Object.Key);

            await VerifyUploadByDownloadingAndCompareAsync(bucketname, objectKey, bytesToUpload);
        }

        [DataTestMethod]
        [DataRow(10 * 512, 10, 2)]
        public async Task AbortMultipartUpload_AfterXParts(long bytes, int partCount, int cancelAfter)
        {
            if (bytes == 0 || partCount == 0)
                return;

            string bucketname = "abortmultipartuploadtest";
            string objectKey = "multipart.txt";

            await _bucketService.CreateBucketAsync(bucketname);
            byte[] bytesToUpload = GetRandomBytes(bytes);

            var multipart = await _multipartUploadService.BeginUploadAsync(bucketname, objectKey, new UploadOptions());

            for (int i = 0; i < partCount; i++)
            {
                var partBytes = bytesToUpload.Skip(i * (int)(bytes / partCount)).Take((int)(bytes / partCount)).ToArray();
                if (i == partCount - 1)
                {
                    partBytes = bytesToUpload.Skip(i * (int)(bytes / partCount)).ToArray();
                }

                if (i == cancelAfter - 1)
                {
                    break;
                }
                else
                {
                    var partResult = await _multipartUploadService.UploadPartAsync(bucketname, objectKey, multipart.UploadId, (uint)i, partBytes);
                    Assert.AreEqual(partBytes.Length, (int)partResult.BytesWritten);
                }
            }

            var uploadListBefore = await _multipartUploadService.ListUploadsAsync(bucketname, new ListUploadOptions() { Recursive = true });
            Assert.AreEqual(1, uploadListBefore.Items.Count);

            await _multipartUploadService.AbortUploadAsync(bucketname, objectKey, multipart.UploadId);

            var uploadListAfter = await _multipartUploadService.ListUploadsAsync(bucketname, new ListUploadOptions() { Recursive = true });
            Assert.AreEqual(0, uploadListAfter.Items.Count);

            var bucket = await _bucketService.GetBucketAsync(bucketname);
            var bucketContent = await _objectService.ListObjectsAsync(bucket, new ListObjectsOptions() { Recursive = true });
            Assert.AreEqual(0, bucketContent.Items.Count);
        }

        [DataTestMethod]
        [DataRow(10 * 512, 10)]
        public async Task ListMultipartUploads_Lists_OpenUploads(long bytes, int partCount)
        {
            if (bytes == 0 || partCount == 0)
                return;

            string bucketname = "abortmultipartuploadtest";
            string objectKey = "multipart.txt";

            await _bucketService.CreateBucketAsync(bucketname);

            var multipart = await _multipartUploadService.BeginUploadAsync(bucketname, objectKey, new UploadOptions());

            var uploadListBefore = await _multipartUploadService.ListUploadsAsync(bucketname, new ListUploadOptions() { Recursive = true });
            Assert.AreEqual(1, uploadListBefore.Items.Count);
        }

        [DataTestMethod]
        [DataRow(10 * 512, 10)]
        public async Task AbortMultipartUploads_Aborts_OpenUpload(long bytes, int partCount)
        {
            if (bytes == 0 || partCount == 0)
                return;

            string bucketname = "abortmultipartuploadtest";
            string objectKey = "multipart.txt";

            await _bucketService.CreateBucketAsync(bucketname);

            var multipart = await _multipartUploadService.BeginUploadAsync(bucketname, objectKey, new UploadOptions());

            var uploadListBefore = await _multipartUploadService.ListUploadsAsync(bucketname, new ListUploadOptions() { Recursive = true });
            Assert.AreEqual(1, uploadListBefore.Items.Count);

            await _multipartUploadService.AbortUploadAsync(bucketname, objectKey, multipart.UploadId);

            var uploadListAfter = await _multipartUploadService.ListUploadsAsync(bucketname, new ListUploadOptions() { Recursive = true });
            Assert.AreEqual(0, uploadListAfter.Items.Count);
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
            await DeleteBucketAsync("multipartuploadtestsingle");
            await DeleteBucketAsync("multipartuploadtest");
            await DeleteBucketAsync("abortmultipartuploadtest");
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
