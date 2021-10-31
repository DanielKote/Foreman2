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
            this.Scale3xCheckBox = new System.Windows.Forms.RadioButton();
            this.Scale2xCheckBox = new System.Windows.Forms.RadioButton();
            this.Scale1xCheckBox = new System.Windows.Forms.RadioButton();
            this.TransparencyCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileTextBox
            // 
            this.fileTextBox.Location = new System.Drawing.Point(16, 18);
            this.fileTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.fileTextBox.Name = "fileTextBox";
            this.fileTextBox.ReadOnly = true;
            this.fileTextBox.Size = new System.Drawing.Size(300, 22);
            this.fileTextBox.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(325, 16);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 28);
            this.button1.TabIndex = 1;
            this.button1.Text = "Browse...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(209, 52);
            this.ExportButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(239, 94);
            this.ExportButton.TabIndex = 2;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Scale3xCheckBox);
            this.groupBox1.Controls.Add(this.Scale2xCheckBox);
            this.groupBox1.Controls.Add(this.Scale1xCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(17, 52);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(184, 64);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Scale";
            // 
            // Scale3xCheckBox
            // 
            this.Scale3xCheckBox.AutoSize = true;
            this.Scale3xCheckBox.Location = new System.Drawing.Point(121, 23);
            this.Scale3xCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Scale3xCheckBox.Name = "Scale3xCheckBox";
            this.Scale3xCheckBox.Size = new System.Drawing.Size(43, 21);
            this.Scale3xCheckBox.TabIndex = 2;
            this.Scale3xCheckBox.Text = "3x";
            this.Scale3xCheckBox.UseVisualStyleBackColor = true;
            // 
            // Scale2xCheckBox
            // 
            this.Scale2xCheckBox.AutoSize = true;
            this.Scale2xCheckBox.Location = new System.Drawing.Point(65, 25);
            this.Scale2xCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Scale2xCheckBox.Name = "Scale2xCheckBox";
            this.Scale2xCheckBox.Size = new System.Drawing.Size(43, 21);
            this.Scale2xCheckBox.TabIndex = 1;
            this.Scale2xCheckBox.Text = "2x";
            this.Scale2xCheckBox.UseVisualStyleBackColor = true;
            // 
            // Scale1xCheckBox
            // 
            this.Scale1xCheckBox.AutoSize = true;
            this.Scale1xCheckBox.Checked = true;
            this.Scale1xCheckBox.Location = new System.Drawing.Point(9, 25);
            this.Scale1xCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Scale1xCheckBox.Name = "Scale1xCheckBox";
            this.Scale1xCheckBox.Size = new System.Drawing.Size(43, 21);
            this.Scale1xCheckBox.TabIndex = 0;
            this.Scale1xCheckBox.TabStop = true;
            this.Scale1xCheckBox.Text = "1x";
            this.Scale1xCheckBox.UseVisualStyleBackColor = true;
            // 
            // TransparencyCheckBox
            // 
            this.TransparencyCheckBox.AutoSize = true;
            this.TransparencyCheckBox.Location = new System.Drawing.Point(17, 123);
            this.TransparencyCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TransparencyCheckBox.Name = "TransparencyCheckBox";
            this.TransparencyCheckBox.Size = new System.Drawing.Size(188, 21);
            this.TransparencyCheckBox.TabIndex = 4;
            this.TransparencyCheckBox.Text = "Transparent Background";
            this.TransparencyCheckBox.UseVisualStyleBackColor = true;
            // 
            // ImageExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 151);
            this.Controls.Add(this.TransparencyCheckBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.fileTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageExportForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Export an Image";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox fileTextBox;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button ExportButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton Scale3xCheckBox;
		private System.Windows.Forms.RadioButton Scale2xCheckBox;
		private System.Windows.Forms.RadioButton Scale1xCheckBox;
		private System.Windows.Forms.CheckBox TransparencyCheckBox;
	}
}