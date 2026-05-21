using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    partial class IRChooserPanel {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent() {
            contentStack = new FlowLayoutPanel();
            headerStack = new FlowLayoutPanel();
            filterRow = new FlowLayoutPanel();
            FilterLabel = new Label();
            FilterTextBox = new TextBox();
            RecipeNameOnlyFilterCheckBox = new CheckBox();
            optionRow = new FlowLayoutPanel();
            IgnoreAssemblerCheckBox = new CheckBox();
            ShowHiddenCheckBox = new CheckBox();
            QualityRow = new FlowLayoutPanel();
            QualityLabel = new Label();
            QualitySelector = new ComboBox();
            recipeRoleRow = new FlowLayoutPanel();
            AsIngredientCheckBox = new CheckBox();
            AsProductCheckBox = new CheckBox();
            AsFuelCheckBox = new CheckBox();
            ItemIconPanel = new Panel();
            groupsPanel = new FlowLayoutPanel();
            iconGridBand = new Panel();
            iconGrid = new ChooserIconGrid();
            nodeOptionsRowA = new FlowLayoutPanel();
            AddSupplyButton = new Button();
            AddPassthroughButton = new Button();
            AddConsumerButton = new Button();
            nodeOptionsRowB = new FlowLayoutPanel();
            AddUnspoilButton = new Button();
            AddUnplantButton = new Button();
            AddSpoilButton = new Button();
            AddPlantButton = new Button();
            contentStack.SuspendLayout();
            headerStack.SuspendLayout();
            filterRow.SuspendLayout();
            optionRow.SuspendLayout();
            QualityRow.SuspendLayout();
            recipeRoleRow.SuspendLayout();
            nodeOptionsRowA.SuspendLayout();
            nodeOptionsRowB.SuspendLayout();
            SuspendLayout();
            // 
            // contentStack
            // 
            contentStack.Controls.Add(headerStack);
            contentStack.Controls.Add(groupsPanel);
            iconGridBand.Controls.Add(iconGrid);
            contentStack.Controls.Add(iconGridBand);
            contentStack.Controls.Add(nodeOptionsRowA);
            contentStack.Controls.Add(nodeOptionsRowB);
            contentStack.AutoSize = false;
            contentStack.BackColor = Color.DimGray;
            contentStack.FlowDirection = FlowDirection.TopDown;
            contentStack.Location = new Point(0, 0);
            contentStack.Margin = new Padding(0);
            contentStack.Name = "contentStack";
            contentStack.Size = new Size(427, 458);
            contentStack.TabIndex = 0;
            contentStack.WrapContents = false;
            // 
            // headerStack
            // 
            headerStack.AutoSize = true;
            headerStack.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            headerStack.BackColor = Color.DimGray;
            headerStack.Controls.Add(filterRow);
            headerStack.Controls.Add(optionRow);
            headerStack.Controls.Add(QualityRow);
            headerStack.Controls.Add(recipeRoleRow);
            headerStack.Controls.Add(ItemIconPanel);
            headerStack.FlowDirection = FlowDirection.TopDown;
            headerStack.Location = new Point(0, 0);
            headerStack.Margin = new Padding(0);
            headerStack.Name = "headerStack";
            headerStack.Padding = new Padding(4, 4, 4, 2);
            headerStack.Size = new Size(354, 195);
            headerStack.TabIndex = 0;
            headerStack.WrapContents = false;
            // 
            // filterRow
            // 
            filterRow.AutoSize = true;
            filterRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            filterRow.Controls.Add(FilterLabel);
            filterRow.Controls.Add(FilterTextBox);
            filterRow.Controls.Add(RecipeNameOnlyFilterCheckBox);
            filterRow.Location = new Point(4, 4);
            filterRow.Margin = new Padding(0);
            filterRow.Name = "filterRow";
            filterRow.Padding = new Padding(4, 2, 4, 2);
            filterRow.Size = new Size(346, 35);
            filterRow.TabIndex = 0;
            filterRow.WrapContents = false;
            // 
            // FilterLabel
            // 
            FilterLabel.AutoSize = true;
            FilterLabel.ForeColor = Color.White;
            FilterLabel.Location = new Point(4, 7);
            FilterLabel.Margin = new Padding(0, 5, 5, 5);
            FilterLabel.Name = "FilterLabel";
            FilterLabel.Size = new Size(45, 20);
            FilterLabel.TabIndex = 0;
            FilterLabel.Text = "Filter:";
            // 
            // FilterTextBox
            // 
            FilterTextBox.BackColor = Color.LightGray;
            FilterTextBox.ForeColor = Color.Black;
            FilterTextBox.Location = new Point(54, 4);
            FilterTextBox.Margin = new Padding(0, 2, 10, 2);
            FilterTextBox.Name = "FilterTextBox";
            FilterTextBox.Size = new Size(127, 23);
            FilterTextBox.TabIndex = 1;
            // 
            // RecipeNameOnlyFilterCheckBox
            // 
            RecipeNameOnlyFilterCheckBox.AutoSize = true;
            RecipeNameOnlyFilterCheckBox.ForeColor = Color.White;
            RecipeNameOnlyFilterCheckBox.Location = new Point(222, 4);
            RecipeNameOnlyFilterCheckBox.Margin = new Padding(0, 2, 10, 2);
            RecipeNameOnlyFilterCheckBox.Name = "RecipeNameOnlyFilterCheckBox";
            RecipeNameOnlyFilterCheckBox.Size = new Size(110, 24);
            RecipeNameOnlyFilterCheckBox.TabIndex = 2;
            RecipeNameOnlyFilterCheckBox.Text = "Recipe Only";
            RecipeNameOnlyFilterCheckBox.UseVisualStyleBackColor = true;
            RecipeNameOnlyFilterCheckBox.Visible = false;
            // 
            // optionRow
            // 
            optionRow.AutoSize = true;
            optionRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            optionRow.Controls.Add(IgnoreAssemblerCheckBox);
            optionRow.Controls.Add(ShowHiddenCheckBox);
            optionRow.Location = new Point(4, 39);
            optionRow.Margin = new Padding(0);
            optionRow.Name = "optionRow";
            optionRow.Padding = new Padding(4, 2, 4, 2);
            optionRow.Size = new Size(295, 32);
            optionRow.TabIndex = 1;
            optionRow.WrapContents = false;
            // 
            // IgnoreAssemblerCheckBox
            // 
            IgnoreAssemblerCheckBox.AutoSize = true;
            IgnoreAssemblerCheckBox.ForeColor = Color.White;
            IgnoreAssemblerCheckBox.Location = new Point(4, 4);
            IgnoreAssemblerCheckBox.Margin = new Padding(0, 2, 10, 2);
            IgnoreAssemblerCheckBox.Name = "IgnoreAssemblerCheckBox";
            IgnoreAssemblerCheckBox.Size = new Size(147, 24);
            IgnoreAssemblerCheckBox.TabIndex = 0;
            IgnoreAssemblerCheckBox.Text = "Ignore Assembler";
            IgnoreAssemblerCheckBox.UseVisualStyleBackColor = true;
            // 
            // ShowHiddenCheckBox
            // 
            ShowHiddenCheckBox.AutoSize = true;
            ShowHiddenCheckBox.ForeColor = Color.White;
            ShowHiddenCheckBox.Location = new Point(161, 4);
            ShowHiddenCheckBox.Margin = new Padding(0, 2, 10, 2);
            ShowHiddenCheckBox.Name = "ShowHiddenCheckBox";
            ShowHiddenCheckBox.Size = new Size(120, 24);
            ShowHiddenCheckBox.TabIndex = 1;
            ShowHiddenCheckBox.Text = "Show Hidden";
            ShowHiddenCheckBox.UseVisualStyleBackColor = true;
            // 
            // QualityRow
            // 
            QualityRow.AutoSize = true;
            QualityRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            QualityRow.Controls.Add(QualityLabel);
            QualityRow.Controls.Add(QualitySelector);
            QualityRow.Location = new Point(4, 71);
            QualityRow.Margin = new Padding(0);
            QualityRow.Name = "QualityRow";
            QualityRow.Padding = new Padding(4, 2, 4, 2);
            QualityRow.Size = new Size(254, 36);
            QualityRow.TabIndex = 2;
            QualityRow.Visible = false;
            QualityRow.WrapContents = false;
            // 
            // QualityLabel
            // 
            QualityLabel.AutoSize = true;
            QualityLabel.ForeColor = Color.White;
            QualityLabel.Location = new Point(4, 7);
            QualityLabel.Margin = new Padding(0, 5, 5, 5);
            QualityLabel.Name = "QualityLabel";
            QualityLabel.Size = new Size(59, 20);
            QualityLabel.TabIndex = 0;
            QualityLabel.Text = "Quality:";
            // 
            // QualitySelector
            // 
            QualitySelector.DropDownStyle = ComboBoxStyle.DropDownList;
            QualitySelector.Location = new Point(68, 4);
            QualitySelector.Margin = new Padding(0, 2, 0, 2);
            QualitySelector.Name = "QualitySelector";
            QualitySelector.Size = new Size(146, 23);
            QualitySelector.TabIndex = 1;
            // 
            // recipeRoleRow
            // 
            recipeRoleRow.AutoSize = true;
            recipeRoleRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            recipeRoleRow.Controls.Add(AsIngredientCheckBox);
            recipeRoleRow.Controls.Add(AsProductCheckBox);
            recipeRoleRow.Controls.Add(AsFuelCheckBox);
            recipeRoleRow.Location = new Point(4, 107);
            recipeRoleRow.Margin = new Padding(0);
            recipeRoleRow.Name = "recipeRoleRow";
            recipeRoleRow.Padding = new Padding(4, 2, 4, 2);
            recipeRoleRow.Size = new Size(277, 32);
            recipeRoleRow.TabIndex = 3;
            recipeRoleRow.Visible = false;
            recipeRoleRow.WrapContents = false;
            // 
            // AsIngredientCheckBox
            // 
            AsIngredientCheckBox.AutoSize = true;
            AsIngredientCheckBox.Checked = true;
            AsIngredientCheckBox.CheckState = CheckState.Checked;
            AsIngredientCheckBox.ForeColor = Color.White;
            AsIngredientCheckBox.Location = new Point(4, 4);
            AsIngredientCheckBox.Margin = new Padding(0, 2, 10, 2);
            AsIngredientCheckBox.Name = "AsIngredientCheckBox";
            AsIngredientCheckBox.Size = new Size(99, 24);
            AsIngredientCheckBox.TabIndex = 0;
            AsIngredientCheckBox.Text = "Ingredient";
            AsIngredientCheckBox.UseVisualStyleBackColor = true;
            // 
            // AsProductCheckBox
            // 
            AsProductCheckBox.AutoSize = true;
            AsProductCheckBox.Checked = true;
            AsProductCheckBox.CheckState = CheckState.Checked;
            AsProductCheckBox.ForeColor = Color.White;
            AsProductCheckBox.Location = new Point(113, 4);
            AsProductCheckBox.Margin = new Padding(0, 2, 10, 2);
            AsProductCheckBox.Name = "AsProductCheckBox";
            AsProductCheckBox.Size = new Size(82, 24);
            AsProductCheckBox.TabIndex = 1;
            AsProductCheckBox.Text = "Product";
            AsProductCheckBox.UseVisualStyleBackColor = true;
            // 
            // AsFuelCheckBox
            // 
            AsFuelCheckBox.AutoSize = true;
            AsFuelCheckBox.Checked = true;
            AsFuelCheckBox.CheckState = CheckState.Checked;
            AsFuelCheckBox.ForeColor = Color.White;
            AsFuelCheckBox.Location = new Point(205, 4);
            AsFuelCheckBox.Margin = new Padding(0, 2, 10, 2);
            AsFuelCheckBox.Name = "AsFuelCheckBox";
            AsFuelCheckBox.Size = new Size(58, 24);
            AsFuelCheckBox.TabIndex = 2;
            AsFuelCheckBox.Text = "Fuel";
            AsFuelCheckBox.UseVisualStyleBackColor = true;
            // 
            // ItemIconPanel
            // 
            ItemIconPanel.BackgroundImageLayout = ImageLayout.Stretch;
            ItemIconPanel.Location = new Point(8, 141);
            ItemIconPanel.Margin = new Padding(4, 2, 4, 2);
            ItemIconPanel.Name = "ItemIconPanel";
            ItemIconPanel.Size = new Size(40, 40);
            ItemIconPanel.TabIndex = 4;
            ItemIconPanel.Visible = false;
            // 
            // groupsPanel
            // 
            groupsPanel.AutoSize = false;
            groupsPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
            groupsPanel.FlowDirection = FlowDirection.LeftToRight;
            groupsPanel.WrapContents = true;
            groupsPanel.BackColor = Color.DimGray;
            groupsPanel.Location = new Point(0, 195);
            groupsPanel.Margin = new Padding(0);
            groupsPanel.Name = "groupsPanel";
            groupsPanel.Padding = new Padding(4, 1, 4, 4);
            groupsPanel.Size = new Size(8, 5);
            groupsPanel.TabIndex = 1;
            // 
            // iconGridBand
            // 
            iconGridBand.BackColor = Color.DimGray;
            iconGridBand.Controls.Add(iconGrid);
            iconGridBand.Location = new Point(0, 200);
            iconGridBand.Margin = new Padding(0);
            iconGridBand.Name = "iconGridBand";
            iconGridBand.Size = new Size(421, 320);
            iconGridBand.TabIndex = 2;
            // 
            // iconGrid
            // 
            iconGrid.BackColor = Color.DimGray;
            iconGrid.Location = new Point(0, 0);
            iconGrid.Margin = new Padding(0);
            iconGrid.MinimumSize = new Size(421, 320);
            iconGrid.Name = "iconGrid";
            iconGrid.Size = new Size(421, 320);
            iconGrid.TabIndex = 0;
            // 
            // nodeOptionsRowA
            // 
            nodeOptionsRowA.AutoSize = true;
            nodeOptionsRowA.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            nodeOptionsRowA.BackColor = Color.Black;
            nodeOptionsRowA.Controls.Add(AddSupplyButton);
            nodeOptionsRowA.Controls.Add(AddPassthroughButton);
            nodeOptionsRowA.Controls.Add(AddConsumerButton);
            nodeOptionsRowA.Location = new Point(0, 601);
            nodeOptionsRowA.Margin = new Padding(0);
            nodeOptionsRowA.Name = "nodeOptionsRowA";
            nodeOptionsRowA.Padding = new Padding(4, 2, 4, 2);
            nodeOptionsRowA.Size = new Size(341, 43);
            nodeOptionsRowA.TabIndex = 3;
            nodeOptionsRowA.Visible = false;
            nodeOptionsRowA.WrapContents = false;
            // 
            // AddSupplyButton
            // 
            AddSupplyButton.AutoSize = true;
            AddSupplyButton.Location = new Point(8, 2);
            AddSupplyButton.Margin = new Padding(4, 0, 4, 1);
            AddSupplyButton.Name = "AddSupplyButton";
            AddSupplyButton.Size = new Size(80, 38);
            AddSupplyButton.TabIndex = 0;
            AddSupplyButton.Text = "Source";
            AddSupplyButton.UseVisualStyleBackColor = true;
            // 
            // AddPassthroughButton
            // 
            AddPassthroughButton.AutoSize = true;
            AddPassthroughButton.Location = new Point(103, 2);
            AddPassthroughButton.Margin = new Padding(11, 0, 11, 1);
            AddPassthroughButton.Name = "AddPassthroughButton";
            AddPassthroughButton.Size = new Size(134, 38);
            AddPassthroughButton.TabIndex = 1;
            AddPassthroughButton.Text = "Pass-Through";
            AddPassthroughButton.UseVisualStyleBackColor = true;
            // 
            // AddConsumerButton
            // 
            AddConsumerButton.AutoSize = true;
            AddConsumerButton.Location = new Point(252, 2);
            AddConsumerButton.Margin = new Padding(4, 0, 4, 1);
            AddConsumerButton.Name = "AddConsumerButton";
            AddConsumerButton.Size = new Size(81, 38);
            AddConsumerButton.TabIndex = 2;
            AddConsumerButton.Text = "Output";
            AddConsumerButton.UseVisualStyleBackColor = true;
            // 
            // nodeOptionsRowB
            // 
            nodeOptionsRowB.AutoSize = true;
            nodeOptionsRowB.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            nodeOptionsRowB.BackColor = Color.Black;
            nodeOptionsRowB.Controls.Add(AddUnspoilButton);
            nodeOptionsRowB.Controls.Add(AddUnplantButton);
            nodeOptionsRowB.Controls.Add(AddSpoilButton);
            nodeOptionsRowB.Controls.Add(AddPlantButton);
            nodeOptionsRowB.Location = new Point(0, 644);
            nodeOptionsRowB.Margin = new Padding(0);
            nodeOptionsRowB.Name = "nodeOptionsRowB";
            nodeOptionsRowB.Padding = new Padding(4, 2, 4, 2);
            nodeOptionsRowB.Size = new Size(376, 43);
            nodeOptionsRowB.TabIndex = 4;
            nodeOptionsRowB.Visible = false;
            nodeOptionsRowB.WrapContents = false;
            // 
            // AddUnspoilButton
            // 
            AddUnspoilButton.AutoSize = true;
            AddUnspoilButton.Location = new Point(8, 2);
            AddUnspoilButton.Margin = new Padding(4, 0, 4, 1);
            AddUnspoilButton.Name = "AddUnspoilButton";
            AddUnspoilButton.Size = new Size(89, 38);
            AddUnspoilButton.TabIndex = 0;
            AddUnspoilButton.Text = "UnSpoil";
            AddUnspoilButton.UseVisualStyleBackColor = true;
            // 
            // AddUnplantButton
            // 
            AddUnplantButton.AutoSize = true;
            AddUnplantButton.Location = new Point(112, 2);
            AddUnplantButton.Margin = new Padding(11, 0, 11, 1);
            AddUnplantButton.Name = "AddUnplantButton";
            AddUnplantButton.Size = new Size(88, 38);
            AddUnplantButton.TabIndex = 1;
            AddUnplantButton.Text = "UnPlant";
            AddUnplantButton.UseVisualStyleBackColor = true;
            // 
            // AddSpoilButton
            // 
            AddSpoilButton.AutoSize = true;
            AddSpoilButton.Location = new Point(222, 2);
            AddSpoilButton.Margin = new Padding(11, 0, 11, 1);
            AddSpoilButton.Name = "AddSpoilButton";
            AddSpoilButton.Size = new Size(66, 38);
            AddSpoilButton.TabIndex = 2;
            AddSpoilButton.Text = "Spoil";
            AddSpoilButton.UseVisualStyleBackColor = true;
            // 
            // AddPlantButton
            // 
            AddPlantButton.AutoSize = true;
            AddPlantButton.Location = new Point(303, 2);
            AddPlantButton.Margin = new Padding(4, 0, 4, 1);
            AddPlantButton.Name = "AddPlantButton";
            AddPlantButton.Size = new Size(65, 38);
            AddPlantButton.TabIndex = 3;
            AddPlantButton.Text = "Plant";
            AddPlantButton.UseVisualStyleBackColor = true;
            // 
            // IRChooserPanel
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.Black;
            Controls.Add(contentStack);
            Margin = new Padding(0);
            MinimumSize = new Size(427, 416);
            Name = "IRChooserPanel";
            Size = new Size(427, 458);
            contentStack.ResumeLayout(false);
            contentStack.PerformLayout();
            headerStack.ResumeLayout(false);
            headerStack.PerformLayout();
            filterRow.ResumeLayout(false);
            filterRow.PerformLayout();
            optionRow.ResumeLayout(false);
            optionRow.PerformLayout();
            QualityRow.ResumeLayout(false);
            QualityRow.PerformLayout();
            recipeRoleRow.ResumeLayout(false);
            recipeRoleRow.PerformLayout();
            nodeOptionsRowA.ResumeLayout(false);
            nodeOptionsRowA.PerformLayout();
            nodeOptionsRowB.ResumeLayout(false);
            nodeOptionsRowB.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel contentStack;
        private FlowLayoutPanel headerStack;
        private FlowLayoutPanel filterRow;
        private FlowLayoutPanel optionRow;
        protected FlowLayoutPanel groupsPanel;
        private Panel iconGridBand;
        protected ChooserIconGrid iconGrid;
        protected FlowLayoutPanel nodeOptionsRowA;
        protected FlowLayoutPanel nodeOptionsRowB;
        protected FlowLayoutPanel recipeRoleRow;
        protected TextBox FilterTextBox;
        protected Label FilterLabel;
        protected CheckBox ShowHiddenCheckBox;
        protected CheckBox IgnoreAssemblerCheckBox;
        protected CheckBox RecipeNameOnlyFilterCheckBox;
        protected CheckBox AsIngredientCheckBox;
        protected CheckBox AsProductCheckBox;
        protected CheckBox AsFuelCheckBox;
        protected ComboBox QualitySelector;
        protected Label QualityLabel;
        protected FlowLayoutPanel QualityRow;
        protected Panel ItemIconPanel;
        protected Button AddSupplyButton;
        protected Button AddPassthroughButton;
        protected Button AddConsumerButton;
        protected Button AddSpoilButton;
        protected Button AddPlantButton;
        protected Button AddUnspoilButton;
        protected Button AddUnplantButton;
    }
}
