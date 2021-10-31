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
        public Action EndAction;

        private static readonly Color SelectedGroupButtonBGColor = Color.FromArgb(255,255,180,100);
        protected static readonly Color IRButtonDefaultColor = Color.FromArgb(255, 70, 70, 70);
        protected static readonly Color IRButtonHiddenColor = Color.FromArgb(255, 120, 0, 0);
        protected static readonly Color IRButtonNoAssemblerColor = Color.FromArgb(255, 100, 100, 0);

        protected const int IRPanelColumns = 10;
        protected const int IRPanelRows = 8; //8 = 294 height of IRPanel; 10 = 364 height //keep in mind that 8x10 is better for smoother scrolling
        private NFButton[,] IRButtons = new NFButton[IRPanelRows, IRPanelColumns];
        private List<NFButton> GroupButtons = new List<NFButton>();
        private Dictionary<Group, NFButton> GroupButtonLinks = new Dictionary<Group, NFButton>();
        private List<KeyValuePair<DataObjectBase, Color>[]> filteredIRRowsList = new List<KeyValuePair<DataObjectBase, Color>[]>(); //updated on every filter command & group selection. Represents the full set of items/recipes in the IRFlowPanel (the visible ones will come from this set based on scrolling), with each array being size 10 (#buttons/line). bool (value) is the 'use BW icon'
        protected int CurrentRow { get; private set; } //used to ensure we dont update twice when filtering or group change (once due to update request, second due to setting scroll bar value to 0)

        protected List<Group> SortedGroups;
        protected Group SelectedGroup; //provides some continuity between selections - if you last selected from the intermediates group for example, adding another recipe will select that group as the starting group
        private static Group StartingGroup;
        protected ProductionGraphViewer PGViewer;

        protected abstract ToolTip IRButtonToolTip { get; }
        private CustomToolTip GroupButtonToolTip;

        protected abstract List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList();
        protected abstract void IRButton_MouseUp(object sender, MouseEventArgs e);
        //protected abstract void IRButton_Hover(object sender, EventArgs e);

        public IRChooserPanel(ProductionGraphViewer parent, Point originPoint)
        {
            PGViewer = parent;
            this.DoubleBuffered = true;

            InitializeComponent();
            IRFlowPanel.Height = IRPanelRows * 35 + 12;

            GroupButtonToolTip = new CustomToolTip();

            IRPanelScrollBar.Minimum = 0;
            IRPanelScrollBar.Maximum = 0;
            IRPanelScrollBar.Enabled = false;
            IRPanelScrollBar.SmallChange = 1;
            IRPanelScrollBar.LargeChange = IRPanelRows;
            CurrentRow = 0;

            IRFlowPanel.MouseWheel += new MouseEventHandler(IRFlowPanel_MouseWheel);

            ShowHiddenCheckBox.Checked = Properties.Settings.Default.ShowHidden;
            IgnoreAssemblerCheckBox.Checked = Properties.Settings.Default.IgnoreAssemblerStatus;
            RecipeNameOnlyFilterCheckBox.Checked = Properties.Settings.Default.RecipeNameOnlyFilter;

            InitializeButtons();

            parent.Controls.Add(this);
            this.Location = originPoint;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            this.BringToFront();
            parent.PerformLayout();

            //set up the event handlers last so as not to cause unexpected calls when setting checked status ob checkboxes
            ShowHiddenCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);
            IgnoreAssemblerCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);

            FilterTextBox.Focus();
        }

        private void InitializeButtons()
        {
            //initialize the group buttons
            SortedGroups = GetSortedGroups();

            foreach(Group group in SortedGroups)
            {
                NFButton button = new NFButton();
                button.BackColor = Color.DimGray;
                button.UseVisualStyleBackColor = false;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.TabStop = false;
                button.Margin = new Padding(0);
                button.Size = new Size(64, 64);
                button.Image = new Bitmap(group.Icon, 58, 58);
                button.Tag = group;

                GroupButtonToolTip.SetToolTip(button, string.IsNullOrEmpty(group.FriendlyName) ? "-" : group.FriendlyName);

                button.Click += new EventHandler(GroupButton_Click);
                button.MouseHover += new EventHandler(GroupButton_MouseHover);
                button.MouseLeave += new EventHandler(GroupButton_MouseLeave);

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

                    button.MouseUp += new MouseEventHandler(IRButton_MouseUp);
                    button.MouseHover += new EventHandler(IRButton_MouseHover);
                    button.MouseLeave += new EventHandler(IRButton_MouseLeave);
                    IRButtons[row, column] = button;
                }
            }
            IRFlowPanel.SuspendLayout();
            IRFlowPanel.Controls.AddRange(IRButtons.Cast<Button>().ToArray());
            IRFlowPanel.ResumeLayout();

        }

        protected virtual List<Group> GetSortedGroups()
        {
            List<Group> groups = PGViewer.DCache.Groups.Values.ToList();
            groups.Sort();
            return groups;
        }

        protected void UpdateIRButtons(int startRow = 0, bool scrollOnly = false) //if scroll only, then we dont need to update the filtered set, just use what is there
        {
            if (IRFlowPanel.Visible)
            {
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
                IRButtonToolTip.RemoveAll();
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
                IRButtonToolTip.SetToolTip(b, string.IsNullOrEmpty(irObject.FriendlyName)? "-": irObject.FriendlyName);
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

        protected void SetSelectedGroup(Group sGroup, bool causeUpdate = true)
        {
            if (sGroup == null || !SortedGroups.Contains(sGroup)) //want to select the starting group, then update all buttons (including a possibility of group change)
            {
                sGroup = SortedGroups.Contains(StartingGroup) ? StartingGroup : SortedGroups[0];
                StartingGroup = sGroup;
                SelectedGroup = sGroup;
                UpdateIRButtons();
                //foreach (NFButton groupButton in GroupButtons)
                //    groupButton.BackColor = (groupButton.Tag as Group == SelectedGroup) ? SelectedGroupButtonBGColor : Color.DimGray;
            }
            else
            {
                foreach (NFButton groupButton in GroupButtons)
                    groupButton.BackColor = ((Group)(groupButton.Tag) == sGroup) ? SelectedGroupButtonBGColor : Color.DimGray;
                if (SelectedGroup != sGroup)
                {
                    StartingGroup = sGroup;
                    SelectedGroup = sGroup;
                    if (causeUpdate)
                        UpdateIRButtons();
                }
            }
        }

        protected void UpdateGroupButton(Group group, bool enabled) { GroupButtonLinks[group].Enabled = enabled; }

        private void GroupButton_Click(object sender, EventArgs e)
        {
            SetSelectedGroup((Group)((NFButton)sender).Tag);
        }

        private void GroupButton_MouseHover(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            GroupButtonToolTip.SetText(GroupButtonToolTip.GetToolTip(control));
            GroupButtonToolTip.Show(control, new Point(control.Width, 10));
        }
        private void GroupButton_MouseLeave(object sender, EventArgs e)
        {
            GroupButtonToolTip.Hide((Control)sender);
        }

        private void IRChooserPanel_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
            Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            Properties.Settings.Default.RecipeNameOnlyFilter = RecipeNameOnlyFilterCheckBox.Checked;
            Properties.Settings.Default.Save();
            EndAction?.Invoke();
            Dispose();
        }

        protected void FilterCheckBox_CheckedChanged(object sender, EventArgs e)
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

        private void IRFlowPanel_MouseWheel(object sender, MouseEventArgs e)
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

        internal virtual void IRButton_MouseHover(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            (IRButtonToolTip as CustomToolTip).SetText(IRButtonToolTip.GetToolTip(control));
            (IRButtonToolTip as CustomToolTip).Show(control, new Point(control.Width, 10));
        }
        private void IRButton_MouseLeave(object sender, EventArgs e)
        {
            IRButtonToolTip.Hide((Control)sender);
        }
    }

    public class ItemChooserPanel : IRChooserPanel
    {
        public Action<Item> CallbackMethod; //returns the selected item
        public void Show(Action<Item> callback, Action endAction = null) { CallbackMethod = callback; EndAction = endAction; }

        private ToolTip iToolTip = new CustomToolTip();
        protected override ToolTip IRButtonToolTip { get { return iToolTip; } }

        public ItemChooserPanel(ProductionGraphViewer parent, Point originPoint) : base(parent, originPoint)
        {
            SetSelectedGroup(null);
        }

        protected override List<Group> GetSortedGroups()
        {
            List<Group> groups = new List<Group>();
            foreach (Group group in PGViewer.DCache.Groups.Values)
            {
                int itemCount = 0;
                foreach (Subgroup sgroup in group.Subgroups)
                    itemCount += sgroup.Items.Count;
                if (itemCount > 0)
                    groups.Add(group);
            }
            groups.Sort();
            return groups;
        }

        protected override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList()
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
                    foreach (Item item in sgroup.Items.Where(n => n.LFriendlyName.Contains(filterString) || n.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1))
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
            if (filteredItemCount[SelectedGroup] == 0)
            {
                int selectedGroupIndex = 0;
                for (int i = 0; i < SortedGroups.Count; i++)
                    if (SortedGroups[i] == SelectedGroup)
                        selectedGroupIndex = i;
                for (int i = selectedGroupIndex; i >= 0; i--)
                    if (filteredItemCount[SortedGroups[i]] > 0)
                        alternateGroup = SortedGroups[i];
                if (alternateGroup == null)
                    for (int i = selectedGroupIndex; i < SortedGroups.Count; i++)
                        if (filteredItemCount[SortedGroups[i]] > 0)
                            alternateGroup = SortedGroups[i];
                if (alternateGroup == null)
                    alternateGroup = SelectedGroup;
            }
            SetSelectedGroup(alternateGroup == null? SelectedGroup : alternateGroup, false);

            //now the base class will take care of setting up the buttons based on the filtered items
            return filteredItems[SelectedGroup];
        }

        protected override void IRButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
                Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
                Properties.Settings.Default.Save();
                CallbackMethod((Item)((Button)sender).Tag);
                Dispose();
            }
        }
    }

    public class RecipeChooserPanel : IRChooserPanel
    {
        public Action<NodeType, Recipe> CallbackMethod; //returns the selected node type and the selected recipe (or null if not a recipe node)
        public void Show(Action<NodeType, Recipe> callback, Action endAction = null) { CallbackMethod = callback; EndAction = endAction; }
        protected Item KeyItem;
        protected fRange KeyItemTempRange;

        private ToolTip rToolTip = new RecipeToolTip();
        protected override ToolTip IRButtonToolTip { get { return rToolTip; } }

        public RecipeChooserPanel(ProductionGraphViewer parent, Point originPoint, Item item, fRange tempRange, bool includeSuppliers, bool includeConsumers) : base(parent, originPoint)
        {
            AsIngredientCheckBox.Checked = includeConsumers;
            AsProductCheckBox.Checked = includeSuppliers;
            ShowHiddenCheckBox.Text = "Show Disabled";

            AddConsumerButton.Click += new EventHandler(AddConsumerButton_Click);
            AddPassthroughButton.Click += new EventHandler(AddPassthroughButton_Click);
            AddSupplyButton.Click += new EventHandler(AddSupplyButton_Click);
            AsIngredientCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);
            AsProductCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);
            RecipeNameOnlyFilterCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);

            KeyItem = item;
            KeyItemTempRange = (includeSuppliers && includeConsumers) ? new fRange(0, 0, true) : tempRange; //cant use temp range if adding both s&c (its a disconnected node anyway)

            RecipeNameOnlyFilterCheckBox.Visible = true; 
            if (KeyItem == null)
            {
                OtherNodeOptionsTableLayoutPanel.Visible = false;
            }
            else
            {
                ItemIconPanel.Visible = true;
                ItemIconPanel.BackgroundImage = KeyItem.Icon;
                OtherNodeOptionsTableLayoutPanel.Visible = true;
                AddConsumerButton.Visible = includeConsumers;
                AddSupplyButton.Visible = includeSuppliers;
                if (!(includeConsumers && KeyItem.ConsumptionRecipes.Count > 0) && !(includeSuppliers && KeyItem.ProductionRecipes.Count > 0))
                {
                    GroupFlowPanel.Visible = false;
                    IRFlowPanel.Visible = false;
                    IRPanelScrollBar.Visible = false;
                    FilterTextBox.Visible = false;
                    FilterLabel.Visible = false;
                    ShowHiddenCheckBox.Visible = false;
                    IgnoreAssemblerCheckBox.Visible = false;
                    ItemIconPanel.Location = new Point(4, 4);

                }
                else if(includeConsumers && includeSuppliers)
                {
                    AsIngredientCheckBox.Visible = true;
                    AsProductCheckBox.Visible = true;
                }
            }
            SetSelectedGroup(null);
        }

        protected override List<Group> GetSortedGroups()
        {
            List<Group> groups = new List<Group>();
            foreach (Group group in PGViewer.DCache.Groups.Values)
            {
                int recipeCount = 0;
                foreach (Subgroup sgroup in group.Subgroups)
                    recipeCount += sgroup.Recipes.Count;
                if (recipeCount > 0)
                    groups.Add(group);
            }
            groups.Sort();
            return groups;
        }

        protected override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList()
        {
            //step 1: calculate the visible recipes for each group (those that pass filter & hidden status)
            string filterString = FilterTextBox.Text.ToLower();
            bool ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            bool checkRecipeIPs = !RecipeNameOnlyFilterCheckBox.Checked;
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
                    foreach (Recipe recipe in sgroup.Recipes.Where(r => ignoreItem || //filter for the items first (simpler and clears out most recipes if there is a key item provided)
                        (includeConsumers && r.IngredientSet.ContainsKey(KeyItem)) ||
                        (includeSuppliers && r.ProductSet.ContainsKey(KeyItem))))
                    {
                        //quick hidden / enabled assembler check (done prior to name check for speed)
                        if ((!recipe.Hidden || showHidden) && (recipe.HasEnabledAssemblers || ignoreAssemblerStatus))
                        {
                            //name check - have to check recipe name along with all ingredients and products (both friendly name and base name) - if selected
                            if (recipe.LFriendlyName.Contains(filterString) ||
                                recipe.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1 || (checkRecipeIPs && (
                                recipe.IngredientList.FirstOrDefault(i => i.LFriendlyName.Contains(filterString) || i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1) != null ||
                                recipe.ProductList.FirstOrDefault(i => i.LFriendlyName.Contains(filterString) || i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1) != null)))
                            {
                                //further check for temperature
                                if (KeyItemTempRange.Ignore ||
                                    (includeConsumers && recipe.IngredientTemperatureMap[KeyItem].Contains(KeyItemTempRange)) ||
                                    (includeSuppliers && KeyItemTempRange.Contains(recipe.ProductTemperatureMap[KeyItem])))
                                {
                                    //holy... so - we finally finished all the checks, eh? Well, throw it on the pile of recipes to show then.
                                    Color bgColor = (!recipe.Hidden ? (recipe.HasEnabledAssemblers ? IRButtonDefaultColor : IRButtonNoAssemblerColor) : IRButtonHiddenColor);
                                    recipeCounter++;
                                    recipeList.Add(new KeyValuePair<DataObjectBase, Color>(recipe, bgColor));
                                }
                            }
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
            }
            SetSelectedGroup(alternateGroup == null? SelectedGroup : alternateGroup, false);

            //now the base class will take care of setting up the buttons based on the filtered recipes
            return filteredRecipes[SelectedGroup];
        }

        protected override void IRButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) //select recipe
            {
                Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
                Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
                Properties.Settings.Default.Save();
                Recipe selectedRecipe = (Recipe)((Button)sender).Tag;
                CallbackMethod(NodeType.Recipe, selectedRecipe);

                if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
                    Dispose();
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
            Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
            Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            Properties.Settings.Default.Save();
            CallbackMethod(NodeType.Supplier, null);

            if((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
                Dispose();
        }

        private void AddConsumerButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
            Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            Properties.Settings.Default.Save();
            CallbackMethod(NodeType.Consumer, null);

            if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
                Dispose();
        }

        private void AddPassthroughButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
            Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            Properties.Settings.Default.Save();
            CallbackMethod(NodeType.Passthrough, null);

            if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
                Dispose();
        }

        internal override void IRButton_MouseHover(object sender, EventArgs e)
        {
            Control control = (Control)sender;

            int yoffset = -control.Location.Y + 16 + Math.Max(-100, Math.Min(0, 348 - RecipeToolTip.GetRecipeToolTipHeight((Recipe)((Button)sender).Tag)));

            (IRButtonToolTip as RecipeToolTip).SetRecipe((Recipe)((Button)sender).Tag);
            (IRButtonToolTip as RecipeToolTip).Show(control, new Point(control.Width, yoffset));
        }
    }

    public class NFButton : Button
    {
        public NFButton() : base() { this.SetStyle(ControlStyles.Selectable, false); }
        protected override bool ShowFocusCues { get { return false; } }
    }
}
