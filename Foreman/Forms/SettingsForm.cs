using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class SettingsForm : Form {
        public class SettingsFormOptions(DataCache cache) {
            public DataCache DCache { get; private set; } = cache;

            public List<Preset> Presets = [];
            public Preset SelectedPreset;
            public bool RequireReload;

            public uint QualitySteps;

            public ProductionGraphViewer.Lod LevelOfDetail;
            public int NodeCountForSimpleView;
            public int IconsOnlyIconSize;

            public bool ArrowsOnLinks;
            public bool SimplePassthroughNodes;
            public bool DynamicLinkWidth;
            public bool AbbreviateSciPacks;
            public bool ShowRecipeToolTip;
            public bool RoundAssemblerCount;
            public bool LockedRecipeEditPanelPosition;
            public bool FlagOuSuppliedNodes;

            public bool ShowErrorArrows;
            public bool ShowWarningArrows;
            public bool ShowDisconnectedArrows;
            public bool ShowOuSuppliedArrows;

            public AssemblerSelector.Style DefaultAssemblerStyle;
            public ModuleSelector.Style DefaultModuleStyle;
            public NodeDirection DefaultNodeDirection;
            public bool SmartNodeDirection;

            public bool EnableExtraProductivityForNonMiners;
            public bool DevShowUnavailableItems;
            public bool DevUseRecipeBwFilters;

            public double SolverLowPriorityPower;
            public double SolverPullConsumerNodesPower;
            public bool SolverPullConsumerNodes;

            public HashSet<DataObjectBase> EnabledObjects = [];
        }

        private static readonly Color AvailableObjectColor = Color.White;
        private static readonly Color UnavailableObjectColor = Color.Pink;

        public SettingsFormOptions Options;

        private List<ListViewItem> _unfilteredAssemblerList;
        private List<ListViewItem> _unfilteredMinerList;
        private List<ListViewItem> _unfilteredPowerList;
        private List<ListViewItem> _unfilteredBeaconList;
        private List<ListViewItem> _unfilteredModuleList;
        private List<ListViewItem> _unfilteredRecipeList;
        private List<ListViewItem> _unfilteredQualityList;

        private List<ListViewItem> _filteredAssemblerList;
        private List<ListViewItem> _filteredMinerList;
        private List<ListViewItem> _filteredPowerList;
        private List<ListViewItem> _filteredBeaconList;
        private List<ListViewItem> _filteredModuleList;
        private List<ListViewItem> _filteredRecipeList;
        private List<ListViewItem> _filteredQualityList;

        private MouseHoverDetector _mhDetector;
        private MainForm _mainForm;

        public SettingsForm(SettingsFormOptions options, MainForm mainForm) {
            Options = options;

            InitializeComponent();
            MainForm.SetDoubleBuffered(AssemblerListView);
            MainForm.SetDoubleBuffered(MinerListView);
            MainForm.SetDoubleBuffered(ModuleListView);
            MainForm.SetDoubleBuffered(RecipeListView);
            MainForm.SetDoubleBuffered(QualityListView);

            _mainForm = mainForm;

            AssemblerListView.Columns[0].Width = AssemblerListView.Width - 32;
            MinerListView.Columns[0].Width = MinerListView.Width - 32;
            ModuleListView.Columns[0].Width = ModuleListView.Width - 32;
            RecipeListView.Columns[0].Width = RecipeListView.Width - 32;
            QualityListView.Columns[0].Width = QualityListView.Width - 32;

            _unfilteredAssemblerList = [];
            _unfilteredMinerList = [];
            _unfilteredPowerList = [];
            _unfilteredBeaconList = [];
            _unfilteredModuleList = [];
            _unfilteredRecipeList = [];
            _unfilteredQualityList = [];

            _filteredAssemblerList = [];
            _filteredMinerList = [];
            _filteredPowerList = [];
            _filteredBeaconList = [];
            _filteredModuleList = [];
            _filteredRecipeList = [];
            _filteredQualityList = [];

            SelectPresetMenuItem.Click += SelectPresetMenuItem_Click;
            DeletePresetMenuItem.Click += DeletePresetMenuItem_Click;

            _mhDetector = new MouseHoverDetector(100);
            _mhDetector.Add(RecipeListView, RecipeListView_StartHover, RecipeListView_EndHover);

            CurrentPresetLabel.Text = Options.SelectedPreset.Name;
            PresetListBox.Items.AddRange(Options.Presets.ToArray());
            PresetListBox.Items.RemoveAt(0); //0 is the currently active preset.

            //settings

            QualityStepsInput.Value = Options.QualitySteps;

            DynamicLWCheckBox.Checked = Options.DynamicLinkWidth;
            NodeCountForSimpleViewInput.Value = Math.Min(NodeCountForSimpleViewInput.Maximum, Options.NodeCountForSimpleView);

            IconsSizeInput.Value = Options.IconsOnlyIconSize;

            ArrowsOnLinksCheckBox.Checked = Options.ArrowsOnLinks;
            SimplePassthroughNodesCheckBox.Checked = Options.SimplePassthroughNodes;
            ShowNodeRecipeCheckBox.Checked = Options.ShowRecipeToolTip;
            RoundAssemblerCountCheckBox.Checked = Options.RoundAssemblerCount;
            AbbreviateSciPackCheckBox.Checked = Options.AbbreviateSciPacks;
            RecipeEditPanelPositionLockCheckBox.Checked = Options.LockedRecipeEditPanelPosition;
            FlagOUSupplyNodesCheckBox.Checked = Options.FlagOuSuppliedNodes;

            ErrorArrowsCheckBox.Checked = Options.ShowErrorArrows;
            WarningArrowsCheckBox.Checked = Options.ShowWarningArrows;
            DisconnectedArrowsCheckBox.Checked = Options.ShowDisconnectedArrows;
            OUSuppliedArrowsCheckBox.Checked = Options.ShowOuSuppliedArrows;

            switch (Options.LevelOfDetail) {
                case ProductionGraphViewer.Lod.Low:
                    LowLodRadioButton.Checked = true;
                    break;
                case ProductionGraphViewer.Lod.Medium:
                    MediumLodRadioButton.Checked = true;
                    break;
                case ProductionGraphViewer.Lod.High:
                    HighLodRadioButton.Checked = true;
                    break;
            }

            switch (Options.DefaultNodeDirection) {
                case NodeDirection.Down:
                    NodeDirectionDropDown.SelectedIndex = 1;
                    break;
                case NodeDirection.Up:
                default:
                    NodeDirectionDropDown.SelectedIndex = 0;
                    break;
            }

            SmartNodeDirectionCheckBox.Checked = Options.SmartNodeDirection;

            AssemblerSelectorStyleDropDown.Items.AddRange(AssemblerSelector.StyleNames);
            AssemblerSelectorStyleDropDown.SelectedIndex = (int) Options.DefaultAssemblerStyle;
            ModuleSelectorStyleDropDown.Items.AddRange(ModuleSelector.StyleNames);
            ModuleSelectorStyleDropDown.SelectedIndex = (int) Options.DefaultModuleStyle;

            ShowProductivityBonusOnAllCheckBox.Checked = Options.EnableExtraProductivityForNonMiners;
            ShowUnavailablesCheckBox.Checked = Options.DevShowUnavailableItems;
            LoadBarrelingCheckBox.Checked = !Options.DevUseRecipeBwFilters;

            LowPriorityPowerInput.Value = Math.Min(LowPriorityPowerInput.Maximum, (decimal) Options.SolverLowPriorityPower);
            PullConsumerNodesCheckBox.Checked = Options.SolverPullConsumerNodes;
            PullConsumerNodesPowerInput.Value = Math.Min(PullConsumerNodesPowerInput.Maximum, (decimal) Options.SolverPullConsumerNodesPower);

            //lists
            LoadUnfilteredLists();
            UpdateModList();
        }

        private void UpdateModList() {
            var selectedPreset = (Preset) PresetListBox.SelectedItem ?? Options.SelectedPreset;

            var presetInfo = PresetProcessor.ReadPresetInfo(selectedPreset);
            ModSelectionBox.Items.Clear();
            if (presetInfo.ModList != null) {
                var modlist = presetInfo.ModList.Select(kvp => kvp.Key + "_" + kvp.Value).ToList();
                modlist.Sort();
                ModSelectionBox.Items.AddRange(modlist.ToArray());
            }

            RecipeDifficultyLabel.Text = presetInfo.ExpensiveRecipes ? "Expensive" : "Normal";
            TechnologyDifficultyLabel.Text = presetInfo.ExpensiveTechnology ? "Expensive" : "Normal";
        }

        private void LoadUnfilteredLists() {
            IconList.Images.Clear();
            IconList.Images.Add(DataCache.UnknownIcon);

            LoadUnfilteredList(Options.DCache.Assemblers.Values.Where(a => a.EntityType == EntityType.Assembler), _unfilteredAssemblerList);
            LoadUnfilteredList(Options.DCache.Assemblers.Values.Where(a => a.EntityType is EntityType.Miner or EntityType.OffshorePump),
                _unfilteredMinerList);
            LoadUnfilteredList(
                Options.DCache.Assemblers.Values.Where(a =>
                    a.EntityType is EntityType.Boiler or EntityType.BurnerGenerator or EntityType.Generator or EntityType.Reactor), _unfilteredPowerList);
            LoadUnfilteredList(Options.DCache.Beacons.Values, _unfilteredBeaconList);
            LoadUnfilteredList(Options.DCache.Modules.Values, _unfilteredModuleList);
            LoadUnfilteredList(Options.DCache.Recipes.Values, _unfilteredRecipeList);
            LoadUnfilteredList(Options.DCache.Qualities.Values, _unfilteredQualityList);

            UpdateFilteredLists();
        }

        private void LoadUnfilteredList(IEnumerable<DataObjectBase> origin, List<ListViewItem> lviList) {
            var orderedList = origin is IEnumerable<Quality>
                ? origin.OrderByDescending(a => a.Available).ThenBy(a => a)
                : origin.OrderByDescending(a => a.Available).ThenBy(a => a.FriendlyName);

            foreach (var dObject in orderedList) {
                var lvItem = new ListViewItem();
                if (dObject.Icon != null) {
                    IconList.Images.Add(dObject.Icon);
                    lvItem.ImageIndex = IconList.Images.Count - 1;
                } else {
                    lvItem.ImageIndex = 0;
                }

                lvItem.Text = dObject.FriendlyName;
                lvItem.Tag = dObject;
                // key
                lvItem.Name = dObject.Name;
                // have to set this to true before (potentially) changing to false in order for the checkboxes to appear
                lvItem.Checked = true;
                lvItem.Checked = Options.EnabledObjects.Contains(dObject);
                lvItem.BackColor = dObject.Available ? AvailableObjectColor : UnavailableObjectColor;
                lviList.Add(lvItem);
            }
        }

        private void UpdateFilteredLists() {
            UpdateFilteredList(_unfilteredAssemblerList, _filteredAssemblerList, AssemblerListView);
            UpdateFilteredList(_unfilteredMinerList, _filteredMinerList, MinerListView);
            UpdateFilteredList(_unfilteredPowerList, _filteredPowerList, PowerListView);
            UpdateFilteredList(_unfilteredBeaconList, _filteredBeaconList, BeaconListView);
            UpdateFilteredList(_unfilteredModuleList, _filteredModuleList, ModuleListView);
            UpdateFilteredList(_unfilteredRecipeList, _filteredRecipeList, RecipeListView);
            UpdateFilteredList(_unfilteredQualityList, _filteredQualityList, QualityListView);
        }

        private void UpdateFilteredList(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner) {
            var filterString = FilterTextBox.Text.ToLower();
            var showUnavailable = ShowUnavailablesFilterCheckBox.Checked;

            filteredList.Clear();

            foreach (var lvItem in unfilteredList) {
                if ((showUnavailable || ((DataObjectBase) lvItem.Tag).Available) &&
                    (string.IsNullOrEmpty(filterString) || lvItem.Text.ToLower().Contains(filterString)))
                    filteredList.Add(lvItem);
            }


            owner.VirtualListSize = filteredList.Count;
            owner.Invalidate();
        }

        //PRESETS LIST------------------------------------------------------------------------------------------
        private void EnableSelectionBox_Enter(object sender, EventArgs e) {
            PresetListBox.SelectedItem = null;
        }

        private void CurrentPresetLabel_Click(object sender, EventArgs e) {
            PresetListBox.SelectedItem = null;
        }

        private void PresetListBox_SelectedValueChanged(object sender, EventArgs e) {
            UpdateModList();
            CurrentPresetLabel.Font = PresetListBox.SelectedItem == null
                ? new Font(CurrentPresetLabel.Font, FontStyle.Bold)
                : new Font(CurrentPresetLabel.Font, FontStyle.Regular);
        }

        private void PresetListBox_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Right) return;

            var index = PresetListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches) {
                var rClickedPreset = (Preset) PresetListBox.Items[index];
                PresetListBox.SelectedIndex = index;

                if (rClickedPreset.IsCurrentlySelected) {
                    SelectPresetMenuItem.Text = "Current Preset";
                    SelectPresetMenuItem.Enabled = false;
                } else {
                    SelectPresetMenuItem.Text = "Use This Preset";
                    SelectPresetMenuItem.Enabled = true;
                }

                SelectPresetMenuItem.Enabled = !rClickedPreset.IsCurrentlySelected;
                if (rClickedPreset.IsDefaultPreset) {
                    DeletePresetMenuItem.Text = "Default Preset";
                    DeletePresetMenuItem.Enabled = false;
                } else {
                    DeletePresetMenuItem.Text = "Delete This Preset";
                    DeletePresetMenuItem.Enabled = !rClickedPreset.IsCurrentlySelected;
                }

                PresetMenuStrip.Show(Cursor.Position);
                PresetMenuStrip.Visible = true;
            } else
                PresetMenuStrip.Visible = false;
        }

        private void PresetListBox_MouseDoubleClick(object sender, MouseEventArgs e) {
            var index = PresetListBox.IndexFromPoint(e.Location);
            if (index == ListBox.NoMatches)
                return;

            Options.SelectedPreset = (Preset) PresetListBox.Items[index];
            UpdateSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DeletePresetMenuItem_Click(object sender, EventArgs e) {
            var selectedPreset = (Preset) PresetListBox.SelectedItem;

            // safety check - should always pass
            if (selectedPreset.IsCurrentlySelected || selectedPreset.IsDefaultPreset)
                return;

            if (MessageBox.Show("Are you sure you wish to delete the \"" + selectedPreset.Name + "\" preset? This is irreversible.", "Confirm Delete",
                    MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            var jsonPath = Path.Combine([Application.StartupPath, "Presets", selectedPreset.Name + ".pjson"]);
            var customJsonPath = Path.Combine([Application.StartupPath, "Presets", selectedPreset.Name + ".json"]);
            var iconPath = Path.Combine([Application.StartupPath, "Presets", selectedPreset.Name + ".dat"]);

            if (File.Exists(jsonPath))
                File.Delete(jsonPath);
            if (File.Exists(customJsonPath))
                File.Delete(customJsonPath);
            if (File.Exists(iconPath))
                File.Delete(iconPath);

            PresetListBox.Items.Remove(selectedPreset);
            Options.Presets.Remove(selectedPreset);
        }

        private void SelectPresetMenuItem_Click(object sender, EventArgs e) {
            Options.SelectedPreset = (Preset) PresetListBox.SelectedItem;
            UpdateSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Filters_Changed(object sender, EventArgs e) {
            UpdateFilteredLists();
        }

        //LIST VIEWS------------------------------------------------------------------------------------------

        private void ListView_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.A && (e.Modifiers & Keys.Control) != 0)
                NativeMethods.SelectAllItems(sender as ListView);
        }

        private void ListView_MouseClick(object sender, MouseEventArgs e) {
            var lvi = (sender as ListView).GetItemAt(e.X, e.Y);
            if (lvi == null || e.X >= lvi.Bounds.Left + 16)
                return;

            if (lvi.Selected) { // check all selected
                var setCheck = !lvi.Checked;
                foreach (int index in (sender as ListView).SelectedIndices) {
                    lvi = (sender as ListView).Items[index];
                    lvi.Checked = setCheck;
                    if (lvi.Checked)
                        Options.EnabledObjects.Add((DataObjectBase) lvi.Tag);
                    else
                        Options.EnabledObjects.Remove((DataObjectBase) lvi.Tag);
                }
            } else {
                lvi.Checked = !lvi.Checked;
                if (lvi.Checked)
                    Options.EnabledObjects.Add((DataObjectBase) lvi.Tag);
                else
                    Options.EnabledObjects.Remove((DataObjectBase) lvi.Tag);
            }

            (sender as ListView).Invalidate();
        }

        private void ListView_MouseDoubleClick(object sender, MouseEventArgs e) {
            var lvi = (sender as ListView).GetItemAt(e.X, e.Y);
            if (lvi == null || e.X >= lvi.Bounds.Left + 16)
                return;

            if (lvi.Selected) { // check all selected
                var setCheck = lvi.Checked;
                foreach (int index in (sender as ListView).SelectedIndices) {
                    lvi = (sender as ListView).Items[index];
                    lvi.Checked = setCheck;
                    if (lvi.Checked)
                        Options.EnabledObjects.Add((DataObjectBase) lvi.Tag);
                    else
                        Options.EnabledObjects.Remove((DataObjectBase) lvi.Tag);
                }
            } else { // lvi.Checked = lvi.Checked;
                if (lvi.Checked)
                    Options.EnabledObjects.Add((DataObjectBase) lvi.Tag);
                else
                    Options.EnabledObjects.Remove((DataObjectBase) lvi.Tag);
            }

            (sender as ListView).Invalidate();
        }

        private void AssemblerListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredAssemblerList[e.ItemIndex];
        }

        private void MinerListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredMinerList[e.ItemIndex];
        }

        private void PowerListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredPowerList[e.ItemIndex];
        }

        private void BeaconListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredBeaconList[e.ItemIndex];
        }

        private void ModuleListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredModuleList[e.ItemIndex];
        }

        private void RecipeListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredRecipeList[e.ItemIndex];
        }

        private void QualityListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredQualityList[e.ItemIndex];
        }

        private void RecipeListView_StartHover(object sender, MouseEventArgs e) {
            var lvi = ((ListView) sender).GetItemAt(e.Location.X, e.Location.Y);
            var location = new Point(e.X + 15, e.Y);
            if (lvi == null)
                return;

            RecipeToolTip.SetRecipe(lvi.Tag as Recipe);
            RecipeToolTip.Show((Control) sender, location);
        }

        private void RecipeListView_EndHover(object sender, EventArgs e) {
            RecipeToolTip.Hide((Control) sender);
        }

        //CONFIRM / RELOAD / CANCEL------------------------------------------------------------------------------------------
        private void ConfirmButton_Click(object sender, EventArgs e) {
            UpdateSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void UpdateSettings() {
            Options.QualitySteps = (uint) QualityStepsInput.Value;

            Options.LevelOfDetail = LowLodRadioButton.Checked ? ProductionGraphViewer.Lod.Low :
                MediumLodRadioButton.Checked ? ProductionGraphViewer.Lod.Medium : ProductionGraphViewer.Lod.High;
            Options.NodeCountForSimpleView = (int) NodeCountForSimpleViewInput.Value;
            Options.IconsOnlyIconSize = (int) IconsSizeInput.Value;

            Options.ArrowsOnLinks = ArrowsOnLinksCheckBox.Checked;
            Options.SimplePassthroughNodes = SimplePassthroughNodesCheckBox.Checked;
            Options.DynamicLinkWidth = DynamicLWCheckBox.Checked;
            Options.AbbreviateSciPacks = AbbreviateSciPackCheckBox.Checked;
            Options.ShowRecipeToolTip = ShowNodeRecipeCheckBox.Checked;
            Options.RoundAssemblerCount = RoundAssemblerCountCheckBox.Checked;
            Options.LockedRecipeEditPanelPosition = RecipeEditPanelPositionLockCheckBox.Checked;
            Options.FlagOuSuppliedNodes = FlagOUSupplyNodesCheckBox.Checked;

            Options.ShowErrorArrows = ErrorArrowsCheckBox.Checked;
            Options.ShowWarningArrows = WarningArrowsCheckBox.Checked;
            Options.ShowDisconnectedArrows = DisconnectedArrowsCheckBox.Checked;
            Options.ShowOuSuppliedArrows = OUSuppliedArrowsCheckBox.Checked;

            Options.DefaultAssemblerStyle = (AssemblerSelector.Style) AssemblerSelectorStyleDropDown.SelectedIndex;
            Options.DefaultModuleStyle = (ModuleSelector.Style) ModuleSelectorStyleDropDown.SelectedIndex;
            Options.DefaultNodeDirection = NodeDirectionDropDown.SelectedIndex == 0 ? NodeDirection.Up : NodeDirection.Down;
            Options.SmartNodeDirection = SmartNodeDirectionCheckBox.Checked;

            Options.EnableExtraProductivityForNonMiners = ShowProductivityBonusOnAllCheckBox.Checked;
            Options.DevShowUnavailableItems = ShowUnavailablesCheckBox.Checked;
            Options.DevUseRecipeBwFilters = !LoadBarrelingCheckBox.Checked;

            Options.SolverLowPriorityPower = (double) LowPriorityPowerInput.Value;
            Options.SolverPullConsumerNodes = PullConsumerNodesCheckBox.Checked;
            Options.SolverPullConsumerNodesPower = (double) PullConsumerNodesPowerInput.Value;

            _mainForm.SetLightMode();
        }

        //PRESET FORMS (Import / compare)------------------------------------------------------------------------------------------

        private void ImportPresetButton_Click(object sender, EventArgs e) {
            using var form = new PresetImportForm();

            form.StartPosition = FormStartPosition.Manual;
            form.Left = Left + 250;
            form.Top = Top + 50;
            var result = form.ShowDialog();

            // we just processed a new preset (either fully or cancelled) - this required the opening of (potentially)
            // a lot of zip files and processing of a ton of bitmaps that are now stuck in garbage.
            // In large mod packs like A&B this could clear out 2GB+ of memory.
            if (form.ImportStarted)
                GC.Collect();

            // we have added a new preset
            if (result != DialogResult.OK || string.IsNullOrEmpty(form.NewPresetName))
                return;

            // extra check just in case we were overwriting
            var newPreset = Options.Presets.FirstOrDefault(p => string.Equals(p.Name, form.NewPresetName, StringComparison.CurrentCultureIgnoreCase));
            if (newPreset == null) {
                newPreset = new Preset(form.NewPresetName, false, false);
                Options.Presets.Add(newPreset);
                PresetListBox.Items.Add(newPreset);
            }


            // we have overwritten the currently active preset. Must force a reload
            if (newPreset == Options.Presets[0]) {
                Options.RequireReload = true;
                UpdateSettings();
                DialogResult = DialogResult.OK;
                Close();
            } else if (MessageBox.Show("Preset import complete! Do you wish to switch to the new preset?", "", MessageBoxButtons.YesNo) ==
                DialogResult.Yes) {
                Options.SelectedPreset = newPreset;
                UpdateSettings();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void ComparePresetsButton_Click(object sender, EventArgs e) {
            if (Options.Presets.Count < 2) {
                MessageBox.Show("Can not compare presets!\n...you only have 1 preset :/");
                return;
            }

            using var form = new PresetComparatorForm();

            form.StartPosition = FormStartPosition.Manual;
            form.Left = Left + 50;
            form.Top = Top + 50;
            form.ShowDialog();
        }

        //SET ENABLED STATUS------------------------------------------------------------------------------------------

        private void LoadEnabledFromSaveButton_Click(object sender, EventArgs e) {
            using var form = new SaveFileLoadForm(Options.DCache, Options.EnabledObjects);

            form.StartPosition = FormStartPosition.Manual;
            form.Left = Left + 50;
            form.Top = Top + 50;
            var result = form.ShowDialog();

            if (result == DialogResult.OK)
                UpdateEnabledStatus();
            else if (result == DialogResult.Abort)
                MessageBox.Show("Error while reading save file. Try running factorio, opening the save game, saving again, and retrying?");
        }

        private void SetEnabledFromSciencePacksButton_Click(object sender, EventArgs e) {
            using var form = new SciencePacksLoadForm(Options.DCache, Options.EnabledObjects);

            form.StartPosition = FormStartPosition.Manual;
            form.Left = Left + 50;
            form.Top = Top + 50;
            var result = form.ShowDialog();

            if (result == DialogResult.OK)
                UpdateEnabledStatus();
        }

        private void EnableAllButton_Click(object sender, EventArgs e) {
            Options.EnabledObjects.Clear();
            Options.EnabledObjects.Add(Options.DCache.PlayerAssembler);

            foreach (var assembler in Options.DCache.Assemblers.Values.Where(m => m.AssociatedItems.Any(i => i.Available)))
                Options.EnabledObjects.Add(assembler);

            foreach (var beacon in Options.DCache.Beacons.Values.Where(m => m.AssociatedItems.Any(i => i.Available)))
                Options.EnabledObjects.Add(beacon);

            foreach (var module in Options.DCache.Modules.Values.Where(m => m.AssociatedItem.Available))
                Options.EnabledObjects.Add(module);

            foreach (var recipe in Options.DCache.Recipes.Values.Where(r => r.Available))
                Options.EnabledObjects.Add(recipe);

            foreach (var quality in Options.DCache.Qualities.Values.Where(r => r.Available))
                Options.EnabledObjects.Add(quality);

            UpdateEnabledStatus();
        }

        private void UpdateEnabledStatus() {
            // this requires a bit of juggling in order to prevent listview (virtual) from throwing a fit.
            // we will ensure filtered lists contain all from unfiltered, then conduct the check updates, then update filtered.

            _filteredAssemblerList.Clear();
            _filteredAssemblerList.AddRange(_unfilteredAssemblerList);
            AssemblerListView.VirtualListSize = _filteredAssemblerList.Count;

            _filteredBeaconList.Clear();
            _filteredBeaconList.AddRange(_unfilteredBeaconList);
            BeaconListView.VirtualListSize = _filteredBeaconList.Count;

            _filteredMinerList.Clear();
            _filteredMinerList.AddRange(_unfilteredMinerList);
            MinerListView.VirtualListSize = _filteredMinerList.Count;

            _filteredModuleList.Clear();
            _filteredModuleList.AddRange(_unfilteredModuleList);
            ModuleListView.VirtualListSize = _filteredModuleList.Count;

            _filteredPowerList.Clear();
            _filteredPowerList.AddRange(_unfilteredPowerList);
            PowerListView.VirtualListSize = _filteredPowerList.Count;

            _filteredRecipeList.Clear();
            _filteredRecipeList.AddRange(_unfilteredRecipeList);
            RecipeListView.VirtualListSize = _filteredRecipeList.Count;

            _filteredQualityList.Clear();
            _filteredQualityList.AddRange(_unfilteredQualityList);
            QualityListView.VirtualListSize += _filteredQualityList.Count;


            foreach (var item in _unfilteredAssemblerList)
                item.Checked = Options.EnabledObjects.Contains((DataObjectBase) item.Tag);
            foreach (var item in _unfilteredBeaconList)
                item.Checked = Options.EnabledObjects.Contains((DataObjectBase) item.Tag);
            foreach (var item in _unfilteredMinerList)
                item.Checked = Options.EnabledObjects.Contains((DataObjectBase) item.Tag);
            foreach (var item in _unfilteredModuleList)
                item.Checked = Options.EnabledObjects.Contains((DataObjectBase) item.Tag);
            foreach (var item in _unfilteredPowerList)
                item.Checked = Options.EnabledObjects.Contains((DataObjectBase) item.Tag);
            foreach (var item in _unfilteredRecipeList)
                item.Checked = Options.EnabledObjects.Contains((DataObjectBase) item.Tag);
            foreach (var item in _unfilteredQualityList)
                item.Checked = Options.EnabledObjects.Contains((DataObjectBase) item.Tag);


            UpdateFilteredLists();
        }

        protected override void OnClosed(EventArgs e) {
            _mhDetector.Dispose();
            base.OnClosed(e);
        }
    }
}