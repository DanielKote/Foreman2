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
            if(string.IsNullOrEmpty(displayedString))
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

        private Recipe displayedRecipe;
        private Recipe comparedRecipe; //if given, we will display both displayed and compared as a 'VS' display

        public RecipeToolTip()
        {
            this.AutoPopDelay = 100000;
            this.InitialDelay = 100000;
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
            return new Size(SectionWidth + (comparedRecipe == null ? 0 : SectionWidth + 10), Math.Max(GetRecipeToolTipHeight(displayedRecipe), GetRecipeToolTipHeight(comparedRecipe)));
        }

        private void OnPopup(object sender, PopupEventArgs e)
        {
            if(displayedRecipe == null)
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
                g.FillRectangle(BackgroundBrush, e.Bounds);

                Recipe[] recipes = (comparedRecipe == null) ? new Recipe[] { displayedRecipe } : new Recipe[] { displayedRecipe, comparedRecipe };
                int ingredientCount = Math.Max(displayedRecipe.IngredientList.Count, comparedRecipe == null ? 0 : comparedRecipe.IngredientList.Count);
                int productCount = Math.Max(displayedRecipe.ProductList.Count, comparedRecipe == null ? 0 : comparedRecipe.ProductList.Count);

                int xOffset = 0;
                foreach (Recipe recipe in recipes)
                {
                    //Title
                    int yOffset = 0;
                    g.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 40));
                    g.DrawImage(recipe.Icon, 4 + xOffset, 4 + yOffset, 32, 32);
                    Font recipeFont = (g.MeasureString(recipe.FriendlyName, RecipeFont).Width > SectionWidth - 50) ? SmallRecipeFont : RecipeFont;
                    g.DrawString(recipe.FriendlyName, recipeFont, TextBrush, new Point(42 + xOffset, 12 + yOffset));

                    //Ingredient list:
                    yOffset += 44;
                    g.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 20));
                    yOffset += 2;
                    g.DrawString("Ingredients:", SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);
                    yOffset += 20;
                    for(int i=0; i<ingredientCount; i++)
                    {
                        if (i < recipe.IngredientList.Count)
                        {
                            Item ingredient = recipe.IngredientList[i];
                            string name = recipe.GetIngredientFriendlyName(ingredient);
                            g.DrawImage(ingredient.Icon, 14 + xOffset, 4 + yOffset, 32, 32);
                            Font itemFont = (g.MeasureString(name, RecipeFont).Width > SectionWidth - 50) ? SmallItemFont : ItemFont;
                            g.DrawString(name, itemFont, TextBrush, new Point(52 + xOffset, 2 + yOffset));
                            g.DrawString(recipe.IngredientSet[ingredient].ToString("0.##") + "x", QuantityFont, TextBrush, new Point(52 + xOffset, 20 + yOffset));
                        }
                        yOffset += 40;
                    }

                    //Products list:
                    g.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 20));
                    yOffset += 2;
                    g.DrawString("Products:", SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);
                    yOffset += 20;
                    for (int i = 0; i < productCount; i++)
                    {
                        if (i < recipe.ProductList.Count)
                        {
                            Item product = recipe.ProductList[i];
                            string name = recipe.GetProductFriendlyName(product);
                            g.DrawImage(product.Icon, 14 + xOffset, 4 + yOffset, 32, 32);
                            Font itemFont = (g.MeasureString(name, RecipeFont).Width > SectionWidth - 50) ? SmallItemFont : ItemFont;
                            g.DrawString(name, itemFont, TextBrush, new Point(52 + xOffset, 2 + yOffset));
                            g.DrawString(recipe.ProductSet[product].ToString("0.##") + "x", QuantityFont, TextBrush, new Point(52 + xOffset, 20 + yOffset));
                        }
                        yOffset += 40;
                    }

                    //time
                    g.FillRectangle(DarkBackgroundBrush, new Rectangle(xOffset, yOffset, SectionWidth, 22));
                    yOffset += 2;
                    g.DrawString("Crafting Time: " + recipe.Time.ToString("0.##") + " s", SectionFont, TextBrush, 4 + xOffset, 0 + yOffset);

                    //breaker
                    g.DrawLine(BreakerPen, SectionWidth + 5, 0, SectionWidth + 5, e.Bounds.Height);
                    xOffset += SectionWidth+10;
                }

                g.DrawRectangle(BorderPen, e.Bounds);
            }
        }

        public static int GetRecipeToolTipHeight(Recipe recipe)
        {
            if (recipe == null)
                return 110;
            return 110 + recipe.IngredientList.Count * 40 + recipe.ProductList.Count * 40;
        }
    }
}
