namespace Foreman
{
    partial class SettingsForm
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
            this.ModsGroupBox = new System.Windows.Forms.GroupBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.ModuleSelectionNoneButton = new System.Windows.Forms.Button();
            this.ModuleSelectionAllButton = new System.Windows.Forms.Button();
            this.ModuleSelectionBox = new System.Windows.Forms.CheckedListBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.MinerSelectionNoneButton = new System.Windows.Forms.Button();
            this.MinerSelectionAllButton = new System.Windows.Forms.Button();
            this.MinerSelectionBox = new System.Windows.Forms.CheckedListBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.AssemblerSelectionNoneButton = new System.Windows.Forms.Button();
            this.AssemblerSelectionAllButton = new System.Windows.Forms.Button();
            this.AssemblerSelectionBox = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.CancelSettingsButton = new System.Windows.Forms.Button();
            this.ReloadButton = new System.Windows.Forms.Button();
            this.ModSelectionBox = new Foreman.CheckboxListWithErrors();
            this.ModsGroupBox.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // ModsGroupBox
            // 
            this.ModsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ModsGroupBox.Controls.Add(this.ModSelectionBox);
            this.ModsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.ModsGroupBox.Location = new System.Drawing.Point(274, 21);
            this.ModsGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.ModsGroupBox.Name = "ModsGroupBox";
            this.ModsGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.ModsGroupBox.Size = new System.Drawing.Size(257, 566);
            this.ModsGroupBox.TabIndex = 13;
            this.ModsGroupBox.TabStop = false;
            this.ModsGroupBox.Text = "Mods (read-only)";
            // 
            // groupBox7
            // 
            this.groupBox7.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox7.Controls.Add(this.ModuleSelectionNoneButton);
            this.groupBox7.Controls.Add(this.ModuleSelectionAllButton);
            this.groupBox7.Controls.Add(this.ModuleSelectionBox);
            this.groupBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.groupBox7.Location = new System.Drawing.Point(8, 403);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.groupBox7.Size = new System.Drawing.Size(259, 183);
            this.groupBox7.TabIndex = 14;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Modules";
            // 
            // ModuleSelectionNoneButton
            // 
            this.ModuleSelectionNoneButton.Location = new System.Drawing.Point(134, 152);
            this.ModuleSelectionNoneButton.Name = "ModuleSelectionNoneButton";
            this.ModuleSelectionNoneButton.Size = new System.Drawing.Size(115, 22);
            this.ModuleSelectionNoneButton.TabIndex = 9;
            this.ModuleSelectionNoneButton.Text = "None";
            this.ModuleSelectionNoneButton.UseVisualStyleBackColor = true;
            this.ModuleSelectionNoneButton.Click += new System.EventHandler(this.ModuleSelectionNoneButton_Click);
            // 
            // ModuleSelectionAllButton
            // 
            this.ModuleSelectionAllButton.Location = new System.Drawing.Point(7, 152);
            this.ModuleSelectionAllButton.Name = "ModuleSelectionAllButton";
            this.ModuleSelectionAllButton.Size = new System.Drawing.Size(115, 22);
            this.ModuleSelectionAllButton.TabIndex = 8;
            this.ModuleSelectionAllButton.Text = "All";
            this.ModuleSelectionAllButton.UseVisualStyleBackColor = true;
            this.ModuleSelectionAllButton.Click += new System.EventHandler(this.ModuleSelectionAllButton_Click);
            // 
            // ModuleSelectionBox
            // 
            this.ModuleSelectionBox.CheckOnClick = true;
            this.ModuleSelectionBox.FormattingEnabled = true;
            this.ModuleSelectionBox.Location = new System.Drawing.Point(8, 28);
            this.ModuleSelectionBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.ModuleSelectionBox.Name = "ModuleSelectionBox";
            this.ModuleSelectionBox.Size = new System.Drawing.Size(240, 123);
            this.ModuleSelectionBox.TabIndex = 7;
            this.ModuleSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ModuleSelectionBox_ItemCheck);
            // 
            // groupBox6
            // 
            this.groupBox6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox6.Controls.Add(this.MinerSelectionNoneButton);
            this.groupBox6.Controls.Add(this.MinerSelectionAllButton);
            this.groupBox6.Controls.Add(this.MinerSelectionBox);
            this.groupBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.groupBox6.Location = new System.Drawing.Point(8, 212);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.groupBox6.Size = new System.Drawing.Size(259, 183);
            this.groupBox6.TabIndex = 13;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Miners/Pumpjacks";
            // 
            // MinerSelectionNoneButton
            // 
            this.MinerSelectionNoneButton.Location = new System.Drawing.Point(133, 152);
            this.MinerSelectionNoneButton.Name = "MinerSelectionNoneButton";
            this.MinerSelectionNoneButton.Size = new System.Drawing.Size(115, 22);
            this.MinerSelectionNoneButton.TabIndex = 6;
            this.MinerSelectionNoneButton.Text = "None";
            this.MinerSelectionNoneButton.UseVisualStyleBackColor = true;
            this.MinerSelectionNoneButton.Click += new System.EventHandler(this.MinerSelectionNoneButton_Click);
            // 
            // MinerSelectionAllButton
            // 
            this.MinerSelectionAllButton.Location = new System.Drawing.Point(7, 152);
            this.MinerSelectionAllButton.Name = "MinerSelectionAllButton";
            this.MinerSelectionAllButton.Size = new System.Drawing.Size(115, 22);
            this.MinerSelectionAllButton.TabIndex = 5;
            this.MinerSelectionAllButton.Text = "All";
            this.MinerSelectionAllButton.UseVisualStyleBackColor = true;
            this.MinerSelectionAllButton.Click += new System.EventHandler(this.MinerSelectionAllButton_Click);
            // 
            // MinerSelectionBox
            // 
            this.MinerSelectionBox.CheckOnClick = true;
            this.MinerSelectionBox.FormattingEnabled = true;
            this.MinerSelectionBox.Location = new System.Drawing.Point(8, 28);
            this.MinerSelectionBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.MinerSelectionBox.Name = "MinerSelectionBox";
            this.MinerSelectionBox.Size = new System.Drawing.Size(240, 123);
            this.MinerSelectionBox.TabIndex = 4;
            this.MinerSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.MinerSelectionBox_ItemCheck);
            // 
            // groupBox5
            // 
            this.groupBox5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox5.Controls.Add(this.AssemblerSelectionNoneButton);
            this.groupBox5.Controls.Add(this.AssemblerSelectionAllButton);
            this.groupBox5.Controls.Add(this.AssemblerSelectionBox);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.groupBox5.Location = new System.Drawing.Point(8, 22);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.groupBox5.Size = new System.Drawing.Size(259, 182);
            this.groupBox5.TabIndex = 12;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Assemblers/Smelters";
            // 
            // AssemblerSelectionNoneButton
            // 
            this.AssemblerSelectionNoneButton.Location = new System.Drawing.Point(133, 151);
            this.AssemblerSelectionNoneButton.Name = "AssemblerSelectionNoneButton";
            this.AssemblerSelectionNoneButton.Size = new System.Drawing.Size(115, 22);
            this.AssemblerSelectionNoneButton.TabIndex = 3;
            this.AssemblerSelectionNoneButton.Text = "None";
            this.AssemblerSelectionNoneButton.UseVisualStyleBackColor = true;
            this.AssemblerSelectionNoneButton.Click += new System.EventHandler(this.AssemblerSelectionNoneButton_Click);
            // 
            // AssemblerSelectionAllButton
            // 
            this.AssemblerSelectionAllButton.Location = new System.Drawing.Point(8, 151);
            this.AssemblerSelectionAllButton.Name = "AssemblerSelectionAllButton";
            this.AssemblerSelectionAllButton.Size = new System.Drawing.Size(115, 22);
            this.AssemblerSelectionAllButton.TabIndex = 2;
            this.AssemblerSelectionAllButton.Text = "All";
            this.AssemblerSelectionAllButton.UseVisualStyleBackColor = true;
            this.AssemblerSelectionAllButton.Click += new System.EventHandler(this.AssemblerSelectionAllButton_Click);
            // 
            // AssemblerSelectionBox
            // 
            this.AssemblerSelectionBox.CheckOnClick = true;
            this.AssemblerSelectionBox.FormattingEnabled = true;
            this.AssemblerSelectionBox.Location = new System.Drawing.Point(8, 27);
            this.AssemblerSelectionBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.AssemblerSelectionBox.Name = "AssemblerSelectionBox";
            this.AssemblerSelectionBox.Size = new System.Drawing.Size(240, 123);
            this.AssemblerSelectionBox.TabIndex = 1;
            this.AssemblerSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.AssemblerSelectionBox_ItemCheck);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.groupBox7);
            this.groupBox2.Controls.Add(this.ModsGroupBox);
            this.groupBox2.Controls.Add(this.groupBox6);
            this.groupBox2.Controls.Add(this.groupBox5);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.groupBox2.Location = new System.Drawing.Point(475, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(539, 595);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Enable / Disable Loaded Objects";
            // 
            // groupBox3
            // 
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.groupBox3.Location = new System.Drawing.Point(12, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(453, 554);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Factorio Presets:";
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.Location = new System.Drawing.Point(12, 565);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(261, 32);
            this.ConfirmButton.TabIndex = 25;
            this.ConfirmButton.Text = "Confirm";
            this.ConfirmButton.UseVisualStyleBackColor = true;
            this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
            // 
            // CancelSettingsButton
            // 
            this.CancelSettingsButton.Location = new System.Drawing.Point(375, 565);
            this.CancelSettingsButton.Name = "CancelSettingsButton";
            this.CancelSettingsButton.Size = new System.Drawing.Size(90, 32);
            this.CancelSettingsButton.TabIndex = 26;
            this.CancelSettingsButton.Text = "Cancel";
            this.CancelSettingsButton.UseVisualStyleBackColor = true;
            this.CancelSettingsButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ReloadButton
            // 
            this.ReloadButton.Location = new System.Drawing.Point(279, 565);
            this.ReloadButton.Name = "ReloadButton";
            this.ReloadButton.Size = new System.Drawing.Size(90, 32);
            this.ReloadButton.TabIndex = 27;
            this.ReloadButton.Text = "Reload";
            this.ReloadButton.UseVisualStyleBackColor = true;
            this.ReloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
            // 
            // ModSelectionBox
            // 
            this.ModSelectionBox.FormattingEnabled = true;
            this.ModSelectionBox.Location = new System.Drawing.Point(9, 24);
            this.ModSelectionBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.ModSelectionBox.Name = "ModSelectionBox";
            this.ModSelectionBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.ModSelectionBox.Size = new System.Drawing.Size(240, 531);
            this.ModSelectionBox.TabIndex = 10;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1025, 609);
            this.Controls.Add(this.ReloadButton);
            this.Controls.Add(this.CancelSettingsButton);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.ConfirmButton);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.ModsGroupBox.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox ModsGroupBox;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.CheckedListBox ModuleSelectionBox;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckedListBox MinerSelectionBox;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckedListBox AssemblerSelectionBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button CancelSettingsButton;
        private System.Windows.Forms.Button ConfirmButton;
        private System.Windows.Forms.Button ModuleSelectionNoneButton;
        private System.Windows.Forms.Button ModuleSelectionAllButton;
        private System.Windows.Forms.Button MinerSelectionNoneButton;
        private System.Windows.Forms.Button MinerSelectionAllButton;
        private System.Windows.Forms.Button AssemblerSelectionNoneButton;
        private System.Windows.Forms.Button AssemblerSelectionAllButton;
        private System.Windows.Forms.Button ReloadButton;
        private CheckboxListWithErrors ModSelectionBox;
    }
}