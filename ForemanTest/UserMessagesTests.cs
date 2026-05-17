using Foreman;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace ForemanTest {
    [TestClass]
    public class UserMessagesTests {
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
            UserMessages.ShowHandler? previous = UserMessages.TestHandler;
            try {
                UserMessages.TestHandler = (_, _, buttons, _) =>
                    buttons == MessageBoxButtons.YesNo ? DialogResult.No : DialogResult.OK;

                Assert.AreEqual(DialogResult.No,
                    UserMessages.Show("confirm?", "caption", MessageBoxButtons.YesNo));
            } finally {
                UserMessages.TestHandler = previous;
            }

            Assert.ThrowsExactly<UnexpectedUserMessageException>(() =>
                UserMessages.Show("still blocked in CI mode"));
        }
    }
}