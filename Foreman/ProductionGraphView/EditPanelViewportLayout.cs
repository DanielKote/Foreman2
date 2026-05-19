using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    /// <summary>Caps floating edit panels to the graph viewer and scrolls overflowing content.</summary>
    public static class EditPanelViewportLayout {
        public const string ScrollHostName = "viewportScrollHost";

        public static Panel EnsureScrollHost(UserControl editPanel, Control contentRoot) {
            Control? existing = editPanel.Controls.Find(ScrollHostName, false).FirstOrDefault();
            if (existing is Panel scrollHost)
                return scrollHost;

            editPanel.Controls.Remove(contentRoot);
            scrollHost = new Panel {
                Name = ScrollHostName,
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Margin = Padding.Empty,
                BackColor = editPanel.BackColor,
            };
            contentRoot.Dock = DockStyle.None;
            contentRoot.Location = Point.Empty;
            contentRoot.Margin = Padding.Empty;
            scrollHost.Controls.Add(contentRoot);
            editPanel.Controls.Add(scrollHost);
            return scrollHost;
        }

        public static Size MeasureContentSize(Control contentRoot) {
            contentRoot.PerformLayout();
            Size preferred = contentRoot.GetPreferredSize(Size.Empty);
            if (preferred.Width > 0 && preferred.Height > 0)
                return preferred;
            return contentRoot.Size;
        }

        public static void Apply(UserControl editPanel, Control contentRoot, ProductionGraphViewer viewer) {
            Panel scrollHost = EnsureScrollHost(editPanel, contentRoot);

            int margin = EditPanelScreenLayout.DefaultMargin;
            int maxHeight = Math.Max(1, viewer.ClientSize.Height - margin * 2);
            int maxWidth = Math.Max(1, viewer.ClientSize.Width - margin * 2);
            int scrollBarWidth = SystemInformation.VerticalScrollBarWidth;

            editPanel.AutoSize = false;
            contentRoot.AutoSize = true;
            if (contentRoot is UserControl contentPanel)
                contentPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            Size natural = MeasureContentSize(contentRoot);
            bool needsVerticalScroll = natural.Height > maxHeight;
            int width = Math.Min(Math.Max(1, natural.Width), maxWidth);
            if (needsVerticalScroll)
                width = Math.Min(maxWidth, width + scrollBarWidth);
            int height = Math.Min(Math.Max(1, natural.Height), maxHeight);

            editPanel.Size = new Size(width, height);
            scrollHost.PerformLayout();
            LayoutScrollableContent(scrollHost, contentRoot);

            if (needsVerticalScroll && scrollHost.HorizontalScroll.Visible) {
                editPanel.Width = Math.Min(maxWidth, editPanel.Width + scrollBarWidth);
                scrollHost.PerformLayout();
                LayoutScrollableContent(scrollHost, contentRoot);
            }
        }

        private static void LayoutScrollableContent(Panel scrollHost, Control contentRoot) {
            int contentWidth = Math.Max(1, scrollHost.ClientSize.Width);
            contentRoot.Width = contentWidth;
            contentRoot.PerformLayout();
            Size natural = MeasureContentSize(contentRoot);
            contentRoot.Size = new Size(contentWidth, Math.Max(1, natural.Height));
        }
    }
}