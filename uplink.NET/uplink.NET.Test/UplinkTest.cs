using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace uplink.NET.Test
{
    [TestClass]
    public class UplinkTest
    {        
        [TestMethod]
        public void CreateUplink()
        {
            using (Uplink uplink = new Uplink(new UplinkConfig()))
            {
                Assert.IsNotNull(uplink);
            }
        }
    }
}
