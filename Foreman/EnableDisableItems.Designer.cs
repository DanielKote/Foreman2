namespace Foreman
{
	partial class EnableDisableItemsForm
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
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.AssemblerSelectionBox = new System.Windows.Forms.CheckedListBox();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.MinerSelectionBox = new System.Windows.Forms.CheckedListBox();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.ModuleSelectionBox = new System.Windows.Forms.CheckedListBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.ModSelectionBox = new System.Windows.Forms.CheckedListBox();
			this.flowLayoutPanel1.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.groupBox7.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.groupBox5);
			this.flowLayoutPanel1.Controls.Add(this.groupBox6);
			this.flowLayoutPanel1.Controls.Add(this.groupBox7);
			this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(202, 682);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// groupBox5
			// 
			this.groupBox5.AutoSize = true;
			this.groupBox5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox5.Controls.Add(this.AssemblerSelectionBox);
			this.groupBox5.Location = new System.Drawing.Point(3, 3);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox5.Size = new System.Drawing.Size(195, 168);
			this.groupBox5.TabIndex = 12;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Assemblers/Smelters";
			// 
			// AssemblerSelectionBox
			// 
			this.AssemblerSelectionBox.CheckOnClick = true;
			this.AssemblerSelectionBox.FormattingEnabled = true;
			this.AssemblerSelectionBox.Location = new System.Drawing.Point(6, 16);
			this.AssemblerSelectionBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.AssemblerSelectionBox.Name = "AssemblerSelectionBox";
			this.AssemblerSelectionBox.Size = new System.Drawing.Size(183, 139);
			this.AssemblerSelectionBox.TabIndex = 4;
			this.AssemblerSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.AssemblerSelectionBox_ItemCheck);
			// 
			// groupBox6
			// 
			this.groupBox6.AutoSize = true;
			this.groupBox6.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox6.Controls.Add(this.MinerSelectionBox);
			this.groupBox6.Location = new System.Drawing.Point(3, 177);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox6.Size = new System.Drawing.Size(195, 168);
			this.groupBox6.TabIndex = 13;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Miners/Pumpjacks";
			// 
			// MinerSelectionBox
			// 
			this.MinerSelectionBox.CheckOnClick = true;
			this.MinerSelectionBox.FormattingEnabled = true;
			this.MinerSelectionBox.Location = new System.Drawing.Point(6, 16);
			this.MinerSelectionBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.MinerSelectionBox.Name = "MinerSelectionBox";
			this.MinerSelectionBox.Size = new System.Drawing.Size(183, 139);
			this.MinerSelectionBox.TabIndex = 5;
			this.MinerSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.MinerSelectionBox_ItemCheck);
			// 
			// groupBox7
			// 
			this.groupBox7.AutoSize = true;
			this.groupBox7.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox7.Controls.Add(this.ModuleSelectionBox);
			this.groupBox7.Location = new System.Drawing.Point(3, 351);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox7.Size = new System.Drawing.Size(195, 168);
			this.groupBox7.TabIndex = 14;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "Modules";
			// 
			// ModuleSelectionBox
			// 
			this.ModuleSelectionBox.CheckOnClick = true;
			this.ModuleSelectionBox.FormattingEnabled = true;
			this.ModuleSelectionBox.Location = new System.Drawing.Point(6, 16);
			this.ModuleSelectionBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.ModuleSelectionBox.Name = "ModuleSelectionBox";
			this.ModuleSelectionBox.Size = new System.Drawing.Size(183, 139);
			this.ModuleSelectionBox.TabIndex = 6;
			this.ModuleSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ModuleSelectionBox_ItemCheck);
			// 
			// groupBox1
			// 
			this.groupBox1.AutoSize = true;
			this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBox1.Controls.Add(this.ModSelectionBox);
			this.groupBox1.Location = new System.Drawing.Point(220, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.groupBox1.Size = new System.Drawing.Size(195, 528);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Mods";
			// 
			// ModSelectionBox
			// 
			this.ModSelectionBox.CheckOnClick = true;
			this.ModSelectionBox.FormattingEnabled = true;
			this.ModSelectionBox.Location = new System.Drawing.Point(6, 16);
			this.ModSelectionBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.ModSelectionBox.Name = "ModSelectionBox";
			this.ModSelectionBox.Size = new System.Drawing.Size(183, 499);
			this.ModSelectionBox.TabIndex = 4;
			this.ModSelectionBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ModSelectionBox_ItemCheck);
			// 
			// EnableDisableItemsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(430, 543);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.flowLayoutPanel1);
			this.MaximizeBox = false;
			this.Name = "EnableDisableItemsForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Enable/Disable Objects";
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.groupBox5.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox7.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.CheckedListBox AssemblerSelectionBox;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.CheckedListBox MinerSelectionBox;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.CheckedListBox ModuleSelectionBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckedListBox ModSelectionBox;

	}
}