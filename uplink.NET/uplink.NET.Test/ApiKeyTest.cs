using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using uplink.NET.Models;

namespace uplink.NET.Test
{
    [TestClass]
    public class APIKeyTest
    {        
        [TestMethod]
        public void CreateValidAPIKey()
        {
            using (APIKey key = new APIKey(TestConstants.VALID_API_KEY))
            {
                Assert.IsNotNull(key);
            }
        }

        [TestMethod]
        public void CreateInvalidAPIKey_ThrowsError()
        {
            try
            {
                using (APIKey key = new APIKey(TestConstants.INVALID_API_KEY))
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
        public void GetAPIKey()
        {
            using (APIKey key = new APIKey(TestConstants.VALID_API_KEY))
            {
                Assert.AreEqual(TestConstants.VALID_API_KEY, key.GetAPIKey());
            }
        }
    }
}
