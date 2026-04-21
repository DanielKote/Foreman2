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
    public abstract partial class IrChooserPanel : UserControl {
        public enum ChooserPanelCloseReason {
            RecipeSelected,
            ItemSelected,
            AltNodeSelected,
            RequiresItemSelection,
            Cancelled,
        }

        public event EventHandler<PanelChooserCloseArgs> PanelClosed;
        internal ChooserPanelCloseReason PanelCloseReason;

        private static readonly Color SelectedGroupButtonBgColor = Color.SandyBrown;
        protected static readonly Color IrButtonDefaultColor = Color.FromArgb(255, 70, 70, 70);
        protected static readonly Color IrButtonHiddenColor = Color.FromArgb(255, 120, 0, 0);
        protected static readonly Color IrButtonNoAssemblerColor = Color.FromArgb(255, 100, 100, 0);
        protected static readonly Color IrButtonUnavailableColor = Color.FromArgb(255, 170, 10, 160);


        private NfButton[,] _irButtons;
        private List<NfButton> _groupButtons = [];
        private Dictionary<Group, NfButton> _groupButtonLinks = new();

        // updated on every filter command & group selection.
        // Represents the full set of items/recipes in the IRFlowPanel (the visible ones will come from this set based on scrolling),
        // with each array being size 10 (#buttons/line). bool (value) is the 'use BW icon'
        private List<KeyValuePair<DataObjectBase, Color>[]> _filteredIrRowsList = [];

        // used to ensure we don't update twice when filtering or group change (once due to update request, second due to setting scroll bar value to 0)
        protected int CurrentRow { get; private set; }

        protected List<Group> SortedGroups;

        // provides some continuity between selections - if you last selected from the intermediates group for example,
        // adding another recipe will select that group as the starting group
        protected Group SelectedGroup;

        private static Group _startingGroup;
        protected ProductionGraphViewer PgViewer;

        protected abstract ToolTip IrButtonToolTip { get; }
        private CustomToolTip _groupButtonToolTip;

        protected abstract List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList();
        protected abstract void IRButton_MouseUp(object sender, MouseEventArgs e);
        //protected abstract void IRButton_Hover(object sender, EventArgs e);

        protected bool ShowUnavailable { get; private set; }

        public IrChooserPanel(ProductionGraphViewer parent, Point originPoint) {
            PgViewer = parent;
            DoubleBuffered = true;
            ShowUnavailable = Properties.Settings.Default.ShowUnavailable;
            PanelCloseReason = ChooserPanelCloseReason.Cancelled;

            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Disposed += IRChooserPanel_Disposed;
            Anchor = AnchorStyles.Top | AnchorStyles.Left;

            _irButtons = new NfButton[IRTable.ColumnCount - 1, IRTable.RowCount];

            _groupButtonToolTip = new CustomToolTip();

            IRScrollBar.Minimum = 0;
            IRScrollBar.Maximum = 0;
            IRScrollBar.Enabled = false;
            IRScrollBar.SmallChange = 1;
            IRScrollBar.LargeChange = IRTable.RowCount;
            CurrentRow = 0;

            IRTable.MouseWheel += IRFlowPanel_MouseWheel;

            ShowHiddenCheckBox.Checked = Properties.Settings.Default.ShowHidden;
            IgnoreAssemblerCheckBox.Checked = Properties.Settings.Default.IgnoreAssemblerStatus;
            RecipeNameOnlyFilterCheckBox.Checked = Properties.Settings.Default.RecipeNameOnlyFilter;

            Location = originPoint;
        }

        public new void Show() {
            InitializeButtons();
            SetSelectedGroup(null);

            // set up the event handlers last so as not to cause unexpected calls when setting checked status ob checkboxes

            ShowHiddenCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
            IgnoreAssemblerCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;

            PgViewer.Controls.Add(this);
            BringToFront();
            PgViewer.PerformLayout();
            Focus();
            FilterTextBox.Focus();
        }

        //-----------------------------------------------------------------------------------------------------Button initialization & update

        private void InitializeButtons() {
            // initialize the group buttons

            SortedGroups = GetSortedGroups();

            GroupTable.SuspendLayout();

            // add any extra rows to handle the amount of sorted groups

            for (var i = 0; i < (SortedGroups.Count - 1) / GroupTable.ColumnCount; i++)
                GroupTable.RowStyles.Add(new RowStyle(GroupTable.RowStyles[0].SizeType, GroupTable.RowStyles[0].Height));

            // add in the group buttons

            for (var i = 0; i < SortedGroups.Count; i++) {
                var button = new NfButton();
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

                _groupButtonToolTip.SetToolTip(button, string.IsNullOrEmpty(SortedGroups[i].FriendlyName) ? "-" : SortedGroups[i].FriendlyName);

                button.Click += GroupButton_Click;
                button.MouseHover += GroupButton_MouseHover;
                button.MouseLeave += GroupButton_MouseLeave;

                _groupButtons.Add(button);
                _groupButtonLinks.Add(SortedGroups[i], button);

                GroupTable.Controls.Add(button, i % GroupTable.ColumnCount, i / GroupTable.ColumnCount);
            }

            GroupTable.ResumeLayout();

            // initialize the item/recipe buttons

            IRTable.SuspendLayout();
            for (var column = 0; column < _irButtons.GetLength(0); column++) {
                for (var row = 0; row < _irButtons.GetLength(1); row++) {
                    var button = new NfButton();
                    button.BackgroundImageLayout = ImageLayout.Zoom;
                    button.UseVisualStyleBackColor = false;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = Math.Max(1, IRTable.Width / (_irButtons.GetLength(0) * 24));
                    button.TabStop = false;
                    button.ForeColor = Color.Gray;
                    button.BackColor = Color.DimGray;
                    button.Margin = new Padding(1);
                    button.Size = new Size(40, 40);
                    button.Dock = DockStyle.Fill;
                    button.BackgroundImage = null;
                    button.Tag = null;
                    button.Enabled = false;

                    button.MouseUp += IRButton_MouseUp;
                    button.MouseHover += IRButton_MouseHover;
                    button.MouseLeave += IRButton_MouseLeave;
                    _irButtons[column, row] = button;

                    IRTable.Controls.Add(button, column, row);
                }
            }

            IRTable.ResumeLayout();
        }

        protected abstract List<Group> GetSortedGroups();

        private long _updateId;

        // if scroll only, then we don't need to update the filtered set, just use what is there
        protected async void UpdateIrButtons(int startRow = 0, bool scrollOnly = false) {
            var currentId = ++_updateId;

            // if we are actually changing the filtered list, then update it (through the GetSubgroupList)
            await Task.Run(() => {
                if (!scrollOnly) {
                    _filteredIrRowsList.Clear();
                    var currentRow = 0;
                    foreach (var sgList in GetSubgroupList().Where(n => n.Count > 0)) {
                        _filteredIrRowsList.Add(new KeyValuePair<DataObjectBase, Color>[10]);
                        var currentColumn = 0;
                        foreach (var kvp in sgList) {
                            if (currentColumn == _irButtons.GetLength(0)) {
                                _filteredIrRowsList.Add(new KeyValuePair<DataObjectBase, Color>[10]);
                                currentColumn = 0;
                                currentRow++;
                            }

                            _filteredIrRowsList[currentRow][currentColumn] = kvp;
                            currentColumn++;
                        }

                        currentRow++;
                    }
                }

                var so = scrollOnly;
                this.UiThread(delegate {
                    if (currentId != _updateId)
                        return;

                    if (!so) {
                        IRScrollBar.Maximum = Math.Max(0, _filteredIrRowsList.Count - 1);
                        IRScrollBar.Enabled = IRScrollBar.Maximum >= IRScrollBar.LargeChange;
                    }

                    CurrentRow = startRow;
                    IRScrollBar.Value = startRow;

                    IRTable.ResumeLayout();
                    IRTable.SuspendLayout();
                });

                // update all the buttons to be based off of the filteredIRSet

                for (var column = 0; column < _irButtons.GetLength(0); column++) {
                    for (var row = 0; row < _irButtons.GetLength(1); row++) {
                        if (currentId != _updateId)
                            return;

                        var c = column;
                        var r = row;
                        this.UiThread(delegate {
                            if (currentId != _updateId)
                                return;

                            var irObject = r + startRow < _filteredIrRowsList.Count ? _filteredIrRowsList[r + startRow][c].Key : null;
                            var b = _irButtons[c, r];
                            if (irObject != null) { // full
                                b.ForeColor = Color.Black;
                                b.BackColor = r + startRow < _filteredIrRowsList.Count ? _filteredIrRowsList[r + startRow][c].Value : Color.DimGray;
                                b.BackgroundImage = irObject.Icon;
                                b.Tag = irObject;
                                b.Enabled = true;
                                IrButtonToolTip.SetToolTip(b, string.IsNullOrEmpty(irObject.FriendlyName) ? "-" : irObject.FriendlyName);
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
            this.UiThread(delegate {
                if (currentId != _updateId)
                    return;
                IRTable.ResumeLayout();
            });
        }

        protected void SetSelectedGroup(Group sGroup, bool causeUpdate = true) {
            // want to select the starting group, then update all buttons (including a possibility of group change)

            if (sGroup == null || !SortedGroups.Contains(sGroup)) {
                sGroup = SortedGroups.Contains(_startingGroup) ? _startingGroup : SortedGroups[0];
                _startingGroup = sGroup;
                SelectedGroup = sGroup;
                UpdateIrButtons();
            } else {
                foreach (var groupButton in _groupButtons)
                    groupButton.BackColor = (Group) groupButton.Tag == sGroup ? SelectedGroupButtonBgColor : Color.DimGray;
                if (SelectedGroup != sGroup) {
                    _startingGroup = sGroup;
                    SelectedGroup = sGroup;
                    if (causeUpdate)
                        UpdateIrButtons();
                }
            }
        }

        protected void UpdateGroupButton(Group group, bool enabled) {
            this.UiThread(delegate { _groupButtonLinks[group].Enabled = enabled; });
        }

        //-----------------------------------------------------------------------------------------------------Group Button events

        private void GroupButton_Click(object sender, EventArgs e) {
            SetSelectedGroup((Group) ((NfButton) sender).Tag);
        }

        private void GroupButton_MouseHover(object sender, EventArgs e) {
            var control = (Control) sender;
            _groupButtonToolTip.SetText(_groupButtonToolTip.GetToolTip(control));
            _groupButtonToolTip.Show(control, new Point(control.Width, 10));
        }

        private void GroupButton_MouseLeave(object sender, EventArgs e) {
            _groupButtonToolTip.Hide((Control) sender);
        }

        //-----------------------------------------------------------------------------------------------------IR button events (including scrolling)

        private void IRPanelScrollBar_Scroll(object sender, ScrollEventArgs e) {
            if (e.NewValue != CurrentRow)
                UpdateIrButtons(e.NewValue, true);
        }

        private void IRFlowPanel_MouseWheel(object sender, MouseEventArgs e) {
            if (e.Delta < 0 && IRScrollBar.Value <= IRScrollBar.Maximum - IRScrollBar.LargeChange) {
                IRScrollBar.Value++;
                UpdateIrButtons(IRScrollBar.Value, true);
            } else if (e.Delta > 0 && IRScrollBar.Value > 0) {
                IRScrollBar.Value--;
                UpdateIrButtons(IRScrollBar.Value, true);
            }
        }

        internal virtual void IRButton_MouseHover(object sender, EventArgs e) {
            var control = (Control) sender;
            (IrButtonToolTip as CustomToolTip).SetText(IrButtonToolTip.GetToolTip(control));
            (IrButtonToolTip as CustomToolTip).Show(control, new Point(control.Width, 10));
        }

        private void IRButton_MouseLeave(object sender, EventArgs e) {
            IrButtonToolTip.Hide((Control) sender);
        }

        //-----------------------------------------------------------------------------------------------------Filter

        protected void FilterCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateIrButtons();
        }

        private void FilterTextBox_TextChanged(object sender, EventArgs e) {
            UpdateIrButtons();
        }

        //-----------------------------------------------------------------------------------------------------Closing functions

        private void IRChooserPanel_Leave(object sender, EventArgs e) {
            Dispose();
        }

        protected virtual void IRChooserPanel_Disposed(object sender, EventArgs e) {
            Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
            Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            Properties.Settings.Default.RecipeNameOnlyFilter = RecipeNameOnlyFilterCheckBox.Checked;
            Properties.Settings.Default.Save();
            PanelClosed?.Invoke(this, new PanelChooserCloseArgs(PanelCloseReason));
        }

        private void MainTable_Paint(object sender, PaintEventArgs e)
        {
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }

    public class ItemChooserPanel : IrChooserPanel {
        public event EventHandler<ItemRequestArgs> ItemRequested;

        private ToolTip _iToolTip = new CustomToolTip();

        protected override ToolTip IrButtonToolTip => _iToolTip;

        private ItemQualityPair _selectedItem;
        private DataCache _dCache;

        private HashSet<Item> _requestedItemList;
        private bool _showAllItems;

        private List<Quality> _qualitySelectorIndexSet;

        public ItemChooserPanel(ProductionGraphViewer parent, Point originPoint, IReadOnlyCollection<Item> itemList = null, Quality itemQuality = null) :
            base(parent, originPoint) {
            _showAllItems = itemList == null;
            _dCache = parent.DCache;
            _qualitySelectorIndexSet = [];

            if (itemQuality == null) {
                QualitySelectorTable.Visible = true;
                foreach (var quality in parent.DCache.AvailableQualities.Where(q => q.Enabled)) {
                    QualitySelector.Items.Add(quality.FriendlyName);
                    _qualitySelectorIndexSet.Add(quality);
                }

                if (QualitySelector.Items.Count == 1)
                    QualitySelector.Enabled = false;
            } else {
                QualitySelector.Items.Add(itemQuality.FriendlyName);
                _qualitySelectorIndexSet.Add(itemQuality);
                QualitySelector.Enabled = false;
            }

            QualitySelector.SelectedIndex = 0;

            if (!_showAllItems)
                _requestedItemList = [..itemList];
        }

        protected override void IRChooserPanel_Disposed(object sender, EventArgs e) {
            base.IRChooserPanel_Disposed(sender, e);
            if (_selectedItem)
                ItemRequested?.Invoke(this, new ItemRequestArgs(_selectedItem));
        }

        protected override List<Group> GetSortedGroups() {
            var groups = new List<Group>();

            if (_showAllItems) {
                foreach (var group in ShowUnavailable ? PgViewer.DCache.Groups.Values : PgViewer.DCache.AvailableGroups) {
                    var itemCount = 0;
                    foreach (var subgroup in group.Subgroups) {
                        if (_showAllItems)
                            itemCount += ShowUnavailable ? subgroup.Items.Count : subgroup.Items.Count(i => i.Available);
                    }

                    if (itemCount > 0)
                        groups.Add(group);
                }
            } else {
                foreach (var item in _requestedItemList) {
                    if ((ShowUnavailable || item.Available) && !groups.Contains(item.MySubgroup.MyGroup))
                        groups.Add(item.MySubgroup.MyGroup);
                }
            }

            groups.Sort();
            return groups;
        }

        protected override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList() {
            // step 1:
            // calculate the visible items within each group
            // (used to disable any group button with 0 items, plus shift the selected group if it contains 0 items)

            var filterString = FilterTextBox.Text.ToLower();
            var ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            var showHidden = ShowHiddenCheckBox.Checked;

            var filteredItems = new Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>>();
            var filteredItemCount = new Dictionary<Group, int>();
            foreach (var group in SortedGroups) {
                var itemCounter = 0;
                var sgList = new List<List<KeyValuePair<DataObjectBase, Color>>>();
                foreach (var subgroup in group.Subgroups) {
                    var itemList = new List<KeyValuePair<DataObjectBase, Color>>();
                    foreach (var item in subgroup.Items.Where(i =>
                        (ShowUnavailable || i.Available) && (i.LFriendlyName.Contains(filterString) ||
                            i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1))) {
                        if (!_showAllItems && !_requestedItemList.Contains(item))
                            continue;

                        var visible = (ShowUnavailable || item.Available) &&
                            (item.ConsumptionRecipes.Any(r => r.Enabled && (ShowUnavailable || r.Available)) ||
                                item.ProductionRecipes.Any(r => r.Enabled && (ShowUnavailable || r.Available)));

                        var validAssembler =
                            item.ConsumptionRecipes.Any(r =>
                                r.Enabled && (ShowUnavailable || r.Available) && r.Assemblers.Any(a => a.Enabled && (ShowUnavailable || a.Available))) ||
                            item.ProductionRecipes.Any(r =>
                                r.Enabled && (ShowUnavailable || r.Available) && r.Assemblers.Any(a => a.Enabled && (ShowUnavailable || a.Available)));


                        var bgColor = visible && item.Available ? validAssembler ? IrButtonDefaultColor : IrButtonNoAssemblerColor : IrButtonHiddenColor;

                        if ((visible || showHidden) && (validAssembler || ignoreAssemblerStatus)) {
                            itemCounter++;
                            itemList.Add(new KeyValuePair<DataObjectBase, Color>(item, bgColor));
                        }
                    }

                    sgList.Add(itemList);
                }

                filteredItems.Add(group, sgList);
                filteredItemCount.Add(group, itemCounter);
                UpdateGroupButton(group, itemCounter != 0);
            }

            // step 2:
            // select working group (currently selected group, or if it has 0 items then the first group with >0 items to the left,
            // then the first group with >0 items to the right, then itself)

            Group alternateGroup = null;
            if (filteredItemCount[SelectedGroup] == 0) {
                var selectedGroupIndex = 0;
                for (var i = 0; i < SortedGroups.Count; i++)
                    if (SortedGroups[i] == SelectedGroup)
                        selectedGroupIndex = i;
                for (var i = selectedGroupIndex; i >= 0; i--)
                    if (filteredItemCount[SortedGroups[i]] > 0)
                        alternateGroup = SortedGroups[i];
                if (alternateGroup == null)
                    for (var i = selectedGroupIndex; i < SortedGroups.Count; i++)
                        if (filteredItemCount[SortedGroups[i]] > 0)
                            alternateGroup = SortedGroups[i];
                if (alternateGroup == null)
                    alternateGroup = SelectedGroup;
            }

            SetSelectedGroup(alternateGroup ?? SelectedGroup, false);

            // now the base class will take care of setting up the buttons based on the filtered items

            return filteredItems[SelectedGroup];
        }

        protected override void IRButton_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                PanelCloseReason = ChooserPanelCloseReason.ItemSelected;
                _selectedItem = new ItemQualityPair((Item) ((Button) sender).Tag, _qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                Dispose();
            }
        }
    }

    public class RecipeChooserPanel : IrChooserPanel {
        public event EventHandler<RecipeRequestArgs> RecipeRequested;

        protected ItemQualityPair KeyItem;
        protected bool IsDefaultQuality;
        protected FRange KeyItemTempRange;
        protected DataCache DCache;

        private ToolTip _rToolTip = new RecipeToolTip();

        protected override ToolTip IrButtonToolTip => _rToolTip;

        private List<Quality> _qualitySelectorIndexSet;

        public RecipeChooserPanel(ProductionGraphViewer parent, Point originPoint, ItemQualityPair item, FRange tempRange, NewNodeType nodeType) : base(parent,
            originPoint) {
            DCache = parent.DCache;
            _qualitySelectorIndexSet = [];

            if (!item) {
                QualitySelectorTable.Visible = true;
                foreach (var quality in parent.DCache.AvailableQualities.Where(q => q.Enabled)) {
                    QualitySelector.Items.Add(quality.FriendlyName);
                    _qualitySelectorIndexSet.Add(quality);
                }

                if (QualitySelector.Items.Count == 1)
                    QualitySelector.Enabled = false;
            } else {
                QualitySelector.Items.Add(item.Quality.FriendlyName);
                _qualitySelectorIndexSet.Add(item.Quality);
                QualitySelector.Enabled = false;
            }

            QualitySelector.SelectedIndex = 0;

            var asIngredient = nodeType is NewNodeType.Consumer or NewNodeType.Disconnected;
            var asProduct = nodeType is NewNodeType.Supplier or NewNodeType.Disconnected;

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
            RecipeNameOnlyFilterCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;

            KeyItem = item;
            // cant use temp range if it's a disconnected node
            KeyItemTempRange = nodeType == NewNodeType.Disconnected ? new FRange(0, 0, true) : tempRange;
            IsDefaultQuality = !KeyItem || KeyItem.Quality == DCache.DefaultQuality;

            RecipeNameOnlyFilterCheckBox.Visible = true;
            if (KeyItem) {
                ItemIconPanel.Visible = true;
                ItemIconPanel.BackgroundImage = KeyItem.Icon;
                OtherNodeOptionsATable.Visible = true;
                AddConsumerButton.Visible = asIngredient;
                AddSupplyButton.Visible = asProduct;

                OtherNodeOptionsBTable.Visible = true;
                AddSpoilButton.Visible = asIngredient && KeyItem.Item.SpoilResult != null;
                AddUnspoilButton.Visible = asProduct && KeyItem.Item.SpoilOrigins.Count > 0;
                AddPlantButton.Visible = asIngredient && KeyItem.Item.PlantResult != null;
                AddUnplantButton.Visible = asProduct && IsDefaultQuality && KeyItem.Item.PlantOrigins.Count > 0;
                var totalVisible = (AddSpoilButton.Visible ? 1 : 0) + (AddUnspoilButton.Visible ? 1 : 0) + (AddPlantButton.Visible ? 1 : 0) +
                    (AddUnplantButton.Visible ? 1 : 0);
                OtherNodeOptionsBTable.Visible = totalVisible > 0;

                var hasConsumptionRecipes = Properties.Settings.Default.ShowUnavailable
                    ? KeyItem.Item.ConsumptionRecipes.Count > 0
                    : KeyItem.Item.ConsumptionRecipes.Count(r => r.Available) > 0;
                var hasFuelConsumptionRecipes = IsDefaultQuality &&
                    KeyItem.Item.FuelsEntities.Any(a => a is Assembler { Enabled: true } assembler && assembler.RecipesView.Any(r => r.Enabled));
                var hasProductionRecipes = Properties.Settings.Default.ShowUnavailable
                    ? KeyItem.Item.ProductionRecipes.Count > 0
                    : KeyItem.Item.ProductionRecipes.Count(r => r.Available) > 0;
                var hasFuelProductionRecipes = IsDefaultQuality && KeyItem.Item.FuelOrigin != null &&
                    KeyItem.Item.FuelOrigin.FuelsEntities.Any(a =>
                        a is Assembler { Enabled: true } assembler && assembler.RecipesView.Any(r => r.Enabled));

                if (!(asIngredient && (hasConsumptionRecipes || hasFuelConsumptionRecipes)) &&
                    !(asProduct && (hasProductionRecipes || hasFuelProductionRecipes))) //no valid recipes
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
                    AsFuelCheckBox.Visible = asIngredient && KeyItem.Item.FuelsEntities.Count > 0;
                }
            } else {
                OtherNodeOptionsATable.Visible = false;
                OtherNodeOptionsBTable.Visible = false;
            }
        }

        protected override List<Group> GetSortedGroups() {
            var groups = new List<Group>();
            foreach (var group in ShowUnavailable ? PgViewer.DCache.Groups.Values : PgViewer.DCache.AvailableGroups) {
                var recipeCount = 0;
                foreach (var subgroup in group.Subgroups)
                    recipeCount += ShowUnavailable ? subgroup.Recipes.Count : subgroup.Recipes.Count(r => r.Available);
                if (recipeCount > 0)
                    groups.Add(group);
            }

            groups.Sort();
            return groups;
        }

        protected override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList() {
            // step 1:
            // calculate the visible recipes for each group
            // (those that pass filter & hidden status)

            var filterString = FilterTextBox.Text.ToLower();
            var ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            var checkRecipeIPs = !RecipeNameOnlyFilterCheckBox.Checked;
            var showHidden = ShowHiddenCheckBox.Checked;
            var includeSuppliers = AsProductCheckBox.Checked;
            var includeConsumers = AsIngredientCheckBox.Checked;
            var includeFuel = AsFuelCheckBox.Checked && IsDefaultQuality;
            var ignoreItem = !KeyItem;

            var filteredRecipes =
                new Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>>();
            var filteredRecipeCount = new Dictionary<Group, int>();
            foreach (var group in SortedGroups) {
                var recipeCounter = 0;
                var sgList = new List<List<KeyValuePair<DataObjectBase, Color>>>();
                foreach (var subgroup in group.Subgroups) {
                    var recipeList = new List<KeyValuePair<DataObjectBase, Color>>();

                    // filter recipes... I tried to break up the filter into several parts to prevent this from being one GIANT '.where' call
                    foreach (var recipe in subgroup.Recipes.Where(r => ignoreItem ||
                        (includeConsumers && r.IngredientSet.ContainsKey(KeyItem.Item) &&
                            (KeyItemTempRange.Ignore ||
                                r.IngredientTemperatureMap[KeyItem.Item]
                                    .Contains(KeyItemTempRange))) || // consumers of item with temperature range containing required
                        (includeSuppliers && r.ProductSet.ContainsKey(KeyItem.Item) &&
                            (KeyItemTempRange.Ignore ||
                                KeyItemTempRange.Contains(
                                    r.ProductTemperatureMap[KeyItem.Item]))) || // producers of item with temperature within the temperature range
                        (includeConsumers && includeFuel && KeyItem.Item.FuelsEntities.Count > 0 &&
                            r.Assemblers.Any(a =>
                                a.Fuels.Contains(KeyItem.Item) &&
                                (a.Enabled ||
                                    ignoreAssemblerStatus))) || // consumers of item (as fuel) -> have to check assembler status here for this specific assembler that accepts this fuel
                        (includeSuppliers && includeFuel && KeyItem.Item.FuelOrigin != null && r.Assemblers.Any(a =>
                            a.Fuels.Contains(KeyItem.Item.FuelOrigin) &&
                            (a.Enabled ||
                                ignoreAssemblerStatus))))) // producers of item (as fuel remains) -> check assembler status here as well for same reason
                    {
                        // quick hidden / enabled / available assembler check (done prior to name check for speed)
                        if ((recipe.Enabled || showHidden) && (recipe.Assemblers.Any(a => a.Enabled) || ignoreAssemblerStatus) &&
                            (recipe.Available || ShowUnavailable)) {
                            // name check - have to check recipe name along with all ingredients and products (both friendly name and base name) - if selected
                            if (recipe.LFriendlyName.Contains(filterString) ||
                                recipe.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1 || (checkRecipeIPs && (
                                    recipe.IngredientList.Any(i =>
                                        i.LFriendlyName.Contains(filterString) || i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1) ||
                                    recipe.ProductList.Any(i =>
                                        i.LFriendlyName.Contains(filterString) || i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1)))) {
                                // holy... so - we finally finished all the checks, eh? Well, throw it on the pile of recipes to show then.
                                var bgColor = !recipe.Enabled ? IrButtonHiddenColor :
                                    !recipe.Available || !recipe.Assemblers.Any(a => a.Available) ? IrButtonUnavailableColor :
                                    !recipe.Assemblers.Any(a => a.Enabled) ? IrButtonNoAssemblerColor : IrButtonDefaultColor;
                                recipeCounter++;
                                recipeList.Add(new KeyValuePair<DataObjectBase, Color>(recipe, bgColor));
                            }
                        }
                    }

                    sgList.Add(recipeList);
                }

                filteredRecipes.Add(group, sgList);
                filteredRecipeCount.Add(group, recipeCounter);
                UpdateGroupButton(group, recipeCounter != 0);
            }

            // step 2:
            // select working group (currently selected group, or if it has 0 recipes then the first group with >0 recipes to the left,
            // then the first group with >0 recipes to the right, then itself)

            Group alternateGroup = null;
            if (filteredRecipeCount[SelectedGroup] == 0) {
                var selectedGroupIndex = 0;
                for (var i = 0; i < SortedGroups.Count; i++)
                    if (SortedGroups[i] == SelectedGroup)
                        selectedGroupIndex = i;
                for (var i = selectedGroupIndex; i >= 0; i--)
                    if (filteredRecipeCount[SortedGroups[i]] > 0)
                        alternateGroup = SortedGroups[i];
                if (alternateGroup == null)
                    for (var i = selectedGroupIndex; i < SortedGroups.Count; i++)
                        if (filteredRecipeCount[SortedGroups[i]] > 0)
                            alternateGroup = SortedGroups[i];
                if (alternateGroup == null)
                    alternateGroup = SelectedGroup;
            }

            SetSelectedGroup(alternateGroup ?? SelectedGroup, false);

            // now the base class will take care of setting up the buttons based on the filtered recipes

            return filteredRecipes[SelectedGroup];
        }

        protected override void IRButton_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) { // select recipe
                var sRecipe = (Recipe) ((Button) sender).Tag;
                RecipeRequested?.Invoke(this,
                    new RecipeRequestArgs(new RecipeQualityPair((Recipe) ((Button) sender).Tag, _qualitySelectorIndexSet[QualitySelector.SelectedIndex])));

                if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                    PanelCloseReason = ChooserPanelCloseReason.RecipeSelected;
                    Dispose();
                }
            } else if (e.Button == MouseButtons.Right) { // flip hidden status of recipe
                var selectedRecipe = (sender as NfButton).Tag as Recipe;
                selectedRecipe.Enabled = !selectedRecipe.Enabled;
                UpdateIrButtons();
            }
        }

        private void AddSupplyButton_Click(object sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Supplier));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                PanelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddConsumerButton_Click(object sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Consumer));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                PanelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddPassthroughButton_Click(object sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Passthrough));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                PanelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddSpoilButton_Click(object sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Spoil, NodeDirection.Up));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                PanelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddUnSpoilButton_Click(object sender, EventArgs e) {
            if (KeyItem.Item.SpoilOrigins.Count < 2) {
                PanelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Spoil, NodeDirection.Down));
                Dispose(true);
            } else {
                PanelCloseReason = ChooserPanelCloseReason.RequiresItemSelection;
                RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Spoil, NodeDirection.Down));
                //Dispose(); // since close reason is 'requires item selection, this will panel will auto close on 'recipe requested' invoke
            }
        }

        private void AddPlantButton_Click(object sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Plant, NodeDirection.Up));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift) {
                PanelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                Dispose();
            }
        }

        private void AddUnPlantButton_Click(object sender, EventArgs e) {
            if (KeyItem.Item.PlantOrigins.Count < 2) {
                PanelCloseReason = ChooserPanelCloseReason.AltNodeSelected;
                RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Plant, NodeDirection.Down));
                Dispose(true);
            } else {
                PanelCloseReason = ChooserPanelCloseReason.RequiresItemSelection;
                RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Plant, NodeDirection.Down));
                //Dispose(); // since close reason is 'requires item selection, this will panel will auto close on 'recipe requested' invoke
            }
        }

        internal override void IRButton_MouseHover(object sender, EventArgs e) {
            var control = (Control) sender;

            var yoffset = -control.Location.Y + 16 + Math.Max(-100, Math.Min(0, 348 - RecipeToolTip.GetRecipeToolTipHeight((Recipe) ((Button) sender).Tag)));

            (IrButtonToolTip as RecipeToolTip).SetRecipe((Recipe) ((Button) sender).Tag);
            (IrButtonToolTip as RecipeToolTip).Show(control, new Point(control.Width, yoffset));
        }
    }

    public class NfButton : Button {
        private static ColorMatrix _grayMatrix = new([
            [.2126f, .2126f, .2126f, 0, 0],
            [.7152f, .7152f, .7152f, 0, 0],
            [.0722f, .0722f, .0722f, 0, 0],
            [0, 0, 0, 0.4f, 0],
            [0, 0, 0, 0, 1.0f]
        ]);

        private Image _bgImg;

        public NfButton() {
            SetStyle(ControlStyles.Selectable, false);
        }

        protected override bool ShowFocusCues => false;

        protected override void OnBackgroundImageChanged(EventArgs e) {
            base.OnBackgroundImageChanged(e);
            if (Enabled)
                _bgImg = BackgroundImage;
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
                        attrib.SetColorMatrix(_grayMatrix);
                        g.DrawImage(BackgroundImage, new Rectangle(0, 0, BackgroundImage.Width, BackgroundImage.Height), 0, 0, BackgroundImage.Width,
                            BackgroundImage.Height, GraphicsUnit.Pixel, attrib);
                        BackgroundImage = gray;
                    }
                }
            } else if (_bgImg != null) {
                BackgroundImage = _bgImg;
            }
        }
    }

    public class RecipeRequestArgs(NodeType nodeType, RecipeQualityPair recipe, NodeDirection direction) : EventArgs {
        public RecipeQualityPair Recipe = recipe;
        public NodeType NodeType = nodeType;
        public NodeDirection Direction = direction;
        public RecipeRequestArgs(RecipeQualityPair recipe) : this(NodeType.Recipe, recipe, NodeDirection.Down) { }

        public RecipeRequestArgs(NodeType nodeType) : this(nodeType, new RecipeQualityPair("non-recipe request args"), NodeDirection.Down) {
            if (nodeType == NodeType.Recipe)
                Trace.Fail("RecipeRequestArgs need a recipe for a recipe node request!");
            if (nodeType is NodeType.Spoil or NodeType.Plant)
                Trace.Fail("RecipeRequestArgs need a direction for a spoil / plant node request!");
        }

        public RecipeRequestArgs(NodeType nodeType, NodeDirection direction) : this(nodeType, new RecipeQualityPair("non-recipe request args"), direction) {
            if (nodeType != NodeType.Spoil && nodeType != NodeType.Plant)
                Trace.Fail("RecipeRequestArgs with direction only supported for spoil & plant requests!");
        }
    }

    public class ItemRequestArgs(ItemQualityPair item) : EventArgs {
        public ItemQualityPair Item = item;
    }

    public class PanelChooserCloseArgs(IrChooserPanel.ChooserPanelCloseReason option) : EventArgs {
        public IrChooserPanel.ChooserPanelCloseReason Option = option;
    }
}