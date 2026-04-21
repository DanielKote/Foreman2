using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    // helper class to draw the recipe in a panel (container)
    public class RecipePanel : UserControl {
        private Recipe[] _recipes;

        public RecipePanel(Recipe[] recipes) {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;

            BackColor = Color.Black;

            _recipes = recipes;
            Size = RecipePainter.GetSize(_recipes);
            Location = new Point(0, 0);
        }

        protected override void OnPaint(PaintEventArgs e) {
            RecipePainter.Paint(_recipes, e.Graphics, new Point(0, 0));
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }
    }
}