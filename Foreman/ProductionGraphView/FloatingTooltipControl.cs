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
		public TooltipInfo(Point screenLocation, Size screenSize, Direction direction, String text)
		{
			ScreenLocation = screenLocation;
			ScreenSize = screenSize;
			Direction = direction;
			Text = text;
		}

		public Point ScreenLocation;
		public Size ScreenSize;
		public Direction Direction;
		public String Text;
	}

	public class FloatingTooltipControl : IDisposable
	{
		public Control Control { get; private set; }
		public Direction Direction { get; private set; }
		public Point GraphLocation { get; private set; }
		public ProductionGraphViewer GraphViewer { get; private set; }
		public event EventHandler Closing;

		public FloatingTooltipControl(Control control, Direction direction, Point graphLocation, ProductionGraphViewer parent)
		{
			Control = control;
			Direction = direction;
			GraphLocation = graphLocation;
			GraphViewer = parent;

			parent.floatingTooltipControls.Add(this);
			parent.Controls.Add(control);
			Rectangle ttRect = parent.getTooltipScreenBounds(parent.GraphToScreen(graphLocation), control.Size, direction);
			control.Location = ttRect.Location;
			control.Focus();
		}

		public void Dispose()
		{
			Control.Dispose();
			GraphViewer.floatingTooltipControls.Remove(this);
			if (Closing != null)
			{
				Closing.Invoke(this, null);
			}
		}
	}
}
