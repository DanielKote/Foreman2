using System.Drawing;
using System.Windows.Forms;

namespace Foreman
{
    public class IGToolTip : ToolTip
    {
        private static readonly Color BackgroundColor = Color.FromArgb(65, 65, 65);

        public IGToolTip()
        {
            this.AutoPopDelay = 100000;
            this.InitialDelay = 100000;
            this.ReshowDelay = 100000;

            this.OwnerDraw = true;
            this.BackColor = BackgroundColor;
            this.ForeColor = Color.White;
            this.Draw += new DrawToolTipEventHandler(IGTooltip_Draw);
        }

        private void IGTooltip_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.Black), 2), e.Bounds);
            e.DrawText();
        }

    }

    public class RecipeToolTip : ToolTip
    {
        private static readonly Brush BackgroundBrush = new SolidBrush(Color.FromArgb(65, 65, 65));
        private static readonly Brush DarkBackgroundBrush = new SolidBrush(Color.FromArgb(255, 40, 40, 40));
        private static readonly Brush TextBrush = new SolidBrush(Color.White);
        private static readonly Pen BorderPen = new Pen(new SolidBrush(Color.Black), 2);
        private static readonly Font RecipeFont = new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Bold);
        private static readonly Font SmallRecipeFont = new Font(FontFamily.GenericSansSerif, 6f, FontStyle.Bold);
        private static readonly Font ItemFont = new Font(FontFamily.GenericSansSerif, 7.8f);
        private static readonly Font SmallItemFont = new Font(FontFamily.GenericSansSerif, 6f);
        private static readonly Font QuantityFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
        private static readonly Font SectionFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);

        public RecipeToolTip()
        {
            this.AutoPopDelay = 100000;
            this.InitialDelay = 100000;
            this.ReshowDelay = 100000;

            this.OwnerDraw = true;
            this.BackColor = Color.DimGray;
            this.ForeColor = Color.White;
            this.Popup += new PopupEventHandler(this.OnPopup);
            this.Draw += new DrawToolTipEventHandler(this.OnDraw);
        }

        private void OnPopup(object sender, PopupEventArgs e)
        {
            Recipe recipe = (Recipe)((Button)e.AssociatedControl).Tag;
            e.ToolTipSize = new Size(250, GetRecipeToolTipHeight(recipe));

        }

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            using (Graphics g = e.Graphics)
            {
                Recipe recipe = (Recipe)((Button)e.AssociatedControl).Tag;

                g.FillRectangle(BackgroundBrush, e.Bounds);

                //Title
                int yOffset = 0;
                g.FillRectangle(DarkBackgroundBrush, new Rectangle(0, yOffset, e.Bounds.Width, 40));
                g.DrawImage(recipe.Icon, 4, 4 + yOffset, 32, 32);
                Font recipeFont = (g.MeasureString(recipe.FriendlyName, RecipeFont).Width > e.Bounds.Width - 50) ? SmallRecipeFont : RecipeFont;
                g.DrawString(recipe.FriendlyName, recipeFont, TextBrush, new Point(42, 12 + yOffset));

                //Ingredient list:
                yOffset += 44;
                g.FillRectangle(DarkBackgroundBrush, new Rectangle(0, yOffset, e.Bounds.Width, 20));
                yOffset += 2;
                g.DrawString("Ingredients:", SectionFont, TextBrush, 4, 0 + yOffset);
                yOffset += 20;
                foreach (Item ingredient in recipe.IngredientList)
                {
                    string name = recipe.GetIngredientFriendlyName(ingredient);
                    g.DrawImage(ingredient.Icon, 14, 4 + yOffset, 32, 32);
                    Font itemFont = (g.MeasureString(name, RecipeFont).Width > e.Bounds.Width - 50) ? SmallItemFont : ItemFont;
                    g.DrawString(name, itemFont, TextBrush, new Point(52, 2 + yOffset));
                    g.DrawString(recipe.IngredientSet[ingredient].ToString("0.##") + "x", QuantityFont, TextBrush, new Point(52, 20 + yOffset));
                    yOffset += 40;
                }

                //Products list:
                g.FillRectangle(DarkBackgroundBrush, new Rectangle(0, yOffset, e.Bounds.Width, 20));
                yOffset += 2;
                g.DrawString("Products:", SectionFont, TextBrush, 4, 0 + yOffset);
                yOffset += 20;
                foreach (Item product in recipe.ProductList)
                {
                    string name = recipe.GetProductFriendlyName(product);

                    g.DrawImage(product.Icon, 14, 4 + yOffset, 32, 32);
                    Font itemFont = (g.MeasureString(name, RecipeFont).Width > e.Bounds.Width - 50) ? SmallItemFont : ItemFont;
                    g.DrawString(name, itemFont, TextBrush, new Point(52, 2 + yOffset));
                    g.DrawString(recipe.ProductSet[product].ToString("0.##") + "x", QuantityFont, TextBrush, new Point(52, 20 + yOffset));
                    yOffset += 40;
                }

                //time
                g.FillRectangle(DarkBackgroundBrush, new Rectangle(0, yOffset, e.Bounds.Width, 22));
                yOffset += 2;
                g.DrawString("Crafting Time: " + recipe.Time.ToString("0.##") + " s", SectionFont, TextBrush, 4, 0 + yOffset);

                g.DrawRectangle(BorderPen, e.Bounds);
            }
        }

        public static int GetRecipeToolTipHeight(Recipe recipe)
        {
            return 110 + recipe.IngredientList.Count * 40 + recipe.ProductList.Count * 40;
        }
    }
}
