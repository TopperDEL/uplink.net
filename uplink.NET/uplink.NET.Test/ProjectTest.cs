using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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

            using (Project project = new Project(uplink, apiKey, "satellite.stefan-benten.de:7777", projectOptions))
            {
                Assert.IsNotNull(project);
            }
        }
    }
}
