using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public class FloatingTooltipRenderer(ProductionGraphViewer graphViewer) {
        private const int border = 2;
        private const int textPadding = 2;
        private const int arrowSize = 10;

        private static readonly Font Size10Font = new(FontFamily.GenericSansSerif, 10);
        private static readonly Brush BgBrush = new SolidBrush(Color.FromArgb(65, 65, 65));
        private static readonly Brush BorderBrush = Brushes.Black;
        private static readonly Brush TextBrush = Brushes.White;
        private static readonly StringFormat StringFormat = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };

        private Dictionary<FloatingTooltipControl, bool> _floatingTooltipControls = new();
        private List<TooltipInfo> _extraTooltips = [];

        public void AddToolTip(FloatingTooltipControl tt, bool showOverride) {
            _floatingTooltipControls.Add(tt, showOverride);
        }

        public void RemoveToolTip(FloatingTooltipControl tt) {
            _floatingTooltipControls.Remove(tt);
        }

        public void AddExtraToolTip(TooltipInfo tt) {
            _extraTooltips.Add(tt);
        }

        public void ClearExtraToolTips() {
            _extraTooltips.Clear();
        }

        public void ClearFloatingControls() {
            foreach (var control in _floatingTooltipControls.Keys.ToArray())
                control.Dispose();
        }

        public void Paint(Graphics graphics, bool paintAll) {
            if (paintAll) {
                foreach (var fttp in _floatingTooltipControls.Keys)
                    DrawTooltip(graphViewer.GraphToScreen(fttp.GraphLocation), fttp.Control.Size, fttp.Direction, graphics);
                foreach (var tti in _extraTooltips)
                    DrawTooltip(tti.ScreenLocation, tti.ScreenSize, tti.Direction, graphics, tti.Text, tti.CustomDraw);

                var element = graphViewer.GetNodeAtPoint(graphViewer.ScreenToGraph(graphViewer.PointToClient(Control.MousePosition)));
                if (element == null)
                    return;

                foreach (var tti in element.GetToolTips(graphViewer.ScreenToGraph(graphViewer.PointToClient(Control.MousePosition))))
                    DrawTooltip(tti.ScreenLocation, tti.ScreenSize, tti.Direction, graphics, tti.Text, tti.CustomDraw);
            } else {
                foreach (var fttp in _floatingTooltipControls.Where(kvp => kvp.Value).Select(kvp => kvp.Key))
                    DrawTooltip(graphViewer.GraphToScreen(fttp.GraphLocation), fttp.Control.Bounds, fttp.Direction, graphics);
                foreach (var tti in _extraTooltips)
                    DrawTooltip(tti.ScreenLocation, tti.ScreenSize, tti.Direction, graphics, tti.Text, tti.CustomDraw);
            }
        }

        // places the tool tip centered on the arrow
        private void DrawTooltip(
            Point screenArrowPoint,
            Size size,
            Direction direction,
            Graphics graphics,
            string text = null,
            Action<Graphics, Point> customDraw = null
        ) {
            if (text != null) {
                var stringSize = graphics.MeasureString(text, Size10Font);
                size = new Size((int) stringSize.Width + textPadding * 2, (int) stringSize.Height + textPadding * 2);
            }

            var rect = direction != Direction.None
                ? GetTooltipScreenBounds(screenArrowPoint, size, direction)
                : new Rectangle(screenArrowPoint, size);

            DrawTooltip(screenArrowPoint, rect, direction, graphics, text, customDraw);
        }


        // places the tool tip based on the bounds provided
        private void DrawTooltip(
            Point screenArrowPoint,
            Rectangle bounds,
            Direction direction,
            Graphics graphics,
            string text = null,
            Action<Graphics, Point> customDraw = null
        ) {
            var arrowPoint1 = new Point();
            var arrowPoint2 = new Point();

            switch (direction) {
                case Direction.Down:
                    arrowPoint1 = new Point(screenArrowPoint.X - arrowSize / 2, screenArrowPoint.Y - arrowSize);
                    arrowPoint2 = new Point(screenArrowPoint.X + arrowSize / 2, screenArrowPoint.Y - arrowSize);
                    break;
                case Direction.Left:
                    arrowPoint1 = new Point(screenArrowPoint.X + arrowSize, screenArrowPoint.Y - arrowSize / 2);
                    arrowPoint2 = new Point(screenArrowPoint.X + arrowSize, screenArrowPoint.Y + arrowSize / 2);
                    break;
                case Direction.Up:
                    arrowPoint1 = new Point(screenArrowPoint.X - arrowSize / 2, screenArrowPoint.Y + arrowSize);
                    arrowPoint2 = new Point(screenArrowPoint.X + arrowSize / 2, screenArrowPoint.Y + arrowSize);
                    break;
                case Direction.Right:
                    arrowPoint1 = new Point(screenArrowPoint.X - arrowSize, screenArrowPoint.Y - arrowSize / 2);
                    arrowPoint2 = new Point(screenArrowPoint.X - arrowSize, screenArrowPoint.Y + arrowSize / 2);
                    break;
            }

            var points = new[] { screenArrowPoint, arrowPoint1, arrowPoint2 };

            graphics.FillPolygon(BgBrush, points);
            GraphicsStuff.FillRoundRect(bounds.X - border, bounds.Y - border, bounds.Width + border * 2, bounds.Height + border * 2, 3, graphics, BorderBrush);
            GraphicsStuff.FillRoundRect(bounds.X, bounds.Y, bounds.Width, bounds.Height, 3, graphics, BgBrush);

            if (text != null) {
                var point = StringFormat.Alignment == StringAlignment.Center
                    ? new Point(bounds.X + textPadding + bounds.Width / 2, bounds.Y + textPadding - 1 + bounds.Height / 2)
                    : new Point(bounds.X + textPadding, bounds.Y + textPadding - 1 + bounds.Height / 2);

                graphics.DrawString(text, Size10Font, TextBrush, point, StringFormat);
            }

            customDraw?.Invoke(graphics, bounds.Location);
        }

        public Rectangle GetTooltipScreenBounds(Point screenArrowPoint, Size screenSize, Direction direction) {
            var centreOffset = new Point();
            var arrowSize = 10;

            switch (direction) {
                case Direction.Down:
                    centreOffset = new Point(0, -arrowSize - screenSize.Height / 2);
                    break;
                case Direction.Left:
                    centreOffset = new Point(arrowSize + screenSize.Width / 2, 0);
                    break;
                case Direction.Up:
                    centreOffset = new Point(0, arrowSize + screenSize.Height / 2);
                    break;
                case Direction.Right:
                    centreOffset = new Point(-arrowSize - screenSize.Width / 2, 0);
                    break;
            }

            var x = screenArrowPoint.X + centreOffset.X - screenSize.Width / 2;
            var y = screenArrowPoint.Y + centreOffset.Y - screenSize.Height / 2;
            var width = screenSize.Width;
            var height = screenSize.Height;

            return new Rectangle(x, y, width, height);
        }
    }
}