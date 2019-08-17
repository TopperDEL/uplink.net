using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using uplink.Net.Contracts.Interfaces;
using uplink.Net.LocalModels;
using uplink.Net.Services;

namespace uplink.NET.Test
{
    [TestClass]
    public class BucketServiceTest
    {
        [TestMethod]
        public void CreateBucketTest()
        {
            IBucketService service = new BucketService();
            UplinkConfig config = new UplinkConfig();
            Uplink uplink = new Uplink(config);
            ApiKey apiKey = new ApiKey(TestConstants.VALID_API_KEY);
            ProjectOptions projectOptions = new ProjectOptions();
            Project project = new Project(uplink, apiKey, "satellite.stefan-benten.de:7777", projectOptions);
            BucketConfig bucketConfig = new BucketConfig();

            var result = service.CreateBucket(project, "testbucket", bucketConfig);
        }
    }
}
