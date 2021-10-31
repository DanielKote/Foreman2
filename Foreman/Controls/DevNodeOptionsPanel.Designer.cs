namespace Foreman
{
    partial class DevNodeOptionsPanel
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
            this.AModuleSelectionBox = new System.Windows.Forms.ComboBox();
            this.AssemblerSelectionBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.AssemblerGroup = new System.Windows.Forms.GroupBox();
            this.AFuelSelectionBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.BeaconGroup = new System.Windows.Forms.GroupBox();
            this.BeaconCounter = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.BeaconSelectionBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BModuleSelectionBox = new System.Windows.Forms.ComboBox();
            this.RateGroup = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.AssemblerGroup.SuspendLayout();
            this.BeaconGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BeaconCounter)).BeginInit();
            this.RateGroup.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // autoOption
            // 
            this.autoOption.AutoSize = true;
            this.autoOption.Checked = true;
            this.autoOption.Location = new System.Drawing.Point(7, 22);
            this.autoOption.Margin = new System.Windows.Forms.Padding(4);
            this.autoOption.Name = "autoOption";
            this.autoOption.Size = new System.Drawing.Size(58, 21);
            this.autoOption.TabIndex = 0;
            this.autoOption.TabStop = true;
            this.autoOption.Text = "Auto";
            this.autoOption.UseVisualStyleBackColor = true;
            this.autoOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
            // 
            // fixedOption
            // 
            this.fixedOption.AutoSize = true;
            this.fixedOption.Location = new System.Drawing.Point(73, 22);
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
            this.fixedTextBox.Location = new System.Drawing.Point(143, 21);
            this.fixedTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.fixedTextBox.Name = "fixedTextBox";
            this.fixedTextBox.Size = new System.Drawing.Size(118, 22);
            this.fixedTextBox.TabIndex = 2;
            this.fixedTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
            this.fixedTextBox.LostFocus += new System.EventHandler(this.fixedTextBox_LostFocus);
            // 
            // AModuleSelectionBox
            // 
            this.AModuleSelectionBox.DisplayMember = "FriendlyName";
            this.AModuleSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AModuleSelectionBox.FormattingEnabled = true;
            this.AModuleSelectionBox.Location = new System.Drawing.Point(79, 51);
            this.AModuleSelectionBox.Name = "AModuleSelectionBox";
            this.AModuleSelectionBox.Size = new System.Drawing.Size(183, 24);
            this.AModuleSelectionBox.TabIndex = 9;
            this.AModuleSelectionBox.ValueMember = "FriendlyName";
            this.AModuleSelectionBox.SelectedIndexChanged += new System.EventHandler(this.AModuleSelectionBox_SelectedIndexChanged);
            // 
            // AssemblerSelectionBox
            // 
            this.AssemblerSelectionBox.DisplayMember = "FriendlyName";
            this.AssemblerSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AssemblerSelectionBox.FormattingEnabled = true;
            this.AssemblerSelectionBox.Location = new System.Drawing.Point(10, 21);
            this.AssemblerSelectionBox.Name = "AssemblerSelectionBox";
            this.AssemblerSelectionBox.Size = new System.Drawing.Size(252, 24);
            this.AssemblerSelectionBox.TabIndex = 8;
            this.AssemblerSelectionBox.ValueMember = "FriendlyName";
            this.AssemblerSelectionBox.SelectedIndexChanged += new System.EventHandler(this.AssemblerSelectionBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 54);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Modules:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AssemblerGroup
            // 
            this.AssemblerGroup.AutoSize = true;
            this.AssemblerGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AssemblerGroup.Controls.Add(this.AFuelSelectionBox);
            this.AssemblerGroup.Controls.Add(this.label4);
            this.AssemblerGroup.Controls.Add(this.AssemblerSelectionBox);
            this.AssemblerGroup.Controls.Add(this.label2);
            this.AssemblerGroup.Controls.Add(this.AModuleSelectionBox);
            this.AssemblerGroup.Location = new System.Drawing.Point(3, 60);
            this.AssemblerGroup.Name = "AssemblerGroup";
            this.AssemblerGroup.Size = new System.Drawing.Size(268, 129);
            this.AssemblerGroup.TabIndex = 10;
            this.AssemblerGroup.TabStop = false;
            this.AssemblerGroup.Text = "Assembler";
            // 
            // AFuelSelectionBox
            // 
            this.AFuelSelectionBox.DisplayMember = "FriendlyName";
            this.AFuelSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AFuelSelectionBox.FormattingEnabled = true;
            this.AFuelSelectionBox.Location = new System.Drawing.Point(79, 84);
            this.AFuelSelectionBox.Name = "AFuelSelectionBox";
            this.AFuelSelectionBox.Size = new System.Drawing.Size(183, 24);
            this.AFuelSelectionBox.TabIndex = 11;
            this.AFuelSelectionBox.ValueMember = "FriendlyName";
            this.AFuelSelectionBox.SelectedIndexChanged += new System.EventHandler(this.AFuelSelectionBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 87);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 17);
            this.label4.TabIndex = 10;
            this.label4.Text = "Fuel:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // BeaconGroup
            // 
            this.BeaconGroup.Controls.Add(this.BeaconCounter);
            this.BeaconGroup.Controls.Add(this.label3);
            this.BeaconGroup.Controls.Add(this.BeaconSelectionBox);
            this.BeaconGroup.Controls.Add(this.label1);
            this.BeaconGroup.Controls.Add(this.BModuleSelectionBox);
            this.BeaconGroup.Location = new System.Drawing.Point(3, 195);
            this.BeaconGroup.Name = "BeaconGroup";
            this.BeaconGroup.Size = new System.Drawing.Size(268, 116);
            this.BeaconGroup.TabIndex = 11;
            this.BeaconGroup.TabStop = false;
            this.BeaconGroup.Text = "Beacon";
            // 
            // BeaconCounter
            // 
            this.BeaconCounter.DecimalPlaces = 1;
            this.BeaconCounter.Location = new System.Drawing.Point(79, 81);
            this.BeaconCounter.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.BeaconCounter.Name = "BeaconCounter";
            this.BeaconCounter.Size = new System.Drawing.Size(183, 22);
            this.BeaconCounter.TabIndex = 11;
            this.BeaconCounter.ValueChanged += new System.EventHandler(this.BeaconCounter_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 17);
            this.label3.TabIndex = 10;
            this.label3.Text = "Count:";
            // 
            // BeaconSelectionBox
            // 
            this.BeaconSelectionBox.DisplayMember = "FriendlyName";
            this.BeaconSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BeaconSelectionBox.FormattingEnabled = true;
            this.BeaconSelectionBox.Location = new System.Drawing.Point(6, 21);
            this.BeaconSelectionBox.Name = "BeaconSelectionBox";
            this.BeaconSelectionBox.Size = new System.Drawing.Size(256, 24);
            this.BeaconSelectionBox.TabIndex = 8;
            this.BeaconSelectionBox.ValueMember = "FriendlyName";
            this.BeaconSelectionBox.SelectedIndexChanged += new System.EventHandler(this.BeaconSelectionBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 54);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Modules:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // BModuleSelectionBox
            // 
            this.BModuleSelectionBox.DisplayMember = "FriendlyName";
            this.BModuleSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BModuleSelectionBox.FormattingEnabled = true;
            this.BModuleSelectionBox.Location = new System.Drawing.Point(79, 51);
            this.BModuleSelectionBox.Name = "BModuleSelectionBox";
            this.BModuleSelectionBox.Size = new System.Drawing.Size(183, 24);
            this.BModuleSelectionBox.TabIndex = 9;
            this.BModuleSelectionBox.ValueMember = "FriendlyName";
            this.BModuleSelectionBox.SelectedIndexChanged += new System.EventHandler(this.BModuleSelectionBox_SelectedIndexChanged);
            // 
            // RateGroup
            // 
            this.RateGroup.Controls.Add(this.autoOption);
            this.RateGroup.Controls.Add(this.fixedTextBox);
            this.RateGroup.Controls.Add(this.fixedOption);
            this.RateGroup.Location = new System.Drawing.Point(3, 3);
            this.RateGroup.Name = "RateGroup";
            this.RateGroup.Size = new System.Drawing.Size(268, 51);
            this.RateGroup.TabIndex = 12;
            this.RateGroup.TabStop = false;
            this.RateGroup.Text = "Rate:";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.RateGroup);
            this.flowLayoutPanel1.Controls.Add(this.AssemblerGroup);
            this.flowLayoutPanel1.Controls.Add(this.BeaconGroup);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(274, 314);
            this.flowLayoutPanel1.TabIndex = 13;
            // 
            // DevNodeOptionsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DevNodeOptionsPanel";
            this.Size = new System.Drawing.Size(274, 314);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
            this.AssemblerGroup.ResumeLayout(false);
            this.AssemblerGroup.PerformLayout();
            this.BeaconGroup.ResumeLayout(false);
            this.BeaconGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BeaconCounter)).EndInit();
            this.RateGroup.ResumeLayout(false);
            this.RateGroup.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.RadioButton autoOption;
        public System.Windows.Forms.RadioButton fixedOption;
        public System.Windows.Forms.TextBox fixedTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox AModuleSelectionBox;
        private System.Windows.Forms.ComboBox AssemblerSelectionBox;
        private System.Windows.Forms.GroupBox AssemblerGroup;
        private System.Windows.Forms.GroupBox RateGroup;
        private System.Windows.Forms.GroupBox BeaconGroup;
        private System.Windows.Forms.NumericUpDown BeaconCounter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox BeaconSelectionBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox BModuleSelectionBox;
        private System.Windows.Forms.ComboBox AFuelSelectionBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
