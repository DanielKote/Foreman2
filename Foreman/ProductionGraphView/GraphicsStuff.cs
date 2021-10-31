using System;
using System.Collections.Generic;
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

		public static void FillRoundRectTLFlag(int x, int y, int width, int height, int radius, Graphics graphics, Brush brush)
		{
			int radius2 = radius * 2;
			int Left = x;
			int Top = y;
			int Bottom = y + height;
			int Right = x + width;

			using (GraphicsPath path = new GraphicsPath())
			{
				path.StartFigure();

				path.AddArc(Left, Top, 2 * radius, 2 * radius, 180f, 90f);
				path.AddLine(Right, Top, Left, Bottom);

				path.CloseFigure();

				graphics.FillPath(brush, path);
			}
		}

		public static string FloatToString(float value)
		{
			string str;
			if (value >= 100000)
				str = value.ToString("0.00e0");
			else if (value >= 10000)
				str = value.ToString("0");
			else if (value >= 100)
				str = value.ToString("0.#");
			else if (value >= 10)
				str = value.ToString("0.##");
			else if (value >= 0.1)
				str = value.ToString("0.###");
			else if (value != 0)
				str = value.ToString("0.######");
			else
				str = "0";
			return str;
		}
	}

	public static class RecipePainter //As I found out the painted recipe look is rather nice. And when I needed to display it outside the tooltip, well... it moved to its own class instead of me copy-pasting the code.
	{
		private static readonly Brush BackgroundBrush = new SolidBrush(Color.FromArgb(65, 65, 65));
		private static readonly Brush DarkBackgroundBrush = new SolidBrush(Color.FromArgb(255, 40, 40, 40));
		private static readonly Brush TextBrush = new SolidBrush(Color.White);
		private static readonly Pen BorderPen = new Pen(new SolidBrush(Color.Black), 2);
		private static readonly Pen BreakerPen = new Pen(new SolidBrush(Color.Black), 10);
		private static readonly Font QuantityFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
		private static readonly Font SectionFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
		private static readonly Pen devPen = new Pen(Brushes.Orange, 1);

		private static readonly Font RecipeFont = new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Bold);
		private static readonly Font ItemFont = new Font(FontFamily.GenericSansSerif, 7.8f);
		private static readonly StringFormat TextFormat = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };

		private const int SectionWidth = 200;

		public static Size GetSize(ICollection<Recipe> recipes)
		{
			int height = 0;
			int width = ((SectionWidth + 10) * recipes.Count) - 10;
			foreach (Recipe recipe in recipes)
				height = Math.Max(height, 110 + recipe.IngredientList.Count * 40 + recipe.ProductList.Count * 40);

			return new Size(width, height);
		}

		public static void Paint(IList<Recipe> recipes, Graphics graphics, Point offset)
		{
			Rectangle boundary = new Rectangle(offset, GetSize(recipes));
			graphics.FillRectangle(BackgroundBrush, boundary);

			int maxIngredientCount = 0;
			int maxProductCount = 0;
			foreach (Recipe recipe in recipes)
			{
				maxIngredientCount = Math.Max(maxIngredientCount, recipe.IngredientList.Count);
				maxProductCount = Math.Max(maxProductCount, recipe.ProductList.Count);
			}

			Rectangle textbox;
			int xOffset = boundary.X;
			for (int r = 0; r < recipes.Count; r++)
			{
				//Title
				int yOffset = boundary.Y;
				graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 40));
				graphics.DrawImage(recipes[r].Icon, 4 + xOffset, 4 + yOffset, 32, 32);

				textbox = new Rectangle(xOffset + 42, yOffset + 4, SectionWidth - 48, 32);
				//graphics.DrawRectangle(devPen, textbox);
				GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, recipes[r].FriendlyName, RecipeFont, textbox);

				//Ingredient list:
				yOffset += 44;
				graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 20));
				yOffset += 2;
				graphics.DrawString("Ingredients:", SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);
				yOffset += 20;
				for (int i = 0; i < maxIngredientCount; i++)
				{
					if (i < recipes[r].IngredientList.Count)
					{
						Item ingredient = recipes[r].IngredientList[i];
						graphics.DrawImage(ingredient.Icon, 14 + xOffset, 4 + yOffset, 32, 32);

						textbox = new Rectangle(xOffset + 52, yOffset + 2, SectionWidth - 58, 20);
						//graphics.DrawRectangle(devPen, textbox);
						graphics.DrawString(recipes[r].IngredientSet[ingredient].ToString("0.##") + "x", QuantityFont, TextBrush, new Point(52 + xOffset, 20 + yOffset));
						GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, recipes[r].GetIngredientFriendlyName(ingredient), ItemFont, textbox);
					}
					yOffset += 40;
				}

				//Products list:
				graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 20));
				yOffset += 2;
				graphics.DrawString("Products:", SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);
				yOffset += 20;
				for (int i = 0; i < maxProductCount; i++)
				{
					if (i < recipes[r].ProductList.Count)
					{
						Item product = recipes[r].ProductList[i];
						graphics.DrawImage(product.Icon, 14 + xOffset, 4 + yOffset, 32, 32);

						textbox = new Rectangle(xOffset + 52, yOffset + 2, SectionWidth - 58, 20);
						//graphics.DrawRectangle(devPen, textbox);
						graphics.DrawString(recipes[r].ProductSet[product].ToString("0.##") + "x", QuantityFont, TextBrush, new Point(52 + xOffset, 20 + yOffset));
						GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, recipes[r].GetProductFriendlyName(product), ItemFont, textbox);
					}
					yOffset += 40;
				}

				//time
				graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 22));
				yOffset += 2;
				graphics.DrawString("Crafting Time: " + recipes[r].Time.ToString("0.##") + " s", SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);

				//breaker
				if (r < recipes.Count - 1)
				{
					graphics.DrawLine(BreakerPen, xOffset + SectionWidth + 5, boundary.Y, xOffset + SectionWidth + 5, boundary.Y + boundary.Height);
					xOffset += SectionWidth + 10;
				}
			}

			graphics.DrawRectangle(BorderPen, boundary);
		}
	}

}
