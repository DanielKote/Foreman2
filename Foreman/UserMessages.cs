using System;
using System.Windows.Forms;

namespace Foreman {
    /// <summary>Raised when production code shows a user message while <see cref="UserMessages.TestHandler"/> is active.</summary>
    public sealed class UnexpectedUserMessageException : Exception {
        public string MessageText { get; }
        public string Caption { get; }
        public MessageBoxButtons Buttons { get; }
        public MessageBoxIcon Icon { get; }

        public UnexpectedUserMessageException(string messageText, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
            : base(FormatMessage(messageText, caption, buttons, icon)) {
            MessageText = messageText;
            Caption = caption;
            Buttons = buttons;
            Icon = icon;
        }

        private static string FormatMessage(string messageText, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) {
            string title = string.IsNullOrEmpty(caption) ? "(no caption)" : caption;
            return $"Unexpected user message [{buttons}, {icon}] {title}: {messageText}";
        }
    }

    /// <summary>User-visible modal messages. Use instead of <see cref="MessageBox.Show"/> so tests never block on real dialogs.</summary>
    public static class UserMessages {
        public delegate DialogResult ShowHandler(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);

        /// <summary>When set (e.g. by ForemanTest), all <see cref="Show"/> calls use this instead of WinForms.</summary>
        public static ShowHandler? TestHandler { get; set; }

        public static DialogResult Show(string text) => Show(text, string.Empty);

        public static DialogResult Show(string text, string caption) =>
            Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons) =>
            Show(text, caption, buttons, MessageBoxIcon.None);

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) {
            if (TestHandler is { } handler)
                return handler(text, caption, buttons, icon);
            return MessageBox.Show(text, caption, buttons, icon);
        }

        internal static DialogResult FailTestOnAnyMessage(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) =>
            throw new UnexpectedUserMessageException(text, caption, buttons, icon);
    }
}