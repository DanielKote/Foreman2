using Foreman;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForemanTest.support {
    [TestClass]
    public static class ForemanTestAssemblySetup {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext _) {
            UserMessages.TestHandler = UserMessages.FailTestOnAnyMessage;
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup() {
            UserMessages.TestHandler = null;
        }
    }
}
