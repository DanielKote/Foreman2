using System;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    class CustomProgressBar : ProgressBar {
        // Property to hold the custom text
        public string CustomText { get; set; }

        // Modify the ControlStyles flags
        // http://msdn.microsoft.com/en-us/library/system.windows.forms.controlstyles.aspx
        public CustomProgressBar() {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e) {
            var rect = ClientRectangle;
            var g = e.Graphics;

            ProgressBarRenderer.DrawHorizontalBar(g, rect);
            rect.Inflate(-3, -3);

            // As we're doing this ourselves we need to draw the chunks on the progress bar
            if (Value > 0) {
                var clip = new Rectangle(rect.X, rect.Y, (int) Math.Round((float) Value / Maximum * rect.Width), rect.Height);
                ProgressBarRenderer.DrawHorizontalChunks(g, clip);
            }

            // Set the Display text (Either a % amount or our custom text)

            var percent = (int) (Value / (double) Maximum * 100);
            var text = "(" + percent + "%) " + CustomText;

            using (var f = new Font(FontFamily.GenericSerif, 10)) {
                var len = g.MeasureString(text, f);
                var location = new Point(Convert.ToInt32(Width / 2 - len.Width / 2), Convert.ToInt32(Height / 2 - len.Height / 2));
                g.DrawString(text, f, Brushes.Black, location);
            }
        }
    }
}