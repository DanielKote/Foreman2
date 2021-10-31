using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman
{
    public abstract partial class IRChooserPanel : UserControl
    {
        private static readonly Color SelectedGroupButtonBGColor = Color.FromArgb(255,255,180,100);
        internal static readonly Color IRButtonDefaultColor = Color.FromArgb(255, 40, 40, 40);
        internal static readonly Color IRButtonHiddenColor = Color.FromArgb(255, 100, 0, 0);
        internal static readonly Color IRButtonNoAssemblerColor = Color.FromArgb(255, 80, 80, 0);

        internal const int IRPanelColumns = 10;
        internal const int IRPanelRows = 8; //8 = 294 height of IRPanel; 10 = 364 height //keep in mind that 8x10 is better for smoother scrolling
        private NFButton[,] IRButtons = new NFButton[IRPanelRows, IRPanelColumns];
        private List<NFButton> GroupButtons = new List<NFButton>();
        private Dictionary<Group, NFButton> GroupButtonLinks = new Dictionary<Group, NFButton>();
        private List<KeyValuePair<DataObjectBase, Color>[]> filteredIRRowsList = new List<KeyValuePair<DataObjectBase, Color>[]>(); //updated on every filter command & group selection. Represents the full set of items/recipes in the IRFlowPanel (the visible ones will come from this set based on scrolling), with each array being size 10 (#buttons/line). bool (value) is the 'use BW icon'
        internal int CurrentRow { get; private set; } //used to ensure we dont update twice when filtering or group change (once due to update request, second due to setting scroll bar value to 0)

        internal List<Group> SortedGroups;
        internal Group SelectedGroup; //provides some continuity between selections - if you last selected from the intermediates group for example, adding another recipe will select that group as the starting group
        private static Group StartingGroup;
        internal ProductionGraphViewer PGViewer;

        public IRChooserPanel(ProductionGraphViewer parent)
        {
            InitializeComponent();
            IRFlowPanel.Height = IRPanelRows * 35 + 4; //both need to be set to this in order to size it appropriately
            IRPanelScrollBar.Height = IRPanelRows * 35 + 8;

            IRPanelScrollBar.Minimum = 0;
            IRPanelScrollBar.Maximum = 0;
            IRPanelScrollBar.Enabled = false;
            IRPanelScrollBar.SmallChange = 1;
            IRPanelScrollBar.LargeChange = IRPanelRows;
            CurrentRow = 0;

            IRFlowPanel.MouseWheel += new MouseEventHandler(IRFlowPanel_MouseWheel);

            ShowHiddenCheckBox.Checked = Properties.Settings.Default.ShowHidden;
            IgnoreAssemblerCheckBox.Checked = Properties.Settings.Default.IgnoreAssemblerStatus;

            InitializeIRButtons();

            PGViewer = parent;
            parent.Controls.Add(this);
            this.Location = new Point(10,10);
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.BringToFront();
            parent.PerformLayout();

            //set up the event handlers last so as not to cause unexpected calls when setting checked status ob checkboxes
            ShowHiddenCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);
            IgnoreAssemblerCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);
        }

        private void IRFlowPanel_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta < 0 && IRPanelScrollBar.Value <= (IRPanelScrollBar.Maximum - IRPanelScrollBar.LargeChange))
            {
                IRPanelScrollBar.Value++;
                UpdateIRButtons(IRPanelScrollBar.Value, true);
            }
            else if (e.Delta > 0 && IRPanelScrollBar.Value > 0)
            {
                IRPanelScrollBar.Value--;
                UpdateIRButtons(IRPanelScrollBar.Value, true);
            }
        }

        private void InitializeIRButtons()
        {
            //initialize the group buttons
            SortedGroups = DataCache.Groups.Values.ToList();
            SortedGroups.Sort();
            foreach(Group group in SortedGroups)
            {
                NFButton button = new NFButton();
                button.BackColor = Color.DimGray;
                button.BackgroundImageLayout = ImageLayout.Zoom;
                button.UseVisualStyleBackColor = false;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.TabStop = false;
                button.Click += new EventHandler(GroupButton_Click);
                button.Margin = new Padding(0);
                button.Size = new Size(64, 64);
                button.Image = group.Icon;
                button.Tag = group;
                GroupButtons.Add(button);
                GroupButtonLinks.Add(group, button);
            }
            GroupFlowPanel.SuspendLayout();
            GroupFlowPanel.Controls.AddRange(GroupButtons.ToArray());
            GroupFlowPanel.ResumeLayout();

            //initialize the item/recipe buttons
            for (int row = 0; row < IRPanelRows; row++)
            {
                for(int column = 0; column < IRPanelColumns; column++)
                {
                    NFButton button = new NFButton();
                    button.BackColor = Color.Gray;
                    button.BackgroundImageLayout = ImageLayout.Zoom;
                    button.UseVisualStyleBackColor = false;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 2;
                    button.TabStop = false;
                    button.ForeColor = Color.Gray;
                    button.BackColor = Color.DimGray;
                    button.Margin = new Padding(12);
                    button.Size = new Size(12, 12);
                    button.BackgroundImage = null;
                    button.Tag = null;
                    button.Enabled = false;

                    //button.Click += new EventHandler(IRButton_Click);
                    button.MouseUp += new MouseEventHandler(IRButton_MouseUp);
                    //button.MouseHover += new EventHandler(IRButton_Hover);
                    IRButtons[row, column] = button;
                }
            }
            IRFlowPanel.SuspendLayout();
            IRFlowPanel.Controls.AddRange(IRButtons.Cast<Button>().ToArray());
            IRFlowPanel.ResumeLayout();

        }

        private void SetIRButton(DataObjectBase irObject, Color bgColor, int row, int column)
        {
            NFButton b = IRButtons[row, column];
            if (irObject != null) //full
            {
                b.ForeColor = Color.Black;
                b.BackColor = bgColor; //Color.FromArgb(255,100,0,0) : Color.FromArgb(255,40,40,40);
                b.Margin = new Padding(1);
                b.Size = new Size(34, 34);
                b.BackgroundImage = irObject.Icon;
                b.Tag = irObject;
                b.Enabled = true;
            }
            else
            {
                b.ForeColor = Color.Gray;
                b.BackColor = Color.DimGray;
                b.Margin = new Padding(12);
                b.Size = new Size(12, 12);
                b.BackgroundImage = null;
                b.Tag = null;
                b.Enabled = false;
            }
        }

        private int upD = 0;
        internal void UpdateIRButtons(int startRow = 0, bool scrollOnly = false) //if scroll only, then we dont need to update the filtered set, just use what is there
        {
            Console.WriteLine("Chooser Panel UPDATE: " + upD++);

            //if we are actually changing the filtered list, then update it (through the GetSubgroupList)
            if (!scrollOnly)
            {
                filteredIRRowsList.Clear();
                int currentRow = 0;
                foreach (List<KeyValuePair<DataObjectBase, Color>> sgList in GetSubgroupList().Where(n => n.Count > 0))
                {
                    filteredIRRowsList.Add(new KeyValuePair<DataObjectBase, Color>[10]);
                    int currentColumn = 0;
                    foreach (KeyValuePair<DataObjectBase, Color> kvp in sgList)
                    {
                        if (currentColumn == IRPanelColumns)
                        {
                            filteredIRRowsList.Add(new KeyValuePair<DataObjectBase, Color>[10]);
                            currentColumn = 0;
                            currentRow++;
                        }
                        filteredIRRowsList[currentRow][currentColumn] = kvp;
                        currentColumn++;
                    }
                    currentRow++;
                }
                IRPanelScrollBar.Maximum = Math.Max(0, filteredIRRowsList.Count - 1);
                IRPanelScrollBar.Enabled = IRPanelScrollBar.Maximum >= IRPanelScrollBar.LargeChange;
            }
            CurrentRow = startRow;
            IRPanelScrollBar.Value = startRow;

            //update all the buttons to be based off of the filteredIRSet
            IRFlowPanel.SuspendLayout();
            IRFlowPanel.Visible = false;
            IRFlowPanel.Controls.Clear();
            for (int row = 0; row < IRPanelRows; row++)
                for (int column = 0; column < IRPanelColumns; column++)
                    SetIRButton(
                        (row + startRow < filteredIRRowsList.Count) ? filteredIRRowsList[row + startRow][column].Key : null,
                        (row + startRow < filteredIRRowsList.Count) ? filteredIRRowsList[row + startRow][column].Value : Color.DimGray,
                        row, column); //if we are beyond the bounds of the filtered rows, each button is set to null (disabled)
            IRFlowPanel.Controls.AddRange(IRButtons.Cast<Button>().ToArray());
            IRFlowPanel.Visible = true;
            IRFlowPanel.ResumeLayout();
        }


        bool exitScriptDone = false;
        internal void Exit()
        {
            if (!exitScriptDone)
            {
                exitScriptDone = true;

                Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
                Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
                Properties.Settings.Default.Save();
                Dispose();
            }
        }

        internal void SetSelectedGroup(Group sGroup, bool causeUpdate = true)
        {
            if (sGroup == null || !SortedGroups.Contains(sGroup)) //want to select the starting group, then update all buttons (including a possibility of group change)
            {
                sGroup = SortedGroups.Contains(StartingGroup) ? StartingGroup : SortedGroups[0];
                StartingGroup = sGroup;
                SelectedGroup = sGroup;
                UpdateIRButtons();
            }
            else
            {
                foreach (NFButton groupButton in GroupButtons)
                    groupButton.BackColor = (groupButton.Tag as Group == sGroup) ? SelectedGroupButtonBGColor : Color.DimGray;
                if (SelectedGroup != sGroup)
                {
                    StartingGroup = sGroup;
                    SelectedGroup = sGroup;
                    if (causeUpdate)
                        UpdateIRButtons();
                }
            }
        }

        internal void UpdateGroupButton(Group group, bool enabled) { GroupButtonLinks[group].Enabled = enabled; }

        internal abstract List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList();
        internal abstract void IRButton_MouseUp(object sender, MouseEventArgs e);
        //internal abstract void IRButton_Hover(object sender, EventArgs e);

        private void GroupButton_Click(object sender, EventArgs e)
        {
            SetSelectedGroup((sender as NFButton).Tag as Group);
        }

        private void IRChooserPanel_Leave(object sender, EventArgs e)
        {
            Exit();
        }

        internal void FilterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateIRButtons();
        }

        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateIRButtons();
        }

        private void IRPanelScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.NewValue != CurrentRow)
                UpdateIRButtons(e.NewValue, true);
        }
    }

    public class NFButton : Button
    {
        public NFButton() : base() { this.SetStyle(ControlStyles.Selectable, false); }
        protected override bool ShowFocusCues {  get { return false; } }
    }

    public class ItemChooserPanel : IRChooserPanel
    {
        public Action<Item> CallbackMethod; //returns the selected item
        public void Show(Action<Item> callback) { CallbackMethod = callback; }

        public ItemChooserPanel(ProductionGraphViewer parent) : base(parent)
        {
            SetSelectedGroup(null);
        }

        internal override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList()
        {
            //step 1: calculate the visible items within each group (used to disable any group button with 0 items, plus shift the selected group if it contains 0 items)
            string filterString = FilterTextBox.Text.ToLower();
            bool ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            bool showHidden = ShowHiddenCheckBox.Checked;
            Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>> filteredItems = new Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>>();
            Dictionary<Group, int> filteredItemCount = new Dictionary<Group, int>();
            foreach(Group group in SortedGroups)
            {
                int itemCounter = 0;
                List<List<KeyValuePair<DataObjectBase, Color>>> sgList = new List<List<KeyValuePair<DataObjectBase, Color>>>();
                foreach(Subgroup sgroup in group.Subgroups)
                {
                    List<KeyValuePair<DataObjectBase, Color>> itemList = new List<KeyValuePair<DataObjectBase, Color>>();
                    foreach (Item item in sgroup.Items.Where(n => n.LFriendlyName.Contains(filterString)))
                    {
                        bool visible =
                            (item.ConsumptionRecipes.FirstOrDefault(n => !n.Hidden) != null) ||
                            (item.ProductionRecipes.FirstOrDefault(n => !n.Hidden) != null);
                        bool validAssembler = 
                            (item.ConsumptionRecipes.FirstOrDefault(n => n.HasEnabledAssemblers) != null) ||
                            (item.ProductionRecipes.FirstOrDefault(n => n.HasEnabledAssemblers) != null);
                        Color bgColor = (visible ? (validAssembler ? IRButtonDefaultColor : IRButtonNoAssemblerColor) : IRButtonHiddenColor);

                        if ( (visible || showHidden) && (validAssembler || ignoreAssemblerStatus) )
                        {
                            itemCounter++;
                            itemList.Add(new KeyValuePair<DataObjectBase, Color>(item, bgColor));
                        }    
                    }
                    sgList.Add(itemList);
                }
                filteredItems.Add(group, sgList);
                filteredItemCount.Add(group, itemCounter);
                UpdateGroupButton(group, (itemCounter != 0));
            }

            //step 2: select working group (currently selected group, or if it has 0 items then the first group with >0 items to the left, then the first group with >0 items to the right, then itself)
            Group alternateGroup = null;
            if(filteredItemCount[SelectedGroup] == 0)
            {
                int selectedGroupIndex = 0;
                for (int i = 0; i < SortedGroups.Count; i++)
                    if (SortedGroups[i] == SelectedGroup)
                        selectedGroupIndex = i;
                for (int i = selectedGroupIndex; i >= 0; i--)
                    if (filteredItemCount[SortedGroups[i]] > 0)
                        alternateGroup = SortedGroups[i];
                if(alternateGroup == null)
                    for(int i = selectedGroupIndex; i < SortedGroups.Count; i++)
                        if (filteredItemCount[SortedGroups[i]] > 0)
                            alternateGroup = SortedGroups[i];
                if (alternateGroup == null)
                    alternateGroup = SelectedGroup;

                SetSelectedGroup(alternateGroup, false);
            }

            //now the base class will take care of setting up the buttons based on the filtered items
            return filteredItems[SelectedGroup];
        }

        internal override void IRButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                CallbackMethod((sender as NFButton).Tag as Item);
                Exit();
            }
        }
    }

    public class RecipeChooserPanel : IRChooserPanel
    {
        public Action<ProductionNode> CallbackMethod; //returns the created production node (or null if not created)
        internal Item KeyItem;
        public void Show(Action<ProductionNode> callback) { CallbackMethod = callback; }

        public RecipeChooserPanel(ProductionGraphViewer parent, Item item, bool includeSuppliers, bool includeConsumers) : base(parent)
        {
            AddConsumerButton.Click += new EventHandler(AddConsumerButton_Click);
            AddPassthroughButton.Click += new EventHandler(AddPassthroughButton_Click);
            AddSupplyButton.Click += new EventHandler(AddSupplyButton_Click);

            KeyItem = item;
            if (KeyItem != null)
                ItemIconPanel.BackgroundImage = KeyItem.Icon;

            if (includeConsumers || includeSuppliers)
            {
                IgnoreAssemblerCheckBox.Visible = true;
                ItemIconPanel.Visible = true;
                if (includeSuppliers && includeConsumers)
                {
                    AsIngredientCheckBox.Visible = true;
                    AsProductCheckBox.Visible = true;
                }
            }
            AsIngredientCheckBox.Checked = includeConsumers;
            AsProductCheckBox.Checked = includeSuppliers;
            ShowHiddenCheckBox.Text = "Show Disabled";

            AsIngredientCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);
            AsProductCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);

            SetSelectedGroup(null);
        }

        internal override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList()
        {
            //step 1: calculate the visible recipes for each group (those that pass filter & hidden status)
            string filterString = FilterTextBox.Text.ToLower();
            bool ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            bool showHidden = ShowHiddenCheckBox.Checked;
            bool includeSuppliers = AsProductCheckBox.Checked;
            bool includeConsumers = AsIngredientCheckBox.Checked;
            bool ignoreItem = KeyItem == null;
            Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>> filteredRecipes = new Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>>();
            Dictionary<Group, int> filteredRecipeCount = new Dictionary<Group, int>();
            foreach (Group group in SortedGroups)
            {
                int recipeCounter = 0;
                List<List<KeyValuePair<DataObjectBase, Color>>> sgList = new List<List<KeyValuePair<DataObjectBase, Color>>>();
                foreach (Subgroup sgroup in group.Subgroups)
                {
                    List<KeyValuePair<DataObjectBase, Color>> recipeList = new List<KeyValuePair<DataObjectBase, Color>>();
                    foreach (Recipe recipe in sgroup.Recipes.Where(n => 
                        n.LFriendlyName.Contains(filterString) && (
                            ignoreItem || 
                            (includeConsumers && n.Ingredients.ContainsKey(KeyItem)) ||
                            (includeSuppliers && n.Results.ContainsKey(KeyItem)) ) ))
                    {
                        if ((!recipe.Hidden || showHidden) && (recipe.HasEnabledAssemblers || ignoreAssemblerStatus))
                        {
                            Color bgColor = (!recipe.Hidden ? (recipe.HasEnabledAssemblers ? IRButtonDefaultColor : IRButtonNoAssemblerColor) : IRButtonHiddenColor);
                            recipeCounter++;
                            recipeList.Add(new KeyValuePair<DataObjectBase, Color>(recipe, bgColor));
                        }
                    }
                    sgList.Add(recipeList);
                }
                filteredRecipes.Add(group, sgList);
                filteredRecipeCount.Add(group, recipeCounter);
                UpdateGroupButton(group, (recipeCounter != 0));
            }

            //step 2: select working group (currently selected group, or if it has 0 recipes then the first group with >0 recipes to the left, then the first group with >0 recipes to the right, then itself)
            Group alternateGroup = null;
            if (filteredRecipeCount[SelectedGroup] == 0)
            {
                int selectedGroupIndex = 0;
                for (int i = 0; i < SortedGroups.Count; i++)
                    if (SortedGroups[i] == SelectedGroup)
                        selectedGroupIndex = i;
                for (int i = selectedGroupIndex; i >= 0; i--)
                    if (filteredRecipeCount[SortedGroups[i]] > 0)
                        alternateGroup = SortedGroups[i];
                if (alternateGroup == null)
                    for (int i = selectedGroupIndex; i < SortedGroups.Count; i++)
                        if (filteredRecipeCount[SortedGroups[i]] > 0)
                            alternateGroup = SortedGroups[i];
                if (alternateGroup == null)
                    alternateGroup = SelectedGroup;

                SetSelectedGroup(alternateGroup, false);
            }

            //now the base class will take care of setting up the buttons based on the filtered recipes
            return filteredRecipes[SelectedGroup];
        }

        internal override void IRButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) //select recipe
            {
                Recipe selectedRecipe = (sender as NFButton).Tag as Recipe;
                CallbackMethod(RecipeNode.Create(selectedRecipe, PGViewer.Graph));
                Exit();
            }
            else if(e.Button == MouseButtons.Right) //flip hidden status of recipe
            {
                Recipe selectedRecipe = (sender as NFButton).Tag as Recipe;
                selectedRecipe.Hidden = !selectedRecipe.Hidden;
                UpdateIRButtons();
            }
        }

        private void AddSupplyButton_Click(object sender, EventArgs e)
        {
            CallbackMethod(SupplyNode.Create(KeyItem, PGViewer.Graph));
            Exit();
        }

        private void AddPassthroughButton_Click(object sender, EventArgs e)
        {
            CallbackMethod(PassthroughNode.Create(KeyItem, PGViewer.Graph));
            Exit();
        }

        private void AddConsumerButton_Click(object sender, EventArgs e)
        {
            CallbackMethod(ConsumerNode.Create(KeyItem, PGViewer.Graph));
            Exit();
        }
    }
}
