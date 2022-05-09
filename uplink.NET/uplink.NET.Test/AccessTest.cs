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
    public class AccessTest
    {
        static int SATELLITE_WAIT_DURATION = 30000;
        Access _access;
        IBucketService _bucketService;
        IObjectService _objectService;

        [TestInitialize]
        public void Init()
        {
            Access.SetTempDirectory(System.IO.Path.GetTempPath());
            _access = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET);
            _bucketService = new BucketService(_access);
            _objectService = new ObjectService(_access);
        }

        [TestMethod]
        public void CreateValidAccess_Creates_ValidAccess()
        {
            try
            {
                using (Access access = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET))
                {
                    return;
                }
            }
            catch { }
            Assert.IsTrue(false, "Access could not be created");
        }

        [TestMethod]
        public void CreateInvalidAccess_Raises_Error()
        {
            try
            {
                using (Access access = new Access(TestConstants.SATELLITE_URL, TestConstants.INVALID_API_KEY, TestConstants.ENCRYPTION_SECRET))
                {
                    Assert.IsTrue(false, "Invalid API-Key not checked");
                }
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(AccessException));
                return;
            }
            Assert.IsTrue(false, "Invalid API-Key not checked");
        }

        [TestMethod]
        public void AccessShare_Creates_ValidAccess()
        {
            using (Access access = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET))
            {
                Permission permission = new Permission() { AllowDelete = false };
                List<SharePrefix> sharePrefixes = new List<SharePrefix>();
                sharePrefixes.Add(new SharePrefix() { Bucket = "bucket1", Prefix = "/" });
                var restricted = access.Share(permission, sharePrefixes);
                Assert.IsNotNull(restricted);
            }
        }

        [TestMethod]
        public async Task RevokeAccess_MakesAccesUnusable()
        {
            string serializedAccess;
            string bucketname = "revoke-access-makes-access-unusable";
            byte[] bytesToUpload = ObjectServiceTest.GetRandomBytes(2048);

            using (Access scope = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET))
            {
                await _bucketService.CreateBucketAsync(bucketname);

                Permission permission = new Permission();
                permission.AllowUpload = true;
                List<SharePrefix> sharePrefixes = new List<SharePrefix>();
                sharePrefixes.Add(new SharePrefix() { Bucket = bucketname, Prefix = "test/" });
                var restricted = scope.Share(permission, sharePrefixes);
                serializedAccess = restricted.Serialize();

                await Task.Delay(SATELLITE_WAIT_DURATION); //Wait a bit so that some things can happen on the satellite

                Access restrictedEnv;
                try
                {
                    restrictedEnv = new Access(serializedAccess);
                }
                catch
                {
                    Assert.Fail("Failed to create restricted scope from serialized scope");
                    return;
                }

                var restrictedObjectService = new ObjectService(restrictedEnv);
                var restrictedBucketService = new BucketService(restrictedEnv);
                var restrictedBucket = await restrictedBucketService.GetBucketAsync(bucketname);
                var uploadOperationRestricted = await restrictedObjectService.UploadObjectAsync(restrictedBucket, "test/subfolder/test-file-upload", new UploadOptions(), bytesToUpload, false);
                await uploadOperationRestricted.StartUploadAsync();

                Assert.IsTrue(uploadOperationRestricted.Completed);
                Assert.AreEqual(bytesToUpload.Length, uploadOperationRestricted.BytesSent);

                //Revoke access
                await scope.RevokeAsync();

                //Try uploading again
                var uploadOperationRestricted2 = await restrictedObjectService.UploadObjectAsync(restrictedBucket, "test/subfolder/test-file-upload", new UploadOptions(), bytesToUpload, false);
                await uploadOperationRestricted2.StartUploadAsync();

                Assert.IsFalse(uploadOperationRestricted2.Completed);
                Assert.IsTrue(uploadOperationRestricted2.Failed);
            }
        }

        [TestMethod]
        public async Task AccessShare_Creates_UsableSharedAccessForUpload()
        {
            string serializedAccess;
            string bucketname = "accessshare-creates-usablesharedaccessforupload";
            byte[] bytesToUpload = ObjectServiceTest.GetRandomBytes(2048);

            using (Access scope = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET))
            {
                await _bucketService.CreateBucketAsync(bucketname);

                Permission permission = new Permission();
                permission.AllowUpload = true;
                List<SharePrefix> sharePrefixes = new List<SharePrefix>();
                sharePrefixes.Add(new SharePrefix() { Bucket = bucketname, Prefix = "test/" });
                var restricted = scope.Share(permission, sharePrefixes);
                serializedAccess = restricted.Serialize();
            }

            await Task.Delay(SATELLITE_WAIT_DURATION); //Wait a bit so that some things can happen on the satellite

            Access restrictedEnv;
            try
            {
                restrictedEnv = new Access(serializedAccess);
            }
            catch
            {
                Assert.Fail("Failed to create restricted scope from serialized scope");
                return;
            }

            var restrictedObjectService = new ObjectService(restrictedEnv);
            var restrictedBucketService = new BucketService(restrictedEnv);
            var restrictedBucket = await restrictedBucketService.GetBucketAsync(bucketname);
            var uploadOperationRestricted = await restrictedObjectService.UploadObjectAsync(restrictedBucket, "test/subfolder/test-file-upload", new UploadOptions(), bytesToUpload, false);
            await uploadOperationRestricted.StartUploadAsync();

            Assert.IsTrue(uploadOperationRestricted.Completed);
            Assert.AreEqual(bytesToUpload.Length, uploadOperationRestricted.BytesSent);
        }

        [TestMethod]
        public async Task AccessShare_Creates_UsableSharedAccessForUploadWithDisallowDeletes()
        {
            string serializedAccess;
            string bucketname = "accessshare-creates-usablesharedaccessforupload";
            byte[] bytesToUpload = ObjectServiceTest.GetRandomBytes(2048);

            using (Access scope = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET))
            {
                await _bucketService.CreateBucketAsync(bucketname);

                Permission permission = new Permission();
                permission.AllowUpload = true; 
                permission.AllowDownload = false; //should not change anything as we are uploading here
                List<SharePrefix> sharePrefixes = new List<SharePrefix>();
                sharePrefixes.Add(new SharePrefix() { Bucket = bucketname, Prefix = "test/" });
                var restricted = scope.Share(permission, sharePrefixes);
                serializedAccess = restricted.Serialize();
            }

            await Task.Delay(SATELLITE_WAIT_DURATION); //Wait a bit so that some things can happen on the satellite

            Access restrictedEnv;
            try
            {
                restrictedEnv = new Access(serializedAccess);
            }
            catch
            {
                Assert.Fail("Failed to create restricted scope from serialized scope");
                return;
            }

            var restrictedObjectService = new ObjectService(restrictedEnv);
            var restrictedBucketService = new BucketService(restrictedEnv);
            var restrictedBucket = await restrictedBucketService.GetBucketAsync(bucketname);
            var uploadOperationRestricted = await restrictedObjectService.UploadObjectAsync(restrictedBucket, "test/subfolder/test-file-upload", new UploadOptions(), bytesToUpload, false);
            await uploadOperationRestricted.StartUploadAsync();

            Assert.IsTrue(uploadOperationRestricted.Completed);
            Assert.AreEqual(bytesToUpload.Length, uploadOperationRestricted.BytesSent);
        }

        [TestMethod]
        public async Task AccessShare_Creates_UsableSharedAccessForUploadDeep()
        {
            string serializedAccess;
            string bucketname = "accessshare-creates-usablesharedaccessforuploaddeep";
            byte[] bytesToUpload = ObjectServiceTest.GetRandomBytes(2048);

            using (Access scope = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET))
            {
                await _bucketService.CreateBucketAsync(bucketname);

                Permission permission = new Permission();
                permission.AllowUpload = true;
                permission.AllowDownload = false; //should not change anything as we are uploading here
                List<SharePrefix> sharePrefixes = new List<SharePrefix>();
                sharePrefixes.Add(new SharePrefix() { Bucket = bucketname, Prefix = "test/subfolder/" });
                var restricted = scope.Share(permission, sharePrefixes);
                serializedAccess = restricted.Serialize();
            }

            await Task.Delay(SATELLITE_WAIT_DURATION); //Wait a bit so that some things can happen on the satellite

            Access restrictedEnv;
            try
            {
                restrictedEnv = new Access(serializedAccess);
            }
            catch
            {
                Assert.Fail("Failed to create restricted scope from serialized scope");
                return;
            }

            var restrictedObjectService = new ObjectService(restrictedEnv);
            var restrictedBucketService = new BucketService(restrictedEnv);
            var restrictedBucket = await restrictedBucketService.GetBucketAsync(bucketname);
            var uploadOperationRestricted = await restrictedObjectService.UploadObjectAsync(restrictedBucket, "test/subfolder/test-file-upload", new UploadOptions(), bytesToUpload, false);
            await uploadOperationRestricted.StartUploadAsync();

            Assert.IsTrue(uploadOperationRestricted.Completed);
            Assert.AreEqual(bytesToUpload.Length, uploadOperationRestricted.BytesSent);
        }

        [TestMethod]
        public async Task AccessShare_Creates_UsableSharedAccessForDownload()
        {
            string serializedAccess;
            string bucketname = "accessshare-creates-usablesharedaccessfordownload";
            byte[] bytesToUpload = ObjectServiceTest.GetRandomBytes(2048);

            using (Access scope = new Access(TestConstants.SATELLITE_URL, TestConstants.VALID_API_KEY, TestConstants.ENCRYPTION_SECRET))
            {
                await _bucketService.CreateBucketAsync(bucketname);
                var bucket = await _bucketService.GetBucketAsync(bucketname);

                var uploadOperation = await _objectService.UploadObjectAsync(bucket, "test/test-file", new UploadOptions(), bytesToUpload, false);
                await uploadOperation.StartUploadAsync();

                Permission permission = new Permission();
                permission.AllowUpload = false;  //Should not change anything as we are downloading here
                permission.AllowDownload = true;
                List<SharePrefix> sharePrefixes = new List<SharePrefix>();
                sharePrefixes.Add(new SharePrefix() { Bucket = bucketname, Prefix = "test/" });
                var restricted = scope.Share(permission, sharePrefixes);
                serializedAccess = restricted.Serialize();
            }

            await Task.Delay(SATELLITE_WAIT_DURATION); //Wait a bit so that some things can happen on the satellite

            Access restrictedEnv;
            try
            {
                restrictedEnv = new Access(serializedAccess);
            }
            catch
            {
                Assert.Fail("Failed to create restricted scope from serialized scope");
                return;
            }

            var restrictedObjectService = new ObjectService(restrictedEnv);
            var restrictedBucketService = new BucketService(restrictedEnv);
            var restrictedBucket = await restrictedBucketService.GetBucketAsync(bucketname);
            var downloadOperation = await restrictedObjectService.DownloadObjectAsync(restrictedBucket, "test/test-file", new DownloadOptions(), false);
            await downloadOperation.StartDownloadAsync();

            Assert.IsTrue(downloadOperation.Completed);
            Assert.AreEqual(bytesToUpload.Length, downloadOperation.BytesReceived);

            for (int i = 0; i < bytesToUpload.Length; i++)
            {
                Assert.AreEqual(bytesToUpload[i], downloadOperation.DownloadedBytes[i], "DownloadedBytes are not equal at index " + i);
            }
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            await DeleteBucketAsync("revoke-access-makes-access-unusable");
            await DeleteBucketAsync("accessshare-creates-usablesharedaccessforupload");
            await DeleteBucketAsync("accessshare-creates-usablesharedaccessforuploaddeep");
            await DeleteBucketAsync("accessshare-creates-usablesharedaccessfordownload");
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