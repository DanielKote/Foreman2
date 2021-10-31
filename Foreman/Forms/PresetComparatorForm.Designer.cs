
namespace Foreman
{
    partial class PresetComparatorForm
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
            this.PresetSelectionGroup = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.RightPresetSelectionBox = new System.Windows.Forms.ComboBox();
            this.LeftPresetSelectionBox = new System.Windows.Forms.ComboBox();
            this.ProcessPresetsButton = new System.Windows.Forms.Button();
            this.ComparisonTabControl = new System.Windows.Forms.TabControl();
            this.ModTabPage = new System.Windows.Forms.TabPage();
            this.ItemTabPage = new System.Windows.Forms.TabPage();
            this.RecipeTabPage = new System.Windows.Forms.TabPage();
            this.AssemblerTabPage = new System.Windows.Forms.TabPage();
            this.MinerTabPage = new System.Windows.Forms.TabPage();
            this.ModuleTabPage = new System.Windows.Forms.TabPage();
            this.CloseButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.RightOnlyListView = new FFListView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.LeftOnlyListView = new FFListView();
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.IconList = new System.Windows.Forms.ImageList(this.components);
            this.RightOnlyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LeftOnlyHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RightListView = new Foreman.SyncListView();
            this.LeftListView = new Foreman.SyncListView();
            this.LeftHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RightHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PresetSelectionGroup.SuspendLayout();
            this.ComparisonTabControl.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // PresetSelectionGroup
            // 
            this.PresetSelectionGroup.Controls.Add(this.label1);
            this.PresetSelectionGroup.Controls.Add(this.RightPresetSelectionBox);
            this.PresetSelectionGroup.Controls.Add(this.LeftPresetSelectionBox);
            this.PresetSelectionGroup.Location = new System.Drawing.Point(12, 12);
            this.PresetSelectionGroup.Name = "PresetSelectionGroup";
            this.PresetSelectionGroup.Size = new System.Drawing.Size(501, 62);
            this.PresetSelectionGroup.TabIndex = 0;
            this.PresetSelectionGroup.TabStop = false;
            this.PresetSelectionGroup.Text = "Preset Selection";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(212, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "<-- vs -->";
            // 
            // RightPresetSelectionBox
            // 
            this.RightPresetSelectionBox.DisplayMember = "Name";
            this.RightPresetSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RightPresetSelectionBox.FormattingEnabled = true;
            this.RightPresetSelectionBox.Location = new System.Drawing.Point(294, 21);
            this.RightPresetSelectionBox.Name = "RightPresetSelectionBox";
            this.RightPresetSelectionBox.Size = new System.Drawing.Size(200, 24);
            this.RightPresetSelectionBox.TabIndex = 1;
            this.RightPresetSelectionBox.SelectedValueChanged += new System.EventHandler(this.PresetSelectionBox_SelectedValueChanged);
            // 
            // LeftPresetSelectionBox
            // 
            this.LeftPresetSelectionBox.DisplayMember = "Name";
            this.LeftPresetSelectionBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LeftPresetSelectionBox.FormattingEnabled = true;
            this.LeftPresetSelectionBox.Location = new System.Drawing.Point(6, 21);
            this.LeftPresetSelectionBox.Name = "LeftPresetSelectionBox";
            this.LeftPresetSelectionBox.Size = new System.Drawing.Size(200, 24);
            this.LeftPresetSelectionBox.TabIndex = 0;
            this.LeftPresetSelectionBox.SelectedValueChanged += new System.EventHandler(this.PresetSelectionBox_SelectedValueChanged);
            // 
            // ProcessPresetsButton
            // 
            this.ProcessPresetsButton.Location = new System.Drawing.Point(519, 18);
            this.ProcessPresetsButton.Name = "ProcessPresetsButton";
            this.ProcessPresetsButton.Size = new System.Drawing.Size(247, 24);
            this.ProcessPresetsButton.TabIndex = 1;
            this.ProcessPresetsButton.Text = "Read Presets And Compare";
            this.ProcessPresetsButton.UseVisualStyleBackColor = true;
            this.ProcessPresetsButton.Click += new System.EventHandler(this.ProcessPresetsButton_Click);
            // 
            // ComparisonTabControl
            // 
            this.ComparisonTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ComparisonTabControl.Controls.Add(this.ModTabPage);
            this.ComparisonTabControl.Controls.Add(this.ItemTabPage);
            this.ComparisonTabControl.Controls.Add(this.RecipeTabPage);
            this.ComparisonTabControl.Controls.Add(this.AssemblerTabPage);
            this.ComparisonTabControl.Controls.Add(this.MinerTabPage);
            this.ComparisonTabControl.Controls.Add(this.ModuleTabPage);
            this.ComparisonTabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.ComparisonTabControl.Location = new System.Drawing.Point(4, 80);
            this.ComparisonTabControl.Name = "ComparisonTabControl";
            this.ComparisonTabControl.SelectedIndex = 0;
            this.ComparisonTabControl.Size = new System.Drawing.Size(775, 370);
            this.ComparisonTabControl.TabIndex = 2;
            this.ComparisonTabControl.SelectedIndexChanged += new System.EventHandler(this.ComparisonTabControl_SelectedIndexChanged);
            // 
            // ModTabPage
            // 
            this.ModTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.ModTabPage.Location = new System.Drawing.Point(4, 29);
            this.ModTabPage.Name = "ModTabPage";
            this.ModTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ModTabPage.Size = new System.Drawing.Size(767, 337);
            this.ModTabPage.TabIndex = 0;
            this.ModTabPage.Text = "Mods";
            // 
            // ItemTabPage
            // 
            this.ItemTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.ItemTabPage.Location = new System.Drawing.Point(4, 29);
            this.ItemTabPage.Name = "ItemTabPage";
            this.ItemTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.ItemTabPage.Size = new System.Drawing.Size(767, 337);
            this.ItemTabPage.TabIndex = 1;
            this.ItemTabPage.Text = "Items";
            // 
            // RecipeTabPage
            // 
            this.RecipeTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.RecipeTabPage.Location = new System.Drawing.Point(4, 29);
            this.RecipeTabPage.Name = "RecipeTabPage";
            this.RecipeTabPage.Size = new System.Drawing.Size(767, 337);
            this.RecipeTabPage.TabIndex = 2;
            this.RecipeTabPage.Text = "Recipes";
            // 
            // AssemblerTabPage
            // 
            this.AssemblerTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.AssemblerTabPage.Location = new System.Drawing.Point(4, 29);
            this.AssemblerTabPage.Name = "AssemblerTabPage";
            this.AssemblerTabPage.Size = new System.Drawing.Size(767, 337);
            this.AssemblerTabPage.TabIndex = 3;
            this.AssemblerTabPage.Text = "Assemblers";
            // 
            // MinerTabPage
            // 
            this.MinerTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.MinerTabPage.Location = new System.Drawing.Point(4, 29);
            this.MinerTabPage.Name = "MinerTabPage";
            this.MinerTabPage.Size = new System.Drawing.Size(767, 337);
            this.MinerTabPage.TabIndex = 4;
            this.MinerTabPage.Text = "Miners";
            // 
            // ModuleTabPage
            // 
            this.ModuleTabPage.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.ModuleTabPage.Location = new System.Drawing.Point(4, 29);
            this.ModuleTabPage.Name = "ModuleTabPage";
            this.ModuleTabPage.Size = new System.Drawing.Size(767, 337);
            this.ModuleTabPage.TabIndex = 5;
            this.ModuleTabPage.Text = "Modules";
            // 
            // CloseButton
            // 
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = new System.Drawing.Point(519, 50);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(247, 24);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel.Controls.Add(this.panel2, 2, 0);
            this.tableLayoutPanel.Controls.Add(this.RightOnlyListView, 3, 1);
            this.tableLayoutPanel.Controls.Add(this.RightListView, 2, 1);
            this.tableLayoutPanel.Controls.Add(this.LeftListView, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.panel1, 3, 0);
            this.tableLayoutPanel.Controls.Add(this.panel3, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.panel4, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.LeftOnlyListView, 0, 1);
            this.tableLayoutPanel.Location = new System.Drawing.Point(9, 114);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.Size = new System.Drawing.Size(760, 332);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(380, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(190, 24);
            this.panel2.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Right Preset";
            // 
            // RightOnlyListView
            // 
            this.RightOnlyListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.RightOnlyHeader});
            this.RightOnlyListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightOnlyListView.FullRowSelect = true;
            this.RightOnlyListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.RightOnlyListView.HideSelection = false;
            this.RightOnlyListView.Location = new System.Drawing.Point(590, 27);
            this.RightOnlyListView.Margin = new System.Windows.Forms.Padding(20, 3, 10, 3);
            this.RightOnlyListView.MultiSelect = false;
            this.RightOnlyListView.Name = "RightOnlyListView";
            this.RightOnlyListView.Size = new System.Drawing.Size(160, 302);
            this.RightOnlyListView.SmallImageList = this.IconList;
            this.RightOnlyListView.TabIndex = 7;
            this.RightOnlyListView.UseCompatibleStateImageBehavior = false;
            this.RightOnlyListView.View = System.Windows.Forms.View.Details;
            this.RightOnlyListView.VirtualMode = true;
            this.RightOnlyListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.RightOnlyListView_RetrieveVirtualItem);
            this.RightOnlyListView.Resize += new System.EventHandler(this.RightOnlyListView_Resize);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(570, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(190, 24);
            this.panel1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Right Preset Only";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(190, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(190, 24);
            this.panel3.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(33, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "Left Preset";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label5);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(190, 24);
            this.panel4.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 17);
            this.label5.TabIndex = 3;
            this.label5.Text = "Left Preset Only";
            // 
            // LeftOnlyListView
            // 
            this.LeftOnlyListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LeftOnlyHeader});
            this.LeftOnlyListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftOnlyListView.FullRowSelect = true;
            this.LeftOnlyListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.LeftOnlyListView.HideSelection = false;
            this.LeftOnlyListView.Location = new System.Drawing.Point(10, 27);
            this.LeftOnlyListView.Margin = new System.Windows.Forms.Padding(10, 3, 20, 3);
            this.LeftOnlyListView.MultiSelect = false;
            this.LeftOnlyListView.Name = "LeftOnlyListView";
            this.LeftOnlyListView.Size = new System.Drawing.Size(160, 302);
            this.LeftOnlyListView.SmallImageList = this.IconList;
            this.LeftOnlyListView.TabIndex = 4;
            this.LeftOnlyListView.UseCompatibleStateImageBehavior = false;
            this.LeftOnlyListView.View = System.Windows.Forms.View.Details;
            this.LeftOnlyListView.VirtualMode = true;
            this.LeftOnlyListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.LeftOnlyListView_RetrieveVirtualItem);
            this.LeftOnlyListView.Resize += new System.EventHandler(this.LeftOnlyListView_Resize);
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.Location = new System.Drawing.Point(519, 83);
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size(247, 22);
            this.FilterTextBox.TabIndex = 4;
            this.FilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(474, 85);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 17);
            this.label6.TabIndex = 5;
            this.label6.Text = "Filter:";
            // 
            // IconList
            // 
            this.IconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.IconList.ImageSize = new System.Drawing.Size(32, 32);
            this.IconList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // RightOnlyHeader
            // 
            this.RightOnlyHeader.Text = "";
            // 
            // LeftOnlyHeader
            // 
            this.LeftOnlyHeader.Text = "";
            // 
            // RightListView
            // 
            this.RightListView.Buddy = this.LeftListView;
            this.RightListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.RightHeader});
            this.RightListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightListView.FullRowSelect = true;
            this.RightListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.RightListView.HideSelection = false;
            this.RightListView.Location = new System.Drawing.Point(380, 27);
            this.RightListView.Margin = new System.Windows.Forms.Padding(0, 3, 30, 3);
            this.RightListView.MultiSelect = false;
            this.RightListView.Name = "RightListView";
            this.RightListView.Size = new System.Drawing.Size(160, 302);
            this.RightListView.SmallImageList = this.IconList;
            this.RightListView.TabIndex = 6;
            this.RightListView.UseCompatibleStateImageBehavior = false;
            this.RightListView.View = System.Windows.Forms.View.Details;
            this.RightListView.VirtualMode = true;
            this.RightListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.RightListView_RetrieveVirtualItem);
            this.RightListView.Resize += new System.EventHandler(this.RightListView_Resize);
            // 
            // LeftListView
            // 
            this.LeftListView.Buddy = this.RightListView;
            this.LeftListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.LeftHeader});
            this.LeftListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftListView.FullRowSelect = true;
            this.LeftListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.LeftListView.HideSelection = false;
            this.LeftListView.Location = new System.Drawing.Point(220, 27);
            this.LeftListView.Margin = new System.Windows.Forms.Padding(30, 3, 0, 3);
            this.LeftListView.MultiSelect = false;
            this.LeftListView.Name = "LeftListView";
            this.LeftListView.Size = new System.Drawing.Size(160, 302);
            this.LeftListView.SmallImageList = this.IconList;
            this.LeftListView.TabIndex = 5;
            this.LeftListView.UseCompatibleStateImageBehavior = false;
            this.LeftListView.View = System.Windows.Forms.View.Details;
            this.LeftListView.VirtualMode = true;
            this.LeftListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.LeftListView_RetrieveVirtualItem);
            this.LeftListView.Resize += new System.EventHandler(this.LeftListView_Resize);
            // 
            // LeftHeader
            // 
            this.LeftHeader.Text = "";
            // 
            // RightHeader
            // 
            this.RightHeader.Text = "";
            // 
            // PresetComparatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CloseButton;
            this.ClientSize = new System.Drawing.Size(782, 453);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.FilterTextBox);
            this.Controls.Add(this.tableLayoutPanel);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.ComparisonTabControl);
            this.Controls.Add(this.ProcessPresetsButton);
            this.Controls.Add(this.PresetSelectionGroup);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "PresetComparatorForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Preset Comparer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PresetComparatorForm_FormClosed);
            this.PresetSelectionGroup.ResumeLayout(false);
            this.PresetSelectionGroup.PerformLayout();
            this.ComparisonTabControl.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox PresetSelectionGroup;
        private System.Windows.Forms.ComboBox LeftPresetSelectionBox;
        private System.Windows.Forms.Button ProcessPresetsButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox RightPresetSelectionBox;
        private System.Windows.Forms.TabControl ComparisonTabControl;
        private System.Windows.Forms.TabPage ModTabPage;
        private System.Windows.Forms.TabPage ItemTabPage;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.TabPage RecipeTabPage;
        private System.Windows.Forms.TabPage AssemblerTabPage;
        private System.Windows.Forms.TabPage MinerTabPage;
        private System.Windows.Forms.TabPage ModuleTabPage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private FFListView RightOnlyListView;
        private SyncListView RightListView;
        private SyncListView LeftListView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private FFListView LeftOnlyListView;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ImageList IconList;
        private System.Windows.Forms.ColumnHeader RightOnlyHeader;
        private System.Windows.Forms.ColumnHeader LeftHeader;
        private System.Windows.Forms.ColumnHeader RightHeader;
        private System.Windows.Forms.ColumnHeader LeftOnlyHeader;
    }
}