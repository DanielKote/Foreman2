using System;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    public enum Direction {
        Up,
        Down,
        Left,
        Right,
        None
    }

    public struct TooltipInfo(Point screenLocation, Size screenSize, Direction direction, string text, Action<Graphics, Point> customDraw) {
        public Point ScreenLocation = screenLocation;
        public Size ScreenSize = screenSize;
        public Direction Direction = direction;
        public string Text = text;
        public Action<Graphics, Point> CustomDraw = customDraw;
    }

    public class FloatingTooltipControl : IDisposable {
        public Control Control { get; private set; }
        public Direction Direction { get; private set; }
        public Point GraphLocation { get; private set; }
        public ProductionGraphViewer GraphViewer { get; private set; }
        public event EventHandler Closing;

        public FloatingTooltipControl(Control control, Direction direction, Point graphLocation, ProductionGraphViewer parent, bool showOverride,
            bool useControlLocation) {
            Control = control;
            Direction = direction;
            GraphLocation = graphLocation;
            GraphViewer = parent;

            parent.ToolTipRenderer.AddToolTip(this, showOverride);
            parent.Controls.Add(control);
            var ttRect = GraphViewer.ToolTipRenderer.GetTooltipScreenBounds(parent.GraphToScreen(graphLocation), control.Size, direction);

            if (!useControlLocation)
                control.Location = ttRect.Location;
            control.Focus();
        }

        public void Dispose() {
            Control.Dispose();
            GraphViewer.ToolTipRenderer.RemoveToolTip(this);
            Closing?.Invoke(this, null);
        }
    }
}