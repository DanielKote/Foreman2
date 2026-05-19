using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman.Controls {
    internal class CustomProgressBar : ProgressBar {
        //Property to hold the custom text
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? CustomText { get; set; }

        public CustomProgressBar() : base() {
            // Modify the ControlStyles flags
            //http://msdn.microsoft.com/en-us/library/system.windows.forms.controlstyles.aspx
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e) {
            Rectangle rect = ClientRectangle;
            Graphics g = e.Graphics;

            ProgressBarRenderer.DrawHorizontalBar(g, rect);
            rect.Inflate(-3, -3);
            if (Value > 0) {
                // As we doing this ourselves we need to draw the chunks on the progress bar
                var clip = new Rectangle(rect.X, rect.Y, (int)Math.Round(((float)Value / Maximum) * rect.Width), rect.Height);
                ProgressBarRenderer.DrawHorizontalChunks(g, clip);
            }

            // Set the Display text (Either a % amount or our custom text
            int percent = (int)(((double)this.Value / (double)this.Maximum) * 100);
            string text = "(" + percent.ToString(DisplayCulture.Format) + "%) " + CustomText;

            using var f = new Font(FontFamily.GenericSerif, 10);

            SizeF len = g.MeasureString(text, f);
            var location = new Point(Convert.ToInt32((Width / 2) - len.Width / 2), Convert.ToInt32((Height / 2) - len.Height / 2));
            g.DrawString(text, f, Brushes.Black, location);
        }
    }
}
