using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Forms;
using Foreman.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class SettingsForm : Form {
        public class SettingsFormOptions(DataCache cache) {
            public DataCache DCache { get; private set; } = cache;

            public List<Preset>? Presets { get; set; } = [];
            public Preset? SelectedPreset { get; set; }
            public bool RequireReload { get; set; }
            public uint QualitySteps { get; set; }
            public ProductionGraphViewer.LOD LevelOfDetail { get; set; }
            public int NodeCountForSimpleView { get; set; }
            public int IconsOnlyIconSize { get; set; }
            public bool ArrowsOnLinks { get; set; }
            public bool SimplePassthroughNodes { get; set; }
            public bool DynamicLinkWidth { get; set; }
            public bool AbbreviateSciPacks { get; set; }
            public bool ShowRecipeToolTip { get; set; }
            public bool RoundAssemblerCount { get; set; }
            public bool LockedRecipeEditPanelPosition { get; set; }
            public bool FlagOUSuppliedNodes { get; set; }
            public bool FlagDarkMode { get; set; }
            public bool ShowErrorArrows { get; set; }
            public bool ShowWarningArrows { get; set; }
            public bool ShowDisconnectedArrows { get; set; }
            public bool ShowOUSuppliedArrows { get; set; }
            public AssemblerSelector.Style DefaultAssemblerStyle { get; set; }
            public ModuleSelector.Style DefaultModuleStyle { get; set; }
            public NodeDirection DefaultNodeDirection { get; set; }
            public bool SmartNodeDirection { get; set; }
            public bool EnableExtraProductivityForNonMiners { get; set; }
            public bool DevShowUnavailableItems { get; set; }
            public bool DevUseRecipeBWFilters { get; set; }

            public double SolverLowPriorityPower { get; set; }
            public double SolverPullConsumerNodesPower { get; set; }
            public bool SolverPullConsumerNodes { get; set; }

            public HashSet<IDataObjectBase> EnabledObjects { get; set; } = [];
        }

        private static readonly Color AvailableObjectColor = Color.White;
        private static readonly Color UnavailableObjectColor = Color.Pink;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SettingsFormOptions Options { get; set; }
        private readonly List<ListViewItem> unfilteredAssemblerList;
        private readonly List<ListViewItem> unfilteredMinerList;
        private readonly List<ListViewItem> unfilteredPowerList;
        private readonly List<ListViewItem> unfilteredBeaconList;
        private readonly List<ListViewItem> unfilteredModuleList;
        private readonly List<ListViewItem> unfilteredRecipeList;
        private readonly List<ListViewItem> unfilteredQualityList;

        private readonly List<ListViewItem> filteredAssemblerList;
        private readonly List<ListViewItem> filteredMinerList;
        private readonly List<ListViewItem> filteredPowerList;
        private readonly List<ListViewItem> filteredBeaconList;
        private readonly List<ListViewItem> filteredModuleList;
        private readonly List<ListViewItem> filteredRecipeList;
        private readonly List<ListViewItem> filteredQualityList;

        private readonly MouseHoverDetector mhDetector;
        private readonly MainForm mainForm;

        public SettingsForm(SettingsFormOptions options, MainForm mainForm) {
            Options = options;

            InitializeComponent();
            MainForm.SetDoubleBuffered(AssemblerListView);
            MainForm.SetDoubleBuffered(MinerListView);
            MainForm.SetDoubleBuffered(ModuleListView);
            MainForm.SetDoubleBuffered(RecipeListView);
            MainForm.SetDoubleBuffered(QualityListView);

            this.mainForm = mainForm;

            AssemblerListView.Columns[0].Width = AssemblerListView.Width - 32;
            MinerListView.Columns[0].Width = MinerListView.Width - 32;
            ModuleListView.Columns[0].Width = ModuleListView.Width - 32;
            RecipeListView.Columns[0].Width = RecipeListView.Width - 32;
            QualityListView.Columns[0].Width = QualityListView.Width - 32;

            unfilteredAssemblerList = [];
            unfilteredMinerList = [];
            unfilteredPowerList = [];
            unfilteredBeaconList = [];
            unfilteredModuleList = [];
            unfilteredRecipeList = [];
            unfilteredQualityList = [];

            filteredAssemblerList = [];
            filteredMinerList = [];
            filteredPowerList = [];
            filteredBeaconList = [];
            filteredModuleList = [];
            filteredRecipeList = [];
            filteredQualityList = [];

            SelectPresetMenuItem.Click += SelectPresetMenuItem_Click;
            DeletePresetMenuItem.Click += DeletePresetMenuItem_Click;

            mhDetector = new MouseHoverDetector(100, 200);
            mhDetector.Add(RecipeListView, RecipeListView_StartHover, RecipeListView_EndHover);

            CurrentPresetLabel.Text = Options.SelectedPreset?.Name;
            PresetListBox.Items.AddRange(Options.Presets?.ToArray() ?? []);
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
            FlagOUSupplyNodesCheckBox.Checked = Options.FlagOUSuppliedNodes;
            FlagDarkModeCheckBox.Checked = Options.FlagDarkMode;

            ErrorArrowsCheckBox.Checked = Options.ShowErrorArrows;
            WarningArrowsCheckBox.Checked = Options.ShowWarningArrows;
            DisconnectedArrowsCheckBox.Checked = Options.ShowDisconnectedArrows;
            OUSuppliedArrowsCheckBox.Checked = Options.ShowOUSuppliedArrows;

            switch (Options.LevelOfDetail) {
                case ProductionGraphViewer.LOD.Low:
                    LowLodRadioButton.Checked = true;
                    break;
                case ProductionGraphViewer.LOD.Medium:
                    MediumLodRadioButton.Checked = true;
                    break;
                case ProductionGraphViewer.LOD.High:
                    HighLodRadioButton.Checked = true;
                    break;
            }

            NodeDirectionDropDown.SelectedIndex = Options.DefaultNodeDirection switch {
                NodeDirection.Down => 1,
                _ => 0,
            };
            SmartNodeDirectionCheckBox.Checked = Options.SmartNodeDirection;

            AssemblerSelectorStyleDropDown.Items.AddRange(AssemblerSelector.StyleNames);
            AssemblerSelectorStyleDropDown.SelectedIndex = (int)Options.DefaultAssemblerStyle;
            ModuleSelectorStyleDropDown.Items.AddRange(ModuleSelector.StyleNames);
            ModuleSelectorStyleDropDown.SelectedIndex = (int)Options.DefaultModuleStyle;

            ShowProductivityBonusOnAllCheckBox.Checked = Options.EnableExtraProductivityForNonMiners;
            ShowUnavailablesCheckBox.Checked = Options.DevShowUnavailableItems;
            LoadBarrelingCheckBox.Checked = !Options.DevUseRecipeBWFilters;

            LowPriorityPowerInput.Value = Math.Min(LowPriorityPowerInput.Maximum, (decimal)Options.SolverLowPriorityPower);
            PullConsumerNodesCheckBox.Checked = Options.SolverPullConsumerNodes;
            PullConsumerNodesPowerInput.Value = Math.Min(PullConsumerNodesPowerInput.Maximum, (decimal)Options.SolverPullConsumerNodesPower);

            //lists
            LoadUnfilteredLists();
            UpdateModList();
        }

        private void UpdateModList() {
            var selectedPreset = PresetListBox.SelectedItem as Preset ?? Options.SelectedPreset;
            if (selectedPreset is null)
                return;

            PresetInfo presetInfo = PresetProcessor.ReadPresetInfo(selectedPreset);
            ModSelectionBox.Items.Clear();
            if (presetInfo.ModList != null) {
                List<string> modList = [.. presetInfo.ModList.Select(kvp => kvp.Key + "_" + kvp.Value)];
                modList.Sort();
                ModSelectionBox.Items.AddRange([.. modList]);
            }
            RecipeDifficultyLabel.Text = presetInfo.ExpensiveRecipes ? "Expensive" : "Normal";
            TechnologyDifficultyLabel.Text = presetInfo.ExpensiveTechnology ? "Expensive" : "Normal";

        }

        private void LoadUnfilteredLists() {
            var iconIndex = new EnabledObjectsIconIndex(IconList);

            LoadUnfilteredList(iconIndex, Options.DCache.Assemblers.Values.Where(a => a.EntityType == EntityType.Assembler), unfilteredAssemblerList);
            LoadUnfilteredList(iconIndex, Options.DCache.Assemblers.Values.Where(a => a.EntityType == EntityType.Miner || a.EntityType == EntityType.OffshorePump), unfilteredMinerList);
            LoadUnfilteredList(iconIndex, Options.DCache.Assemblers.Values.Where(a => a.EntityType == EntityType.Boiler || a.EntityType == EntityType.BurnerGenerator || a.EntityType == EntityType.Generator || a.EntityType == EntityType.Reactor), unfilteredPowerList);
            LoadUnfilteredList(iconIndex, Options.DCache.Beacons.Values, unfilteredBeaconList);
            LoadUnfilteredList(iconIndex, Options.DCache.Modules.Values, unfilteredModuleList);
            LoadUnfilteredList(iconIndex, Options.DCache.Recipes.Values, unfilteredRecipeList);
            LoadUnfilteredList(iconIndex, Options.DCache.Qualities.Values, unfilteredQualityList);

            UpdateFilteredLists();
        }

        private void LoadUnfilteredList(EnabledObjectsIconIndex iconIndex, IEnumerable<IDataObjectBase> origin, List<ListViewItem> lviList) {
            var orderedList = origin is IEnumerable<IQuality>
                ? origin.OrderByDescending(a => a.Available).ThenBy(a => a)
                : origin.OrderByDescending(a => a.Available).ThenBy(a => a.FriendlyName);

            foreach (IDataObjectBase dObject in orderedList) {
                var lvItem = new ListViewItem {
                    ImageIndex = iconIndex.GetImageIndex(dObject.Icon),

                    Text = dObject.FriendlyName,
                    Tag = dObject,
                    Name = dObject.Name, //key
                    Checked = true //have to set this to true before (potentially) changing to false in order for the check boxes to appear
                };
                lvItem.Checked = Options.EnabledObjects.Contains(dObject);
                lvItem.BackColor = dObject.Available ? AvailableObjectColor : UnavailableObjectColor;
                lviList.Add(lvItem);
            }
        }

        private void UpdateFilteredLists() {
            UpdateFilteredList(unfilteredAssemblerList, filteredAssemblerList, AssemblerListView);
            UpdateFilteredList(unfilteredMinerList, filteredMinerList, MinerListView);
            UpdateFilteredList(unfilteredPowerList, filteredPowerList, PowerListView);
            UpdateFilteredList(unfilteredBeaconList, filteredBeaconList, BeaconListView);
            UpdateFilteredList(unfilteredModuleList, filteredModuleList, ModuleListView);
            UpdateFilteredList(unfilteredRecipeList, filteredRecipeList, RecipeListView);
            UpdateFilteredList(unfilteredQualityList, filteredQualityList, QualityListView);
        }

        private void UpdateFilteredList(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner) {
            string filterString = FilterTextBox.Text.ToLowerInvariant();
            bool showUnavailables = ShowUnavailablesFilterCheckBox.Checked;

            filteredList.Clear();

            foreach (ListViewItem lvItem in unfilteredList)
                if ((showUnavailables || lvItem.Tag is IDataObjectBase { Available: true }) && (string.IsNullOrEmpty(filterString) || lvItem.Text.Contains(filterString, StringComparison.OrdinalIgnoreCase)))
                    filteredList.Add(lvItem);


            owner.VirtualListSize = filteredList.Count;
            owner.Invalidate();
        }

        //PRESETS LIST------------------------------------------------------------------------------------------
        private void EnableSelectionBox_Enter(object? sender, EventArgs e) { PresetListBox.SelectedItem = null; }
        private void CurrentPresetLabel_Click(object? sender, EventArgs e) { PresetListBox.SelectedItem = null; }

        private void PresetListBox_SelectedValueChanged(object? sender, EventArgs e) {
            UpdateModList();
            CurrentPresetLabel.Font = PresetListBox.SelectedItem == null
                ? new Font(CurrentPresetLabel.Font, FontStyle.Bold)
                : new Font(CurrentPresetLabel.Font, FontStyle.Regular);
        }

        private void PresetListBox_MouseDown(object? sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Right)
                return;

            var index = PresetListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches) {
                var rclickedPreset = ((Preset)PresetListBox.Items[index]);
                PresetListBox.SelectedIndex = index;

                if (rclickedPreset.IsCurrentlySelected) {
                    SelectPresetMenuItem.Text = "Current Preset";
                    SelectPresetMenuItem.Enabled = false;
                } else {
                    SelectPresetMenuItem.Text = "Use This Preset";
                    SelectPresetMenuItem.Enabled = true;
                }
                SelectPresetMenuItem.Enabled = !rclickedPreset.IsCurrentlySelected;
                if (rclickedPreset.IsDefaultPreset) {
                    DeletePresetMenuItem.Text = "Default Preset";
                    DeletePresetMenuItem.Enabled = false;
                } else {
                    DeletePresetMenuItem.Text = "Delete This Preset";
                    DeletePresetMenuItem.Enabled = !rclickedPreset.IsCurrentlySelected;
                }

                PresetMenuStrip.Show(Cursor.Position);
                PresetMenuStrip.Visible = true;
            } else
                PresetMenuStrip.Visible = false;
        }

        private void PresetListBox_MouseDoubleClick(object? sender, MouseEventArgs e) {
            var index = PresetListBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches) {
                Options.SelectedPreset = ((Preset)PresetListBox.Items[index]);
                UpdateSettings();
                DialogResult = DialogResult.OK;
                this.Close();

            }
        }

        private void DeletePresetMenuItem_Click(object? sender, EventArgs e) {
            if (PresetListBox.SelectedItem is Preset selectedPreset && selectedPreset.IsCurrentlySelected && !selectedPreset.IsDefaultPreset) //safety check - should always pass
            {
                if (UserMessages.Show("Are you sure you wish to delete the \"" + selectedPreset.Name + "\" preset? This is irreversible.", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    string jsonPath = PresetProcessor.GetPresetPath(selectedPreset.Name, ".pjson");
                    string customjsonPath = PresetProcessor.GetPresetPath(selectedPreset.Name, ".json");
                    string iconPath = PresetProcessor.GetPresetPath(selectedPreset.Name, ".dat");

                    if (File.Exists(jsonPath))
                        File.Delete(jsonPath);
                    if (File.Exists(customjsonPath))
                        File.Delete(customjsonPath);
                    if (File.Exists(iconPath))
                        File.Delete(iconPath);

                    PresetListBox.Items.Remove(selectedPreset);
                    Options.Presets?.Remove(selectedPreset);
                }
            }
        }

        private void SelectPresetMenuItem_Click(object? sender, EventArgs e) {
            Options.SelectedPreset = PresetListBox.SelectedItem as Preset;
            UpdateSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Filters_Changed(object? sender, EventArgs e) {
            UpdateFilteredLists();
        }

        //LIST VIEWS------------------------------------------------------------------------------------------

        private void ListView_KeyDown(object? sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.A && (e.Modifiers & Keys.Control) != 0 && sender is ListView lv)
                NativeMethods.SelectAllItems(lv);
        }

        private void ListView_MouseClick(object? sender, MouseEventArgs e) {
            if (sender is ListView lv && lv.GetItemAt(e.X, e.Y) is ListViewItem lvi && e.X < (lvi.Bounds.Left + 16)) {
                if (lvi.Selected) //check all selected
                {
                    bool setCheck = !lvi.Checked;
                    foreach (int index in lv.SelectedIndices) {
                        lvi = lv.Items[index];
                        if (lvi.Tag is not IDataObjectBase dob)
                            continue;
                        lvi.Checked = setCheck;
                        if (lvi.Checked)
                            Options.EnabledObjects.Add(dob);
                        else
                            Options.EnabledObjects.Remove(dob);
                    }
                } else {
                    lvi.Checked = !lvi.Checked;
                    if (lvi.Tag is IDataObjectBase dob) {
                        if (lvi.Checked)
                            Options.EnabledObjects.Add(dob);
                        else
                            Options.EnabledObjects.Remove(dob);
                    }
                }
                lv.Invalidate();
            }
        }

        private void ListView_MouseDoubleClick(object? sender, MouseEventArgs e) {
            if (sender is ListView lv && lv.GetItemAt(e.X, e.Y) is ListViewItem lvi && e.X < (lvi.Bounds.Left + 16)) {
                if (lvi.Selected) //check all selected
                {
                    bool setCheck = lvi.Checked;
                    foreach (int index in lv.SelectedIndices) {
                        lvi = lv.Items[index];
                        if (lvi.Tag is not IDataObjectBase dob)
                            continue;
                        lvi.Checked = setCheck;
                        if (lvi.Checked)
                            Options.EnabledObjects.Add(dob);
                        else
                            Options.EnabledObjects.Remove(dob);
                    }
                } else {
                    if (lvi.Tag is IDataObjectBase dob) {
                        if (lvi.Checked)
                            Options.EnabledObjects.Add(dob);
                        else
                            Options.EnabledObjects.Remove(dob);
                    }
                }
                lv.Invalidate();
            }
        }

        private void AssemblerListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredAssemblerList[e.ItemIndex]; }
        private void MinerListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredMinerList[e.ItemIndex]; }
        private void PowerListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredPowerList[e.ItemIndex]; }
        private void BeaconListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredBeaconList[e.ItemIndex]; }
        private void ModuleListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredModuleList[e.ItemIndex]; }
        private void RecipeListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredRecipeList[e.ItemIndex]; }
        private void QualityListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredQualityList[e.ItemIndex]; }

        private void RecipeListView_StartHover(object? sender, MouseEventArgs e) {
            if (sender is not ListView lv)
                return;
            var lvi = lv.GetItemAt(e.Location.X, e.Location.Y);
            var location = new Point(e.X + 15, e.Y);
            if (lvi?.Tag is IRecipe recipe) {
                RecipeToolTip.SetRecipe(recipe);
                RecipeToolTip.Show(lv, location);
            }
        }
        private void RecipeListView_EndHover(object? sender, EventArgs e) {
            if (sender is Control c)
                RecipeToolTip.Hide(c);
        }

        //CONFIRM / RELOAD / CANCEL------------------------------------------------------------------------------------------
        private void ConfirmButton_Click(object? sender, EventArgs e) {
            UpdateSettings();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object? sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void UpdateSettings() {
            Options.QualitySteps = (uint)QualityStepsInput.Value;

            Options.LevelOfDetail = LowLodRadioButton.Checked ? ProductionGraphViewer.LOD.Low : MediumLodRadioButton.Checked ? ProductionGraphViewer.LOD.Medium : ProductionGraphViewer.LOD.High;
            Options.NodeCountForSimpleView = (int)NodeCountForSimpleViewInput.Value;
            Options.IconsOnlyIconSize = (int)IconsSizeInput.Value;

            Options.ArrowsOnLinks = ArrowsOnLinksCheckBox.Checked;
            Options.SimplePassthroughNodes = SimplePassthroughNodesCheckBox.Checked;
            Options.DynamicLinkWidth = DynamicLWCheckBox.Checked;
            Options.AbbreviateSciPacks = AbbreviateSciPackCheckBox.Checked;
            Options.ShowRecipeToolTip = ShowNodeRecipeCheckBox.Checked;
            Options.RoundAssemblerCount = RoundAssemblerCountCheckBox.Checked;
            Options.LockedRecipeEditPanelPosition = RecipeEditPanelPositionLockCheckBox.Checked;
            Options.FlagOUSuppliedNodes = FlagOUSupplyNodesCheckBox.Checked;

            Options.ShowErrorArrows = ErrorArrowsCheckBox.Checked;
            Options.ShowWarningArrows = WarningArrowsCheckBox.Checked;
            Options.ShowDisconnectedArrows = DisconnectedArrowsCheckBox.Checked;
            Options.ShowOUSuppliedArrows = OUSuppliedArrowsCheckBox.Checked;

            Options.DefaultAssemblerStyle = (AssemblerSelector.Style)AssemblerSelectorStyleDropDown.SelectedIndex;
            Options.DefaultModuleStyle = (ModuleSelector.Style)ModuleSelectorStyleDropDown.SelectedIndex;
            Options.DefaultNodeDirection = NodeDirectionDropDown.SelectedIndex == 0 ? NodeDirection.Up : NodeDirection.Down;
            Options.SmartNodeDirection = SmartNodeDirectionCheckBox.Checked;

            Options.EnableExtraProductivityForNonMiners = ShowProductivityBonusOnAllCheckBox.Checked;
            Options.DevShowUnavailableItems = ShowUnavailablesCheckBox.Checked;
            Options.DevUseRecipeBWFilters = !LoadBarrelingCheckBox.Checked;

            Options.SolverLowPriorityPower = (double)LowPriorityPowerInput.Value;
            Options.SolverPullConsumerNodes = PullConsumerNodesCheckBox.Checked;
            Options.SolverPullConsumerNodesPower = (double)PullConsumerNodesPowerInput.Value;

            if (Options.FlagDarkMode != FlagDarkModeCheckBox.Checked) {
                Options.FlagDarkMode = FlagDarkModeCheckBox.Checked;
                if (Options.FlagDarkMode) {
                    mainForm.SetDarkMode();
                } else {
                    mainForm.SetLightMode();
                }
            }
        }

        //PRESET FORMS (Import / compare)------------------------------------------------------------------------------------------

        private void ImportPresetButton_Click(object? sender, EventArgs e) {
            using var form = new PresetImportForm();
            form.StartPosition = FormStartPosition.Manual;
            form.Left = this.Left + 250;
            form.Top = this.Top + 50;
            DialogResult result = form.ShowDialog();

            if (form.ImportStarted)
                GC.Collect(); //we just processed a new preset (either fully or cancelled) - this required the opening of (potentially) alot of zip files and processing of a ton of bitmaps that are now stuck in garbate. In large mod packs like A&B this could clear out 2GB+ of memory.

            if (result == DialogResult.OK && !string.IsNullOrEmpty(form.NewPresetName)) //we have added a new preset
            {
                var newPreset = Options.Presets?.FirstOrDefault(p => string.Equals(p.Name, form.NewPresetName, StringComparison.OrdinalIgnoreCase)); //extra check just in case we were overwriting
                if (newPreset == null) {
                    newPreset = new Preset(form.NewPresetName, false, false);
                    Options.Presets?.Add(newPreset);
                    PresetListBox.Items.Add(newPreset);
                }


                if (newPreset == Options.Presets?[0]) //we have overwritten the currently active preset. Must force a reload
                {
                    Options.RequireReload = true;
                    UpdateSettings();
                    DialogResult = DialogResult.OK;
                    Close();
                } else if (UserMessages.Show("Preset import complete! Do you wish to switch to the new preset?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    Options.SelectedPreset = newPreset;
                    UpdateSettings();
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private void ComparePresetsButton_Click(object? sender, EventArgs e) {
            if (Options.Presets?.Count < 2) {
                UserMessages.Show("Can not compare presets!\n...you only have 1 preset :/");
                return;
            }

            using var form = new PresetComparatorForm();
            form.StartPosition = FormStartPosition.Manual;
            form.Left = this.Left + 50;
            form.Top = this.Top + 50;
            form.ShowDialog();
        }

        //SET ENABLED STATUS------------------------------------------------------------------------------------------

        private void LoadEnabledFromSaveButton_Click(object? sender, EventArgs e) {
            using var form = new SaveFileLoadForm(Options.DCache, Options.EnabledObjects);
            form.StartPosition = FormStartPosition.Manual;
            form.Left = this.Left + 50;
            form.Top = this.Top + 50;
            DialogResult result = form.ShowDialog();

            if (result == DialogResult.OK)
                UpdateEnabledStatus();
            else if (result == DialogResult.Abort)
                UserMessages.Show("Error while reading save file. Try running factorio, opening the save game, saving again, and retrying?");
        }

        private void SetEnabledFromSciencePacksButton_Click(object? sender, EventArgs e) {
            using var form = new SciencePacksLoadForm(Options.DCache, Options.EnabledObjects);
            form.StartPosition = FormStartPosition.Manual;
            form.Left = this.Left + 50;
            form.Top = this.Top + 50;
            DialogResult result = form.ShowDialog();

            if (result == DialogResult.OK)
                UpdateEnabledStatus();
        }

        private void EnableAllButton_Click(object? sender, EventArgs e) {
            Options.EnabledObjects.Clear();
            if (Options.DCache.PlayerAssembler is not null)
                Options.EnabledObjects.Add(Options.DCache.PlayerAssembler);

            foreach (IAssembler assembler in Options.DCache.Assemblers.Values.Where(m => m.AssociatedItems.Any(i => i.Available)))
                Options.EnabledObjects.Add(assembler);

            foreach (IBeacon beacon in Options.DCache.Beacons.Values.Where(m => m.AssociatedItems.Any(i => i.Available)))
                Options.EnabledObjects.Add(beacon);

            foreach (IModule module in Options.DCache.Modules.Values.Where(m => m.AssociatedItem.Available))
                Options.EnabledObjects.Add(module);

            foreach (IRecipe recipe in Options.DCache.Recipes.Values.Where(r => r.Available))
                Options.EnabledObjects.Add(recipe);

            foreach (IQuality quality in Options.DCache.Qualities.Values.Where(r => r.Available))
                Options.EnabledObjects.Add(quality);

            UpdateEnabledStatus();
        }

        private void UpdateEnabledStatus() {
            //this requires a bit of juggling in order to prevent listview (virtual) from throwing a fit. we will ensure filtered lists contain all from unfiltered, then conduct the check updates, then update filtered.

            filteredAssemblerList.Clear();
            filteredAssemblerList.AddRange(unfilteredAssemblerList);
            AssemblerListView.VirtualListSize = filteredAssemblerList.Count;

            filteredBeaconList.Clear();
            filteredBeaconList.AddRange(unfilteredBeaconList);
            BeaconListView.VirtualListSize = filteredBeaconList.Count;

            filteredMinerList.Clear();
            filteredMinerList.AddRange(unfilteredMinerList);
            MinerListView.VirtualListSize = filteredMinerList.Count;

            filteredModuleList.Clear();
            filteredModuleList.AddRange(unfilteredModuleList);
            ModuleListView.VirtualListSize = filteredModuleList.Count;

            filteredPowerList.Clear();
            filteredPowerList.AddRange(unfilteredPowerList);
            PowerListView.VirtualListSize = filteredPowerList.Count;

            filteredRecipeList.Clear();
            filteredRecipeList.AddRange(unfilteredRecipeList);
            RecipeListView.VirtualListSize = filteredRecipeList.Count;

            filteredQualityList.Clear();
            filteredQualityList.AddRange(unfilteredQualityList);
            QualityListView.VirtualListSize = filteredQualityList.Count;

            foreach (ListViewItem item in unfilteredAssemblerList)
                if (item.Tag is IDataObjectBase dob)
                    item.Checked = Options.EnabledObjects.Contains(dob);
            foreach (ListViewItem item in unfilteredBeaconList)
                if (item.Tag is IDataObjectBase dob)
                    item.Checked = Options.EnabledObjects.Contains(dob);
            foreach (ListViewItem item in unfilteredMinerList)
                if (item.Tag is IDataObjectBase dob)
                    item.Checked = Options.EnabledObjects.Contains(dob);
            foreach (ListViewItem item in unfilteredModuleList)
                if (item.Tag is IDataObjectBase dob)
                    item.Checked = Options.EnabledObjects.Contains(dob);
            foreach (ListViewItem item in unfilteredPowerList)
                if (item.Tag is IDataObjectBase dob)
                    item.Checked = Options.EnabledObjects.Contains(dob);
            foreach (ListViewItem item in unfilteredRecipeList)
                if (item.Tag is IDataObjectBase dob)
                    item.Checked = Options.EnabledObjects.Contains(dob);
            foreach (ListViewItem item in unfilteredQualityList)
                if (item.Tag is IDataObjectBase dob)
                    item.Checked = Options.EnabledObjects.Contains(dob);


            UpdateFilteredLists();
        }

        protected override void OnFormClosed(FormClosedEventArgs e) {
            mhDetector.Dispose();
            base.OnFormClosed(e);
        }
    }
}
