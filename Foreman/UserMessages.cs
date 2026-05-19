using System;
using System.Windows.Forms;

namespace Foreman {
    /// <summary>Raised when production code shows a user message while <see cref="UserMessages.TestHandler"/> is active.</summary>
    public sealed class UnexpectedUserMessageException(string messageText, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) : Exception(FormatMessage(messageText, caption, buttons, icon)) {
        public string MessageText { get; } = messageText;
        public string Caption { get; } = caption;
        public MessageBoxButtons Buttons { get; } = buttons;
        public MessageBoxIcon Icon { get; } = icon;

        private static string FormatMessage(string messageText, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) {
            string title = string.IsNullOrEmpty(caption) ? "(no caption)" : caption;
            return $"Unexpected user message [{buttons}, {icon}] {title}: {messageText}";
        }
    }

    /// <summary>User-visible modal messages. Use instead of <see cref="MessageBox.Show(string, string, MessageBoxButtons, MessageBoxIcon)"/> so tests never block on real dialogs.</summary>
    public static class UserMessages {
        public delegate DialogResult ShowHandler(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);

        /// <summary>When set (e.g. by ForemanTest), all <see cref="Show(string)"/> calls use this instead of WinForms.</summary>
        public static ShowHandler? TestHandler { get; set; }

        /// <summary>Temporarily replaces <see cref="TestHandler"/>; disposing restores the previous handler.</summary>
        public static IDisposable UseHandler(ShowHandler handler) {
            ArgumentNullException.ThrowIfNull(handler);
            return new HandlerScope(handler);
        }

        public static DialogResult Show(string text) => Show(text, string.Empty);

        public static DialogResult Show(string text, string caption) =>
            Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons) =>
            Show(text, caption, buttons, MessageBoxIcon.None);

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) {
            return TestHandler is { } handler ? handler(text, caption, buttons, icon) : MessageBox.Show(text, caption, buttons, icon);
        }

        internal static DialogResult FailTestOnAnyMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) =>
            throw new UnexpectedUserMessageException(text, caption, buttons, icon);

        private sealed class HandlerScope : IDisposable {
            private readonly ShowHandler? previous;
            private bool disposed;

            public HandlerScope(ShowHandler handler) {
                previous = TestHandler;
                TestHandler = handler;
            }

            public void Dispose() {
                if (disposed)
                    return;
                TestHandler = previous;
                disposed = true;
            }
        }
    }
}
