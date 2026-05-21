using System;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView {
    /// <summary>Screen-space placement for floating node edit panels (no viewport pan).</summary>
    public static class EditPanelScreenLayout {
        public const int DefaultMargin = 25;

        public static Rectangle ClampRectToViewer(Rectangle bounds, int viewerWidth, int viewerHeight, int margin = DefaultMargin) {
            int x = bounds.X;
            int y = bounds.Y;
            int maxX = Math.Max(margin, viewerWidth - margin - bounds.Width);
            int maxY = Math.Max(margin, viewerHeight - margin - bounds.Height);
            if (x < margin)
                x = margin;
            else if (x > maxX)
                x = maxX;
            if (y < margin)
                y = margin;
            else if (y > maxY)
                y = maxY;
            return new Rectangle(x, y, bounds.Width, bounds.Height);
        }

        public static Point GetShiftToFit(Rectangle desiredBounds, int viewerWidth, int viewerHeight, int margin = DefaultMargin) {
            Rectangle clamped = ClampRectToViewer(desiredBounds, viewerWidth, viewerHeight, margin);
            return new Point(clamped.X - desiredBounds.X, clamped.Y - desiredBounds.Y);
        }

        public static bool FitsViewer(Rectangle bounds, int viewerWidth, int viewerHeight, int margin = DefaultMargin) =>
            bounds.Left >= margin
            && bounds.Top >= margin
            && bounds.Right <= viewerWidth - margin
            && bounds.Bottom <= viewerHeight - margin;

        public static void ShiftControlsToFit(Rectangle desiredUnion, int viewerWidth, int viewerHeight, int margin, params Control[] panels) {
            Point delta = GetShiftToFit(desiredUnion, viewerWidth, viewerHeight, margin);
            if (delta.X == 0 && delta.Y == 0)
                return;
            foreach (Control panel in panels)
                panel.Location = Point.Add(panel.Location, (Size)delta);
        }

        /// <summary>Places a floating chooser near <paramref name="anchor"/> (client coords), then clamps to the viewer.</summary>
        public static Point GetChooserTopLeft(Point anchor, Size panelSize, int viewerWidth, int viewerHeight, int margin = DefaultMargin) {
            const int anchorInsetX = 24;
            const int anchorInsetY = 16;
            var desired = new Rectangle(anchor.X - anchorInsetX, anchor.Y - anchorInsetY, panelSize.Width, panelSize.Height);
            return ClampRectToViewer(desired, viewerWidth, viewerHeight, margin).Location;
        }
    }

}
