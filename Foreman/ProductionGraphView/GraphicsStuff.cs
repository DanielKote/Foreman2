using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Foreman
{
	public static class GraphicsStuff
	{
		public static void DrawRoundRect(int x, int y, int width, int height, int radius, Graphics graphics, Pen pen)
		{
			int radius2 = radius * 2;
			int Left = x;
			int Top = y;
			int Bottom = y + height;
			int Right = x + width;

			using (GraphicsPath path = new GraphicsPath())
			{
				path.StartFigure();

				path.AddArc(Left, Top, 2 * radius, 2 * radius, 180, 90f);
				path.AddArc(Right - radius2, Top, radius2, radius2, 270f, 90f);
				path.AddArc(Right - radius2, Bottom - radius2, radius2, radius2, 0f, 90f);
				path.AddArc(Left, Bottom - radius2, radius2, radius2, 90f, 90f);

				path.CloseFigure();

				graphics.DrawPath(pen, path);
			}
		}

		public static void FillRoundRect(int x, int y, int width, int height, int radius, Graphics graphics, Brush brush)
		{
			int radius2 = radius * 2;
			int Left = x;
			int Top = y;
			int Bottom = y + height;
			int Right = x + width;

			using (GraphicsPath path = new GraphicsPath())
			{
				path.StartFigure();

				path.AddArc(Left, Top, 2 * radius, 2 * radius, 180, 90f);
				path.AddArc(Right - radius2, Top, radius2, radius2, 270f, 90f);
				path.AddArc(Right - radius2, Bottom - radius2, radius2, radius2, 0f, 90f);
				path.AddArc(Left, Bottom - radius2, radius2, radius2, 90f, 90f);

				path.CloseFigure();

				graphics.FillPath(brush, path);
			}
		}
	}
}
