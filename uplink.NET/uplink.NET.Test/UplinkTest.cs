using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using uplink.Net.LocalModels;

namespace uplink.NET.Test
{
    [TestClass]
    public class UplinkTest
    {        
        [TestMethod]
        public void CreateUplink_TLS_false()
        {
            UplinkConfig config = new UplinkConfig() { Volatile_TLS_SkipPeerCAWhitelist = false };
            using (Uplink uplink = new Uplink(config))
            {
                Assert.IsNotNull(uplink);
            }
        }

        [TestMethod]
        public void CreateUplink_TLS_true()
        {
            UplinkConfig config = new UplinkConfig() { Volatile_TLS_SkipPeerCAWhitelist = true };
            using (Uplink uplink = new Uplink(config))
            {
                Assert.IsNotNull(uplink);
            }
        }
    }
}
