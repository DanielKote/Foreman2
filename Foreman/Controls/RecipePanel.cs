using Foreman.DataCaching.DataTypes;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman.Controls {
    public class RecipePanel : UserControl //helper class to draw the recipe in a panel (container)
    {
        private readonly IRecipe[] Recipes;
        public RecipePanel(IRecipe[] recipes) {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;

            this.BackColor = Color.Black;

            Recipes = recipes;
            this.Size = RecipePainter.GetSize(Recipes);
            this.Location = new Point(0, 0);
        }

        protected override void OnPaint(PaintEventArgs e) { RecipePainter.Paint(Recipes, e.Graphics, new Point(0, 0)); }
        protected override void OnPaintBackground(PaintEventArgs e) { }
    }
}
