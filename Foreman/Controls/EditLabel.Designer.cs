namespace Foreman
{
    partial class EditLabel
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.FontSize = new System.Windows.Forms.NumericUpDown();
            this.txtLabelText = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FontSize)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.66667F));
            this.tableLayoutPanel1.Controls.Add(this.FontSize, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtLabelText, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(450, 26);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // FontSize
            // 
            this.FontSize.Cursor = System.Windows.Forms.Cursors.Default;
            this.FontSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FontSize.Location = new System.Drawing.Point(333, 3);
            this.FontSize.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.FontSize.Name = "FontSize";
            this.FontSize.Size = new System.Drawing.Size(114, 20);
            this.FontSize.TabIndex = 5;
            this.FontSize.ThousandsSeparator = true;
            this.FontSize.ValueChanged += new System.EventHandler(this.FontSize_ValueChanged);
            // 
            // txtLabelText
            // 
            this.txtLabelText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLabelText.Location = new System.Drawing.Point(3, 3);
            this.txtLabelText.Name = "txtLabelText";
            this.txtLabelText.Size = new System.Drawing.Size(324, 20);
            this.txtLabelText.TabIndex = 0;
            this.txtLabelText.TextChanged += new System.EventHandler(this.txtLabelText_TextChanged);
            // 
            // EditLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "EditLabel";
            this.Size = new System.Drawing.Size(450, 26);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FontSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox txtLabelText;
        private System.Windows.Forms.NumericUpDown FontSize;
    }
}
