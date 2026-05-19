using Foreman;
using Foreman.DataCaching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace ForemanTest {
    [TestClass]
    public class PresetExportFormatTests {
        [TestMethod]
        public void ReadVersion_MissingProperty_ReturnsZero() {
            var dc = new DataCache(false);
            Assert.AreEqual(0, PresetExportFormat.ReadVersion(dc));
            Assert.IsTrue(PresetExportFormat.IsOutdated(dc));
        }

        [TestMethod]
        public void ReadVersion_CurrentVersion_IsNotOutdated() {
            var dc = new DataCache(false);
            dc.GetType().GetProperty("Version")!.SetMethod!.Invoke(dc, [1]);
            Assert.AreEqual(PresetExportFormat.CurrentVersion, PresetExportFormat.ReadVersion(dc));
            Assert.IsFalse(PresetExportFormat.IsOutdated(dc));
        }

        [TestMethod]
        public void ShowOutdatedWarningIfNeeded_OldVersion_RaisesMessage() {
            var dc = new DataCache(false);
            string? shown = null;
            using (UserMessages.UseHandler((text, caption, buttons, icon) => {
                shown = text;
                return DialogResult.OK;
            })) {
                PresetExportFormat.ShowOutdatedWarningIfNeeded(dc);
            }
            Assert.IsNotNull(shown);
            Assert.Contains("older version of Foreman", shown);
            Assert.Contains("Settings menu", shown);
        }

        [TestMethod]
        public void ShowOutdatedWarningIfNeeded_CurrentVersion_DoesNotRaiseMessage() {
            var dc = new DataCache(false);
            dc.GetType().GetProperty("Version")!.SetMethod!.Invoke(dc, [1]);
            bool shown = false;
            using (UserMessages.UseHandler((_, _, _, _) => {
                shown = true;
                return DialogResult.OK;
            })) {
                PresetExportFormat.ShowOutdatedWarningIfNeeded(dc);
            }
            Assert.IsFalse(shown);
        }
    }
}
