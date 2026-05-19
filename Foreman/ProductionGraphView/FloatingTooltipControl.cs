using System;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView {
    public enum Direction { Up, Down, Left, Right, None }

    public record struct TooltipInfo(Point ScreenLocation, Size ScreenSize, Direction Direction, string Text, Action<Graphics, Point> CustomDraw);

    public class FloatingTooltipControl : IDisposable {
        public Control Control { get; private set; }
        public Direction Direction { get; private set; }
        public Point GraphLocation { get; private set; }
        public ProductionGraphViewer GraphViewer { get; private set; }
        public event EventHandler? Closing;

        public FloatingTooltipControl(Control control, Direction direction, Point graphLocation, ProductionGraphViewer parent, bool showOverride, bool useControlLocation) {
            Control = control;
            Direction = direction;
            GraphLocation = graphLocation;
            GraphViewer = parent;

            parent.ToolTipRenderer.AddToolTip(this, showOverride);
            parent.Controls.Add(control);
            Rectangle ttRect = FloatingTooltipRenderer.getTooltipScreenBounds(parent.GraphToScreen(graphLocation), control.Size, direction);

            if (!useControlLocation)
                control.Location = ttRect.Location;
            control.Focus();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            Control.Dispose();
            GraphViewer.ToolTipRenderer.RemoveToolTip(this);
            Closing?.Invoke(this, EventArgs.Empty);
        }
    }
}
