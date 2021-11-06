using System;
using System.Drawing;
using System.Linq;

namespace Foreman
{
	public class PointingArrowRenderer
	{
		private enum Border { Top, Bottom, Left, Right }

		public bool ShowErrorArrows { get; set; }
		public bool ShowWarningArrows { get; set; }
		public bool ShowDisconnectedArrows { get; set; }

		private static readonly Pen ErrorArrowPen = new Pen(Brushes.DarkRed, ArrowScale) { StartCap = System.Drawing.Drawing2D.LineCap.Square, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };
		private static readonly Pen WarningArrowPen = new Pen(Brushes.DarkOrange, ArrowScale) { StartCap = System.Drawing.Drawing2D.LineCap.Square, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };
		private static readonly Pen DisconnectedArrowPen = new Pen(Brushes.Goldenrod, ArrowScale) { StartCap = System.Drawing.Drawing2D.LineCap.Square, EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor };
		
		private const int ArrowScale = 8;
		private const int Padding = 10;

		private readonly ProductionGraphViewer Viewer;

		public PointingArrowRenderer(ProductionGraphViewer viewer) { Viewer = viewer; }

		public void Paint(Graphics graphics, ProductionGraph graph)
		{
			if (ShowErrorArrows)
				foreach (Point errorPoint in graph.Nodes.Where(node => node.State == NodeState.Error).Select(node => Viewer.GraphToScreen(node.Location)))
					DrawArrow(graphics, errorPoint, ErrorArrowPen);
			if (ShowWarningArrows)
				foreach (Point warningPoint in graph.Nodes.Where(node => node.State == NodeState.Warning).Select(node => Viewer.GraphToScreen(node.Location)))
					DrawArrow(graphics, warningPoint, WarningArrowPen);
			if (ShowDisconnectedArrows)
				foreach (Point errorPoint in graph.Nodes.Where(node => node.State == NodeState.MissingLink).Select(node => Viewer.GraphToScreen(node.Location)))
					DrawArrow(graphics, errorPoint, DisconnectedArrowPen);
		}

		private void DrawArrow(Graphics graphics, Point nodeOrigin, Pen arrowPen)
		{
			if (nodeOrigin.X > -Padding && nodeOrigin.X < Viewer.Width + Padding && nodeOrigin.Y > -Padding && nodeOrigin.Y < Viewer.Height + Padding) //roughly 'in bounds'
				return;

			Point center = new Point(Viewer.Width / 2, Viewer.Height / 2);
			Point borderPoint;

			if (nodeOrigin.Y < Padding)
			{
				borderPoint = IntersectionPoint(nodeOrigin, center, Padding, true);
				if (borderPoint.X >= Padding && borderPoint.X <= Viewer.Width - Padding) //within the top segment of the border
				{
					DrawArrow(graphics, center, borderPoint, ArrowScale * 4, arrowPen);
					return;
				}
			}

			if (nodeOrigin.Y > Viewer.Height - Padding)
			{
				borderPoint = IntersectionPoint(nodeOrigin, center, Viewer.Height - Padding, true);
				if (borderPoint.X >= Padding && borderPoint.X <= Viewer.Width - Padding) //within the bottom segment of the border
				{
					DrawArrow(graphics, center, borderPoint, ArrowScale * 4, arrowPen);
					return;
				}
			}

			if (nodeOrigin.X < Padding)
			{
				borderPoint = IntersectionPoint(nodeOrigin, center, Padding, false);
				if (borderPoint.Y >= Padding && borderPoint.Y <= Viewer.Height - Padding) //within the left segment of the border
				{
					DrawArrow(graphics, center, borderPoint, ArrowScale * 4, arrowPen);
					return;
				}
			}

			if (nodeOrigin.X > Viewer.Width - Padding)
			{
				borderPoint = IntersectionPoint(nodeOrigin, center, Viewer.Width - Padding, false);
				if (borderPoint.Y >= Padding && borderPoint.Y <= Viewer.Height - Padding) //within the right segment of the border
				{
					DrawArrow(graphics, center, borderPoint, ArrowScale * 4, arrowPen);
					return;
				}
			}
			//if we are here, then there was no need to paint the arrow (within borders). Due to previous checks this shouldnt happen though.
		}

		private void DrawArrow(Graphics graphics, Point origin, Point endpoint, float length,  Pen arrowPen)
		{
			SizeF sizedVector = new SizeF(origin.X - endpoint.X, origin.Y - endpoint.Y);
			float vectorLength = (float)Math.Sqrt(sizedVector.Width * sizedVector.Width + sizedVector.Height * sizedVector.Height);
			sizedVector = new SizeF(sizedVector.Width * length / vectorLength, sizedVector.Height * length / vectorLength);
			origin = Point.Add(endpoint, sizedVector.ToSize());
			graphics.DrawLine(arrowPen, origin, endpoint);
		}

		private Point IntersectionPoint(Point a, Point b, int c, bool horizontal) //c is x if vertical line, and y if horizontal line
		{
			if (horizontal)
				return new Point(a.X + ((b.X - a.X) * (c - a.Y) / (b.Y - a.Y)), c);
			else
				return new Point(c, a.Y + ((b.Y - a.Y) * (c - a.X) / (b.X - a.X)));
		}
	}
}
