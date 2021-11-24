namespace Foreman
{
	partial class ImageExportForm
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
			this.fileTextBox = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.ExportButton = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.TransparencyCheckBox = new System.Windows.Forms.CheckBox();
			this.ViewLimitCheckBox = new System.Windows.Forms.CheckBox();
			this.ScaleSelectionBox = new System.Windows.Forms.ComboBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// fileTextBox
			// 
			this.fileTextBox.Location = new System.Drawing.Point(12, 15);
			this.fileTextBox.Name = "fileTextBox";
			this.fileTextBox.ReadOnly = true;
			this.fileTextBox.Size = new System.Drawing.Size(226, 20);
			this.fileTextBox.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(244, 13);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(92, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "Browse...";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// ExportButton
			// 
			this.ExportButton.Location = new System.Drawing.Point(157, 42);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = new System.Drawing.Size(179, 98);
			this.ExportButton.TabIndex = 2;
			this.ExportButton.Text = "Export";
			this.ExportButton.UseVisualStyleBackColor = true;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.ScaleSelectionBox);
			this.groupBox1.Location = new System.Drawing.Point(13, 42);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(138, 52);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Scale";
			// 
			// TransparencyCheckBox
			// 
			this.TransparencyCheckBox.AutoSize = true;
			this.TransparencyCheckBox.Location = new System.Drawing.Point(13, 100);
			this.TransparencyCheckBox.Name = "TransparencyCheckBox";
			this.TransparencyCheckBox.Size = new System.Drawing.Size(144, 17);
			this.TransparencyCheckBox.TabIndex = 4;
			this.TransparencyCheckBox.Text = "Transparent Background";
			this.TransparencyCheckBox.UseVisualStyleBackColor = true;
			// 
			// ViewLimitCheckBox
			// 
			this.ViewLimitCheckBox.AutoSize = true;
			this.ViewLimitCheckBox.Location = new System.Drawing.Point(12, 123);
			this.ViewLimitCheckBox.Name = "ViewLimitCheckBox";
			this.ViewLimitCheckBox.Size = new System.Drawing.Size(85, 17);
			this.ViewLimitCheckBox.TabIndex = 5;
			this.ViewLimitCheckBox.Text = "Limit to View";
			this.ViewLimitCheckBox.UseVisualStyleBackColor = true;
			// 
			// ScaleSelectionBox
			// 
			this.ScaleSelectionBox.Location = new System.Drawing.Point(6, 19);
			this.ScaleSelectionBox.Name = "ScaleSelectionBox";
			this.ScaleSelectionBox.Size = new System.Drawing.Size(121, 21);
			this.ScaleSelectionBox.TabIndex = 6;
			// 
			// ImageExportForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(348, 191);
			this.Controls.Add(this.ViewLimitCheckBox);
			this.Controls.Add(this.TransparencyCheckBox);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.fileTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImageExportForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Export an Image";
			this.TopMost = true;
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox fileTextBox;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button ExportButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox TransparencyCheckBox;
		private System.Windows.Forms.CheckBox ViewLimitCheckBox;
		private System.Windows.Forms.ComboBox ScaleSelectionBox;
	}
}