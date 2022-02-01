using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
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

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload1 = GetRandomBytes(bytes);
            byte[] bytesToUpload2 = GetRandomBytes(bytes * 2);

            await _uploadQueueService.AddObjectToUploadQueueAsync(bucketname, "myqueuefile1.txt", _access.Serialize(), bytesToUpload1, "file1");
            await _uploadQueueService.AddObjectToUploadQueueAsync(bucketname, "myqueuefile2.txt", _access.Serialize(), bytesToUpload2, "file2");

            _uploadQueueService.ProcessQueueInBackground();
            while (_uploadQueueService.UploadInProgress)
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

        [TestMethod]
        public async Task UploadObjectFromStream_Uploads_512KiB()
        {
            await Upload_X_BytesFromStream(524288);
        }

        private async Task Upload_X_BytesFromStream(long bytes)
        {
            string bucketname = "uploadqueuetest";

            await ((UploadQueueService)_uploadQueueService).ClearAllPendingUploadsAsync();

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload1 = GetRandomBytes(bytes);
            byte[] bytesToUpload2 = GetRandomBytes(bytes * 2);
            var mstream1 = new MemoryStream(bytesToUpload1);
            var mstream2 = new MemoryStream(bytesToUpload2);

            await _uploadQueueService.AddObjectToUploadQueueAsync(bucketname, "myqueuefile1.txt", _access.Serialize(), mstream1, "file1");
            await _uploadQueueService.AddObjectToUploadQueueAsync(bucketname, "myqueuefile2.txt", _access.Serialize(), mstream2, "file2");

            _uploadQueueService.ProcessQueueInBackground();
            while (_uploadQueueService.UploadInProgress)
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

        [TestMethod]
        public async Task UploadObjectFromStreamWithMetadata_Uploads_512KiB()
        {
            await Upload_X_BytesFromStreamWithMetadata(524288);
        }

        private async Task Upload_X_BytesFromStreamWithMetadata(long bytes)
        {
            string bucketname = "uploadqueuetestwithmetadata";

            await ((UploadQueueService)_uploadQueueService).ClearAllPendingUploadsAsync();

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload1 = GetRandomBytes(bytes);
            byte[] bytesToUpload2 = GetRandomBytes(bytes * 2);
            var mstream1 = new MemoryStream(bytesToUpload1);
            var mstream2 = new MemoryStream(bytesToUpload2);
            var customMetadata1 = new CustomMetadata();
            customMetadata1.Entries.Add(new CustomMetadataEntry { Key = "META1KEY", Value = "meta1value" });
            var customMetadata2 = new CustomMetadata();
            customMetadata2.Entries.Add(new CustomMetadataEntry { Key = "META2KEY", Value = "meta2value" });

            await _uploadQueueService.AddObjectToUploadQueueAsync(bucketname, "myqueuefile1.txt", _access.Serialize(), mstream1, "file1", customMetadata1);
            await _uploadQueueService.AddObjectToUploadQueueAsync(bucketname, "myqueuefile2.txt", _access.Serialize(), mstream2, "file2", customMetadata2);

            _uploadQueueService.ProcessQueueInBackground();
            while (_uploadQueueService.UploadInProgress)
                await Task.Delay(100);

            _uploadQueueService.StopQueueInBackground();

            var download1 = await _objectService.DownloadObjectAsync(bucket, "myqueuefile1.txt", new DownloadOptions(), false);
            await download1.StartDownloadAsync();

            Assert.IsTrue(download1.Completed);
            Assert.AreEqual(bytesToUpload1.Count(), download1.BytesReceived);

            var object1 = await _objectService.GetObjectAsync(bucket, "myqueuefile1.txt");
            Assert.AreEqual("META1KEY", object1.CustomMetadata.Entries[0].Key);
            Assert.AreEqual("meta1value", object1.CustomMetadata.Entries[0].Value);

            var download2 = await _objectService.DownloadObjectAsync(bucket, "myqueuefile2.txt", new DownloadOptions(), false);
            await download2.StartDownloadAsync();

            Assert.IsTrue(download2.Completed);
            Assert.AreEqual(bytesToUpload2.Count(), download2.BytesReceived);

            var object2 = await _objectService.GetObjectAsync(bucket, "myqueuefile2.txt");
            Assert.AreEqual("META2KEY", object2.CustomMetadata.Entries[0].Key);
            Assert.AreEqual("meta2value", object2.CustomMetadata.Entries[0].Value);
        }

        [TestMethod]
        public async Task UploadProvidesCorrectCount()
        {
            string bucketname = "uploadqueuetest";

            await ((UploadQueueService)_uploadQueueService).ClearAllPendingUploadsAsync();

            await _bucketService.CreateBucketAsync(bucketname);
            await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload1 = GetRandomBytes(524288);

            Assert.AreEqual(0, await _uploadQueueService.GetOpenUploadCountAsync());

            await _uploadQueueService.AddObjectToUploadQueueAsync(bucketname, "mycountedqueuefile1.txt", _access.Serialize(), bytesToUpload1, "file1");

            Assert.AreEqual(1, await _uploadQueueService.GetOpenUploadCountAsync());

            _uploadQueueService.ProcessQueueInBackground();

            while (_uploadQueueService.UploadInProgress)
            {
                Assert.AreEqual(1, await _uploadQueueService.GetOpenUploadCountAsync());
                await Task.Delay(100);
            }

            _uploadQueueService.StopQueueInBackground();

            Assert.AreEqual(0, await _uploadQueueService.GetOpenUploadCountAsync());
        }

        [TestMethod]
        public async Task UploadsWithInteruptionAndEvents()
        {
            string bucketname = "uploadqueuetest";

            await ((UploadQueueService)_uploadQueueService).ClearAllPendingUploadsAsync();

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload1 = GetRandomBytes(524288 * 2 * 10); //~around 10MB

            int added = 0;
            int changed = 0;
            int removed = 0;

            _uploadQueueService.UploadQueueChangedEvent += (changeType, entry) =>
            {
                if (changeType == QueueChangeType.EntryAdded)
                {
                    added++;
                }
                else if (changeType == QueueChangeType.EntryUpdated)
                {
                    changed++;
                }
                else if (changeType == QueueChangeType.EntryRemoved)
                {
                    removed++;
                }
            };
            await _uploadQueueService.AddObjectToUploadQueueAsync(bucketname, "myinteruptedqueuefile1.txt", _access.Serialize(), bytesToUpload1, "file1");

            _uploadQueueService.ProcessQueueInBackground();

            while (_uploadQueueService.UploadInProgress)
            {
                //if at ~ 25%, force cancellation of the token
                var uploads = await _uploadQueueService.GetAwaitingUploadsAsync();
                Assert.AreEqual(1, uploads.Count);
                if (uploads[0].BytesCompleted > 524288 * 10 / 2)
                {
                    _uploadQueueService.StopQueueInBackground();
                }
                await Task.Delay(100);
            }

            _uploadQueueService.ProcessQueueInBackground();

            while (_uploadQueueService.UploadInProgress)
            {
                await Task.Delay(100);
            }

            _uploadQueueService.StopQueueInBackground();

            var download1 = await _objectService.DownloadObjectAsync(bucket, "myinteruptedqueuefile1.txt", new DownloadOptions(), false);
            await download1.StartDownloadAsync();

            Assert.IsTrue(download1.Completed);
            Assert.AreEqual(bytesToUpload1.Count(), download1.BytesReceived);

            Assert.AreEqual(1, added);
            Assert.AreEqual(1, removed);
            Assert.IsTrue(changed > 1);
        }

        [TestMethod]
        public async Task UploadsWithInteruptionAndRetry()
        {
            string bucketname = "uploadqueuetest";

            await ((UploadQueueService)_uploadQueueService).ClearAllPendingUploadsAsync();

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload1 = GetRandomBytes(524288 * 2 * 10); //~around 10MB

            int added = 0;
            int changed = 0;
            int removed = 0;

            _uploadQueueService.UploadQueueChangedEvent += (changeType, entry) =>
            {
                if (changeType == QueueChangeType.EntryAdded)
                {
                    added++;
                }
                else if (changeType == QueueChangeType.EntryUpdated)
                {
                    changed++;
                }
                else if (changeType == QueueChangeType.EntryRemoved)
                {
                    removed++;
                }
            };
            await _uploadQueueService.AddObjectToUploadQueueAsync(bucketname, "myinteruptedqueuefile1.txt", _access.Serialize(), bytesToUpload1, "file1");

            _uploadQueueService.ProcessQueueInBackground();

            while (_uploadQueueService.UploadInProgress)
            {
                //if at ~ 25%, force cancellation of the token
                var uploads = await _uploadQueueService.GetAwaitingUploadsAsync();
                Assert.AreEqual(1, uploads.Count);
                if (uploads[0].BytesCompleted > 524288 * 10 / 2)
                {
                    _uploadQueueService.StopQueueInBackground();
                }
                await Task.Delay(100);
            }

            var uploadsVerify = await _uploadQueueService.GetAwaitingUploadsAsync();
            Assert.IsTrue(uploadsVerify[0].BytesCompleted > 0);
            await _uploadQueueService.RetryAsync("myinteruptedqueuefile1.txt");
            var uploadsVerify2 = await _uploadQueueService.GetAwaitingUploadsAsync();
            Assert.IsFalse(uploadsVerify2[0].Failed);
            Assert.AreEqual(0, uploadsVerify2[0].BytesCompleted);
            Assert.AreEqual(0, (int)uploadsVerify2[0].CurrentPartNumber);
            Assert.AreEqual(string.Empty, uploadsVerify2[0].UploadId);

            _uploadQueueService.ProcessQueueInBackground();

            while (_uploadQueueService.UploadInProgress)
            {
                await Task.Delay(100);
            }

            _uploadQueueService.StopQueueInBackground();

            var download1 = await _objectService.DownloadObjectAsync(bucket, "myinteruptedqueuefile1.txt", new DownloadOptions(), false);
            await download1.StartDownloadAsync();

            Assert.IsTrue(download1.Completed);
            Assert.AreEqual(bytesToUpload1.Count(), download1.BytesReceived);

            Assert.AreEqual(1, added);
            Assert.AreEqual(1, removed);
            Assert.IsTrue(changed > 1);
        }

        private void _uploadQueueService_UploadQueueChangedEvent(QueueChangeType queueChangeType, UploadQueueEntry entry)
        {
            throw new NotImplementedException();
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
            await DeleteBucketAsync("uploadqueuetestwithmetadata");
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
