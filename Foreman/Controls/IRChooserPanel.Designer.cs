
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
			this.MainTable = new System.Windows.Forms.TableLayoutPanel();
			this.FilterPanel = new System.Windows.Forms.Panel();
			this.RecipeNameOnlyFilterCheckBox = new System.Windows.Forms.CheckBox();
			this.ItemIconPanel = new System.Windows.Forms.Panel();
			this.IgnoreAssemblerCheckBox = new System.Windows.Forms.CheckBox();
			this.FilterLabel = new System.Windows.Forms.Label();
			this.AsProductCheckBox = new System.Windows.Forms.CheckBox();
			this.AsIngredientCheckBox = new System.Windows.Forms.CheckBox();
			this.ShowHiddenCheckBox = new System.Windows.Forms.CheckBox();
			this.FilterTextBox = new System.Windows.Forms.TextBox();
			this.OtherNodeOptionsTable = new System.Windows.Forms.TableLayoutPanel();
			this.AddSupplyButton = new System.Windows.Forms.Button();
			this.AddPassthroughButton = new System.Windows.Forms.Button();
			this.AddConsumerButton = new System.Windows.Forms.Button();
			this.IRTable = new System.Windows.Forms.TableLayoutPanel();
			this.IRScrollBar = new System.Windows.Forms.VScrollBar();
			this.GroupTable = new System.Windows.Forms.TableLayoutPanel();
			this.MainTable.SuspendLayout();
			this.FilterPanel.SuspendLayout();
			this.OtherNodeOptionsTable.SuspendLayout();
			this.IRTable.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTable
			// 
			this.MainTable.AutoSize = true;
			this.MainTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MainTable.BackColor = System.Drawing.Color.Black;
			this.MainTable.ColumnCount = 1;
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.MainTable.Controls.Add(this.FilterPanel, 0, 0);
			this.MainTable.Controls.Add(this.OtherNodeOptionsTable, 0, 3);
			this.MainTable.Controls.Add(this.IRTable, 0, 2);
			this.MainTable.Controls.Add(this.GroupTable, 0, 1);
			this.MainTable.Location = new System.Drawing.Point(0, 0);
			this.MainTable.Margin = new System.Windows.Forms.Padding(0);
			this.MainTable.Name = "MainTable";
			this.MainTable.Padding = new System.Windows.Forms.Padding(0, 2, 0, 1);
			this.MainTable.RowCount = 4;
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainTable.Size = new System.Drawing.Size(276, 360);
			this.MainTable.TabIndex = 0;
			// 
			// FilterPanel
			// 
			this.FilterPanel.AutoSize = true;
			this.FilterPanel.BackColor = System.Drawing.Color.DimGray;
			this.FilterPanel.Controls.Add(this.RecipeNameOnlyFilterCheckBox);
			this.FilterPanel.Controls.Add(this.ItemIconPanel);
			this.FilterPanel.Controls.Add(this.IgnoreAssemblerCheckBox);
			this.FilterPanel.Controls.Add(this.FilterLabel);
			this.FilterPanel.Controls.Add(this.AsProductCheckBox);
			this.FilterPanel.Controls.Add(this.AsIngredientCheckBox);
			this.FilterPanel.Controls.Add(this.ShowHiddenCheckBox);
			this.FilterPanel.Controls.Add(this.FilterTextBox);
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.ForeColor = System.Drawing.Color.White;
			this.FilterPanel.Location = new System.Drawing.Point(3, 3);
			this.FilterPanel.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = new System.Drawing.Size(270, 67);
			this.FilterPanel.TabIndex = 2;
			// 
			// RecipeNameOnlyFilterCheckBox
			// 
			this.RecipeNameOnlyFilterCheckBox.AutoSize = true;
			this.RecipeNameOnlyFilterCheckBox.Location = new System.Drawing.Point(177, 4);
			this.RecipeNameOnlyFilterCheckBox.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.RecipeNameOnlyFilterCheckBox.Name = "RecipeNameOnlyFilterCheckBox";
			this.RecipeNameOnlyFilterCheckBox.Size = new System.Drawing.Size(84, 17);
			this.RecipeNameOnlyFilterCheckBox.TabIndex = 7;
			this.RecipeNameOnlyFilterCheckBox.Text = "Recipe Only";
			this.RecipeNameOnlyFilterCheckBox.UseVisualStyleBackColor = true;
			this.RecipeNameOnlyFilterCheckBox.Visible = false;
			// 
			// ItemIconPanel
			// 
			this.ItemIconPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.ItemIconPanel.Location = new System.Drawing.Point(3, 27);
			this.ItemIconPanel.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.ItemIconPanel.Name = "ItemIconPanel";
			this.ItemIconPanel.Size = new System.Drawing.Size(36, 39);
			this.ItemIconPanel.TabIndex = 6;
			this.ItemIconPanel.Visible = false;
			// 
			// IgnoreAssemblerCheckBox
			// 
			this.IgnoreAssemblerCheckBox.AutoSize = true;
			this.IgnoreAssemblerCheckBox.Location = new System.Drawing.Point(45, 27);
			this.IgnoreAssemblerCheckBox.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.IgnoreAssemblerCheckBox.Name = "IgnoreAssemblerCheckBox";
			this.IgnoreAssemblerCheckBox.Size = new System.Drawing.Size(107, 17);
			this.IgnoreAssemblerCheckBox.TabIndex = 2;
			this.IgnoreAssemblerCheckBox.Text = "Ignore Assembler";
			this.IgnoreAssemblerCheckBox.UseVisualStyleBackColor = true;
			// 
			// FilterLabel
			// 
			this.FilterLabel.AutoSize = true;
			this.FilterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
			this.FilterLabel.Location = new System.Drawing.Point(3, 3);
			this.FilterLabel.Name = "FilterLabel";
			this.FilterLabel.Size = new System.Drawing.Size(43, 17);
			this.FilterLabel.TabIndex = 4;
			this.FilterLabel.Text = "Filter:";
			// 
			// AsProductCheckBox
			// 
			this.AsProductCheckBox.AutoSize = true;
			this.AsProductCheckBox.Checked = true;
			this.AsProductCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.AsProductCheckBox.Location = new System.Drawing.Point(177, 47);
			this.AsProductCheckBox.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.AsProductCheckBox.Name = "AsProductCheckBox";
			this.AsProductCheckBox.Size = new System.Drawing.Size(87, 17);
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
			this.AsIngredientCheckBox.Location = new System.Drawing.Point(45, 47);
			this.AsIngredientCheckBox.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.AsIngredientCheckBox.Name = "AsIngredientCheckBox";
			this.AsIngredientCheckBox.Size = new System.Drawing.Size(97, 17);
			this.AsIngredientCheckBox.TabIndex = 3;
			this.AsIngredientCheckBox.Text = "As Ingredient...";
			this.AsIngredientCheckBox.UseVisualStyleBackColor = true;
			this.AsIngredientCheckBox.Visible = false;
			// 
			// ShowHiddenCheckBox
			// 
			this.ShowHiddenCheckBox.AutoSize = true;
			this.ShowHiddenCheckBox.Location = new System.Drawing.Point(177, 27);
			this.ShowHiddenCheckBox.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.ShowHiddenCheckBox.Name = "ShowHiddenCheckBox";
			this.ShowHiddenCheckBox.Size = new System.Drawing.Size(90, 17);
			this.ShowHiddenCheckBox.TabIndex = 1;
			this.ShowHiddenCheckBox.Text = "Show Hidden";
			this.ShowHiddenCheckBox.UseVisualStyleBackColor = true;
			// 
			// FilterTextBox
			// 
			this.FilterTextBox.BackColor = System.Drawing.Color.LightGray;
			this.FilterTextBox.ForeColor = System.Drawing.Color.Black;
			this.FilterTextBox.Location = new System.Drawing.Point(45, 1);
			this.FilterTextBox.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
			this.FilterTextBox.Name = "FilterTextBox";
			this.FilterTextBox.Size = new System.Drawing.Size(127, 20);
			this.FilterTextBox.TabIndex = 0;
			this.FilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
			// 
			// OtherNodeOptionsTable
			// 
			this.OtherNodeOptionsTable.AutoSize = true;
			this.OtherNodeOptionsTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.OtherNodeOptionsTable.ColumnCount = 3;
			this.OtherNodeOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.OtherNodeOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.OtherNodeOptionsTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.OtherNodeOptionsTable.Controls.Add(this.AddSupplyButton, 0, 0);
			this.OtherNodeOptionsTable.Controls.Add(this.AddPassthroughButton, 1, 0);
			this.OtherNodeOptionsTable.Controls.Add(this.AddConsumerButton, 2, 0);
			this.OtherNodeOptionsTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherNodeOptionsTable.Location = new System.Drawing.Point(0, 335);
			this.OtherNodeOptionsTable.Margin = new System.Windows.Forms.Padding(0);
			this.OtherNodeOptionsTable.Name = "OtherNodeOptionsTable";
			this.OtherNodeOptionsTable.RowCount = 1;
			this.OtherNodeOptionsTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.OtherNodeOptionsTable.Size = new System.Drawing.Size(276, 24);
			this.OtherNodeOptionsTable.TabIndex = 4;
			this.OtherNodeOptionsTable.Visible = false;
			// 
			// AddSupplyButton
			// 
			this.AddSupplyButton.AutoSize = true;
			this.AddSupplyButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AddSupplyButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddSupplyButton.Location = new System.Drawing.Point(3, 0);
			this.AddSupplyButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 1);
			this.AddSupplyButton.Name = "AddSupplyButton";
			this.AddSupplyButton.Size = new System.Drawing.Size(63, 23);
			this.AddSupplyButton.TabIndex = 5;
			this.AddSupplyButton.Text = "Source";
			this.AddSupplyButton.UseVisualStyleBackColor = true;
			// 
			// AddPassthroughButton
			// 
			this.AddPassthroughButton.AutoSize = true;
			this.AddPassthroughButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AddPassthroughButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddPassthroughButton.Location = new System.Drawing.Point(78, 0);
			this.AddPassthroughButton.Margin = new System.Windows.Forms.Padding(9, 0, 9, 1);
			this.AddPassthroughButton.Name = "AddPassthroughButton";
			this.AddPassthroughButton.Size = new System.Drawing.Size(120, 23);
			this.AddPassthroughButton.TabIndex = 6;
			this.AddPassthroughButton.Text = "Pass-Through";
			this.AddPassthroughButton.UseVisualStyleBackColor = true;
			// 
			// AddConsumerButton
			// 
			this.AddConsumerButton.AutoSize = true;
			this.AddConsumerButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AddConsumerButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddConsumerButton.Location = new System.Drawing.Point(210, 0);
			this.AddConsumerButton.Margin = new System.Windows.Forms.Padding(3, 0, 2, 1);
			this.AddConsumerButton.Name = "AddConsumerButton";
			this.AddConsumerButton.Size = new System.Drawing.Size(64, 23);
			this.AddConsumerButton.TabIndex = 7;
			this.AddConsumerButton.Text = "Output";
			this.AddConsumerButton.UseVisualStyleBackColor = true;
			// 
			// IRTable
			// 
			this.IRTable.AutoSize = true;
			this.IRTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.IRTable.BackColor = System.Drawing.Color.DimGray;
			this.IRTable.ColumnCount = 11;
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0004F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0004F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0004F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0004F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0004F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0004F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0004F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0004F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10.0004F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.996401F));
			this.IRTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 12F));
			this.IRTable.Controls.Add(this.IRScrollBar, 10, 0);
			this.IRTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IRTable.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
			this.IRTable.Location = new System.Drawing.Point(3, 124);
			this.IRTable.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
			this.IRTable.Name = "IRTable";
			this.IRTable.RowCount = 8;
			this.IRTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.IRTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.IRTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.IRTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.IRTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.IRTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.IRTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.IRTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.IRTable.Size = new System.Drawing.Size(270, 208);
			this.IRTable.TabIndex = 5;
			// 
			// IRScrollBar
			// 
			this.IRScrollBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IRScrollBar.Location = new System.Drawing.Point(250, 0);
			this.IRScrollBar.Name = "IRScrollBar";
			this.IRScrollBar.Padding = new System.Windows.Forms.Padding(0, 0, 1, 0);
			this.IRTable.SetRowSpan(this.IRScrollBar, 8);
			this.IRScrollBar.Size = new System.Drawing.Size(20, 208);
			this.IRScrollBar.TabIndex = 3;
			this.IRScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.IRPanelScrollBar_Scroll);
			// 
			// GroupTable
			// 
			this.GroupTable.AutoSize = true;
			this.GroupTable.BackColor = System.Drawing.Color.DimGray;
			this.GroupTable.ColumnCount = 6;
			this.GroupTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.GroupTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.GroupTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.GroupTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.GroupTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.GroupTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
			this.GroupTable.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupTable.Location = new System.Drawing.Point(3, 74);
			this.GroupTable.Margin = new System.Windows.Forms.Padding(3, 1, 3, 3);
			this.GroupTable.Name = "GroupTable";
			this.GroupTable.RowCount = 1;
			this.GroupTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.GroupTable.Size = new System.Drawing.Size(270, 46);
			this.GroupTable.TabIndex = 6;
			// 
			// IRChooserPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Black;
			this.Controls.Add(this.MainTable);
			this.DoubleBuffered = true;
			this.Margin = new System.Windows.Forms.Padding(1);
			this.Name = "IRChooserPanel";
			this.Size = new System.Drawing.Size(276, 360);
			this.Leave += new System.EventHandler(this.IRChooserPanel_Leave);
			this.MainTable.ResumeLayout(false);
			this.MainTable.PerformLayout();
			this.FilterPanel.ResumeLayout(false);
			this.FilterPanel.PerformLayout();
			this.OtherNodeOptionsTable.ResumeLayout(false);
			this.OtherNodeOptionsTable.PerformLayout();
			this.IRTable.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel MainTable;
        private System.Windows.Forms.Panel FilterPanel;
        protected System.Windows.Forms.VScrollBar IRScrollBar;
        protected System.Windows.Forms.CheckBox AsProductCheckBox;
        protected System.Windows.Forms.CheckBox AsIngredientCheckBox;
        protected System.Windows.Forms.CheckBox ShowHiddenCheckBox;
        protected System.Windows.Forms.TextBox FilterTextBox;
        protected System.Windows.Forms.CheckBox IgnoreAssemblerCheckBox;
        protected System.Windows.Forms.Panel ItemIconPanel;
        protected System.Windows.Forms.Label FilterLabel;
        protected System.Windows.Forms.CheckBox RecipeNameOnlyFilterCheckBox;
		protected System.Windows.Forms.TableLayoutPanel OtherNodeOptionsTable;
		protected System.Windows.Forms.Button AddSupplyButton;
		protected System.Windows.Forms.Button AddPassthroughButton;
		protected System.Windows.Forms.Button AddConsumerButton;
		protected System.Windows.Forms.TableLayoutPanel IRTable;
		protected System.Windows.Forms.TableLayoutPanel GroupTable;
	}
}
