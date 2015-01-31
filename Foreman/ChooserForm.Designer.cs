namespace Foreman
{
	partial class ChooserForm
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
			this.listPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.SuspendLayout();
			// 
			// recipeListPanel
			// 
			this.listPanel.AutoScroll = true;
			this.listPanel.AutoSize = true;
			this.listPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.listPanel.BackColor = System.Drawing.SystemColors.Control;
			this.listPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.listPanel.Location = new System.Drawing.Point(0, 0);
			this.listPanel.Name = "recipeListPanel";
			this.listPanel.Size = new System.Drawing.Size(324, 38);
			this.listPanel.TabIndex = 0;
			this.listPanel.WrapContents = false;
			this.listPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecipeChooserForm_MouseMove);
			// 
			// RecipeChooserForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(324, 38);
			this.Controls.Add(this.listPanel);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RecipeChooserForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Choose a node to create";
			this.Load += new System.EventHandler(this.RecipeChooserForm_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RecipeChooserForm_KeyDown);
			this.MouseLeave += new System.EventHandler(this.RecipeChooserForm_MouseLeave);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecipeChooserForm_MouseMove);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel listPanel;
	}
}