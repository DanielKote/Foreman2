using Foreman;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForemanTest.support {
    /// <summary>
    /// Resets <see cref="UserMessages.TestHandler"/> before and after each test so a forgotten restore
    /// cannot leak into later tests. Prefer <see cref="UserMessages.UseHandler"/> for temporary overrides.
    /// </summary>
    public abstract class ForemanTestBase {
        [TestInitialize]
        public void BlockUserMessagesForTest() {
            UserMessages.TestHandler = UserMessages.FailTestOnAnyMessage;
        }

        [TestCleanup]
        public void RestoreBlockedUserMessagesAfterTest() {
            UserMessages.TestHandler = UserMessages.FailTestOnAnyMessage;
        }
    }
}
