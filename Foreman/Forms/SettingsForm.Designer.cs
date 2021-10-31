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
			this.components = new System.ComponentModel.Container();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.ModSelectionBox = new System.Windows.Forms.ListBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.TechnologyDifficultyLabel = new System.Windows.Forms.Label();
			this.RecipeDifficultyLabel = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.ModuleSelectionNoneButton = new System.Windows.Forms.Button();
			this.ModuleSelectionAllButton = new System.Windows.Forms.Button();
			this.ModuleSelectionBox = new Foreman.DataObjectCheckedListBox();
			this.MinerSelectionNoneButton = new System.Windows.Forms.Button();
			this.MinerSelectionAllButton = new System.Windows.Forms.Button();
			this.MinerSelectionBox = new Foreman.DataObjectCheckedListBox();
			this.AssemblerSelectionNoneButton = new System.Windows.Forms.Button();
			this.AssemblerSelectionAllButton = new System.Windows.Forms.Button();
			this.AssemblerSelectionBox = new Foreman.DataObjectCheckedListBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.ShowUnavailablesCheckBox = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.FilterTextBox = new System.Windows.Forms.TextBox();
			this.LoadEnabledFromSaveButton = new System.Windows.Forms.Button();
			this.EnabledObjectsTabControl = new System.Windows.Forms.TabControl();
			this.AssemblersPage = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.MinersPage = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.ModulesPage = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.RecipePage = new System.Windows.Forms.TabPage();
			this.RecipeListView = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.IconList = new System.Windows.Forms.ImageList(this.components);
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.CurrentPresetLabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.ComparePresetsButton = new System.Windows.Forms.Button();
			this.ImportPresetButton = new System.Windows.Forms.Button();
			this.PresetListBox = new System.Windows.Forms.ListBox();
			this.ConfirmButton = new System.Windows.Forms.Button();
			this.CancelSettingsButton = new System.Windows.Forms.Button();
			this.PresetMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SelectPresetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DeletePresetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.groupBox4.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.EnabledObjectsTabControl.SuspendLayout();
			this.AssemblersPage.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.MinersPage.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.ModulesPage.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			this.RecipePage.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.PresetMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.ModSelectionBox);
			this.groupBox4.Location = new System.Drawing.Point(249, 21);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(259, 477);
			this.groupBox4.TabIndex = 12;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Mods (read-only):";
			// 
			// ModSelectionBox
			// 
			this.ModSelectionBox.BackColor = System.Drawing.Color.WhiteSmoke;
			this.ModSelectionBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.ModSelectionBox.FormattingEnabled = true;
			this.ModSelectionBox.ItemHeight = 16;
			this.ModSelectionBox.Location = new System.Drawing.Point(7, 29);
			this.ModSelectionBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
			this.ModSelectionBox.Name = "ModSelectionBox";
			this.ModSelectionBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.ModSelectionBox.Size = new System.Drawing.Size(245, 436);
			this.ModSelectionBox.TabIndex = 10;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.TechnologyDifficultyLabel);
			this.groupBox1.Controls.Add(this.RecipeDifficultyLabel);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Location = new System.Drawing.Point(249, 504);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(259, 72);
			this.groupBox1.TabIndex = 11;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Difficulty (read-only)";
			// 
			// TechnologyDifficultyLabel
			// 
			this.TechnologyDifficultyLabel.AutoSize = true;
			this.TechnologyDifficultyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.TechnologyDifficultyLabel.Location = new System.Drawing.Point(96, 46);
			this.TechnologyDifficultyLabel.Name = "TechnologyDifficultyLabel";
			this.TechnologyDifficultyLabel.Size = new System.Drawing.Size(53, 17);
			this.TechnologyDifficultyLabel.TabIndex = 3;
			this.TechnologyDifficultyLabel.Text = "Normal";
			// 
			// RecipeDifficultyLabel
			// 
			this.RecipeDifficultyLabel.AutoSize = true;
			this.RecipeDifficultyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.RecipeDifficultyLabel.Location = new System.Drawing.Point(96, 22);
			this.RecipeDifficultyLabel.Name = "RecipeDifficultyLabel";
			this.RecipeDifficultyLabel.Size = new System.Drawing.Size(53, 17);
			this.RecipeDifficultyLabel.TabIndex = 2;
			this.RecipeDifficultyLabel.Text = "Normal";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.label3.Location = new System.Drawing.Point(7, 46);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(86, 17);
			this.label3.TabIndex = 1;
			this.label3.Text = "Technology:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.label2.Location = new System.Drawing.Point(7, 22);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 17);
			this.label2.TabIndex = 0;
			this.label2.Text = "Recipe:";
			// 
			// ModuleSelectionNoneButton
			// 
			this.ModuleSelectionNoneButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModuleSelectionNoneButton.Location = new System.Drawing.Point(133, 415);
			this.ModuleSelectionNoneButton.Name = "ModuleSelectionNoneButton";
			this.ModuleSelectionNoneButton.Size = new System.Drawing.Size(125, 22);
			this.ModuleSelectionNoneButton.TabIndex = 9;
			this.ModuleSelectionNoneButton.Text = "None";
			this.ModuleSelectionNoneButton.UseVisualStyleBackColor = true;
			this.ModuleSelectionNoneButton.Click += new System.EventHandler(this.ModuleSelectionNoneButton_Click);
			// 
			// ModuleSelectionAllButton
			// 
			this.ModuleSelectionAllButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModuleSelectionAllButton.Location = new System.Drawing.Point(3, 415);
			this.ModuleSelectionAllButton.Name = "ModuleSelectionAllButton";
			this.ModuleSelectionAllButton.Size = new System.Drawing.Size(124, 22);
			this.ModuleSelectionAllButton.TabIndex = 8;
			this.ModuleSelectionAllButton.Text = "All";
			this.ModuleSelectionAllButton.UseVisualStyleBackColor = true;
			this.ModuleSelectionAllButton.Click += new System.EventHandler(this.ModuleSelectionAllButton_Click);
			// 
			// ModuleSelectionBox
			// 
			this.ModuleSelectionBox.CheckOnClick = true;
			this.tableLayoutPanel4.SetColumnSpan(this.ModuleSelectionBox, 2);
			this.ModuleSelectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModuleSelectionBox.FormattingEnabled = true;
			this.ModuleSelectionBox.Location = new System.Drawing.Point(4, 4);
			this.ModuleSelectionBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
			this.ModuleSelectionBox.Name = "ModuleSelectionBox";
			this.ModuleSelectionBox.Size = new System.Drawing.Size(253, 408);
			this.ModuleSelectionBox.TabIndex = 7;
			this.ModuleSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ModuleSelectionBox_ItemCheck);
			this.ModuleSelectionBox.Enter += new System.EventHandler(this.EnableSelectionBox_Enter);
			this.ModuleSelectionBox.Leave += new System.EventHandler(this.ModuleSelectionBox_Leave);
			// 
			// MinerSelectionNoneButton
			// 
			this.MinerSelectionNoneButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MinerSelectionNoneButton.Location = new System.Drawing.Point(133, 415);
			this.MinerSelectionNoneButton.Name = "MinerSelectionNoneButton";
			this.MinerSelectionNoneButton.Size = new System.Drawing.Size(125, 22);
			this.MinerSelectionNoneButton.TabIndex = 6;
			this.MinerSelectionNoneButton.Text = "None";
			this.MinerSelectionNoneButton.UseVisualStyleBackColor = true;
			this.MinerSelectionNoneButton.Click += new System.EventHandler(this.MinerSelectionNoneButton_Click);
			// 
			// MinerSelectionAllButton
			// 
			this.MinerSelectionAllButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MinerSelectionAllButton.Location = new System.Drawing.Point(3, 415);
			this.MinerSelectionAllButton.Name = "MinerSelectionAllButton";
			this.MinerSelectionAllButton.Size = new System.Drawing.Size(124, 22);
			this.MinerSelectionAllButton.TabIndex = 5;
			this.MinerSelectionAllButton.Text = "All";
			this.MinerSelectionAllButton.UseVisualStyleBackColor = true;
			this.MinerSelectionAllButton.Click += new System.EventHandler(this.MinerSelectionAllButton_Click);
			// 
			// MinerSelectionBox
			// 
			this.MinerSelectionBox.CheckOnClick = true;
			this.tableLayoutPanel3.SetColumnSpan(this.MinerSelectionBox, 2);
			this.MinerSelectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MinerSelectionBox.FormattingEnabled = true;
			this.MinerSelectionBox.Location = new System.Drawing.Point(4, 4);
			this.MinerSelectionBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
			this.MinerSelectionBox.Name = "MinerSelectionBox";
			this.MinerSelectionBox.Size = new System.Drawing.Size(253, 408);
			this.MinerSelectionBox.TabIndex = 4;
			this.MinerSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.MinerSelectionBox_ItemCheck);
			this.MinerSelectionBox.Enter += new System.EventHandler(this.EnableSelectionBox_Enter);
			this.MinerSelectionBox.Leave += new System.EventHandler(this.MinerSelectionBox_Leave);
			// 
			// AssemblerSelectionNoneButton
			// 
			this.AssemblerSelectionNoneButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerSelectionNoneButton.Location = new System.Drawing.Point(133, 415);
			this.AssemblerSelectionNoneButton.Name = "AssemblerSelectionNoneButton";
			this.AssemblerSelectionNoneButton.Size = new System.Drawing.Size(125, 22);
			this.AssemblerSelectionNoneButton.TabIndex = 3;
			this.AssemblerSelectionNoneButton.Text = "None";
			this.AssemblerSelectionNoneButton.UseVisualStyleBackColor = true;
			this.AssemblerSelectionNoneButton.Click += new System.EventHandler(this.AssemblerSelectionNoneButton_Click);
			// 
			// AssemblerSelectionAllButton
			// 
			this.AssemblerSelectionAllButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerSelectionAllButton.Location = new System.Drawing.Point(3, 415);
			this.AssemblerSelectionAllButton.Name = "AssemblerSelectionAllButton";
			this.AssemblerSelectionAllButton.Size = new System.Drawing.Size(124, 22);
			this.AssemblerSelectionAllButton.TabIndex = 2;
			this.AssemblerSelectionAllButton.Text = "All";
			this.AssemblerSelectionAllButton.UseVisualStyleBackColor = true;
			this.AssemblerSelectionAllButton.Click += new System.EventHandler(this.AssemblerSelectionAllButton_Click);
			// 
			// AssemblerSelectionBox
			// 
			this.AssemblerSelectionBox.CheckOnClick = true;
			this.tableLayoutPanel1.SetColumnSpan(this.AssemblerSelectionBox, 2);
			this.AssemblerSelectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerSelectionBox.FormattingEnabled = true;
			this.AssemblerSelectionBox.Location = new System.Drawing.Point(4, 4);
			this.AssemblerSelectionBox.Margin = new System.Windows.Forms.Padding(4);
			this.AssemblerSelectionBox.Name = "AssemblerSelectionBox";
			this.AssemblerSelectionBox.Size = new System.Drawing.Size(253, 404);
			this.AssemblerSelectionBox.TabIndex = 1;
			this.AssemblerSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.AssemblerSelectionBox_ItemCheck);
			this.AssemblerSelectionBox.Enter += new System.EventHandler(this.EnableSelectionBox_Enter);
			this.AssemblerSelectionBox.Leave += new System.EventHandler(this.AssemblerSelectionBox_Leave);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.ShowUnavailablesCheckBox);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.FilterTextBox);
			this.groupBox2.Controls.Add(this.LoadEnabledFromSaveButton);
			this.groupBox2.Controls.Add(this.EnabledObjectsTabControl);
			this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.groupBox2.Location = new System.Drawing.Point(527, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(283, 588);
			this.groupBox2.TabIndex = 17;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Enabled Objects:";
			// 
			// ShowUnavailablesCheckBox
			// 
			this.ShowUnavailablesCheckBox.AutoSize = true;
			this.ShowUnavailablesCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.ShowUnavailablesCheckBox.Location = new System.Drawing.Point(66, 80);
			this.ShowUnavailablesCheckBox.Name = "ShowUnavailablesCheckBox";
			this.ShowUnavailablesCheckBox.Size = new System.Drawing.Size(149, 21);
			this.ShowUnavailablesCheckBox.TabIndex = 31;
			this.ShowUnavailablesCheckBox.Text = "Show Unavailables";
			this.ShowUnavailablesCheckBox.UseVisualStyleBackColor = true;
			this.ShowUnavailablesCheckBox.CheckedChanged += new System.EventHandler(this.Filters_Changed);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.label4.Location = new System.Drawing.Point(14, 56);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(43, 17);
			this.label4.TabIndex = 30;
			this.label4.Text = "Filter:";
			// 
			// FilterTextBox
			// 
			this.FilterTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.FilterTextBox.Location = new System.Drawing.Point(66, 53);
			this.FilterTextBox.Name = "FilterTextBox";
			this.FilterTextBox.Size = new System.Drawing.Size(203, 22);
			this.FilterTextBox.TabIndex = 29;
			this.FilterTextBox.TextChanged += new System.EventHandler(this.Filters_Changed);
			// 
			// LoadEnabledFromSaveButton
			// 
			this.LoadEnabledFromSaveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.LoadEnabledFromSaveButton.Location = new System.Drawing.Point(8, 24);
			this.LoadEnabledFromSaveButton.Name = "LoadEnabledFromSaveButton";
			this.LoadEnabledFromSaveButton.Size = new System.Drawing.Size(265, 23);
			this.LoadEnabledFromSaveButton.TabIndex = 28;
			this.LoadEnabledFromSaveButton.Text = "Load From Save";
			this.LoadEnabledFromSaveButton.UseVisualStyleBackColor = true;
			this.LoadEnabledFromSaveButton.Click += new System.EventHandler(this.LoadEnabledFromSaveButton_Click);
			// 
			// EnabledObjectsTabControl
			// 
			this.EnabledObjectsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EnabledObjectsTabControl.Controls.Add(this.AssemblersPage);
			this.EnabledObjectsTabControl.Controls.Add(this.MinersPage);
			this.EnabledObjectsTabControl.Controls.Add(this.ModulesPage);
			this.EnabledObjectsTabControl.Controls.Add(this.RecipePage);
			this.EnabledObjectsTabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.EnabledObjectsTabControl.Location = new System.Drawing.Point(8, 107);
			this.EnabledObjectsTabControl.Name = "EnabledObjectsTabControl";
			this.EnabledObjectsTabControl.SelectedIndex = 0;
			this.EnabledObjectsTabControl.Size = new System.Drawing.Size(269, 469);
			this.EnabledObjectsTabControl.TabIndex = 27;
			// 
			// AssemblersPage
			// 
			this.AssemblersPage.Controls.Add(this.tableLayoutPanel1);
			this.AssemblersPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.AssemblersPage.Location = new System.Drawing.Point(4, 25);
			this.AssemblersPage.Name = "AssemblersPage";
			this.AssemblersPage.Size = new System.Drawing.Size(261, 440);
			this.AssemblersPage.TabIndex = 0;
			this.AssemblersPage.Text = "Assemblers";
			this.AssemblersPage.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.AssemblerSelectionBox, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.AssemblerSelectionNoneButton, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.AssemblerSelectionAllButton, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(261, 440);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// MinersPage
			// 
			this.MinersPage.Controls.Add(this.tableLayoutPanel3);
			this.MinersPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.MinersPage.Location = new System.Drawing.Point(4, 25);
			this.MinersPage.Name = "MinersPage";
			this.MinersPage.Size = new System.Drawing.Size(261, 440);
			this.MinersPage.TabIndex = 2;
			this.MinersPage.Text = "Miners";
			this.MinersPage.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.BackColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Controls.Add(this.MinerSelectionNoneButton, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.MinerSelectionBox, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.MinerSelectionAllButton, 0, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(261, 440);
			this.tableLayoutPanel3.TabIndex = 1;
			// 
			// ModulesPage
			// 
			this.ModulesPage.Controls.Add(this.tableLayoutPanel4);
			this.ModulesPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.ModulesPage.Location = new System.Drawing.Point(4, 25);
			this.ModulesPage.Name = "ModulesPage";
			this.ModulesPage.Size = new System.Drawing.Size(261, 440);
			this.ModulesPage.TabIndex = 3;
			this.ModulesPage.Text = "Modules";
			this.ModulesPage.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.BackColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel4.ColumnCount = 2;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.Controls.Add(this.ModuleSelectionNoneButton, 1, 1);
			this.tableLayoutPanel4.Controls.Add(this.ModuleSelectionBox, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.ModuleSelectionAllButton, 0, 1);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 2;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(261, 440);
			this.tableLayoutPanel4.TabIndex = 1;
			// 
			// RecipePage
			// 
			this.RecipePage.Controls.Add(this.RecipeListView);
			this.RecipePage.Location = new System.Drawing.Point(4, 25);
			this.RecipePage.Name = "RecipePage";
			this.RecipePage.Size = new System.Drawing.Size(261, 440);
			this.RecipePage.TabIndex = 4;
			this.RecipePage.Text = "Recipes";
			this.RecipePage.UseVisualStyleBackColor = true;
			// 
			// RecipeListView
			// 
			this.RecipeListView.CheckBoxes = true;
			this.RecipeListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
			this.RecipeListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipeListView.FullRowSelect = true;
			this.RecipeListView.GridLines = true;
			this.RecipeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.RecipeListView.HideSelection = false;
			this.RecipeListView.LabelWrap = false;
			this.RecipeListView.Location = new System.Drawing.Point(0, 0);
			this.RecipeListView.Margin = new System.Windows.Forms.Padding(4);
			this.RecipeListView.Name = "RecipeListView";
			this.RecipeListView.Size = new System.Drawing.Size(261, 440);
			this.RecipeListView.SmallImageList = this.IconList;
			this.RecipeListView.TabIndex = 16;
			this.RecipeListView.UseCompatibleStateImageBehavior = false;
			this.RecipeListView.View = System.Windows.Forms.View.Details;
			this.RecipeListView.VirtualMode = true;
			this.RecipeListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.RecipeListView_RetrieveVirtualItem);
			this.RecipeListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RecipeListView_KeyDown);
			this.RecipeListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RecipeListView_MouseClick);
			this.RecipeListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RecipeListView_MouseDoubleClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 225;
			// 
			// IconList
			// 
			this.IconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.IconList.ImageSize = new System.Drawing.Size(24, 24);
			this.IconList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.groupBox4);
			this.groupBox3.Controls.Add(this.CurrentPresetLabel);
			this.groupBox3.Controls.Add(this.groupBox1);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.ComparePresetsButton);
			this.groupBox3.Controls.Add(this.ImportPresetButton);
			this.groupBox3.Controls.Add(this.PresetListBox);
			this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.groupBox3.Location = new System.Drawing.Point(7, 3);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(514, 588);
			this.groupBox3.TabIndex = 18;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Factorio Presets:";
			// 
			// CurrentPresetLabel
			// 
			this.CurrentPresetLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold);
			this.CurrentPresetLabel.Location = new System.Drawing.Point(70, 20);
			this.CurrentPresetLabel.Name = "CurrentPresetLabel";
			this.CurrentPresetLabel.Size = new System.Drawing.Size(173, 28);
			this.CurrentPresetLabel.TabIndex = 4;
			this.CurrentPresetLabel.Text = "preset";
			this.CurrentPresetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.CurrentPresetLabel.Click += new System.EventHandler(this.CurrentPresetLabel_Click);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold);
			this.label1.Location = new System.Drawing.Point(6, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(67, 27);
			this.label1.TabIndex = 3;
			this.label1.Text = "Current:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label1.Click += new System.EventHandler(this.CurrentPresetLabel_Click);
			// 
			// ComparePresetsButton
			// 
			this.ComparePresetsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.ComparePresetsButton.Location = new System.Drawing.Point(3, 542);
			this.ComparePresetsButton.Name = "ComparePresetsButton";
			this.ComparePresetsButton.Size = new System.Drawing.Size(240, 32);
			this.ComparePresetsButton.TabIndex = 2;
			this.ComparePresetsButton.Text = "Compare Presets";
			this.ComparePresetsButton.UseVisualStyleBackColor = true;
			this.ComparePresetsButton.Click += new System.EventHandler(this.ComparePresetsButton_Click);
			// 
			// ImportPresetButton
			// 
			this.ImportPresetButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.ImportPresetButton.Location = new System.Drawing.Point(3, 504);
			this.ImportPresetButton.Name = "ImportPresetButton";
			this.ImportPresetButton.Size = new System.Drawing.Size(240, 32);
			this.ImportPresetButton.TabIndex = 1;
			this.ImportPresetButton.Text = "Import New Preset From Factorio";
			this.ImportPresetButton.UseVisualStyleBackColor = true;
			this.ImportPresetButton.Click += new System.EventHandler(this.ImportPresetButton_Click);
			// 
			// PresetListBox
			// 
			this.PresetListBox.DisplayMember = "Name";
			this.PresetListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.PresetListBox.FormattingEnabled = true;
			this.PresetListBox.ItemHeight = 16;
			this.PresetListBox.Location = new System.Drawing.Point(3, 50);
			this.PresetListBox.Name = "PresetListBox";
			this.PresetListBox.Size = new System.Drawing.Size(240, 436);
			this.PresetListBox.TabIndex = 0;
			this.PresetListBox.SelectedValueChanged += new System.EventHandler(this.PresetListBox_SelectedValueChanged);
			this.PresetListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PresetListBox_MouseDown);
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.Location = new System.Drawing.Point(7, 594);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.Size = new System.Drawing.Size(694, 32);
			this.ConfirmButton.TabIndex = 25;
			this.ConfirmButton.Text = "Confirm";
			this.ConfirmButton.UseVisualStyleBackColor = true;
			this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// CancelSettingsButton
			// 
			this.CancelSettingsButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelSettingsButton.Location = new System.Drawing.Point(707, 594);
			this.CancelSettingsButton.Name = "CancelSettingsButton";
			this.CancelSettingsButton.Size = new System.Drawing.Size(103, 32);
			this.CancelSettingsButton.TabIndex = 26;
			this.CancelSettingsButton.Text = "Cancel";
			this.CancelSettingsButton.UseVisualStyleBackColor = true;
			this.CancelSettingsButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// PresetMenuStrip
			// 
			this.PresetMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.PresetMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SelectPresetMenuItem,
            this.DeletePresetMenuItem});
			this.PresetMenuStrip.Name = "PresetMenuStrip";
			this.PresetMenuStrip.Size = new System.Drawing.Size(167, 52);
			// 
			// SelectPresetMenuItem
			// 
			this.SelectPresetMenuItem.Name = "SelectPresetMenuItem";
			this.SelectPresetMenuItem.Size = new System.Drawing.Size(166, 24);
			this.SelectPresetMenuItem.Text = "Select Preset";
			// 
			// DeletePresetMenuItem
			// 
			this.DeletePresetMenuItem.Name = "DeletePresetMenuItem";
			this.DeletePresetMenuItem.Size = new System.Drawing.Size(166, 24);
			this.DeletePresetMenuItem.Text = "Delete Preset";
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.ConfirmButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelSettingsButton;
			this.ClientSize = new System.Drawing.Size(820, 635);
			this.Controls.Add(this.CancelSettingsButton);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.ConfirmButton);
			this.Controls.Add(this.groupBox2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.Name = "SettingsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			this.groupBox4.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.EnabledObjectsTabControl.ResumeLayout(false);
			this.AssemblersPage.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.MinersPage.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.ModulesPage.ResumeLayout(false);
			this.tableLayoutPanel4.ResumeLayout(false);
			this.RecipePage.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.PresetMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion
        private DataObjectCheckedListBox ModuleSelectionBox;
        private DataObjectCheckedListBox MinerSelectionBox;
        private DataObjectCheckedListBox AssemblerSelectionBox;
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
        private System.Windows.Forms.ListBox ModSelectionBox;
        private System.Windows.Forms.Button ImportPresetButton;
        private System.Windows.Forms.ListBox PresetListBox;
        private System.Windows.Forms.ContextMenuStrip PresetMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem SelectPresetMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DeletePresetMenuItem;
        private System.Windows.Forms.Button ComparePresetsButton;
        private System.Windows.Forms.Label CurrentPresetLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label TechnologyDifficultyLabel;
        private System.Windows.Forms.Label RecipeDifficultyLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl EnabledObjectsTabControl;
        private System.Windows.Forms.TabPage AssemblersPage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TabPage MinersPage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TabPage ModulesPage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button LoadEnabledFromSaveButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.TabPage RecipePage;
        private System.Windows.Forms.ListView RecipeListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ImageList IconList;
		private System.Windows.Forms.CheckBox ShowUnavailablesCheckBox;
	}
}