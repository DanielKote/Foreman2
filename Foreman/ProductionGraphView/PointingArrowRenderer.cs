using System;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public class PointingArrowRenderer(ProductionGraphViewer viewer) {
        private enum Border {
            Top,
            Bottom,
            Left,
            Right
        }

        public bool ShowErrorArrows { get; set; }
        public bool ShowWarningArrows { get; set; }
        public bool ShowDisconnectedArrows { get; set; }
        public bool ShowOuNodeArrows { get; set; }

        private static readonly Pen ErrorArrowPen = new(Brushes.DarkRed, ArrowScale)
            { StartCap = System.Drawing.Drawing2D.LineCap.Square, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };

        private static readonly Pen WarningArrowPen = new(Brushes.DarkOrange, ArrowScale)
            { StartCap = System.Drawing.Drawing2D.LineCap.Square, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };

        private static readonly Pen DisconnectedArrowPen = new(Brushes.Goldenrod, ArrowScale)
            { StartCap = System.Drawing.Drawing2D.LineCap.Square, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };

        private static readonly Pen OuNodeArrowPen = new(Brushes.Goldenrod, ArrowScale)
            { StartCap = System.Drawing.Drawing2D.LineCap.Square, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };

        private const int ArrowScale = 8;
        private const int Padding = 10;

        public void Paint(Graphics graphics, ProductionGraph graph) {
            if (ShowErrorArrows)
                foreach (var errorPoint in graph.Nodes.Where(node => node.State == NodeState.Error).Select(node => viewer.GraphToScreen(node.Location)))
                    DrawArrow(graphics, errorPoint, ErrorArrowPen);
            if (ShowWarningArrows)
                foreach (var warningPoint in graph.Nodes.Where(node => node.State == NodeState.Warning).Select(node => viewer.GraphToScreen(node.Location)))
                    DrawArrow(graphics, warningPoint, WarningArrowPen);
            if (ShowDisconnectedArrows)
                foreach (var errorPoint in graph.Nodes.Where(node => node.State == NodeState.MissingLink).Select(node => viewer.GraphToScreen(node.Location)))
                    DrawArrow(graphics, errorPoint, DisconnectedArrowPen);
            if (ShowOuNodeArrows)
                foreach (var errorPoint in graph.Nodes.Where(node => node.IsOverproducing() || node.ManualRateNotMet())
                    .Select(node => viewer.GraphToScreen(node.Location)))
                    DrawArrow(graphics, errorPoint, OuNodeArrowPen);
        }

        private void DrawArrow(Graphics graphics, Point nodeOrigin, Pen arrowPen) {
            // roughly 'in bounds'
            if (nodeOrigin.X > -Padding && nodeOrigin.X < viewer.Width + Padding && nodeOrigin.Y > -Padding && nodeOrigin.Y < viewer.Height + Padding)
                return;

            var center = new Point(viewer.Width / 2, viewer.Height / 2);
            Point borderPoint;

            if (nodeOrigin.Y < Padding) {
                borderPoint = IntersectionPoint(nodeOrigin, center, Padding, true);

                // within the top segment of the border
                if (borderPoint.X >= Padding && borderPoint.X <= viewer.Width - Padding) {
                    DrawArrow(graphics, center, borderPoint, ArrowScale * 4, arrowPen);
                    return;
                }
            }

            if (nodeOrigin.Y > viewer.Height - Padding) {
                borderPoint = IntersectionPoint(nodeOrigin, center, viewer.Height - Padding, true);

                // within the bottom segment of the border
                if (borderPoint.X >= Padding && borderPoint.X <= viewer.Width - Padding) {
                    DrawArrow(graphics, center, borderPoint, ArrowScale * 4, arrowPen);
                    return;
                }
            }

            if (nodeOrigin.X < Padding) {
                borderPoint = IntersectionPoint(nodeOrigin, center, Padding, false);

                // within the left segment of the border
                if (borderPoint.Y >= Padding && borderPoint.Y <= viewer.Height - Padding) {
                    DrawArrow(graphics, center, borderPoint, ArrowScale * 4, arrowPen);
                    return;
                }
            }

            if (nodeOrigin.X > viewer.Width - Padding) {
                borderPoint = IntersectionPoint(nodeOrigin, center, viewer.Width - Padding, false);

                // within the right segment of the border
                if (borderPoint.Y >= Padding && borderPoint.Y <= viewer.Height - Padding) {
                    DrawArrow(graphics, center, borderPoint, ArrowScale * 4, arrowPen);
                    return;
                }
            }

            // if we are here, then there was no need to paint the arrow (within borders).
            // Due to previous checks this shouldn't happen though.
        }

        private void DrawArrow(Graphics graphics, Point origin, Point endpoint, float length, Pen arrowPen) {
            var sizedVector = new SizeF(origin.X - endpoint.X, origin.Y - endpoint.Y);
            var vectorLength = (float) Math.Sqrt(sizedVector.Width * sizedVector.Width + sizedVector.Height * sizedVector.Height);
            sizedVector = new SizeF(sizedVector.Width * length / vectorLength, sizedVector.Height * length / vectorLength);
            origin = Point.Add(endpoint, sizedVector.ToSize());
            graphics.DrawLine(arrowPen, origin, endpoint);
        }

        // c is x if vertical line, and y if horizontal line
        private Point IntersectionPoint(Point a, Point b, int c, bool horizontal) {
            return horizontal
                ? new Point(a.X + (b.X - a.X) * (c - a.Y) / (b.Y - a.Y), c)
                : new Point(c, a.Y + (b.Y - a.Y) * (c - a.X) / (b.X - a.X));
        }
    }
}