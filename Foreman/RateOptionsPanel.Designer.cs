namespace Foreman
{
	partial class RateOptionsPanel
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
			this.autoOption = new System.Windows.Forms.RadioButton();
			this.fixedOption = new System.Windows.Forms.RadioButton();
			this.fixedTextBox = new System.Windows.Forms.TextBox();
			this.unitLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// autoOption
			// 
			this.autoOption.AutoSize = true;
			this.autoOption.Checked = true;
			this.autoOption.Location = new System.Drawing.Point(4, 4);
			this.autoOption.Name = "autoOption";
			this.autoOption.Size = new System.Drawing.Size(47, 17);
			this.autoOption.TabIndex = 0;
			this.autoOption.TabStop = true;
			this.autoOption.Text = "Auto";
			this.autoOption.UseVisualStyleBackColor = true;
			this.autoOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// fixedOption
			// 
			this.fixedOption.AutoSize = true;
			this.fixedOption.Location = new System.Drawing.Point(4, 27);
			this.fixedOption.Name = "fixedOption";
			this.fixedOption.Size = new System.Drawing.Size(50, 17);
			this.fixedOption.TabIndex = 1;
			this.fixedOption.Text = "Fixed";
			this.fixedOption.UseVisualStyleBackColor = true;
			this.fixedOption.CheckedChanged += new System.EventHandler(this.fixedOption_CheckedChanged);
			this.fixedOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// fixedTextBox
			// 
			this.fixedTextBox.Location = new System.Drawing.Point(4, 51);
			this.fixedTextBox.Name = "fixedTextBox";
			this.fixedTextBox.Size = new System.Drawing.Size(65, 20);
			this.fixedTextBox.TabIndex = 2;
			this.fixedTextBox.TextChanged += new System.EventHandler(this.fixedTextBox_TextChanged);
			this.fixedTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// unitLabel
			// 
			this.unitLabel.AutoSize = true;
			this.unitLabel.Location = new System.Drawing.Point(74, 54);
			this.unitLabel.Name = "unitLabel";
			this.unitLabel.Size = new System.Drawing.Size(17, 13);
			this.unitLabel.TabIndex = 3;
			this.unitLabel.Text = "/s";
			// 
			// RateOptionsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.unitLabel);
			this.Controls.Add(this.fixedTextBox);
			this.Controls.Add(this.fixedOption);
			this.Controls.Add(this.autoOption);
			this.Name = "RateOptionsPanel";
			this.Size = new System.Drawing.Size(93, 75);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.RadioButton autoOption;
		public System.Windows.Forms.RadioButton fixedOption;
		public System.Windows.Forms.TextBox fixedTextBox;
		private System.Windows.Forms.Label unitLabel;
	}
}
