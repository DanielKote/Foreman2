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
			this.AddItemButton = new System.Windows.Forms.Button();
			this.ItemListView = new System.Windows.Forms.ListView();
			this.h_Name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ItemImageList = new System.Windows.Forms.ImageList(this.components);
			this.MainToolbar = new System.Windows.Forms.FlowLayoutPanel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rateOptionsDropDown = new System.Windows.Forms.ComboBox();
			this.rateButton = new System.Windows.Forms.RadioButton();
			this.fixedAmountButton = new System.Windows.Forms.RadioButton();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.AutomaticCompleteButton = new System.Windows.Forms.Button();
			this.ClearButton = new System.Windows.Forms.Button();
			this.ExportImageButton = new System.Windows.Forms.Button();
			this.saveGraphButton = new System.Windows.Forms.Button();
			this.loadGraphButton = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.SingleAssemblerPerRecipeCheckBox = new System.Windows.Forms.CheckBox();
			this.AssemblerDisplayCheckBox = new System.Windows.Forms.CheckBox();
			this.MinerDisplayCheckBox = new System.Windows.Forms.CheckBox();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.FactorioDirectoryButton = new System.Windows.Forms.Button();
			this.ModDirectoryButton = new System.Windows.Forms.Button();
			this.ReloadButton = new System.Windows.Forms.Button();
			this.FilterTextBox = new System.Windows.Forms.TextBox();
			this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
			this.EnableDisableButton = new System.Windows.Forms.Button();
			this.GraphViewer = new Foreman.ProductionGraphViewer();
			this.tableLayoutPanel1.SuspendLayout();
			this.MainToolbar.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.flowLayoutPanel2.SuspendLayout();
			this.flowLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.AddItemButton, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.ItemListView, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.MainToolbar, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.GraphViewer, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.FilterTextBox, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1306, 800);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// AddItemButton
			// 
			this.AddItemButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddItemButton.Location = new System.Drawing.Point(3, 773);
			this.AddItemButton.Name = "AddItemButton";
			this.AddItemButton.Size = new System.Drawing.Size(220, 24);
			this.AddItemButton.TabIndex = 2;
			this.AddItemButton.Text = "Add Output";
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
			this.ItemListView.Location = new System.Drawing.Point(3, 134);
			this.ItemListView.Name = "ItemListView";
			this.ItemListView.Size = new System.Drawing.Size(220, 633);
			this.ItemListView.SmallImageList = this.ItemImageList;
			this.ItemListView.TabIndex = 11;
			this.ItemListView.UseCompatibleStateImageBehavior = false;
			this.ItemListView.View = System.Windows.Forms.View.Details;
			this.ItemListView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ItemListView_ItemDrag);
			this.ItemListView.SelectedIndexChanged += new System.EventHandler(this.ItemListView_SelectedIndexChanged);
			this.ItemListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ItemListView_MouseDoubleClick);
			// 
			// h_Name
			// 
			this.h_Name.Text = "Name";
			this.h_Name.Width = 216;
			// 
			// ItemImageList
			// 
			this.ItemImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.ItemImageList.ImageSize = new System.Drawing.Size(32, 32);
			this.ItemImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// MainToolbar
			// 
			this.MainToolbar.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.MainToolbar, 2);
			this.MainToolbar.Controls.Add(this.groupBox1);
			this.MainToolbar.Controls.Add(this.flowLayoutPanel1);
			this.MainToolbar.Controls.Add(this.flowLayoutPanel3);
			this.MainToolbar.Controls.Add(this.groupBox2);
			this.MainToolbar.Controls.Add(this.flowLayoutPanel2);
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
			this.flowLayoutPanel1.Controls.Add(this.ClearButton);
			this.flowLayoutPanel1.Controls.Add(this.EnableDisableButton);
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(201, 3);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(185, 93);
			this.flowLayoutPanel1.TabIndex = 9;
			// 
			// AutomaticCompleteButton
			// 
			this.AutomaticCompleteButton.Location = new System.Drawing.Point(3, 3);
			this.AutomaticCompleteButton.Name = "AutomaticCompleteButton";
			this.AutomaticCompleteButton.Size = new System.Drawing.Size(179, 25);
			this.AutomaticCompleteButton.TabIndex = 5;
			this.AutomaticCompleteButton.Text = "Automatically complete flowchart";
			this.AutomaticCompleteButton.UseVisualStyleBackColor = true;
			this.AutomaticCompleteButton.Click += new System.EventHandler(this.AutomaticCompleteButton_Click);
			// 
			// ClearButton
			// 
			this.ClearButton.Location = new System.Drawing.Point(3, 34);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = new System.Drawing.Size(179, 25);
			this.ClearButton.TabIndex = 6;
			this.ClearButton.Text = "Clear flowchart";
			this.ClearButton.UseVisualStyleBackColor = true;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// ExportImageButton
			// 
			this.ExportImageButton.Location = new System.Drawing.Point(3, 65);
			this.ExportImageButton.Name = "ExportImageButton";
			this.ExportImageButton.Size = new System.Drawing.Size(179, 25);
			this.ExportImageButton.TabIndex = 8;
			this.ExportImageButton.Text = "Export as Image";
			this.ExportImageButton.UseVisualStyleBackColor = true;
			this.ExportImageButton.Click += new System.EventHandler(this.ExportImageButton_Click);
			// 
			// saveGraphButton
			// 
			this.saveGraphButton.Location = new System.Drawing.Point(3, 3);
			this.saveGraphButton.Name = "saveGraphButton";
			this.saveGraphButton.Size = new System.Drawing.Size(179, 25);
			this.saveGraphButton.TabIndex = 9;
			this.saveGraphButton.Text = "Save";
			this.saveGraphButton.UseVisualStyleBackColor = true;
			this.saveGraphButton.Click += new System.EventHandler(this.saveGraphButton_Click);
			// 
			// loadGraphButton
			// 
			this.loadGraphButton.Location = new System.Drawing.Point(3, 34);
			this.loadGraphButton.Name = "loadGraphButton";
			this.loadGraphButton.Size = new System.Drawing.Size(179, 25);
			this.loadGraphButton.TabIndex = 10;
			this.loadGraphButton.Text = "Load";
			this.loadGraphButton.UseVisualStyleBackColor = true;
			this.loadGraphButton.Click += new System.EventHandler(this.loadGraphButton_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.AutoSize = true;
			this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox2.Controls.Add(this.SingleAssemblerPerRecipeCheckBox);
			this.groupBox2.Controls.Add(this.AssemblerDisplayCheckBox);
			this.groupBox2.Controls.Add(this.MinerDisplayCheckBox);
			this.groupBox2.Location = new System.Drawing.Point(583, 3);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox2.Size = new System.Drawing.Size(182, 95);
			this.groupBox2.TabIndex = 7;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Assemblers";
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
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.AutoSize = true;
			this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel2.Controls.Add(this.FactorioDirectoryButton);
			this.flowLayoutPanel2.Controls.Add(this.ModDirectoryButton);
			this.flowLayoutPanel2.Controls.Add(this.ReloadButton);
			this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel2.Location = new System.Drawing.Point(771, 3);
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
			// FilterTextBox
			// 
			this.FilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterTextBox.Location = new System.Drawing.Point(3, 108);
			this.FilterTextBox.Name = "FilterTextBox";
			this.FilterTextBox.Size = new System.Drawing.Size(220, 20);
			this.FilterTextBox.TabIndex = 13;
			this.FilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
			// 
			// flowLayoutPanel3
			// 
			this.flowLayoutPanel3.AutoSize = true;
			this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel3.Controls.Add(this.saveGraphButton);
			this.flowLayoutPanel3.Controls.Add(this.loadGraphButton);
			this.flowLayoutPanel3.Controls.Add(this.ExportImageButton);
			this.flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel3.Location = new System.Drawing.Point(392, 3);
			this.flowLayoutPanel3.Name = "flowLayoutPanel3";
			this.flowLayoutPanel3.Size = new System.Drawing.Size(185, 93);
			this.flowLayoutPanel3.TabIndex = 14;
			// 
			// EnableDisableButton
			// 
			this.EnableDisableButton.Location = new System.Drawing.Point(3, 65);
			this.EnableDisableButton.Name = "EnableDisableButton";
			this.EnableDisableButton.Size = new System.Drawing.Size(179, 25);
			this.EnableDisableButton.TabIndex = 7;
			this.EnableDisableButton.Text = "Enable/disable objects";
			this.EnableDisableButton.UseVisualStyleBackColor = true;
			this.EnableDisableButton.Click += new System.EventHandler(this.EnableDisableButton_Click);
			// 
			// GraphViewer
			// 
			this.GraphViewer.AllowDrop = true;
			this.GraphViewer.BackColor = System.Drawing.Color.White;
			this.GraphViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.GraphViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GraphViewer.DraggedElement = null;
			this.GraphViewer.Location = new System.Drawing.Point(229, 108);
			this.GraphViewer.Name = "GraphViewer";
			this.tableLayoutPanel1.SetRowSpan(this.GraphViewer, 3);
			this.GraphViewer.Size = new System.Drawing.Size(1074, 689);
			this.GraphViewer.TabIndex = 12;
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
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.flowLayoutPanel2.ResumeLayout(false);
			this.flowLayoutPanel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button AddItemButton;
		private System.Windows.Forms.ListView ItemListView;
		private System.Windows.Forms.ImageList ItemImageList;
		private System.Windows.Forms.ColumnHeader h_Name;
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
		private System.Windows.Forms.TextBox FilterTextBox;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Button FactorioDirectoryButton;
		private System.Windows.Forms.Button ModDirectoryButton;
		private System.Windows.Forms.Button ReloadButton;
		private System.Windows.Forms.Button saveGraphButton;
		private System.Windows.Forms.Button loadGraphButton;
		private System.Windows.Forms.Button EnableDisableButton;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
	}
}

