namespace Foreman
{
	partial class RecipeChooserForm
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
			this.recipeListPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.SuspendLayout();
			// 
			// recipeListPanel
			// 
			this.recipeListPanel.AutoSize = true;
			this.recipeListPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.recipeListPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(103)))), ((int)(((byte)(134)))), ((int)(((byte)(158)))));
			this.recipeListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.recipeListPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.recipeListPanel.Location = new System.Drawing.Point(0, 0);
			this.recipeListPanel.Name = "recipeListPanel";
			this.recipeListPanel.Size = new System.Drawing.Size(320, 315);
			this.recipeListPanel.TabIndex = 0;
			this.recipeListPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecipeChooserForm_MouseMove);
			// 
			// RecipeChooserForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Gray;
			this.ClientSize = new System.Drawing.Size(320, 315);
			this.Controls.Add(this.recipeListPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.Name = "RecipeChooserForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "RecipeChooserForm";
			this.Load += new System.EventHandler(this.RecipeChooserForm_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RecipeChooserForm_KeyDown);
			this.MouseLeave += new System.EventHandler(this.RecipeChooserForm_MouseLeave);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RecipeChooserForm_MouseMove);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel recipeListPanel;
	}
}