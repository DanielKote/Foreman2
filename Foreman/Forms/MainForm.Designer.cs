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
			this.GraphViewer = new Foreman.ProductionGraphViewer();
			this.MenuTable = new System.Windows.Forms.TableLayoutPanel();
			this.MenuButtonsTable = new System.Windows.Forms.TableLayoutPanel();
			this.AddItemButton = new System.Windows.Forms.Button();
			this.AddRecipeButton = new System.Windows.Forms.Button();
			this.ExportImageButton = new System.Windows.Forms.Button();
			this.EnableDisableButton = new System.Windows.Forms.Button();
			this.MainHelpButton = new System.Windows.Forms.Button();
			this.ClearButton = new System.Windows.Forms.Button();
			this.LoadGraphButton = new System.Windows.Forms.Button();
			this.SaveGraphButton = new System.Windows.Forms.Button();
			this.ProductionGroupBox = new System.Windows.Forms.GroupBox();
			this.GraphOptionsTable = new System.Windows.Forms.TableLayoutPanel();
			this.PauseUpdatesCheckbox = new System.Windows.Forms.CheckBox();
			this.ShowNodeRecipeCheckBox = new System.Windows.Forms.CheckBox();
			this.HighLodRadioButton = new System.Windows.Forms.RadioButton();
			this.DynamicLWCheckBox = new System.Windows.Forms.CheckBox();
			this.label6 = new System.Windows.Forms.Label();
			this.MediumLodRadioButton = new System.Windows.Forms.RadioButton();
			this.LowLodRadioButton = new System.Windows.Forms.RadioButton();
			this.ShowUnavailableCheckBox = new System.Windows.Forms.CheckBox();
			this.GridLinesGroupBox = new System.Windows.Forms.GroupBox();
			this.GridlinesTable = new System.Windows.Forms.TableLayoutPanel();
			this.AlignSelectionButton = new System.Windows.Forms.Button();
			this.MinorGridlinesDropDown = new System.Windows.Forms.ComboBox();
			this.GridlinesCheckbox = new System.Windows.Forms.CheckBox();
			this.MajorGridlinesDropDown = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.DefaultsGroupBox = new System.Windows.Forms.GroupBox();
			this.DefaultsTable = new System.Windows.Forms.TableLayoutPanel();
			this.ModuleDropDown = new System.Windows.Forms.ComboBox();
			this.AssemblerDropDown = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.RateOptionsDropDown = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.RecipeEditPanelPositionLockCheckBox = new System.Windows.Forms.CheckBox();
			this.MainLayoutPanel.SuspendLayout();
			this.MenuTable.SuspendLayout();
			this.MenuButtonsTable.SuspendLayout();
			this.ProductionGroupBox.SuspendLayout();
			this.GraphOptionsTable.SuspendLayout();
			this.GridLinesGroupBox.SuspendLayout();
			this.GridlinesTable.SuspendLayout();
			this.DefaultsGroupBox.SuspendLayout();
			this.DefaultsTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainLayoutPanel
			// 
			this.MainLayoutPanel.ColumnCount = 1;
			this.MainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MainLayoutPanel.Controls.Add(this.GraphViewer, 0, 1);
			this.MainLayoutPanel.Controls.Add(this.MenuTable, 0, 0);
			this.MainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.MainLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.MainLayoutPanel.Name = "MainLayoutPanel";
			this.MainLayoutPanel.RowCount = 2;
			this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainLayoutPanel.Size = new System.Drawing.Size(934, 661);
			this.MainLayoutPanel.TabIndex = 1;
			// 
			// GraphViewer
			// 
			this.GraphViewer.AllowDrop = true;
			this.GraphViewer.AutoSize = true;
			this.GraphViewer.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.GraphViewer.BackColor = System.Drawing.Color.White;
			this.GraphViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.GraphViewer.DCache = null;
			this.GraphViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GraphViewer.LevelOfDetail = Foreman.ProductionGraphViewer.LOD.Medium;
			this.GraphViewer.Location = new System.Drawing.Point(3, 132);
			this.GraphViewer.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.GraphViewer.MouseDownElement = null;
			this.GraphViewer.Name = "GraphViewer";
			this.GraphViewer.RecipeTooltipEnabled = false;
			this.GraphViewer.SelectedRateUnit = Foreman.ProductionGraphViewer.RateUnit.Per1Sec;
			this.GraphViewer.Size = new System.Drawing.Size(928, 526);
			this.GraphViewer.TabIndex = 12;
			this.GraphViewer.TooltipsEnabled = true;
			this.GraphViewer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GraphViewer_KeyDown);
			// 
			// MenuTable
			// 
			this.MenuTable.AutoSize = true;
			this.MenuTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MenuTable.ColumnCount = 6;
			this.MenuTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MenuTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MenuTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MenuTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MenuTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MenuTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MenuTable.Controls.Add(this.MenuButtonsTable, 0, 0);
			this.MenuTable.Controls.Add(this.ProductionGroupBox, 1, 0);
			this.MenuTable.Controls.Add(this.ShowUnavailableCheckBox, 4, 0);
			this.MenuTable.Controls.Add(this.GridLinesGroupBox, 2, 0);
			this.MenuTable.Controls.Add(this.DefaultsGroupBox, 3, 0);
			this.MenuTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MenuTable.Location = new System.Drawing.Point(3, 3);
			this.MenuTable.Name = "MenuTable";
			this.MenuTable.RowCount = 1;
			this.MenuTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MenuTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.MenuTable.Size = new System.Drawing.Size(928, 126);
			this.MenuTable.TabIndex = 18;
			// 
			// MenuButtonsTable
			// 
			this.MenuButtonsTable.AutoSize = true;
			this.MenuButtonsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MenuButtonsTable.ColumnCount = 2;
			this.MenuButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MenuButtonsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MenuButtonsTable.Controls.Add(this.AddItemButton, 1, 2);
			this.MenuButtonsTable.Controls.Add(this.AddRecipeButton, 1, 3);
			this.MenuButtonsTable.Controls.Add(this.ExportImageButton, 1, 1);
			this.MenuButtonsTable.Controls.Add(this.EnableDisableButton, 1, 0);
			this.MenuButtonsTable.Controls.Add(this.MainHelpButton, 0, 3);
			this.MenuButtonsTable.Controls.Add(this.ClearButton, 0, 2);
			this.MenuButtonsTable.Controls.Add(this.LoadGraphButton, 0, 1);
			this.MenuButtonsTable.Controls.Add(this.SaveGraphButton, 0, 0);
			this.MenuButtonsTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MenuButtonsTable.Location = new System.Drawing.Point(3, 7);
			this.MenuButtonsTable.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
			this.MenuButtonsTable.Name = "MenuButtonsTable";
			this.MenuButtonsTable.RowCount = 4;
			this.MenuButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.MenuButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.MenuButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.MenuButtonsTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.MenuButtonsTable.Size = new System.Drawing.Size(164, 116);
			this.MenuButtonsTable.TabIndex = 0;
			// 
			// AddItemButton
			// 
			this.AddItemButton.AutoSize = true;
			this.AddItemButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AddItemButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddItemButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddItemButton.Location = new System.Drawing.Point(79, 60);
			this.AddItemButton.Margin = new System.Windows.Forms.Padding(2);
			this.AddItemButton.Name = "AddItemButton";
			this.AddItemButton.Size = new System.Drawing.Size(83, 25);
			this.AddItemButton.TabIndex = 11;
			this.AddItemButton.Text = "Add Item";
			this.AddItemButton.UseVisualStyleBackColor = true;
			this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
			// 
			// AddRecipeButton
			// 
			this.AddRecipeButton.AutoSize = true;
			this.AddRecipeButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AddRecipeButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddRecipeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddRecipeButton.Location = new System.Drawing.Point(79, 89);
			this.AddRecipeButton.Margin = new System.Windows.Forms.Padding(2);
			this.AddRecipeButton.Name = "AddRecipeButton";
			this.AddRecipeButton.Size = new System.Drawing.Size(83, 25);
			this.AddRecipeButton.TabIndex = 10;
			this.AddRecipeButton.Text = "Add Recipe";
			this.AddRecipeButton.UseVisualStyleBackColor = true;
			this.AddRecipeButton.Click += new System.EventHandler(this.AddRecipeButton_Click);
			// 
			// ExportImageButton
			// 
			this.ExportImageButton.AutoSize = true;
			this.ExportImageButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ExportImageButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportImageButton.Location = new System.Drawing.Point(79, 31);
			this.ExportImageButton.Margin = new System.Windows.Forms.Padding(2);
			this.ExportImageButton.Name = "ExportImageButton";
			this.ExportImageButton.Size = new System.Drawing.Size(83, 25);
			this.ExportImageButton.TabIndex = 8;
			this.ExportImageButton.Text = "Export Image";
			this.ExportImageButton.UseVisualStyleBackColor = true;
			this.ExportImageButton.Click += new System.EventHandler(this.ExportImageButton_Click);
			// 
			// EnableDisableButton
			// 
			this.EnableDisableButton.AutoSize = true;
			this.EnableDisableButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.EnableDisableButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EnableDisableButton.Location = new System.Drawing.Point(79, 2);
			this.EnableDisableButton.Margin = new System.Windows.Forms.Padding(2);
			this.EnableDisableButton.Name = "EnableDisableButton";
			this.EnableDisableButton.Size = new System.Drawing.Size(83, 25);
			this.EnableDisableButton.TabIndex = 7;
			this.EnableDisableButton.Text = "Settings";
			this.EnableDisableButton.UseVisualStyleBackColor = true;
			this.EnableDisableButton.Click += new System.EventHandler(this.SettingsButton_Click);
			// 
			// MainHelpButton
			// 
			this.MainHelpButton.AutoSize = true;
			this.MainHelpButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainHelpButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainHelpButton.Location = new System.Drawing.Point(2, 89);
			this.MainHelpButton.Margin = new System.Windows.Forms.Padding(2);
			this.MainHelpButton.Name = "MainHelpButton";
			this.MainHelpButton.Size = new System.Drawing.Size(73, 25);
			this.MainHelpButton.TabIndex = 9;
			this.MainHelpButton.Text = "Help";
			this.MainHelpButton.UseVisualStyleBackColor = true;
			this.MainHelpButton.Click += new System.EventHandler(this.MainHelpButton_Click);
			// 
			// ClearButton
			// 
			this.ClearButton.AutoSize = true;
			this.ClearButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClearButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClearButton.Location = new System.Drawing.Point(2, 60);
			this.ClearButton.Margin = new System.Windows.Forms.Padding(2);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = new System.Drawing.Size(73, 25);
			this.ClearButton.TabIndex = 6;
			this.ClearButton.Text = "Clear Graph";
			this.ClearButton.UseVisualStyleBackColor = true;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// LoadGraphButton
			// 
			this.LoadGraphButton.AutoSize = true;
			this.LoadGraphButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.LoadGraphButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LoadGraphButton.Location = new System.Drawing.Point(2, 31);
			this.LoadGraphButton.Margin = new System.Windows.Forms.Padding(2);
			this.LoadGraphButton.Name = "LoadGraphButton";
			this.LoadGraphButton.Size = new System.Drawing.Size(73, 25);
			this.LoadGraphButton.TabIndex = 10;
			this.LoadGraphButton.Text = "Load";
			this.LoadGraphButton.UseVisualStyleBackColor = true;
			this.LoadGraphButton.Click += new System.EventHandler(this.LoadGraphButton_Click);
			// 
			// SaveGraphButton
			// 
			this.SaveGraphButton.AutoSize = true;
			this.SaveGraphButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.SaveGraphButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SaveGraphButton.Location = new System.Drawing.Point(2, 2);
			this.SaveGraphButton.Margin = new System.Windows.Forms.Padding(2);
			this.SaveGraphButton.Name = "SaveGraphButton";
			this.SaveGraphButton.Size = new System.Drawing.Size(73, 25);
			this.SaveGraphButton.TabIndex = 9;
			this.SaveGraphButton.Text = "Save";
			this.SaveGraphButton.UseVisualStyleBackColor = true;
			this.SaveGraphButton.Click += new System.EventHandler(this.SaveGraphButton_Click);
			// 
			// ProductionGroupBox
			// 
			this.ProductionGroupBox.AutoSize = true;
			this.ProductionGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ProductionGroupBox.Controls.Add(this.GraphOptionsTable);
			this.ProductionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductionGroupBox.Location = new System.Drawing.Point(173, 3);
			this.ProductionGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
			this.ProductionGroupBox.Name = "ProductionGroupBox";
			this.ProductionGroupBox.Padding = new System.Windows.Forms.Padding(0);
			this.ProductionGroupBox.Size = new System.Drawing.Size(192, 118);
			this.ProductionGroupBox.TabIndex = 4;
			this.ProductionGroupBox.TabStop = false;
			this.ProductionGroupBox.Text = "Graph Options:";
			// 
			// GraphOptionsTable
			// 
			this.GraphOptionsTable.AutoSize = true;
			this.GraphOptionsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.GraphOptionsTable.ColumnCount = 4;
			this.GraphOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.GraphOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.GraphOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.GraphOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.GraphOptionsTable.Controls.Add(this.ShowNodeRecipeCheckBox, 0, 2);
			this.GraphOptionsTable.Controls.Add(this.HighLodRadioButton, 3, 0);
			this.GraphOptionsTable.Controls.Add(this.DynamicLWCheckBox, 0, 1);
			this.GraphOptionsTable.Controls.Add(this.label6, 0, 0);
			this.GraphOptionsTable.Controls.Add(this.MediumLodRadioButton, 2, 0);
			this.GraphOptionsTable.Controls.Add(this.LowLodRadioButton, 1, 0);
			this.GraphOptionsTable.Controls.Add(this.PauseUpdatesCheckbox, 0, 4);
			this.GraphOptionsTable.Controls.Add(this.RecipeEditPanelPositionLockCheckBox, 0, 3);
			this.GraphOptionsTable.Location = new System.Drawing.Point(3, 16);
			this.GraphOptionsTable.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.GraphOptionsTable.Name = "GraphOptionsTable";
			this.GraphOptionsTable.RowCount = 5;
			this.GraphOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GraphOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GraphOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GraphOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GraphOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GraphOptionsTable.Size = new System.Drawing.Size(186, 89);
			this.GraphOptionsTable.TabIndex = 2;
			// 
			// PauseUpdatesCheckbox
			// 
			this.PauseUpdatesCheckbox.AutoSize = true;
			this.GraphOptionsTable.SetColumnSpan(this.PauseUpdatesCheckbox, 4);
			this.PauseUpdatesCheckbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PauseUpdatesCheckbox.Location = new System.Drawing.Point(3, 72);
			this.PauseUpdatesCheckbox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.PauseUpdatesCheckbox.Name = "PauseUpdatesCheckbox";
			this.PauseUpdatesCheckbox.Size = new System.Drawing.Size(180, 17);
			this.PauseUpdatesCheckbox.TabIndex = 3;
			this.PauseUpdatesCheckbox.Text = "Pause all calculations";
			this.PauseUpdatesCheckbox.UseVisualStyleBackColor = true;
			this.PauseUpdatesCheckbox.CheckedChanged += new System.EventHandler(this.PauseUpdatesCheckbox_CheckedChanged);
			// 
			// ShowNodeRecipeCheckBox
			// 
			this.ShowNodeRecipeCheckBox.AutoSize = true;
			this.GraphOptionsTable.SetColumnSpan(this.ShowNodeRecipeCheckBox, 4);
			this.ShowNodeRecipeCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShowNodeRecipeCheckBox.Location = new System.Drawing.Point(3, 38);
			this.ShowNodeRecipeCheckBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.ShowNodeRecipeCheckBox.Name = "ShowNodeRecipeCheckBox";
			this.ShowNodeRecipeCheckBox.Size = new System.Drawing.Size(180, 17);
			this.ShowNodeRecipeCheckBox.TabIndex = 6;
			this.ShowNodeRecipeCheckBox.Text = "Show Recipes";
			this.ShowNodeRecipeCheckBox.UseVisualStyleBackColor = true;
			this.ShowNodeRecipeCheckBox.CheckedChanged += new System.EventHandler(this.ShowNodeRecipeCheckBox_CheckedChanged);
			// 
			// HighLodRadioButton
			// 
			this.HighLodRadioButton.AutoSize = true;
			this.HighLodRadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HighLodRadioButton.Location = new System.Drawing.Point(137, 2);
			this.HighLodRadioButton.Margin = new System.Windows.Forms.Padding(2);
			this.HighLodRadioButton.Name = "HighLodRadioButton";
			this.HighLodRadioButton.Size = new System.Drawing.Size(47, 17);
			this.HighLodRadioButton.TabIndex = 10;
			this.HighLodRadioButton.TabStop = true;
			this.HighLodRadioButton.Text = "High";
			this.HighLodRadioButton.UseVisualStyleBackColor = true;
			this.HighLodRadioButton.CheckedChanged += new System.EventHandler(this.LODRadioButton_CheckedChanged);
			// 
			// DynamicLWCheckBox
			// 
			this.DynamicLWCheckBox.AutoSize = true;
			this.GraphOptionsTable.SetColumnSpan(this.DynamicLWCheckBox, 4);
			this.DynamicLWCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicLWCheckBox.Location = new System.Drawing.Point(3, 21);
			this.DynamicLWCheckBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.DynamicLWCheckBox.Name = "DynamicLWCheckBox";
			this.DynamicLWCheckBox.Size = new System.Drawing.Size(180, 17);
			this.DynamicLWCheckBox.TabIndex = 4;
			this.DynamicLWCheckBox.Text = "Dynamic Link-Width";
			this.DynamicLWCheckBox.UseVisualStyleBackColor = true;
			this.DynamicLWCheckBox.CheckedChanged += new System.EventHandler(this.DynamicLWCheckBox_CheckedChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label6.Location = new System.Drawing.Point(2, 0);
			this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(32, 21);
			this.label6.TabIndex = 7;
			this.label6.Text = "LOD:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MediumLodRadioButton
			// 
			this.MediumLodRadioButton.AutoSize = true;
			this.MediumLodRadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MediumLodRadioButton.Location = new System.Drawing.Point(87, 2);
			this.MediumLodRadioButton.Margin = new System.Windows.Forms.Padding(2);
			this.MediumLodRadioButton.Name = "MediumLodRadioButton";
			this.MediumLodRadioButton.Size = new System.Drawing.Size(46, 17);
			this.MediumLodRadioButton.TabIndex = 9;
			this.MediumLodRadioButton.TabStop = true;
			this.MediumLodRadioButton.Text = "Med";
			this.MediumLodRadioButton.UseVisualStyleBackColor = true;
			this.MediumLodRadioButton.CheckedChanged += new System.EventHandler(this.LODRadioButton_CheckedChanged);
			// 
			// LowLodRadioButton
			// 
			this.LowLodRadioButton.AutoSize = true;
			this.LowLodRadioButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LowLodRadioButton.Location = new System.Drawing.Point(38, 2);
			this.LowLodRadioButton.Margin = new System.Windows.Forms.Padding(2);
			this.LowLodRadioButton.Name = "LowLodRadioButton";
			this.LowLodRadioButton.Size = new System.Drawing.Size(45, 17);
			this.LowLodRadioButton.TabIndex = 8;
			this.LowLodRadioButton.TabStop = true;
			this.LowLodRadioButton.Text = "Low";
			this.LowLodRadioButton.UseVisualStyleBackColor = true;
			this.LowLodRadioButton.CheckedChanged += new System.EventHandler(this.LODRadioButton_CheckedChanged);
			// 
			// ShowUnavailableCheckBox
			// 
			this.ShowUnavailableCheckBox.AutoSize = true;
			this.ShowUnavailableCheckBox.Location = new System.Drawing.Point(784, 3);
			this.ShowUnavailableCheckBox.Name = "ShowUnavailableCheckBox";
			this.ShowUnavailableCheckBox.Size = new System.Drawing.Size(98, 17);
			this.ShowUnavailableCheckBox.TabIndex = 0;
			this.ShowUnavailableCheckBox.Text = "DEV: Show Un";
			this.ShowUnavailableCheckBox.UseVisualStyleBackColor = true;
			this.ShowUnavailableCheckBox.CheckedChanged += new System.EventHandler(this.DEV_ShowUnavailableCheckBox_CheckChanged);
			// 
			// GridLinesGroupBox
			// 
			this.GridLinesGroupBox.AutoSize = true;
			this.GridLinesGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.GridLinesGroupBox.Controls.Add(this.GridlinesTable);
			this.GridLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridLinesGroupBox.Location = new System.Drawing.Point(371, 3);
			this.GridLinesGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
			this.GridLinesGroupBox.Name = "GridLinesGroupBox";
			this.GridLinesGroupBox.Padding = new System.Windows.Forms.Padding(0);
			this.GridLinesGroupBox.Size = new System.Drawing.Size(208, 118);
			this.GridLinesGroupBox.TabIndex = 17;
			this.GridLinesGroupBox.TabStop = false;
			this.GridLinesGroupBox.Text = "Gridlines (2n scaling)";
			// 
			// GridlinesTable
			// 
			this.GridlinesTable.AutoSize = true;
			this.GridlinesTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.GridlinesTable.ColumnCount = 2;
			this.GridlinesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.GridlinesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.GridlinesTable.Controls.Add(this.AlignSelectionButton, 1, 2);
			this.GridlinesTable.Controls.Add(this.MinorGridlinesDropDown, 1, 0);
			this.GridlinesTable.Controls.Add(this.GridlinesCheckbox, 0, 2);
			this.GridlinesTable.Controls.Add(this.MajorGridlinesDropDown, 1, 1);
			this.GridlinesTable.Controls.Add(this.label3, 0, 1);
			this.GridlinesTable.Controls.Add(this.label2, 0, 0);
			this.GridlinesTable.Location = new System.Drawing.Point(3, 16);
			this.GridlinesTable.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.GridlinesTable.Name = "GridlinesTable";
			this.GridlinesTable.RowCount = 3;
			this.GridlinesTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GridlinesTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GridlinesTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GridlinesTable.Size = new System.Drawing.Size(202, 75);
			this.GridlinesTable.TabIndex = 2;
			// 
			// AlignSelectionButton
			// 
			this.AlignSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlignSelectionButton.Location = new System.Drawing.Point(103, 52);
			this.AlignSelectionButton.Margin = new System.Windows.Forms.Padding(1, 2, 1, 0);
			this.AlignSelectionButton.Name = "AlignSelectionButton";
			this.AlignSelectionButton.Size = new System.Drawing.Size(98, 23);
			this.AlignSelectionButton.TabIndex = 6;
			this.AlignSelectionButton.Text = "Align Selected";
			this.AlignSelectionButton.UseVisualStyleBackColor = true;
			this.AlignSelectionButton.Click += new System.EventHandler(this.AlignSelectionButton_Click);
			// 
			// MinorGridlinesDropDown
			// 
			this.MinorGridlinesDropDown.Dock = System.Windows.Forms.DockStyle.Fill;
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
			this.MinorGridlinesDropDown.Location = new System.Drawing.Point(104, 2);
			this.MinorGridlinesDropDown.Margin = new System.Windows.Forms.Padding(2);
			this.MinorGridlinesDropDown.Name = "MinorGridlinesDropDown";
			this.MinorGridlinesDropDown.Size = new System.Drawing.Size(96, 21);
			this.MinorGridlinesDropDown.TabIndex = 3;
			this.MinorGridlinesDropDown.SelectedIndexChanged += new System.EventHandler(this.MinorGridlinesDropDown_SelectedIndexChanged);
			// 
			// GridlinesCheckbox
			// 
			this.GridlinesCheckbox.AutoSize = true;
			this.GridlinesCheckbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridlinesCheckbox.Location = new System.Drawing.Point(3, 53);
			this.GridlinesCheckbox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.GridlinesCheckbox.Name = "GridlinesCheckbox";
			this.GridlinesCheckbox.Size = new System.Drawing.Size(96, 22);
			this.GridlinesCheckbox.TabIndex = 3;
			this.GridlinesCheckbox.Text = "Show Gridlines";
			this.GridlinesCheckbox.UseVisualStyleBackColor = true;
			this.GridlinesCheckbox.CheckedChanged += new System.EventHandler(this.GridlinesCheckbox_CheckedChanged);
			// 
			// MajorGridlinesDropDown
			// 
			this.MajorGridlinesDropDown.Dock = System.Windows.Forms.DockStyle.Fill;
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
			this.MajorGridlinesDropDown.Location = new System.Drawing.Point(104, 27);
			this.MajorGridlinesDropDown.Margin = new System.Windows.Forms.Padding(2);
			this.MajorGridlinesDropDown.Name = "MajorGridlinesDropDown";
			this.MajorGridlinesDropDown.Size = new System.Drawing.Size(96, 21);
			this.MajorGridlinesDropDown.TabIndex = 4;
			this.MajorGridlinesDropDown.SelectedIndexChanged += new System.EventHandler(this.MajorGridlinesDropDown_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label3.Location = new System.Drawing.Point(2, 25);
			this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(98, 25);
			this.label3.TabIndex = 5;
			this.label3.Text = "Major Gridlines:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(2, 0);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(98, 25);
			this.label2.TabIndex = 3;
			this.label2.Text = "Minor Gridlines:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DefaultsGroupBox
			// 
			this.DefaultsGroupBox.AutoSize = true;
			this.DefaultsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.DefaultsGroupBox.Controls.Add(this.DefaultsTable);
			this.DefaultsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefaultsGroupBox.Location = new System.Drawing.Point(585, 3);
			this.DefaultsGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 5);
			this.DefaultsGroupBox.Name = "DefaultsGroupBox";
			this.DefaultsGroupBox.Padding = new System.Windows.Forms.Padding(0);
			this.DefaultsGroupBox.Size = new System.Drawing.Size(193, 118);
			this.DefaultsGroupBox.TabIndex = 7;
			this.DefaultsGroupBox.TabStop = false;
			this.DefaultsGroupBox.Text = "Defaults:";
			// 
			// DefaultsTable
			// 
			this.DefaultsTable.AutoSize = true;
			this.DefaultsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.DefaultsTable.ColumnCount = 2;
			this.DefaultsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.DefaultsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.DefaultsTable.Controls.Add(this.ModuleDropDown, 1, 2);
			this.DefaultsTable.Controls.Add(this.AssemblerDropDown, 1, 1);
			this.DefaultsTable.Controls.Add(this.label5, 0, 1);
			this.DefaultsTable.Controls.Add(this.RateOptionsDropDown, 1, 0);
			this.DefaultsTable.Controls.Add(this.label4, 0, 0);
			this.DefaultsTable.Controls.Add(this.label1, 0, 2);
			this.DefaultsTable.Location = new System.Drawing.Point(3, 16);
			this.DefaultsTable.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.DefaultsTable.Name = "DefaultsTable";
			this.DefaultsTable.RowCount = 3;
			this.DefaultsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DefaultsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DefaultsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.DefaultsTable.Size = new System.Drawing.Size(187, 77);
			this.DefaultsTable.TabIndex = 2;
			// 
			// ModuleDropDown
			// 
			this.ModuleDropDown.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModuleDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ModuleDropDown.FormattingEnabled = true;
			this.ModuleDropDown.Location = new System.Drawing.Point(69, 54);
			this.ModuleDropDown.Margin = new System.Windows.Forms.Padding(2);
			this.ModuleDropDown.Name = "ModuleDropDown";
			this.ModuleDropDown.Size = new System.Drawing.Size(116, 21);
			this.ModuleDropDown.TabIndex = 1;
			this.ModuleDropDown.SelectedIndexChanged += new System.EventHandler(this.ModuleDropDown_SelectedIndexChanged);
			// 
			// AssemblerDropDown
			// 
			this.AssemblerDropDown.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssemblerDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.AssemblerDropDown.FormattingEnabled = true;
			this.AssemblerDropDown.Location = new System.Drawing.Point(69, 29);
			this.AssemblerDropDown.Margin = new System.Windows.Forms.Padding(2);
			this.AssemblerDropDown.Name = "AssemblerDropDown";
			this.AssemblerDropDown.Size = new System.Drawing.Size(116, 21);
			this.AssemblerDropDown.TabIndex = 3;
			this.AssemblerDropDown.SelectedIndexChanged += new System.EventHandler(this.AssemblerDropDown_SelectedIndexChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(2, 27);
			this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(63, 25);
			this.label5.TabIndex = 4;
			this.label5.Text = "Assemblers:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// RateOptionsDropDown
			// 
			this.RateOptionsDropDown.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateOptionsDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.RateOptionsDropDown.FormattingEnabled = true;
			this.RateOptionsDropDown.Location = new System.Drawing.Point(70, 3);
			this.RateOptionsDropDown.Name = "RateOptionsDropDown";
			this.RateOptionsDropDown.Size = new System.Drawing.Size(114, 21);
			this.RateOptionsDropDown.TabIndex = 2;
			this.RateOptionsDropDown.SelectedIndexChanged += new System.EventHandler(this.RateOptionsDropDown_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(2, 0);
			this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(63, 27);
			this.label4.TabIndex = 5;
			this.label4.Text = "Base Time:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(2, 52);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 25);
			this.label1.TabIndex = 2;
			this.label1.Text = "Modules:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// RecipeEditPanelPositionLockCheckBox
			// 
			this.RecipeEditPanelPositionLockCheckBox.AutoSize = true;
			this.GraphOptionsTable.SetColumnSpan(this.RecipeEditPanelPositionLockCheckBox, 4);
			this.RecipeEditPanelPositionLockCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipeEditPanelPositionLockCheckBox.Location = new System.Drawing.Point(3, 55);
			this.RecipeEditPanelPositionLockCheckBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
			this.RecipeEditPanelPositionLockCheckBox.Name = "RecipeEditPanelPositionLockCheckBox";
			this.RecipeEditPanelPositionLockCheckBox.Size = new System.Drawing.Size(180, 17);
			this.RecipeEditPanelPositionLockCheckBox.TabIndex = 11;
			this.RecipeEditPanelPositionLockCheckBox.Text = "Lock Recipe Editor to TL corner";
			this.RecipeEditPanelPositionLockCheckBox.UseVisualStyleBackColor = true;
			this.RecipeEditPanelPositionLockCheckBox.CheckedChanged += new System.EventHandler(this.RecipeEditPanelPositionLockCheckBox_CheckedChanged);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(934, 661);
			this.Controls.Add(this.MainLayoutPanel);
			this.DoubleBuffered = true;
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(950, 700);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Foreman 2.0";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.MainLayoutPanel.ResumeLayout(false);
			this.MainLayoutPanel.PerformLayout();
			this.MenuTable.ResumeLayout(false);
			this.MenuTable.PerformLayout();
			this.MenuButtonsTable.ResumeLayout(false);
			this.MenuButtonsTable.PerformLayout();
			this.ProductionGroupBox.ResumeLayout(false);
			this.ProductionGroupBox.PerformLayout();
			this.GraphOptionsTable.ResumeLayout(false);
			this.GraphOptionsTable.PerformLayout();
			this.GridLinesGroupBox.ResumeLayout(false);
			this.GridLinesGroupBox.PerformLayout();
			this.GridlinesTable.ResumeLayout(false);
			this.GridlinesTable.PerformLayout();
			this.DefaultsGroupBox.ResumeLayout(false);
			this.DefaultsGroupBox.PerformLayout();
			this.DefaultsTable.ResumeLayout(false);
			this.DefaultsTable.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
        private System.Windows.Forms.TableLayoutPanel MainLayoutPanel;
        private System.Windows.Forms.Button SaveGraphButton;
        private System.Windows.Forms.Button LoadGraphButton;
        private System.Windows.Forms.Button ClearButton;
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
		private System.Windows.Forms.TableLayoutPanel MenuTable;
		private System.Windows.Forms.TableLayoutPanel MenuButtonsTable;
		private System.Windows.Forms.TableLayoutPanel GraphOptionsTable;
		private System.Windows.Forms.TableLayoutPanel GridlinesTable;
		private System.Windows.Forms.TableLayoutPanel DefaultsTable;
		private System.Windows.Forms.CheckBox RecipeEditPanelPositionLockCheckBox;
	}
}

