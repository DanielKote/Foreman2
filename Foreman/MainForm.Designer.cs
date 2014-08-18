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
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ItemListBox
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.ItemListBox, 2);
			this.ItemListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemListBox.FormattingEnabled = true;
			this.ItemListBox.IntegralHeight = false;
			this.ItemListBox.Location = new System.Drawing.Point(3, 3);
			this.ItemListBox.Name = "ItemListBox";
			this.ItemListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.ItemListBox.Size = new System.Drawing.Size(194, 650);
			this.ItemListBox.TabIndex = 0;
			this.ItemListBox.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.ProductionGraph, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.ItemListBox, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.AddItemButton, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.RemoveNodeButton, 1, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
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
			this.tableLayoutPanel1.SetRowSpan(this.ProductionGraph, 2);
			this.ProductionGraph.Size = new System.Drawing.Size(823, 684);
			this.ProductionGraph.TabIndex = 1;
			// 
			// AddItemButton
			// 
			this.AddItemButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddItemButton.Location = new System.Drawing.Point(3, 659);
			this.AddItemButton.Name = "AddItemButton";
			this.AddItemButton.Size = new System.Drawing.Size(94, 28);
			this.AddItemButton.TabIndex = 2;
			this.AddItemButton.Text = "Add Output(s)";
			this.AddItemButton.UseVisualStyleBackColor = true;
			this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
			// 
			// RemoveNodeButton
			// 
			this.RemoveNodeButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RemoveNodeButton.Location = new System.Drawing.Point(103, 659);
			this.RemoveNodeButton.Name = "RemoveNodeButton";
			this.RemoveNodeButton.Size = new System.Drawing.Size(94, 28);
			this.RemoveNodeButton.TabIndex = 3;
			this.RemoveNodeButton.Text = "Remove Node";
			this.RemoveNodeButton.UseVisualStyleBackColor = true;
			this.RemoveNodeButton.Click += new System.EventHandler(this.RemoveNodeButton_Click);
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
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox ItemListBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private ProductionGraphViewer ProductionGraph;
		private System.Windows.Forms.Button AddItemButton;
		private System.Windows.Forms.Button RemoveNodeButton;
	}
}

