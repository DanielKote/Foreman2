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
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.saveGraphButton = new System.Windows.Forms.Button();
            this.loadGraphButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.EnableDisableButton = new System.Windows.Forms.Button();
            this.ExportImageButton = new System.Windows.Forms.Button();
            this.HelpButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.PauseUpdatesCheckbox = new System.Windows.Forms.CheckBox();
            this.rateOptionsDropDown = new System.Windows.Forms.ComboBox();
            this.rateButton = new System.Windows.Forms.RadioButton();
            this.fixedAmountButton = new System.Windows.Forms.RadioButton();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.AlignSelectionButton = new System.Windows.Forms.Button();
            this.GridlinesCheckbox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.MajorGridlinesDropDown = new System.Windows.Forms.ComboBox();
            this.MinorGridlinesDropDown = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ModuleDropDown = new System.Windows.Forms.ComboBox();
            this.AssemblerDisplayCheckBox = new System.Windows.Forms.CheckBox();
            this.MinerDisplayCheckBox = new System.Windows.Forms.CheckBox();
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
            this.GraphViewer = new Foreman.ProductionGraphViewer();
            this.tableLayoutPanel1.SuspendLayout();
            this.MainToolbar.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
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
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 280F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.MainToolbar, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.GraphViewer, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.ListTabControl, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1182, 753);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // MainToolbar
            // 
            this.MainToolbar.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.MainToolbar, 2);
            this.MainToolbar.Controls.Add(this.flowLayoutPanel3);
            this.MainToolbar.Controls.Add(this.flowLayoutPanel2);
            this.MainToolbar.Controls.Add(this.groupBox1);
            this.MainToolbar.Controls.Add(this.groupBox5);
            this.MainToolbar.Controls.Add(this.groupBox2);
            this.MainToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainToolbar.Location = new System.Drawing.Point(4, 4);
            this.MainToolbar.Margin = new System.Windows.Forms.Padding(4);
            this.MainToolbar.Name = "MainToolbar";
            this.MainToolbar.Size = new System.Drawing.Size(1174, 128);
            this.MainToolbar.TabIndex = 2;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel3.Controls.Add(this.saveGraphButton);
            this.flowLayoutPanel3.Controls.Add(this.loadGraphButton);
            this.flowLayoutPanel3.Controls.Add(this.ClearButton);
            this.flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(4, 4);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(108, 120);
            this.flowLayoutPanel3.TabIndex = 14;
            // 
            // saveGraphButton
            // 
            this.saveGraphButton.Location = new System.Drawing.Point(4, 4);
            this.saveGraphButton.Margin = new System.Windows.Forms.Padding(4);
            this.saveGraphButton.Name = "saveGraphButton";
            this.saveGraphButton.Size = new System.Drawing.Size(100, 32);
            this.saveGraphButton.TabIndex = 9;
            this.saveGraphButton.Text = "Save";
            this.saveGraphButton.UseVisualStyleBackColor = true;
            this.saveGraphButton.Click += new System.EventHandler(this.saveGraphButton_Click);
            // 
            // loadGraphButton
            // 
            this.loadGraphButton.Location = new System.Drawing.Point(4, 44);
            this.loadGraphButton.Margin = new System.Windows.Forms.Padding(4);
            this.loadGraphButton.Name = "loadGraphButton";
            this.loadGraphButton.Size = new System.Drawing.Size(100, 32);
            this.loadGraphButton.TabIndex = 10;
            this.loadGraphButton.Text = "Load";
            this.loadGraphButton.UseVisualStyleBackColor = true;
            this.loadGraphButton.Click += new System.EventHandler(this.loadGraphButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(4, 84);
            this.ClearButton.Margin = new System.Windows.Forms.Padding(4);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(100, 32);
            this.ClearButton.TabIndex = 6;
            this.ClearButton.Text = "Clear Chart";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.EnableDisableButton);
            this.flowLayoutPanel2.Controls.Add(this.ExportImageButton);
            this.flowLayoutPanel2.Controls.Add(this.HelpButton);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(120, 4);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(108, 120);
            this.flowLayoutPanel2.TabIndex = 13;
            // 
            // EnableDisableButton
            // 
            this.EnableDisableButton.Location = new System.Drawing.Point(4, 4);
            this.EnableDisableButton.Margin = new System.Windows.Forms.Padding(4);
            this.EnableDisableButton.Name = "EnableDisableButton";
            this.EnableDisableButton.Size = new System.Drawing.Size(100, 32);
            this.EnableDisableButton.TabIndex = 7;
            this.EnableDisableButton.Text = "Settings";
            this.EnableDisableButton.UseVisualStyleBackColor = true;
            this.EnableDisableButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // ExportImageButton
            // 
            this.ExportImageButton.Location = new System.Drawing.Point(4, 44);
            this.ExportImageButton.Margin = new System.Windows.Forms.Padding(4);
            this.ExportImageButton.Name = "ExportImageButton";
            this.ExportImageButton.Size = new System.Drawing.Size(100, 32);
            this.ExportImageButton.TabIndex = 8;
            this.ExportImageButton.Text = "Export Image";
            this.ExportImageButton.UseVisualStyleBackColor = true;
            this.ExportImageButton.Click += new System.EventHandler(this.ExportImageButton_Click);
            // 
            // HelpButton
            // 
            this.HelpButton.Location = new System.Drawing.Point(4, 84);
            this.HelpButton.Margin = new System.Windows.Forms.Padding(4);
            this.HelpButton.Name = "HelpButton";
            this.HelpButton.Size = new System.Drawing.Size(100, 32);
            this.HelpButton.TabIndex = 9;
            this.HelpButton.Text = "Help";
            this.HelpButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.PauseUpdatesCheckbox);
            this.groupBox1.Controls.Add(this.rateOptionsDropDown);
            this.groupBox1.Controls.Add(this.rateButton);
            this.groupBox1.Controls.Add(this.fixedAmountButton);
            this.groupBox1.Location = new System.Drawing.Point(236, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(210, 117);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Production properties:";
            // 
            // PauseUpdatesCheckbox
            // 
            this.PauseUpdatesCheckbox.AutoSize = true;
            this.PauseUpdatesCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PauseUpdatesCheckbox.Location = new System.Drawing.Point(12, 77);
            this.PauseUpdatesCheckbox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.PauseUpdatesCheckbox.Name = "PauseUpdatesCheckbox";
            this.PauseUpdatesCheckbox.Size = new System.Drawing.Size(188, 21);
            this.PauseUpdatesCheckbox.TabIndex = 3;
            this.PauseUpdatesCheckbox.Text = "Pause all calculations";
            this.PauseUpdatesCheckbox.UseVisualStyleBackColor = true;
            this.PauseUpdatesCheckbox.CheckedChanged += new System.EventHandler(this.PauseUpdatesCheckbox_CheckedChanged);
            // 
            // rateOptionsDropDown
            // 
            this.rateOptionsDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.rateOptionsDropDown.Enabled = false;
            this.rateOptionsDropDown.FormattingEnabled = true;
            this.rateOptionsDropDown.Items.AddRange(new object[] {
            "/sec",
            "/min"});
            this.rateOptionsDropDown.Location = new System.Drawing.Point(149, 47);
            this.rateOptionsDropDown.Margin = new System.Windows.Forms.Padding(4);
            this.rateOptionsDropDown.Name = "rateOptionsDropDown";
            this.rateOptionsDropDown.Size = new System.Drawing.Size(53, 24);
            this.rateOptionsDropDown.TabIndex = 2;
            this.rateOptionsDropDown.SelectedIndexChanged += new System.EventHandler(this.rateOptionsDropDown_SelectedIndexChanged);
            // 
            // rateButton
            // 
            this.rateButton.AutoSize = true;
            this.rateButton.Location = new System.Drawing.Point(12, 47);
            this.rateButton.Margin = new System.Windows.Forms.Padding(4);
            this.rateButton.Name = "rateButton";
            this.rateButton.Size = new System.Drawing.Size(139, 21);
            this.rateButton.TabIndex = 1;
            this.rateButton.Text = "Calculate as rate:";
            this.rateButton.UseVisualStyleBackColor = true;
            this.rateButton.CheckedChanged += new System.EventHandler(this.rateButton_CheckedChanged);
            // 
            // fixedAmountButton
            // 
            this.fixedAmountButton.AutoSize = true;
            this.fixedAmountButton.Checked = true;
            this.fixedAmountButton.Location = new System.Drawing.Point(12, 22);
            this.fixedAmountButton.Margin = new System.Windows.Forms.Padding(4);
            this.fixedAmountButton.Name = "fixedAmountButton";
            this.fixedAmountButton.Size = new System.Drawing.Size(190, 21);
            this.fixedAmountButton.TabIndex = 0;
            this.fixedAmountButton.TabStop = true;
            this.fixedAmountButton.Text = "Calculate as fixed amount";
            this.fixedAmountButton.UseVisualStyleBackColor = true;
            this.fixedAmountButton.CheckedChanged += new System.EventHandler(this.fixedAmountButton_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.AutoSize = true;
            this.groupBox5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox5.Controls.Add(this.AlignSelectionButton);
            this.groupBox5.Controls.Add(this.GridlinesCheckbox);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.MajorGridlinesDropDown);
            this.groupBox5.Controls.Add(this.MinorGridlinesDropDown);
            this.groupBox5.Location = new System.Drawing.Point(454, 4);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.groupBox5.Size = new System.Drawing.Size(251, 117);
            this.groupBox5.TabIndex = 17;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Gridlines (2n scaling)";
            // 
            // AlignSelectionButton
            // 
            this.AlignSelectionButton.Location = new System.Drawing.Point(136, 74);
            this.AlignSelectionButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.AlignSelectionButton.Name = "AlignSelectionButton";
            this.AlignSelectionButton.Size = new System.Drawing.Size(108, 28);
            this.AlignSelectionButton.TabIndex = 6;
            this.AlignSelectionButton.Text = "Align Selected";
            this.AlignSelectionButton.UseVisualStyleBackColor = true;
            this.AlignSelectionButton.Click += new System.EventHandler(this.AlignSelectionButton_Click);
            // 
            // GridlinesCheckbox
            // 
            this.GridlinesCheckbox.AutoSize = true;
            this.GridlinesCheckbox.Location = new System.Drawing.Point(8, 79);
            this.GridlinesCheckbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.GridlinesCheckbox.Name = "GridlinesCheckbox";
            this.GridlinesCheckbox.Size = new System.Drawing.Size(124, 21);
            this.GridlinesCheckbox.TabIndex = 3;
            this.GridlinesCheckbox.Text = "Show Gridlines";
            this.GridlinesCheckbox.UseVisualStyleBackColor = true;
            this.GridlinesCheckbox.CheckedChanged += new System.EventHandler(this.gridlinesCheckbox_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(110, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Major Gridlines";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(110, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Minor Gridlines";
            // 
            // MajorGridlinesDropDown
            // 
            this.MajorGridlinesDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MajorGridlinesDropDown.FormattingEnabled = true;
            this.MajorGridlinesDropDown.Items.AddRange(new object[] {
            "None",
            "1",
            "2",
            "4",
            "8",
            "16",
            "32",
            "64",
            "128",
            "256",
            "512",
            "1024"});
            this.MajorGridlinesDropDown.Location = new System.Drawing.Point(8, 50);
            this.MajorGridlinesDropDown.Name = "MajorGridlinesDropDown";
            this.MajorGridlinesDropDown.Size = new System.Drawing.Size(96, 24);
            this.MajorGridlinesDropDown.TabIndex = 4;
            this.MajorGridlinesDropDown.SelectedIndexChanged += new System.EventHandler(this.MajorGridlinesDropDown_SelectedIndexChanged);
            // 
            // MinorGridlinesDropDown
            // 
            this.MinorGridlinesDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MinorGridlinesDropDown.FormattingEnabled = true;
            this.MinorGridlinesDropDown.Items.AddRange(new object[] {
            "none",
            "1",
            "2",
            "4",
            "8",
            "16",
            "32",
            "64",
            "128",
            "256",
            "512",
            "1024"});
            this.MinorGridlinesDropDown.Location = new System.Drawing.Point(8, 22);
            this.MinorGridlinesDropDown.Name = "MinorGridlinesDropDown";
            this.MinorGridlinesDropDown.Size = new System.Drawing.Size(96, 24);
            this.MinorGridlinesDropDown.TabIndex = 3;
            this.MinorGridlinesDropDown.SelectedIndexChanged += new System.EventHandler(this.MinorGridlinesDropDown_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.ModuleDropDown);
            this.groupBox2.Controls.Add(this.AssemblerDisplayCheckBox);
            this.groupBox2.Controls.Add(this.MinerDisplayCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(713, 4);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.groupBox2.Size = new System.Drawing.Size(225, 117);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Assemblers";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(108, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Default modules";
            // 
            // ModuleDropDown
            // 
            this.ModuleDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModuleDropDown.FormattingEnabled = true;
            this.ModuleDropDown.Items.AddRange(new object[] {
            "None",
            "Efficiency",
            "Productivity"});
            this.ModuleDropDown.Location = new System.Drawing.Point(9, 75);
            this.ModuleDropDown.Name = "ModuleDropDown";
            this.ModuleDropDown.Size = new System.Drawing.Size(96, 24);
            this.ModuleDropDown.TabIndex = 1;
            this.ModuleDropDown.SelectedIndexChanged += new System.EventHandler(this.ModuleDropDown_SelectedIndexChanged);
            // 
            // AssemblerDisplayCheckBox
            // 
            this.AssemblerDisplayCheckBox.AutoSize = true;
            this.AssemblerDisplayCheckBox.Location = new System.Drawing.Point(9, 23);
            this.AssemblerDisplayCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.AssemblerDisplayCheckBox.Name = "AssemblerDisplayCheckBox";
            this.AssemblerDisplayCheckBox.Size = new System.Drawing.Size(153, 21);
            this.AssemblerDisplayCheckBox.TabIndex = 0;
            this.AssemblerDisplayCheckBox.Text = "Display Assemblers";
            this.AssemblerDisplayCheckBox.UseVisualStyleBackColor = true;
            this.AssemblerDisplayCheckBox.CheckedChanged += new System.EventHandler(this.AssemblerDisplayCheckBox_CheckedChanged);
            // 
            // MinerDisplayCheckBox
            // 
            this.MinerDisplayCheckBox.AutoSize = true;
            this.MinerDisplayCheckBox.Location = new System.Drawing.Point(9, 50);
            this.MinerDisplayCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.MinerDisplayCheckBox.Name = "MinerDisplayCheckBox";
            this.MinerDisplayCheckBox.Size = new System.Drawing.Size(194, 21);
            this.MinerDisplayCheckBox.TabIndex = 0;
            this.MinerDisplayCheckBox.Text = "Display Miners/Pumpjacks";
            this.MinerDisplayCheckBox.UseVisualStyleBackColor = true;
            this.MinerDisplayCheckBox.CheckedChanged += new System.EventHandler(this.MinerDisplayCheckBox_CheckedChanged);
            // 
            // ListTabControl
            // 
            this.ListTabControl.Controls.Add(this.ItemTabPage);
            this.ListTabControl.Controls.Add(this.RecipeTabPage);
            this.ListTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListTabControl.Location = new System.Drawing.Point(0, 136);
            this.ListTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.ListTabControl.Name = "ListTabControl";
            this.ListTabControl.Padding = new System.Drawing.Point(0, 0);
            this.ListTabControl.SelectedIndex = 0;
            this.ListTabControl.Size = new System.Drawing.Size(280, 617);
            this.ListTabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.ListTabControl.TabIndex = 17;
            // 
            // ItemTabPage
            // 
            this.ItemTabPage.Controls.Add(this.tableLayoutPanel2);
            this.ItemTabPage.Location = new System.Drawing.Point(4, 25);
            this.ItemTabPage.Margin = new System.Windows.Forms.Padding(0);
            this.ItemTabPage.Name = "ItemTabPage";
            this.ItemTabPage.Size = new System.Drawing.Size(272, 588);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(272, 588);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // ItemFilterTextBox
            // 
            this.ItemFilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemFilterTextBox.Location = new System.Drawing.Point(4, 4);
            this.ItemFilterTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.ItemFilterTextBox.Name = "ItemFilterTextBox";
            this.ItemFilterTextBox.Size = new System.Drawing.Size(264, 22);
            this.ItemFilterTextBox.TabIndex = 19;
            this.ItemFilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            this.ItemFilterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FilterTextBox_KeyDown);
            // 
            // AddItemButton
            // 
            this.AddItemButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddItemButton.Location = new System.Drawing.Point(4, 553);
            this.AddItemButton.Margin = new System.Windows.Forms.Padding(4);
            this.AddItemButton.Name = "AddItemButton";
            this.AddItemButton.Size = new System.Drawing.Size(264, 31);
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
            this.ItemListView.Location = new System.Drawing.Point(4, 34);
            this.ItemListView.Margin = new System.Windows.Forms.Padding(4);
            this.ItemListView.Name = "ItemListView";
            this.ItemListView.Size = new System.Drawing.Size(264, 511);
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
            this.RecipeTabPage.Location = new System.Drawing.Point(4, 25);
            this.RecipeTabPage.Margin = new System.Windows.Forms.Padding(4);
            this.RecipeTabPage.Name = "RecipeTabPage";
            this.RecipeTabPage.Size = new System.Drawing.Size(272, 588);
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
            this.tableLayoutPanel3.Size = new System.Drawing.Size(272, 588);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // RecipeFilterTextBox
            // 
            this.RecipeFilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecipeFilterTextBox.Location = new System.Drawing.Point(4, 4);
            this.RecipeFilterTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.RecipeFilterTextBox.Name = "RecipeFilterTextBox";
            this.RecipeFilterTextBox.Size = new System.Drawing.Size(264, 22);
            this.RecipeFilterTextBox.TabIndex = 19;
            this.RecipeFilterTextBox.TextChanged += new System.EventHandler(this.RecipeFilterTextBox_TextChanged);
            this.RecipeFilterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RecipeFilterTextBox_KeyDown);
            // 
            // AddRecipeButton
            // 
            this.AddRecipeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddRecipeButton.Location = new System.Drawing.Point(4, 553);
            this.AddRecipeButton.Margin = new System.Windows.Forms.Padding(4);
            this.AddRecipeButton.Name = "AddRecipeButton";
            this.AddRecipeButton.Size = new System.Drawing.Size(264, 31);
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
            this.RecipeListView.Location = new System.Drawing.Point(4, 34);
            this.RecipeListView.Margin = new System.Windows.Forms.Padding(4);
            this.RecipeListView.Name = "RecipeListView";
            this.RecipeListView.Size = new System.Drawing.Size(260, 511);
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
            // GraphViewer
            // 
            this.GraphViewer.AllowDrop = true;
            this.GraphViewer.BackColor = System.Drawing.Color.White;
            this.GraphViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GraphViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GraphViewer.Location = new System.Drawing.Point(285, 141);
            this.GraphViewer.Margin = new System.Windows.Forms.Padding(5);
            this.GraphViewer.MouseDownElement = null;
            this.GraphViewer.Name = "GraphViewer";
            this.GraphViewer.Size = new System.Drawing.Size(892, 607);
            this.GraphViewer.TabIndex = 12;
            this.GraphViewer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GraphViewer_KeyDown);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 753);
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Foreman";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.MainToolbar.ResumeLayout(false);
            this.MainToolbar.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
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
		private System.Windows.Forms.Button ClearButton;
		private System.Windows.Forms.Button ExportImageButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox AssemblerDisplayCheckBox;
		private System.Windows.Forms.CheckBox MinerDisplayCheckBox;
		private ProductionGraphViewer GraphViewer;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Button saveGraphButton;
		private System.Windows.Forms.Button loadGraphButton;
		private System.Windows.Forms.Button EnableDisableButton;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ModuleDropDown;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox GridlinesCheckbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox MajorGridlinesDropDown;
        private System.Windows.Forms.ComboBox MinorGridlinesDropDown;
        private System.Windows.Forms.CheckBox PauseUpdatesCheckbox;
        private System.Windows.Forms.Button AlignSelectionButton;
        private System.Windows.Forms.Button HelpButton;
    }
}

