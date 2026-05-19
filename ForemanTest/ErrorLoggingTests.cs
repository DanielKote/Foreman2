using Foreman.DataCaching;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Windows.Forms;

namespace ForemanTest {
    [TestClass]
    public class ErrorLoggingTests : ForemanTestBase {
        [TestMethod]
        public void LogException_AndLogLine_DoNotThrow() {
            string logPath = Path.Combine(Application.StartupPath, "errorlog.txt");
            ErrorLogging.ClearLog();
            ErrorLogging.LogLine("test line");
            ErrorLogging.LogException(new InvalidOperationException("inner"), "test context");
            Assert.IsTrue(File.Exists(logPath));
            string log = File.ReadAllText(logPath);
            Assert.Contains("test line", log);
            Assert.Contains("test context", log);
            Assert.Contains("InvalidOperationException", log);
        }
    }
}
