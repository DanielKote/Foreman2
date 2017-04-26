namespace Foreman
{
	partial class MainForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.MainToolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rateOptionsDropDown = new System.Windows.Forms.ComboBox();
            this.rateButton = new System.Windows.Forms.RadioButton();
            this.fixedAmountButton = new System.Windows.Forms.RadioButton();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.AutomaticCompleteButton = new System.Windows.Forms.Button();
            this.ArrangeNodesButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.saveGraphButton = new System.Windows.Forms.Button();
            this.loadGraphButton = new System.Windows.Forms.Button();
            this.ExportImageButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.FactorioDirectoryButton = new System.Windows.Forms.Button();
            this.ModDirectoryButton = new System.Windows.Forms.Button();
            this.ReloadButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.EnableDisableButton = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.LanguageDropDown = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.MinersUseModulesCheckBox = new System.Windows.Forms.CheckBox();
            this.SingleAssemblerPerRecipeCheckBox = new System.Windows.Forms.CheckBox();
            this.AssemblerDisplayCheckBox = new System.Windows.Forms.CheckBox();
            this.MinerDisplayCheckBox = new System.Windows.Forms.CheckBox();
            this.GraphViewer = new Foreman.ProductionGraphViewer();
            this.ListTabControl = new System.Windows.Forms.TabControl();
            this.ItemTabPage = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.ItemFilterTextBox = new System.Windows.Forms.TextBox();
            this.AddItemButton = new System.Windows.Forms.Button();
            this.ItemListView = new System.Windows.Forms.ListView();
            this.h_Name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ItemImageList = new System.Windows.Forms.ImageList(this.components);
            this.RecipeTabPage = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.RecipeFilterTextBox = new System.Windows.Forms.TextBox();
            this.AddRecipeButton = new System.Windows.Forms.Button();
            this.RecipeListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RecipeImageList = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.MainToolbar.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.ListTabControl.SuspendLayout();
            this.ItemTabPage.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.RecipeTabPage.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 210F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.MainToolbar, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.GraphViewer, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.ListTabControl, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1306, 800);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // MainToolbar
            // 
            this.MainToolbar.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.MainToolbar, 2);
            this.MainToolbar.Controls.Add(this.groupBox1);
            this.MainToolbar.Controls.Add(this.flowLayoutPanel1);
            this.MainToolbar.Controls.Add(this.flowLayoutPanel3);
            this.MainToolbar.Controls.Add(this.flowLayoutPanel2);
            this.MainToolbar.Controls.Add(this.panel1);
            this.MainToolbar.Controls.Add(this.groupBox2);
            this.MainToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainToolbar.Location = new System.Drawing.Point(3, 3);
            this.MainToolbar.Name = "MainToolbar";
            this.MainToolbar.Size = new System.Drawing.Size(1300, 99);
            this.MainToolbar.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.rateOptionsDropDown);
            this.groupBox1.Controls.Add(this.rateButton);
            this.groupBox1.Controls.Add(this.fixedAmountButton);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(192, 82);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Show production amounts as...";
            // 
            // rateOptionsDropDown
            // 
            this.rateOptionsDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rateOptionsDropDown.Enabled = false;
            this.rateOptionsDropDown.FormattingEnabled = true;
            this.rateOptionsDropDown.Items.AddRange(new object[] {
            "per Second",
            "per Minute"});
            this.rateOptionsDropDown.Location = new System.Drawing.Point(63, 42);
            this.rateOptionsDropDown.Name = "rateOptionsDropDown";
            this.rateOptionsDropDown.Size = new System.Drawing.Size(123, 21);
            this.rateOptionsDropDown.TabIndex = 2;
            this.rateOptionsDropDown.SelectedIndexChanged += new System.EventHandler(this.rateOptionsDropDown_SelectedIndexChanged);
            // 
            // rateButton
            // 
            this.rateButton.AutoSize = true;
            this.rateButton.Location = new System.Drawing.Point(9, 42);
            this.rateButton.Name = "rateButton";
            this.rateButton.Size = new System.Drawing.Size(48, 17);
            this.rateButton.TabIndex = 1;
            this.rateButton.Text = "Rate";
            this.rateButton.UseVisualStyleBackColor = true;
            this.rateButton.CheckedChanged += new System.EventHandler(this.rateButton_CheckedChanged);
            // 
            // fixedAmountButton
            // 
            this.fixedAmountButton.AutoSize = true;
            this.fixedAmountButton.Checked = true;
            this.fixedAmountButton.Location = new System.Drawing.Point(9, 19);
            this.fixedAmountButton.Name = "fixedAmountButton";
            this.fixedAmountButton.Size = new System.Drawing.Size(89, 17);
            this.fixedAmountButton.TabIndex = 0;
            this.fixedAmountButton.TabStop = true;
            this.fixedAmountButton.Text = "Fixed Amount";
            this.fixedAmountButton.UseVisualStyleBackColor = true;
            this.fixedAmountButton.CheckedChanged += new System.EventHandler(this.fixedAmountButton_CheckedChanged);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.AutomaticCompleteButton);
            this.flowLayoutPanel1.Controls.Add(this.ArrangeNodesButton);
            this.flowLayoutPanel1.Controls.Add(this.ClearButton);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(201, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(188, 93);
            this.flowLayoutPanel1.TabIndex = 9;
            // 
            // AutomaticCompleteButton
            // 
            this.AutomaticCompleteButton.Location = new System.Drawing.Point(3, 3);
            this.AutomaticCompleteButton.Name = "AutomaticCompleteButton";
            this.AutomaticCompleteButton.Size = new System.Drawing.Size(182, 25);
            this.AutomaticCompleteButton.TabIndex = 5;
            this.AutomaticCompleteButton.Text = "Automatically create missing nodes";
            this.AutomaticCompleteButton.UseVisualStyleBackColor = true;
            this.AutomaticCompleteButton.Click += new System.EventHandler(this.AutomaticCompleteButton_Click);
            // 
            // ArrangeNodesButton
            // 
            this.ArrangeNodesButton.Location = new System.Drawing.Point(3, 34);
            this.ArrangeNodesButton.Name = "ArrangeNodesButton";
            this.ArrangeNodesButton.Size = new System.Drawing.Size(182, 25);
            this.ArrangeNodesButton.TabIndex = 7;
            this.ArrangeNodesButton.Text = "Reposition nodes";
            this.ArrangeNodesButton.UseVisualStyleBackColor = true;
            this.ArrangeNodesButton.Click += new System.EventHandler(this.ArrangeNodesButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(3, 65);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(182, 25);
            this.ClearButton.TabIndex = 6;
            this.ClearButton.Text = "Clear flowchart";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel3.Controls.Add(this.saveGraphButton);
            this.flowLayoutPanel3.Controls.Add(this.loadGraphButton);
            this.flowLayoutPanel3.Controls.Add(this.ExportImageButton);
            this.flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(395, 3);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(124, 93);
            this.flowLayoutPanel3.TabIndex = 14;
            // 
            // saveGraphButton
            // 
            this.saveGraphButton.Location = new System.Drawing.Point(3, 3);
            this.saveGraphButton.Name = "saveGraphButton";
            this.saveGraphButton.Size = new System.Drawing.Size(118, 25);
            this.saveGraphButton.TabIndex = 9;
            this.saveGraphButton.Text = "Save";
            this.saveGraphButton.UseVisualStyleBackColor = true;
            this.saveGraphButton.Click += new System.EventHandler(this.saveGraphButton_Click);
            // 
            // loadGraphButton
            // 
            this.loadGraphButton.Location = new System.Drawing.Point(3, 34);
            this.loadGraphButton.Name = "loadGraphButton";
            this.loadGraphButton.Size = new System.Drawing.Size(118, 25);
            this.loadGraphButton.TabIndex = 10;
            this.loadGraphButton.Text = "Load";
            this.loadGraphButton.UseVisualStyleBackColor = true;
            this.loadGraphButton.Click += new System.EventHandler(this.loadGraphButton_Click);
            // 
            // ExportImageButton
            // 
            this.ExportImageButton.Location = new System.Drawing.Point(3, 65);
            this.ExportImageButton.Name = "ExportImageButton";
            this.ExportImageButton.Size = new System.Drawing.Size(118, 25);
            this.ExportImageButton.TabIndex = 8;
            this.ExportImageButton.Text = "Export as Image";
            this.ExportImageButton.UseVisualStyleBackColor = true;
            this.ExportImageButton.Click += new System.EventHandler(this.ExportImageButton_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.FactorioDirectoryButton);
            this.flowLayoutPanel2.Controls.Add(this.ModDirectoryButton);
            this.flowLayoutPanel2.Controls.Add(this.ReloadButton);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(525, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(125, 90);
            this.flowLayoutPanel2.TabIndex = 13;
            // 
            // FactorioDirectoryButton
            // 
            this.FactorioDirectoryButton.Location = new System.Drawing.Point(3, 3);
            this.FactorioDirectoryButton.Name = "FactorioDirectoryButton";
            this.FactorioDirectoryButton.Size = new System.Drawing.Size(119, 24);
            this.FactorioDirectoryButton.TabIndex = 5;
            this.FactorioDirectoryButton.Text = "Factorio Directory...";
            this.FactorioDirectoryButton.UseVisualStyleBackColor = true;
            this.FactorioDirectoryButton.Click += new System.EventHandler(this.FactorioDirectoryButton_Click);
            // 
            // ModDirectoryButton
            // 
            this.ModDirectoryButton.Location = new System.Drawing.Point(3, 33);
            this.ModDirectoryButton.Name = "ModDirectoryButton";
            this.ModDirectoryButton.Size = new System.Drawing.Size(119, 24);
            this.ModDirectoryButton.TabIndex = 6;
            this.ModDirectoryButton.Text = "Mod Directory...";
            this.ModDirectoryButton.UseVisualStyleBackColor = true;
            this.ModDirectoryButton.Click += new System.EventHandler(this.ModDirectoryButton_Click);
            // 
            // ReloadButton
            // 
            this.ReloadButton.Location = new System.Drawing.Point(3, 63);
            this.ReloadButton.Name = "ReloadButton";
            this.ReloadButton.Size = new System.Drawing.Size(119, 24);
            this.ReloadButton.TabIndex = 7;
            this.ReloadButton.Text = "Reload";
            this.ReloadButton.UseVisualStyleBackColor = true;
            this.ReloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.EnableDisableButton);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Location = new System.Drawing.Point(656, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(167, 90);
            this.panel1.TabIndex = 16;
            // 
            // EnableDisableButton
            // 
            this.EnableDisableButton.Location = new System.Drawing.Point(3, 63);
            this.EnableDisableButton.Name = "EnableDisableButton";
            this.EnableDisableButton.Size = new System.Drawing.Size(159, 25);
            this.EnableDisableButton.TabIndex = 7;
            this.EnableDisableButton.Text = "Enable/disable loaded objects";
            this.EnableDisableButton.UseVisualStyleBackColor = true;
            this.EnableDisableButton.Click += new System.EventHandler(this.EnableDisableButton_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.AutoSize = true;
            this.groupBox3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox3.Controls.Add(this.LanguageDropDown);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.groupBox3.Size = new System.Drawing.Size(159, 57);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Language";
            // 
            // LanguageDropDown
            // 
            this.LanguageDropDown.DisplayMember = "LocalName";
            this.LanguageDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LanguageDropDown.FormattingEnabled = true;
            this.LanguageDropDown.Location = new System.Drawing.Point(6, 20);
            this.LanguageDropDown.MaxDropDownItems = 20;
            this.LanguageDropDown.Name = "LanguageDropDown";
            this.LanguageDropDown.Size = new System.Drawing.Size(147, 21);
            this.LanguageDropDown.TabIndex = 1;
            this.LanguageDropDown.ValueMember = "Name";
            this.LanguageDropDown.SelectedIndexChanged += new System.EventHandler(this.LanguageDropDown_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox2.Controls.Add(this.MinersUseModulesCheckBox);
            this.groupBox2.Controls.Add(this.SingleAssemblerPerRecipeCheckBox);
            this.groupBox2.Controls.Add(this.AssemblerDisplayCheckBox);
            this.groupBox2.Controls.Add(this.MinerDisplayCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(829, 3);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.groupBox2.Size = new System.Drawing.Size(300, 95);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Assemblers";
            // 
            // MinersUseModulesCheckBox
            // 
            this.MinersUseModulesCheckBox.AutoSize = true;
            this.MinersUseModulesCheckBox.Location = new System.Drawing.Point(175, 20);
            this.MinersUseModulesCheckBox.Name = "MinersUseModulesCheckBox";
            this.MinersUseModulesCheckBox.Size = new System.Drawing.Size(119, 17);
            this.MinersUseModulesCheckBox.TabIndex = 2;
            this.MinersUseModulesCheckBox.Text = "Miners use modules";
            this.MinersUseModulesCheckBox.UseVisualStyleBackColor = true;
            this.MinersUseModulesCheckBox.CheckedChanged += new System.EventHandler(this.MinersUseModulesCheckBox_CheckedChanged);
            // 
            // SingleAssemblerPerRecipeCheckBox
            // 
            this.SingleAssemblerPerRecipeCheckBox.AutoSize = true;
            this.SingleAssemblerPerRecipeCheckBox.Location = new System.Drawing.Point(7, 43);
            this.SingleAssemblerPerRecipeCheckBox.Name = "SingleAssemblerPerRecipeCheckBox";
            this.SingleAssemblerPerRecipeCheckBox.Size = new System.Drawing.Size(169, 17);
            this.SingleAssemblerPerRecipeCheckBox.TabIndex = 1;
            this.SingleAssemblerPerRecipeCheckBox.Text = "One assembler type per recipe";
            this.SingleAssemblerPerRecipeCheckBox.UseVisualStyleBackColor = true;
            this.SingleAssemblerPerRecipeCheckBox.CheckedChanged += new System.EventHandler(this.SingleAssemblerPerRecipeCheckBox_CheckedChanged);
            // 
            // AssemblerDisplayCheckBox
            // 
            this.AssemblerDisplayCheckBox.AutoSize = true;
            this.AssemblerDisplayCheckBox.Enabled = false;
            this.AssemblerDisplayCheckBox.Location = new System.Drawing.Point(7, 20);
            this.AssemblerDisplayCheckBox.Name = "AssemblerDisplayCheckBox";
            this.AssemblerDisplayCheckBox.Size = new System.Drawing.Size(116, 17);
            this.AssemblerDisplayCheckBox.TabIndex = 0;
            this.AssemblerDisplayCheckBox.Text = "Display Assemblers";
            this.AssemblerDisplayCheckBox.UseVisualStyleBackColor = true;
            this.AssemblerDisplayCheckBox.CheckedChanged += new System.EventHandler(this.AssemblerDisplayCheckBox_CheckedChanged);
            // 
            // MinerDisplayCheckBox
            // 
            this.MinerDisplayCheckBox.AutoSize = true;
            this.MinerDisplayCheckBox.Enabled = false;
            this.MinerDisplayCheckBox.Location = new System.Drawing.Point(7, 65);
            this.MinerDisplayCheckBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.MinerDisplayCheckBox.Name = "MinerDisplayCheckBox";
            this.MinerDisplayCheckBox.Size = new System.Drawing.Size(151, 17);
            this.MinerDisplayCheckBox.TabIndex = 0;
            this.MinerDisplayCheckBox.Text = "Display Miners/Pumpjacks";
            this.MinerDisplayCheckBox.UseVisualStyleBackColor = true;
            // 
            // GraphViewer
            // 
            this.GraphViewer.AllowDrop = true;
            this.GraphViewer.BackColor = System.Drawing.Color.White;
            this.GraphViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GraphViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GraphViewer.DraggedElement = null;
            this.GraphViewer.Location = new System.Drawing.Point(213, 108);
            this.GraphViewer.Name = "GraphViewer";
            this.GraphViewer.Size = new System.Drawing.Size(1090, 689);
            this.GraphViewer.TabIndex = 12;
            // 
            // ListTabControl
            // 
            this.ListTabControl.Controls.Add(this.ItemTabPage);
            this.ListTabControl.Controls.Add(this.RecipeTabPage);
            this.ListTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListTabControl.Location = new System.Drawing.Point(0, 105);
            this.ListTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.ListTabControl.Name = "ListTabControl";
            this.ListTabControl.Padding = new System.Drawing.Point(0, 0);
            this.ListTabControl.SelectedIndex = 0;
            this.ListTabControl.Size = new System.Drawing.Size(210, 695);
            this.ListTabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.ListTabControl.TabIndex = 17;
            // 
            // ItemTabPage
            // 
            this.ItemTabPage.Controls.Add(this.tableLayoutPanel2);
            this.ItemTabPage.Location = new System.Drawing.Point(4, 22);
            this.ItemTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.ItemTabPage.Name = "ItemTabPage";
            this.ItemTabPage.Size = new System.Drawing.Size(202, 669);
            this.ItemTabPage.TabIndex = 0;
            this.ItemTabPage.Text = "Items";
            this.ItemTabPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.ItemFilterTextBox, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.AddItemButton, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.ItemListView, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(202, 669);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // ItemFilterTextBox
            // 
            this.ItemFilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemFilterTextBox.Location = new System.Drawing.Point(3, 3);
            this.ItemFilterTextBox.Name = "ItemFilterTextBox";
            this.ItemFilterTextBox.Size = new System.Drawing.Size(196, 20);
            this.ItemFilterTextBox.TabIndex = 19;
            this.ItemFilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.ItemFilterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            // 
            // AddItemButton
            // 
            this.AddItemButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddItemButton.Location = new System.Drawing.Point(3, 641);
            this.AddItemButton.Name = "AddItemButton";
            this.AddItemButton.Size = new System.Drawing.Size(196, 25);
            this.AddItemButton.TabIndex = 18;
            this.AddItemButton.Text = "Add Item";
            this.AddItemButton.UseVisualStyleBackColor = true;
            this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
            // 
            // ItemListView
            // 
            this.ItemListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.h_Name});
            this.ItemListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemListView.FullRowSelect = true;
            this.ItemListView.GridLines = true;
            this.ItemListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.ItemListView.HideSelection = false;
            this.ItemListView.LabelWrap = false;
            this.ItemListView.Location = new System.Drawing.Point(3, 29);
            this.ItemListView.Name = "ItemListView";
            this.ItemListView.Size = new System.Drawing.Size(196, 606);
            this.ItemListView.SmallImageList = this.ItemImageList;
            this.ItemListView.TabIndex = 15;
            this.ItemListView.UseCompatibleStateImageBehavior = false;
            this.ItemListView.View = System.Windows.Forms.View.Details;
            this.ItemListView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ItemListView_ItemDrag);
            this.ItemListView.SelectedIndexChanged += new System.EventHandler(this.ItemListView_SelectedIndexChanged);
            this.ItemListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ItemListView_MouseDoubleClick);
            // 
            // h_Name
            // 
            this.h_Name.Text = "Name";
            this.h_Name.Width = 175;
            // 
            // ItemImageList
            // 
            this.ItemImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.ItemImageList.ImageSize = new System.Drawing.Size(32, 32);
            this.ItemImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // RecipeTabPage
            // 
            this.RecipeTabPage.Controls.Add(this.tableLayoutPanel3);
            this.RecipeTabPage.Location = new System.Drawing.Point(4, 22);
            this.RecipeTabPage.Name = "RecipeTabPage";
            this.RecipeTabPage.Size = new System.Drawing.Size(202, 669);
            this.RecipeTabPage.TabIndex = 1;
            this.RecipeTabPage.Text = "Recipes";
            this.RecipeTabPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.RecipeFilterTextBox, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.AddRecipeButton, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.RecipeListView, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(202, 669);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // RecipeFilterTextBox
            // 
            this.RecipeFilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecipeFilterTextBox.Location = new System.Drawing.Point(3, 3);
            this.RecipeFilterTextBox.Name = "RecipeFilterTextBox";
            this.RecipeFilterTextBox.Size = new System.Drawing.Size(196, 20);
            this.RecipeFilterTextBox.TabIndex = 19;
            this.RecipeFilterTextBox.TextChanged += new System.EventHandler(this.RecipeFilterTextBox_TextChanged);
            this.RecipeFilterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RecipeFilterTextBox_KeyDown);
            // 
            // AddRecipeButton
            // 
            this.AddRecipeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddRecipeButton.Location = new System.Drawing.Point(3, 641);
            this.AddRecipeButton.Name = "AddRecipeButton";
            this.AddRecipeButton.Size = new System.Drawing.Size(196, 25);
            this.AddRecipeButton.TabIndex = 18;
            this.AddRecipeButton.Text = "Add Recipe";
            this.AddRecipeButton.UseVisualStyleBackColor = true;
            this.AddRecipeButton.Click += new System.EventHandler(this.AddRecipeButton_Click);
            // 
            // RecipeListView
            // 
            this.RecipeListView.CheckBoxes = true;
            this.RecipeListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.RecipeListView.Dock = System.Windows.Forms.DockStyle.Left;
            this.RecipeListView.FullRowSelect = true;
            this.RecipeListView.GridLines = true;
            this.RecipeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.RecipeListView.HideSelection = false;
            this.RecipeListView.LabelWrap = false;
            this.RecipeListView.Location = new System.Drawing.Point(3, 29);
            this.RecipeListView.Name = "RecipeListView";
            this.RecipeListView.Size = new System.Drawing.Size(196, 606);
            this.RecipeListView.SmallImageList = this.RecipeImageList;
            this.RecipeListView.TabIndex = 15;
            this.RecipeListView.UseCompatibleStateImageBehavior = false;
            this.RecipeListView.View = System.Windows.Forms.View.Details;
            this.RecipeListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.RecipeListView_ItemChecked);
            this.RecipeListView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.RecipeListView_ItemDrag);
            this.RecipeListView.SelectedIndexChanged += new System.EventHandler(this.RecipeListView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 175;
            // 
            // RecipeImageList
            // 
            this.RecipeImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.RecipeImageList.ImageSize = new System.Drawing.Size(32, 32);
            this.RecipeImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1306, 800);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "Foreman";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.MainToolbar.ResumeLayout(false);
            this.MainToolbar.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ListTabControl.ResumeLayout(false);
            this.ItemTabPage.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.RecipeTabPage.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ImageList ItemImageList;
		private System.Windows.Forms.FlowLayoutPanel MainToolbar;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox rateOptionsDropDown;
		private System.Windows.Forms.RadioButton rateButton;
		private System.Windows.Forms.RadioButton fixedAmountButton;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button AutomaticCompleteButton;
		private System.Windows.Forms.Button ClearButton;
		private System.Windows.Forms.Button ExportImageButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox SingleAssemblerPerRecipeCheckBox;
		private System.Windows.Forms.CheckBox AssemblerDisplayCheckBox;
		private System.Windows.Forms.CheckBox MinerDisplayCheckBox;
		private ProductionGraphViewer GraphViewer;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Button FactorioDirectoryButton;
		private System.Windows.Forms.Button ModDirectoryButton;
		private System.Windows.Forms.Button ReloadButton;
		private System.Windows.Forms.Button saveGraphButton;
		private System.Windows.Forms.Button loadGraphButton;
		private System.Windows.Forms.Button EnableDisableButton;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.ComboBox LanguageDropDown;
		private System.Windows.Forms.Button ArrangeNodesButton;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TabControl ListTabControl;
		private System.Windows.Forms.TabPage ItemTabPage;
		private System.Windows.Forms.ListView ItemListView;
		private System.Windows.Forms.ColumnHeader h_Name;
		private System.Windows.Forms.TabPage RecipeTabPage;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.TextBox ItemFilterTextBox;
		private System.Windows.Forms.Button AddItemButton;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.TextBox RecipeFilterTextBox;
		private System.Windows.Forms.Button AddRecipeButton;
		private System.Windows.Forms.ListView RecipeListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ImageList RecipeImageList;
        private System.Windows.Forms.CheckBox MinersUseModulesCheckBox;
    }
}

