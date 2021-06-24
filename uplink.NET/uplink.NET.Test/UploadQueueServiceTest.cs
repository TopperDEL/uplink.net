using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Services;

namespace uplink.NET.Test
{
    [TestClass]
    public class UploadQueueServiceTest
    {
        Access _access;
        IBucketService _bucketService;
        IObjectService _objectService;
        IUploadQueueService _uploadQueueService;

        [TestInitialize]
        public void Init()
        {
            Access.SetTempDirectory(System.IO.Path.GetTempPath());
            _access = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET);
            _bucketService = new BucketService(_access);
            _objectService = new ObjectService(_access);
            _uploadQueueService = new UploadQueueService();
        }

        [TestMethod]
        public async Task UploadObject_Uploads_2048Bytes()
        {
            await Upload_X_Bytes(2048);
        }

        [TestMethod]
        public async Task UploadObject_Uploads_512KiB()
        {
            await Upload_X_Bytes(524288);
        }

        private async Task Upload_X_Bytes(long bytes)
        {
            string bucketname = "uploadqueuetest";

            await ((UploadQueueService)_uploadQueueService).ClearAllPendingUploadsAsync();

            var result = await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload1 = GetRandomBytes(bytes);
            byte[] bytesToUpload2 = GetRandomBytes(bytes * 2);

            await _uploadQueueService.AddObjectToUploadQueue(bucketname, "myqueuefile1.txt", _access.Serialize(), bytesToUpload1, "file1");
            await _uploadQueueService.AddObjectToUploadQueue(bucketname, "myqueuefile2.txt", _access.Serialize(), bytesToUpload2, "file2");

            _uploadQueueService.ProcessQueueInBackground();
            while(_uploadQueueService.UploadInProgress)
                await Task.Delay(100);

            _uploadQueueService.StopQueueInBackground();

            var download1 = await _objectService.DownloadObjectAsync(bucket, "myqueuefile1.txt", new DownloadOptions(), false);
            await download1.StartDownloadAsync();

            Assert.IsTrue(download1.Completed);
            Assert.AreEqual(bytesToUpload1.Count(), download1.BytesReceived);

            var download2 = await _objectService.DownloadObjectAsync(bucket, "myqueuefile2.txt", new DownloadOptions(), false);
            await download2.StartDownloadAsync();

            Assert.IsTrue(download2.Completed);
            Assert.AreEqual(bytesToUpload2.Count(), download2.BytesReceived);
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
            await DeleteBucketAsync("uploadqueuetest");
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
