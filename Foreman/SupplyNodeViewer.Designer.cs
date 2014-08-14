namespace Foreman
{
	partial class SupplyNodeViewer
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
			this.label1 = new System.Windows.Forms.Label();
			this.RateLabel = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.NameBox = new System.Windows.Forms.TextBox();
			this.RateTextBox = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 26);
			this.label1.TabIndex = 0;
			this.label1.Text = "Item Name";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// RateLabel
			// 
			this.RateLabel.AutoSize = true;
			this.RateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateLabel.Location = new System.Drawing.Point(3, 26);
			this.RateLabel.Name = "RateLabel";
			this.RateLabel.Size = new System.Drawing.Size(94, 26);
			this.RateLabel.TabIndex = 7;
			this.RateLabel.Text = "Rate (per Second)";
			this.RateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.NameBox, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.RateLabel, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.RateTextBox, 1, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(226, 52);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// NameBox
			// 
			this.NameBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NameBox.Location = new System.Drawing.Point(103, 3);
			this.NameBox.Name = "NameBox";
			this.NameBox.Size = new System.Drawing.Size(120, 20);
			this.NameBox.TabIndex = 3;
			// 
			// RateTextBox
			// 
			this.RateTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateTextBox.Location = new System.Drawing.Point(103, 29);
			this.RateTextBox.Name = "RateTextBox";
			this.RateTextBox.ReadOnly = true;
			this.RateTextBox.Size = new System.Drawing.Size(120, 20);
			this.RateTextBox.TabIndex = 8;
			// 
			// SupplyNodeViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.Khaki;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SupplyNodeViewer";
			this.Size = new System.Drawing.Size(226, 52);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.Label RateLabel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		public System.Windows.Forms.TextBox NameBox;
		public System.Windows.Forms.TextBox RateTextBox;
	}
}
