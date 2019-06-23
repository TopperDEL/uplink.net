using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
                Assert.AreEqual("api key format error: invalid api key format", ex.Message); //Might be different if the message from uplink itself changed
                return;
            }

            Assert.IsTrue(false, "No exception on wrong API-Key thrown");
        }
    }
}
