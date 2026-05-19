using Foreman;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForemanTest {
    [TestClass]
    public class FactorioBenchmarkRunnerTests : ForemanTestBase {
        [TestMethod]
        public void IsCrashOutput_DetectsSigSegvAndCrashHandler() {
            Assert.IsTrue(FactorioBenchmarkRunner.IsCrashOutput("Error CrashHandler.cpp:641: Received SIGSEGV"));
            Assert.IsTrue(FactorioBenchmarkRunner.IsCrashOutput("Factorio crashed. Generating symbolized stacktrace"));
            Assert.IsTrue(FactorioBenchmarkRunner.IsCrashOutput("CrashDump success"));
        }

        [TestMethod]
        public void IsCrashOutput_NormalExportOutput_IsFalse() {
            Assert.IsFalse(FactorioBenchmarkRunner.IsCrashOutput("<<<END-EXPORT-P1>>>\n<<<END-EXPORT-P2>>>"));
            Assert.IsFalse(FactorioBenchmarkRunner.IsCrashOutput(""));
        }
    }
}
