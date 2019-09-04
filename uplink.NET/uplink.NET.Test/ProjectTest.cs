using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using uplink.NET.Models;

namespace uplink.NET.Test
{
    [TestClass]
    public class ProjectTest
    {
        [TestMethod]
        public void CreateProject()
        {
            var uplinkConfig = new UplinkConfig();
            var uplink = new Uplink(uplinkConfig);
            var apiKey = new APIKey(TestConstants.VALID_API_KEY);

            using (Project project = new Project(uplink, apiKey, TestConstants.SATELLITE_URL))
            {
                Assert.IsNotNull(project);
            }
        }
    }
}
