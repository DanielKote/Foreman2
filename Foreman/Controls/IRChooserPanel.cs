using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public abstract partial class IRChooserPanel : UserControl {
        public enum ChooserPanelCloseReason {
            RecipeSelected,
            ItemSelected,
            AltNodeSelected,
            RequiresItemSelection,
            Cancelled,
        }
        public event EventHandler<PanelChooserCloseArgs>? PanelClosed;
        internal ChooserPanelCloseReason panelCloseReason;

        private static readonly Color SelectedGroupButtonBGColor = Color.SandyBrown;
        protected static readonly Color IRButtonDefaultColor = Color.FromArgb(255, 70, 70, 70);
        protected static readonly Color IRButtonHiddenColor = Color.FromArgb(255, 120, 0, 0);
        protected static readonly Color IRButtonNoAssemblerColor = Color.FromArgb(255, 100, 100, 0);
        protected static readonly Color IRButtonUnavailableColor = Color.FromArgb(255, 170, 10, 160);


        private NFButton[,] IRButtons;
        private List<NFButton> GroupButtons = new List<NFButton>();
        private Dictionary<Group, NFButton> GroupButtonLinks = new Dictionary<Group, NFButton>();
        private List<KeyValuePair<DataObjectBase, Color>[]> filteredIRRowsList = new List<KeyValuePair<DataObjectBase, Color>[]>(); //updated on every filter command & group selection. Represents the full set of items/recipes in the IRFlowPanel (the visible ones will come from this set based on scrolling), with each array being size 10 (#buttons/line). bool (value) is the 'use BW icon'
        protected int CurrentRow { get; private set; } //used to ensure we dont update twice when filtering or group change (once due to update request, second due to setting scroll bar value to 0)

        protected List<Group>? SortedGroups;
        protected Group? SelectedGroup; //provides some continuity between selections - if you last selected from the intermediates group for example, adding another recipe will select that group as the starting group
        private static Group? StartingGroup;
        protected ProductionGraphViewer PGViewer;

        protected abstract ToolTip IRButtonToolTip { get; }
        private CustomToolTip GroupButtonToolTip;

        protected abstract List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList();
        protected abstract void IRButton_MouseUp(object? sender, MouseEventArgs e);
        //protected abstract void IRButton_Hover(object? sender, EventArgs e);

        protected bool ShowUnavailable { get; private set; }

        public IRChooserPanel(ProductionGraphViewer parent, Point originPoint) {
            PGViewer = parent;
            this.DoubleBuffered = true;
            this.ShowUnavailable = Properties.Settings.Default.ShowUnavailable;
            panelCloseReason = ChooserPanelCloseReason.Cancelled;

            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.Disposed += IRChooserPanel_Disposed;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            IRButtons = new NFButton[IRTable.ColumnCount - 1, IRTable.RowCount];

            GroupButtonToolTip = new CustomToolTip();

            IRScrollBar.Minimum = 0;
            IRScrollBar.Maximum = 0;
            IRScrollBar.Enabled = false;
            IRScrollBar.SmallChange = 1;
            IRScrollBar.LargeChange = IRTable.RowCount;
            CurrentRow = 0;

            IRTable.MouseWheel += new MouseEventHandler(IRFlowPanel_MouseWheel);

            ShowHiddenCheckBox.Checked = Properties.Settings.Default.ShowHidden;
            IgnoreAssemblerCheckBox.Checked = Properties.Settings.Default.IgnoreAssemblerStatus;
            RecipeNameOnlyFilterCheckBox.Checked = Properties.Settings.Default.RecipeNameOnlyFilter;

            this.Location = originPoint;
        }

        public new void Show() {
            InitializeButtons();
            SetSelectedGroup(null);

            //set up the event handlers last so as not to cause unexpected calls when setting checked status ob checkboxes
            ShowHiddenCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);
            IgnoreAssemblerCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);

            PGViewer.Controls.Add(this);
            this.BringToFront();
            PGViewer.PerformLayout();
            this.Focus();
            FilterTextBox.Focus();
        }

        //-----------------------------------------------------------------------------------------------------Button initialization & update

        private void InitializeButtons() {
            //initialize the group buttons
            SortedGroups = GetSortedGroups();

            GroupTable.SuspendLayout();

            //add any extra rows to handle the amount of sorted groups
            for (int i = 0; i < (SortedGroups.Count - 1) / GroupTable.ColumnCount; i++)
                GroupTable.RowStyles.Add(new RowStyle(GroupTable.RowStyles[0].SizeType, GroupTable.RowStyles[0].Height));

            //add in the group buttons
            for (int i = 0; i < SortedGroups.Count; i++) {
                NFButton button = new NFButton();
                button.BackColor = Color.DimGray;
                button.UseVisualStyleBackColor = false;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.TabStop = false;
                button.Margin = new Padding(0);
                button.Size = new Size(1, 64);
                button.Dock = DockStyle.Fill;
                button.BackgroundImage = SortedGroups[i].Icon;
                button.BackgroundImageLayout = ImageLayout.Center;
                button.Tag = SortedGroups[i];

                GroupButtonToolTip.SetToolTip(button, string.IsNullOrEmpty(SortedGroups[i].FriendlyName) ? "-" : SortedGroups[i].FriendlyName);

                button.Click += new EventHandler(GroupButton_Click);
                button.MouseHover += new EventHandler(GroupButton_MouseHover);
                button.MouseLeave += new EventHandler(GroupButton_MouseLeave);

                GroupButtons.Add(button);
                GroupButtonLinks.Add(SortedGroups[i], button);

                GroupTable.Controls.Add(button, i % GroupTable.ColumnCount, i / GroupTable.ColumnCount);
            }
            GroupTable.ResumeLayout();

            //initialize the item/recipe buttons
            IRTable.SuspendLayout();
            for (int column = 0; column < IRButtons.GetLength(0); column++) {
                for (int row = 0; row < IRButtons.GetLength(1); row++) {
                    NFButton button = new NFButton();
                    button.BackgroundImageLayout = ImageLayout.Zoom;
                    button.UseVisualStyleBackColor = false;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = Math.Max(1, IRTable.Width / (IRButtons.GetLength(0) * 24));
                    button.TabStop = false;
                    button.ForeColor = Color.Gray;
                    button.BackColor = Color.DimGray;
                    button.Margin = new Padding(1);
                    button.Size = new Size(40, 40);
                    button.Dock = DockStyle.Fill;
                    button.BackgroundImage = null;
                    button.Tag = null;
                    button.Enabled = false;

                    button.MouseUp += new MouseEventHandler(IRButton_MouseUp);
                    button.MouseHover += new EventHandler(IRButton_MouseHover);
                    button.MouseLeave += new EventHandler(IRButton_MouseLeave);
                    IRButtons[column, row] = button;

                    IRTable.Controls.Add(button, column, row);
                }
            }
            IRTable.ResumeLayout();
        }

        protected abstract List<Group> GetSortedGroups();

        private long updateID = 0;
        protected async void UpdateIRButtons(int startRow = 0, bool scrollOnly = false) //if scroll only, then we dont need to update the filtered set, just use what is there
        {
            long currentID = ++updateID;

            await Task.Run(() => {
                //if we are actually changing the filtered list, then update it (through the GetSubgroupList)
                if (!scrollOnly) {
                    filteredIRRowsList.Clear();
                    int currentRow = 0;
                    foreach (List<KeyValuePair<DataObjectBase, Color>> sgList in GetSubgroupList().Where(n => n.Count > 0)) {
                        filteredIRRowsList.Add(new KeyValuePair<DataObjectBase, Color>[10]);
                        int currentColumn = 0;
                        foreach (KeyValuePair<DataObjectBase, Color> kvp in sgList) {
                            if (currentColumn == IRButtons.GetLength(0)) {
                                filteredIRRowsList.Add(new KeyValuePair<DataObjectBase, Color>[10]);
                                currentColumn = 0;
                                currentRow++;
                            }
                            filteredIRRowsList[currentRow][currentColumn] = kvp;
                            currentColumn++;
                        }
                        currentRow++;
                    }
                }

                bool so = scrollOnly;
                this.UIThread(delegate {
                    if (currentID != updateID)
                        return;

                    if (!so) {
                        IRScrollBar.Maximum = Math.Max(0, filteredIRRowsList.Count - 1);
                        IRScrollBar.Enabled = IRScrollBar.Maximum >= IRScrollBar.LargeChange;
                    }
                    CurrentRow = startRow;
                    IRScrollBar.Value = startRow;

                    IRTable.ResumeLayout();
                    IRTable.SuspendLayout();
                });

                //update all the buttons to be based off of the filteredIRSet
                for (int column = 0; column < IRButtons.GetLength(0); column++) {
                    for (int row = 0; row < IRButtons.GetLength(1); row++) {
                        if (currentID != updateID)
                            return;

                        int c = column;
                        int r = row;
                        this.UIThread(delegate {
                            if (currentID != updateID)
                                return;

                            DataObjectBase? irObject = (r + startRow < filteredIRRowsList.Count) ? filteredIRRowsList[r + startRow][c].Key : null;
                            NFButton b = IRButtons[c, r];
                            if (irObject != null) //full
                            {

                                b.ForeColor = Color.Black;
                                b.BackColor = (r + startRow < filteredIRRowsList.Count) ? filteredIRRowsList[r + startRow][c].Value : Color.DimGray;
                                b.BackgroundImage = irObject.Icon;
                                b.Tag = irObject;
                                b.Enabled = true;
                                IRButtonToolTip.SetToolTip(b, string.IsNullOrEmpty(irObject.FriendlyName) ? "-" : irObject.FriendlyName);
                            } else {
                                b.ForeColor = Color.Gray;
                                b.BackColor = Color.DimGray;
                                b.BackgroundImage = null;
                                b.Tag = null;
                                b.Enabled = false;
                            }
                        });
                    }
                }
            });
            this.UIThread(delegate {
                if (currentID != updateID)
                    return;
                IRTable.ResumeLayout();
            });
        }

        protected void SetSelectedGroup(Group? sGroup, bool causeUpdate = true) {
            if (SortedGroups is not null && StartingGroup is not null && (sGroup is null || !SortedGroups.Contains(sGroup))) //want to select the starting group, then update all buttons (including a possibility of group change)
            {
                sGroup = SortedGroups.Contains(StartingGroup) is true ? StartingGroup : SortedGroups[0];
                StartingGroup = sGroup;
                SelectedGroup = sGroup;
                UpdateIRButtons();
            } else {
                foreach (NFButton groupButton in GroupButtons)
                    if (groupButton.Tag is Group grp)
                        groupButton.BackColor = grp == sGroup ? SelectedGroupButtonBGColor : Color.DimGray;
                if (SelectedGroup != sGroup) {
                    StartingGroup = sGroup;
                    SelectedGroup = sGroup;
                    if (causeUpdate)
                        UpdateIRButtons();
                }
            }
        }

        protected void UpdateGroupButton(Group group, bool enabled) {
            this.UIThread(delegate {
                GroupButtonLinks[group].Enabled = enabled;
            });
        }

        //-----------------------------------------------------------------------------------------------------Group Button events

        private void GroupButton_Click(object? sender, EventArgs e) {
            if (sender is NFButton btn && btn.Tag is Group grp)
                SetSelectedGroup(grp);
        }

        private void GroupButton_MouseHover(object? sender, EventArgs e) {
            if (sender is not Control control)
                return;
            GroupButtonToolTip.SetText(GroupButtonToolTip.GetToolTip(control) ?? "");
            GroupButtonToolTip.Show(control, new Point(control.Width, 10));
        }

        private void GroupButton_MouseLeave(object? sender, EventArgs e) {
            if (sender is Control ctrl)
                GroupButtonToolTip.Hide(ctrl);
        }

        //-----------------------------------------------------------------------------------------------------IR button events (including scrolling)

        private void IRPanelScrollBar_Scroll(object? sender, ScrollEventArgs e) {
            if (e.NewValue != CurrentRow)
                UpdateIRButtons(e.NewValue, true);
        }

        private void IRFlowPanel_MouseWheel(object? sender, MouseEventArgs e) {
            if (e.Delta < 0 && IRScrollBar.Value <= (IRScrollBar.Maximum - IRScrollBar.LargeChange)) {
                IRScrollBar.Value++;
                UpdateIRButtons(IRScrollBar.Value, true);
            } else if (e.Delta > 0 && IRScrollBar.Value > 0) {
                IRScrollBar.Value--;
                UpdateIRButtons(IRScrollBar.Value, true);
            }
        }

        internal virtual void IRButton_MouseHover(object? sender, EventArgs e) {
            if (sender is not Control control || IRButtonToolTip is not CustomToolTip ctt)
                return;
            ctt.SetText(IRButtonToolTip.GetToolTip(control) ?? "");
            ctt.Show(control, new Point(control.Width, 10));
        }
        private void IRButton_MouseLeave(object? sender, EventArgs e) {
            if (sender is Control ctrl)
                IRButtonToolTip.Hide(ctrl);
        }

        //-----------------------------------------------------------------------------------------------------Filter

        protected void FilterCheckBox_CheckedChanged(object? sender, EventArgs e) {
            UpdateIRButtons();
        }

        private void FilterTextBox_TextChanged(object? sender, EventArgs e) {
            UpdateIRButtons();
        }

        //-----------------------------------------------------------------------------------------------------Closing functions

        private void IRChooserPanel_Leave(object? sender, EventArgs e) {
            Dispose();
        }

        protected virtual void IRChooserPanel_Disposed(object? sender, EventArgs e) {
            Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
            Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            Properties.Settings.Default.RecipeNameOnlyFilter = RecipeNameOnlyFilterCheckBox.Checked;
            Properties.Settings.Default.Save();
            PanelClosed?.Invoke(this, new PanelChooserCloseArgs(panelCloseReason));
        }

        private void MainTable_Paint(object? sender, PaintEventArgs e) {

        }

        private void comboBox1_SelectedIndexChanged(object? sender, EventArgs e) {

        }
    }

    public class ItemChooserPanel : IRChooserPanel {
        public event EventHandler<ItemRequestArgs>? ItemRequested;

        private ToolTip iToolTip = new CustomToolTip();
        protected override ToolTip IRButtonToolTip { get { return iToolTip; } }
        private ItemQualityPair selectedItem;
        private DataCache DCache;

        private HashSet<Item>? requestedItemList;
        private bool showAllItems;

        private List<Quality> qualitySelectorIndexSet;

        public ItemChooserPanel(ProductionGraphViewer parent, Point originPoint, IReadOnlyCollection<Item>? itemList = null, Quality? itemQuality = null) : base(parent, originPoint) {
            showAllItems = (itemList == null);
            DCache = parent.DCache ?? throw new InvalidOperationException("Data cache is not loaded.");
            qualitySelectorIndexSet = new List<Quality>();

            if (itemQuality == null) {
                QualitySelectorTable.Visible = true;
                foreach (Quality quality in DCache.AvailableQualities.Where(q => q.Enabled)) {
                    QualitySelector.Items.Add(quality.FriendlyName);
                    qualitySelectorIndexSet.Add(quality);
                }

                if (QualitySelector.Items.Count == 1)
                    QualitySelector.Enabled = false;
            } else {
                QualitySelector.Items.Add(itemQuality.FriendlyName);
                qualitySelectorIndexSet.Add(itemQuality);
                QualitySelector.Enabled = false;
            }
            QualitySelector.SelectedIndex = 0;

            if (!showAllItems && itemList is not null)
                requestedItemList = [.. itemList];
        }

        protected override void IRChooserPanel_Disposed(object? sender, EventArgs e) {
            base.IRChooserPanel_Disposed(sender, e);
            if (selectedItem)
                ItemRequested?.Invoke(this, new ItemRequestArgs(selectedItem));
        }

        protected override List<Group> GetSortedGroups() {
            List<Group> groups = new List<Group>();

            if (showAllItems) {
                foreach (Group group in ShowUnavailable ? DCache.Groups.Values : DCache.AvailableGroups) {
                    int itemCount = 0;
                    foreach (Subgroup sgroup in group.Subgroups) {
                        if (showAllItems)
                            itemCount += ShowUnavailable ? sgroup.Items.Count : sgroup.Items.Count(i => i.Available);
                    }
                    if (itemCount > 0)
                        groups.Add(group);
                }
            } else {
                foreach (Item item in requestedItemList ?? []) {
                    if ((ShowUnavailable || item.Available) && item.MySubgroup.MyGroup is Group g && !groups.Contains(g))
                        groups.Add(g);
                }
            }
            groups.Sort();
            return groups;
        }

        protected override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList() {
            //step 1: calculate the visible items within each group (used to disable any group button with 0 items, plus shift the selected group if it contains 0 items)
            string filterString = FilterTextBox.Text.ToLower();
            bool ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            bool showHidden = ShowHiddenCheckBox.Checked;

            Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>> filteredItems = new Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>>();
            Dictionary<Group, int> filteredItemCount = new Dictionary<Group, int>();
            foreach (Group group in SortedGroups ?? []) {
                int itemCounter = 0;
                List<List<KeyValuePair<DataObjectBase, Color>>> sgList = new List<List<KeyValuePair<DataObjectBase, Color>>>();
                foreach (Subgroup sgroup in group.Subgroups) {
                    List<KeyValuePair<DataObjectBase, Color>> itemList = new List<KeyValuePair<DataObjectBase, Color>>();
                    foreach (Item item in sgroup.Items.Where(i => ((ShowUnavailable || i.Available) && (i.LFriendlyName.Contains(filterString) || i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1)))) {
                        if (!showAllItems && (!requestedItemList?.Contains(item)) is true)
                            continue;

                        bool visible = (ShowUnavailable || item.Available) &&
                            ((item.ConsumptionRecipes.Any(r => r.Enabled && (ShowUnavailable || r.Available))) ||
                            (item.ProductionRecipes.Any(r => r.Enabled && (ShowUnavailable || r.Available))));

                        bool validAssembler =
                            (item.ConsumptionRecipes.Any(r => r.Enabled && (ShowUnavailable || r.Available) && r.Assemblers.Any(a => a.Enabled && (ShowUnavailable || a.Available)))) ||
                            (item.ProductionRecipes.Any(r => r.Enabled && (ShowUnavailable || r.Available) && r.Assemblers.Any(a => a.Enabled && (ShowUnavailable || a.Available))));


                        Color bgColor = (visible && item.Available) ? validAssembler ? IRButtonDefaultColor : IRButtonNoAssemblerColor : IRButtonHiddenColor;

                        if ((visible || showHidden) && (validAssembler || ignoreAssemblerStatus)) {
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
            Group? alternateGroup = null;
            if (SelectedGroup is not null && SortedGroups is not null && filteredItemCount[SelectedGroup] == 0) {
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
            SetSelectedGroup(alternateGroup == null ? SelectedGroup : alternateGroup, false);

            //now the base class will take care of setting up the buttons based on the filtered items
            return SelectedGroup is not null ? filteredItems[SelectedGroup] : new();
        }

        protected override void IRButton_MouseUp(object? sender, MouseEventArgs e) {
            if (sender is Button b && b.Tag is Item i && e.Button == MouseButtons.Left) {
                panelCloseReason = ChooserPanelCloseReason.ItemSelected;
                selectedItem = new ItemQualityPair(i, qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                Dispose();
            }
        }
    }

    public class RecipeChooserPanel : IRChooserPanel {
        public event EventHandler<RecipeRequestArgs>? RecipeRequested;

        protected ItemQualityPair KeyItem;
        protected bool isDefaultQuality;
        protected fRange KeyItemTempRange;
        protected DataCache DCache;

        private ToolTip rToolTip = new RecipeToolTip();
        protected override ToolTip IRButtonToolTip { get { return rToolTip; } }

        private List<Quality> qualitySelectorIndexSet;

        public RecipeChooserPanel(ProductionGraphViewer parent, Point originPoint, ItemQualityPair item, fRange tempRange, NewNodeType nodeType) : base(parent, originPoint) {
            DCache = parent.DCache ?? throw new InvalidOperationException("Data cache is not loaded.");
            qualitySelectorIndexSet = new List<Quality>();

            if (!item) {
                QualitySelectorTable.Visible = true;
                foreach (Quality quality in DCache.AvailableQualities.Where(q => q.Enabled)) {
                    QualitySelector.Items.Add(quality.FriendlyName);
                    qualitySelectorIndexSet.Add(quality);

                }

                if (QualitySelector.Items.Count == 1)
                    QualitySelector.Enabled = false;
            } else if (item is { Quality: Quality fixedItemQuality }) {
                QualitySelector.Items.Add(fixedItemQuality.FriendlyName);
                qualitySelectorIndexSet.Add(fixedItemQuality);
                QualitySelector.Enabled = false;
            }
            QualitySelector.SelectedIndex = 0;

            bool asIngredient = (nodeType == NewNodeType.Consumer || nodeType == NewNodeType.Disconnected);
            bool asProduct = (nodeType == NewNodeType.Supplier || nodeType == NewNodeType.Disconnected);

            AsIngredientCheckBox.Checked = asIngredient;
            AsProductCheckBox.Checked = asProduct;
            ShowHiddenCheckBox.Text = "Show Disabled";

            AddConsumerButton.Click += AddConsumerButton_Click;
            AddPassthroughButton.Click += AddPassthroughButton_Click;
            AddSupplyButton.Click += AddSupplyButton_Click;
            AddSpoilButton.Click += AddSpoilButton_Click;
            AddUnspoilButton.Click += AddUnSpoilButton_Click;
            AddPlantButton.Click += AddPlantButton_Click;
            AddUnplantButton.Click += AddUnPlantButton_Click;

            AsIngredientCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
            AsProductCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
            AsFuelCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
            RecipeNameOnlyFilterCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);

            KeyItem = item;
            KeyItemTempRange = (nodeType == NewNodeType.Disconnected) ? new fRange(0, 0, true) : tempRange; //cant use temp range if its a disconnected node
            isDefaultQuality = !KeyItem || (KeyItem is { Quality: Quality keyItemQuality } && keyItemQuality == DCache.DefaultQuality);

            RecipeNameOnlyFilterCheckBox.Visible = true;
            if (KeyItem is { Item: Item keyItem, Quality: Quality keyQuality }) {
                ItemIconPanel.Visible = true;
                ItemIconPanel.BackgroundImage = KeyItem.Icon;
                OtherNodeOptionsATable.Visible = true;
                AddConsumerButton.Visible = asIngredient;
                AddSupplyButton.Visible = asProduct;

                OtherNodeOptionsBTable.Visible = true;
                AddSpoilButton.Visible = asIngredient && keyItem.SpoilResult != null;
                AddUnspoilButton.Visible = asProduct && keyItem.SpoilOrigins.Count > 0;
                AddPlantButton.Visible = asIngredient && keyItem.PlantResult != null;
                AddUnplantButton.Visible = asProduct && isDefaultQuality && keyItem.PlantOrigins.Count > 0;
                int totalVisible = (AddSpoilButton.Visible ? 1 : 0) + (AddUnspoilButton.Visible ? 1 : 0) + (AddPlantButton.Visible ? 1 : 0) + (AddUnplantButton.Visible ? 1 : 0);
                OtherNodeOptionsBTable.Visible = totalVisible > 0;

                bool hasConsumptionRecipes = Properties.Settings.Default.ShowUnavailable ? keyItem.ConsumptionRecipes.Count > 0 : keyItem.ConsumptionRecipes.Count(r => r.Available) > 0;
                bool hasFuelConsumptionRecipes = isDefaultQuality && (keyItem.FuelsEntities.Any(a => (a is Assembler assembler) && assembler.Enabled && assembler.Recipes.Any(r => r.Enabled)));
                bool hasProductionRecipes = Properties.Settings.Default.ShowUnavailable ? keyItem.ProductionRecipes.Count > 0 : keyItem.ProductionRecipes.Count(r => r.Available) > 0;
                bool hasFuelProductionRecipes = isDefaultQuality && (keyItem.FuelOrigin != null && keyItem.FuelOrigin.FuelsEntities.Any(a => (a is Assembler assembler) && assembler.Enabled && assembler.Recipes.Any(r => r.Enabled)));

                if (!(asIngredient && (hasConsumptionRecipes || hasFuelConsumptionRecipes)) && !(asProduct && (hasProductionRecipes || hasFuelProductionRecipes))) //no valid recipes
                {
                    GroupTable.Visible = false;
                    IRTable.Visible = false;
                    IRScrollBar.Visible = false;
                    FilterTextBox.Visible = false;
                    FilterLabel.Visible = false;
                    RecipeNameOnlyFilterCheckBox.Visible = false;
                    ShowHiddenCheckBox.Visible = false;
                    IgnoreAssemblerCheckBox.Visible = false;
                    ItemIconPanel.Location = new Point(4, 4);
                } else if (asIngredient && asProduct) {
                    AsFuelCheckBox.Visible = (asIngredient && hasFuelConsumptionRecipes) || (asProduct && hasFuelProductionRecipes);
                    AsIngredientCheckBox.Visible = true;
                    AsProductCheckBox.Visible = true;
                } else if (asIngredient) {
                    AsFuelCheckBox.Visible = (asIngredient && keyItem.FuelsEntities.Count > 0);
                }
            } else {
                OtherNodeOptionsATable.Visible = false;
                OtherNodeOptionsBTable.Visible = false;
            }
        }

        private static bool RecipeMatchesKeyItem(Recipe recipe, Item keyItem, bool includeConsumers, bool includeSuppliers, bool includeFuel, bool ignoreAssemblerStatus, fRange keyItemTempRange) {
            return (includeConsumers && recipe.IngredientSet.ContainsKey(keyItem) && (keyItemTempRange.Ignore || recipe.IngredientTemperatureMap[keyItem].Contains(keyItemTempRange))) ||
                (includeSuppliers && recipe.ProductSet.ContainsKey(keyItem) && (keyItemTempRange.Ignore || keyItemTempRange.Contains(recipe.ProductTemperatureMap[keyItem]))) ||
                (includeConsumers && includeFuel && keyItem.FuelsEntities.Count > 0 && recipe.Assemblers.Any(a => a.Fuels.Contains(keyItem) && (a.Enabled || ignoreAssemblerStatus))) ||
                (includeSuppliers && includeFuel && keyItem.FuelOrigin is Item fuelOrigin && recipe.Assemblers.Any(a => a.Fuels.Contains(fuelOrigin) && (a.Enabled || ignoreAssemblerStatus)));
        }

        protected override List<Group> GetSortedGroups() {
            List<Group> groups = new List<Group>();
            foreach (Group group in ShowUnavailable ? DCache.Groups.Values : DCache.AvailableGroups) {
                int recipeCount = 0;
                foreach (Subgroup sgroup in group.Subgroups)
                    recipeCount += ShowUnavailable ? sgroup.Recipes.Count : sgroup.Recipes.Count(r => r.Available);
                if (recipeCount > 0)
                    groups.Add(group);
            }
            groups.Sort();
            return groups;
        }

        protected override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList() {
            //step 1: calculate the visible recipes for each group (those that pass filter & hidden status)
            string filterString = FilterTextBox.Text.ToLower();
            bool ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            bool checkRecipeIPs = !RecipeNameOnlyFilterCheckBox.Checked;
            bool showHidden = ShowHiddenCheckBox.Checked;
            bool includeSuppliers = AsProductCheckBox.Checked;
            bool includeConsumers = AsIngredientCheckBox.Checked;
            bool includeFuel = AsFuelCheckBox.Checked && isDefaultQuality;
            bool ignoreItem = !KeyItem;
            Item? filterKeyItem = KeyItem is { Item: Item filterItem } ? filterItem : null;

            Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>> filteredRecipes = new Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>>();
            Dictionary<Group, int> filteredRecipeCount = new Dictionary<Group, int>();
            foreach (Group group in SortedGroups ?? []) {
                int recipeCounter = 0;
                List<List<KeyValuePair<DataObjectBase, Color>>> sgList = new List<List<KeyValuePair<DataObjectBase, Color>>>();
                foreach (Subgroup sgroup in group.Subgroups) {
                    List<KeyValuePair<DataObjectBase, Color>> recipeList = new List<KeyValuePair<DataObjectBase, Color>>();
                    //filter recipes... I tried to break up the filter into several parts to prevent this from being one GIANT '.where' call
                    foreach (Recipe recipe in sgroup.Recipes.Where(r => ignoreItem || (filterKeyItem is Item keyItemForFilter && RecipeMatchesKeyItem(r, keyItemForFilter, includeConsumers, includeSuppliers, includeFuel, ignoreAssemblerStatus, KeyItemTempRange)))) {
                        //quick hidden / enabled / available assembler check (done prior to name check for speed)
                        if ((recipe.Enabled || showHidden) && (recipe.Assemblers.Any(a => a.Enabled) || ignoreAssemblerStatus) && (recipe.Available || ShowUnavailable)) {
                            //name check - have to check recipe name along with all ingredients and products (both friendly name and base name) - if selected
                            if (recipe.LFriendlyName.Contains(filterString) ||
                                recipe.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1 || (checkRecipeIPs && (
                                recipe.IngredientList.Any(i => i.LFriendlyName.Contains(filterString) || i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1) ||
                                recipe.ProductList.Any(i => i.LFriendlyName.Contains(filterString) || i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1)))) {
                                //holy... so - we finally finished all the checks, eh? Well, throw it on the pile of recipes to show then.
                                Color bgColor = !recipe.Enabled ? IRButtonHiddenColor :
                                    (!recipe.Available || !recipe.Assemblers.Any(a => a.Available)) ? IRButtonUnavailableColor :
                                    !recipe.Assemblers.Any(a => a.Enabled) ? IRButtonNoAssemblerColor : IRButtonDefaultColor;
                                recipeCounter++;
                                recipeList.Add(new KeyValuePair<DataObjectBase, Color>(recipe, bgColor));
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
            Group? alternateGroup = null;
            if (SelectedGroup is not null && SortedGroups is not null && filteredRecipeCount[SelectedGroup] == 0) {
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
            SetSelectedGroup(alternateGroup == null ? SelectedGroup : alternateGroup, false);

            //now the base class will take care of setting up the buttons based on the filtered recipes
            return SelectedGroup is not null ? filteredRecipes[SelectedGroup] : new();
        }

        protected override void IRButton_MouseUp(object? sender, MouseEventArgs e) {
            if (sender is Button btn && btn.Tag is Recipe sRecipe && e.Button == MouseButtons.Left) //select recipe
            {
                RecipeRequested?.Invoke(this, new RecipeRequestArgs(new RecipeQualityPair(sRecipe, qualitySelectorIndexSet[QualitySelector.SelectedIndex])));

                if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                    panelCloseReason = ChooserPanelCloseReason.RecipeSelected;
                    Dispose();
                }
            } else if (sender is NFButton nfBtn && nfBtn.Tag is Recipe selectedRecipe && e.Button == MouseButtons.Right) //flip hidden status of recipe
              {
                selectedRecipe.Enabled = !selectedRecipe.Enabled;
                UpdateIRButtons();
            }
        }

        private void AddSupplyButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Supplier));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                panelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddConsumerButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Consumer));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                panelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddPassthroughButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Passthrough));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                panelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddSpoilButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Spoil, NodeDirection.Up));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                panelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddUnSpoilButton_Click(object? sender, EventArgs e) {
            if (KeyItem is not { Item: Item spoilKeyItem })
                return;
            if (spoilKeyItem.SpoilOrigins.Count < 2) {
                panelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Spoil, NodeDirection.Down));
                Dispose(true);
            } else {
                panelCloseReason = ChooserPanelCloseReason.RequiresItemSelection;
                RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Spoil, NodeDirection.Down));
                //Dispose(); //since close reason is 'requires item selection, this will panel will auto close on 'recipe requested' invoke
            }
        }

        private void AddPlantButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Plant, NodeDirection.Up));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                panelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddUnPlantButton_Click(object? sender, EventArgs e) {
            if (KeyItem is not { Item: Item plantKeyItem })
                return;
            if (plantKeyItem.PlantOrigins.Count < 2) {
                panelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Plant, NodeDirection.Down));
                Dispose(true);
            } else {
                panelCloseReason = ChooserPanelCloseReason.RequiresItemSelection;
                RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Plant, NodeDirection.Down));
                //Dispose(); //since close reason is 'requires item selection, this will panel will auto close on 'recipe requested' invoke
            }
        }

        internal override void IRButton_MouseHover(object? sender, EventArgs e) {
            if (sender is not Button btn || btn.Tag is not Recipe recipe || IRButtonToolTip is not RecipeToolTip rtt)
                return;

            int yoffset = -btn.Location.Y + 16 + Math.Max(-100, Math.Min(0, 348 - RecipeToolTip.GetRecipeToolTipHeight(recipe)));

            rtt.SetRecipe(recipe);
            rtt.Show(btn, new Point(btn.Width, yoffset));
        }
    }

    public class NFButton : Button {
        private static ColorMatrix grayMatrix = new ColorMatrix(
        [
            [.2126f, .2126f, .2126f, 0, 0],
            [.7152f, .7152f, .7152f, 0, 0],
            [.0722f, .0722f, .0722f, 0, 0],
            [0, 0, 0, 0.4f, 0],
            [0, 0, 0, 0, 1],
        ]);
        private Image? bgImg;

        public NFButton() : base() { SetStyle(ControlStyles.Selectable, false); }
        protected override bool ShowFocusCues { get { return false; } }
        protected override void OnBackgroundImageChanged(EventArgs e) {
            base.OnBackgroundImageChanged(e);
            if (Enabled)
                bgImg = BackgroundImage;
        }
        protected override void OnEnabledChanged(EventArgs e) {
            base.OnEnabledChanged(e);
            if (BackgroundImage == null)
                return;
            if (!Enabled) {
                var gray = new Bitmap(BackgroundImage.Width, BackgroundImage.Height, BackgroundImage.PixelFormat);
                gray.SetResolution(BackgroundImage.HorizontalResolution, BackgroundImage.VerticalResolution);
                using (var g = Graphics.FromImage(gray)) {
                    using (var attrib = new ImageAttributes()) {
                        attrib.SetColorMatrix(grayMatrix);
                        g.DrawImage(BackgroundImage, new Rectangle(0, 0, BackgroundImage.Width, BackgroundImage.Height), 0, 0, BackgroundImage.Width, BackgroundImage.Height, GraphicsUnit.Pixel, attrib);
                        BackgroundImage = gray;
                    }
                }
            } else if (bgImg != null) {
                BackgroundImage = bgImg;
            }
        }
    }

    public class RecipeRequestArgs : EventArgs {
        public RecipeQualityPair Recipe;
        public NodeType NodeType;
        public NodeDirection Direction;
        public RecipeRequestArgs(RecipeQualityPair recipe) : this(NodeType.Recipe, recipe, NodeDirection.Down) { }
        public RecipeRequestArgs(NodeType nodeType) : this(nodeType, new RecipeQualityPair("non-recipe request args"), NodeDirection.Down) {
            if (nodeType == NodeType.Recipe)
                Trace.Fail("RecipeRequestArgs need a recipe for a recipe node request!");
            if (nodeType == NodeType.Spoil || nodeType == NodeType.Plant)
                Trace.Fail("RecipeRequestArgs need a direction for a spoil / plant node request!");
        }
        public RecipeRequestArgs(NodeType nodeType, NodeDirection direction) : this(nodeType, new RecipeQualityPair("non-recipe request args"), direction) {
            if (nodeType != NodeType.Spoil && nodeType != NodeType.Plant)
                Trace.Fail("RecipeRequestArgs with direction only supported for spoil & plant requests!");
        }
        public RecipeRequestArgs(NodeType nodeType, RecipeQualityPair recipe, NodeDirection direction) {
            Recipe = recipe;
            NodeType = nodeType;
            Direction = direction;
        }
    }

    public class ItemRequestArgs : EventArgs {
        public ItemQualityPair Item;
        public ItemRequestArgs(ItemQualityPair item) { Item = item; }
    }

    public class PanelChooserCloseArgs : EventArgs {
        public IRChooserPanel.ChooserPanelCloseReason Option;
        public PanelChooserCloseArgs(IRChooserPanel.ChooserPanelCloseReason option) { Option = option; }
    }
}