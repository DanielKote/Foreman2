
namespace Foreman
{
	partial class GraphSummaryForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{

            this.GS = new Foreman.Controls.GraphSummary();

            this.SuspendLayout();

            // GS
            // 
            this.GS.AutoSize = true;
            this.GS.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GS.Location = new System.Drawing.Point(3, 3);
            this.GS.Name = "GS";
            this.GS.Size = new System.Drawing.Size(758, 495);
            this.GS.TabIndex = 0;
            // 
            // GraphSummaryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 501);
            this.Controls.Add(this.GS);
            this.DoubleBuffered = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(780, 540);
            this.Name = "GraphSummaryForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Factory summary:";
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private Controls.GraphSummary GS;
    }
}