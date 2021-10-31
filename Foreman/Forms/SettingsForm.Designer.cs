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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.IgnoreUserDataLocationCheckBox = new System.Windows.Forms.CheckBox();
            this.UserDataLocationBrowseButton = new System.Windows.Forms.Button();
            this.UserDataLocationTextBox = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.InstallLocationBrowseButton = new System.Windows.Forms.Button();
            this.InstallLocationTextBox = new System.Windows.Forms.TextBox();
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.CancelSettingsButton = new System.Windows.Forms.Button();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.UseSaveFileDataCheckBox = new System.Windows.Forms.CheckBox();
            this.UseForemanModRadioButton = new System.Windows.Forms.RadioButton();
            this.UseFactorioBaseRadioButton = new System.Windows.Forms.RadioButton();
            this.UseFactorioBaseOptionsGroup = new System.Windows.Forms.GroupBox();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.ExpensiveDifficultyRadioButton = new System.Windows.Forms.RadioButton();
            this.NormalDifficultyRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.LanguageDropDown = new System.Windows.Forms.ComboBox();
            this.ReloadButton = new System.Windows.Forms.Button();
            this.ModsGroupBox = new System.Windows.Forms.GroupBox();
            this.ModSelectionNoneButton = new System.Windows.Forms.Button();
            this.ModSelectionAllButton = new System.Windows.Forms.Button();
            this.ModSelectionBox = new Foreman.CheckboxListWithErrors();
            this.groupBox3.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.UseFactorioBaseOptionsGroup.SuspendLayout();
            this.groupBox14.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.ModsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox9);
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.groupBox3.Location = new System.Drawing.Point(12, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(453, 185);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Factorio Setup Folders:";
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.IgnoreUserDataLocationCheckBox);
            this.groupBox9.Controls.Add(this.UserDataLocationBrowseButton);
            this.groupBox9.Controls.Add(this.UserDataLocationTextBox);
            this.groupBox9.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.groupBox9.Location = new System.Drawing.Point(10, 85);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(434, 87);
            this.groupBox9.TabIndex = 3;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "User Data Location (if different from factorio location):";
            // 
            // IgnoreUserDataLocationCheckBox
            // 
            this.IgnoreUserDataLocationCheckBox.AutoSize = true;
            this.IgnoreUserDataLocationCheckBox.Location = new System.Drawing.Point(7, 29);
            this.IgnoreUserDataLocationCheckBox.Name = "IgnoreUserDataLocationCheckBox";
            this.IgnoreUserDataLocationCheckBox.Size = new System.Drawing.Size(189, 21);
            this.IgnoreUserDataLocationCheckBox.TabIndex = 19;
            this.IgnoreUserDataLocationCheckBox.Text = "Same as factorio location";
            this.IgnoreUserDataLocationCheckBox.UseVisualStyleBackColor = true;
            this.IgnoreUserDataLocationCheckBox.CheckedChanged += new System.EventHandler(this.IgnoreUserDataLocationCheckBox_CheckedChanged);
            // 
            // UserDataLocationBrowseButton
            // 
            this.UserDataLocationBrowseButton.Location = new System.Drawing.Point(353, 58);
            this.UserDataLocationBrowseButton.Name = "UserDataLocationBrowseButton";
            this.UserDataLocationBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.UserDataLocationBrowseButton.TabIndex = 18;
            this.UserDataLocationBrowseButton.Text = "Browse...";
            this.UserDataLocationBrowseButton.UseVisualStyleBackColor = true;
            this.UserDataLocationBrowseButton.Click += new System.EventHandler(this.UserDataLocationBrowseButton_Click);
            // 
            // UserDataLocationTextBox
            // 
            this.UserDataLocationTextBox.Location = new System.Drawing.Point(7, 59);
            this.UserDataLocationTextBox.Name = "UserDataLocationTextBox";
            this.UserDataLocationTextBox.Size = new System.Drawing.Size(340, 22);
            this.UserDataLocationTextBox.TabIndex = 17;
            this.UserDataLocationTextBox.TextChanged += new System.EventHandler(this.UserDataLocationTextBox_TextChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.InstallLocationBrowseButton);
            this.groupBox4.Controls.Add(this.InstallLocationTextBox);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.groupBox4.Location = new System.Drawing.Point(10, 22);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(434, 57);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Factorio Location:";
            // 
            // InstallLocationBrowseButton
            // 
            this.InstallLocationBrowseButton.Location = new System.Drawing.Point(353, 25);
            this.InstallLocationBrowseButton.Name = "InstallLocationBrowseButton";
            this.InstallLocationBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.InstallLocationBrowseButton.TabIndex = 14;
            this.InstallLocationBrowseButton.Text = "Browse...";
            this.InstallLocationBrowseButton.UseVisualStyleBackColor = true;
            this.InstallLocationBrowseButton.Click += new System.EventHandler(this.InstallLocationBrowseButton_Click);
            // 
            // InstallLocationTextBox
            // 
            this.InstallLocationTextBox.Location = new System.Drawing.Point(7, 26);
            this.InstallLocationTextBox.Name = "InstallLocationTextBox";
            this.InstallLocationTextBox.Size = new System.Drawing.Size(340, 22);
            this.InstallLocationTextBox.TabIndex = 13;
            this.InstallLocationTextBox.TextChanged += new System.EventHandler(this.InstallLocationTextBox_TextChanged);
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.Location = new System.Drawing.Point(230, 499);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(500, 32);
            this.ConfirmButton.TabIndex = 25;
            this.ConfirmButton.Text = "Confirm";
            this.ConfirmButton.UseVisualStyleBackColor = true;
            this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
            // 
            // CancelSettingsButton
            // 
            this.CancelSettingsButton.Location = new System.Drawing.Point(12, 499);
            this.CancelSettingsButton.Name = "CancelSettingsButton";
            this.CancelSettingsButton.Size = new System.Drawing.Size(90, 32);
            this.CancelSettingsButton.TabIndex = 26;
            this.CancelSettingsButton.Text = "Cancel";
            this.CancelSettingsButton.UseVisualStyleBackColor = true;
            this.CancelSettingsButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.UseSaveFileDataCheckBox);
            this.groupBox10.Controls.Add(this.UseForemanModRadioButton);
            this.groupBox10.Controls.Add(this.UseFactorioBaseRadioButton);
            this.groupBox10.Controls.Add(this.UseFactorioBaseOptionsGroup);
            this.groupBox10.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.groupBox10.Location = new System.Drawing.Point(12, 204);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(453, 289);
            this.groupBox10.TabIndex = 19;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Factorio Options:";
            // 
            // UseSaveFileDataCheckBox
            // 
            this.UseSaveFileDataCheckBox.AutoSize = true;
            this.UseSaveFileDataCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UseSaveFileDataCheckBox.Location = new System.Drawing.Point(17, 25);
            this.UseSaveFileDataCheckBox.Name = "UseSaveFileDataCheckBox";
            this.UseSaveFileDataCheckBox.Size = new System.Drawing.Size(248, 21);
            this.UseSaveFileDataCheckBox.TabIndex = 20;
            this.UseSaveFileDataCheckBox.Text = "Use Save-file data if available";
            this.UseSaveFileDataCheckBox.UseVisualStyleBackColor = true;
            this.UseSaveFileDataCheckBox.CheckedChanged += new System.EventHandler(this.UseSaveFileDataCheckBox_CheckedChanged);
            // 
            // UseForemanModRadioButton
            // 
            this.UseForemanModRadioButton.AutoSize = true;
            this.UseForemanModRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.UseForemanModRadioButton.Location = new System.Drawing.Point(17, 79);
            this.UseForemanModRadioButton.Name = "UseForemanModRadioButton";
            this.UseForemanModRadioButton.Size = new System.Drawing.Size(426, 21);
            this.UseForemanModRadioButton.TabIndex = 19;
            this.UseForemanModRadioButton.TabStop = true;
            this.UseForemanModRadioButton.Text = "Use Foreman mod to generate objects (Highly Recommended!)";
            this.UseForemanModRadioButton.UseVisualStyleBackColor = true;
            // 
            // UseFactorioBaseRadioButton
            // 
            this.UseFactorioBaseRadioButton.AutoSize = true;
            this.UseFactorioBaseRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.UseFactorioBaseRadioButton.Location = new System.Drawing.Point(17, 112);
            this.UseFactorioBaseRadioButton.Name = "UseFactorioBaseRadioButton";
            this.UseFactorioBaseRadioButton.Size = new System.Drawing.Size(352, 21);
            this.UseFactorioBaseRadioButton.TabIndex = 19;
            this.UseFactorioBaseRadioButton.TabStop = true;
            this.UseFactorioBaseRadioButton.Text = "Auto generate objects from factorio base LUA code";
            this.UseFactorioBaseRadioButton.UseVisualStyleBackColor = true;
            this.UseFactorioBaseRadioButton.CheckedChanged += new System.EventHandler(this.UseFactorioBaseRadioButton_CheckedChanged);
            // 
            // UseFactorioBaseOptionsGroup
            // 
            this.UseFactorioBaseOptionsGroup.Controls.Add(this.groupBox14);
            this.UseFactorioBaseOptionsGroup.Controls.Add(this.groupBox13);
            this.UseFactorioBaseOptionsGroup.Location = new System.Drawing.Point(10, 116);
            this.UseFactorioBaseOptionsGroup.Name = "UseFactorioBaseOptionsGroup";
            this.UseFactorioBaseOptionsGroup.Size = new System.Drawing.Size(434, 162);
            this.UseFactorioBaseOptionsGroup.TabIndex = 2;
            this.UseFactorioBaseOptionsGroup.TabStop = false;
            // 
            // groupBox14
            // 
            this.groupBox14.Controls.Add(this.ExpensiveDifficultyRadioButton);
            this.groupBox14.Controls.Add(this.NormalDifficultyRadioButton);
            this.groupBox14.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.groupBox14.Location = new System.Drawing.Point(10, 91);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(415, 58);
            this.groupBox14.TabIndex = 2;
            this.groupBox14.TabStop = false;
            this.groupBox14.Text = "Difficulty";
            // 
            // ExpensiveDifficultyRadioButton
            // 
            this.ExpensiveDifficultyRadioButton.AutoSize = true;
            this.ExpensiveDifficultyRadioButton.Location = new System.Drawing.Point(127, 26);
            this.ExpensiveDifficultyRadioButton.Name = "ExpensiveDifficultyRadioButton";
            this.ExpensiveDifficultyRadioButton.Size = new System.Drawing.Size(93, 21);
            this.ExpensiveDifficultyRadioButton.TabIndex = 21;
            this.ExpensiveDifficultyRadioButton.TabStop = true;
            this.ExpensiveDifficultyRadioButton.Text = "Expensive";
            this.ExpensiveDifficultyRadioButton.UseVisualStyleBackColor = true;
            // 
            // NormalDifficultyRadioButton
            // 
            this.NormalDifficultyRadioButton.AutoSize = true;
            this.NormalDifficultyRadioButton.Location = new System.Drawing.Point(16, 26);
            this.NormalDifficultyRadioButton.Name = "NormalDifficultyRadioButton";
            this.NormalDifficultyRadioButton.Size = new System.Drawing.Size(74, 21);
            this.NormalDifficultyRadioButton.TabIndex = 21;
            this.NormalDifficultyRadioButton.TabStop = true;
            this.NormalDifficultyRadioButton.Text = "Normal";
            this.NormalDifficultyRadioButton.UseVisualStyleBackColor = true;
            this.NormalDifficultyRadioButton.CheckedChanged += new System.EventHandler(this.NormalDifficultyRadioButton_CheckedChanged);
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.LanguageDropDown);
            this.groupBox13.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.groupBox13.Location = new System.Drawing.Point(10, 23);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Size = new System.Drawing.Size(415, 57);
            this.groupBox13.TabIndex = 1;
            this.groupBox13.TabStop = false;
            this.groupBox13.Text = "Language";
            // 
            // LanguageDropDown
            // 
            this.LanguageDropDown.DisplayMember = "LocalName";
            this.LanguageDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LanguageDropDown.FormattingEnabled = true;
            this.LanguageDropDown.Location = new System.Drawing.Point(6, 26);
            this.LanguageDropDown.Name = "LanguageDropDown";
            this.LanguageDropDown.Size = new System.Drawing.Size(403, 24);
            this.LanguageDropDown.TabIndex = 20;
            this.LanguageDropDown.ValueMember = "Name";
            this.LanguageDropDown.SelectedIndexChanged += new System.EventHandler(this.LanguageDropDown_SelectedIndexChanged);
            // 
            // ReloadButton
            // 
            this.ReloadButton.Location = new System.Drawing.Point(117, 499);
            this.ReloadButton.Name = "ReloadButton";
            this.ReloadButton.Size = new System.Drawing.Size(90, 32);
            this.ReloadButton.TabIndex = 27;
            this.ReloadButton.Text = "Reload";
            this.ReloadButton.UseVisualStyleBackColor = true;
            this.ReloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
            // 
            // ModsGroupBox
            // 
            this.ModsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ModsGroupBox.Controls.Add(this.ModSelectionNoneButton);
            this.ModsGroupBox.Controls.Add(this.ModSelectionAllButton);
            this.ModsGroupBox.Controls.Add(this.ModSelectionBox);
            this.ModsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.ModsGroupBox.Location = new System.Drawing.Point(472, 2);
            this.ModsGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.ModsGroupBox.Name = "ModsGroupBox";
            this.ModsGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.ModsGroupBox.Size = new System.Drawing.Size(257, 491);
            this.ModsGroupBox.TabIndex = 13;
            this.ModsGroupBox.TabStop = false;
            this.ModsGroupBox.Text = "Mods";
            // 
            // ModSelectionNoneButton
            // 
            this.ModSelectionNoneButton.Location = new System.Drawing.Point(133, 448);
            this.ModSelectionNoneButton.Name = "ModSelectionNoneButton";
            this.ModSelectionNoneButton.Size = new System.Drawing.Size(115, 32);
            this.ModSelectionNoneButton.TabIndex = 12;
            this.ModSelectionNoneButton.Text = "None";
            this.ModSelectionNoneButton.UseVisualStyleBackColor = true;
            this.ModSelectionNoneButton.Click += new System.EventHandler(this.ModSelectionNoneButton_Click);
            // 
            // ModSelectionAllButton
            // 
            this.ModSelectionAllButton.Location = new System.Drawing.Point(8, 448);
            this.ModSelectionAllButton.Name = "ModSelectionAllButton";
            this.ModSelectionAllButton.Size = new System.Drawing.Size(115, 32);
            this.ModSelectionAllButton.TabIndex = 11;
            this.ModSelectionAllButton.Text = "All";
            this.ModSelectionAllButton.UseVisualStyleBackColor = true;
            this.ModSelectionAllButton.Click += new System.EventHandler(this.ModSelectionAllButton_Click);
            // 
            // ModSelectionBox
            // 
            this.ModSelectionBox.CheckOnClick = true;
            this.ModSelectionBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.ModSelectionBox.FormattingEnabled = true;
            this.ModSelectionBox.Location = new System.Drawing.Point(8, 27);
            this.ModSelectionBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.ModSelectionBox.Name = "ModSelectionBox";
            this.ModSelectionBox.Size = new System.Drawing.Size(240, 395);
            this.ModSelectionBox.TabIndex = 10;
            this.ModSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ModSelectionBox_ItemCheck);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 542);
            this.Controls.Add(this.ModsGroupBox);
            this.Controls.Add(this.ReloadButton);
            this.Controls.Add(this.CancelSettingsButton);
            this.Controls.Add(this.groupBox10);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.ConfirmButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.groupBox3.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.UseFactorioBaseOptionsGroup.ResumeLayout(false);
            this.groupBox14.ResumeLayout(false);
            this.groupBox14.PerformLayout();
            this.groupBox13.ResumeLayout(false);
            this.ModsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

		}

        #endregion
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.Button UserDataLocationBrowseButton;
        private System.Windows.Forms.TextBox UserDataLocationTextBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button InstallLocationBrowseButton;
        private System.Windows.Forms.TextBox InstallLocationTextBox;
        private System.Windows.Forms.Button CancelSettingsButton;
        private System.Windows.Forms.Button ConfirmButton;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.RadioButton UseForemanModRadioButton;
        private System.Windows.Forms.GroupBox UseFactorioBaseOptionsGroup;
        private System.Windows.Forms.RadioButton UseFactorioBaseRadioButton;
        private System.Windows.Forms.GroupBox groupBox13;
        private System.Windows.Forms.GroupBox groupBox14;
        private System.Windows.Forms.ComboBox LanguageDropDown;
        private System.Windows.Forms.RadioButton ExpensiveDifficultyRadioButton;
        private System.Windows.Forms.RadioButton NormalDifficultyRadioButton;
        private System.Windows.Forms.Button ReloadButton;
        private System.Windows.Forms.CheckBox IgnoreUserDataLocationCheckBox;
        private System.Windows.Forms.CheckBox UseSaveFileDataCheckBox;
        private System.Windows.Forms.GroupBox ModsGroupBox;
        private System.Windows.Forms.Button ModSelectionNoneButton;
        private System.Windows.Forms.Button ModSelectionAllButton;
        private CheckboxListWithErrors ModSelectionBox;
    }
}