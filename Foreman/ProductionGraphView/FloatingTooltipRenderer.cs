using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class FloatingTooltipRenderer
	{
		private const int border = 1;
		private const int arrowSize = 10;

		private static readonly Font size10Font = new Font(FontFamily.GenericSansSerif, 10);
		private static readonly Brush bgBrush = new SolidBrush(Color.FromArgb(65, 65, 65));
		private static readonly Brush borderBrush = Brushes.Black;
		private static readonly Brush textBrush = Brushes.White;
		private static readonly StringFormat stringFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near };

		private Dictionary<FloatingTooltipControl, bool> floatingTooltipControls;

		private ProductionGraphViewer parent;

		public FloatingTooltipRenderer(ProductionGraphViewer graphViewer)
		{
			parent = graphViewer;

			floatingTooltipControls = new Dictionary<FloatingTooltipControl, bool>();
		}

		public void AddToolTip(FloatingTooltipControl tt, bool showOverride) { floatingTooltipControls.Add(tt, showOverride); }
		public void RemoveToolTip(FloatingTooltipControl tt) { floatingTooltipControls.Remove(tt); }

		public void ClearFloatingControls()
		{
			foreach (var control in floatingTooltipControls.Keys.ToArray())
				control.Dispose();
		}

		public void Paint(Graphics graphics, bool paintAll)
		{
			if (paintAll)
			{
				foreach (FloatingTooltipControl fttp in floatingTooltipControls.Keys)
					DrawTooltip(parent.GraphToScreen(fttp.GraphLocation), fttp.Control.Size, fttp.Direction, graphics, null);

				BaseNodeElement element = parent.GetNodeAtPoint(parent.ScreenToGraph(parent.PointToClient(Control.MousePosition)));
				if (element != null)
				{
					foreach (TooltipInfo tti in element.GetToolTips(parent.ScreenToGraph(parent.PointToClient(Control.MousePosition))))
						DrawTooltip(tti.ScreenLocation, tti.ScreenSize, tti.Direction, graphics, tti.Text, tti.CustomDraw);
				}
			}
			else
			{
				foreach (FloatingTooltipControl fttp in floatingTooltipControls.Where(kvp => kvp.Value).Select(kvp => kvp.Key))
					DrawTooltip(parent.GraphToScreen(fttp.GraphLocation), fttp.Control.Size, fttp.Direction, graphics, null);

			}
		}

		private void DrawTooltip(Point screenArrowPoint, Size size, Direction direction, Graphics graphics, String text = null, Action<Graphics, Point> customDraw = null)
		{
			if (text != null)
			{
				SizeF stringSize = graphics.MeasureString(text, size10Font);
				size = new Size((int)stringSize.Width, (int)stringSize.Height);
			}

			Point arrowPoint1 = new Point();
			Point arrowPoint2 = new Point();

			switch (direction)
			{
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

			Rectangle rect = getTooltipScreenBounds(screenArrowPoint, size, direction);
			Point[] points = new Point[] { screenArrowPoint, arrowPoint1, arrowPoint2 };

			if (direction == Direction.None)
			{
				rect = new Rectangle(screenArrowPoint, size);
			}

			graphics.FillPolygon(bgBrush, points);
			GraphicsStuff.FillRoundRect(rect.X - border, rect.Y - border, rect.Width + border * 2, rect.Height + border * 2, 3, graphics, borderBrush);
			GraphicsStuff.FillRoundRect(rect.X, rect.Y, rect.Width, rect.Height, 3, graphics, bgBrush);

			if (text != null)
			{
				Point point;
				if (stringFormat.Alignment == StringAlignment.Center)
					point = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
				else
					point = new Point(rect.X, rect.Y + rect.Height / 2);

				graphics.DrawString(text, size10Font, textBrush, point, stringFormat);
			}

			if (customDraw != null)
				customDraw.Invoke(graphics, rect.Location);
		}

		public Rectangle getTooltipScreenBounds(Point screenArrowPoint, Size screenSize, Direction direction)
		{
			Point centreOffset = new Point();
			int arrowSize = 10;

			switch (direction)
			{
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
