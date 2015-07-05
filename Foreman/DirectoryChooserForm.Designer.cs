namespace Foreman
{
	partial class DirectoryChooserForm
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
			this.DirTextBox = new System.Windows.Forms.TextBox();
			this.BrowseButton = new System.Windows.Forms.Button();
			this.OKButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// DirTextBox
			// 
			this.DirTextBox.Location = new System.Drawing.Point(14, 15);
			this.DirTextBox.Name = "DirTextBox";
			this.DirTextBox.Size = new System.Drawing.Size(276, 20);
			this.DirTextBox.TabIndex = 0;
			this.DirTextBox.TextChanged += new System.EventHandler(this.DirTextBox_TextChanged);
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = new System.Drawing.Point(296, 13);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = new System.Drawing.Size(98, 23);
			this.BrowseButton.TabIndex = 1;
			this.BrowseButton.Text = "Browse...";
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Location = new System.Drawing.Point(238, 43);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(155, 26);
			this.OKButton.TabIndex = 2;
			this.OKButton.Text = "OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// DirectoryChooserForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(406, 81);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.DirTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "DirectoryChooserForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Please locate the Factorio directory";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox DirTextBox;
		private System.Windows.Forms.Button BrowseButton;
		private System.Windows.Forms.Button OKButton;
	}
}