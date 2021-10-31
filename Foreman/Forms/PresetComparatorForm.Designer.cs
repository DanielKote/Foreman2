
namespace Foreman
{
    partial class PresetComparatorForm
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
			this.PresetSelectionGroup = new System.Windows.Forms.GroupBox();
			this.VsTable = new System.Windows.Forms.TableLayoutPanel();
			this.RightPresetSelectionBox = new System.Windows.Forms.ComboBox();
			this.LeftPresetSelectionBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ProcessPresetsButton = new System.Windows.Forms.Button();
			this.ComparisonTabControl = new System.Windows.Forms.TabControl();
			this.ModTabPage = new System.Windows.Forms.TabPage();
			this.ItemTabPage = new System.Windows.Forms.TabPage();
			this.RecipeTabPage = new System.Windows.Forms.TabPage();
			this.AssemblerTabPage = new System.Windows.Forms.TabPage();
			this.MinerTabPage = new System.Windows.Forms.TabPage();
			this.PowerTabPage = new System.Windows.Forms.TabPage();
			this.BeaconTabPage = new System.Windows.Forms.TabPage();
			this.ModuleTabPage = new System.Windows.Forms.TabPage();
			this.CloseButton = new System.Windows.Forms.Button();
			this.TabTable = new System.Windows.Forms.TableLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.LeftOnlyListView = new Foreman.FFListView();
			this.LeftOnlyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.IconList = new System.Windows.Forms.ImageList(this.components);
			this.label5 = new System.Windows.Forms.Label();
			this.LeftListView = new Foreman.SyncListView();
			this.RightListView = new Foreman.SyncListView();
			this.RightHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.LeftHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.RightOnlyListView = new Foreman.FFListView();
			this.RightOnlyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.BothPresetsTable = new System.Windows.Forms.TableLayoutPanel();
			this.HideSimilarObjectsCheckBox = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.HideEqualObjectsCheckBox = new System.Windows.Forms.CheckBox();
			this.FilterTable = new System.Windows.Forms.TableLayoutPanel();
			this.ShowUnavailableCheckBox = new System.Windows.Forms.CheckBox();
			this.label6 = new System.Windows.Forms.Label();
			this.FilterTextBox = new System.Windows.Forms.TextBox();
			this.TextToolTip = new Foreman.CustomToolTip();
			this.RecipeToolTip = new Foreman.RecipeToolTip();
			this.MainTable = new System.Windows.Forms.TableLayoutPanel();
			this.PresetSelectionGroup.SuspendLayout();
			this.VsTable.SuspendLayout();
			this.ComparisonTabControl.SuspendLayout();
			this.TabTable.SuspendLayout();
			this.BothPresetsTable.SuspendLayout();
			this.FilterTable.SuspendLayout();
			this.MainTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// PresetSelectionGroup
			// 
			this.PresetSelectionGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.PresetSelectionGroup.Controls.Add(this.VsTable);
			this.PresetSelectionGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PresetSelectionGroup.Location = new System.Drawing.Point(2, 2);
			this.PresetSelectionGroup.Margin = new System.Windows.Forms.Padding(2);
			this.PresetSelectionGroup.Name = "PresetSelectionGroup";
			this.PresetSelectionGroup.Padding = new System.Windows.Forms.Padding(2);
			this.MainTable.SetRowSpan(this.PresetSelectionGroup, 2);
			this.PresetSelectionGroup.Size = new System.Drawing.Size(376, 50);
			this.PresetSelectionGroup.TabIndex = 0;
			this.PresetSelectionGroup.TabStop = false;
			this.PresetSelectionGroup.Text = "Preset Selection";
			// 
			// VsTable
			// 
			this.VsTable.AutoSize = true;
			this.VsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.VsTable.ColumnCount = 3;
			this.VsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.VsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.VsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.VsTable.Controls.Add(this.RightPresetSelectionBox, 2, 0);
			this.VsTable.Controls.Add(this.LeftPresetSelectionBox, 0, 0);
			this.VsTable.Controls.Add(this.label1, 1, 0);
			this.VsTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VsTable.Location = new System.Drawing.Point(2, 15);
			this.VsTable.Name = "VsTable";
			this.VsTable.RowCount = 1;
			this.VsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.VsTable.Size = new System.Drawing.Size(372, 33);
			this.VsTable.TabIndex = 2;
			// 
			// RightPresetSelectionBox
			// 
			this.RightPresetSelectionBox.DisplayMember = "Name";
			this.RightPresetSelectionBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.RightPresetSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.RightPresetSelectionBox.FormattingEnabled = true;
			this.RightPresetSelectionBox.Location = new System.Drawing.Point(220, 2);
			this.RightPresetSelectionBox.Margin = new System.Windows.Forms.Padding(2);
			this.RightPresetSelectionBox.Name = "RightPresetSelectionBox";
			this.RightPresetSelectionBox.Size = new System.Drawing.Size(150, 21);
			this.RightPresetSelectionBox.TabIndex = 1;
			this.RightPresetSelectionBox.SelectedValueChanged += new System.EventHandler(this.PresetSelectionBox_SelectedValueChanged);
			// 
			// LeftPresetSelectionBox
			// 
			this.LeftPresetSelectionBox.DisplayMember = "Name";
			this.LeftPresetSelectionBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPresetSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.LeftPresetSelectionBox.FormattingEnabled = true;
			this.LeftPresetSelectionBox.Location = new System.Drawing.Point(2, 2);
			this.LeftPresetSelectionBox.Margin = new System.Windows.Forms.Padding(2);
			this.LeftPresetSelectionBox.Name = "LeftPresetSelectionBox";
			this.LeftPresetSelectionBox.Size = new System.Drawing.Size(150, 21);
			this.LeftPresetSelectionBox.TabIndex = 0;
			this.LeftPresetSelectionBox.SelectedValueChanged += new System.EventHandler(this.PresetSelectionBox_SelectedValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(157, 3);
			this.label1.Margin = new System.Windows.Forms.Padding(3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 27);
			this.label1.TabIndex = 2;
			this.label1.Text = "<-- vs -->";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// ProcessPresetsButton
			// 
			this.ProcessPresetsButton.AutoSize = true;
			this.ProcessPresetsButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ProcessPresetsButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcessPresetsButton.Location = new System.Drawing.Point(382, 2);
			this.ProcessPresetsButton.Margin = new System.Windows.Forms.Padding(2);
			this.ProcessPresetsButton.Name = "ProcessPresetsButton";
			this.ProcessPresetsButton.Size = new System.Drawing.Size(148, 23);
			this.ProcessPresetsButton.TabIndex = 1;
			this.ProcessPresetsButton.Text = "Read Presets And Compare";
			this.ProcessPresetsButton.UseVisualStyleBackColor = true;
			this.ProcessPresetsButton.Click += new System.EventHandler(this.ProcessPresetsButton_Click);
			// 
			// ComparisonTabControl
			// 
			this.MainTable.SetColumnSpan(this.ComparisonTabControl, 3);
			this.ComparisonTabControl.Controls.Add(this.ModTabPage);
			this.ComparisonTabControl.Controls.Add(this.ItemTabPage);
			this.ComparisonTabControl.Controls.Add(this.RecipeTabPage);
			this.ComparisonTabControl.Controls.Add(this.AssemblerTabPage);
			this.ComparisonTabControl.Controls.Add(this.MinerTabPage);
			this.ComparisonTabControl.Controls.Add(this.PowerTabPage);
			this.ComparisonTabControl.Controls.Add(this.BeaconTabPage);
			this.ComparisonTabControl.Controls.Add(this.ModuleTabPage);
			this.ComparisonTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComparisonTabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.ComparisonTabControl.Location = new System.Drawing.Point(2, 56);
			this.ComparisonTabControl.Margin = new System.Windows.Forms.Padding(2);
			this.ComparisonTabControl.Name = "ComparisonTabControl";
			this.ComparisonTabControl.SelectedIndex = 0;
			this.ComparisonTabControl.Size = new System.Drawing.Size(659, 26);
			this.ComparisonTabControl.TabIndex = 2;
			this.ComparisonTabControl.SelectedIndexChanged += new System.EventHandler(this.ComparisonTabControl_SelectedIndexChanged);
			// 
			// ModTabPage
			// 
			this.ModTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.ModTabPage.Location = new System.Drawing.Point(4, 25);
			this.ModTabPage.Margin = new System.Windows.Forms.Padding(2);
			this.ModTabPage.Name = "ModTabPage";
			this.ModTabPage.Padding = new System.Windows.Forms.Padding(2);
			this.ModTabPage.Size = new System.Drawing.Size(651, 0);
			this.ModTabPage.TabIndex = 0;
			this.ModTabPage.Text = "Mods";
			// 
			// ItemTabPage
			// 
			this.ItemTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.ItemTabPage.Location = new System.Drawing.Point(4, 25);
			this.ItemTabPage.Margin = new System.Windows.Forms.Padding(2);
			this.ItemTabPage.Name = "ItemTabPage";
			this.ItemTabPage.Padding = new System.Windows.Forms.Padding(2);
			this.ItemTabPage.Size = new System.Drawing.Size(651, 0);
			this.ItemTabPage.TabIndex = 1;
			this.ItemTabPage.Text = "Items";
			// 
			// RecipeTabPage
			// 
			this.RecipeTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.RecipeTabPage.Location = new System.Drawing.Point(4, 25);
			this.RecipeTabPage.Margin = new System.Windows.Forms.Padding(2);
			this.RecipeTabPage.Name = "RecipeTabPage";
			this.RecipeTabPage.Size = new System.Drawing.Size(651, 0);
			this.RecipeTabPage.TabIndex = 2;
			this.RecipeTabPage.Text = "Recipes";
			// 
			// AssemblerTabPage
			// 
			this.AssemblerTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.AssemblerTabPage.Location = new System.Drawing.Point(4, 25);
			this.AssemblerTabPage.Margin = new System.Windows.Forms.Padding(2);
			this.AssemblerTabPage.Name = "AssemblerTabPage";
			this.AssemblerTabPage.Size = new System.Drawing.Size(651, 0);
			this.AssemblerTabPage.TabIndex = 3;
			this.AssemblerTabPage.Text = "Assemblers";
			// 
			// MinerTabPage
			// 
			this.MinerTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.MinerTabPage.Location = new System.Drawing.Point(4, 25);
			this.MinerTabPage.Margin = new System.Windows.Forms.Padding(2);
			this.MinerTabPage.Name = "MinerTabPage";
			this.MinerTabPage.Size = new System.Drawing.Size(651, 0);
			this.MinerTabPage.TabIndex = 4;
			this.MinerTabPage.Text = "Miners";
			// 
			// PowerTabPage
			// 
			this.PowerTabPage.Location = new System.Drawing.Point(4, 25);
			this.PowerTabPage.Name = "PowerTabPage";
			this.PowerTabPage.Size = new System.Drawing.Size(651, 0);
			this.PowerTabPage.TabIndex = 6;
			this.PowerTabPage.Text = "Power";
			this.PowerTabPage.UseVisualStyleBackColor = true;
			// 
			// BeaconTabPage
			// 
			this.BeaconTabPage.Location = new System.Drawing.Point(4, 25);
			this.BeaconTabPage.Name = "BeaconTabPage";
			this.BeaconTabPage.Size = new System.Drawing.Size(651, 0);
			this.BeaconTabPage.TabIndex = 7;
			this.BeaconTabPage.Text = "Beacons";
			this.BeaconTabPage.UseVisualStyleBackColor = true;
			// 
			// ModuleTabPage
			// 
			this.ModuleTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			this.ModuleTabPage.Location = new System.Drawing.Point(4, 25);
			this.ModuleTabPage.Margin = new System.Windows.Forms.Padding(2);
			this.ModuleTabPage.Name = "ModuleTabPage";
			this.ModuleTabPage.Size = new System.Drawing.Size(651, 0);
			this.ModuleTabPage.TabIndex = 5;
			this.ModuleTabPage.Text = "Modules";
			// 
			// CloseButton
			// 
			this.CloseButton.AutoSize = true;
			this.CloseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CloseButton.Location = new System.Drawing.Point(382, 29);
			this.CloseButton.Margin = new System.Windows.Forms.Padding(2);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = new System.Drawing.Size(148, 23);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Text = "Close";
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// TabTable
			// 
			this.TabTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TabTable.ColumnCount = 4;
			this.MainTable.SetColumnSpan(this.TabTable, 3);
			this.TabTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TabTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TabTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TabTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.TabTable.Controls.Add(this.label2, 3, 1);
			this.TabTable.Controls.Add(this.LeftOnlyListView, 0, 2);
			this.TabTable.Controls.Add(this.label5, 0, 1);
			this.TabTable.Controls.Add(this.LeftListView, 1, 2);
			this.TabTable.Controls.Add(this.RightListView, 2, 2);
			this.TabTable.Controls.Add(this.RightOnlyListView, 3, 2);
			this.TabTable.Controls.Add(this.BothPresetsTable, 1, 1);
			this.TabTable.Controls.Add(this.FilterTable, 0, 0);
			this.TabTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabTable.Location = new System.Drawing.Point(2, 86);
			this.TabTable.Margin = new System.Windows.Forms.Padding(2);
			this.TabTable.Name = "TabTable";
			this.TabTable.RowCount = 3;
			this.TabTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TabTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TabTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TabTable.Size = new System.Drawing.Size(659, 287);
			this.TabTable.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(510, 29);
			this.label2.Margin = new System.Windows.Forms.Padding(18, 3, 3, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(146, 17);
			this.label2.TabIndex = 0;
			this.label2.Text = "Right Preset Exclusive:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LeftOnlyListView
			// 
			this.LeftOnlyListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LeftOnlyHeader});
			this.LeftOnlyListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftOnlyListView.FullRowSelect = true;
			this.LeftOnlyListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.LeftOnlyListView.HideSelection = false;
			this.LeftOnlyListView.Location = new System.Drawing.Point(8, 51);
			this.LeftOnlyListView.Margin = new System.Windows.Forms.Padding(8, 2, 15, 2);
			this.LeftOnlyListView.MultiSelect = false;
			this.LeftOnlyListView.Name = "LeftOnlyListView";
			this.LeftOnlyListView.Size = new System.Drawing.Size(141, 234);
			this.LeftOnlyListView.SmallImageList = this.IconList;
			this.LeftOnlyListView.TabIndex = 4;
			this.LeftOnlyListView.UseCompatibleStateImageBehavior = false;
			this.LeftOnlyListView.View = System.Windows.Forms.View.Details;
			this.LeftOnlyListView.VirtualMode = true;
			this.LeftOnlyListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.LeftOnlyListView_RetrieveVirtualItem);
			this.LeftOnlyListView.Resize += new System.EventHandler(this.LeftOnlyListView_Resize);
			// 
			// LeftOnlyHeader
			// 
			this.LeftOnlyHeader.Text = "";
			// 
			// IconList
			// 
			this.IconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.IconList.ImageSize = new System.Drawing.Size(32, 32);
			this.IconList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(11, 29);
			this.label5.Margin = new System.Windows.Forms.Padding(11, 3, 3, 3);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(150, 17);
			this.label5.TabIndex = 3;
			this.label5.Text = "Left Preset Exclusive:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LeftListView
			// 
			this.LeftListView.Buddy = this.RightListView;
			this.LeftListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LeftHeader});
			this.LeftListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftListView.FullRowSelect = true;
			this.LeftListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.LeftListView.HideSelection = false;
			this.LeftListView.Location = new System.Drawing.Point(186, 51);
			this.LeftListView.Margin = new System.Windows.Forms.Padding(22, 2, 0, 2);
			this.LeftListView.MultiSelect = false;
			this.LeftListView.Name = "LeftListView";
			this.LeftListView.Size = new System.Drawing.Size(142, 234);
			this.LeftListView.SmallImageList = this.IconList;
			this.LeftListView.TabIndex = 5;
			this.LeftListView.UseCompatibleStateImageBehavior = false;
			this.LeftListView.View = System.Windows.Forms.View.Details;
			this.LeftListView.VirtualMode = true;
			this.LeftListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.LeftListView_RetrieveVirtualItem);
			this.LeftListView.Resize += new System.EventHandler(this.LeftListView_Resize);
			// 
			// RightListView
			// 
			this.RightListView.Buddy = this.LeftListView;
			this.RightListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.RightHeader});
			this.RightListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightListView.FullRowSelect = true;
			this.RightListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.RightListView.HideSelection = false;
			this.RightListView.Location = new System.Drawing.Point(328, 51);
			this.RightListView.Margin = new System.Windows.Forms.Padding(0, 2, 22, 2);
			this.RightListView.MultiSelect = false;
			this.RightListView.Name = "RightListView";
			this.RightListView.Size = new System.Drawing.Size(142, 234);
			this.RightListView.SmallImageList = this.IconList;
			this.RightListView.TabIndex = 6;
			this.RightListView.UseCompatibleStateImageBehavior = false;
			this.RightListView.View = System.Windows.Forms.View.Details;
			this.RightListView.VirtualMode = true;
			this.RightListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.RightListView_RetrieveVirtualItem);
			this.RightListView.Resize += new System.EventHandler(this.RightListView_Resize);
			// 
			// RightHeader
			// 
			this.RightHeader.Text = "";
			// 
			// LeftHeader
			// 
			this.LeftHeader.Text = "";
			// 
			// RightOnlyListView
			// 
			this.RightOnlyListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.RightOnlyHeader});
			this.RightOnlyListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightOnlyListView.FullRowSelect = true;
			this.RightOnlyListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.RightOnlyListView.HideSelection = false;
			this.RightOnlyListView.Location = new System.Drawing.Point(507, 51);
			this.RightOnlyListView.Margin = new System.Windows.Forms.Padding(15, 2, 8, 2);
			this.RightOnlyListView.MultiSelect = false;
			this.RightOnlyListView.Name = "RightOnlyListView";
			this.RightOnlyListView.Size = new System.Drawing.Size(144, 234);
			this.RightOnlyListView.SmallImageList = this.IconList;
			this.RightOnlyListView.TabIndex = 7;
			this.RightOnlyListView.UseCompatibleStateImageBehavior = false;
			this.RightOnlyListView.View = System.Windows.Forms.View.Details;
			this.RightOnlyListView.VirtualMode = true;
			this.RightOnlyListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.RightOnlyListView_RetrieveVirtualItem);
			this.RightOnlyListView.Resize += new System.EventHandler(this.RightOnlyListView_Resize);
			// 
			// RightOnlyHeader
			// 
			this.RightOnlyHeader.Text = "";
			// 
			// BothPresetsTable
			// 
			this.BothPresetsTable.AutoSize = true;
			this.BothPresetsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BothPresetsTable.ColumnCount = 3;
			this.TabTable.SetColumnSpan(this.BothPresetsTable, 2);
			this.BothPresetsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.BothPresetsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.BothPresetsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.BothPresetsTable.Controls.Add(this.HideSimilarObjectsCheckBox, 2, 0);
			this.BothPresetsTable.Controls.Add(this.label4, 0, 0);
			this.BothPresetsTable.Controls.Add(this.HideEqualObjectsCheckBox, 1, 0);
			this.BothPresetsTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BothPresetsTable.Location = new System.Drawing.Point(186, 26);
			this.BothPresetsTable.Margin = new System.Windows.Forms.Padding(22, 0, 22, 0);
			this.BothPresetsTable.Name = "BothPresetsTable";
			this.BothPresetsTable.RowCount = 1;
			this.BothPresetsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.BothPresetsTable.Size = new System.Drawing.Size(284, 23);
			this.BothPresetsTable.TabIndex = 9;
			// 
			// HideSimilarObjectsCheckBox
			// 
			this.HideSimilarObjectsCheckBox.AutoSize = true;
			this.HideSimilarObjectsCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HideSimilarObjectsCheckBox.Location = new System.Drawing.Point(175, 3);
			this.HideSimilarObjectsCheckBox.Name = "HideSimilarObjectsCheckBox";
			this.HideSimilarObjectsCheckBox.Size = new System.Drawing.Size(106, 17);
			this.HideSimilarObjectsCheckBox.TabIndex = 4;
			this.HideSimilarObjectsCheckBox.Text = "Hide Similar";
			this.HideSimilarObjectsCheckBox.UseVisualStyleBackColor = true;
			this.HideSimilarObjectsCheckBox.CheckedChanged += new System.EventHandler(this.Filters_Changed);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(3, 3);
			this.label4.Margin = new System.Windows.Forms.Padding(3);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(82, 17);
			this.label4.TabIndex = 2;
			this.label4.Text = "Both Presets:    ";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// HideEqualObjectsCheckBox
			// 
			this.HideEqualObjectsCheckBox.AutoSize = true;
			this.HideEqualObjectsCheckBox.Checked = true;
			this.HideEqualObjectsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.HideEqualObjectsCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HideEqualObjectsCheckBox.Location = new System.Drawing.Point(91, 3);
			this.HideEqualObjectsCheckBox.Name = "HideEqualObjectsCheckBox";
			this.HideEqualObjectsCheckBox.Size = new System.Drawing.Size(78, 17);
			this.HideEqualObjectsCheckBox.TabIndex = 3;
			this.HideEqualObjectsCheckBox.Text = "Hide Equal";
			this.HideEqualObjectsCheckBox.UseVisualStyleBackColor = true;
			this.HideEqualObjectsCheckBox.CheckedChanged += new System.EventHandler(this.Filters_Changed);
			// 
			// FilterTable
			// 
			this.FilterTable.AutoSize = true;
			this.FilterTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.FilterTable.ColumnCount = 3;
			this.TabTable.SetColumnSpan(this.FilterTable, 4);
			this.FilterTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.FilterTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.FilterTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.FilterTable.Controls.Add(this.ShowUnavailableCheckBox, 2, 0);
			this.FilterTable.Controls.Add(this.label6, 0, 0);
			this.FilterTable.Controls.Add(this.FilterTextBox, 1, 0);
			this.FilterTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterTable.Location = new System.Drawing.Point(0, 0);
			this.FilterTable.Margin = new System.Windows.Forms.Padding(0);
			this.FilterTable.Name = "FilterTable";
			this.FilterTable.RowCount = 1;
			this.FilterTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.FilterTable.Size = new System.Drawing.Size(659, 26);
			this.FilterTable.TabIndex = 10;
			// 
			// ShowUnavailableCheckBox
			// 
			this.ShowUnavailableCheckBox.AutoSize = true;
			this.ShowUnavailableCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShowUnavailableCheckBox.Location = new System.Drawing.Point(191, 3);
			this.ShowUnavailableCheckBox.Name = "ShowUnavailableCheckBox";
			this.ShowUnavailableCheckBox.Size = new System.Drawing.Size(465, 20);
			this.ShowUnavailableCheckBox.TabIndex = 6;
			this.ShowUnavailableCheckBox.Text = "Show Unavailables";
			this.ShowUnavailableCheckBox.UseVisualStyleBackColor = true;
			this.ShowUnavailableCheckBox.CheckedChanged += new System.EventHandler(this.Filters_Changed);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Location = new System.Drawing.Point(3, 3);
			this.label6.Margin = new System.Windows.Forms.Padding(3);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(32, 20);
			this.label6.TabIndex = 5;
			this.label6.Text = "Filter:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FilterTextBox
			// 
			this.FilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterTextBox.Location = new System.Drawing.Point(41, 3);
			this.FilterTextBox.Name = "FilterTextBox";
			this.FilterTextBox.Size = new System.Drawing.Size(144, 20);
			this.FilterTextBox.TabIndex = 4;
			this.FilterTextBox.TextChanged += new System.EventHandler(this.Filters_Changed);
			// 
			// TextToolTip
			// 
			this.TextToolTip.AutoPopDelay = 100000;
			this.TextToolTip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
			this.TextToolTip.ForeColor = System.Drawing.Color.White;
			this.TextToolTip.InitialDelay = 200;
			this.TextToolTip.OwnerDraw = true;
			this.TextToolTip.ReshowDelay = 100;
			this.TextToolTip.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			// 
			// RecipeToolTip
			// 
			this.RecipeToolTip.AutoPopDelay = 100000;
			this.RecipeToolTip.BackColor = System.Drawing.Color.DimGray;
			this.RecipeToolTip.ForeColor = System.Drawing.Color.White;
			this.RecipeToolTip.InitialDelay = 200;
			this.RecipeToolTip.OwnerDraw = true;
			this.RecipeToolTip.ReshowDelay = 100;
			// 
			// MainTable
			// 
			this.MainTable.AutoSize = true;
			this.MainTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTable.ColumnCount = 3;
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MainTable.Controls.Add(this.PresetSelectionGroup, 0, 0);
			this.MainTable.Controls.Add(this.TabTable, 0, 3);
			this.MainTable.Controls.Add(this.ProcessPresetsButton, 1, 0);
			this.MainTable.Controls.Add(this.CloseButton, 1, 1);
			this.MainTable.Controls.Add(this.ComparisonTabControl, 0, 2);
			this.MainTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTable.Location = new System.Drawing.Point(0, 0);
			this.MainTable.Name = "MainTable";
			this.MainTable.RowCount = 4;
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.Size = new System.Drawing.Size(663, 375);
			this.MainTable.TabIndex = 4;
			// 
			// PresetComparatorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CloseButton;
			this.ClientSize = new System.Drawing.Size(663, 375);
			this.Controls.Add(this.MainTable);
			this.DoubleBuffered = true;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MinimumSize = new System.Drawing.Size(679, 414);
			this.Name = "PresetComparatorForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Preset Comparer";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PresetComparatorForm_FormClosed);
			this.PresetSelectionGroup.ResumeLayout(false);
			this.PresetSelectionGroup.PerformLayout();
			this.VsTable.ResumeLayout(false);
			this.VsTable.PerformLayout();
			this.ComparisonTabControl.ResumeLayout(false);
			this.TabTable.ResumeLayout(false);
			this.TabTable.PerformLayout();
			this.BothPresetsTable.ResumeLayout(false);
			this.BothPresetsTable.PerformLayout();
			this.FilterTable.ResumeLayout(false);
			this.FilterTable.PerformLayout();
			this.MainTable.ResumeLayout(false);
			this.MainTable.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox PresetSelectionGroup;
        private System.Windows.Forms.ComboBox LeftPresetSelectionBox;
        private System.Windows.Forms.Button ProcessPresetsButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox RightPresetSelectionBox;
        private System.Windows.Forms.TabControl ComparisonTabControl;
        private System.Windows.Forms.TabPage ItemTabPage;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.TabPage RecipeTabPage;
        private System.Windows.Forms.TabPage AssemblerTabPage;
        private System.Windows.Forms.TabPage MinerTabPage;
        private System.Windows.Forms.TabPage ModuleTabPage;
        private System.Windows.Forms.TableLayoutPanel TabTable;
        private FFListView RightOnlyListView;
        private SyncListView RightListView;
        private SyncListView LeftListView;
        private FFListView LeftOnlyListView;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ImageList IconList;
        private System.Windows.Forms.ColumnHeader RightOnlyHeader;
        private System.Windows.Forms.ColumnHeader LeftHeader;
        private System.Windows.Forms.ColumnHeader RightHeader;
        private System.Windows.Forms.ColumnHeader LeftOnlyHeader;
        private CustomToolTip TextToolTip;
        private RecipeToolTip RecipeToolTip;
        private System.Windows.Forms.CheckBox HideEqualObjectsCheckBox;
        private System.Windows.Forms.CheckBox HideSimilarObjectsCheckBox;
		private System.Windows.Forms.CheckBox ShowUnavailableCheckBox;
		private System.Windows.Forms.TableLayoutPanel BothPresetsTable;
		private System.Windows.Forms.TableLayoutPanel FilterTable;
		private System.Windows.Forms.TableLayoutPanel VsTable;
		private System.Windows.Forms.TableLayoutPanel MainTable;
		private System.Windows.Forms.TabPage ModTabPage;
		private System.Windows.Forms.TabPage PowerTabPage;
		private System.Windows.Forms.TabPage BeaconTabPage;
	}
}