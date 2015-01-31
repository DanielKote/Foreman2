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
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.SingleAssemblerPerRecipeCheckBox = new System.Windows.Forms.CheckBox();
			this.AssemblerDisplayCheckBox = new System.Windows.Forms.CheckBox();
			this.MinerDisplayCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.AssemblerSelectionBox = new System.Windows.Forms.CheckedListBox();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.MinerSelectionBox = new System.Windows.Forms.CheckedListBox();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.ModuleSelectionBox = new System.Windows.Forms.CheckedListBox();
			this.GraphViewer = new Foreman.ProductionGraphViewer();
			this.tableLayoutPanel1.SuspendLayout();
			this.MainToolbar.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.groupBox7.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.AddItemButton, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.ItemListView, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.MainToolbar, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.GraphViewer, 1, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1175, 800);
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
			this.ItemListView.Location = new System.Drawing.Point(3, 108);
			this.ItemListView.Name = "ItemListView";
			this.ItemListView.Size = new System.Drawing.Size(220, 659);
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
			this.ItemImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.ItemImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// MainToolbar
			// 
			this.MainToolbar.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.MainToolbar, 2);
			this.MainToolbar.Controls.Add(this.groupBox1);
			this.MainToolbar.Controls.Add(this.flowLayoutPanel1);
			this.MainToolbar.Controls.Add(this.groupBox2);
			this.MainToolbar.Controls.Add(this.groupBox5);
			this.MainToolbar.Controls.Add(this.groupBox6);
			this.MainToolbar.Controls.Add(this.groupBox7);
			this.MainToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainToolbar.Location = new System.Drawing.Point(3, 3);
			this.MainToolbar.Name = "MainToolbar";
			this.MainToolbar.Size = new System.Drawing.Size(1169, 99);
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
			this.flowLayoutPanel1.Controls.Add(this.ExportImageButton);
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
			// groupBox2
			// 
			this.groupBox2.AutoSize = true;
			this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox2.Controls.Add(this.SingleAssemblerPerRecipeCheckBox);
			this.groupBox2.Controls.Add(this.AssemblerDisplayCheckBox);
			this.groupBox2.Controls.Add(this.MinerDisplayCheckBox);
			this.groupBox2.Location = new System.Drawing.Point(392, 3);
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
			this.MinerDisplayCheckBox.CheckedChanged += new System.EventHandler(this.MinerDisplayCheckBox_CheckedChanged);
			// 
			// groupBox5
			// 
			this.groupBox5.AutoSize = true;
			this.groupBox5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox5.Controls.Add(this.AssemblerSelectionBox);
			this.groupBox5.Location = new System.Drawing.Point(580, 3);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox5.Size = new System.Drawing.Size(191, 93);
			this.groupBox5.TabIndex = 10;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Enabled assemblers/smelters:";
			// 
			// AssemblerSelectionBox
			// 
			this.AssemblerSelectionBox.CheckOnClick = true;
			this.AssemblerSelectionBox.FormattingEnabled = true;
			this.AssemblerSelectionBox.Location = new System.Drawing.Point(6, 16);
			this.AssemblerSelectionBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.AssemblerSelectionBox.Name = "AssemblerSelectionBox";
			this.AssemblerSelectionBox.Size = new System.Drawing.Size(179, 64);
			this.AssemblerSelectionBox.TabIndex = 4;
			this.AssemblerSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.AssemblerSelectionBox_ItemCheck);
			// 
			// groupBox6
			// 
			this.groupBox6.AutoSize = true;
			this.groupBox6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox6.Controls.Add(this.MinerSelectionBox);
			this.groupBox6.Location = new System.Drawing.Point(777, 3);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox6.Size = new System.Drawing.Size(191, 93);
			this.groupBox6.TabIndex = 11;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Enabled miners/pumpjacks";
			// 
			// MinerSelectionBox
			// 
			this.MinerSelectionBox.CheckOnClick = true;
			this.MinerSelectionBox.FormattingEnabled = true;
			this.MinerSelectionBox.Location = new System.Drawing.Point(6, 16);
			this.MinerSelectionBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.MinerSelectionBox.Name = "MinerSelectionBox";
			this.MinerSelectionBox.Size = new System.Drawing.Size(179, 64);
			this.MinerSelectionBox.TabIndex = 5;
			this.MinerSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.MinerSelectionBox_ItemCheck);
			// 
			// groupBox7
			// 
			this.groupBox7.AutoSize = true;
			this.groupBox7.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox7.Controls.Add(this.ModuleSelectionBox);
			this.groupBox7.Location = new System.Drawing.Point(974, 3);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox7.Size = new System.Drawing.Size(191, 93);
			this.groupBox7.TabIndex = 12;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "Enabled modules";
			// 
			// ModuleSelectionBox
			// 
			this.ModuleSelectionBox.CheckOnClick = true;
			this.ModuleSelectionBox.FormattingEnabled = true;
			this.ModuleSelectionBox.Location = new System.Drawing.Point(6, 16);
			this.ModuleSelectionBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.ModuleSelectionBox.Name = "ModuleSelectionBox";
			this.ModuleSelectionBox.Size = new System.Drawing.Size(179, 64);
			this.ModuleSelectionBox.TabIndex = 6;
			this.ModuleSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ModuleSelectionBox_ItemCheck);
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
			this.tableLayoutPanel1.SetRowSpan(this.GraphViewer, 2);
			this.GraphViewer.Size = new System.Drawing.Size(943, 689);
			this.GraphViewer.TabIndex = 12;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1175, 800);
			this.Controls.Add(this.tableLayoutPanel1);
			this.DoubleBuffered = true;
			this.KeyPreview = true;
			this.Name = "MainForm";
			this.Text = "Foreman";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ItemListForm_KeyDown);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.MainToolbar.ResumeLayout(false);
			this.MainToolbar.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox5.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox7.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Button AddItemButton;
		private System.Windows.Forms.CheckedListBox MinerSelectionBox;
		private System.Windows.Forms.CheckedListBox ModuleSelectionBox;
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
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.CheckedListBox AssemblerSelectionBox;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.GroupBox groupBox7;
		private ProductionGraphViewer GraphViewer;
	}
}

