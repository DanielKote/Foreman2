using System;
using System.Drawing;

namespace Foreman {
    public class GridManager {
        public int CurrentGridUnit = Properties.Settings.Default.MinorGridlines;
        public int CurrentMajorGridUnit = 0;
        public bool ShowGrid = false;
        public bool LockDragToAxis = false;
        public bool ShowZeroAxis = false;
        public Point DragOrigin;

        private static readonly Pen GridPen = new(Color.FromArgb(230, 230, 230), 1);
        private static Pen _gridMPen = new(Color.FromArgb(200, 200, 200), 1);
        private static Brush _gridBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
        private static readonly Pen ZeroAxisPen = new(Color.FromArgb(140, 140, 140), 2);
        private static readonly Pen LockedAxisPen = new(Color.FromArgb(180, 80, 80), 4);
        private const int minGridWidth = 6;

        public static void SetGridColors(Color bgCol, Color fgCol) {
            _gridBrush = new SolidBrush(bgCol);
            _gridMPen = new Pen(fgCol);
        }

        public void Paint(Graphics graphics, float viewScale, Rectangle visibleGraphBounds, BaseNodeElement draggedNode = null) {
            if (ShowGrid) {
                GridPen.Width = 1 / viewScale;
                _gridMPen.Width = 1 / viewScale;
                ZeroAxisPen.Width = 2 / viewScale;
                LockedAxisPen.Width = 3 / viewScale;

                // minor grid

                if (CurrentGridUnit > 0) {
                    if (visibleGraphBounds.Width > CurrentGridUnit && viewScale * CurrentGridUnit > minGridWidth) {
                        for (var ix = visibleGraphBounds.X - visibleGraphBounds.X % CurrentGridUnit;
                            ix < visibleGraphBounds.X + visibleGraphBounds.Width;
                            ix += CurrentGridUnit)
                            graphics.DrawLine(GridPen, ix, visibleGraphBounds.Y, ix, visibleGraphBounds.Y + visibleGraphBounds.Height);

                        for (var iy = visibleGraphBounds.Y - visibleGraphBounds.Y % CurrentGridUnit;
                            iy < visibleGraphBounds.Y + visibleGraphBounds.Height;
                            iy += CurrentGridUnit)
                            graphics.DrawLine(GridPen, visibleGraphBounds.X, iy, visibleGraphBounds.X + visibleGraphBounds.Width, iy);
                    } else
                        graphics.FillRectangle(_gridBrush, visibleGraphBounds);
                }

                // major grid

                if (CurrentMajorGridUnit > CurrentGridUnit) {
                    if (visibleGraphBounds.Width > CurrentMajorGridUnit && viewScale * CurrentMajorGridUnit > minGridWidth) {
                        for (var ix = visibleGraphBounds.X - visibleGraphBounds.X % CurrentMajorGridUnit;
                            ix < visibleGraphBounds.X + visibleGraphBounds.Width;
                            ix += CurrentMajorGridUnit)
                            graphics.DrawLine(_gridMPen, ix, visibleGraphBounds.Y, ix, visibleGraphBounds.Y + visibleGraphBounds.Height);

                        for (var iy = visibleGraphBounds.Y - visibleGraphBounds.Y % CurrentMajorGridUnit;
                            iy < visibleGraphBounds.Y + visibleGraphBounds.Height;
                            iy += CurrentMajorGridUnit)
                            graphics.DrawLine(_gridMPen, visibleGraphBounds.X, iy, visibleGraphBounds.X + visibleGraphBounds.Width, iy);
                    }
                }

                // zero axis

                if (ShowZeroAxis) {
                    graphics.DrawLine(ZeroAxisPen, 0, visibleGraphBounds.Y, 0, visibleGraphBounds.Y + visibleGraphBounds.Height);
                    graphics.DrawLine(ZeroAxisPen, visibleGraphBounds.X, 0, visibleGraphBounds.X + visibleGraphBounds.Width, 0);
                }
            }

            // drag axis

            if (LockDragToAxis && draggedNode != null) {
                var xAxis = DragOrigin.X;
                var yAxis = DragOrigin.Y;
                xAxis = AlignToGrid(xAxis);
                yAxis = AlignToGrid(yAxis);

                graphics.DrawLine(LockedAxisPen, xAxis, visibleGraphBounds.Y, xAxis, visibleGraphBounds.Y + visibleGraphBounds.Height);
                graphics.DrawLine(LockedAxisPen, visibleGraphBounds.X, yAxis, visibleGraphBounds.X + visibleGraphBounds.Width, yAxis);
            }
        }

        public Point AlignToGrid(Point original) {
            return new Point(AlignToGrid(original.X), AlignToGrid(original.Y));
        }

        public int AlignToGrid(int original) {
            if (CurrentGridUnit < 1 || !ShowGrid)
                return original;

            original += Math.Sign(original) * CurrentGridUnit / 2;
            original -= original % CurrentGridUnit;
            return original;
        }
    }
}