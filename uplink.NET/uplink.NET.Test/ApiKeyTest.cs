using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using uplink.Net.Models;

namespace uplink.NET.Test
{
    [TestClass]
    public class ApiKeyTest
    {        
        [TestMethod]
        public void CreateValidApiKey()
        {
            using (ApiKey key = new ApiKey(TestConstants.VALID_API_KEY))
            {
                Assert.IsNotNull(key);
            }
        }

        [TestMethod]
        public void CreateInvalidApiKey_ThrowsError()
        {
            try
            {
                using (ApiKey key = new ApiKey(TestConstants.INVALID_API_KEY))
                {
                    //We should not reach this line - there should be an exception
                }
            }
            catch(Exception ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Message.Contains("api key format error")); //Might be different if the message from uplink itself changed
                return;
            }

            Assert.IsTrue(false, "No exception on wrong API-Key thrown");
        }

        [TestMethod]
        public void GetApiKey()
        {
            using (ApiKey key = new ApiKey(TestConstants.VALID_API_KEY))
            {
                Assert.AreEqual(TestConstants.VALID_API_KEY, key.GetApiKey());
            }
        }
    }
}
