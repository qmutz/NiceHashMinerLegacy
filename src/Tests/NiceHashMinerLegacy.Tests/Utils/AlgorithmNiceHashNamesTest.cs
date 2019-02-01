using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiceHashMiner;
using NiceHashMinerLegacy.Common.Enums;
using System.Collections.Generic;

namespace NiceHashMinerLegacy.Tests.Utils
{
    [TestClass]
    public class AlgorithmNiceHashNamesTest
    {
        [TestMethod]
        public void ShouldReturnNotFoundForInvalid()
        {
            var x = AlgorithmNiceHashNames.GetName((AlgorithmType) 100);

            Assert.AreEqual("NameNotFound type not supported", x);
        }

        [TestMethod]
        public void ShouldReturnNameForValid()
        {
            var expected = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.Blake2s, "Blake2s" },
                { AlgorithmType.CryptoNightV8, "CryptoNightV8" }
            };

            foreach (var key in expected.Keys)
            {
                Assert.AreEqual(expected[key], AlgorithmNiceHashNames.GetName(key));
            }
        }
    }
}
