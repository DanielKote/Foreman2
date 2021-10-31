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
            this.MainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.MainToolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.ButtonsAFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SaveGraphButton = new System.Windows.Forms.Button();
            this.LoadGraphButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.MainHelpButton = new System.Windows.Forms.Button();
            this.ButtonsBFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.EnableDisableButton = new System.Windows.Forms.Button();
            this.ExportImageButton = new System.Windows.Forms.Button();
            this.AddItemButton = new System.Windows.Forms.Button();
            this.AddRecipeButton = new System.Windows.Forms.Button();
            this.ProductionGroupBox = new System.Windows.Forms.GroupBox();
            this.PauseUpdatesCheckbox = new System.Windows.Forms.CheckBox();
            this.RateOptionsDropDown = new System.Windows.Forms.ComboBox();
            this.RateButton = new System.Windows.Forms.RadioButton();
            this.FixedAmountButton = new System.Windows.Forms.RadioButton();
            this.GridLinesGroupBox = new System.Windows.Forms.GroupBox();
            this.AlignSelectionButton = new System.Windows.Forms.Button();
            this.GridlinesCheckbox = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.MajorGridlinesDropDown = new System.Windows.Forms.ComboBox();
            this.MinorGridlinesDropDown = new System.Windows.Forms.ComboBox();
            this.AssemblersGroupBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ModuleDropDown = new System.Windows.Forms.ComboBox();
            this.AssemblerDisplayCheckBox = new System.Windows.Forms.CheckBox();
            this.MinerDisplayCheckBox = new System.Windows.Forms.CheckBox();
            this.GraphViewer = new Foreman.ProductionGraphViewer();
            this.DynamicLWCheckBox = new System.Windows.Forms.CheckBox();
            this.MainLayoutPanel.SuspendLayout();
            this.MainToolbar.SuspendLayout();
            this.ButtonsAFlowLayoutPanel.SuspendLayout();
            this.ButtonsBFlowLayoutPanel.SuspendLayout();
            this.ProductionGroupBox.SuspendLayout();
            this.GridLinesGroupBox.SuspendLayout();
            this.AssemblersGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainLayoutPanel
            // 
            this.MainLayoutPanel.ColumnCount = 1;
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainLayoutPanel.Controls.Add(this.MainToolbar, 0, 0);
            this.MainLayoutPanel.Controls.Add(this.GraphViewer, 0, 1);
            this.MainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.MainLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainLayoutPanel.Name = "MainLayoutPanel";
            this.MainLayoutPanel.RowCount = 2;
            this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayoutPanel.Size = new System.Drawing.Size(1182, 753);
            this.MainLayoutPanel.TabIndex = 1;
            // 
            // MainToolbar
            // 
            this.MainToolbar.AutoSize = true;
            this.MainToolbar.Controls.Add(this.ButtonsAFlowLayoutPanel);
            this.MainToolbar.Controls.Add(this.ButtonsBFlowLayoutPanel);
            this.MainToolbar.Controls.Add(this.ProductionGroupBox);
            this.MainToolbar.Controls.Add(this.GridLinesGroupBox);
            this.MainToolbar.Controls.Add(this.AssemblersGroupBox);
            this.MainToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainToolbar.Location = new System.Drawing.Point(0, 0);
            this.MainToolbar.Margin = new System.Windows.Forms.Padding(0);
            this.MainToolbar.Name = "MainToolbar";
            this.MainToolbar.Size = new System.Drawing.Size(1182, 136);
            this.MainToolbar.TabIndex = 2;
            // 
            // ButtonsAFlowLayoutPanel
            // 
            this.ButtonsAFlowLayoutPanel.AutoSize = true;
            this.ButtonsAFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonsAFlowLayoutPanel.Controls.Add(this.SaveGraphButton);
            this.ButtonsAFlowLayoutPanel.Controls.Add(this.LoadGraphButton);
            this.ButtonsAFlowLayoutPanel.Controls.Add(this.ClearButton);
            this.ButtonsAFlowLayoutPanel.Controls.Add(this.MainHelpButton);
            this.ButtonsAFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ButtonsAFlowLayoutPanel.Location = new System.Drawing.Point(4, 4);
            this.ButtonsAFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonsAFlowLayoutPanel.Name = "ButtonsAFlowLayoutPanel";
            this.ButtonsAFlowLayoutPanel.Size = new System.Drawing.Size(104, 128);
            this.ButtonsAFlowLayoutPanel.TabIndex = 14;
            // 
            // saveGraphButton
            // 
            this.SaveGraphButton.Location = new System.Drawing.Point(2, 2);
            this.SaveGraphButton.Margin = new System.Windows.Forms.Padding(2);
            this.SaveGraphButton.Name = "saveGraphButton";
            this.SaveGraphButton.Size = new System.Drawing.Size(100, 28);
            this.SaveGraphButton.TabIndex = 9;
            this.SaveGraphButton.Text = "Save";
            this.SaveGraphButton.UseVisualStyleBackColor = true;
            this.SaveGraphButton.Click += new System.EventHandler(this.SaveGraphButton_Click);
            // 
            // loadGraphButton
            // 
            this.LoadGraphButton.Location = new System.Drawing.Point(2, 34);
            this.LoadGraphButton.Margin = new System.Windows.Forms.Padding(2);
            this.LoadGraphButton.Name = "loadGraphButton";
            this.LoadGraphButton.Size = new System.Drawing.Size(100, 28);
            this.LoadGraphButton.TabIndex = 10;
            this.LoadGraphButton.Text = "Load";
            this.LoadGraphButton.UseVisualStyleBackColor = true;
            this.LoadGraphButton.Click += new System.EventHandler(this.LoadGraphButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(2, 66);
            this.ClearButton.Margin = new System.Windows.Forms.Padding(2);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(100, 28);
            this.ClearButton.TabIndex = 6;
            this.ClearButton.Text = "Clear Chart";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // MainHelpButton
            // 
            this.MainHelpButton.Location = new System.Drawing.Point(2, 98);
            this.MainHelpButton.Margin = new System.Windows.Forms.Padding(2);
            this.MainHelpButton.Name = "MainHelpButton";
            this.MainHelpButton.Size = new System.Drawing.Size(100, 28);
            this.MainHelpButton.TabIndex = 9;
            this.MainHelpButton.Text = "Help";
            this.MainHelpButton.UseVisualStyleBackColor = true;
            this.MainHelpButton.Click += new System.EventHandler(this.MainHelpButton_Click);
            // 
            // ButtonsBFlowLayoutPanel
            // 
            this.ButtonsBFlowLayoutPanel.AutoSize = true;
            this.ButtonsBFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonsBFlowLayoutPanel.Controls.Add(this.EnableDisableButton);
            this.ButtonsBFlowLayoutPanel.Controls.Add(this.ExportImageButton);
            this.ButtonsBFlowLayoutPanel.Controls.Add(this.AddItemButton);
            this.ButtonsBFlowLayoutPanel.Controls.Add(this.AddRecipeButton);
            this.ButtonsBFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ButtonsBFlowLayoutPanel.Location = new System.Drawing.Point(116, 4);
            this.ButtonsBFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonsBFlowLayoutPanel.Name = "ButtonsBFlowLayoutPanel";
            this.ButtonsBFlowLayoutPanel.Size = new System.Drawing.Size(104, 128);
            this.ButtonsBFlowLayoutPanel.TabIndex = 13;
            // 
            // EnableDisableButton
            // 
            this.EnableDisableButton.Location = new System.Drawing.Point(2, 2);
            this.EnableDisableButton.Margin = new System.Windows.Forms.Padding(2);
            this.EnableDisableButton.Name = "EnableDisableButton";
            this.EnableDisableButton.Size = new System.Drawing.Size(100, 28);
            this.EnableDisableButton.TabIndex = 7;
            this.EnableDisableButton.Text = "Settings";
            this.EnableDisableButton.UseVisualStyleBackColor = true;
            this.EnableDisableButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // ExportImageButton
            // 
            this.ExportImageButton.Location = new System.Drawing.Point(2, 34);
            this.ExportImageButton.Margin = new System.Windows.Forms.Padding(2);
            this.ExportImageButton.Name = "ExportImageButton";
            this.ExportImageButton.Size = new System.Drawing.Size(100, 28);
            this.ExportImageButton.TabIndex = 8;
            this.ExportImageButton.Text = "Export Image";
            this.ExportImageButton.UseVisualStyleBackColor = true;
            this.ExportImageButton.Click += new System.EventHandler(this.ExportImageButton_Click);
            // 
            // AddItemButton
            // 
            this.AddItemButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddItemButton.Location = new System.Drawing.Point(2, 66);
            this.AddItemButton.Margin = new System.Windows.Forms.Padding(2);
            this.AddItemButton.Name = "AddItemButton";
            this.AddItemButton.Size = new System.Drawing.Size(100, 28);
            this.AddItemButton.TabIndex = 11;
            this.AddItemButton.Text = "Add Item";
            this.AddItemButton.UseVisualStyleBackColor = true;
            this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
            // 
            // AddRecipeButton
            // 
            this.AddRecipeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddRecipeButton.Location = new System.Drawing.Point(2, 98);
            this.AddRecipeButton.Margin = new System.Windows.Forms.Padding(2);
            this.AddRecipeButton.Name = "AddRecipeButton";
            this.AddRecipeButton.Size = new System.Drawing.Size(100, 28);
            this.AddRecipeButton.TabIndex = 10;
            this.AddRecipeButton.Text = "Add Recipe";
            this.AddRecipeButton.UseVisualStyleBackColor = true;
            this.AddRecipeButton.Click += new System.EventHandler(this.AddRecipeButton_Click);
            // 
            // ProductionGroupBox
            // 
            this.ProductionGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ProductionGroupBox.Controls.Add(this.DynamicLWCheckBox);
            this.ProductionGroupBox.Controls.Add(this.PauseUpdatesCheckbox);
            this.ProductionGroupBox.Controls.Add(this.RateOptionsDropDown);
            this.ProductionGroupBox.Controls.Add(this.RateButton);
            this.ProductionGroupBox.Controls.Add(this.FixedAmountButton);
            this.ProductionGroupBox.Location = new System.Drawing.Point(228, 4);
            this.ProductionGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.ProductionGroupBox.Name = "ProductionGroupBox";
            this.ProductionGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.ProductionGroupBox.Size = new System.Drawing.Size(210, 126);
            this.ProductionGroupBox.TabIndex = 4;
            this.ProductionGroupBox.TabStop = false;
            this.ProductionGroupBox.Text = "Production properties:";
            // 
            // PauseUpdatesCheckbox
            // 
            this.PauseUpdatesCheckbox.AutoSize = true;
            this.PauseUpdatesCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PauseUpdatesCheckbox.Location = new System.Drawing.Point(12, 101);
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
            this.RateOptionsDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RateOptionsDropDown.Enabled = false;
            this.RateOptionsDropDown.FormattingEnabled = true;
            this.RateOptionsDropDown.Items.AddRange(new object[] {
            "/sec",
            "/min"});
            this.RateOptionsDropDown.Location = new System.Drawing.Point(148, 46);
            this.RateOptionsDropDown.Margin = new System.Windows.Forms.Padding(4);
            this.RateOptionsDropDown.Name = "rateOptionsDropDown";
            this.RateOptionsDropDown.Size = new System.Drawing.Size(53, 24);
            this.RateOptionsDropDown.TabIndex = 2;
            this.RateOptionsDropDown.SelectedIndexChanged += new System.EventHandler(this.RateOptionsDropDown_SelectedIndexChanged);
            // 
            // rateButton
            // 
            this.RateButton.AutoSize = true;
            this.RateButton.Location = new System.Drawing.Point(12, 47);
            this.RateButton.Margin = new System.Windows.Forms.Padding(4);
            this.RateButton.Name = "rateButton";
            this.RateButton.Size = new System.Drawing.Size(139, 21);
            this.RateButton.TabIndex = 1;
            this.RateButton.Text = "Calculate as rate:";
            this.RateButton.UseVisualStyleBackColor = true;
            this.RateButton.CheckedChanged += new System.EventHandler(this.RateButton_CheckedChanged);
            // 
            // fixedAmountButton
            // 
            this.FixedAmountButton.AutoSize = true;
            this.FixedAmountButton.Checked = true;
            this.FixedAmountButton.Location = new System.Drawing.Point(12, 22);
            this.FixedAmountButton.Margin = new System.Windows.Forms.Padding(4);
            this.FixedAmountButton.Name = "fixedAmountButton";
            this.FixedAmountButton.Size = new System.Drawing.Size(190, 21);
            this.FixedAmountButton.TabIndex = 0;
            this.FixedAmountButton.TabStop = true;
            this.FixedAmountButton.Text = "Calculate as fixed amount";
            this.FixedAmountButton.UseVisualStyleBackColor = true;
            this.FixedAmountButton.CheckedChanged += new System.EventHandler(this.FixedAmountButton_CheckedChanged);
            // 
            // GridLinesGroupBox
            // 
            this.GridLinesGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GridLinesGroupBox.Controls.Add(this.AlignSelectionButton);
            this.GridLinesGroupBox.Controls.Add(this.GridlinesCheckbox);
            this.GridLinesGroupBox.Controls.Add(this.label3);
            this.GridLinesGroupBox.Controls.Add(this.label2);
            this.GridLinesGroupBox.Controls.Add(this.MajorGridlinesDropDown);
            this.GridLinesGroupBox.Controls.Add(this.MinorGridlinesDropDown);
            this.GridLinesGroupBox.Location = new System.Drawing.Point(446, 4);
            this.GridLinesGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.GridLinesGroupBox.Name = "GridLinesGroupBox";
            this.GridLinesGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.GridLinesGroupBox.Size = new System.Drawing.Size(251, 126);
            this.GridLinesGroupBox.TabIndex = 17;
            this.GridLinesGroupBox.TabStop = false;
            this.GridLinesGroupBox.Text = "Gridlines (2n scaling)";
            // 
            // AlignSelectionButton
            // 
            this.AlignSelectionButton.Location = new System.Drawing.Point(136, 94);
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
            this.GridlinesCheckbox.Location = new System.Drawing.Point(8, 101);
            this.GridlinesCheckbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.GridlinesCheckbox.Name = "GridlinesCheckbox";
            this.GridlinesCheckbox.Size = new System.Drawing.Size(124, 21);
            this.GridlinesCheckbox.TabIndex = 3;
            this.GridlinesCheckbox.Text = "Show Gridlines";
            this.GridlinesCheckbox.UseVisualStyleBackColor = true;
            this.GridlinesCheckbox.CheckedChanged += new System.EventHandler(this.GridlinesCheckbox_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(110, 56);
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
            this.MajorGridlinesDropDown.Location = new System.Drawing.Point(8, 53);
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
            // AssemblersGroupBox
            // 
            this.AssemblersGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AssemblersGroupBox.Controls.Add(this.label1);
            this.AssemblersGroupBox.Controls.Add(this.ModuleDropDown);
            this.AssemblersGroupBox.Controls.Add(this.AssemblerDisplayCheckBox);
            this.AssemblersGroupBox.Controls.Add(this.MinerDisplayCheckBox);
            this.AssemblersGroupBox.Location = new System.Drawing.Point(705, 4);
            this.AssemblersGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.AssemblersGroupBox.Name = "AssemblersGroupBox";
            this.AssemblersGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.AssemblersGroupBox.Size = new System.Drawing.Size(225, 126);
            this.AssemblersGroupBox.TabIndex = 7;
            this.AssemblersGroupBox.TabStop = false;
            this.AssemblersGroupBox.Text = "Assemblers";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(108, 100);
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
            "Speed",
            "Productivity"});
            this.ModuleDropDown.Location = new System.Drawing.Point(7, 97);
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
            // GraphViewer
            // 
            this.GraphViewer.AllowDrop = true;
            this.GraphViewer.BackColor = System.Drawing.Color.White;
            this.GraphViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GraphViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GraphViewer.Location = new System.Drawing.Point(4, 136);
            this.GraphViewer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 4);
            this.GraphViewer.MouseDownElement = null;
            this.GraphViewer.Name = "GraphViewer";
            this.GraphViewer.Size = new System.Drawing.Size(1174, 613);
            this.GraphViewer.TabIndex = 12;
            this.GraphViewer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GraphViewer_KeyDown);
            // 
            // DynamicLWCheckBox
            // 
            this.DynamicLWCheckBox.AutoSize = true;
            this.DynamicLWCheckBox.Location = new System.Drawing.Point(12, 79);
            this.DynamicLWCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.DynamicLWCheckBox.Name = "DynamicLWCheckBox";
            this.DynamicLWCheckBox.Size = new System.Drawing.Size(146, 21);
            this.DynamicLWCheckBox.TabIndex = 4;
            this.DynamicLWCheckBox.Text = "Dynamic link-width";
            this.DynamicLWCheckBox.UseVisualStyleBackColor = true;
            this.DynamicLWCheckBox.CheckedChanged += new System.EventHandler(this.DynamicLWCheckBox_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1182, 753);
            this.Controls.Add(this.MainLayoutPanel);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1000, 800);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Foreman";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MainLayoutPanel.ResumeLayout(false);
            this.MainLayoutPanel.PerformLayout();
            this.MainToolbar.ResumeLayout(false);
            this.MainToolbar.PerformLayout();
            this.ButtonsAFlowLayoutPanel.ResumeLayout(false);
            this.ButtonsBFlowLayoutPanel.ResumeLayout(false);
            this.ProductionGroupBox.ResumeLayout(false);
            this.ProductionGroupBox.PerformLayout();
            this.GridLinesGroupBox.ResumeLayout(false);
            this.GridLinesGroupBox.PerformLayout();
            this.AssemblersGroupBox.ResumeLayout(false);
            this.AssemblersGroupBox.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion
        private System.Windows.Forms.TableLayoutPanel MainLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel MainToolbar;
        private System.Windows.Forms.FlowLayoutPanel ButtonsAFlowLayoutPanel;
        private System.Windows.Forms.Button SaveGraphButton;
        private System.Windows.Forms.Button LoadGraphButton;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.FlowLayoutPanel ButtonsBFlowLayoutPanel;
        private System.Windows.Forms.Button EnableDisableButton;
        private System.Windows.Forms.Button ExportImageButton;
        private System.Windows.Forms.Button MainHelpButton;
        private System.Windows.Forms.GroupBox ProductionGroupBox;
        private System.Windows.Forms.CheckBox PauseUpdatesCheckbox;
        private System.Windows.Forms.ComboBox RateOptionsDropDown;
        private System.Windows.Forms.RadioButton RateButton;
        private System.Windows.Forms.RadioButton FixedAmountButton;
        private System.Windows.Forms.GroupBox GridLinesGroupBox;
        private System.Windows.Forms.Button AlignSelectionButton;
        private System.Windows.Forms.CheckBox GridlinesCheckbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox MajorGridlinesDropDown;
        private System.Windows.Forms.ComboBox MinorGridlinesDropDown;
        private System.Windows.Forms.GroupBox AssemblersGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ModuleDropDown;
        private System.Windows.Forms.CheckBox AssemblerDisplayCheckBox;
        private System.Windows.Forms.CheckBox MinerDisplayCheckBox;
        private ProductionGraphViewer GraphViewer;
        private System.Windows.Forms.Button AddItemButton;
        private System.Windows.Forms.Button AddRecipeButton;
        private System.Windows.Forms.CheckBox DynamicLWCheckBox;
    }
}

