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
			this.DifficultyTable = new System.Windows.Forms.TableLayoutPanel();
			this.TechnologyDifficultyLabel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.RecipeDifficultyLabel = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.EnabledObjectsTable = new System.Windows.Forms.TableLayoutPanel();
			this.ShowUnavailablesCheckBox = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.LoadEnabledFromSaveButton = new System.Windows.Forms.Button();
			this.EnabledObjectsTabControl = new System.Windows.Forms.TabControl();
			this.AssemblersPage = new System.Windows.Forms.TabPage();
			this.AssemblerListView = new System.Windows.Forms.ListView();
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.IconList = new System.Windows.Forms.ImageList(this.components);
			this.MinersPage = new System.Windows.Forms.TabPage();
			this.MinerListView = new System.Windows.Forms.ListView();
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.PowersPage = new System.Windows.Forms.TabPage();
			this.PowerListView = new System.Windows.Forms.ListView();
			this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.BeaconsPage = new System.Windows.Forms.TabPage();
			this.BeaconListView = new System.Windows.Forms.ListView();
			this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ModulesPage = new System.Windows.Forms.TabPage();
			this.ModuleListView = new System.Windows.Forms.ListView();
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.RecipesPage = new System.Windows.Forms.TabPage();
			this.RecipeListView = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.FilterTextBox = new System.Windows.Forms.TextBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.PresetsTable = new System.Windows.Forms.TableLayoutPanel();
			this.ImportPresetButton = new System.Windows.Forms.Button();
			this.ComparePresetsButton = new System.Windows.Forms.Button();
			this.PresetListBox = new System.Windows.Forms.ListBox();
			this.CurrentPresetTable = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.CurrentPresetLabel = new System.Windows.Forms.Label();
			this.ConfirmButton = new System.Windows.Forms.Button();
			this.CancelSettingsButton = new System.Windows.Forms.Button();
			this.PresetMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.SelectPresetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DeletePresetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MainTable = new System.Windows.Forms.TableLayoutPanel();
			this.FormButtonsTable = new System.Windows.Forms.TableLayoutPanel();
			this.RecipeToolTip = new Foreman.RecipeToolTip();
			this.groupBox4.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.DifficultyTable.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.EnabledObjectsTable.SuspendLayout();
			this.EnabledObjectsTabControl.SuspendLayout();
			this.AssemblersPage.SuspendLayout();
			this.MinersPage.SuspendLayout();
			this.PowersPage.SuspendLayout();
			this.BeaconsPage.SuspendLayout();
			this.ModulesPage.SuspendLayout();
			this.RecipesPage.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.PresetsTable.SuspendLayout();
			this.CurrentPresetTable.SuspendLayout();
			this.PresetMenuStrip.SuspendLayout();
			this.MainTable.SuspendLayout();
			this.FormButtonsTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.ModSelectionBox);
			this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox4.Location = new System.Drawing.Point(210, 2);
			this.groupBox4.Margin = new System.Windows.Forms.Padding(2);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Padding = new System.Windows.Forms.Padding(5);
			this.PresetsTable.SetRowSpan(this.groupBox4, 2);
			this.groupBox4.Size = new System.Drawing.Size(204, 400);
			this.groupBox4.TabIndex = 12;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Mods (read-only):";
			// 
			// ModSelectionBox
			// 
			this.ModSelectionBox.BackColor = System.Drawing.Color.WhiteSmoke;
			this.ModSelectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModSelectionBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.ModSelectionBox.FormattingEnabled = true;
			this.ModSelectionBox.Location = new System.Drawing.Point(5, 21);
			this.ModSelectionBox.Margin = new System.Windows.Forms.Padding(5);
			this.ModSelectionBox.Name = "ModSelectionBox";
			this.ModSelectionBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.ModSelectionBox.Size = new System.Drawing.Size(194, 374);
			this.ModSelectionBox.TabIndex = 10;
			// 
			// groupBox1
			// 
			this.groupBox1.AutoSize = true;
			this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox1.Controls.Add(this.DifficultyTable);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(210, 406);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
			this.PresetsTable.SetRowSpan(this.groupBox1, 2);
			this.groupBox1.Size = new System.Drawing.Size(204, 50);
			this.groupBox1.TabIndex = 11;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Difficulty (read-only)";
			// 
			// DifficultyTable
			// 
			this.DifficultyTable.ColumnCount = 2;
			this.DifficultyTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.DifficultyTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.DifficultyTable.Controls.Add(this.TechnologyDifficultyLabel, 1, 1);
			this.DifficultyTable.Controls.Add(this.label2, 0, 0);
			this.DifficultyTable.Controls.Add(this.label3, 0, 1);
			this.DifficultyTable.Controls.Add(this.RecipeDifficultyLabel, 1, 0);
			this.DifficultyTable.Dock = System.Windows.Forms.DockStyle.Left;
			this.DifficultyTable.Location = new System.Drawing.Point(2, 18);
			this.DifficultyTable.Name = "DifficultyTable";
			this.DifficultyTable.RowCount = 2;
			this.DifficultyTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DifficultyTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DifficultyTable.Size = new System.Drawing.Size(126, 30);
			this.DifficultyTable.TabIndex = 0;
			// 
			// TechnologyDifficultyLabel
			// 
			this.TechnologyDifficultyLabel.AutoSize = true;
			this.TechnologyDifficultyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TechnologyDifficultyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.TechnologyDifficultyLabel.Location = new System.Drawing.Point(72, 13);
			this.TechnologyDifficultyLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.TechnologyDifficultyLabel.Name = "TechnologyDifficultyLabel";
			this.TechnologyDifficultyLabel.Size = new System.Drawing.Size(52, 17);
			this.TechnologyDifficultyLabel.TabIndex = 3;
			this.TechnologyDifficultyLabel.Text = "Normal";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label2.Location = new System.Drawing.Point(2, 0);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(66, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Recipe:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label3.Location = new System.Drawing.Point(2, 13);
			this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(66, 17);
			this.label3.TabIndex = 1;
			this.label3.Text = "Technology:";
			// 
			// RecipeDifficultyLabel
			// 
			this.RecipeDifficultyLabel.AutoSize = true;
			this.RecipeDifficultyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipeDifficultyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.RecipeDifficultyLabel.Location = new System.Drawing.Point(72, 0);
			this.RecipeDifficultyLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.RecipeDifficultyLabel.Name = "RecipeDifficultyLabel";
			this.RecipeDifficultyLabel.Size = new System.Drawing.Size(52, 13);
			this.RecipeDifficultyLabel.TabIndex = 2;
			this.RecipeDifficultyLabel.Text = "Normal";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.EnabledObjectsTable);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.groupBox2.Location = new System.Drawing.Point(426, 2);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
			this.groupBox2.Size = new System.Drawing.Size(343, 478);
			this.groupBox2.TabIndex = 17;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Enabled Objects:";
			// 
			// EnabledObjectsTable
			// 
			this.EnabledObjectsTable.AutoSize = true;
			this.EnabledObjectsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.EnabledObjectsTable.ColumnCount = 2;
			this.EnabledObjectsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.EnabledObjectsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.EnabledObjectsTable.Controls.Add(this.ShowUnavailablesCheckBox, 1, 2);
			this.EnabledObjectsTable.Controls.Add(this.label4, 0, 1);
			this.EnabledObjectsTable.Controls.Add(this.LoadEnabledFromSaveButton, 0, 0);
			this.EnabledObjectsTable.Controls.Add(this.EnabledObjectsTabControl, 0, 3);
			this.EnabledObjectsTable.Controls.Add(this.FilterTextBox, 1, 1);
			this.EnabledObjectsTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnabledObjectsTable.Location = new System.Drawing.Point(2, 18);
			this.EnabledObjectsTable.Name = "EnabledObjectsTable";
			this.EnabledObjectsTable.RowCount = 4;
			this.EnabledObjectsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.EnabledObjectsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.EnabledObjectsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.EnabledObjectsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.EnabledObjectsTable.Size = new System.Drawing.Size(339, 458);
			this.EnabledObjectsTable.TabIndex = 0;
			// 
			// ShowUnavailablesCheckBox
			// 
			this.ShowUnavailablesCheckBox.AutoSize = true;
			this.ShowUnavailablesCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShowUnavailablesCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.ShowUnavailablesCheckBox.Location = new System.Drawing.Point(43, 53);
			this.ShowUnavailablesCheckBox.Margin = new System.Windows.Forms.Padding(2);
			this.ShowUnavailablesCheckBox.Name = "ShowUnavailablesCheckBox";
			this.ShowUnavailablesCheckBox.Size = new System.Drawing.Size(294, 17);
			this.ShowUnavailablesCheckBox.TabIndex = 31;
			this.ShowUnavailablesCheckBox.Text = "Show Unavailables";
			this.ShowUnavailablesCheckBox.UseVisualStyleBackColor = true;
			this.ShowUnavailablesCheckBox.CheckedChanged += new System.EventHandler(this.Filters_Changed);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label4.Location = new System.Drawing.Point(7, 27);
			this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 2, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(32, 24);
			this.label4.TabIndex = 30;
			this.label4.Text = "Filter:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LoadEnabledFromSaveButton
			// 
			this.LoadEnabledFromSaveButton.AutoSize = true;
			this.LoadEnabledFromSaveButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.EnabledObjectsTable.SetColumnSpan(this.LoadEnabledFromSaveButton, 2);
			this.LoadEnabledFromSaveButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LoadEnabledFromSaveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.LoadEnabledFromSaveButton.Location = new System.Drawing.Point(4, 2);
			this.LoadEnabledFromSaveButton.Margin = new System.Windows.Forms.Padding(4, 2, 7, 2);
			this.LoadEnabledFromSaveButton.Name = "LoadEnabledFromSaveButton";
			this.LoadEnabledFromSaveButton.Size = new System.Drawing.Size(328, 23);
			this.LoadEnabledFromSaveButton.TabIndex = 28;
			this.LoadEnabledFromSaveButton.Text = "Load From Save";
			this.LoadEnabledFromSaveButton.UseVisualStyleBackColor = true;
			this.LoadEnabledFromSaveButton.Click += new System.EventHandler(this.LoadEnabledFromSaveButton_Click);
			// 
			// EnabledObjectsTabControl
			// 
			this.EnabledObjectsTable.SetColumnSpan(this.EnabledObjectsTabControl, 2);
			this.EnabledObjectsTabControl.Controls.Add(this.AssemblersPage);
			this.EnabledObjectsTabControl.Controls.Add(this.MinersPage);
			this.EnabledObjectsTabControl.Controls.Add(this.PowersPage);
			this.EnabledObjectsTabControl.Controls.Add(this.BeaconsPage);
			this.EnabledObjectsTabControl.Controls.Add(this.ModulesPage);
			this.EnabledObjectsTabControl.Controls.Add(this.RecipesPage);
			this.EnabledObjectsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnabledObjectsTabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.EnabledObjectsTabControl.Location = new System.Drawing.Point(4, 76);
			this.EnabledObjectsTabControl.Margin = new System.Windows.Forms.Padding(4);
			this.EnabledObjectsTabControl.Multiline = true;
			this.EnabledObjectsTabControl.Name = "EnabledObjectsTabControl";
			this.EnabledObjectsTabControl.SelectedIndex = 0;
			this.EnabledObjectsTabControl.Size = new System.Drawing.Size(331, 378);
			this.EnabledObjectsTabControl.TabIndex = 27;
			// 
			// AssemblersPage
			// 
			this.AssemblersPage.Controls.Add(this.AssemblerListView);
			this.AssemblersPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.AssemblersPage.Location = new System.Drawing.Point(4, 22);
			this.AssemblersPage.Margin = new System.Windows.Forms.Padding(2);
			this.AssemblersPage.Name = "AssemblersPage";
			this.AssemblersPage.Size = new System.Drawing.Size(323, 352);
			this.AssemblersPage.TabIndex = 0;
			this.AssemblersPage.Text = "Assemblers";
			this.AssemblersPage.UseVisualStyleBackColor = true;
			// 
			// AssemblerListView
			// 
			this.AssemblerListView.CheckBoxes = true;
			this.AssemblerListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
			this.AssemblerListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.AssemblerListView.FullRowSelect = true;
			this.AssemblerListView.GridLines = true;
			this.AssemblerListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.AssemblerListView.HideSelection = false;
			this.AssemblerListView.LabelWrap = false;
			this.AssemblerListView.Location = new System.Drawing.Point(0, 0);
			this.AssemblerListView.Name = "AssemblerListView";
			this.AssemblerListView.Size = new System.Drawing.Size(323, 352);
			this.AssemblerListView.SmallImageList = this.IconList;
			this.AssemblerListView.TabIndex = 17;
			this.AssemblerListView.UseCompatibleStateImageBehavior = false;
			this.AssemblerListView.View = System.Windows.Forms.View.Details;
			this.AssemblerListView.VirtualMode = true;
			this.AssemblerListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.AssemblerListView_RetrieveVirtualItem);
			this.AssemblerListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListView_KeyDown);
			this.AssemblerListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseClick);
			this.AssemblerListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDoubleClick);
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Name";
			this.columnHeader4.Width = 290;
			// 
			// IconList
			// 
			this.IconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.IconList.ImageSize = new System.Drawing.Size(24, 24);
			this.IconList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// MinersPage
			// 
			this.MinersPage.Controls.Add(this.MinerListView);
			this.MinersPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.MinersPage.Location = new System.Drawing.Point(4, 22);
			this.MinersPage.Margin = new System.Windows.Forms.Padding(2);
			this.MinersPage.Name = "MinersPage";
			this.MinersPage.Size = new System.Drawing.Size(323, 352);
			this.MinersPage.TabIndex = 2;
			this.MinersPage.Text = "Miners";
			this.MinersPage.UseVisualStyleBackColor = true;
			// 
			// MinerListView
			// 
			this.MinerListView.CheckBoxes = true;
			this.MinerListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3});
			this.MinerListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MinerListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.MinerListView.FullRowSelect = true;
			this.MinerListView.GridLines = true;
			this.MinerListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.MinerListView.HideSelection = false;
			this.MinerListView.LabelWrap = false;
			this.MinerListView.Location = new System.Drawing.Point(0, 0);
			this.MinerListView.Name = "MinerListView";
			this.MinerListView.Size = new System.Drawing.Size(323, 352);
			this.MinerListView.SmallImageList = this.IconList;
			this.MinerListView.TabIndex = 17;
			this.MinerListView.UseCompatibleStateImageBehavior = false;
			this.MinerListView.View = System.Windows.Forms.View.Details;
			this.MinerListView.VirtualMode = true;
			this.MinerListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.MinerListView_RetrieveVirtualItem);
			this.MinerListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListView_KeyDown);
			this.MinerListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseClick);
			this.MinerListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDoubleClick);
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Name";
			this.columnHeader3.Width = 290;
			// 
			// PowersPage
			// 
			this.PowersPage.Controls.Add(this.PowerListView);
			this.PowersPage.Location = new System.Drawing.Point(4, 22);
			this.PowersPage.Name = "PowersPage";
			this.PowersPage.Size = new System.Drawing.Size(323, 352);
			this.PowersPage.TabIndex = 5;
			this.PowersPage.Text = "Power";
			this.PowersPage.UseVisualStyleBackColor = true;
			// 
			// PowerListView
			// 
			this.PowerListView.CheckBoxes = true;
			this.PowerListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5});
			this.PowerListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PowerListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.PowerListView.FullRowSelect = true;
			this.PowerListView.GridLines = true;
			this.PowerListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.PowerListView.HideSelection = false;
			this.PowerListView.LabelWrap = false;
			this.PowerListView.Location = new System.Drawing.Point(0, 0);
			this.PowerListView.Name = "PowerListView";
			this.PowerListView.Size = new System.Drawing.Size(323, 352);
			this.PowerListView.SmallImageList = this.IconList;
			this.PowerListView.TabIndex = 18;
			this.PowerListView.UseCompatibleStateImageBehavior = false;
			this.PowerListView.View = System.Windows.Forms.View.Details;
			this.PowerListView.VirtualMode = true;
			this.PowerListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.PowerListView_RetrieveVirtualItem);
			this.PowerListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListView_KeyDown);
			this.PowerListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseClick);
			this.PowerListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDoubleClick);
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Name";
			this.columnHeader5.Width = 290;
			// 
			// BeaconsPage
			// 
			this.BeaconsPage.Controls.Add(this.BeaconListView);
			this.BeaconsPage.Location = new System.Drawing.Point(4, 22);
			this.BeaconsPage.Name = "BeaconsPage";
			this.BeaconsPage.Size = new System.Drawing.Size(323, 352);
			this.BeaconsPage.TabIndex = 6;
			this.BeaconsPage.Text = "Beacons";
			this.BeaconsPage.UseVisualStyleBackColor = true;
			// 
			// BeaconListView
			// 
			this.BeaconListView.CheckBoxes = true;
			this.BeaconListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6});
			this.BeaconListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BeaconListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.BeaconListView.FullRowSelect = true;
			this.BeaconListView.GridLines = true;
			this.BeaconListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.BeaconListView.HideSelection = false;
			this.BeaconListView.LabelWrap = false;
			this.BeaconListView.Location = new System.Drawing.Point(0, 0);
			this.BeaconListView.Name = "BeaconListView";
			this.BeaconListView.Size = new System.Drawing.Size(323, 352);
			this.BeaconListView.SmallImageList = this.IconList;
			this.BeaconListView.TabIndex = 19;
			this.BeaconListView.UseCompatibleStateImageBehavior = false;
			this.BeaconListView.View = System.Windows.Forms.View.Details;
			this.BeaconListView.VirtualMode = true;
			this.BeaconListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.BeaconListView_RetrieveVirtualItem);
			this.BeaconListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListView_KeyDown);
			this.BeaconListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseClick);
			this.BeaconListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDoubleClick);
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Name";
			this.columnHeader6.Width = 290;
			// 
			// ModulesPage
			// 
			this.ModulesPage.Controls.Add(this.ModuleListView);
			this.ModulesPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.ModulesPage.Location = new System.Drawing.Point(4, 22);
			this.ModulesPage.Margin = new System.Windows.Forms.Padding(2);
			this.ModulesPage.Name = "ModulesPage";
			this.ModulesPage.Size = new System.Drawing.Size(323, 352);
			this.ModulesPage.TabIndex = 3;
			this.ModulesPage.Text = "Modules";
			this.ModulesPage.UseVisualStyleBackColor = true;
			// 
			// ModuleListView
			// 
			this.ModuleListView.CheckBoxes = true;
			this.ModuleListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
			this.ModuleListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModuleListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.ModuleListView.FullRowSelect = true;
			this.ModuleListView.GridLines = true;
			this.ModuleListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.ModuleListView.HideSelection = false;
			this.ModuleListView.LabelWrap = false;
			this.ModuleListView.Location = new System.Drawing.Point(0, 0);
			this.ModuleListView.Name = "ModuleListView";
			this.ModuleListView.Size = new System.Drawing.Size(323, 352);
			this.ModuleListView.SmallImageList = this.IconList;
			this.ModuleListView.TabIndex = 17;
			this.ModuleListView.UseCompatibleStateImageBehavior = false;
			this.ModuleListView.View = System.Windows.Forms.View.Details;
			this.ModuleListView.VirtualMode = true;
			this.ModuleListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.ModuleListView_RetrieveVirtualItem);
			this.ModuleListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListView_KeyDown);
			this.ModuleListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseClick);
			this.ModuleListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDoubleClick);
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Name";
			this.columnHeader2.Width = 290;
			// 
			// RecipesPage
			// 
			this.RecipesPage.Controls.Add(this.RecipeListView);
			this.RecipesPage.Location = new System.Drawing.Point(4, 22);
			this.RecipesPage.Margin = new System.Windows.Forms.Padding(2);
			this.RecipesPage.Name = "RecipesPage";
			this.RecipesPage.Size = new System.Drawing.Size(323, 352);
			this.RecipesPage.TabIndex = 4;
			this.RecipesPage.Text = "Recipes";
			this.RecipesPage.UseVisualStyleBackColor = true;
			// 
			// RecipeListView
			// 
			this.RecipeListView.CheckBoxes = true;
			this.RecipeListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
			this.RecipeListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipeListView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.RecipeListView.FullRowSelect = true;
			this.RecipeListView.GridLines = true;
			this.RecipeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.RecipeListView.HideSelection = false;
			this.RecipeListView.LabelWrap = false;
			this.RecipeListView.Location = new System.Drawing.Point(0, 0);
			this.RecipeListView.Name = "RecipeListView";
			this.RecipeListView.Size = new System.Drawing.Size(323, 352);
			this.RecipeListView.SmallImageList = this.IconList;
			this.RecipeListView.TabIndex = 16;
			this.RecipeListView.UseCompatibleStateImageBehavior = false;
			this.RecipeListView.View = System.Windows.Forms.View.Details;
			this.RecipeListView.VirtualMode = true;
			this.RecipeListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.RecipeListView_RetrieveVirtualItem);
			this.RecipeListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ListView_KeyDown);
			this.RecipeListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseClick);
			this.RecipeListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseDoubleClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 290;
			// 
			// FilterTextBox
			// 
			this.FilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.FilterTextBox.Location = new System.Drawing.Point(43, 29);
			this.FilterTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 7, 2);
			this.FilterTextBox.Name = "FilterTextBox";
			this.FilterTextBox.Size = new System.Drawing.Size(289, 20);
			this.FilterTextBox.TabIndex = 29;
			this.FilterTextBox.TextChanged += new System.EventHandler(this.Filters_Changed);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.PresetsTable);
			this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.groupBox3.Location = new System.Drawing.Point(2, 2);
			this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
			this.groupBox3.Size = new System.Drawing.Size(420, 478);
			this.groupBox3.TabIndex = 18;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Factorio Presets:";
			// 
			// PresetsTable
			// 
			this.PresetsTable.ColumnCount = 2;
			this.PresetsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.PresetsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.PresetsTable.Controls.Add(this.groupBox4, 1, 0);
			this.PresetsTable.Controls.Add(this.ImportPresetButton, 0, 2);
			this.PresetsTable.Controls.Add(this.groupBox1, 1, 2);
			this.PresetsTable.Controls.Add(this.ComparePresetsButton, 0, 3);
			this.PresetsTable.Controls.Add(this.PresetListBox, 0, 1);
			this.PresetsTable.Controls.Add(this.CurrentPresetTable, 0, 0);
			this.PresetsTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PresetsTable.Location = new System.Drawing.Point(2, 18);
			this.PresetsTable.Name = "PresetsTable";
			this.PresetsTable.RowCount = 4;
			this.PresetsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.PresetsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.PresetsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.PresetsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.PresetsTable.Size = new System.Drawing.Size(416, 458);
			this.PresetsTable.TabIndex = 12;
			// 
			// ImportPresetButton
			// 
			this.ImportPresetButton.AutoSize = true;
			this.ImportPresetButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ImportPresetButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportPresetButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.ImportPresetButton.Location = new System.Drawing.Point(5, 406);
			this.ImportPresetButton.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
			this.ImportPresetButton.Name = "ImportPresetButton";
			this.ImportPresetButton.Size = new System.Drawing.Size(198, 23);
			this.ImportPresetButton.TabIndex = 1;
			this.ImportPresetButton.Text = "Import New Preset From Factorio";
			this.ImportPresetButton.UseVisualStyleBackColor = true;
			this.ImportPresetButton.Click += new System.EventHandler(this.ImportPresetButton_Click);
			// 
			// ComparePresetsButton
			// 
			this.ComparePresetsButton.AutoSize = true;
			this.ComparePresetsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ComparePresetsButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComparePresetsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.ComparePresetsButton.Location = new System.Drawing.Point(5, 433);
			this.ComparePresetsButton.Margin = new System.Windows.Forms.Padding(5, 2, 5, 2);
			this.ComparePresetsButton.Name = "ComparePresetsButton";
			this.ComparePresetsButton.Size = new System.Drawing.Size(198, 23);
			this.ComparePresetsButton.TabIndex = 2;
			this.ComparePresetsButton.Text = "Compare Presets";
			this.ComparePresetsButton.UseVisualStyleBackColor = true;
			this.ComparePresetsButton.Click += new System.EventHandler(this.ComparePresetsButton_Click);
			// 
			// PresetListBox
			// 
			this.PresetListBox.DisplayMember = "Name";
			this.PresetListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PresetListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.PresetListBox.FormattingEnabled = true;
			this.PresetListBox.Location = new System.Drawing.Point(5, 24);
			this.PresetListBox.Margin = new System.Windows.Forms.Padding(5, 5, 5, 7);
			this.PresetListBox.Name = "PresetListBox";
			this.PresetListBox.Size = new System.Drawing.Size(198, 373);
			this.PresetListBox.TabIndex = 0;
			this.PresetListBox.SelectedValueChanged += new System.EventHandler(this.PresetListBox_SelectedValueChanged);
			this.PresetListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PresetListBox_MouseDoubleClick);
			this.PresetListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PresetListBox_MouseDown);
			// 
			// CurrentPresetTable
			// 
			this.CurrentPresetTable.AutoSize = true;
			this.CurrentPresetTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CurrentPresetTable.ColumnCount = 2;
			this.CurrentPresetTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CurrentPresetTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CurrentPresetTable.Controls.Add(this.label1, 0, 0);
			this.CurrentPresetTable.Controls.Add(this.CurrentPresetLabel, 1, 0);
			this.CurrentPresetTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrentPresetTable.Location = new System.Drawing.Point(3, 3);
			this.CurrentPresetTable.Name = "CurrentPresetTable";
			this.CurrentPresetTable.RowCount = 1;
			this.CurrentPresetTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.CurrentPresetTable.Size = new System.Drawing.Size(202, 13);
			this.CurrentPresetTable.TabIndex = 13;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.label1.Location = new System.Drawing.Point(2, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(52, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Current:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label1.Click += new System.EventHandler(this.CurrentPresetLabel_Click);
			// 
			// CurrentPresetLabel
			// 
			this.CurrentPresetLabel.AutoSize = true;
			this.CurrentPresetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CurrentPresetLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.CurrentPresetLabel.Location = new System.Drawing.Point(58, 0);
			this.CurrentPresetLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.CurrentPresetLabel.Name = "CurrentPresetLabel";
			this.CurrentPresetLabel.Size = new System.Drawing.Size(142, 13);
			this.CurrentPresetLabel.TabIndex = 4;
			this.CurrentPresetLabel.Text = "preset";
			this.CurrentPresetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.CurrentPresetLabel.Click += new System.EventHandler(this.CurrentPresetLabel_Click);
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.AutoSize = true;
			this.ConfirmButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ConfirmButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfirmButton.Location = new System.Drawing.Point(2, 2);
			this.ConfirmButton.Margin = new System.Windows.Forms.Padding(2);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.Size = new System.Drawing.Size(707, 23);
			this.ConfirmButton.TabIndex = 25;
			this.ConfirmButton.Text = "Confirm";
			this.ConfirmButton.UseVisualStyleBackColor = true;
			this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// CancelSettingsButton
			// 
			this.CancelSettingsButton.AutoSize = true;
			this.CancelSettingsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelSettingsButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelSettingsButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CancelSettingsButton.Location = new System.Drawing.Point(713, 2);
			this.CancelSettingsButton.Margin = new System.Windows.Forms.Padding(2);
			this.CancelSettingsButton.Name = "CancelSettingsButton";
			this.CancelSettingsButton.Size = new System.Drawing.Size(50, 23);
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
			this.PresetMenuStrip.Size = new System.Drawing.Size(143, 48);
			// 
			// SelectPresetMenuItem
			// 
			this.SelectPresetMenuItem.Name = "SelectPresetMenuItem";
			this.SelectPresetMenuItem.Size = new System.Drawing.Size(142, 22);
			this.SelectPresetMenuItem.Text = "Select Preset";
			// 
			// DeletePresetMenuItem
			// 
			this.DeletePresetMenuItem.Name = "DeletePresetMenuItem";
			this.DeletePresetMenuItem.Size = new System.Drawing.Size(142, 22);
			this.DeletePresetMenuItem.Text = "Delete Preset";
			// 
			// MainTable
			// 
			this.MainTable.AutoSize = true;
			this.MainTable.ColumnCount = 2;
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
			this.MainTable.Controls.Add(this.FormButtonsTable, 0, 1);
			this.MainTable.Controls.Add(this.groupBox3, 0, 0);
			this.MainTable.Controls.Add(this.groupBox2, 1, 0);
			this.MainTable.Location = new System.Drawing.Point(3, 3);
			this.MainTable.Name = "MainTable";
			this.MainTable.RowCount = 2;
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.Size = new System.Drawing.Size(771, 515);
			this.MainTable.TabIndex = 27;
			// 
			// FormButtonsTable
			// 
			this.FormButtonsTable.AutoSize = true;
			this.FormButtonsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.FormButtonsTable.ColumnCount = 2;
			this.MainTable.SetColumnSpan(this.FormButtonsTable, 2);
			this.FormButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.FormButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.FormButtonsTable.Controls.Add(this.ConfirmButton, 0, 0);
			this.FormButtonsTable.Controls.Add(this.CancelSettingsButton, 1, 0);
			this.FormButtonsTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FormButtonsTable.Location = new System.Drawing.Point(3, 485);
			this.FormButtonsTable.Name = "FormButtonsTable";
			this.FormButtonsTable.RowCount = 1;
			this.FormButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.FormButtonsTable.Size = new System.Drawing.Size(765, 27);
			this.FormButtonsTable.TabIndex = 28;
			// 
			// RecipeToolTip
			// 
			this.RecipeToolTip.AutoPopDelay = 100000;
			this.RecipeToolTip.BackColor = System.Drawing.Color.DimGray;
			this.RecipeToolTip.ForeColor = System.Drawing.Color.White;
			this.RecipeToolTip.InitialDelay = 100000;
			this.RecipeToolTip.OwnerDraw = true;
			this.RecipeToolTip.ReshowDelay = 100000;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.ConfirmButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.CancelSettingsButton;
			this.ClientSize = new System.Drawing.Size(777, 524);
			this.Controls.Add(this.MainTable);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.Name = "SettingsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			this.groupBox4.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.DifficultyTable.ResumeLayout(false);
			this.DifficultyTable.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.EnabledObjectsTable.ResumeLayout(false);
			this.EnabledObjectsTable.PerformLayout();
			this.EnabledObjectsTabControl.ResumeLayout(false);
			this.AssemblersPage.ResumeLayout(false);
			this.MinersPage.ResumeLayout(false);
			this.PowersPage.ResumeLayout(false);
			this.BeaconsPage.ResumeLayout(false);
			this.ModulesPage.ResumeLayout(false);
			this.RecipesPage.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.PresetsTable.ResumeLayout(false);
			this.PresetsTable.PerformLayout();
			this.CurrentPresetTable.ResumeLayout(false);
			this.CurrentPresetTable.PerformLayout();
			this.PresetMenuStrip.ResumeLayout(false);
			this.MainTable.ResumeLayout(false);
			this.MainTable.PerformLayout();
			this.FormButtonsTable.ResumeLayout(false);
			this.FormButtonsTable.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button CancelSettingsButton;
        private System.Windows.Forms.Button ConfirmButton;
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
        private System.Windows.Forms.TabPage MinersPage;
        private System.Windows.Forms.TabPage ModulesPage;
        private System.Windows.Forms.Button LoadEnabledFromSaveButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.TabPage RecipesPage;
        private System.Windows.Forms.ListView RecipeListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ImageList IconList;
		private System.Windows.Forms.CheckBox ShowUnavailablesCheckBox;
		private RecipeToolTip RecipeToolTip;
		private System.Windows.Forms.ListView AssemblerListView;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ListView MinerListView;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ListView ModuleListView;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.TableLayoutPanel PresetsTable;
		private System.Windows.Forms.TableLayoutPanel DifficultyTable;
		private System.Windows.Forms.TableLayoutPanel CurrentPresetTable;
		private System.Windows.Forms.TableLayoutPanel MainTable;
		private System.Windows.Forms.TableLayoutPanel FormButtonsTable;
		private System.Windows.Forms.TableLayoutPanel EnabledObjectsTable;
		private System.Windows.Forms.TabPage PowersPage;
		private System.Windows.Forms.ListView PowerListView;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.TabPage BeaconsPage;
		private System.Windows.Forms.ListView BeaconListView;
		private System.Windows.Forms.ColumnHeader columnHeader6;
	}
}