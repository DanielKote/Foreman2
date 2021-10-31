using System.Drawing;
using System.Drawing.Drawing2D;

namespace Foreman
{
	public static class GraphicsStuff
	{
		public static int DrawText(Graphics graphics, Brush textBrush, StringFormat textFormat, string text, Font baseFont, Rectangle textbox, bool singleLine = false) //returns the width of the actually drawn text
		{
			float textLength = 0;
			Font textFont = new Font(baseFont, baseFont.Style);
			if (singleLine)
			{
				textLength = graphics.MeasureString(text, textFont).Width;
				while (textLength > textbox.Width)
				{
					Font newNameFont = new Font(textFont.FontFamily, textFont.Size - 0.5f, textFont.Style);
					textFont.Dispose();
					textFont = newNameFont;
					textLength = graphics.MeasureString(text, textFont).Width;
				}
			}
			else
			{
				SizeF textSize = graphics.MeasureString(text, textFont, textbox.Width);
				while (textSize.Height > textbox.Height)
				{
					Font newNameFont = new Font(textFont.FontFamily, textFont.Size - 0.5f, textFont.Style);
					textFont.Dispose();
					textFont = newNameFont;
					textSize = graphics.MeasureString(text, textFont, textbox.Width);
				}
				textLength = textSize.Width;
			}
			graphics.DrawString(text, textFont, textBrush, textbox, textFormat);
			textFont.Dispose();
			return (int)textLength;
		}


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
