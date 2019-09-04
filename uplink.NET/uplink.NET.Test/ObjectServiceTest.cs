using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            APIKey apiKey = new APIKey(TestConstants.VALID_API_KEY);
            _project = new Project(uplink, apiKey, TestConstants.SATELLITE_URL);
            _bucketConfig = new BucketConfig();
        }

        [TestMethod]
        public async Task UploadObject_Uploads_2500Bytes()
        {
            await Upload_X_Bytes(2500, 3, new List<ulong>() { 1024, 2048, 2500 }); //Two 1024 bytes packages, one with 452 bytes
        }

        [TestMethod]
        public async Task UploadObject_Uploads_2048Bytes()
        {
            await Upload_X_Bytes(2048, 2, new List<ulong>() { 1024, 2048 }); //Two 1024 bytes packages
        }

        [TestMethod]
        public async Task UploadObject_Uploads_256Bytes()
        {
            await Upload_X_Bytes(256, 1, new List<ulong>() { 256 }); //One 256 bytes package
        }

        private async Task Upload_X_Bytes(ulong bytes, int exptecedProgressCount, List<ulong> progressValues)
        {
            string bucketname = "uploadtest";

            var result = await _bucketService.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucket = await _bucketService.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));
            byte[] bytesToUpload = GetRandomBytes(bytes);

            var progressChangeCounter = 0;

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
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

        [TestMethod]
        public async Task DownloadObject_Downloads_256Bytes()
        {
            await Download_X_Bytes(256, 1, new List<ulong>() { 256 }); //One 256 bytes package
        }

        [TestMethod]
        public async Task DownloadObject_Downloads_2048Bytes()
        {
            await Download_X_Bytes(2048, 2, new List<ulong>() { 1024, 2048 }); //Two 1024 bytes package
        }

        [TestMethod]
        public async Task DownloadObject_Downloads_2500Bytes()
        {
            await Download_X_Bytes(2500, 3, new List<ulong>() { 1024, 2048, 2500 }); //Two 1024 bytes packages, one with 452 bytes
        }

        private async Task Download_X_Bytes(ulong bytes, int exptecedProgressCount, List<ulong> progressValues)
        {
            string bucketname = "downloadtest";

            var result = await _bucketService.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucket = await _bucketService.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));
            byte[] bytesToUpload = GetRandomBytes(bytes);

            
            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();

            var progressChangeCounter = 0;

            var downloadOperation = await _objectService.DownloadObjectAsync(bucket, "myfile.txt", false);
            downloadOperation.DownloadOperationProgressChanged += (op) =>
            {
                Assert.AreEqual(downloadOperation.BytesReceived, progressValues[progressChangeCounter]);
                progressChangeCounter++;
            };

            await downloadOperation.StartDownloadAsync();

            Assert.AreEqual(exptecedProgressCount, progressChangeCounter);
            Assert.IsTrue(downloadOperation.Completed);
            Assert.AreEqual(bytes, downloadOperation.BytesReceived);
            int index = 0;
            foreach(var b in downloadOperation.DownloadedBytes)
            {
                Assert.AreEqual(bytesToUpload[index], b);
                index++;
            }
        }

        [TestMethod]
        public async Task ListObjects_Lists_RaisesError()
        {
            string bucketname = "listobject-lists-raiseserror";

            await _bucketService.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucket = await _bucketService.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));
            byte[] bytesToUpload = GetRandomBytes(2048);

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();

            try
            {
                await _objectService.ListObjectsAsync(bucket, new ListOptions());
            }catch(ObjectListException ex)
            {
                Assert.IsTrue(ex.Message.Contains("direction"));
                return;
            }

            Assert.IsTrue(false, "Error message not raised");
        }

        [TestMethod]
        public async Task ListObjects_Lists_ExistingObject()
        {
            string bucketname = "listobject-lists-existingobjects";

            await _bucketService.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucket = await _bucketService.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));
            byte[] bytesToUpload = GetRandomBytes(2048);

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile1.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();
            var uploadOperation2 = await _objectService.UploadObjectAsync(bucket, "myfile2.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation2.StartUploadAsync();

            var objectList = await _objectService.ListObjectsAsync(bucket, new ListOptions() { Direction = ListDirection.STORJ_FORWARD });

            Assert.AreEqual(2, objectList.Length);
            Assert.AreEqual("myfile2.txt", objectList.Items[1].Path);
        }

        [TestMethod]
        public async Task GetObjectMeta_Gets_ObjectMeta()
        {
            string bucketname = "getobjectmeta-gets-objectmeta";

            await _bucketService.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucket = await _bucketService.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));
            byte[] bytesToUpload = GetRandomBytes(2048);

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();
            
            var objectMeta = await _objectService.GetObjectMetaAsync(bucket, "myfile.txt");

            Assert.AreEqual("myfile.txt", objectMeta.Path);
            Assert.AreEqual((ulong)2048, objectMeta.Size);
        }

        [TestMethod]
        public async Task GetObjectMeta_Fails_OnNotExistingObject()
        {
            string bucketname = "getobjectmeta-fails-onnotexistingobject";

            await _bucketService.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucket = await _bucketService.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));

            try
            {
                await _objectService.GetObjectMetaAsync(bucket, "notexisting.txt");
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

            await _bucketService.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucket = await _bucketService.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));

            try
            {
                await _objectService.DeleteObjectAsync(bucket, "notexisting.txt");
            }
            catch (ObjectNotFoundException ex)
            {
                Assert.AreEqual("notexisting.txt", ex.TargetPath);
                Assert.IsTrue(ex.Message.Contains("not found"));
                return;
            }

            Assert.IsTrue(false, "DeleteObject does not throw exception of non existing object");
        }

        [TestMethod]
        public async Task DeleteObject_Deletes_Object()
        {
            string bucketname = "deleteobject-deletes-object";

            await _bucketService.CreateBucketAsync(_project, bucketname, _bucketConfig);
            var bucket = await _bucketService.OpenBucketAsync(_project, bucketname, EncryptionAccess.FromPassphrase(_project, TestConstants.ENCRYPTION_SECRET));

            byte[] bytesToUpload = GetRandomBytes(2048);

            var uploadOperation = await _objectService.UploadObjectAsync(bucket, "myfile1.txt", new UploadOptions(), bytesToUpload, false);
            await uploadOperation.StartUploadAsync();

            var objectList = await _objectService.ListObjectsAsync(bucket, new ListOptions() { Direction = ListDirection.STORJ_FORWARD });
            Assert.AreEqual(1, objectList.Length);

            await _objectService.DeleteObjectAsync(bucket, "myfile1.txt");

            var objectList2 = await _objectService.ListObjectsAsync(bucket, new ListOptions() { Direction = ListDirection.STORJ_FORWARD });
            Assert.AreEqual(0, objectList2.Length);
        }

        private byte[] GetRandomBytes(ulong length)
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
            await DeleteBucketAsync("listobject-lists-raiseserror");
            await DeleteBucketAsync("listobject-lists-existingobjects");
            await DeleteBucketAsync("getobjectmeta-gets-objectmeta");
            await DeleteBucketAsync("getobjectmeta-fails-onnotexistingobject");
            await DeleteBucketAsync("deleteobject-fails-onnotexistingobject");
            await DeleteBucketAsync("deleteobject-deletes-object");
        }

        private async Task DeleteBucketAsync(string bucketName)
        {
            try
            {
                await _bucketService.DeleteBucketAsync(_project, bucketName);
            }
            catch
            { }
        }
    }
}
