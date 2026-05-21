using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public event EventHandler<PanelChooserCloseEventArgs>? PanelClosed;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal ChooserPanelCloseReason PanelCloseReason { get; set; }
        private bool isClosing;
        private EventHandler? viewerResizeHandler;
        private System.Windows.Forms.Timer? viewerBoundsDebounceTimer;
        private const int ViewerBoundsDebounceMilliseconds = 200;
        private readonly Point desiredScreenOrigin;

        private static readonly Color SelectedGroupButtonBGColor = Color.SandyBrown;
        protected static readonly Color IRButtonDefaultColor = Color.FromArgb(255, 70, 70, 70);
        protected static readonly Color IRButtonHiddenColor = Color.FromArgb(255, 120, 0, 0);
        protected static readonly Color IRButtonNoAssemblerColor = Color.FromArgb(255, 100, 100, 0);
        protected static readonly Color IRButtonUnavailableColor = Color.FromArgb(255, 170, 10, 160);


        private readonly List<NFButton> GroupButtons = [];
        private readonly Dictionary<IGroup, NFButton> GroupButtonLinks = [];
        private readonly List<KeyValuePair<IDataObjectBase, Color>[]> filteredIRRowsList = []; //updated on every filter command & group selection. Represents the full set of items/recipes in the IRFlowPanel (the visible ones will come from this set based on scrolling), with each array being size 10 (#buttons/line). bool (value) is the 'use BW icon'
        protected int CurrentRow { get; private set; } //used to ensure we dont update twice when filtering or group change (once due to update request, second due to setting scroll bar value to 0)

        protected List<IGroup>? SortedGroups { get; set; }
        protected IGroup? SelectedGroup { get; set; } //provides some continuity between selections - if you last selected from the intermediates group for example, adding another recipe will select that group as the starting group
        private static IGroup? StartingGroup;
        protected ProductionGraphViewer PGViewer { get; set; }
        protected abstract ToolTip IRButtonToolTip { get; }
        private readonly CustomToolTip GroupButtonToolTip;

        protected abstract List<List<KeyValuePair<IDataObjectBase, Color>>> GetSubgroupList();
        protected abstract void IRButtonMouseUp(object? sender, MouseEventArgs e);
        //protected abstract void IRButton_Hover(object? sender, EventArgs e);

        protected bool ShowUnavailable { get; private set; }

        public IRChooserPanel(ProductionGraphViewer parent, Point originPoint) {
            PGViewer = parent;
            this.DoubleBuffered = true;
            this.ShowUnavailable = Properties.Settings.Default.ShowUnavailable;
            PanelCloseReason = ChooserPanelCloseReason.Cancelled;

            InitializeComponent();
            FilterTextBox.TextChanged += FilterTextBox_TextChanged;
            Leave += IRChooserPanel_Leave;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.Disposed += IRChooserPanelDisposed;
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            GroupButtonToolTip = new CustomToolTip();

            IRScrollBar.Minimum = 0;
            IRScrollBar.Maximum = 0;
            IRScrollBar.Enabled = false;
            IRScrollBar.SmallChange = 1;
            IRScrollBar.LargeChange = ChooserIconGrid.VisibleRowCount;
            CurrentRow = 0;

            iconGrid.WireMouseWheel(IRFlowPanel_MouseWheel);
            IRScrollBar.Scroll += IRPanelScrollBar_Scroll;

            for (int column = 0; column < ChooserIconGrid.ColumnCount; column++) {
                for (int row = 0; row < ChooserIconGrid.VisibleRowCount; row++) {
                    NFButton button = IRButtons.ElementAt(column).ElementAt(row);
                    button.MouseUp += IRButtonMouseUp;
                    button.MouseHover += IRButton_MouseHover;
                    button.MouseLeave += IRButton_MouseLeave;
                }
            }

            ShowHiddenCheckBox.Checked = Properties.Settings.Default.ShowHidden;
            IgnoreAssemblerCheckBox.Checked = Properties.Settings.Default.IgnoreAssemblerStatus;
            RecipeNameOnlyFilterCheckBox.Checked = Properties.Settings.Default.RecipeNameOnlyFilter;

            desiredScreenOrigin = originPoint;
        }

        protected override void OnDpiChangedAfterParent(EventArgs e) {
            base.OnDpiChangedAfterParent(e);
            if (isClosing || !IsHandleCreated)
                return;
            ApplyDpiScaling();
            if (PGViewer != null && PGViewer.Controls.Contains(this))
                ApplyViewerBounds();
        }

        public new void Show() {
            ApplyDpiScaling();
            InitializeButtons();
            StartingGroup ??= SortedGroups?.FirstOrDefault(g => g.Name == "logistics");
            SetSelectedGroup(null);

            //set up the event handlers last so as not to cause unexpected calls when setting checked status ob checkboxes
            ShowHiddenCheckBox.CheckedChanged += new EventHandler(FilterCheckBoxCheckedChanged);
            IgnoreAssemblerCheckBox.CheckedChanged += new EventHandler(FilterCheckBoxCheckedChanged);

            if (!IsHandleCreated)
                CreateControl();

            // Lay out at final size/position before parenting onto the graph viewer to avoid flashing
            // designer-default child positions during the expensive bounds pass.
            Visible = true;
            RefreshViewerBounds();

            PGViewer.SuspendLayout();
            try {
                PGViewer.Controls.Add(this);
            } finally {
                PGViewer.ResumeLayout(false);
            }

            viewerResizeHandler ??= QueueViewerBoundsRefresh;
            PGViewer.Resize += viewerResizeHandler;
            BringToFront();
            FilterTextBox.Focus();
        }

        private void QueueViewerBoundsRefresh(object? sender, EventArgs e) {
            if (applyingViewerBounds || IsDisposed || !Visible || PGViewer == null)
                return;
            if (viewerBoundsDebounceTimer == null) {
                viewerBoundsDebounceTimer = new System.Windows.Forms.Timer { Interval = ViewerBoundsDebounceMilliseconds };
                viewerBoundsDebounceTimer.Tick += (_, _) => {
                    viewerBoundsDebounceTimer!.Stop();
                    RefreshViewerBounds();
                };
            }
            viewerBoundsDebounceTimer.Stop();
            viewerBoundsDebounceTimer.Start();
        }

        private void RefreshViewerBounds() {
            if (refreshingViewerBounds || IsDisposed || !Visible || PGViewer == null)
                return;
            refreshingViewerBounds = true;
            try {
                ApplyViewerBounds();
            } finally {
                refreshingViewerBounds = false;
            }
        }

        private void DetachViewerResizeHandler() {
            DisposeViewerBoundsDebounceTimer();
            if (PGViewer == null)
                return;
            if (viewerResizeHandler != null) {
                PGViewer.Resize -= viewerResizeHandler;
                viewerResizeHandler = null;
            }
        }

        private void DisposeViewerBoundsDebounceTimer() {
            if (viewerBoundsDebounceTimer == null)
                return;
            viewerBoundsDebounceTimer.Stop();
            viewerBoundsDebounceTimer.Dispose();
            viewerBoundsDebounceTimer = null;
        }

        //-----------------------------------------------------------------------------------------------------Button initialization & update

        private void InitializeButtons() {
            //initialize the group buttons
            SortedGroups = GetSortedGroups();

            groupsPanel.SuspendLayout();
            groupsPanel.Controls.Clear();
            GroupButtons.Clear();
            GroupButtonLinks.Clear();
            groupsPanel.AutoSize = false;
            groupsPanel.WrapContents = true;

            int groupButtonSize = ChooserLayout.Scale(this, ChooserLayout.DesignGroupIconPixels);
            for (int i = 0; i < SortedGroups.Count; i++) {
                var button = new NFButton {
                    BackColor = Color.DimGray,
                    UseVisualStyleBackColor = false,
                    FlatStyle = FlatStyle.Flat,
                    TabStop = false,
                    Margin = new Padding(1),
                    Size = new Size(groupButtonSize, groupButtonSize),
                    BackgroundImage = SortedGroups[i].Icon,
                    BackgroundImageLayout = ImageLayout.Zoom,
                    Tag = SortedGroups[i],
                };
                button.FlatAppearance.BorderSize = 0;

                GroupButtonToolTip.SetToolTip(button, string.IsNullOrEmpty(SortedGroups[i].FriendlyName) ? "-" : SortedGroups[i].FriendlyName);

                button.Click += GroupButton_Click;
                button.MouseHover += GroupButton_MouseHover;
                button.MouseLeave += GroupButton_MouseLeave;

                GroupButtons.Add(button);
                GroupButtonLinks.Add(SortedGroups[i], button);
                groupsPanel.Controls.Add(button);
            }
            groupsPanel.ResumeLayout(true);
        }

        protected abstract List<IGroup> GetSortedGroups();

        private long updateID;
        protected async void UpdateIRButtons(int startRow = 0, bool scrollOnly = false) //if scroll only, then we dont need to update the filtered set, just use what is there
        {
            long currentID = ++updateID;

            await Task.Run(() => {
                //if we are actually changing the filtered list, then update it (through the GetSubgroupList)
                if (!scrollOnly) {
                    filteredIRRowsList.Clear();
                    int currentRow = 0;
                    foreach (List<KeyValuePair<IDataObjectBase, Color>> sgList in GetSubgroupList().Where(n => n.Count > 0)) {
                        filteredIRRowsList.Add(new KeyValuePair<IDataObjectBase, Color>[10]);
                        int currentColumn = 0;
                        foreach (KeyValuePair<IDataObjectBase, Color> kvp in sgList) {
                            if (currentColumn == IRButtons.Count) {
                                filteredIRRowsList.Add(new KeyValuePair<IDataObjectBase, Color>[10]);
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

                });

                //update all the buttons to be based off of the filteredIRSet
                for (int column = 0; column < IRButtons.Count; column++) {
                    for (int row = 0; row < IRButtons.ElementAt(column).Count; row++) {
                        if (currentID != updateID)
                            return;

                        int c = column;
                        int r = row;
                        this.UIThread(delegate {
                            if (currentID != updateID)
                                return;

                            IDataObjectBase? irObject = (r + startRow < filteredIRRowsList.Count) ? filteredIRRowsList[r + startRow][c].Key : null;
                            NFButton b = IRButtons.ElementAt(c).ElementAt(r);
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
            }).ConfigureAwait(false);
        }

        protected void SetSelectedGroup(IGroup? sGroup, bool causeUpdate = true) {
            if (SortedGroups is not null && StartingGroup is not null && (sGroup is null || !SortedGroups.Contains(sGroup))) //want to select the starting group, then update all buttons (including a possibility of group change)
            {
                sGroup = SortedGroups.Contains(StartingGroup) is true ? StartingGroup : SortedGroups[0];
                StartingGroup = sGroup;
                SelectedGroup = sGroup;
                UpdateIRButtons();
            } else {
                foreach (NFButton groupButton in GroupButtons)
                    if (groupButton.Tag is IGroup grp)
                        groupButton.BackColor = grp == sGroup ? SelectedGroupButtonBGColor : Color.DimGray;
                if (SelectedGroup != sGroup) {
                    StartingGroup = sGroup;
                    SelectedGroup = sGroup;
                    if (causeUpdate)
                        UpdateIRButtons();
                }
            }
        }

        protected void UpdateGroupButton(IGroup group, bool enabled) {
            this.UIThread(delegate {
                GroupButtonLinks[group].Enabled = enabled;
            });
        }

        //-----------------------------------------------------------------------------------------------------IGroup Button events

        private void GroupButton_Click(object? sender, EventArgs e) {
            if (sender is NFButton btn && btn.Tag is IGroup grp)
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

        protected void FilterCheckBoxCheckedChanged(object? sender, EventArgs e) {
            UpdateIRButtons();
        }

        private void FilterTextBox_TextChanged(object? sender, EventArgs e) {
            UpdateIRButtons();
        }

        //-----------------------------------------------------------------------------------------------------Closing functions

        internal void CloseIfClickOutside(Point viewerClientPoint) {
            if (isClosing || IsDisposed)
                return;
            if (!Bounds.Contains(viewerClientPoint))
                ClosePanel(ChooserPanelCloseReason.Cancelled);
        }

        protected void ClosePanel(ChooserPanelCloseReason reason) {
            if (isClosing || IsDisposed)
                return;
            isClosing = true;
            PanelCloseReason = reason;
            DetachViewerResizeHandler();
            PersistChooserSettings();
            PanelClosed?.Invoke(this, new PanelChooserCloseEventArgs(PanelCloseReason));
            Dispose();
        }

        private void PersistChooserSettings() {
            Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
            Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            Properties.Settings.Default.RecipeNameOnlyFilter = RecipeNameOnlyFilterCheckBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void IRChooserPanel_Leave(object? sender, EventArgs e) {
            if (isClosing || IsDisposed)
                return;
            // UserControl.Leave also fires when focus moves to a child; defer so ContainsFocus is accurate.
            try {
                BeginInvoke(new Action(() => {
                    if (!isClosing && !IsDisposed && !ContainsFocus)
                        ClosePanel(ChooserPanelCloseReason.Cancelled);
                }));
            } catch (InvalidOperationException) {
                // Control disposed before the deferred check (e.g. icon click closed the panel).
            }
        }

        protected virtual void IRChooserPanelDisposed(object? sender, EventArgs e) {
            DisposeViewerBoundsDebounceTimer();
            DisposeScaledFooterButtonFont();
        }

    }

    public class ItemChooserPanel : IRChooserPanel {
        public event EventHandler<ItemRequestEventArgs>? ItemRequested;

        private readonly ToolTip iToolTip = new CustomToolTip();
        protected override ToolTip IRButtonToolTip { get { return iToolTip; } }
        private ItemQualityPair selectedItem;
        private readonly DataCache DCache;

        private readonly HashSet<IItem>? requestedItemList;
        private readonly bool showAllItems;

        private readonly List<IQuality> qualitySelectorIndexSet;

        public ItemChooserPanel(ProductionGraphViewer parent, Point originPoint, IReadOnlyCollection<IItem>? itemList = null, IQuality? itemQuality = null) : base(parent, originPoint) {
            showAllItems = (itemList == null);
            DCache = parent.DCache ?? throw new InvalidOperationException("Data cache is not loaded.");
            qualitySelectorIndexSet = [];

            if (itemQuality == null) {
                QualityRow.Visible = true;
                foreach (IQuality quality in DCache.AvailableQualities.Where(q => q.Enabled)) {
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

        protected override void IRChooserPanelDisposed(object? sender, EventArgs e) {
            base.IRChooserPanelDisposed(sender, e);
        }

        protected override List<IGroup> GetSortedGroups() {
            var groups = new List<IGroup>();

            if (showAllItems) {
                foreach (IGroup group in ShowUnavailable ? DCache.Groups.Values : DCache.AvailableGroups) {
                    int itemCount = 0;
                    foreach (ISubgroup sgroup in group.Subgroups) {
                        if (showAllItems)
                            itemCount += ShowUnavailable ? sgroup.Items.Count : sgroup.Items.Count(i => i.Available);
                    }
                    if (itemCount > 0)
                        groups.Add(group);
                }
            } else {
                foreach (IItem item in requestedItemList ?? []) {
                    if ((ShowUnavailable || item.Available) && item.MySubgroup.MyGroup is IGroup g && !groups.Contains(g))
                        groups.Add(g);
                }
            }
            groups.Sort();
            return groups;
        }

        protected override List<List<KeyValuePair<IDataObjectBase, Color>>> GetSubgroupList() {
            //step 1: calculate the visible items within each group (used to disable any group button with 0 items, plus shift the selected group if it contains 0 items)
            string filterString = FilterTextBox.Text.ToLowerInvariant();
            bool ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            bool showHidden = ShowHiddenCheckBox.Checked;

            var filteredItems = new Dictionary<IGroup, List<List<KeyValuePair<IDataObjectBase, Color>>>>();
            var filteredItemCount = new Dictionary<IGroup, int>();
            foreach (IGroup group in SortedGroups ?? []) {
                int itemCounter = 0;
                var sgList = new List<List<KeyValuePair<IDataObjectBase, Color>>>();
                foreach (ISubgroup sgroup in group.Subgroups) {
                    var itemList = new List<KeyValuePair<IDataObjectBase, Color>>();
                    foreach (IItem item in sgroup.Items.Where(i => ((ShowUnavailable || i.Available) && (i.LFriendlyName.Contains(filterString) || i.Name.Contains(filterString, StringComparison.OrdinalIgnoreCase))))) {
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
                            itemList.Add(new KeyValuePair<IDataObjectBase, Color>(item, bgColor));
                        }
                    }
                    sgList.Add(itemList);
                }
                filteredItems.Add(group, sgList);
                filteredItemCount.Add(group, itemCounter);
                UpdateGroupButton(group, (itemCounter != 0));
            }

            //step 2: select working group (currently selected group, or if it has 0 items then the first group with >0 items to the left, then the first group with >0 items to the right, then itself)
            IGroup? alternateGroup = null;
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
                alternateGroup ??= SelectedGroup;
            }
            SetSelectedGroup(alternateGroup ?? SelectedGroup, false);

            //now the base class will take care of setting up the buttons based on the filtered items
            return SelectedGroup is not null ? filteredItems[SelectedGroup] : [];
        }

        protected override void IRButtonMouseUp(object? sender, MouseEventArgs e) {
            if (sender is Button b && b.Tag is IItem i && e.Button == MouseButtons.Left) {
                selectedItem = new ItemQualityPair(i, qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                ItemRequested?.Invoke(this, new ItemRequestEventArgs(selectedItem));
                ClosePanel(ChooserPanelCloseReason.ItemSelected);
            }
        }
    }

    public class RecipeChooserPanel : IRChooserPanel {
        public event EventHandler<RecipeRequestEventArgs>? RecipeRequested;

        protected ItemQualityPair KeyItem { get; set; }
        protected bool IsDefaultQuality { get; set; }
        protected FRange KeyItemTempRange { get; set; }
        protected DataCache DCache { get; set; }
        private readonly ToolTip rToolTip = new RecipeToolTip();
        protected override ToolTip IRButtonToolTip { get { return rToolTip; } }

        private readonly List<IQuality> qualitySelectorIndexSet;

        public RecipeChooserPanel(ProductionGraphViewer parent, Point originPoint, ItemQualityPair item, FRange tempRange, NewNodeType nodeType) : base(parent, originPoint) {
            DCache = parent.DCache ?? throw new InvalidOperationException("Data cache is not loaded.");
            qualitySelectorIndexSet = [];

            if (!item) {
                QualityRow.Visible = true;
                foreach (IQuality quality in DCache.AvailableQualities.Where(q => q.Enabled)) {
                    QualitySelector.Items.Add(quality.FriendlyName);
                    qualitySelectorIndexSet.Add(quality);

                }

                if (QualitySelector.Items.Count == 1)
                    QualitySelector.Enabled = false;
            } else if (item is { Quality: IQuality fixedItemQuality }) {
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

            AsIngredientCheckBox.CheckedChanged += FilterCheckBoxCheckedChanged;
            AsProductCheckBox.CheckedChanged += FilterCheckBoxCheckedChanged;
            AsFuelCheckBox.CheckedChanged += FilterCheckBoxCheckedChanged;
            RecipeNameOnlyFilterCheckBox.CheckedChanged += new EventHandler(FilterCheckBoxCheckedChanged);

            KeyItem = item;
            KeyItemTempRange = (nodeType == NewNodeType.Disconnected) ? new FRange(0, 0, true) : tempRange; //cant use temp range if its a disconnected node
            IsDefaultQuality = !KeyItem || (KeyItem is { Quality: IQuality keyItemQuality } && keyItemQuality == DCache.DefaultQuality);

            RecipeNameOnlyFilterCheckBox.Visible = true;
            if (KeyItem is { Item: IItem keyItem, Quality: IQuality keyQuality }) {
                ItemIconPanel.Visible = true;
                ItemIconPanel.BackgroundImage = KeyItem.Icon;
                nodeOptionsRowA.Visible = true;
                AddConsumerButton.Visible = asIngredient;
                AddSupplyButton.Visible = asProduct;

                nodeOptionsRowB.Visible = true;
                AddSpoilButton.Visible = asIngredient && keyItem.SpoilResult != null;
                AddUnspoilButton.Visible = asProduct && keyItem.SpoilOrigins.Count > 0;
                AddPlantButton.Visible = asIngredient && keyItem.PlantResult != null;
                AddUnplantButton.Visible = asProduct && IsDefaultQuality && keyItem.PlantOrigins.Count > 0;
                int totalVisible = (AddSpoilButton.Visible ? 1 : 0) + (AddUnspoilButton.Visible ? 1 : 0) + (AddPlantButton.Visible ? 1 : 0) + (AddUnplantButton.Visible ? 1 : 0);
                nodeOptionsRowB.Visible = totalVisible > 0;

                bool hasConsumptionRecipes = Properties.Settings.Default.ShowUnavailable ? keyItem.ConsumptionRecipes.Count > 0 : keyItem.ConsumptionRecipes.Any(r => r.Available);
                bool hasFuelConsumptionRecipes = IsDefaultQuality && keyItem.FuelsEntities.Any(a => (a is IAssembler assembler) && assembler.Enabled && assembler.Recipes.Any(r => r.Enabled));
                bool hasProductionRecipes = Properties.Settings.Default.ShowUnavailable ? keyItem.ProductionRecipes.Count > 0 : keyItem.ProductionRecipes.Any(r => r.Available);
                bool hasFuelProductionRecipes = IsDefaultQuality && keyItem.FuelOrigin != null && keyItem.FuelOrigin.FuelsEntities.Any(a => (a is IAssembler assembler) && assembler.Enabled && assembler.Recipes.Any(r => r.Enabled));

                if (!(asIngredient && (hasConsumptionRecipes || hasFuelConsumptionRecipes)) && !(asProduct && (hasProductionRecipes || hasFuelProductionRecipes))) //no valid recipes
                {
                    groupsPanel.Visible = false;
                    iconGrid.Visible = false;
                    FilterTextBox.Visible = false;
                    FilterLabel.Visible = false;
                    RecipeNameOnlyFilterCheckBox.Visible = false;
                    ShowHiddenCheckBox.Visible = false;
                    IgnoreAssemblerCheckBox.Visible = false;
                    recipeRoleRow.Visible = false;
                    ItemIconPanel.Location = new Point(4, 4);
                } else if (asIngredient && asProduct) {
                    recipeRoleRow.Visible = true;
                    AsFuelCheckBox.Visible = (asIngredient && hasFuelConsumptionRecipes) || (asProduct && hasFuelProductionRecipes);
                    AsIngredientCheckBox.Visible = true;
                    AsProductCheckBox.Visible = true;
                } else if (asIngredient) {
                    recipeRoleRow.Visible = true;
                    AsFuelCheckBox.Visible = (asIngredient && keyItem.FuelsEntities.Count > 0);
                } else if (asProduct) {
                    recipeRoleRow.Visible = true;
                }
            } else {
                nodeOptionsRowA.Visible = false;
                nodeOptionsRowB.Visible = false;
                recipeRoleRow.Visible = true;
            }
        }

        private static bool RecipeMatchesKeyItem(IRecipe recipe, IItem keyItem, bool includeConsumers, bool includeSuppliers, bool includeFuel, bool ignoreAssemblerStatus, FRange keyItemTempRange) {
            return (includeConsumers && recipe.IngredientSet.ContainsKey(keyItem) && (keyItemTempRange.Ignore || recipe.IngredientTemperatureMap[keyItem].Contains(keyItemTempRange))) ||
                (includeSuppliers && recipe.ProductSet.ContainsKey(keyItem) && (keyItemTempRange.Ignore || keyItemTempRange.Contains(recipe.ProductTemperatureMap[keyItem]))) ||
                (includeConsumers && includeFuel && keyItem.FuelsEntities.Count > 0 && recipe.Assemblers.Any(a => a.Fuels.Contains(keyItem) && (a.Enabled || ignoreAssemblerStatus))) ||
                (includeSuppliers && includeFuel && keyItem.FuelOrigin is IItem fuelOrigin && recipe.Assemblers.Any(a => a.Fuels.Contains(fuelOrigin) && (a.Enabled || ignoreAssemblerStatus)));
        }

        protected override List<IGroup> GetSortedGroups() {
            var groups = new List<IGroup>();
            foreach (IGroup group in ShowUnavailable ? DCache.Groups.Values : DCache.AvailableGroups) {
                int recipeCount = 0;
                foreach (ISubgroup sgroup in group.Subgroups)
                    recipeCount += ShowUnavailable ? sgroup.Recipes.Count : sgroup.Recipes.Count(r => r.Available);
                if (recipeCount > 0)
                    groups.Add(group);
            }
            groups.Sort();
            return groups;
        }

        protected override List<List<KeyValuePair<IDataObjectBase, Color>>> GetSubgroupList() {
            //step 1: calculate the visible recipes for each group (those that pass filter & hidden status)
            string filterString = FilterTextBox.Text.ToLowerInvariant();
            bool ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
            bool checkRecipeIPs = !RecipeNameOnlyFilterCheckBox.Checked;
            bool showHidden = ShowHiddenCheckBox.Checked;
            bool includeSuppliers = AsProductCheckBox.Checked;
            bool includeConsumers = AsIngredientCheckBox.Checked;
            bool includeFuel = AsFuelCheckBox.Checked && IsDefaultQuality;
            bool ignoreItem = !KeyItem;
            IItem? filterKeyItem = KeyItem is { Item: IItem filterItem } ? filterItem : null;

            var filteredRecipes = new Dictionary<IGroup, List<List<KeyValuePair<IDataObjectBase, Color>>>>();
            var filteredRecipeCount = new Dictionary<IGroup, int>();
            foreach (IGroup group in SortedGroups ?? []) {
                int recipeCounter = 0;
                var sgList = new List<List<KeyValuePair<IDataObjectBase, Color>>>();
                foreach (ISubgroup sgroup in group.Subgroups) {
                    var recipeList = new List<KeyValuePair<IDataObjectBase, Color>>();
                    //filter recipes... I tried to break up the filter into several parts to prevent this from being one GIANT '.where' call
                    foreach (IRecipe recipe in sgroup.Recipes.Where(r => ignoreItem || (filterKeyItem is IItem keyItemForFilter && RecipeMatchesKeyItem(r, keyItemForFilter, includeConsumers, includeSuppliers, includeFuel, ignoreAssemblerStatus, KeyItemTempRange)))) {
                        //quick hidden / enabled / available assembler check (done prior to name check for speed)
                        if ((recipe.Enabled || showHidden) && (recipe.Assemblers.Any(a => a.Enabled) || ignoreAssemblerStatus) && (recipe.Available || ShowUnavailable)) {
                            //name check - have to check recipe name along with all ingredients and products (both friendly name and base name) - if selected
                            if (recipe.LFriendlyName.Contains(filterString) ||
                                recipe.Name.Contains(filterString, StringComparison.OrdinalIgnoreCase) || (checkRecipeIPs && (
                                recipe.IngredientList.Any(i => i.LFriendlyName.Contains(filterString) || i.Name.Contains(filterString, StringComparison.OrdinalIgnoreCase)) ||
                                recipe.ProductList.Any(i => i.LFriendlyName.Contains(filterString) || i.Name.Contains(filterString, StringComparison.OrdinalIgnoreCase))))) {
                                //holy... so - we finally finished all the checks, eh? Well, throw it on the pile of recipes to show then.
                                Color bgColor = !recipe.Enabled ? IRButtonHiddenColor :
                                    (!recipe.Available || !recipe.Assemblers.Any(a => a.Available)) ? IRButtonUnavailableColor :
                                    !recipe.Assemblers.Any(a => a.Enabled) ? IRButtonNoAssemblerColor : IRButtonDefaultColor;
                                recipeCounter++;
                                recipeList.Add(new KeyValuePair<IDataObjectBase, Color>(recipe, bgColor));
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
            IGroup? alternateGroup = null;
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
                alternateGroup ??= SelectedGroup;
            }
            SetSelectedGroup(alternateGroup ?? SelectedGroup, false);

            //now the base class will take care of setting up the buttons based on the filtered recipes
            return SelectedGroup is not null ? filteredRecipes[SelectedGroup] : [];
        }

        protected override void IRButtonMouseUp(object? sender, MouseEventArgs e) {
            if (sender is Button btn && btn.Tag is IRecipe sRecipe && e.Button == MouseButtons.Left) //select recipe
            {
                RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(new RecipeQualityPair(sRecipe, qualitySelectorIndexSet[QualitySelector.SelectedIndex])));

                if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                    ClosePanel(ChooserPanelCloseReason.RecipeSelected);
            } else if (sender is NFButton nfBtn && nfBtn.Tag is IRecipe selectedRecipe && e.Button == MouseButtons.Right) //flip hidden status of recipe
              {
                selectedRecipe.Enabled = !selectedRecipe.Enabled;
                UpdateIRButtons();
            }
        }

        private void AddSupplyButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(NodeType.Supplier));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                ClosePanel(ChooserPanelCloseReason.AltNodeSelected);
        }

        private void AddConsumerButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(NodeType.Consumer));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                ClosePanel(ChooserPanelCloseReason.AltNodeSelected);
        }

        private void AddPassthroughButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(NodeType.Passthrough));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                ClosePanel(ChooserPanelCloseReason.AltNodeSelected);
        }

        private void AddSpoilButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(NodeType.Spoil, NodeDirection.Up));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                ClosePanel(ChooserPanelCloseReason.AltNodeSelected);
        }

        private void AddUnSpoilButton_Click(object? sender, EventArgs e) {
            if (KeyItem is not { Item: IItem spoilKeyItem })
                return;
            if (spoilKeyItem.SpoilOrigins.Count < 2) {
                RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(NodeType.Spoil, NodeDirection.Down));
                ClosePanel(ChooserPanelCloseReason.AltNodeSelected);
            } else {
                PanelCloseReason = ChooserPanelCloseReason.RequiresItemSelection;
                RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(NodeType.Spoil, NodeDirection.Down));
                //Dispose(); //since close reason is 'requires item selection, this will panel will auto close on 'recipe requested' invoke
            }
        }

        private void AddPlantButton_Click(object? sender, EventArgs e) {
            RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(NodeType.Plant, NodeDirection.Up));

            if ((ModifierKeys & Keys.Shift) != Keys.Shift)
                ClosePanel(ChooserPanelCloseReason.AltNodeSelected);
        }

        private void AddUnPlantButton_Click(object? sender, EventArgs e) {
            if (KeyItem is not { Item: IItem plantKeyItem })
                return;
            if (plantKeyItem.PlantOrigins.Count < 2) {
                RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(NodeType.Plant, NodeDirection.Down));
                ClosePanel(ChooserPanelCloseReason.AltNodeSelected);
            } else {
                PanelCloseReason = ChooserPanelCloseReason.RequiresItemSelection;
                RecipeRequested?.Invoke(this, new RecipeRequestEventArgs(NodeType.Plant, NodeDirection.Down));
                //Dispose(); //since close reason is 'requires item selection, this will panel will auto close on 'recipe requested' invoke
            }
        }

        internal override void IRButton_MouseHover(object? sender, EventArgs e) {
            if (sender is not Button btn || btn.Tag is not IRecipe recipe || IRButtonToolTip is not RecipeToolTip rtt)
                return;

            int yoffset = -btn.Location.Y + 16 + Math.Max(-100, Math.Min(0, 348 - RecipeToolTip.GetRecipeToolTipHeight(recipe)));

            rtt.SetRecipe(recipe);
            rtt.Show(btn, new Point(btn.Width, yoffset));
        }
    }

    public class NFButton : Button {
        private static readonly ColorMatrix grayMatrix = new(
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
                using var g = Graphics.FromImage(gray);
                using var attrib = new ImageAttributes();
                attrib.SetColorMatrix(grayMatrix);
                g.DrawImage(BackgroundImage, new Rectangle(0, 0, BackgroundImage.Width, BackgroundImage.Height), 0, 0, BackgroundImage.Width, BackgroundImage.Height, GraphicsUnit.Pixel, attrib);
                BackgroundImage = gray;
            } else if (bgImg != null) {
                BackgroundImage = bgImg;
            }
        }
    }

    public class RecipeRequestEventArgs(NodeType nodeType, RecipeQualityPair recipe, NodeDirection direction) : EventArgs {
        public RecipeQualityPair Recipe { get; set; } = recipe;
        public NodeType NodeType { get; set; } = nodeType;
        public NodeDirection Direction { get; set; } = direction;
        public RecipeRequestEventArgs(RecipeQualityPair recipe) : this(NodeType.Recipe, recipe, NodeDirection.Down) { }
        public RecipeRequestEventArgs(NodeType nodeType) : this(nodeType, new RecipeQualityPair(/*"non-recipe request args"*/), NodeDirection.Down) {
            if (nodeType == NodeType.Recipe)
                Trace.Fail("RecipeRequestEventArgs need a recipe for a recipe node request!");
            if (nodeType == NodeType.Spoil || nodeType == NodeType.Plant)
                Trace.Fail("RecipeRequestEventArgs need a direction for a spoil / plant node request!");
        }
        public RecipeRequestEventArgs(NodeType nodeType, NodeDirection direction) : this(nodeType, new RecipeQualityPair(/*"non-recipe request args"*/), direction) {
            if (nodeType != NodeType.Spoil && nodeType != NodeType.Plant)
                Trace.Fail("RecipeRequestEventArgs with direction only supported for spoil & plant requests!");
        }
    }

    public class ItemRequestEventArgs(ItemQualityPair item) : EventArgs {
        public ItemQualityPair Item { get; set; } = item;
    }

    public class PanelChooserCloseEventArgs(IRChooserPanel.ChooserPanelCloseReason option) : EventArgs {
        public IRChooserPanel.ChooserPanelCloseReason Option { get; set; } = option;
    }
}
