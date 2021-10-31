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
			this.FixedItemFlowInput = new System.Windows.Forms.TextBox();
			this.FixedOption = new System.Windows.Forms.RadioButton();
			this.AutoOption = new System.Windows.Forms.RadioButton();
			this.RateLabel = new System.Windows.Forms.Label();
			this.RateOptionsTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// RateOptionsTable
			// 
			this.RateOptionsTable.AutoSize = true;
			this.RateOptionsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.RateOptionsTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
			this.RateOptionsTable.ColumnCount = 3;
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.RateOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.RateOptionsTable.Controls.Add(this.FixedItemFlowInput, 2, 1);
			this.RateOptionsTable.Controls.Add(this.FixedOption, 1, 1);
			this.RateOptionsTable.Controls.Add(this.AutoOption, 0, 1);
			this.RateOptionsTable.Controls.Add(this.RateLabel, 0, 0);
			this.RateOptionsTable.Location = new System.Drawing.Point(3, 3);
			this.RateOptionsTable.Name = "RateOptionsTable";
			this.RateOptionsTable.RowCount = 2;
			this.RateOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.RateOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.RateOptionsTable.Size = new System.Drawing.Size(215, 47);
			this.RateOptionsTable.TabIndex = 19;
			// 
			// FixedItemFlowInput
			// 
			this.FixedItemFlowInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FixedItemFlowInput.Location = new System.Drawing.Point(112, 24);
			this.FixedItemFlowInput.Name = "FixedItemFlowInput";
			this.FixedItemFlowInput.Size = new System.Drawing.Size(100, 20);
			this.FixedItemFlowInput.TabIndex = 2;
			this.FixedItemFlowInput.TextChanged += new System.EventHandler(this.FixedItemFlowInput_TextChanged);
			this.FixedItemFlowInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.FixedItemFlowInput.Leave += new System.EventHandler(this.FixedItemFlowInput_LostFocus);
			// 
			// FixedOption
			// 
			this.FixedOption.AutoSize = true;
			this.FixedOption.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FixedOption.Location = new System.Drawing.Point(56, 24);
			this.FixedOption.Name = "FixedOption";
			this.FixedOption.Size = new System.Drawing.Size(50, 20);
			this.FixedOption.TabIndex = 1;
			this.FixedOption.Text = "Fixed";
			this.FixedOption.UseVisualStyleBackColor = true;
			this.FixedOption.CheckedChanged += new System.EventHandler(this.FixedOption_CheckChanged);
			this.FixedOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
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
			this.AutoOption.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			// 
			// RateLabel
			// 
			this.RateLabel.AutoSize = true;
			this.RateOptionsTable.SetColumnSpan(this.RateLabel, 3);
			this.RateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
			this.RateLabel.Location = new System.Drawing.Point(3, 1);
			this.RateLabel.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
			this.RateLabel.Name = "RateLabel";
			this.RateLabel.Size = new System.Drawing.Size(209, 17);
			this.RateLabel.TabIndex = 3;
			this.RateLabel.Text = "Item Flowrate (per 1 hour):";
			this.RateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			this.Size = new System.Drawing.Size(221, 53);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyPressed);
			this.RateOptionsTable.ResumeLayout(false);
			this.RateOptionsTable.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		#endregion

		private System.Windows.Forms.TableLayoutPanel RateOptionsTable;
		public System.Windows.Forms.TextBox FixedItemFlowInput;
		public System.Windows.Forms.RadioButton FixedOption;
		public System.Windows.Forms.RadioButton AutoOption;
		private System.Windows.Forms.Label RateLabel;
	}
}
