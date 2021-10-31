
namespace Foreman
{
	partial class SciencePacksLoadForm
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
			this.MainTable = new System.Windows.Forms.TableLayoutPanel();
			this.CancellationButton = new System.Windows.Forms.Button();
			this.ConfirmationButton = new System.Windows.Forms.Button();
			this.SciencePackTable = new System.Windows.Forms.TableLayoutPanel();
			this.ToolTip = new Foreman.CustomToolTip();
			this.MainTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTable
			// 
			this.MainTable.AutoSize = true;
			this.MainTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTable.ColumnCount = 2;
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.MainTable.Controls.Add(this.CancellationButton, 1, 1);
			this.MainTable.Controls.Add(this.ConfirmationButton, 0, 1);
			this.MainTable.Controls.Add(this.SciencePackTable, 0, 0);
			this.MainTable.Location = new System.Drawing.Point(3, 3);
			this.MainTable.Name = "MainTable";
			this.MainTable.RowCount = 2;
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.Size = new System.Drawing.Size(286, 35);
			this.MainTable.TabIndex = 0;
			// 
			// CancellationButton
			// 
			this.CancellationButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancellationButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CancellationButton.Location = new System.Drawing.Point(189, 9);
			this.CancellationButton.Name = "CancellationButton";
			this.CancellationButton.Size = new System.Drawing.Size(94, 23);
			this.CancellationButton.TabIndex = 0;
			this.CancellationButton.Text = "Cancel";
			this.CancellationButton.UseVisualStyleBackColor = true;
			// 
			// ConfirmationButton
			// 
			this.ConfirmationButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfirmationButton.Location = new System.Drawing.Point(3, 9);
			this.ConfirmationButton.Name = "ConfirmationButton";
			this.ConfirmationButton.Size = new System.Drawing.Size(180, 23);
			this.ConfirmationButton.TabIndex = 1;
			this.ConfirmationButton.Text = "Confirm";
			this.ConfirmationButton.UseVisualStyleBackColor = true;
			this.ConfirmationButton.Click += new System.EventHandler(this.ConfirmationButton_Click);
			// 
			// SciencePackTable
			// 
			this.SciencePackTable.AutoSize = true;
			this.SciencePackTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.SciencePackTable.ColumnCount = 1;
			this.MainTable.SetColumnSpan(this.SciencePackTable, 2);
			this.SciencePackTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.SciencePackTable.Location = new System.Drawing.Point(3, 3);
			this.SciencePackTable.Name = "SciencePackTable";
			this.SciencePackTable.RowCount = 1;
			this.SciencePackTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.SciencePackTable.Size = new System.Drawing.Size(0, 0);
			this.SciencePackTable.TabIndex = 2;
			// 
			// ToolTip
			// 
			this.ToolTip.AutoPopDelay = 100000;
			this.ToolTip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(65)))), ((int)(((byte)(65)))));
			this.ToolTip.ForeColor = System.Drawing.Color.White;
			this.ToolTip.InitialDelay = 200;
			this.ToolTip.OwnerDraw = true;
			this.ToolTip.ReshowDelay = 100;
			this.ToolTip.TextFont = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
			// 
			// SciencePacksLoadForm
			// 
			this.AcceptButton = this.ConfirmationButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.CancellationButton;
			this.ClientSize = new System.Drawing.Size(304, 55);
			this.Controls.Add(this.MainTable);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SciencePacksLoadForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Select researched science packs";
			this.MainTable.ResumeLayout(false);
			this.MainTable.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel MainTable;
		private System.Windows.Forms.Button CancellationButton;
		private System.Windows.Forms.Button ConfirmationButton;
		private System.Windows.Forms.TableLayoutPanel SciencePackTable;
		private CustomToolTip ToolTip;
	}
}