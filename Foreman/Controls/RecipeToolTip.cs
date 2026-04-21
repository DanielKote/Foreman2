using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    public class CustomToolTip : ToolTip {
        private static readonly Color BackgroundColor = Color.FromArgb(65, 65, 65);
        private static readonly Pen BorderPen = new(new SolidBrush(Color.Black), 2);
        private static readonly Pen BreakerPen = new(new SolidBrush(Color.Black), 10);
        private static readonly Brush TextBrush = new SolidBrush(Color.White);

        private string _displayedString;
        private string _comparedString;

        [AmbientValue(typeof(Font), null)] public Font TextFont { get; set; }

        public CustomToolTip() {
            AutoPopDelay = 100000;
            // we will be manually showing this - so we don't want the auto-show to happen.
            InitialDelay = 100000;
            ReshowDelay = 100000;
            TextFont = new Font(FontFamily.GenericSansSerif, 7.8f, FontStyle.Regular);

            OwnerDraw = true;
            BackColor = BackgroundColor;
            ForeColor = Color.White;
            Popup += OnPopup;
            Draw += IGTooltip_Draw;
        }

        public void Show(IWin32Window window, Point location) {
            Show("-", window, location);
        }

        public void SetText(string text, string comparedText = "") {
            _displayedString = text;
            _comparedString = comparedText;
        }

        public Size GetExpectedSize() {
            var measuredText = TextRenderer.MeasureText(_displayedString, TextFont);
            var comparedMeasuredText = TextRenderer.MeasureText(_comparedString, TextFont);
            return new Size(measuredText.Width + 4 + (string.IsNullOrEmpty(_comparedString) ? 0 : comparedMeasuredText.Width + 18),
                Math.Max(measuredText.Height + 4, comparedMeasuredText.Height + 4));
        }

        private void OnPopup(object sender, PopupEventArgs e) {
            if (string.IsNullOrEmpty(_displayedString)) {
                e.Cancel = true;
                return;
            }

            e.ToolTipSize = GetExpectedSize();
        }

        private void IGTooltip_Draw(object sender, DrawToolTipEventArgs e) {
            e.DrawBackground();
            e.Graphics.DrawRectangle(BorderPen, e.Bounds);

            e.Graphics.DrawString(_displayedString, TextFont, TextBrush, new Point(2, 2));
            if (!string.IsNullOrEmpty(_comparedString)) {
                var breakpoint = TextRenderer.MeasureText(_displayedString, TextFont).Width + 9;
                e.Graphics.DrawLine(BreakerPen, breakpoint, 0, breakpoint, e.Bounds.Height);
                e.Graphics.DrawString(_comparedString, TextFont, TextBrush, new Point(9 + breakpoint, 2));
            }
        }
    }

    public class RecipeToolTip : ToolTip {
        private Recipe _displayedRecipe;
        // if given, we will display both displayed and compared as a 'VS' display
        private Recipe _comparedRecipe;

        public RecipeToolTip() {
            AutoPopDelay = 100000;
            // we will be manually showing this - so we don't want the auto-show to happen.
            InitialDelay = 100000;
            ReshowDelay = 100000;

            OwnerDraw = true;
            BackColor = Color.DimGray;
            ForeColor = Color.White;
            Popup += OnPopup;
            Draw += OnDraw;
        }

        public void Show(IWin32Window window, Point location) {
            Show("-", window, location);
        }

        public void SetRecipe(Recipe recipe, Recipe comparedRecipe = null) {
            _displayedRecipe = recipe;
            _comparedRecipe = comparedRecipe;
        }

        public Size GetExpectedSize() {
            var recipes = _comparedRecipe == null ? new[] { _displayedRecipe } : new[] { _displayedRecipe, _comparedRecipe };
            return RecipePainter.GetSize(recipes);
        }

        private void OnPopup(object sender, PopupEventArgs e) {
            if (_displayedRecipe == null) {
                e.Cancel = true;
                return;
            }

            e.ToolTipSize = GetExpectedSize();
        }

        private void OnDraw(object sender, DrawToolTipEventArgs e) {
            using (var g = e.Graphics) {
                var recipes = _comparedRecipe == null ? new[] { _displayedRecipe } : new[] { _displayedRecipe, _comparedRecipe };
                RecipePainter.Paint(recipes, g, new Point(0, 0));
            }
        }

        public static int GetRecipeToolTipHeight(Recipe recipe) {
            return recipe != null ? RecipePainter.GetSize([recipe]).Height : 110;
        }
    }
}