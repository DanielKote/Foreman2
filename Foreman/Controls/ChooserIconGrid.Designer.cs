using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    partial class ChooserIconGrid {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent() {
            gridSurface = new Panel();
            scrollBar = new VScrollBar();
            SuspendLayout();
            //
            // gridSurface
            //
            gridSurface.BackColor = Color.DimGray;
            gridSurface.Location = new Point(0, 0);
            gridSurface.Margin = new Padding(0);
            gridSurface.Name = "gridSurface";
            gridSurface.Size = new Size(400, 320);
            gridSurface.TabIndex = 0;
            //
            // scrollBar
            //
            scrollBar.BackColor = Color.DimGray;
            scrollBar.LargeChange = VisibleRowCount;
            scrollBar.Location = new Point(400, 0);
            scrollBar.Margin = new Padding(0);
            scrollBar.Maximum = 0;
            scrollBar.Minimum = 0;
            scrollBar.Name = "scrollBar";
            scrollBar.Size = new Size(17, 320);
            scrollBar.SmallChange = 1;
            scrollBar.TabIndex = 1;
            //
            // ChooserIconGrid
            //
            BackColor = Color.DimGray;
            DoubleBuffered = true;
            Controls.Add(gridSurface);
            Controls.Add(scrollBar);
            Margin = new Padding(0);
            Name = "ChooserIconGrid";
            Size = new Size(417, 320);
            ResumeLayout(false);
        }

        #endregion

        private Panel gridSurface;
        private VScrollBar scrollBar;
    }
}
