
namespace Foreman
{
    partial class IRChooserPanel
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
            this.GroupFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.BaseGroupButton = new System.Windows.Forms.Button();
            this.IRFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.BaseIRUsedButton = new System.Windows.Forms.Button();
            this.BaseIREmptyButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ItemIconPanel = new System.Windows.Forms.Panel();
            this.IgnoreAssemblerCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.AsProductCheckBox = new System.Windows.Forms.CheckBox();
            this.AsIngredientCheckBox = new System.Windows.Forms.CheckBox();
            this.ShowHiddenCheckBox = new System.Windows.Forms.CheckBox();
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.IRPanelScrollBar = new System.Windows.Forms.VScrollBar();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.AddSupplyButton = new System.Windows.Forms.Button();
            this.AddPassthroughButton = new System.Windows.Forms.Button();
            this.AddConsumerButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.GroupFlowPanel.SuspendLayout();
            this.IRFlowPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Black;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 370F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel1.Controls.Add(this.GroupFlowPanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.IRFlowPanel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.IRPanelScrollBar, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(394, 500);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // GroupFlowPanel
            // 
            this.GroupFlowPanel.AutoSize = true;
            this.GroupFlowPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.GroupFlowPanel.BackColor = System.Drawing.Color.DimGray;
            this.tableLayoutPanel1.SetColumnSpan(this.GroupFlowPanel, 2);
            this.GroupFlowPanel.Controls.Add(this.BaseGroupButton);
            this.GroupFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GroupFlowPanel.Location = new System.Drawing.Point(3, 99);
            this.GroupFlowPanel.Name = "GroupFlowPanel";
            this.GroupFlowPanel.Padding = new System.Windows.Forms.Padding(2);
            this.GroupFlowPanel.Size = new System.Drawing.Size(388, 68);
            this.GroupFlowPanel.TabIndex = 0;
            // 
            // BaseGroupButton
            // 
            this.BaseGroupButton.BackColor = System.Drawing.Color.Gray;
            this.BaseGroupButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BaseGroupButton.FlatAppearance.BorderSize = 2;
            this.BaseGroupButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BaseGroupButton.Location = new System.Drawing.Point(2, 2);
            this.BaseGroupButton.Margin = new System.Windows.Forms.Padding(0);
            this.BaseGroupButton.Name = "BaseGroupButton";
            this.BaseGroupButton.Size = new System.Drawing.Size(64, 64);
            this.BaseGroupButton.TabIndex = 1;
            this.BaseGroupButton.TabStop = false;
            this.BaseGroupButton.UseVisualStyleBackColor = false;
            this.BaseGroupButton.Visible = false;
            // 
            // IRFlowPanel
            // 
            this.IRFlowPanel.BackColor = System.Drawing.Color.DimGray;
            this.IRFlowPanel.Controls.Add(this.BaseIRUsedButton);
            this.IRFlowPanel.Controls.Add(this.BaseIREmptyButton);
            this.IRFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IRFlowPanel.Location = new System.Drawing.Point(3, 173);
            this.IRFlowPanel.Name = "IRFlowPanel";
            this.IRFlowPanel.Padding = new System.Windows.Forms.Padding(2);
            this.IRFlowPanel.Size = new System.Drawing.Size(364, 294);
            this.IRFlowPanel.TabIndex = 1;
            // 
            // BaseIRUsedButton
            // 
            this.BaseIRUsedButton.BackColor = System.Drawing.Color.Gray;
            this.BaseIRUsedButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BaseIRUsedButton.FlatAppearance.BorderSize = 2;
            this.BaseIRUsedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BaseIRUsedButton.Location = new System.Drawing.Point(3, 3);
            this.BaseIRUsedButton.Margin = new System.Windows.Forms.Padding(1);
            this.BaseIRUsedButton.Name = "BaseIRUsedButton";
            this.BaseIRUsedButton.Size = new System.Drawing.Size(34, 34);
            this.BaseIRUsedButton.TabIndex = 0;
            this.BaseIRUsedButton.TabStop = false;
            this.BaseIRUsedButton.UseVisualStyleBackColor = false;
            this.BaseIRUsedButton.Visible = false;
            // 
            // BaseIREmptyButton
            // 
            this.BaseIREmptyButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BaseIREmptyButton.Enabled = false;
            this.BaseIREmptyButton.FlatAppearance.BorderSize = 2;
            this.BaseIREmptyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BaseIREmptyButton.ForeColor = System.Drawing.Color.Gray;
            this.BaseIREmptyButton.Location = new System.Drawing.Point(50, 14);
            this.BaseIREmptyButton.Margin = new System.Windows.Forms.Padding(12);
            this.BaseIREmptyButton.Name = "BaseIREmptyButton";
            this.BaseIREmptyButton.Size = new System.Drawing.Size(12, 12);
            this.BaseIREmptyButton.TabIndex = 10;
            this.BaseIREmptyButton.TabStop = false;
            this.BaseIREmptyButton.UseVisualStyleBackColor = false;
            this.BaseIREmptyButton.Visible = false;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.BackColor = System.Drawing.Color.DimGray;
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
            this.panel1.Controls.Add(this.ItemIconPanel);
            this.panel1.Controls.Add(this.IgnoreAssemblerCheckBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.AsProductCheckBox);
            this.panel1.Controls.Add(this.AsIngredientCheckBox);
            this.panel1.Controls.Add(this.ShowHiddenCheckBox);
            this.panel1.Controls.Add(this.FilterTextBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.panel1.ForeColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(388, 90);
            this.panel1.TabIndex = 2;
            // 
            // ItemIconPanel
            // 
            this.ItemIconPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ItemIconPanel.Location = new System.Drawing.Point(7, 34);
            this.ItemIconPanel.Name = "ItemIconPanel";
            this.ItemIconPanel.Size = new System.Drawing.Size(48, 48);
            this.ItemIconPanel.TabIndex = 6;
            this.ItemIconPanel.Visible = false;
            // 
            // IgnoreAssemblerCheckBox
            // 
            this.IgnoreAssemblerCheckBox.AutoSize = true;
            this.IgnoreAssemblerCheckBox.Location = new System.Drawing.Point(61, 34);
            this.IgnoreAssemblerCheckBox.Name = "IgnoreAssemblerCheckBox";
            this.IgnoreAssemblerCheckBox.Size = new System.Drawing.Size(215, 24);
            this.IgnoreAssemblerCheckBox.TabIndex = 2;
            this.IgnoreAssemblerCheckBox.Text = "Ignore Assembler Status";
            this.IgnoreAssemblerCheckBox.UseVisualStyleBackColor = true;
            this.IgnoreAssemblerCheckBox.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "Filter:";
            // 
            // AsProductCheckBox
            // 
            this.AsProductCheckBox.AutoSize = true;
            this.AsProductCheckBox.Checked = true;
            this.AsProductCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AsProductCheckBox.Location = new System.Drawing.Point(236, 63);
            this.AsProductCheckBox.Name = "AsProductCheckBox";
            this.AsProductCheckBox.Size = new System.Drawing.Size(126, 24);
            this.AsProductCheckBox.TabIndex = 4;
            this.AsProductCheckBox.Text = "As Product...";
            this.AsProductCheckBox.UseVisualStyleBackColor = true;
            this.AsProductCheckBox.Visible = false;
            // 
            // AsIngredientCheckBox
            // 
            this.AsIngredientCheckBox.AutoSize = true;
            this.AsIngredientCheckBox.Checked = true;
            this.AsIngredientCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AsIngredientCheckBox.Location = new System.Drawing.Point(61, 63);
            this.AsIngredientCheckBox.Name = "AsIngredientCheckBox";
            this.AsIngredientCheckBox.Size = new System.Drawing.Size(141, 24);
            this.AsIngredientCheckBox.TabIndex = 3;
            this.AsIngredientCheckBox.Text = "As Ingredient...";
            this.AsIngredientCheckBox.UseVisualStyleBackColor = true;
            this.AsIngredientCheckBox.Visible = false;
            // 
            // ShowHiddenCheckBox
            // 
            this.ShowHiddenCheckBox.AutoSize = true;
            this.ShowHiddenCheckBox.Location = new System.Drawing.Point(236, 3);
            this.ShowHiddenCheckBox.Name = "ShowHiddenCheckBox";
            this.ShowHiddenCheckBox.Size = new System.Drawing.Size(130, 24);
            this.ShowHiddenCheckBox.TabIndex = 1;
            this.ShowHiddenCheckBox.Text = "Show Hidden";
            this.ShowHiddenCheckBox.UseVisualStyleBackColor = true;
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.BackColor = System.Drawing.Color.LightGray;
            this.FilterTextBox.ForeColor = System.Drawing.Color.Black;
            this.FilterTextBox.Location = new System.Drawing.Point(61, 2);
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size(169, 26);
            this.FilterTextBox.TabIndex = 0;
            this.FilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            // 
            // IRPanelScrollBar
            // 
            this.IRPanelScrollBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IRPanelScrollBar.Location = new System.Drawing.Point(370, 173);
            this.IRPanelScrollBar.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.IRPanelScrollBar.Name = "IRPanelScrollBar";
            this.IRPanelScrollBar.Size = new System.Drawing.Size(21, 294);
            this.IRPanelScrollBar.TabIndex = 3;
            this.IRPanelScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.IRPanelScrollBar_Scroll);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 2);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.Controls.Add(this.AddSupplyButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.AddPassthroughButton, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.AddConsumerButton, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 470);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(394, 30);
            this.tableLayoutPanel2.TabIndex = 4;
            this.tableLayoutPanel2.Visible = false;
            // 
            // AddSupplyButton
            // 
            this.AddSupplyButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddSupplyButton.Location = new System.Drawing.Point(3, 3);
            this.AddSupplyButton.Name = "AddSupplyButton";
            this.AddSupplyButton.Size = new System.Drawing.Size(112, 24);
            this.AddSupplyButton.TabIndex = 5;
            this.AddSupplyButton.Text = "As Source";
            this.AddSupplyButton.UseVisualStyleBackColor = true;
            // 
            // AddPassthroughButton
            // 
            this.AddPassthroughButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddPassthroughButton.Location = new System.Drawing.Point(128, 3);
            this.AddPassthroughButton.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
            this.AddPassthroughButton.Name = "AddPassthroughButton";
            this.AddPassthroughButton.Size = new System.Drawing.Size(137, 24);
            this.AddPassthroughButton.TabIndex = 6;
            this.AddPassthroughButton.Text = "As Pass-Through";
            this.AddPassthroughButton.UseVisualStyleBackColor = true;
            // 
            // AddConsumerButton
            // 
            this.AddConsumerButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AddConsumerButton.Location = new System.Drawing.Point(278, 3);
            this.AddConsumerButton.Name = "AddConsumerButton";
            this.AddConsumerButton.Size = new System.Drawing.Size(113, 24);
            this.AddConsumerButton.TabIndex = 7;
            this.AddConsumerButton.Text = "As Output";
            this.AddConsumerButton.UseVisualStyleBackColor = true;
            // 
            // IRChooserPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "IRChooserPanel";
            this.Size = new System.Drawing.Size(394, 500);
            this.Leave += new System.EventHandler(this.IRChooserPanel_Leave);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.GroupFlowPanel.ResumeLayout(false);
            this.IRFlowPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        internal System.Windows.Forms.FlowLayoutPanel IRFlowPanel;
        internal System.Windows.Forms.FlowLayoutPanel GroupFlowPanel;
        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.VScrollBar IRPanelScrollBar;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.CheckBox AsProductCheckBox;
        internal System.Windows.Forms.CheckBox AsIngredientCheckBox;
        internal System.Windows.Forms.CheckBox ShowHiddenCheckBox;
        internal System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        internal System.Windows.Forms.Button AddSupplyButton;
        internal System.Windows.Forms.Button AddPassthroughButton;
        internal System.Windows.Forms.Button AddConsumerButton;
        internal System.Windows.Forms.CheckBox IgnoreAssemblerCheckBox;
        internal System.Windows.Forms.Panel ItemIconPanel;
        private System.Windows.Forms.Button BaseGroupButton;
        private System.Windows.Forms.Button BaseIRUsedButton;
        private System.Windows.Forms.Button BaseIREmptyButton;
    }
}
