using System;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman
{
	public static class RecipePainter //As I found out the painted recipe look is rather nice. And when I needed to display it outside the tooltip, well... it moved to its own class instead of me copy-pasting the code.
	{
		private static readonly Brush BackgroundBrush = new SolidBrush(Color.FromArgb(65, 65, 65));
		private static readonly Brush DarkBackgroundBrush = new SolidBrush(Color.FromArgb(255, 40, 40, 40));
		private static readonly Brush TextBrush = new SolidBrush(Color.White);
		private static readonly Pen BorderPen = new Pen(new SolidBrush(Color.Black), 2);
		private static readonly Pen BreakerPen = new Pen(new SolidBrush(Color.Black), 10);
		private static readonly Font RecipeFont = new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Bold);
		private static readonly Font SmallRecipeFont = new Font(FontFamily.GenericSansSerif, 6f, FontStyle.Bold);
		private static readonly Font ItemFont = new Font(FontFamily.GenericSansSerif, 7.8f);
		private static readonly Font SmallItemFont = new Font(FontFamily.GenericSansSerif, 6f);
		private static readonly Font QuantityFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
		private static readonly Font SectionFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);

		private const int SectionWidth = 250;

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
			foreach(Recipe recipe in recipes)
			{
				maxIngredientCount = Math.Max(maxIngredientCount, recipe.IngredientList.Count);
				maxProductCount = Math.Max(maxProductCount, recipe.ProductList.Count);
			}

			int xOffset = boundary.X;
			for (int r = 0; r < recipes.Count; r++)
			{
				//Title
				int yOffset = boundary.Y;
				graphics.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 40));
				graphics.DrawImage(recipes[r].Icon, 4 + xOffset, 4 + yOffset, 32, 32);
				Font recipeFont = (graphics.MeasureString(recipes[r].FriendlyName, RecipeFont).Width > SectionWidth - 50) ? SmallRecipeFont : RecipeFont;
				graphics.DrawString(recipes[r].FriendlyName, recipeFont, TextBrush, new Point(42 + xOffset, 12 + yOffset));

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
						string name = recipes[r].GetIngredientFriendlyName(ingredient);
						graphics.DrawImage(ingredient.Icon, 14 + xOffset, 4 + yOffset, 32, 32);
						Font itemFont = (graphics.MeasureString(name, RecipeFont).Width > SectionWidth - 50) ? SmallItemFont : ItemFont;
						graphics.DrawString(name, itemFont, TextBrush, new Point(52 + xOffset, 2 + yOffset));
						graphics.DrawString(recipes[r].IngredientSet[ingredient].ToString("0.##") + "x", QuantityFont, TextBrush, new Point(52 + xOffset, 20 + yOffset));
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
						string name = recipes[r].GetProductFriendlyName(product);
						graphics.DrawImage(product.Icon, 14 + xOffset, 4 + yOffset, 32, 32);
						Font itemFont = (graphics.MeasureString(name, RecipeFont).Width > SectionWidth - 50) ? SmallItemFont : ItemFont;
						graphics.DrawString(name, itemFont, TextBrush, new Point(52 + xOffset, 2 + yOffset));
						graphics.DrawString(recipes[r].ProductSet[product].ToString("0.##") + "x", QuantityFont, TextBrush, new Point(52 + xOffset, 20 + yOffset));
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
