namespace Foreman
{
    partial class EditFlowPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.RateOptionsTable = new System.Windows.Forms.TableLayoutPanel();
			this.KeyNodeCheckBox = new System.Windows.Forms.CheckBox();
			this.SimplePassthroughNodesCheckBox = new System.Windows.Forms.CheckBox();
			this.FixedOption = new System.Windows.Forms.RadioButton();
			this.AutoOption = new System.Windows.Forms.RadioButton();
			this.RateLabel = new System.Windows.Forms.Label();
			this.FixedFlowInput = new System.Windows.Forms.NumericUpDown();
			this.KeyNodeTitleInput = new System.Windows.Forms.TextBox();
			this.KeyNodeTitleLabel = new System.Windows.Forms.Label();
			this.RateOptionsTable.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FixedFlowInput)).BeginInit();
			this.SuspendLayout();
			// 
			// RateOptionsTable
			// 
			this.RateOptionsTable.AutoSize = true;
			this.RateOptionsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.RateOptionsTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.RateOptionsTable.ColumnCount = 4;
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.RateOptionsTable.Controls.Add(this.KeyNodeCheckBox, 0, 2);
			this.RateOptionsTable.Controls.Add(this.SimplePassthroughNodesCheckBox, 0, 3);
			this.RateOptionsTable.Controls.Add(this.FixedOption, 1, 1);
			this.RateOptionsTable.Controls.Add(this.AutoOption, 0, 1);
			this.RateOptionsTable.Controls.Add(this.RateLabel, 0, 0);
			this.RateOptionsTable.Controls.Add(this.FixedFlowInput, 3, 1);
			this.RateOptionsTable.Controls.Add(this.KeyNodeTitleInput, 3, 2);
			this.RateOptionsTable.Controls.Add(this.KeyNodeTitleLabel, 2, 2);
			this.RateOptionsTable.Location = new System.Drawing.Point(3, 3);
			this.RateOptionsTable.Name = "RateOptionsTable";
			this.RateOptionsTable.RowCount = 4;
			this.RateOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.RateOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.RateOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.RateOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.RateOptionsTable.Size = new System.Drawing.Size(249, 97);
			this.RateOptionsTable.TabIndex = 19;
			// 
			// KeyNodeCheckBox
			// 
			this.KeyNodeCheckBox.AutoSize = true;
			this.RateOptionsTable.SetColumnSpan(this.KeyNodeCheckBox, 2);
			this.KeyNodeCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.KeyNodeCheckBox.Location = new System.Drawing.Point(3, 52);
			this.KeyNodeCheckBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
			this.KeyNodeCheckBox.Name = "KeyNodeCheckBox";
			this.KeyNodeCheckBox.Size = new System.Drawing.Size(73, 17);
			this.KeyNodeCheckBox.TabIndex = 21;
			this.KeyNodeCheckBox.Text = "Key Node";
			this.KeyNodeCheckBox.UseVisualStyleBackColor = true;
			// 
			// SimplePassthroughNodesCheckBox
			// 
			this.SimplePassthroughNodesCheckBox.AutoSize = true;
			this.RateOptionsTable.SetColumnSpan(this.SimplePassthroughNodesCheckBox, 4);
			this.SimplePassthroughNodesCheckBox.Location = new System.Drawing.Point(3, 77);
			this.SimplePassthroughNodesCheckBox.Name = "SimplePassthroughNodesCheckBox";
			this.SimplePassthroughNodesCheckBox.Size = new System.Drawing.Size(142, 17);
			this.SimplePassthroughNodesCheckBox.TabIndex = 20;
			this.SimplePassthroughNodesCheckBox.Text = "Simplify throughput node";
			this.SimplePassthroughNodesCheckBox.UseVisualStyleBackColor = true;
			this.SimplePassthroughNodesCheckBox.Visible = false;
			// 
			// FixedOption
			// 
			this.FixedOption.AutoSize = true;
			this.RateOptionsTable.SetColumnSpan(this.FixedOption, 2);
			this.FixedOption.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FixedOption.Location = new System.Drawing.Point(56, 24);
			this.FixedOption.Name = "FixedOption";
			this.FixedOption.Size = new System.Drawing.Size(70, 20);
			this.FixedOption.TabIndex = 1;
			this.FixedOption.Text = "Fixed";
			this.FixedOption.UseVisualStyleBackColor = true;
			this.FixedOption.CheckedChanged += new System.EventHandler(this.FixedOption_CheckChanged);
			// 
			// AutoOption
			// 
			this.AutoOption.AutoSize = true;
			this.AutoOption.Checked = true;
			this.AutoOption.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoOption.Location = new System.Drawing.Point(3, 24);
			this.AutoOption.Name = "AutoOption";
			this.AutoOption.Size = new System.Drawing.Size(47, 20);
			this.AutoOption.TabIndex = 0;
			this.AutoOption.TabStop = true;
			this.AutoOption.Text = "Auto";
			this.AutoOption.UseVisualStyleBackColor = true;
			// 
			// RateLabel
			// 
			this.RateLabel.AutoSize = true;
			this.RateOptionsTable.SetColumnSpan(this.RateLabel, 4);
			this.RateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
			this.RateLabel.Location = new System.Drawing.Point(3, 1);
			this.RateLabel.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
			this.RateLabel.Name = "RateLabel";
			this.RateLabel.Size = new System.Drawing.Size(243, 17);
			this.RateLabel.TabIndex = 3;
			this.RateLabel.Text = "Item Flowrate (per 1 hour):";
			this.RateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FixedFlowInput
			// 
			this.FixedFlowInput.DecimalPlaces = 4;
			this.FixedFlowInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FixedFlowInput.Location = new System.Drawing.Point(132, 24);
			this.FixedFlowInput.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
			this.FixedFlowInput.Name = "FixedFlowInput";
			this.FixedFlowInput.Size = new System.Drawing.Size(114, 20);
			this.FixedFlowInput.TabIndex = 4;
			this.FixedFlowInput.ThousandsSeparator = true;
			this.FixedFlowInput.ValueChanged += new System.EventHandler(this.FixedFlowInput_ValueChanged);
			// 
			// KeyNodeTitleInput
			// 
			this.KeyNodeTitleInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.KeyNodeTitleInput.Location = new System.Drawing.Point(132, 50);
			this.KeyNodeTitleInput.MaxLength = 200;
			this.KeyNodeTitleInput.Name = "KeyNodeTitleInput";
			this.KeyNodeTitleInput.Size = new System.Drawing.Size(114, 20);
			this.KeyNodeTitleInput.TabIndex = 22;
			// 
			// KeyNodeTitleLabel
			// 
			this.KeyNodeTitleLabel.AutoSize = true;
			this.KeyNodeTitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.KeyNodeTitleLabel.Location = new System.Drawing.Point(82, 47);
			this.KeyNodeTitleLabel.Name = "KeyNodeTitleLabel";
			this.KeyNodeTitleLabel.Size = new System.Drawing.Size(44, 27);
			this.KeyNodeTitleLabel.TabIndex = 25;
			this.KeyNodeTitleLabel.Text = "Title:";
			this.KeyNodeTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// EditFlowPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Black;
			this.Controls.Add(this.RateOptionsTable);
			this.DoubleBuffered = true;
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "EditFlowPanel";
			this.Size = new System.Drawing.Size(255, 103);
			this.RateOptionsTable.ResumeLayout(false);
			this.RateOptionsTable.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FixedFlowInput)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		#endregion

		private System.Windows.Forms.TableLayoutPanel RateOptionsTable;
		public System.Windows.Forms.RadioButton FixedOption;
		public System.Windows.Forms.RadioButton AutoOption;
		private System.Windows.Forms.Label RateLabel;
		private System.Windows.Forms.NumericUpDown FixedFlowInput;
		private System.Windows.Forms.CheckBox SimplePassthroughNodesCheckBox;
		private System.Windows.Forms.CheckBox KeyNodeCheckBox;
		private System.Windows.Forms.TextBox KeyNodeTitleInput;
		private System.Windows.Forms.Label KeyNodeTitleLabel;
	}
}
