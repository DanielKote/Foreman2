
namespace Foreman
{
    partial class PresetSelectionForm
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
			this.PresetSelectionListView = new System.Windows.Forms.ListView();
			this.NameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ModCColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ItemsCColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.RecipesCColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.blank = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.CancellingButton = new System.Windows.Forms.Button();
			this.ConfirmationButton = new System.Windows.Forms.Button();
			this.PresetToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.TextTable = new System.Windows.Forms.TableLayoutPanel();
			this.TextTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// PresetSelectionListView
			// 
			this.PresetSelectionListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameColumn,
            this.ModCColumn,
            this.ItemsCColumn,
            this.RecipesCColumn,
            this.blank});
			this.TextTable.SetColumnSpan(this.PresetSelectionListView, 2);
			this.PresetSelectionListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PresetSelectionListView.FullRowSelect = true;
			this.PresetSelectionListView.GridLines = true;
			this.PresetSelectionListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.PresetSelectionListView.HideSelection = false;
			this.PresetSelectionListView.Location = new System.Drawing.Point(2, 36);
			this.PresetSelectionListView.Margin = new System.Windows.Forms.Padding(2, 10, 2, 2);
			this.PresetSelectionListView.MultiSelect = false;
			this.PresetSelectionListView.Name = "PresetSelectionListView";
			this.PresetSelectionListView.ShowItemToolTips = true;
			this.PresetSelectionListView.Size = new System.Drawing.Size(416, 188);
			this.PresetSelectionListView.TabIndex = 0;
			this.PresetSelectionListView.UseCompatibleStateImageBehavior = false;
			this.PresetSelectionListView.View = System.Windows.Forms.View.Details;
			this.PresetSelectionListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PresetSelectionListView_MouseDoubleClick);
			// 
			// NameColumn
			// 
			this.NameColumn.Text = "Preset";
			this.NameColumn.Width = 200;
			// 
			// ModCColumn
			// 
			this.ModCColumn.Text = "Mods (%)";
			this.ModCColumn.Width = 55;
			// 
			// ItemsCColumn
			// 
			this.ItemsCColumn.Text = "Items (%)";
			this.ItemsCColumn.Width = 54;
			// 
			// RecipesCColumn
			// 
			this.RecipesCColumn.Text = "Recipes (%)";
			this.RecipesCColumn.Width = 70;
			// 
			// blank
			// 
			this.blank.Text = "";
			this.blank.Width = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.TextTable.SetColumnSpan(this.label1, 2);
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(2, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(416, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "No preset was found to match the saved graph exactly.";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.TextTable.SetColumnSpan(this.label2, 2);
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(2, 13);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(416, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Please select which preset you wish to use based on the given compatibility ratin" +
    "gs.";
			// 
			// CancellingButton
			// 
			this.CancellingButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancellingButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CancellingButton.Location = new System.Drawing.Point(254, 228);
			this.CancellingButton.Margin = new System.Windows.Forms.Padding(2);
			this.CancellingButton.Name = "CancellingButton";
			this.CancellingButton.Size = new System.Drawing.Size(164, 26);
			this.CancellingButton.TabIndex = 3;
			this.CancellingButton.Text = "Dont Load";
			this.CancellingButton.UseVisualStyleBackColor = true;
			this.CancellingButton.Click += new System.EventHandler(this.CancellingButton_Click);
			// 
			// ConfirmationButton
			// 
			this.ConfirmationButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfirmationButton.Location = new System.Drawing.Point(2, 228);
			this.ConfirmationButton.Margin = new System.Windows.Forms.Padding(2);
			this.ConfirmationButton.Name = "ConfirmationButton";
			this.ConfirmationButton.Size = new System.Drawing.Size(248, 26);
			this.ConfirmationButton.TabIndex = 4;
			this.ConfirmationButton.Text = "Load with seleted preset";
			this.ConfirmationButton.UseVisualStyleBackColor = true;
			this.ConfirmationButton.Click += new System.EventHandler(this.ConfirmationButton_Click);
			// 
			// PresetToolTip
			// 
			this.PresetToolTip.AutoPopDelay = 20000;
			this.PresetToolTip.InitialDelay = 500;
			this.PresetToolTip.ReshowDelay = 100;
			// 
			// TextTable
			// 
			this.TextTable.AutoSize = true;
			this.TextTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TextTable.ColumnCount = 2;
			this.TextTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.TextTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.TextTable.Controls.Add(this.label1, 0, 0);
			this.TextTable.Controls.Add(this.ConfirmationButton, 0, 3);
			this.TextTable.Controls.Add(this.label2, 0, 1);
			this.TextTable.Controls.Add(this.CancellingButton, 1, 3);
			this.TextTable.Controls.Add(this.PresetSelectionListView, 0, 2);
			this.TextTable.Location = new System.Drawing.Point(3, 9);
			this.TextTable.Name = "TextTable";
			this.TextTable.RowCount = 4;
			this.TextTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TextTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TextTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
			this.TextTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TextTable.Size = new System.Drawing.Size(420, 256);
			this.TextTable.TabIndex = 5;
			// 
			// PresetSelectionForm
			// 
			this.AcceptButton = this.ConfirmationButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CancelButton = this.CancellingButton;
			this.ClientSize = new System.Drawing.Size(430, 270);
			this.Controls.Add(this.TextTable);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "PresetSelectionForm";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Please select Preset";
			this.TextTable.ResumeLayout(false);
			this.TextTable.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView PresetSelectionListView;
        private System.Windows.Forms.ColumnHeader NameColumn;
        private System.Windows.Forms.ColumnHeader ModCColumn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader ItemsCColumn;
        private System.Windows.Forms.ColumnHeader RecipesCColumn;
        private System.Windows.Forms.Button CancellingButton;
        private System.Windows.Forms.Button ConfirmationButton;
        private System.Windows.Forms.ToolTip PresetToolTip;
		private System.Windows.Forms.ColumnHeader blank;
		private System.Windows.Forms.TableLayoutPanel TextTable;
	}
}