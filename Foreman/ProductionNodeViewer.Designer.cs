namespace Foreman
{
	partial class ProductionNodeViewer
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
			this.layoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.NameBox = new System.Windows.Forms.Label();
			this.RateBox = new System.Windows.Forms.Label();
			this.OutputIconSizePanel = new System.Windows.Forms.Panel();
			this.InputIconSizePanel = new System.Windows.Forms.Panel();
			this.layoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// layoutPanel
			// 
			this.layoutPanel.AutoSize = true;
			this.layoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.layoutPanel.ColumnCount = 1;
			this.layoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.layoutPanel.Controls.Add(this.InputIconSizePanel, 0, 3);
			this.layoutPanel.Controls.Add(this.NameBox, 0, 1);
			this.layoutPanel.Controls.Add(this.RateBox, 0, 2);
			this.layoutPanel.Controls.Add(this.OutputIconSizePanel, 0, 0);
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Location = new System.Drawing.Point(0, 0);
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.RowCount = 4;
			this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.layoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.layoutPanel.Size = new System.Drawing.Size(119, 184);
			this.layoutPanel.TabIndex = 0;
			this.layoutPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
			// 
			// NameBox
			// 
			this.NameBox.AutoSize = true;
			this.NameBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NameBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NameBox.Location = new System.Drawing.Point(3, 106);
			this.NameBox.Name = "NameBox";
			this.NameBox.Size = new System.Drawing.Size(113, 17);
			this.NameBox.TabIndex = 2;
			this.NameBox.Text = "Empty label";
			this.NameBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// RateBox
			// 
			this.RateBox.AutoSize = true;
			this.RateBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RateBox.Location = new System.Drawing.Point(3, 123);
			this.RateBox.Name = "RateBox";
			this.RateBox.Size = new System.Drawing.Size(113, 25);
			this.RateBox.TabIndex = 1;
			this.RateBox.Text = "Empty label";
			this.RateBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// OutputIconPanel
			// 
			this.OutputIconSizePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutputIconSizePanel.Location = new System.Drawing.Point(3, 3);
			this.OutputIconSizePanel.Name = "OutputIconPanel";
			this.OutputIconSizePanel.Size = new System.Drawing.Size(113, 100);
			this.OutputIconSizePanel.TabIndex = 3;
			// 
			// InputIconSizePanel
			// 
			this.InputIconSizePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InputIconSizePanel.Location = new System.Drawing.Point(3, 151);
			this.InputIconSizePanel.Name = "InputIconSizePanel";
			this.InputIconSizePanel.Size = new System.Drawing.Size(113, 30);
			this.InputIconSizePanel.TabIndex = 4;
			// 
			// ProductionNodeViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.layoutPanel);
			this.DoubleBuffered = true;
			this.Name = "ProductionNodeViewer";
			this.Size = new System.Drawing.Size(119, 184);
			this.layoutPanel.ResumeLayout(false);
			this.layoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel layoutPanel;
		private System.Windows.Forms.Label RateBox;
		private System.Windows.Forms.Label NameBox;
		private System.Windows.Forms.Panel InputIconSizePanel;
		private System.Windows.Forms.Panel OutputIconSizePanel;

	}
}
