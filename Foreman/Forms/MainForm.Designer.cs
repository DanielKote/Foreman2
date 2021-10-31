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
			this.HighLodRadioButton = new System.Windows.Forms.RadioButton();
			this.MediumLodRadioButton = new System.Windows.Forms.RadioButton();
			this.LowLodRadioButton = new System.Windows.Forms.RadioButton();
			this.label6 = new System.Windows.Forms.Label();
			this.ShowNodeRecipeCheckBox = new System.Windows.Forms.CheckBox();
			this.DynamicLWCheckBox = new System.Windows.Forms.CheckBox();
			this.PauseUpdatesCheckbox = new System.Windows.Forms.CheckBox();
			this.GridLinesGroupBox = new System.Windows.Forms.GroupBox();
			this.AlignSelectionButton = new System.Windows.Forms.Button();
			this.GridlinesCheckbox = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.MajorGridlinesDropDown = new System.Windows.Forms.ComboBox();
			this.MinorGridlinesDropDown = new System.Windows.Forms.ComboBox();
			this.DefaultsGroupBox = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.AssemblerDropDown = new System.Windows.Forms.ComboBox();
			this.RateOptionsDropDown = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ModuleDropDown = new System.Windows.Forms.ComboBox();
			this.ShowUnavailableCheckBox = new System.Windows.Forms.CheckBox();
			this.GraphViewer = new Foreman.ProductionGraphViewer();
			this.MainLayoutPanel.SuspendLayout();
			this.MainToolbar.SuspendLayout();
			this.ButtonsAFlowLayoutPanel.SuspendLayout();
			this.ButtonsBFlowLayoutPanel.SuspendLayout();
			this.ProductionGroupBox.SuspendLayout();
			this.GridLinesGroupBox.SuspendLayout();
			this.DefaultsGroupBox.SuspendLayout();
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
			this.MainLayoutPanel.Size = new System.Drawing.Size(1182, 853);
			this.MainLayoutPanel.TabIndex = 1;
			// 
			// MainToolbar
			// 
			this.MainToolbar.AutoSize = true;
			this.MainToolbar.Controls.Add(this.ButtonsAFlowLayoutPanel);
			this.MainToolbar.Controls.Add(this.ButtonsBFlowLayoutPanel);
			this.MainToolbar.Controls.Add(this.ProductionGroupBox);
			this.MainToolbar.Controls.Add(this.GridLinesGroupBox);
			this.MainToolbar.Controls.Add(this.DefaultsGroupBox);
			this.MainToolbar.Controls.Add(this.ShowUnavailableCheckBox);
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
			// SaveGraphButton
			// 
			this.SaveGraphButton.Location = new System.Drawing.Point(2, 2);
			this.SaveGraphButton.Margin = new System.Windows.Forms.Padding(2);
			this.SaveGraphButton.Name = "SaveGraphButton";
			this.SaveGraphButton.Size = new System.Drawing.Size(100, 28);
			this.SaveGraphButton.TabIndex = 9;
			this.SaveGraphButton.Text = "Save";
			this.SaveGraphButton.UseVisualStyleBackColor = true;
			this.SaveGraphButton.Click += new System.EventHandler(this.SaveGraphButton_Click);
			// 
			// LoadGraphButton
			// 
			this.LoadGraphButton.Location = new System.Drawing.Point(2, 34);
			this.LoadGraphButton.Margin = new System.Windows.Forms.Padding(2);
			this.LoadGraphButton.Name = "LoadGraphButton";
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
			this.ClearButton.Text = "New";
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
			this.ProductionGroupBox.Controls.Add(this.HighLodRadioButton);
			this.ProductionGroupBox.Controls.Add(this.MediumLodRadioButton);
			this.ProductionGroupBox.Controls.Add(this.LowLodRadioButton);
			this.ProductionGroupBox.Controls.Add(this.label6);
			this.ProductionGroupBox.Controls.Add(this.ShowNodeRecipeCheckBox);
			this.ProductionGroupBox.Controls.Add(this.DynamicLWCheckBox);
			this.ProductionGroupBox.Controls.Add(this.PauseUpdatesCheckbox);
			this.ProductionGroupBox.Location = new System.Drawing.Point(228, 4);
			this.ProductionGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
			this.ProductionGroupBox.Name = "ProductionGroupBox";
			this.ProductionGroupBox.Padding = new System.Windows.Forms.Padding(4);
			this.ProductionGroupBox.Size = new System.Drawing.Size(210, 126);
			this.ProductionGroupBox.TabIndex = 4;
			this.ProductionGroupBox.TabStop = false;
			this.ProductionGroupBox.Text = "Graph Options:";
			// 
			// HighLodRadioButton
			// 
			this.HighLodRadioButton.AutoSize = true;
			this.HighLodRadioButton.Location = new System.Drawing.Point(150, 18);
			this.HighLodRadioButton.Name = "HighLodRadioButton";
			this.HighLodRadioButton.Size = new System.Drawing.Size(58, 21);
			this.HighLodRadioButton.TabIndex = 10;
			this.HighLodRadioButton.TabStop = true;
			this.HighLodRadioButton.Text = "High";
			this.HighLodRadioButton.UseVisualStyleBackColor = true;
			this.HighLodRadioButton.CheckedChanged += new System.EventHandler(this.LODRadioButton_CheckedChanged);
			// 
			// MediumLodRadioButton
			// 
			this.MediumLodRadioButton.AutoSize = true;
			this.MediumLodRadioButton.Location = new System.Drawing.Point(98, 18);
			this.MediumLodRadioButton.Name = "MediumLodRadioButton";
			this.MediumLodRadioButton.Size = new System.Drawing.Size(56, 21);
			this.MediumLodRadioButton.TabIndex = 9;
			this.MediumLodRadioButton.TabStop = true;
			this.MediumLodRadioButton.Text = "Med";
			this.MediumLodRadioButton.UseVisualStyleBackColor = true;
			this.MediumLodRadioButton.CheckedChanged += new System.EventHandler(this.LODRadioButton_CheckedChanged);
			// 
			// LowLodRadioButton
			// 
			this.LowLodRadioButton.AutoSize = true;
			this.LowLodRadioButton.Location = new System.Drawing.Point(47, 18);
			this.LowLodRadioButton.Name = "LowLodRadioButton";
			this.LowLodRadioButton.Size = new System.Drawing.Size(54, 21);
			this.LowLodRadioButton.TabIndex = 8;
			this.LowLodRadioButton.TabStop = true;
			this.LowLodRadioButton.Text = "Low";
			this.LowLodRadioButton.UseVisualStyleBackColor = true;
			this.LowLodRadioButton.CheckedChanged += new System.EventHandler(this.LODRadioButton_CheckedChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(7, 20);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(41, 17);
			this.label6.TabIndex = 7;
			this.label6.Text = "LOD:";
			// 
			// ShowNodeRecipeCheckBox
			// 
			this.ShowNodeRecipeCheckBox.AutoSize = true;
			this.ShowNodeRecipeCheckBox.Location = new System.Drawing.Point(8, 74);
			this.ShowNodeRecipeCheckBox.Name = "ShowNodeRecipeCheckBox";
			this.ShowNodeRecipeCheckBox.Size = new System.Drawing.Size(119, 21);
			this.ShowNodeRecipeCheckBox.TabIndex = 6;
			this.ShowNodeRecipeCheckBox.Text = "Show Recipes";
			this.ShowNodeRecipeCheckBox.UseVisualStyleBackColor = true;
			this.ShowNodeRecipeCheckBox.CheckedChanged += new System.EventHandler(this.ShowNodeRecipeCheckBox_CheckedChanged);
			// 
			// DynamicLWCheckBox
			// 
			this.DynamicLWCheckBox.AutoSize = true;
			this.DynamicLWCheckBox.Location = new System.Drawing.Point(8, 47);
			this.DynamicLWCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
			this.DynamicLWCheckBox.Name = "DynamicLWCheckBox";
			this.DynamicLWCheckBox.Size = new System.Drawing.Size(155, 21);
			this.DynamicLWCheckBox.TabIndex = 4;
			this.DynamicLWCheckBox.Text = "Dynamic Link-Width";
			this.DynamicLWCheckBox.UseVisualStyleBackColor = true;
			this.DynamicLWCheckBox.CheckedChanged += new System.EventHandler(this.DynamicLWCheckBox_CheckedChanged);
			// 
			// PauseUpdatesCheckbox
			// 
			this.PauseUpdatesCheckbox.AutoSize = true;
			this.PauseUpdatesCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PauseUpdatesCheckbox.Location = new System.Drawing.Point(8, 98);
			this.PauseUpdatesCheckbox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.PauseUpdatesCheckbox.Name = "PauseUpdatesCheckbox";
			this.PauseUpdatesCheckbox.Size = new System.Drawing.Size(188, 21);
			this.PauseUpdatesCheckbox.TabIndex = 3;
			this.PauseUpdatesCheckbox.Text = "Pause all calculations";
			this.PauseUpdatesCheckbox.UseVisualStyleBackColor = true;
			this.PauseUpdatesCheckbox.CheckedChanged += new System.EventHandler(this.PauseUpdatesCheckbox_CheckedChanged);
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
			this.AlignSelectionButton.Location = new System.Drawing.Point(135, 85);
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
			this.GridlinesCheckbox.Location = new System.Drawing.Point(8, 89);
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
			// DefaultsGroupBox
			// 
			this.DefaultsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.DefaultsGroupBox.Controls.Add(this.label5);
			this.DefaultsGroupBox.Controls.Add(this.label4);
			this.DefaultsGroupBox.Controls.Add(this.AssemblerDropDown);
			this.DefaultsGroupBox.Controls.Add(this.RateOptionsDropDown);
			this.DefaultsGroupBox.Controls.Add(this.label1);
			this.DefaultsGroupBox.Controls.Add(this.ModuleDropDown);
			this.DefaultsGroupBox.Location = new System.Drawing.Point(705, 4);
			this.DefaultsGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
			this.DefaultsGroupBox.Name = "DefaultsGroupBox";
			this.DefaultsGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
			this.DefaultsGroupBox.Size = new System.Drawing.Size(243, 126);
			this.DefaultsGroupBox.TabIndex = 7;
			this.DefaultsGroupBox.TabStop = false;
			this.DefaultsGroupBox.Text = "Defaults:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(7, 56);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(81, 17);
			this.label5.TabIndex = 4;
			this.label5.Text = "Assemblers";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(7, 25);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(79, 17);
			this.label4.TabIndex = 5;
			this.label4.Text = "Base Time:";
			// 
			// AssemblerDropDown
			// 
			this.AssemblerDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.AssemblerDropDown.FormattingEnabled = true;
			this.AssemblerDropDown.Location = new System.Drawing.Point(89, 53);
			this.AssemblerDropDown.Name = "AssemblerDropDown";
			this.AssemblerDropDown.Size = new System.Drawing.Size(147, 24);
			this.AssemblerDropDown.TabIndex = 3;
			this.AssemblerDropDown.SelectedIndexChanged += new System.EventHandler(this.AssemblerDropDown_SelectedIndexChanged);
			// 
			// RateOptionsDropDown
			// 
			this.RateOptionsDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.RateOptionsDropDown.FormattingEnabled = true;
			this.RateOptionsDropDown.Location = new System.Drawing.Point(89, 22);
			this.RateOptionsDropDown.Margin = new System.Windows.Forms.Padding(4);
			this.RateOptionsDropDown.Name = "RateOptionsDropDown";
			this.RateOptionsDropDown.Size = new System.Drawing.Size(146, 24);
			this.RateOptionsDropDown.TabIndex = 2;
			this.RateOptionsDropDown.SelectedIndexChanged += new System.EventHandler(this.RateOptionsDropDown_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 86);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 17);
			this.label1.TabIndex = 2;
			this.label1.Text = "Modules";
			// 
			// ModuleDropDown
			// 
			this.ModuleDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ModuleDropDown.FormattingEnabled = true;
			this.ModuleDropDown.Location = new System.Drawing.Point(89, 83);
			this.ModuleDropDown.Name = "ModuleDropDown";
			this.ModuleDropDown.Size = new System.Drawing.Size(147, 24);
			this.ModuleDropDown.TabIndex = 1;
			this.ModuleDropDown.SelectedIndexChanged += new System.EventHandler(this.ModuleDropDown_SelectedIndexChanged);
			// 
			// ShowUnavailableCheckBox
			// 
			this.ShowUnavailableCheckBox.AutoSize = true;
			this.ShowUnavailableCheckBox.Location = new System.Drawing.Point(956, 4);
			this.ShowUnavailableCheckBox.Margin = new System.Windows.Forms.Padding(4);
			this.ShowUnavailableCheckBox.Name = "ShowUnavailableCheckBox";
			this.ShowUnavailableCheckBox.Size = new System.Drawing.Size(178, 21);
			this.ShowUnavailableCheckBox.TabIndex = 0;
			this.ShowUnavailableCheckBox.Text = "DEV: Show Unavailable";
			this.ShowUnavailableCheckBox.UseVisualStyleBackColor = true;
			this.ShowUnavailableCheckBox.CheckedChanged += new System.EventHandler(this.DEV_ShowUnavailableCheckBox_CheckChanged);
			// 
			// GraphViewer
			// 
			this.GraphViewer.AllowDrop = true;
			this.GraphViewer.BackColor = System.Drawing.Color.White;
			this.GraphViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.GraphViewer.DCache = null;
			this.GraphViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GraphViewer.LevelOfDetail = Foreman.ProductionGraphViewer.LOD.Medium;
			this.GraphViewer.Location = new System.Drawing.Point(4, 136);
			this.GraphViewer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 4);
			this.GraphViewer.MouseDownElement = null;
			this.GraphViewer.Name = "GraphViewer";
			this.GraphViewer.RecipeTooltipEnabled = false;
			this.GraphViewer.SelectedRateUnit = Foreman.ProductionGraphViewer.RateUnit.Per1Sec;
			this.GraphViewer.Size = new System.Drawing.Size(1174, 713);
			this.GraphViewer.TabIndex = 12;
			this.GraphViewer.TooltipsEnabled = true;
			this.GraphViewer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GraphViewer_KeyDown);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1182, 853);
			this.Controls.Add(this.MainLayoutPanel);
			this.DoubleBuffered = true;
			this.KeyPreview = true;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MinimumSize = new System.Drawing.Size(1000, 900);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Foreman 2.0";
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
			this.DefaultsGroupBox.ResumeLayout(false);
			this.DefaultsGroupBox.PerformLayout();
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
        private System.Windows.Forms.GroupBox GridLinesGroupBox;
        private System.Windows.Forms.Button AlignSelectionButton;
        private System.Windows.Forms.CheckBox GridlinesCheckbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox MajorGridlinesDropDown;
        private System.Windows.Forms.ComboBox MinorGridlinesDropDown;
        private System.Windows.Forms.GroupBox DefaultsGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ModuleDropDown;
        private System.Windows.Forms.CheckBox ShowUnavailableCheckBox;
        private ProductionGraphViewer GraphViewer;
        private System.Windows.Forms.Button AddItemButton;
        private System.Windows.Forms.Button AddRecipeButton;
        private System.Windows.Forms.CheckBox DynamicLWCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox AssemblerDropDown;
		private System.Windows.Forms.CheckBox ShowNodeRecipeCheckBox;
		private System.Windows.Forms.RadioButton HighLodRadioButton;
		private System.Windows.Forms.RadioButton MediumLodRadioButton;
		private System.Windows.Forms.RadioButton LowLodRadioButton;
		private System.Windows.Forms.Label label6;
	}
}

