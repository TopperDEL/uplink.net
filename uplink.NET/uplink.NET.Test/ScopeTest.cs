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
    public class ScopeTest
    {
        IStorjEnvironment _environment;
        IBucketService _bucketService;
        IObjectService _objectService;
        BucketConfig _bucketConfig;

        [TestInitialize]
        public void Init()
        {
            StorjEnvironment.SetTempDirectory(System.IO.Path.GetTempPath());
            _environment = new StorjEnvironment();
            _environment.Initialize(TestConstants.VALID_API_KEY, TestConstants.SATELLITE_URL, TestConstants.ENCRYPTION_SECRET);
            _bucketService = new BucketService(_environment);
            _objectService = new ObjectService();
            _bucketConfig = new BucketConfig();
        }

        [TestMethod]
        public void CreateValidScope_Crestes_ValidScope()
        {
            using (Scope scope = new Scope(TestConstants.SATELLITE_URL, _environment.APIKey, _environment.EncryptionAccess))
            {
                Assert.AreEqual(_environment.APIKey.GetAPIKey(), scope.GetAPIKey().GetAPIKey());
                Assert.AreEqual(TestConstants.SATELLITE_URL, scope.GetSatelliteAddress());
            }
        }

        [TestMethod]
        public void RestrictScope_Creates_ValidScope()
        {
            using (Scope scope = new Scope(TestConstants.SATELLITE_URL, _environment.APIKey, _environment.EncryptionAccess))
            {
                Caveat caveat = new Caveat() { DisallowWrites = true };
                List<EncryptionRestriction> restrictions = new List<EncryptionRestriction>();
                restrictions.Add(new EncryptionRestriction() { Bucket = "bucket1", PathPrefix = "/" });
                var restricted = scope.Restrict(caveat, restrictions);
                Assert.IsNotNull(restricted);

                Assert.AreEqual(TestConstants.SATELLITE_URL, restricted.GetSatelliteAddress());
            }
        }

        [TestMethod]
        public async Task RestrictScope_Creates_UsableRestrictedScope()
        {
            using (Scope scope = new Scope(TestConstants.SATELLITE_URL, _environment.APIKey, _environment.EncryptionAccess))
            {
                string bucketname = "restrictscope-creates-usablerestrictedscope";
                await _bucketService.CreateBucketAsync(bucketname, _bucketConfig);
                var bucket = await _bucketService.OpenBucketAsync(bucketname);

                byte[] bytesToUpload = ObjectServiceTest.GetRandomBytes(2048);

                var uploadOperation = await _objectService.UploadObjectAsync(bucket, "test-file", new UploadOptions(), bytesToUpload, false);
                await uploadOperation.StartUploadAsync();

                Caveat caveat = new Caveat();
                List<EncryptionRestriction> restrictions = new List<EncryptionRestriction>();
                restrictions.Add(new EncryptionRestriction() { Bucket = "restrictscope-creates-usablerestrictedscope", PathPrefix = "test-file" });
                var restricted = scope.Restrict(caveat, restrictions);

                var restrictedEnv = new StorjEnvironment();
                var envInitialized = restrictedEnv.Initialize(restricted.Serialize());
                Assert.IsTrue(envInitialized);

                var restrictedObjectService = new ObjectService();
                var restrictedBucketService = new BucketService(restrictedEnv);
                var restrictedBucket = await restrictedBucketService.OpenBucketAsync(bucketname);
                var downloadOperation = await restrictedObjectService.DownloadObjectAsync(restrictedBucket, "test-file", false);
                await downloadOperation.StartDownloadAsync();

                Assert.IsTrue(downloadOperation.Completed);
                Assert.AreEqual(bytesToUpload.Length, downloadOperation.BytesReceived);

                for (int i = 0; i <= bytesToUpload.Length; i++)
                {
                    Assert.AreEqual(bytesToUpload[i], downloadOperation.DownloadedBytes[i], "DownloadedBytes are not equal at index " + i);
                }
            }
        }

        [TestCleanup]
        public async Task CleanupAsync()
        {
            await DeleteBucketAsync("restrictscope-creates-usablerestrictedscope");
        }

        private async Task DeleteBucketAsync(string bucketName)
        {
            try
            {
                await _bucketService.DeleteBucketAsync(bucketName);
            }
            catch
            { }
        }
    }
}
