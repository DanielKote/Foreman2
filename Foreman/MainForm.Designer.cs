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
			this.ProductionGraph = new Foreman.ProductionGraphViewer();
			this.AddItemButton = new System.Windows.Forms.Button();
			this.RemoveNodeButton = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rateOptionsDropDown = new System.Windows.Forms.ComboBox();
			this.rateButton = new System.Windows.Forms.RadioButton();
			this.fixedAmountButton = new System.Windows.Forms.RadioButton();
			this.tableLayoutPanel1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ItemListBox
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.ItemListBox, 2);
			this.ItemListBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ItemListBox.FormattingEnabled = true;
			this.ItemListBox.IntegralHeight = false;
			this.ItemListBox.Location = new System.Drawing.Point(3, 91);
			this.ItemListBox.Name = "ItemListBox";
			this.ItemListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.ItemListBox.Size = new System.Drawing.Size(194, 566);
			this.ItemListBox.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.ProductionGraph, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.AddItemButton, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.RemoveNodeButton, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.ItemListBox, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1029, 690);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// ProductionGraph
			// 
			this.ProductionGraph.AutoScroll = true;
			this.ProductionGraph.BackColor = System.Drawing.Color.White;
			this.ProductionGraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ProductionGraph.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductionGraph.Location = new System.Drawing.Point(203, 3);
			this.ProductionGraph.Name = "ProductionGraph";
			this.tableLayoutPanel1.SetRowSpan(this.ProductionGraph, 3);
			this.ProductionGraph.Size = new System.Drawing.Size(823, 684);
			this.ProductionGraph.TabIndex = 1;
			// 
			// AddItemButton
			// 
			this.AddItemButton.Location = new System.Drawing.Point(3, 663);
			this.AddItemButton.Name = "AddItemButton";
			this.AddItemButton.Size = new System.Drawing.Size(94, 24);
			this.AddItemButton.TabIndex = 2;
			this.AddItemButton.Text = "Add Output(s)";
			this.AddItemButton.UseVisualStyleBackColor = true;
			this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
			// 
			// RemoveNodeButton
			// 
			this.RemoveNodeButton.Location = new System.Drawing.Point(103, 663);
			this.RemoveNodeButton.Name = "RemoveNodeButton";
			this.RemoveNodeButton.Size = new System.Drawing.Size(94, 24);
			this.RemoveNodeButton.TabIndex = 3;
			this.RemoveNodeButton.Text = "Remove Node";
			this.RemoveNodeButton.UseVisualStyleBackColor = true;
			this.RemoveNodeButton.Click += new System.EventHandler(this.RemoveNodeButton_Click);
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
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1029, 690);
			this.Controls.Add(this.tableLayoutPanel1);
			this.DoubleBuffered = true;
			this.KeyPreview = true;
			this.Name = "MainForm";
			this.Text = "Items";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ItemListForm_KeyDown);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox ItemListBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private ProductionGraphViewer ProductionGraph;
		private System.Windows.Forms.Button AddItemButton;
		private System.Windows.Forms.Button RemoveNodeButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox rateOptionsDropDown;
		private System.Windows.Forms.RadioButton rateButton;
		private System.Windows.Forms.RadioButton fixedAmountButton;
	}
}

