namespace Foreman
{
	partial class PresetImportForm
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
			this.FactorioBrowseButton = new System.Windows.Forms.Button();
			this.FactorioLocationComboBox = new System.Windows.Forms.ComboBox();
			this.FactorioLocationGroup = new System.Windows.Forms.GroupBox();
			this.FactorioLocationTable = new System.Windows.Forms.TableLayoutPanel();
			this.FactorioSettingsGroup = new System.Windows.Forms.GroupBox();
			this.FactorioSettingsTable = new System.Windows.Forms.TableLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.RecipeDifficultyGroup = new System.Windows.Forms.GroupBox();
			this.ExpensiveRecipeRButton = new System.Windows.Forms.RadioButton();
			this.NormalRecipeRButton = new System.Windows.Forms.RadioButton();
			this.TechnologyDifficultyGroup = new System.Windows.Forms.GroupBox();
			this.ExpensiveTechnologyRButton = new System.Windows.Forms.RadioButton();
			this.NormalTechnologyRButton = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.PresetNameGroup = new System.Windows.Forms.GroupBox();
			this.PresetNameTable = new System.Windows.Forms.TableLayoutPanel();
			this.label4 = new System.Windows.Forms.Label();
			this.PresetNameTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.MainTable = new System.Windows.Forms.TableLayoutPanel();
			this.CompatibilityModeCheckBox = new System.Windows.Forms.CheckBox();
			this.FactorioModLocationGroup = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.ModsBrowseButton = new System.Windows.Forms.Button();
			this.ModsLocationComboBox = new System.Windows.Forms.ComboBox();
			this.OKButton = new System.Windows.Forms.Button();
			this.CancelImportButtonB = new System.Windows.Forms.Button();
			this.ImportProgressBar = new Foreman.CustomProgressBar();
			this.CancelImportButton = new System.Windows.Forms.Button();
			this.FactorioLocationGroup.SuspendLayout();
			this.FactorioLocationTable.SuspendLayout();
			this.FactorioSettingsGroup.SuspendLayout();
			this.FactorioSettingsTable.SuspendLayout();
			this.RecipeDifficultyGroup.SuspendLayout();
			this.TechnologyDifficultyGroup.SuspendLayout();
			this.PresetNameGroup.SuspendLayout();
			this.PresetNameTable.SuspendLayout();
			this.MainTable.SuspendLayout();
			this.FactorioModLocationGroup.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// FactorioBrowseButton
			// 
			this.FactorioBrowseButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FactorioBrowseButton.Location = new System.Drawing.Point(385, 3);
			this.FactorioBrowseButton.Name = "FactorioBrowseButton";
			this.FactorioBrowseButton.Size = new System.Drawing.Size(62, 23);
			this.FactorioBrowseButton.TabIndex = 1;
			this.FactorioBrowseButton.Text = "Browse...";
			this.FactorioBrowseButton.UseVisualStyleBackColor = true;
			this.FactorioBrowseButton.Click += new System.EventHandler(this.FactorioBrowseButton_Click);
			// 
			// FactorioLocationComboBox
			// 
			this.FactorioLocationComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FactorioLocationComboBox.FormattingEnabled = true;
			this.FactorioLocationComboBox.Location = new System.Drawing.Point(3, 3);
			this.FactorioLocationComboBox.Name = "FactorioLocationComboBox";
			this.FactorioLocationComboBox.Size = new System.Drawing.Size(376, 21);
			this.FactorioLocationComboBox.TabIndex = 3;
			// 
			// FactorioLocationGroup
			// 
			this.FactorioLocationGroup.AutoSize = true;
			this.FactorioLocationGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTable.SetColumnSpan(this.FactorioLocationGroup, 2);
			this.FactorioLocationGroup.Controls.Add(this.FactorioLocationTable);
			this.FactorioLocationGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FactorioLocationGroup.Location = new System.Drawing.Point(2, 2);
			this.FactorioLocationGroup.Margin = new System.Windows.Forms.Padding(2);
			this.FactorioLocationGroup.Name = "FactorioLocationGroup";
			this.FactorioLocationGroup.Padding = new System.Windows.Forms.Padding(2);
			this.FactorioLocationGroup.Size = new System.Drawing.Size(454, 46);
			this.FactorioLocationGroup.TabIndex = 4;
			this.FactorioLocationGroup.TabStop = false;
			this.FactorioLocationGroup.Text = "Factorio Location:";
			// 
			// FactorioLocationTable
			// 
			this.FactorioLocationTable.AutoSize = true;
			this.FactorioLocationTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.FactorioLocationTable.ColumnCount = 2;
			this.FactorioLocationTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.FactorioLocationTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.FactorioLocationTable.Controls.Add(this.FactorioBrowseButton, 1, 0);
			this.FactorioLocationTable.Controls.Add(this.FactorioLocationComboBox, 0, 0);
			this.FactorioLocationTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FactorioLocationTable.Location = new System.Drawing.Point(2, 15);
			this.FactorioLocationTable.Name = "FactorioLocationTable";
			this.FactorioLocationTable.RowCount = 1;
			this.FactorioLocationTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.FactorioLocationTable.Size = new System.Drawing.Size(450, 29);
			this.FactorioLocationTable.TabIndex = 0;
			// 
			// FactorioSettingsGroup
			// 
			this.MainTable.SetColumnSpan(this.FactorioSettingsGroup, 2);
			this.FactorioSettingsGroup.Controls.Add(this.FactorioSettingsTable);
			this.FactorioSettingsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FactorioSettingsGroup.Location = new System.Drawing.Point(2, 102);
			this.FactorioSettingsGroup.Margin = new System.Windows.Forms.Padding(2);
			this.FactorioSettingsGroup.Name = "FactorioSettingsGroup";
			this.FactorioSettingsGroup.Padding = new System.Windows.Forms.Padding(2);
			this.FactorioSettingsGroup.Size = new System.Drawing.Size(454, 82);
			this.FactorioSettingsGroup.TabIndex = 5;
			this.FactorioSettingsGroup.TabStop = false;
			this.FactorioSettingsGroup.Text = "Factorio Settings:";
			// 
			// FactorioSettingsTable
			// 
			this.FactorioSettingsTable.AutoSize = true;
			this.FactorioSettingsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.FactorioSettingsTable.ColumnCount = 3;
			this.FactorioSettingsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.FactorioSettingsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.FactorioSettingsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.FactorioSettingsTable.Controls.Add(this.label2, 1, 1);
			this.FactorioSettingsTable.Controls.Add(this.RecipeDifficultyGroup, 0, 0);
			this.FactorioSettingsTable.Controls.Add(this.TechnologyDifficultyGroup, 2, 0);
			this.FactorioSettingsTable.Controls.Add(this.label1, 0, 1);
			this.FactorioSettingsTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FactorioSettingsTable.Location = new System.Drawing.Point(2, 15);
			this.FactorioSettingsTable.Name = "FactorioSettingsTable";
			this.FactorioSettingsTable.RowCount = 2;
			this.FactorioSettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.FactorioSettingsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.FactorioSettingsTable.Size = new System.Drawing.Size(450, 65);
			this.FactorioSettingsTable.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.FactorioSettingsTable.SetColumnSpan(this.label2, 2);
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(51, 44);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(397, 21);
			this.label2.TabIndex = 4;
			this.label2.Text = "Language, Active Mods, and Mod options are to be set within Factorio!";
			// 
			// RecipeDifficultyGroup
			// 
			this.FactorioSettingsTable.SetColumnSpan(this.RecipeDifficultyGroup, 2);
			this.RecipeDifficultyGroup.Controls.Add(this.ExpensiveRecipeRButton);
			this.RecipeDifficultyGroup.Controls.Add(this.NormalRecipeRButton);
			this.RecipeDifficultyGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipeDifficultyGroup.Location = new System.Drawing.Point(2, 2);
			this.RecipeDifficultyGroup.Margin = new System.Windows.Forms.Padding(2);
			this.RecipeDifficultyGroup.Name = "RecipeDifficultyGroup";
			this.RecipeDifficultyGroup.Padding = new System.Windows.Forms.Padding(2);
			this.RecipeDifficultyGroup.Size = new System.Drawing.Size(206, 40);
			this.RecipeDifficultyGroup.TabIndex = 0;
			this.RecipeDifficultyGroup.TabStop = false;
			this.RecipeDifficultyGroup.Text = "Recipe Difficulty:";
			// 
			// ExpensiveRecipeRButton
			// 
			this.ExpensiveRecipeRButton.AutoSize = true;
			this.ExpensiveRecipeRButton.Location = new System.Drawing.Point(92, 17);
			this.ExpensiveRecipeRButton.Margin = new System.Windows.Forms.Padding(2);
			this.ExpensiveRecipeRButton.Name = "ExpensiveRecipeRButton";
			this.ExpensiveRecipeRButton.Size = new System.Drawing.Size(74, 17);
			this.ExpensiveRecipeRButton.TabIndex = 1;
			this.ExpensiveRecipeRButton.Text = "Expensive";
			this.ExpensiveRecipeRButton.UseVisualStyleBackColor = true;
			// 
			// NormalRecipeRButton
			// 
			this.NormalRecipeRButton.AutoSize = true;
			this.NormalRecipeRButton.Checked = true;
			this.NormalRecipeRButton.Location = new System.Drawing.Point(4, 17);
			this.NormalRecipeRButton.Margin = new System.Windows.Forms.Padding(2);
			this.NormalRecipeRButton.Name = "NormalRecipeRButton";
			this.NormalRecipeRButton.Size = new System.Drawing.Size(58, 17);
			this.NormalRecipeRButton.TabIndex = 0;
			this.NormalRecipeRButton.TabStop = true;
			this.NormalRecipeRButton.Text = "Normal";
			this.NormalRecipeRButton.UseVisualStyleBackColor = true;
			// 
			// TechnologyDifficultyGroup
			// 
			this.TechnologyDifficultyGroup.Controls.Add(this.ExpensiveTechnologyRButton);
			this.TechnologyDifficultyGroup.Controls.Add(this.NormalTechnologyRButton);
			this.TechnologyDifficultyGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TechnologyDifficultyGroup.Location = new System.Drawing.Point(212, 2);
			this.TechnologyDifficultyGroup.Margin = new System.Windows.Forms.Padding(2);
			this.TechnologyDifficultyGroup.Name = "TechnologyDifficultyGroup";
			this.TechnologyDifficultyGroup.Padding = new System.Windows.Forms.Padding(2);
			this.TechnologyDifficultyGroup.Size = new System.Drawing.Size(236, 40);
			this.TechnologyDifficultyGroup.TabIndex = 2;
			this.TechnologyDifficultyGroup.TabStop = false;
			this.TechnologyDifficultyGroup.Text = "Technology Difficulty:";
			// 
			// ExpensiveTechnologyRButton
			// 
			this.ExpensiveTechnologyRButton.AutoSize = true;
			this.ExpensiveTechnologyRButton.Location = new System.Drawing.Point(92, 17);
			this.ExpensiveTechnologyRButton.Margin = new System.Windows.Forms.Padding(2);
			this.ExpensiveTechnologyRButton.Name = "ExpensiveTechnologyRButton";
			this.ExpensiveTechnologyRButton.Size = new System.Drawing.Size(74, 17);
			this.ExpensiveTechnologyRButton.TabIndex = 1;
			this.ExpensiveTechnologyRButton.Text = "Expensive";
			this.ExpensiveTechnologyRButton.UseVisualStyleBackColor = true;
			// 
			// NormalTechnologyRButton
			// 
			this.NormalTechnologyRButton.AutoSize = true;
			this.NormalTechnologyRButton.Checked = true;
			this.NormalTechnologyRButton.Location = new System.Drawing.Point(4, 17);
			this.NormalTechnologyRButton.Margin = new System.Windows.Forms.Padding(2);
			this.NormalTechnologyRButton.Name = "NormalTechnologyRButton";
			this.NormalTechnologyRButton.Size = new System.Drawing.Size(58, 17);
			this.NormalTechnologyRButton.TabIndex = 0;
			this.NormalTechnologyRButton.TabStop = true;
			this.NormalTechnologyRButton.Text = "Normal";
			this.NormalTechnologyRButton.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(2, 44);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(45, 21);
			this.label1.TabIndex = 3;
			this.label1.Text = "NOTE:";
			// 
			// PresetNameGroup
			// 
			this.PresetNameGroup.AutoSize = true;
			this.PresetNameGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTable.SetColumnSpan(this.PresetNameGroup, 2);
			this.PresetNameGroup.Controls.Add(this.PresetNameTable);
			this.PresetNameGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PresetNameGroup.Location = new System.Drawing.Point(2, 188);
			this.PresetNameGroup.Margin = new System.Windows.Forms.Padding(2);
			this.PresetNameGroup.Name = "PresetNameGroup";
			this.PresetNameGroup.Padding = new System.Windows.Forms.Padding(5);
			this.PresetNameGroup.Size = new System.Drawing.Size(454, 49);
			this.PresetNameGroup.TabIndex = 7;
			this.PresetNameGroup.TabStop = false;
			this.PresetNameGroup.Text = "Preset Name:";
			// 
			// PresetNameTable
			// 
			this.PresetNameTable.AutoSize = true;
			this.PresetNameTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.PresetNameTable.ColumnCount = 2;
			this.PresetNameTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.PresetNameTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.PresetNameTable.Controls.Add(this.label4, 1, 1);
			this.PresetNameTable.Controls.Add(this.PresetNameTextBox, 0, 0);
			this.PresetNameTable.Controls.Add(this.label3, 1, 0);
			this.PresetNameTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PresetNameTable.Location = new System.Drawing.Point(5, 18);
			this.PresetNameTable.Name = "PresetNameTable";
			this.PresetNameTable.RowCount = 2;
			this.PresetNameTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.PresetNameTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.PresetNameTable.Size = new System.Drawing.Size(444, 26);
			this.PresetNameTable.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(278, 13);
			this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(164, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "brackets, dash or underscore.";
			// 
			// PresetNameTextBox
			// 
			this.PresetNameTextBox.BackColor = System.Drawing.Color.Moccasin;
			this.PresetNameTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.PresetNameTextBox.Location = new System.Drawing.Point(2, 2);
			this.PresetNameTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.PresetNameTextBox.MaxLength = 40;
			this.PresetNameTextBox.Name = "PresetNameTextBox";
			this.PresetNameTable.SetRowSpan(this.PresetNameTextBox, 2);
			this.PresetNameTextBox.Size = new System.Drawing.Size(269, 20);
			this.PresetNameTextBox.TabIndex = 0;
			this.PresetNameTextBox.TextChanged += new System.EventHandler(this.PresetNameTextBox_TextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(278, 0);
			this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(164, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "5-40 characters: letters, numbers,";
			// 
			// MainTable
			// 
			this.MainTable.AutoSize = true;
			this.MainTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTable.ColumnCount = 2;
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MainTable.Controls.Add(this.CompatibilityModeCheckBox, 0, 4);
			this.MainTable.Controls.Add(this.FactorioModLocationGroup, 0, 1);
			this.MainTable.Controls.Add(this.OKButton, 0, 5);
			this.MainTable.Controls.Add(this.CancelImportButtonB, 1, 6);
			this.MainTable.Controls.Add(this.ImportProgressBar, 0, 6);
			this.MainTable.Controls.Add(this.CancelImportButton, 1, 5);
			this.MainTable.Controls.Add(this.FactorioLocationGroup, 0, 0);
			this.MainTable.Controls.Add(this.FactorioSettingsGroup, 0, 2);
			this.MainTable.Controls.Add(this.PresetNameGroup, 0, 3);
			this.MainTable.Location = new System.Drawing.Point(0, 0);
			this.MainTable.Name = "MainTable";
			this.MainTable.RowCount = 7;
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.Size = new System.Drawing.Size(458, 320);
			this.MainTable.TabIndex = 8;
			// 
			// CompatibilityModeCheckBox
			// 
			this.CompatibilityModeCheckBox.AutoSize = true;
			this.MainTable.SetColumnSpan(this.CompatibilityModeCheckBox, 2);
			this.CompatibilityModeCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompatibilityModeCheckBox.Location = new System.Drawing.Point(10, 242);
			this.CompatibilityModeCheckBox.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.CompatibilityModeCheckBox.Name = "CompatibilityModeCheckBox";
			this.CompatibilityModeCheckBox.Size = new System.Drawing.Size(445, 17);
			this.CompatibilityModeCheckBox.TabIndex = 9;
			this.CompatibilityModeCheckBox.Text = "Compatibility Mode (use only if regular import fails)";
			this.CompatibilityModeCheckBox.UseVisualStyleBackColor = true;
			// 
			// FactorioModLocationGroup
			// 
			this.FactorioModLocationGroup.AutoSize = true;
			this.FactorioModLocationGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTable.SetColumnSpan(this.FactorioModLocationGroup, 2);
			this.FactorioModLocationGroup.Controls.Add(this.tableLayoutPanel1);
			this.FactorioModLocationGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FactorioModLocationGroup.Location = new System.Drawing.Point(2, 52);
			this.FactorioModLocationGroup.Margin = new System.Windows.Forms.Padding(2);
			this.FactorioModLocationGroup.Name = "FactorioModLocationGroup";
			this.FactorioModLocationGroup.Padding = new System.Windows.Forms.Padding(2);
			this.FactorioModLocationGroup.Size = new System.Drawing.Size(454, 46);
			this.FactorioModLocationGroup.TabIndex = 9;
			this.FactorioModLocationGroup.TabStop = false;
			this.FactorioModLocationGroup.Text = "Mod Folder Location (leave blank for auto-detect):";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.ModsBrowseButton, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.ModsLocationComboBox, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 15);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(450, 29);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// ModsBrowseButton
			// 
			this.ModsBrowseButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModsBrowseButton.Location = new System.Drawing.Point(385, 3);
			this.ModsBrowseButton.Name = "ModsBrowseButton";
			this.ModsBrowseButton.Size = new System.Drawing.Size(62, 23);
			this.ModsBrowseButton.TabIndex = 1;
			this.ModsBrowseButton.Text = "Browse...";
			this.ModsBrowseButton.UseVisualStyleBackColor = true;
			this.ModsBrowseButton.Click += new System.EventHandler(this.ModsBrowseButton_Click);
			// 
			// ModsLocationComboBox
			// 
			this.ModsLocationComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModsLocationComboBox.FormattingEnabled = true;
			this.ModsLocationComboBox.Location = new System.Drawing.Point(3, 3);
			this.ModsLocationComboBox.Name = "ModsLocationComboBox";
			this.ModsLocationComboBox.Size = new System.Drawing.Size(376, 21);
			this.ModsLocationComboBox.TabIndex = 3;
			// 
			// OKButton
			// 
			this.OKButton.AutoSize = true;
			this.OKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.OKButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OKButton.Location = new System.Drawing.Point(3, 265);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(396, 23);
			this.OKButton.TabIndex = 2;
			this.OKButton.Text = "Import";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelImportButtonB
			// 
			this.CancelImportButtonB.AutoSize = true;
			this.CancelImportButtonB.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelImportButtonB.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelImportButtonB.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CancelImportButtonB.Location = new System.Drawing.Point(405, 294);
			this.CancelImportButtonB.Name = "CancelImportButtonB";
			this.CancelImportButtonB.Size = new System.Drawing.Size(50, 23);
			this.CancelImportButtonB.TabIndex = 8;
			this.CancelImportButtonB.Text = "Cancel";
			this.CancelImportButtonB.UseVisualStyleBackColor = true;
			this.CancelImportButtonB.Visible = false;
			this.CancelImportButtonB.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ImportProgressBar
			// 
			this.ImportProgressBar.CustomText = null;
			this.ImportProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportProgressBar.Location = new System.Drawing.Point(4, 295);
			this.ImportProgressBar.Margin = new System.Windows.Forms.Padding(4);
			this.ImportProgressBar.Name = "ImportProgressBar";
			this.ImportProgressBar.Size = new System.Drawing.Size(394, 21);
			this.ImportProgressBar.TabIndex = 5;
			this.ImportProgressBar.Visible = false;
			// 
			// CancelImportButton
			// 
			this.CancelImportButton.AutoSize = true;
			this.CancelImportButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelImportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelImportButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CancelImportButton.Location = new System.Drawing.Point(405, 265);
			this.CancelImportButton.Name = "CancelImportButton";
			this.CancelImportButton.Size = new System.Drawing.Size(50, 23);
			this.CancelImportButton.TabIndex = 6;
			this.CancelImportButton.Text = "Cancel";
			this.CancelImportButton.UseVisualStyleBackColor = true;
			this.CancelImportButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// PresetImportForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.CancelImportButton;
			this.ClientSize = new System.Drawing.Size(466, 332);
			this.Controls.Add(this.MainTable);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.Name = "PresetImportForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Import Preset";
			this.FactorioLocationGroup.ResumeLayout(false);
			this.FactorioLocationGroup.PerformLayout();
			this.FactorioLocationTable.ResumeLayout(false);
			this.FactorioSettingsGroup.ResumeLayout(false);
			this.FactorioSettingsGroup.PerformLayout();
			this.FactorioSettingsTable.ResumeLayout(false);
			this.FactorioSettingsTable.PerformLayout();
			this.RecipeDifficultyGroup.ResumeLayout(false);
			this.RecipeDifficultyGroup.PerformLayout();
			this.TechnologyDifficultyGroup.ResumeLayout(false);
			this.TechnologyDifficultyGroup.PerformLayout();
			this.PresetNameGroup.ResumeLayout(false);
			this.PresetNameGroup.PerformLayout();
			this.PresetNameTable.ResumeLayout(false);
			this.PresetNameTable.PerformLayout();
			this.MainTable.ResumeLayout(false);
			this.MainTable.PerformLayout();
			this.FactorioModLocationGroup.ResumeLayout(false);
			this.FactorioModLocationGroup.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button FactorioBrowseButton;
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
        private System.Windows.Forms.GroupBox PresetNameGroup;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TableLayoutPanel MainTable;
		private System.Windows.Forms.TableLayoutPanel PresetNameTable;
		private System.Windows.Forms.Button CancelImportButtonB;
		private System.Windows.Forms.TableLayoutPanel FactorioSettingsTable;
		private System.Windows.Forms.TextBox PresetNameTextBox;
		private System.Windows.Forms.TableLayoutPanel FactorioLocationTable;
		private System.Windows.Forms.Button OKButton;
		private System.Windows.Forms.Button CancelImportButton;
		private CustomProgressBar ImportProgressBar;
		private System.Windows.Forms.GroupBox FactorioModLocationGroup;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button ModsBrowseButton;
		private System.Windows.Forms.ComboBox ModsLocationComboBox;
		private System.Windows.Forms.CheckBox CompatibilityModeCheckBox;
	}
}