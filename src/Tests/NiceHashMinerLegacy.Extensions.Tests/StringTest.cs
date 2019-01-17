using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NiceHashMinerLegacy.Extensions.Tests
{
    [TestClass]
    public class StringTest
    {
        [TestMethod]
        public void GetHashrateAfterShouldParse()
        {
            // xmr-stak
            "[2019-01-06 21:18:33] : Benchmark Total: 756.2 H/S".TryGetHashrateAfter("Benchmark Total:", out var hash);
            Assert.AreEqual(756.2, hash);

            // Claymore 
            "ETH - Total Speed: 32.339 Mh/s, Total Shares: 0, Rejected: 0, Time: 00:00"
                .TryGetHashrateAfter("Total Speed:", out hash);
            Assert.AreEqual(32339000, hash);

            // T-rex
            "20190109 23:53:41 Total: 71.89 MH/s".TryGetHashrateAfter("Total:", out hash);
            Assert.AreEqual(71890000, hash);
        }

        [TestMethod]
        public void GetHashrateAfterShouldReturnNull()
        {
            Assert.IsFalse("[2019-01-06 21:18:33] : Benchmark Total: --- H/S"
                .TryGetHashrateAfter("Benchmark Total:", out var hash));
            Assert.AreEqual(0, hash);
        }
        
        [TestMethod]
        public void AfterFirstNumberShouldMatch()
        {
            var a = "NVIDIA GTX 1080 Ti".AfterFirstOccurence("NVIDIA ");
            Assert.AreEqual("GTX 1080 Ti", a);

            var b = "blarg".AfterFirstOccurence("NV ");
            Assert.AreEqual("", b);

            var c = "NVIDIA GTX 750 Ti".AfterFirstOccurence("GTX");
            Assert.AreEqual(" 750 Ti", c);
        }
    }
}
