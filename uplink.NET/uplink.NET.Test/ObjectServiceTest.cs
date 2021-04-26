using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Exceptions;
using uplink.NET.Interfaces;
using uplink.NET.Models;
using uplink.NET.Services;

namespace uplink.NET.Test
{
    [TestClass]

    public class ObjectServiceTest
    {
        Access _access;
        IBucketService _bucketService;
        IObjectService _objectService;

        [TestInitialize]
        public async Task Init()
        {
            Access.SetTempDirectory(System.IO.Path.GetTempPath());
            _access = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET);
            _bucketService = new BucketService(_access);
            _objectService = new ObjectService(_access);

            await CleanupAsync();
        }

        [TestMethod]
        public async Task UploadObject_Uploads_LargeFile()
        {
            await Upload_X_Bytes(1024 * 512);
        }

        [TestMethod]
        public async Task UploadObject_Uploads_2500Bytes()
        {
            await Upload_X_Bytes(2500);
        }

        [TestMethod]
        public async Task UploadObject_Uploads_2048Bytes()
        {
            await Upload_X_Bytes(2048);
        }

        [TestMethod]
        public async Task UploadObject_Uploads_256Bytes()
        {
            await Upload_X_Bytes(256);
        }

        [TestMethod]
        public async Task UploadObject_Uploads_LargeFileAsStream()
        {
            await Upload_X_Bytes_AsStream(1024 * 512);
        }

        [TestMethod]
        public async Task UploadObject_Uploads_2500BytesAsStream()
        {
            await Upload_X_Bytes_AsStream(2500);
        }

        [TestMethod]
        public async Task UploadObject_Uploads_2048BytesAsStream()
        {
            await Upload_X_Bytes_AsStream(2048);
        }

        [TestMethod]
        public async Task UploadObject_Uploads_256BytesAsStream()
        {
            await Upload_X_Bytes_AsStream(256);
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

        private async Task Upload_X_Bytes_AsStream(long bytes)
        {
            string bucketname = "uploadtest";

            var result = await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload = GetRandomBytes(bytes);
            Stream stream = new MemoryStream(bytesToUpload);

            bool progressChangeCounterCalled = false;

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), stream, false);
            uploadOperation.UploadOperationProgressChanged += (op) =>
            {
                progressChangeCounterCalled = true;
            };

            await uploadOperation.StartUploadAsync();

            Assert.IsTrue(progressChangeCounterCalled);
            Assert.IsTrue(uploadOperation.Completed, uploadOperation.ErrorMessage);
            Assert.AreEqual(bytes, uploadOperation.BytesSent);
        }

        [TestMethod]
        public async Task DownloadObject_Downloads_256Bytes()
        {
            await Download_X_Bytes(256);
        }

        [TestMethod]
        public async Task DownloadObject_Downloads_2048Bytes()
        {
            await Download_X_Bytes(2048);
        }

        [TestMethod]
        public async Task DownloadObject_Downloads_2500Bytes()
        {
            await Download_X_Bytes(2500);
        }

        [TestMethod]
        public async Task DownloadObject_Downloads_LargeFile()
        {
            await Download_X_Bytes(1024*512);
        }

        private async Task Download_X_Bytes(long bytes)
        {
            string bucketname = "downloadtest";

            var result = await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload = GetRandomBytes(bytes);

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();

            bool progressChangeCounterCalled = false;

            var downloadOperation = await _objectService.DownloadObjectAsync(bucket, "myfile.txt",new DownloadOptions(), false);
            downloadOperation.DownloadOperationProgressChanged += (op) =>
            {
                progressChangeCounterCalled = true;
            };

            await downloadOperation.StartDownloadAsync();

            Assert.IsTrue(progressChangeCounterCalled);
            Assert.IsTrue(downloadOperation.Completed);
            Assert.AreEqual(bytes, downloadOperation.BytesReceived);
            int index = 0;
            foreach (var b in downloadOperation.DownloadedBytes)
            {
                Assert.AreEqual(bytesToUpload[index], b);
                index++;
            }
        }

        [TestMethod]
        public async Task DownloadStream_Provides_First50Bytes()
        {
            string bucketname = "downloadstreamtest1";

            var result = await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload = new byte[250];
            for (int i = 0; i < 250; i++)
            {
                bytesToUpload[i] = Convert.ToByte(i);
            }

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();

            var stream = new DownloadStream(bucket, bytesToUpload.Length, "myfile.txt");
            byte[] bytesReceived = new byte[50];
            await stream.ReadAsync(bytesReceived, 0, 50);

            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual(i, Convert.ToInt32(bytesReceived[i]));
            }
        }

        [TestMethod]
        public async Task DownloadStream_Supports_Seeking()
        {
            string bucketname = "downloadstreamtest2";

            var result = await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload = new byte[250];
            for (int i = 0; i < 250; i++)
            {
                bytesToUpload[i] = Convert.ToByte(i);
            }

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();

            var stream = new DownloadStream(bucket, bytesToUpload.Length, "myfile.txt");
            byte[] bytesReceived = new byte[50];
            stream.Seek(100, System.IO.SeekOrigin.Begin);
            await stream.ReadAsync(bytesReceived, 0, 50);

            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual(100 + i, Convert.ToInt32(bytesReceived[i]));
            }
        }

        [TestMethod]
        public async Task ListObjects_Lists_ExistingObject()
        {
            string bucketname = "listobject-lists-existingobjects";

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload = GetRandomBytes(2048);

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile1.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();
            var uploadOperation2 = await _objectService.UploadObjectAsync(bucket, "myfile2.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation2.StartUploadAsync();

            var objectList = await _objectService.ListObjectsAsync(bucket, new ListObjectsOptions());

            Assert.AreEqual(2, objectList.Items.Count);
            Assert.AreEqual("myfile2.txt", objectList.Items[1].Key);
        }

        [TestMethod]
        public async Task GetObject_Gets_Object()
        {
            string bucketname = "getobjectmeta-gets-objectmeta";

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload = GetRandomBytes(2048);

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();

            var storjObject = await _objectService.GetObjectAsync(bucket, "myfile.txt");

            Assert.AreEqual("myfile.txt", storjObject.Key);
            Assert.AreEqual(2048, storjObject.SystemMetaData.ContentLength);
        }

        [TestMethod]
        public async Task GetObject_Fails_OnNotExistingObject()
        {
            string bucketname = "getobjectmeta-fails-onnotexistingobject";

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);

            try
            {
                await _objectService.GetObjectAsync(bucket, "notexisting.txt");
            }
            catch (ObjectNotFoundException ex)
            {
                Assert.AreEqual("notexisting.txt", ex.TargetPath);
                Assert.IsTrue(ex.Message.Contains("not found"));
                return;
            }

            Assert.IsTrue(false, "GetObjectMeta does not throw exception of non existing object");
        }

        [TestMethod]
        public async Task DeleteObject_Fails_OnNotExistingObject()
        {
            string bucketname = "deleteobject-fails-onnotexistingobject";

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);

            try
            {
                await _objectService.DeleteObjectAsync(bucket, "notexisting.txt");
            }
            catch (ObjectNotFoundException ex)
            {
                Assert.AreEqual("notexisting.txt", ex.TargetPath);
                return;
            }

            Assert.IsTrue(false, "DeleteObject does not throw exception of non existing object");
        }

        [TestMethod]
        public async Task DeleteObject_Deletes_Object()
        {
            string bucketname = "deleteobject-deletes-object";

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);

            byte[] bytesToUpload = GetRandomBytes(2048);

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile1.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();

            var objectList = await _objectService.ListObjectsAsync(bucket, new ListObjectsOptions());
            Assert.AreEqual(1, objectList.Items.Count);

            await _objectService.DeleteObjectAsync(bucket, "myfile1.txt");

            var objectList2 = await _objectService.ListObjectsAsync(bucket, new ListObjectsOptions());
            Assert.AreEqual(0, objectList2.Items.Count);
        }

        [TestMethod]
        public async Task SetCustomMetaData_Works()
        {
            string bucketname = "set-custom-metadata-works";

            await _bucketService.CreateBucketAsync(bucketname);
            var bucket = await _bucketService.GetBucketAsync(bucketname);
            byte[] bytesToUpload = GetRandomBytes(2048);

            CustomMetadata customMetadata = new CustomMetadata();
            customMetadata.Entries.Add(new CustomMetadataEntry() { Key = "my-key 1", Value = "my-value 1" });
            customMetadata.Entries.Add(new CustomMetadataEntry() { Key = "my-key 2", Value = "my-value 2" });

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile1.txt", new UploadOptions(), bytesToUpload, customMetadata, false);
            await uploadOperation.StartUploadAsync();

            var stat = await _objectService.GetObjectAsync(bucket, "myfile1.txt");
            var objectList = await _objectService.ListObjectsAsync(bucket, new ListObjectsOptions());

            Assert.AreEqual(1, objectList.Items.Count);
            Assert.AreEqual(2, stat.CustomMetaData.Entries.Count);
            Assert.AreEqual("my-key 1", stat.CustomMetaData.Entries[0].Key);
            Assert.AreEqual("my-value 1", stat.CustomMetaData.Entries[0].Value);
            Assert.AreEqual("my-key 2", stat.CustomMetaData.Entries[1].Key);
            Assert.AreEqual("my-value 2", stat.CustomMetaData.Entries[1].Value);
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
            await DeleteBucketAsync("uploadtest");
            await DeleteBucketAsync("downloadtest");
            await DeleteBucketAsync("downloadstreamtest1");
            await DeleteBucketAsync("downloadstreamtest2");
            await DeleteBucketAsync("listobject-lists-existingobjects");
            await DeleteBucketAsync("getobjectmeta-gets-objectmeta");
            await DeleteBucketAsync("getobjectmeta-fails-onnotexistingobject");
            await DeleteBucketAsync("deleteobject-fails-onnotexistingobject");
            await DeleteBucketAsync("deleteobject-deletes-object");
            await DeleteBucketAsync("set-custom-metadata-works");
        }

        private async Task DeleteBucketAsync(string bucketName)
        {
            try
            {
                var bucket = await _bucketService.GetBucketAsync(bucketName);
                var result = await _objectService.ListObjectsAsync(bucket, new ListObjectsOptions() { Recursive = true });
                foreach(var obj in result.Items)
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
