using SvgNet.SvgGdi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public enum Direction { Up, Down, Left, Right, None }

	public struct TooltipInfo
	{
		public TooltipInfo(Point screenLocation, Size screenSize, Direction direction, string text, Action<IGraphics, Point> customDraw)
		{
			ScreenLocation = screenLocation;
			ScreenSize = screenSize;
			Direction = direction;
			Text = text;

			CustomDraw = customDraw;
		}

		public Point ScreenLocation;
		public Size ScreenSize;
		public Direction Direction;
		public string Text;
		public Action<IGraphics, Point> CustomDraw;
	}

	public class FloatingTooltipControl : IDisposable
	{
		public Control Control { get; private set; }
		public Direction Direction { get; private set; }
		public Point GraphLocation { get; private set; }
		public ProductionGraphViewer GraphViewer { get; private set; }
		public event EventHandler Closing;

		public FloatingTooltipControl(Control control, Direction direction, Point graphLocation, ProductionGraphViewer parent, bool showOverride, bool useControlLocation)
		{
			Control = control;
			Direction = direction;
			GraphLocation = graphLocation;
			GraphViewer = parent;

			parent.ToolTipRenderer.AddToolTip(this, showOverride);
			parent.Controls.Add(control);
			Rectangle ttRect = GraphViewer.ToolTipRenderer.getTooltipScreenBounds(parent.GraphToScreen(graphLocation), control.Size, direction);

			if (!useControlLocation)
				control.Location = ttRect.Location;
			control.Focus();
		}

		public void Dispose()
		{
			Control.Dispose();
			GraphViewer.ToolTipRenderer.RemoveToolTip(this);
			if (Closing != null)
			{
				Closing.Invoke(this, null);
			}
		}
	}
}
