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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.NameBox = new System.Windows.Forms.Label();
			this.RateBox = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.NameBox, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.RateBox, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(119, 42);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// NameBox
			// 
			this.NameBox.AutoSize = true;
			this.NameBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NameBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NameBox.Location = new System.Drawing.Point(3, 0);
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
			this.RateBox.Location = new System.Drawing.Point(3, 17);
			this.RateBox.Name = "RateBox";
			this.RateBox.Size = new System.Drawing.Size(113, 25);
			this.RateBox.TabIndex = 1;
			this.RateBox.Text = "Empty label";
			this.RateBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ProductionNodeViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tableLayoutPanel1);
			this.DoubleBuffered = true;
			this.Name = "ProductionNodeViewer";
			this.Size = new System.Drawing.Size(119, 42);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label RateBox;
		private System.Windows.Forms.Label NameBox;

	}
}
