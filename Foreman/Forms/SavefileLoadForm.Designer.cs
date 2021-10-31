namespace Foreman
{
    partial class SaveFileLoadForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.CancellationButton = new System.Windows.Forms.Button();
			this.MainTable = new System.Windows.Forms.TableLayoutPanel();
			this.MainTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.label1.Location = new System.Drawing.Point(2, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(203, 27);
			this.label1.TabIndex = 0;
			this.label1.Text = "Please wait... Running Factorio";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// CancellationButton
			// 
			this.CancellationButton.AutoSize = true;
			this.CancellationButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancellationButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancellationButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CancellationButton.Location = new System.Drawing.Point(209, 2);
			this.CancellationButton.Margin = new System.Windows.Forms.Padding(2);
			this.CancellationButton.Name = "CancellationButton";
			this.CancellationButton.Size = new System.Drawing.Size(50, 23);
			this.CancellationButton.TabIndex = 1;
			this.CancellationButton.Text = "Cancel";
			this.CancellationButton.UseVisualStyleBackColor = true;
			this.CancellationButton.Click += new System.EventHandler(this.CancellationButton_Click);
			// 
			// MainTable
			// 
			this.MainTable.AutoSize = true;
			this.MainTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTable.ColumnCount = 2;
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MainTable.Controls.Add(this.label1, 0, 0);
			this.MainTable.Controls.Add(this.CancellationButton, 1, 0);
			this.MainTable.Location = new System.Drawing.Point(3, 3);
			this.MainTable.Name = "MainTable";
			this.MainTable.RowCount = 1;
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.Size = new System.Drawing.Size(261, 27);
			this.MainTable.TabIndex = 2;
			// 
			// SaveFileLoadForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.CancellationButton;
			this.ClientSize = new System.Drawing.Size(281, 41);
			this.ControlBox = false;
			this.Controls.Add(this.MainTable);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SaveFileLoadForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Loading Factorio Save";
			this.Load += new System.EventHandler(this.ProgressForm_Load);
			this.MainTable.ResumeLayout(false);
			this.MainTable.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button CancellationButton;
		private System.Windows.Forms.TableLayoutPanel MainTable;
	}
}