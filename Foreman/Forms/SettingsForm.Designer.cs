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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ModSelectionNoneButton = new System.Windows.Forms.Button();
            this.ModSelectionAllButton = new System.Windows.Forms.Button();
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
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.IgnoreUserDataLocationCheckBox = new System.Windows.Forms.CheckBox();
            this.UserDataLocationBrowseButton = new System.Windows.Forms.Button();
            this.UserDataLocationTextBox = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.InstallLocationBrowseButton = new System.Windows.Forms.Button();
            this.InstallLocationTextBox = new System.Windows.Forms.TextBox();
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.UseForemanModRadioButton = new System.Windows.Forms.RadioButton();
            this.UseFactorioBaseRadioButton = new System.Windows.Forms.RadioButton();
            this.UseFactorioBaseOptionsGroup = new System.Windows.Forms.GroupBox();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.ExpensiveDifficultyRadioButton = new System.Windows.Forms.RadioButton();
            this.NormalDifficultyRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.LanguageDropDown = new System.Windows.Forms.ComboBox();
            this.ReloadButton = new System.Windows.Forms.Button();
            this.ModSelectionBox = new Foreman.CheckboxListWithErrors();
            this.groupBox1.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.UseFactorioBaseOptionsGroup.SuspendLayout();
            this.groupBox14.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.ModSelectionNoneButton);
            this.groupBox1.Controls.Add(this.ModSelectionAllButton);
            this.groupBox1.Controls.Add(this.ModSelectionBox);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.groupBox1.Location = new System.Drawing.Point(274, 21);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.groupBox1.Size = new System.Drawing.Size(257, 566);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mods";
            // 
            // ModSelectionNoneButton
            // 
            this.ModSelectionNoneButton.Location = new System.Drawing.Point(134, 534);
            this.ModSelectionNoneButton.Name = "ModSelectionNoneButton";
            this.ModSelectionNoneButton.Size = new System.Drawing.Size(115, 22);
            this.ModSelectionNoneButton.TabIndex = 12;
            this.ModSelectionNoneButton.Text = "None";
            this.ModSelectionNoneButton.UseVisualStyleBackColor = true;
            this.ModSelectionNoneButton.Click += new System.EventHandler(this.ModSelectionNoneButton_Click);
            // 
            // ModSelectionAllButton
            // 
            this.ModSelectionAllButton.Location = new System.Drawing.Point(8, 534);
            this.ModSelectionAllButton.Name = "ModSelectionAllButton";
            this.ModSelectionAllButton.Size = new System.Drawing.Size(115, 22);
            this.ModSelectionAllButton.TabIndex = 11;
            this.ModSelectionAllButton.Text = "All";
            this.ModSelectionAllButton.UseVisualStyleBackColor = true;
            this.ModSelectionAllButton.Click += new System.EventHandler(this.ModSelectionAllButton_Click);
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
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Controls.Add(this.groupBox6);
            this.groupBox2.Controls.Add(this.groupBox5);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.groupBox2.Location = new System.Drawing.Point(11, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(539, 595);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Enable / Disable Loaded Objects";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox9);
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.groupBox3.Location = new System.Drawing.Point(559, 6);
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
            this.ConfirmButton.Location = new System.Drawing.Point(559, 569);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(261, 32);
            this.ConfirmButton.TabIndex = 25;
            this.ConfirmButton.Text = "Confirm";
            this.ConfirmButton.UseVisualStyleBackColor = true;
            this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(922, 569);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(90, 32);
            this.CancelButton.TabIndex = 26;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.UseForemanModRadioButton);
            this.groupBox10.Controls.Add(this.UseFactorioBaseRadioButton);
            this.groupBox10.Controls.Add(this.UseFactorioBaseOptionsGroup);
            this.groupBox10.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.groupBox10.Location = new System.Drawing.Point(559, 207);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Size = new System.Drawing.Size(453, 275);
            this.groupBox10.TabIndex = 19;
            this.groupBox10.TabStop = false;
            this.groupBox10.Text = "Factorio Options:";
            // 
            // UseForemanModRadioButton
            // 
            this.UseForemanModRadioButton.AutoSize = true;
            this.UseForemanModRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.UseForemanModRadioButton.Location = new System.Drawing.Point(17, 35);
            this.UseForemanModRadioButton.Name = "UseForemanModRadioButton";
            this.UseForemanModRadioButton.Size = new System.Drawing.Size(271, 21);
            this.UseForemanModRadioButton.TabIndex = 19;
            this.UseForemanModRadioButton.TabStop = true;
            this.UseForemanModRadioButton.Text = "Use Foreman mod to generate objects";
            this.UseForemanModRadioButton.UseVisualStyleBackColor = true;
            // 
            // UseFactorioBaseRadioButton
            // 
            this.UseFactorioBaseRadioButton.AutoSize = true;
            this.UseFactorioBaseRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.UseFactorioBaseRadioButton.Location = new System.Drawing.Point(17, 68);
            this.UseFactorioBaseRadioButton.Name = "UseFactorioBaseRadioButton";
            this.UseFactorioBaseRadioButton.Size = new System.Drawing.Size(286, 21);
            this.UseFactorioBaseRadioButton.TabIndex = 19;
            this.UseFactorioBaseRadioButton.TabStop = true;
            this.UseFactorioBaseRadioButton.Text = "Auto generate objects from factorio base";
            this.UseFactorioBaseRadioButton.UseVisualStyleBackColor = true;
            this.UseFactorioBaseRadioButton.CheckedChanged += new System.EventHandler(this.UseFactorioBaseRadioButton_CheckedChanged);
            // 
            // UseFactorioBaseOptionsGroup
            // 
            this.UseFactorioBaseOptionsGroup.Controls.Add(this.groupBox14);
            this.UseFactorioBaseOptionsGroup.Controls.Add(this.groupBox13);
            this.UseFactorioBaseOptionsGroup.Location = new System.Drawing.Point(10, 72);
            this.UseFactorioBaseOptionsGroup.Name = "UseFactorioBaseOptionsGroup";
            this.UseFactorioBaseOptionsGroup.Size = new System.Drawing.Size(434, 190);
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
            this.groupBox14.Size = new System.Drawing.Size(415, 90);
            this.groupBox14.TabIndex = 2;
            this.groupBox14.TabStop = false;
            this.groupBox14.Text = "Difficulty";
            // 
            // ExpensiveDifficultyRadioButton
            // 
            this.ExpensiveDifficultyRadioButton.AutoSize = true;
            this.ExpensiveDifficultyRadioButton.Location = new System.Drawing.Point(16, 58);
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
            this.ReloadButton.Location = new System.Drawing.Point(826, 569);
            this.ReloadButton.Name = "ReloadButton";
            this.ReloadButton.Size = new System.Drawing.Size(90, 32);
            this.ReloadButton.TabIndex = 27;
            this.ReloadButton.Text = "Reload";
            this.ReloadButton.UseVisualStyleBackColor = true;
            this.ReloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
            // 
            // ModSelectionBox
            // 
            this.ModSelectionBox.CheckOnClick = true;
            this.ModSelectionBox.FormattingEnabled = true;
            this.ModSelectionBox.Location = new System.Drawing.Point(9, 19);
            this.ModSelectionBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.ModSelectionBox.Name = "ModSelectionBox";
            this.ModSelectionBox.Size = new System.Drawing.Size(240, 514);
            this.ModSelectionBox.TabIndex = 10;
            this.ModSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ModSelectionBox_ItemCheck);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1022, 609);
            this.Controls.Add(this.ReloadButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.groupBox10);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.ConfirmButton);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
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
            this.ResumeLayout(false);

		}

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private CheckboxListWithErrors ModSelectionBox;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.CheckedListBox ModuleSelectionBox;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckedListBox MinerSelectionBox;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckedListBox AssemblerSelectionBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.Button UserDataLocationBrowseButton;
        private System.Windows.Forms.TextBox UserDataLocationTextBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button InstallLocationBrowseButton;
        private System.Windows.Forms.TextBox InstallLocationTextBox;
        private System.Windows.Forms.Button CancelButton;
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
        private System.Windows.Forms.Button ModSelectionNoneButton;
        private System.Windows.Forms.Button ModSelectionAllButton;
        private System.Windows.Forms.Button ModuleSelectionNoneButton;
        private System.Windows.Forms.Button ModuleSelectionAllButton;
        private System.Windows.Forms.Button MinerSelectionNoneButton;
        private System.Windows.Forms.Button MinerSelectionAllButton;
        private System.Windows.Forms.Button AssemblerSelectionNoneButton;
        private System.Windows.Forms.Button AssemblerSelectionAllButton;
        private System.Windows.Forms.Button ReloadButton;
        private System.Windows.Forms.CheckBox IgnoreUserDataLocationCheckBox;
    }
}