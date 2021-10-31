namespace Foreman
{
	partial class RecipeChooserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.iconPictureBox = new System.Windows.Forms.PictureBox();
			this.RecipeLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.nameLabel = new System.Windows.Forms.Label();
			this.inputListBox = new System.Windows.Forms.ListBox();
			this.outputListBox = new System.Windows.Forms.ListBox();
			((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).BeginInit();
			this.RecipeLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// iconPictureBox
			// 
			this.iconPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.iconPictureBox.Location = new System.Drawing.Point(3, 3);
			this.iconPictureBox.Name = "iconPictureBox";
			this.RecipeLayoutPanel.SetRowSpan(this.iconPictureBox, 2);
			this.iconPictureBox.Size = new System.Drawing.Size(10, 10);
			this.iconPictureBox.TabIndex = 0;
			this.iconPictureBox.TabStop = false;
			// 
			// tableLayoutPanel1
			// 
			this.RecipeLayoutPanel.AutoSize = true;
			this.RecipeLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.RecipeLayoutPanel.ColumnCount = 3;
			this.RecipeLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
			this.RecipeLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.RecipeLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.RecipeLayoutPanel.Controls.Add(this.iconPictureBox, 0, 0);
			this.RecipeLayoutPanel.Controls.Add(this.nameLabel, 1, 0);
			this.RecipeLayoutPanel.Controls.Add(this.inputListBox, 1, 1);
			this.RecipeLayoutPanel.Controls.Add(this.outputListBox, 2, 1);
			this.RecipeLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipeLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.RecipeLayoutPanel.Name = "tableLayoutPanel1";
			this.RecipeLayoutPanel.RowCount = 2;
			this.RecipeLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.RecipeLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.RecipeLayoutPanel.TabIndex = 1;
			// 
			// nameLabel
			// 
			this.nameLabel.AutoSize = true;
			this.RecipeLayoutPanel.SetColumnSpan(this.nameLabel, 2);
			this.nameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nameLabel.Location = new System.Drawing.Point(39, 0);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.TabIndex = 1;
			this.nameLabel.Text = "Recipe name:";
			this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// inputListBox
			// 
			this.inputListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.inputListBox.FormattingEnabled = true;
			this.inputListBox.Location = new System.Drawing.Point(39, 16);
			this.inputListBox.Name = "inputListBox";
			this.inputListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.inputListBox.TabIndex = 2;
			this.inputListBox.Enabled = false;
			// 
			// outputListBox
			// 
			this.outputListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outputListBox.FormattingEnabled = true;
			this.outputListBox.Location = new System.Drawing.Point(171, 16);
			this.outputListBox.Name = "outputListBox";
			this.outputListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.outputListBox.TabIndex = 3;
			this.outputListBox.Enabled = false;
			// 
			// RecipeChooserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.RecipeLayoutPanel);
			this.DoubleBuffered = true;
			this.Name = "RecipeChooserControl";
			this.Load += new System.EventHandler(this.RecipeChooserOption_Load);
			((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).EndInit();
			this.RecipeLayoutPanel.ResumeLayout(false);
			this.RecipeLayoutPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox iconPictureBox;
		private System.Windows.Forms.TableLayoutPanel RecipeLayoutPanel;
		private System.Windows.Forms.Label nameLabel;
		private System.Windows.Forms.ListBox inputListBox;
		private System.Windows.Forms.ListBox outputListBox;
	}
}
