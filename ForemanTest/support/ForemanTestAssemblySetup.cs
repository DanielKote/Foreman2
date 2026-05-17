using Foreman;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForemanTest.support {
    [TestClass]
    public sealed class ForemanTestAssemblySetup {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context) {
            UserMessages.TestHandler = UserMessages.FailTestOnAnyMessage;
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup() {
            UserMessages.TestHandler = null;
        }
    }
}