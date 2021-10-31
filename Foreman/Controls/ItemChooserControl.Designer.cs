namespace Foreman
{
	partial class ItemChooserControl
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
			this.DoubleBuffered = true;
			this.ItemLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.iconPictureBox = new System.Windows.Forms.PictureBox();
			this.TextLabel = new System.Windows.Forms.Label();
			this.ItemLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.ItemLayoutPanel.AutoSize = true;
			this.ItemLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ItemLayoutPanel.ColumnCount = 2;
			this.ItemLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
			this.ItemLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ItemLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.ItemLayoutPanel.Controls.Add(this.iconPictureBox, 0, 0);
			this.ItemLayoutPanel.Controls.Add(this.TextLabel, 1, 0);
			this.ItemLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.ItemLayoutPanel.Name = "tableLayoutPanel1";
			this.ItemLayoutPanel.RowCount = 1;
			this.ItemLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ItemLayoutPanel.TabIndex = 2;
			// 
			// iconPictureBox
			// 
			this.iconPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.iconPictureBox.Location = new System.Drawing.Point(3, 3);
			this.iconPictureBox.Name = "iconPictureBox";
			this.iconPictureBox.Size = new System.Drawing.Size(10, 10);
			this.iconPictureBox.TabIndex = 0;
			this.iconPictureBox.TabStop = false;
			// 
			// TextLabel
			// 
			this.TextLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TextLabel.Location = new System.Drawing.Point(39, 0);
			this.TextLabel.Name = "TextLabel";
			this.TextLabel.TabIndex = 1;
			this.TextLabel.Text = "Use Infinite Supply Node";
			this.TextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// SupplyNodeChooserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.ItemLayoutPanel);
			this.Name = "SupplyNodeChooserControl";
			this.Load += new System.EventHandler(this.RecipeChooserSupplyNodeOption_Load);
			this.ItemLayoutPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel ItemLayoutPanel;
		private System.Windows.Forms.PictureBox iconPictureBox;
		private System.Windows.Forms.Label TextLabel;
	}
}
