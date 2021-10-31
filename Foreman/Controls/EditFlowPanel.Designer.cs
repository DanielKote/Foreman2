namespace Foreman
{
    partial class EditFlowPanel
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
			this.RateGroup = new System.Windows.Forms.GroupBox();
			this.RateGroup.SuspendLayout();
			this.SuspendLayout();
			// 
			// autoOption
			// 
			this.autoOption.AutoSize = true;
			this.autoOption.Checked = true;
			this.autoOption.Location = new System.Drawing.Point(7, 22);
			this.autoOption.Margin = new System.Windows.Forms.Padding(4);
			this.autoOption.Name = "autoOption";
			this.autoOption.Size = new System.Drawing.Size(121, 21);
			this.autoOption.TabIndex = 0;
			this.autoOption.TabStop = true;
			this.autoOption.Text = "Auto-Calculate";
			this.autoOption.UseVisualStyleBackColor = true;
			this.autoOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// fixedOption
			// 
			this.fixedOption.AutoSize = true;
			this.fixedOption.Location = new System.Drawing.Point(7, 51);
			this.fixedOption.Margin = new System.Windows.Forms.Padding(4);
			this.fixedOption.Name = "fixedOption";
			this.fixedOption.Size = new System.Drawing.Size(62, 21);
			this.fixedOption.TabIndex = 1;
			this.fixedOption.Text = "Fixed";
			this.fixedOption.UseVisualStyleBackColor = true;
			this.fixedOption.CheckedChanged += new System.EventHandler(this.fixedOption_CheckedChanged);
			this.fixedOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// fixedTextBox
			// 
			this.fixedTextBox.Location = new System.Drawing.Point(71, 50);
			this.fixedTextBox.Margin = new System.Windows.Forms.Padding(4);
			this.fixedTextBox.Name = "fixedTextBox";
			this.fixedTextBox.Size = new System.Drawing.Size(111, 22);
			this.fixedTextBox.TabIndex = 2;
			this.fixedTextBox.TextChanged += new System.EventHandler(this.fixedTextBox_TextChanged);
			this.fixedTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.fixedTextBox.LostFocus += new System.EventHandler(this.fixedTextBox_LostFocus);
			// 
			// RateGroup
			// 
			this.RateGroup.Controls.Add(this.autoOption);
			this.RateGroup.Controls.Add(this.fixedTextBox);
			this.RateGroup.Controls.Add(this.fixedOption);
			this.RateGroup.Location = new System.Drawing.Point(3, 1);
			this.RateGroup.Name = "RateGroup";
			this.RateGroup.Size = new System.Drawing.Size(191, 80);
			this.RateGroup.TabIndex = 12;
			this.RateGroup.TabStop = false;
			this.RateGroup.Text = "Item Flowrate (per 1hour):";
			// 
			// EditFlowPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.RateGroup);
			this.DoubleBuffered = true;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "EditFlowPanel";
			this.Size = new System.Drawing.Size(197, 84);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.RateGroup.ResumeLayout(false);
			this.RateGroup.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RadioButton autoOption;
        public System.Windows.Forms.RadioButton fixedOption;
        public System.Windows.Forms.TextBox fixedTextBox;
        private System.Windows.Forms.GroupBox RateGroup;
    }
}
