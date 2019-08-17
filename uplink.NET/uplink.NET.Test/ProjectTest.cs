using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using uplink.Net.LocalModels;

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
            var apiKey = new ApiKey(TestConstants.VALID_API_KEY);
            var projectOptions = new ProjectOptions();

            using (Project project = new Project(uplink, apiKey, TestConstants.SATELLITE_URL, projectOptions))
            {
                Assert.IsNotNull(project);
            }
        }
    }
}
