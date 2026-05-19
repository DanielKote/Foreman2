using Foreman.DataCaching.Loading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForemanTest {
    [TestClass]
    public class BaselineQualitiesTests {
        [TestMethod]
        public void GetDisplayName_returns_fixed_labels_for_baseline_tiers() {
            Assert.AreEqual("Normal", BaselineQualities.GetDisplayName("normal"));
            Assert.AreEqual("Unknown", BaselineQualities.GetDisplayName("quality-unknown"));
            Assert.IsNull(BaselineQualities.GetDisplayName("legendary"));
        }
    }
}
