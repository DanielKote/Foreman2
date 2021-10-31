using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman
{
	public class CustomToolTip : ToolTip
	{
		private static readonly Color BackgroundColor = Color.FromArgb(65, 65, 65);
		private static readonly Pen BorderPen = new Pen(new SolidBrush(Color.Black), 2);
		private static readonly Pen BreakerPen = new Pen(new SolidBrush(Color.Black), 10);
		private static readonly Brush TextBrush = new SolidBrush(Color.White);

		private string displayedString;
		private string comparedString;

		[AmbientValue(typeof(Font), null)]
		public Font TextFont { get; set; }

		public CustomToolTip()
		{
			this.AutoPopDelay = 100000;
			this.InitialDelay = 100000;
			this.ReshowDelay = 100000;
			this.TextFont = new Font(FontFamily.GenericSansSerif, 7.8f, FontStyle.Regular);

			this.OwnerDraw = true;
			this.BackColor = BackgroundColor;
			this.ForeColor = Color.White;
			this.Popup += new PopupEventHandler(OnPopup);
			this.Draw += new DrawToolTipEventHandler(IGTooltip_Draw);
		}

		public void Show(IWin32Window window, Point location) { this.Show("-", window, location); }

		public void SetText(string text, string comparedText = "") { displayedString = text; this.comparedString = comparedText; }

		public Size GetExpectedSize()
		{
			Size measuredText = TextRenderer.MeasureText(displayedString, TextFont);
			Size comparedMeasuredText = TextRenderer.MeasureText(comparedString, TextFont);
			return new Size(measuredText.Width + 4 + (string.IsNullOrEmpty(comparedString) ? 0 : (comparedMeasuredText.Width + 18)), Math.Max(measuredText.Height + 4, comparedMeasuredText.Height + 4));
		}

		private void OnPopup(object sender, PopupEventArgs e)
		{
			if (string.IsNullOrEmpty(displayedString))
			{
				e.Cancel = true;
				return;
			}

			e.ToolTipSize = GetExpectedSize();
		}

		private void IGTooltip_Draw(object sender, DrawToolTipEventArgs e)
		{
			e.DrawBackground();
			e.Graphics.DrawRectangle(BorderPen, e.Bounds);

			e.Graphics.DrawString(displayedString, TextFont, TextBrush, new Point(2, 2));
			if (!string.IsNullOrEmpty(comparedString))
			{
				int breakpoint = TextRenderer.MeasureText(displayedString, TextFont).Width + 9;
				e.Graphics.DrawLine(BreakerPen, breakpoint, 0, breakpoint, e.Bounds.Height);
				e.Graphics.DrawString(comparedString, TextFont, TextBrush, new Point(9 + breakpoint, 2));
			}
		}

	}

	public class RecipeToolTip : ToolTip
	{
		private Recipe displayedRecipe;
		private Recipe comparedRecipe; //if given, we will display both displayed and compared as a 'VS' display

		public RecipeToolTip()
		{
			this.AutoPopDelay = 100000;
			this.InitialDelay = 100;
			this.ReshowDelay = 100000;

			this.OwnerDraw = true;
			this.BackColor = Color.DimGray;
			this.ForeColor = Color.White;
			this.Popup += new PopupEventHandler(OnPopup);
			this.Draw += new DrawToolTipEventHandler(OnDraw);
		}

		public void Show(IWin32Window window, Point location) { this.Show("-", window, location); }

		public void SetRecipe(Recipe recipe, Recipe comparedRecipe = null) { displayedRecipe = recipe; this.comparedRecipe = comparedRecipe; }

		public Size GetExpectedSize()
		{
			Recipe[] recipes = (comparedRecipe == null) ? new Recipe[] { displayedRecipe } : new Recipe[] { displayedRecipe, comparedRecipe };
			return RecipePainter.GetSize(recipes);
		}

		private void OnPopup(object sender, PopupEventArgs e)
		{
			if (displayedRecipe == null)
			{
				e.Cancel = true;
				return;
			}

			e.ToolTipSize = GetExpectedSize();
		}

		private void OnDraw(object sender, DrawToolTipEventArgs e)
		{
			using (Graphics g = e.Graphics)
			{
				Recipe[] recipes = (comparedRecipe == null) ? new Recipe[] { displayedRecipe } : new Recipe[] { displayedRecipe, comparedRecipe };
				RecipePainter.Paint(recipes, g, new Point(0, 0));
			}
		}

		public static int GetRecipeToolTipHeight(Recipe recipe)
		{
			if (recipe == null)
				return 110;
			return RecipePainter.GetSize(new Recipe[] { recipe }).Height;
		}
	}
}
