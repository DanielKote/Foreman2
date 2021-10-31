
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
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CancellingButton = new System.Windows.Forms.Button();
            this.ConfirmationButton = new System.Windows.Forms.Button();
            this.PresetToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // PresetSelectionListView
            // 
            this.PresetSelectionListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PresetSelectionListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameColumn,
            this.ModCColumn,
            this.ItemsCColumn,
            this.columnHeader1});
            this.PresetSelectionListView.FullRowSelect = true;
            this.PresetSelectionListView.GridLines = true;
            this.PresetSelectionListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.PresetSelectionListView.HideSelection = false;
            this.PresetSelectionListView.Location = new System.Drawing.Point(12, 59);
            this.PresetSelectionListView.MultiSelect = false;
            this.PresetSelectionListView.Name = "PresetSelectionListView";
            this.PresetSelectionListView.ShowItemToolTips = true;
            this.PresetSelectionListView.Size = new System.Drawing.Size(535, 221);
            this.PresetSelectionListView.TabIndex = 0;
            this.PresetSelectionListView.UseCompatibleStateImageBehavior = false;
            this.PresetSelectionListView.View = System.Windows.Forms.View.Details;
            this.PresetSelectionListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PresetSelectionListView_MouseDoubleClick);
            // 
            // NameColumn
            // 
            this.NameColumn.Text = "Preset";
            this.NameColumn.Width = 240;
            // 
            // ModCColumn
            // 
            this.ModCColumn.Text = "Mods (%)";
            this.ModCColumn.Width = 90;
            // 
            // ItemsCColumn
            // 
            this.ItemsCColumn.Text = "Items (%)";
            this.ItemsCColumn.Width = 90;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Recipes (%)";
            this.columnHeader1.Width = 90;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(354, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "No preset was found to match the saved graph exactly.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(533, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Please select which preset you wish to use based on the given compatibility ratin" +
    "gs.";
            // 
            // CancellingButton
            // 
            this.CancellingButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancellingButton.Location = new System.Drawing.Point(346, 286);
            this.CancellingButton.Name = "CancellingButton";
            this.CancellingButton.Size = new System.Drawing.Size(201, 32);
            this.CancellingButton.TabIndex = 3;
            this.CancellingButton.Text = "Dont Load";
            this.CancellingButton.UseVisualStyleBackColor = true;
            this.CancellingButton.Click += new System.EventHandler(this.CancellingButton_Click);
            // 
            // ConfirmationButton
            // 
            this.ConfirmationButton.Location = new System.Drawing.Point(12, 286);
            this.ConfirmationButton.Name = "ConfirmationButton";
            this.ConfirmationButton.Size = new System.Drawing.Size(328, 32);
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
            // PresetSelectionForm
            // 
            this.AcceptButton = this.ConfirmationButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancellingButton;
            this.ClientSize = new System.Drawing.Size(559, 329);
            this.Controls.Add(this.ConfirmationButton);
            this.Controls.Add(this.CancellingButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PresetSelectionListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PresetSelectionForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Please select Preset";
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
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button CancellingButton;
        private System.Windows.Forms.Button ConfirmationButton;
        private System.Windows.Forms.ToolTip PresetToolTip;
    }
}