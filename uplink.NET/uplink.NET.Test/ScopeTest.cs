using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using uplink.NET.Interfaces;
using uplink.NET.Models;

namespace uplink.NET.Test
{
    [TestClass]
    public class ScopeTest
    {
        IStorjEnvironment _environment;

        [TestInitialize]
        public void Init()
        {
            StorjEnvironment.SetTempDirectory(System.IO.Path.GetTempPath());
            _environment = new StorjEnvironment();
            _environment.Initialize(TestConstants.VALID_API_KEY, TestConstants.SATELLITE_URL, TestConstants.ENCRYPTION_SECRET);
        }

        [TestMethod]
        public void CreateValidScope()
        {
            using (Scope scope = new Scope(TestConstants.SATELLITE_URL, _environment.APIKey, _environment.EncryptionAccess))
            {
                Assert.AreEqual(_environment.APIKey.GetAPIKey(), scope.GetAPIKey().GetAPIKey());
                Assert.AreEqual(TestConstants.SATELLITE_URL, scope.GetSatelliteAddress());
            }
        }

        [TestMethod]
        public void RestrictScope()
        {
            using (Scope scope = new Scope(TestConstants.SATELLITE_URL, _environment.APIKey, _environment.EncryptionAccess))
            {
                Caveat caveat = new Caveat() { DisallowWrites = true };
                List<EncryptionRestriction> restrictions = new List<EncryptionRestriction>();
                restrictions.Add(new EncryptionRestriction() { Bucket = "bucket1", PathPrefix = "/" });
                var restricted = scope.Restrict(caveat, restrictions);
                Assert.IsNotNull(restricted);
            }
        }
    }
}
