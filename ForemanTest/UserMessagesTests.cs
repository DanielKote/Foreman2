using Foreman;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace ForemanTest {
    [TestClass]
    public class UserMessagesTests : ForemanTestBase {
        [TestMethod]
        public void Show_WithAssemblyTestHandler_ThrowsUnexpectedUserMessageException() {
            var ex = Assert.ThrowsExactly<UnexpectedUserMessageException>(() =>
                UserMessages.Show("test body", "test caption", MessageBoxButtons.YesNo, MessageBoxIcon.Warning));

            Assert.AreEqual("test body", ex.MessageText);
            Assert.AreEqual("test caption", ex.Caption);
            Assert.AreEqual(MessageBoxButtons.YesNo, ex.Buttons);
            Assert.AreEqual(MessageBoxIcon.Warning, ex.Icon);
        }

        [TestMethod]
        public void Show_WithTemporaryHandler_UsesHandlerThenRestoresDefault() {
            using (UserMessages.UseHandler((_, _, buttons, _) =>
                       buttons == MessageBoxButtons.YesNo ? DialogResult.No : DialogResult.OK)) {
                Assert.AreEqual(DialogResult.No,
                    UserMessages.Show("confirm?", "caption", MessageBoxButtons.YesNo));
            }

            Assert.ThrowsExactly<UnexpectedUserMessageException>(() =>
                UserMessages.Show("still blocked in CI mode"));
        }

        [TestMethod]
        public void UseHandler_NestedScopesRestoreInOrder() {
            using (UserMessages.UseHandler((_, _, _, _) => DialogResult.Yes)) {
                Assert.AreEqual(DialogResult.Yes, UserMessages.Show("outer"));
                using (UserMessages.UseHandler((_, _, _, _) => DialogResult.No)) {
                    Assert.AreEqual(DialogResult.No, UserMessages.Show("inner"));
                }
                Assert.AreEqual(DialogResult.Yes, UserMessages.Show("outer again"));
            }

            Assert.ThrowsExactly<UnexpectedUserMessageException>(() => UserMessages.Show("blocked again"));
        }
    }
}
