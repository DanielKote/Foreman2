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
			this.ItemListBox = new System.Windows.Forms.ListBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.AddItemButton = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rateOptionsDropDown = new System.Windows.Forms.ComboBox();
			this.rateButton = new System.Windows.Forms.RadioButton();
			this.fixedAmountButton = new System.Windows.Forms.RadioButton();
			this.AutomaticCompleteButton = new System.Windows.Forms.Button();
			this.ClearButton = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.SingleAssemblerPerRecipeCheckBox = new System.Windows.Forms.CheckBox();
			this.AssemblerDisplayCheckBox = new System.Windows.Forms.CheckBox();
			this.GraphViewer = new Foreman.ProductionGraphViewer();
			this.ExportImageButton = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// ItemListBox
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.ItemListBox, 2);
			this.ItemListBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ItemListBox.FormattingEnabled = true;
			this.ItemListBox.IntegralHeight = false;
			this.ItemListBox.Location = new System.Drawing.Point(3, 269);
			this.ItemListBox.Name = "ItemListBox";
			this.ItemListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.ItemListBox.Size = new System.Drawing.Size(194, 388);
			this.ItemListBox.TabIndex = 0;
			this.ItemListBox.SelectedIndexChanged += new System.EventHandler(this.ItemListBox_SelectedIndexChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.GraphViewer, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.AddItemButton, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.ItemListBox, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.AutomaticCompleteButton, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.ClearButton, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.ExportImageButton, 0, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 7;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1029, 690);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// AddItemButton
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.AddItemButton, 2);
			this.AddItemButton.Location = new System.Drawing.Point(3, 663);
			this.AddItemButton.Name = "AddItemButton";
			this.AddItemButton.Size = new System.Drawing.Size(194, 24);
			this.AddItemButton.TabIndex = 2;
			this.AddItemButton.Text = "Add Output";
			this.AddItemButton.UseVisualStyleBackColor = true;
			this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.AutoSize = true;
			this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
			this.groupBox1.Controls.Add(this.rateOptionsDropDown);
			this.groupBox1.Controls.Add(this.rateButton);
			this.groupBox1.Controls.Add(this.fixedAmountButton);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(3, 3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(194, 82);
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
			this.rateOptionsDropDown.Size = new System.Drawing.Size(119, 21);
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
			// AutomaticCompleteButton
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.AutomaticCompleteButton, 2);
			this.AutomaticCompleteButton.Location = new System.Drawing.Point(3, 91);
			this.AutomaticCompleteButton.Name = "AutomaticCompleteButton";
			this.AutomaticCompleteButton.Size = new System.Drawing.Size(194, 25);
			this.AutomaticCompleteButton.TabIndex = 5;
			this.AutomaticCompleteButton.Text = "Automatically complete flowchart";
			this.AutomaticCompleteButton.UseVisualStyleBackColor = true;
			this.AutomaticCompleteButton.Click += new System.EventHandler(this.AutomaticCompleteButton_Click);
			// 
			// ClearButton
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.ClearButton, 2);
			this.ClearButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.ClearButton.Location = new System.Drawing.Point(3, 122);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = new System.Drawing.Size(194, 25);
			this.ClearButton.TabIndex = 6;
			this.ClearButton.Text = "Clear flowchart";
			this.ClearButton.UseVisualStyleBackColor = true;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.AutoSize = true;
			this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.SetColumnSpan(this.groupBox2, 2);
			this.groupBox2.Controls.Add(this.SingleAssemblerPerRecipeCheckBox);
			this.groupBox2.Controls.Add(this.AssemblerDisplayCheckBox);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox2.Location = new System.Drawing.Point(3, 153);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(194, 79);
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
			// GraphViewer
			// 
			this.GraphViewer.AutoScroll = true;
			this.GraphViewer.BackColor = System.Drawing.Color.White;
			this.GraphViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.GraphViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GraphViewer.DraggedElement = null;
			this.GraphViewer.Location = new System.Drawing.Point(203, 3);
			this.GraphViewer.Name = "GraphViewer";
			this.tableLayoutPanel1.SetRowSpan(this.GraphViewer, 7);
			this.GraphViewer.Size = new System.Drawing.Size(823, 684);
			this.GraphViewer.TabIndex = 1;
			// 
			// ExportImageButton
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.ExportImageButton, 2);
			this.ExportImageButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportImageButton.Location = new System.Drawing.Point(3, 238);
			this.ExportImageButton.Name = "ExportImageButton";
			this.ExportImageButton.Size = new System.Drawing.Size(194, 25);
			this.ExportImageButton.TabIndex = 8;
			this.ExportImageButton.Text = "Export as Image";
			this.ExportImageButton.UseVisualStyleBackColor = true;
			this.ExportImageButton.Click += new System.EventHandler(this.ExportImageButton_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1029, 690);
			this.Controls.Add(this.tableLayoutPanel1);
			this.DoubleBuffered = true;
			this.KeyPreview = true;
			this.Name = "MainForm";
			this.Text = "Foreman";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ItemListForm_KeyDown);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox ItemListBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private ProductionGraphViewer GraphViewer;
		private System.Windows.Forms.Button AddItemButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton fixedAmountButton;
		private System.Windows.Forms.Button AutomaticCompleteButton;
		private System.Windows.Forms.Button ClearButton;
		private System.Windows.Forms.ComboBox rateOptionsDropDown;
		private System.Windows.Forms.RadioButton rateButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox AssemblerDisplayCheckBox;
		private System.Windows.Forms.CheckBox SingleAssemblerPerRecipeCheckBox;
		private System.Windows.Forms.Button ExportImageButton;
	}
}

