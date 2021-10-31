namespace Foreman
{
	partial class ImportPresetForm
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
            this.BrowseButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.FactorioLocationComboBox = new System.Windows.Forms.ComboBox();
            this.FactorioLocationGroup = new System.Windows.Forms.GroupBox();
            this.FactorioSettingsGroup = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TechnologyDifficultyGroup = new System.Windows.Forms.GroupBox();
            this.ExpensiveTechnologyRButton = new System.Windows.Forms.RadioButton();
            this.NormalTechnologyRButton = new System.Windows.Forms.RadioButton();
            this.RecipeDifficultyGroup = new System.Windows.Forms.GroupBox();
            this.ExpensiveRecipeRButton = new System.Windows.Forms.RadioButton();
            this.NormalRecipeRButton = new System.Windows.Forms.RadioButton();
            this.CancelButton = new System.Windows.Forms.Button();
            this.PresetNameGroup = new System.Windows.Forms.GroupBox();
            this.PresetNameTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ImportProgressBar = new Foreman.CustomProgressBar();
            this.FactorioLocationGroup.SuspendLayout();
            this.FactorioSettingsGroup.SuspendLayout();
            this.TechnologyDifficultyGroup.SuspendLayout();
            this.RecipeDifficultyGroup.SuspendLayout();
            this.PresetNameGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(510, 19);
            this.BrowseButton.Margin = new System.Windows.Forms.Padding(4);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(83, 28);
            this.BrowseButton.TabIndex = 1;
            this.BrowseButton.Text = "Browse...";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(12, 241);
            this.OKButton.Margin = new System.Windows.Forms.Padding(4);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(413, 32);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "Import";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // FactorioLocationComboBox
            // 
            this.FactorioLocationComboBox.FormattingEnabled = true;
            this.FactorioLocationComboBox.Location = new System.Drawing.Point(6, 21);
            this.FactorioLocationComboBox.Name = "FactorioLocationComboBox";
            this.FactorioLocationComboBox.Size = new System.Drawing.Size(497, 24);
            this.FactorioLocationComboBox.TabIndex = 3;
            // 
            // FactorioLocationGroup
            // 
            this.FactorioLocationGroup.Controls.Add(this.FactorioLocationComboBox);
            this.FactorioLocationGroup.Controls.Add(this.BrowseButton);
            this.FactorioLocationGroup.Location = new System.Drawing.Point(12, 12);
            this.FactorioLocationGroup.Name = "FactorioLocationGroup";
            this.FactorioLocationGroup.Size = new System.Drawing.Size(600, 55);
            this.FactorioLocationGroup.TabIndex = 4;
            this.FactorioLocationGroup.TabStop = false;
            this.FactorioLocationGroup.Text = "Factorio Location:";
            // 
            // FactorioSettingsGroup
            // 
            this.FactorioSettingsGroup.Controls.Add(this.label2);
            this.FactorioSettingsGroup.Controls.Add(this.label1);
            this.FactorioSettingsGroup.Controls.Add(this.TechnologyDifficultyGroup);
            this.FactorioSettingsGroup.Controls.Add(this.RecipeDifficultyGroup);
            this.FactorioSettingsGroup.Location = new System.Drawing.Point(13, 74);
            this.FactorioSettingsGroup.Name = "FactorioSettingsGroup";
            this.FactorioSettingsGroup.Size = new System.Drawing.Size(599, 101);
            this.FactorioSettingsGroup.TabIndex = 5;
            this.FactorioSettingsGroup.TabStop = false;
            this.FactorioSettingsGroup.Text = "Factorio Settings:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(68, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(450, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Language, Active Mods, and Mod options are to be set within Factorio!";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "NOTE:";
            // 
            // TechnologyDifficultyGroup
            // 
            this.TechnologyDifficultyGroup.Controls.Add(this.ExpensiveTechnologyRButton);
            this.TechnologyDifficultyGroup.Controls.Add(this.NormalTechnologyRButton);
            this.TechnologyDifficultyGroup.Location = new System.Drawing.Point(317, 22);
            this.TechnologyDifficultyGroup.Name = "TechnologyDifficultyGroup";
            this.TechnologyDifficultyGroup.Size = new System.Drawing.Size(275, 49);
            this.TechnologyDifficultyGroup.TabIndex = 2;
            this.TechnologyDifficultyGroup.TabStop = false;
            this.TechnologyDifficultyGroup.Text = "Technology Difficulty:";
            // 
            // ExpensiveTechnologyRButton
            // 
            this.ExpensiveTechnologyRButton.AutoSize = true;
            this.ExpensiveTechnologyRButton.Location = new System.Drawing.Point(123, 21);
            this.ExpensiveTechnologyRButton.Name = "ExpensiveTechnologyRButton";
            this.ExpensiveTechnologyRButton.Size = new System.Drawing.Size(93, 21);
            this.ExpensiveTechnologyRButton.TabIndex = 1;
            this.ExpensiveTechnologyRButton.Text = "Expensive";
            this.ExpensiveTechnologyRButton.UseVisualStyleBackColor = true;
            // 
            // NormalTechnologyRButton
            // 
            this.NormalTechnologyRButton.AutoSize = true;
            this.NormalTechnologyRButton.Checked = true;
            this.NormalTechnologyRButton.Location = new System.Drawing.Point(6, 21);
            this.NormalTechnologyRButton.Name = "NormalTechnologyRButton";
            this.NormalTechnologyRButton.Size = new System.Drawing.Size(74, 21);
            this.NormalTechnologyRButton.TabIndex = 0;
            this.NormalTechnologyRButton.TabStop = true;
            this.NormalTechnologyRButton.Text = "Normal";
            this.NormalTechnologyRButton.UseVisualStyleBackColor = true;
            // 
            // RecipeDifficultyGroup
            // 
            this.RecipeDifficultyGroup.Controls.Add(this.ExpensiveRecipeRButton);
            this.RecipeDifficultyGroup.Controls.Add(this.NormalRecipeRButton);
            this.RecipeDifficultyGroup.Location = new System.Drawing.Point(7, 22);
            this.RecipeDifficultyGroup.Name = "RecipeDifficultyGroup";
            this.RecipeDifficultyGroup.Size = new System.Drawing.Size(275, 49);
            this.RecipeDifficultyGroup.TabIndex = 0;
            this.RecipeDifficultyGroup.TabStop = false;
            this.RecipeDifficultyGroup.Text = "Recipe Difficulty:";
            // 
            // ExpensiveRecipeRButton
            // 
            this.ExpensiveRecipeRButton.AutoSize = true;
            this.ExpensiveRecipeRButton.Location = new System.Drawing.Point(123, 21);
            this.ExpensiveRecipeRButton.Name = "ExpensiveRecipeRButton";
            this.ExpensiveRecipeRButton.Size = new System.Drawing.Size(93, 21);
            this.ExpensiveRecipeRButton.TabIndex = 1;
            this.ExpensiveRecipeRButton.Text = "Expensive";
            this.ExpensiveRecipeRButton.UseVisualStyleBackColor = true;
            // 
            // NormalRecipeRButton
            // 
            this.NormalRecipeRButton.AutoSize = true;
            this.NormalRecipeRButton.Checked = true;
            this.NormalRecipeRButton.Location = new System.Drawing.Point(6, 21);
            this.NormalRecipeRButton.Name = "NormalRecipeRButton";
            this.NormalRecipeRButton.Size = new System.Drawing.Size(74, 21);
            this.NormalRecipeRButton.TabIndex = 0;
            this.NormalRecipeRButton.TabStop = true;
            this.NormalRecipeRButton.Text = "Normal";
            this.NormalRecipeRButton.UseVisualStyleBackColor = true;
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(435, 241);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(177, 32);
            this.CancelButton.TabIndex = 6;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // PresetNameGroup
            // 
            this.PresetNameGroup.Controls.Add(this.label3);
            this.PresetNameGroup.Controls.Add(this.PresetNameTextBox);
            this.PresetNameGroup.Location = new System.Drawing.Point(12, 181);
            this.PresetNameGroup.Name = "PresetNameGroup";
            this.PresetNameGroup.Size = new System.Drawing.Size(600, 53);
            this.PresetNameGroup.TabIndex = 7;
            this.PresetNameGroup.TabStop = false;
            this.PresetNameGroup.Text = "Preset Name:";
            // 
            // PresetNameTextBox
            // 
            this.PresetNameTextBox.BackColor = System.Drawing.Color.Moccasin;
            this.PresetNameTextBox.Location = new System.Drawing.Point(7, 22);
            this.PresetNameTextBox.MaxLength = 30;
            this.PresetNameTextBox.Name = "PresetNameTextBox";
            this.PresetNameTextBox.Size = new System.Drawing.Size(183, 22);
            this.PresetNameTextBox.TabIndex = 0;
            this.PresetNameTextBox.TextChanged += new System.EventHandler(this.PresetNameTextBox_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.label3.Location = new System.Drawing.Point(196, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(397, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "(5-30 characters: letters, numbers, space, brackets, dash or underscore)";
            // 
            // ImportProgressBar
            // 
            this.ImportProgressBar.CustomText = null;
            this.ImportProgressBar.Location = new System.Drawing.Point(13, 241);
            this.ImportProgressBar.Name = "ImportProgressBar";
            this.ImportProgressBar.Size = new System.Drawing.Size(599, 32);
            this.ImportProgressBar.TabIndex = 5;
            this.ImportProgressBar.Visible = false;
            // 
            // ImportPresetForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 288);
            this.Controls.Add(this.PresetNameGroup);
            this.Controls.Add(this.ImportProgressBar);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.FactorioSettingsGroup);
            this.Controls.Add(this.FactorioLocationGroup);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "ImportPresetForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Importing Preset";
            this.FactorioLocationGroup.ResumeLayout(false);
            this.FactorioSettingsGroup.ResumeLayout(false);
            this.FactorioSettingsGroup.PerformLayout();
            this.TechnologyDifficultyGroup.ResumeLayout(false);
            this.TechnologyDifficultyGroup.PerformLayout();
            this.RecipeDifficultyGroup.ResumeLayout(false);
            this.RecipeDifficultyGroup.PerformLayout();
            this.PresetNameGroup.ResumeLayout(false);
            this.PresetNameGroup.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button BrowseButton;
		private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.ComboBox FactorioLocationComboBox;
        private System.Windows.Forms.GroupBox FactorioLocationGroup;
        private System.Windows.Forms.GroupBox FactorioSettingsGroup;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox TechnologyDifficultyGroup;
        private System.Windows.Forms.RadioButton ExpensiveTechnologyRButton;
        private System.Windows.Forms.RadioButton NormalTechnologyRButton;
        private System.Windows.Forms.GroupBox RecipeDifficultyGroup;
        private System.Windows.Forms.RadioButton ExpensiveRecipeRButton;
        private System.Windows.Forms.RadioButton NormalRecipeRButton;
        private CustomProgressBar ImportProgressBar;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.GroupBox PresetNameGroup;
        private System.Windows.Forms.TextBox PresetNameTextBox;
        private System.Windows.Forms.Label label3;
    }
}