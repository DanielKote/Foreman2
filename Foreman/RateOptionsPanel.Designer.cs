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
			this.ratePanel = new System.Windows.Forms.Panel();
			this.assemblerPanel = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.modulesButton = new System.Windows.Forms.Button();
			this.assemblerButton = new System.Windows.Forms.Button();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.ratePanel.SuspendLayout();
			this.assemblerPanel.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
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
			// ratePanel
			// 
			this.ratePanel.Controls.Add(this.autoOption);
			this.ratePanel.Controls.Add(this.unitLabel);
			this.ratePanel.Controls.Add(this.fixedOption);
			this.ratePanel.Controls.Add(this.fixedTextBox);
			this.ratePanel.Location = new System.Drawing.Point(3, 3);
			this.ratePanel.Name = "ratePanel";
			this.ratePanel.Size = new System.Drawing.Size(95, 76);
			this.ratePanel.TabIndex = 4;
			// 
			// assemblerPanel
			// 
			this.assemblerPanel.AutoSize = true;
			this.assemblerPanel.Controls.Add(this.label2);
			this.assemblerPanel.Controls.Add(this.label1);
			this.assemblerPanel.Controls.Add(this.modulesButton);
			this.assemblerPanel.Controls.Add(this.assemblerButton);
			this.assemblerPanel.Location = new System.Drawing.Point(104, 3);
			this.assemblerPanel.Name = "assemblerPanel";
			this.assemblerPanel.Size = new System.Drawing.Size(171, 52);
			this.assemblerPanel.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 31);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(50, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Modules:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Assembler:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// modulesButton
			// 
			this.modulesButton.AutoSize = true;
			this.modulesButton.Location = new System.Drawing.Point(68, 26);
			this.modulesButton.Name = "modulesButton";
			this.modulesButton.Size = new System.Drawing.Size(100, 23);
			this.modulesButton.TabIndex = 1;
			this.modulesButton.Text = "Best";
			this.modulesButton.UseVisualStyleBackColor = true;
			this.modulesButton.Click += new System.EventHandler(this.modulesButton_Click);
			// 
			// assemblerButton
			// 
			this.assemblerButton.AutoSize = true;
			this.assemblerButton.Location = new System.Drawing.Point(68, 3);
			this.assemblerButton.Name = "assemblerButton";
			this.assemblerButton.Size = new System.Drawing.Size(100, 23);
			this.assemblerButton.TabIndex = 0;
			this.assemblerButton.Text = "Best";
			this.assemblerButton.UseVisualStyleBackColor = true;
			this.assemblerButton.Click += new System.EventHandler(this.assemblerButton_Click);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel1.Controls.Add(this.ratePanel);
			this.flowLayoutPanel1.Controls.Add(this.assemblerPanel);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(278, 82);
			this.flowLayoutPanel1.TabIndex = 8;
			// 
			// RateOptionsPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Name = "RateOptionsPanel";
			this.Size = new System.Drawing.Size(284, 88);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.ratePanel.ResumeLayout(false);
			this.ratePanel.PerformLayout();
			this.assemblerPanel.ResumeLayout(false);
			this.assemblerPanel.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.RadioButton autoOption;
		public System.Windows.Forms.RadioButton fixedOption;
		public System.Windows.Forms.TextBox fixedTextBox;
		private System.Windows.Forms.Label unitLabel;
		private System.Windows.Forms.Panel ratePanel;
		private System.Windows.Forms.Panel assemblerPanel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button modulesButton;
		private System.Windows.Forms.Button assemblerButton;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
	}
}
