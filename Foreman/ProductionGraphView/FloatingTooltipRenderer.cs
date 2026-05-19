using Foreman.Controls;
using Foreman.ProductionGraphView.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView {
    public class FloatingTooltipRenderer(ProductionGraphViewer graphViewer) {
        private const int border = 2;
        private const int textPadding = 2;
        private const int arrowSize = 10;

        private static readonly Font size10Font = new(FontFamily.GenericSansSerif, 10);
        private static readonly Brush bgBrush = new SolidBrush(Color.FromArgb(65, 65, 65));
        private static readonly Brush borderBrush = Brushes.Black;
        private static readonly Brush textBrush = Brushes.White;
        private static readonly StringFormat stringFormat = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };

        private readonly Dictionary<FloatingTooltipControl, bool> floatingTooltipControls = [];
        private readonly List<TooltipInfo> extraTooltips = [];

        private readonly ProductionGraphViewer parent = graphViewer;

        public void AddToolTip(FloatingTooltipControl tt, bool showOverride) { floatingTooltipControls.Add(tt, showOverride); }
        public void RemoveToolTip(FloatingTooltipControl tt) { floatingTooltipControls.Remove(tt); }
        public bool ContainsTooltip(FloatingTooltipControl tt) => floatingTooltipControls.ContainsKey(tt);
        public void AddExtraToolTip(TooltipInfo tt) { extraTooltips.Add(tt); }
        public void ClearExtraToolTips() { extraTooltips.Clear(); }

        public void ClearFloatingControls() {
            foreach (var control in floatingTooltipControls.Keys.ToArray())
                control.Dispose();
        }

        public void Paint(Graphics graphics, bool paintAll) {
            if (paintAll) {
                foreach (FloatingTooltipControl fttp in floatingTooltipControls.Keys)
                    DrawTooltip(parent.GraphToScreen(fttp.GraphLocation), fttp.Control.Size, fttp.Direction, graphics, null);
                foreach (TooltipInfo tti in extraTooltips)
                    DrawTooltip(tti.ScreenLocation, tti.ScreenSize, tti.Direction, graphics, tti.Text, tti.CustomDraw);

                BaseNodeElement? element = parent.GetNodeAtPoint(parent.ScreenToGraph(parent.PointToClient(Control.MousePosition)));
                if (element?.GetToolTips(parent.ScreenToGraph(parent.PointToClient(Control.MousePosition))) is List<TooltipInfo> tooltips) {
                    foreach (TooltipInfo tti in tooltips)
                        DrawTooltip(tti.ScreenLocation, tti.ScreenSize, tti.Direction, graphics, tti.Text, tti.CustomDraw);
                }
            } else {
                foreach (FloatingTooltipControl fttp in floatingTooltipControls.Where(kvp => kvp.Value).Select(kvp => kvp.Key))
                    DrawTooltip(parent.GraphToScreen(fttp.GraphLocation), fttp.Control.Bounds, fttp.Direction, graphics, null);
                foreach (TooltipInfo tti in extraTooltips)
                    DrawTooltip(tti.ScreenLocation, tti.ScreenSize, tti.Direction, graphics, tti.Text, tti.CustomDraw);
            }
        }

        private static void DrawTooltip(Point screenArrowPoint, Size size, Direction direction, Graphics graphics, string? text = null, Action<Graphics, Point>? custonDraw = null) //places the tool tip centered on the arrow
                {
            if (text != null) {
                SizeF stringSize = graphics.MeasureString(text, size10Font);
                size = new Size((int)stringSize.Width + (textPadding * 2), (int)stringSize.Height + (textPadding * 2));
            }

            var rect = direction == Direction.None
                ? new Rectangle(screenArrowPoint, size)
                : getTooltipScreenBounds(screenArrowPoint, size, direction);

            DrawTooltip(screenArrowPoint, rect, direction, graphics, text, custonDraw);
        }


        private static void DrawTooltip(Point screenArrowPoint, Rectangle bounds, Direction direction, Graphics graphics, string? text = null, Action<Graphics, Point>? customDraw = null) //places the tool tip based on the bounds provided
                {
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

            var points = new Point[] { screenArrowPoint, arrowPoint1, arrowPoint2 };

            graphics.FillPolygon(bgBrush, points);
            GraphicsStuff.FillRoundRect(bounds.X - border, bounds.Y - border, bounds.Width + border * 2, bounds.Height + border * 2, 3, graphics, borderBrush);
            GraphicsStuff.FillRoundRect(bounds.X, bounds.Y, bounds.Width, bounds.Height, 3, graphics, bgBrush);

            if (text != null) {
                var point = stringFormat.Alignment == StringAlignment.Center
                    ? new Point(bounds.X + textPadding + bounds.Width / 2, bounds.Y + textPadding - 1 + bounds.Height / 2)
                    : new Point(bounds.X + textPadding, bounds.Y + textPadding - 1 + bounds.Height / 2);

                graphics.DrawString(text, size10Font, textBrush, point, stringFormat);
            }

            if (customDraw != null)
                customDraw.Invoke(graphics, bounds.Location);
        }

        public static Rectangle getTooltipScreenBounds(Point screenArrowPoint, Size screenSize, Direction direction) {
            var centreOffset = new Point();
            int arrowSize = 10;

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
            int X = (screenArrowPoint.X + centreOffset.X - screenSize.Width / 2);
            int Y = (screenArrowPoint.Y + centreOffset.Y - screenSize.Height / 2);
            int Width = screenSize.Width;
            int Height = screenSize.Height;

            return new Rectangle(X, Y, Width, Height);
        }
    }
}
