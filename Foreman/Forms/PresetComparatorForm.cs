using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class PresetComparatorForm : Form {
        // true means we loaded the presets and are displaying the comparison (preset switching disabled), false means we are selecting presets
        private bool _comparing;

        private DataCache _leftCache;
        private DataCache _rightCache;

        // all of these are of array size 4 (representing the 4 lists):
        // Left Only (from LeftCache),
        // Left (from LeftCache),
        // Right(from RightCache),
        // Right Only (from RightCache)

        // Left and Right ([1] and [2]) have the exact same length.
        // the base lists are populated during initial cache loading and comparison and include the full lists.
        // the unfiltered selected tab list is set to equal one of the base lists based on which tab is selected.
        // the filtered selected tab list is further updated from the unfiltered tab list based on the filter string
        // (and is the one used to populate the 4 item-lists)

        private List<object>[] _unfilteredSelectedTabObjects;
        private List<ListViewItem>[] _unfilteredSelectedTabLvIs;
        private List<ListViewItem>[] _filteredSelectedTabLvIs;

        // strings
        private List<object>[] _unfilteredModTabObjects;
        // Items
        private List<object>[] _unfilteredItemTabObjects;
        // RecipesView
        private List<object>[] _unfilteredRecipeTabObjects;
        // Assemblers
        private List<object>[] _unfilteredAssemblerTabObjects;
        // Assemblers (miners)
        private List<object>[] _unfilteredMinerTabObjects;
        // Assemblers (power generation)
        private List<object>[] _unfilteredPowerTabObjects;
        // Beacons
        private List<object>[] _unfilteredBeaconTabObjects;
        // Modules
        private List<object>[] _unfilteredModuleTabObjects;
        // just a helper array to set unfilteredSelectedTabObjects to the correct value without having to if/switch
        private List<object>[][] _tabSet;

        private static readonly Color EqualBgColor = Color.White;
        private static readonly Color CloseEnoughBgColor = Color.Khaki;
        private static readonly Color DifferentGbColor = Color.Pink;
        private static readonly Color AvailableTextColor = Color.Black;
        private static readonly Color UnavailableTextColor = Color.DarkRed;
        private static readonly Font AvailableTextFont = new(FontFamily.GenericSansSerif, 7.8f, FontStyle.Regular);
        private static readonly Font UnavailableTextFont = new(FontFamily.GenericSansSerif, 7.8f, FontStyle.Italic);

        public PresetComparatorForm() {
            _comparing = false;

            InitializeComponent();
            RightOnlyHeader.Width = RightOnlyListView.Width - 30;
            RightHeader.Width = RightListView.Width - 30;
            LeftHeader.Width = LeftListView.Width - 30;
            LeftOnlyHeader.Width = LeftOnlyListView.Width - 30;
            // scrolling issues if we set it directly, so we set it to the min allowable size and set it to the preferred size here
            Size = new Size(1000, 700);

            TextToolTip.TextFont = new Font(FontFamily.GenericMonospace, 7.8f, FontStyle.Regular);

            var mhDetector = new MouseHoverDetector(100);
            mhDetector.Add(LeftOnlyListView, ListView_StartHover, ListView_EndHover);
            mhDetector.Add(LeftListView, ListView_StartHover, ListView_EndHover);
            mhDetector.Add(RightListView, ListView_StartHover, ListView_EndHover);
            mhDetector.Add(RightOnlyListView, ListView_StartHover, ListView_EndHover);

            LoadPresetOptions();

            _unfilteredModTabObjects = [[], [], [], []];
            _unfilteredItemTabObjects = [[], [], [], []];
            _unfilteredRecipeTabObjects = [[], [], [], []];
            _unfilteredAssemblerTabObjects = [[], [], [], []];
            _unfilteredMinerTabObjects = [[], [], [], []];
            _unfilteredPowerTabObjects = [[], [], [], []];
            _unfilteredBeaconTabObjects = [[], [], [], []];
            _unfilteredModuleTabObjects = [[], [], [], []];

            _tabSet = [
                _unfilteredModTabObjects,
                _unfilteredItemTabObjects,
                _unfilteredRecipeTabObjects,
                _unfilteredAssemblerTabObjects,
                _unfilteredMinerTabObjects,
                _unfilteredPowerTabObjects,
                _unfilteredBeaconTabObjects,
                _unfilteredModuleTabObjects
            ];

            _unfilteredSelectedTabObjects = _tabSet[0];

            _unfilteredSelectedTabLvIs = [[], [], [], []];
            _filteredSelectedTabLvIs = [[], [], [], []];
        }

        private void LoadPresetOptions() {
            var existingPresetFiles = new List<string>();
            foreach (var presetFile in Directory.GetFiles(Path.Combine(Application.StartupPath, "Presets"), "*.pjson")) {
                if (File.Exists(Path.ChangeExtension(presetFile, "dat")))
                    existingPresetFiles.Add(Path.GetFileNameWithoutExtension(presetFile));
            }

            existingPresetFiles.Sort();
            var presets = new List<Preset>();
            // we don't care about default or selected states here.
            foreach (var presetFile in existingPresetFiles)
                presets.Add(new Preset(presetFile, false, false));

            if (existingPresetFiles.Count < 2)
                Close();

            LeftPresetSelectionBox.Items.AddRange(presets.ToArray());
            RightPresetSelectionBox.Items.AddRange(presets.ToArray());
            LeftPresetSelectionBox.SelectedIndex = 0;
            RightPresetSelectionBox.SelectedIndex = 1;
        }

        private void ClearAllLists() {
            LeftOnlyListView.VirtualListSize = 0;
            LeftListView.VirtualListSize = 0;
            RightListView.VirtualListSize = 0;
            RightOnlyListView.VirtualListSize = 0;

            for (var i = 0; i < 4; i++) {
                _unfilteredModTabObjects[i].Clear();
                _unfilteredItemTabObjects[i].Clear();
                _unfilteredRecipeTabObjects[i].Clear();
                _unfilteredAssemblerTabObjects[i].Clear();
                _unfilteredMinerTabObjects[i].Clear();
                _unfilteredPowerTabObjects[i].Clear();
                _unfilteredBeaconTabObjects[i].Clear();
                _unfilteredModuleTabObjects[i].Clear();

                _filteredSelectedTabLvIs[i].Clear();
                _unfilteredSelectedTabLvIs[i].Clear();
            }
        }

        private void ComparePresets() {
            // step 1:
            // load in left and right caches

            using (var form = new DataLoadForm(LeftPresetSelectionBox.SelectedItem as Preset)) {
                form.StartPosition = FormStartPosition.Manual;
                form.Left = Left + 150;
                form.Top = Top + 100;
                // LOAD FACTORIO DATA for left preset
                form.ShowDialog();
                _leftCache = form.GetDataCache();
            }

            using (var form = new DataLoadForm(RightPresetSelectionBox.SelectedItem as Preset)) {
                form.StartPosition = FormStartPosition.Manual;
                form.Left = Left + 150;
                form.Top = Top + 100;
                // LOAD FACTORIO DATA for left preset
                form.ShowDialog();
                _rightCache = form.GetDataCache();
            }

            // step 2:
            // fill in the unfiltered tab lists

            // 2.1:
            // mods

            foreach (var kvp in _leftCache.IncludedMods) {
                if (_rightCache.IncludedMods.ContainsKey(kvp.Key))
                    _unfilteredModTabObjects[1].Add(kvp.Key + "_" + kvp.Value);
                else
                    _unfilteredModTabObjects[0].Add(kvp.Key + "_" + kvp.Value);
            }

            foreach (var kvp in _rightCache.IncludedMods) {
                if (_leftCache.IncludedMods.ContainsKey(kvp.Key))
                    _unfilteredModTabObjects[2].Add(kvp.Key + "_" + kvp.Value);
                else
                    _unfilteredModTabObjects[3].Add(kvp.Key + "_" + kvp.Value);
            }

            for (var i = 0; i < 4; i++)
                _unfilteredModTabObjects[i].Sort((a, b) => string.Compare(((string) a), (string) b, StringComparison.Ordinal));

            // 2.2:
            // items, recipes, assemblers, miners, and modules

            ProcessObject(_leftCache.Items, _rightCache.Items, _unfilteredItemTabObjects);
            ProcessObject(_leftCache.Recipes, _rightCache.Recipes, _unfilteredRecipeTabObjects);
            ProcessObject(_leftCache.Assemblers.Values.Where(a => a.EntityType == EntityType.Assembler).ToDictionary(a => a.Name),
                _rightCache.Assemblers.Values.Where(a => a.EntityType == EntityType.Assembler).ToDictionary(a => a.Name), _unfilteredAssemblerTabObjects);
            ProcessObject(
                _leftCache.Assemblers.Values.Where(a => a.EntityType is EntityType.Miner or EntityType.OffshorePump).ToDictionary(a => a.Name),
                _rightCache.Assemblers.Values.Where(a => a.EntityType is EntityType.Miner or EntityType.OffshorePump).ToDictionary(a => a.Name),
                _unfilteredMinerTabObjects);
            ProcessObject(
                _leftCache.Assemblers.Values
                    .Where(a => a.EntityType is EntityType.Boiler or EntityType.BurnerGenerator or EntityType.Generator or EntityType.Reactor)
                    .ToDictionary(a => a.Name),
                _rightCache.Assemblers.Values
                    .Where(a => a.EntityType is EntityType.Boiler or EntityType.BurnerGenerator or EntityType.Generator or EntityType.Reactor)
                    .ToDictionary(a => a.Name), _unfilteredPowerTabObjects);
            ProcessObject(_leftCache.Beacons.Values.ToDictionary(a => a.Name), _rightCache.Beacons.Values.ToDictionary(a => a.Name),
                _unfilteredBeaconTabObjects);
            ProcessObject(_leftCache.Modules, _rightCache.Modules, _unfilteredModuleTabObjects);

            // process the tab (for the first time) - it will also populate the actual lists.

            UpdateUnfilteredLvIs();
            UpdateFilteredLists();

            return;

            // helpful inner function to process items, recipes, assemblers, miners, and modules (so... everything but mods)
            void ProcessObject<T>(IReadOnlyDictionary<string, T> leftCacheDictionary, IReadOnlyDictionary<string, T> rightCacheDictionary,
                List<object>[] outputLists) where T : DataObjectBase {
                var tempCenterSet = new List<Tuple<T, T>>();
                foreach (var kvp in leftCacheDictionary.OrderByDescending(k => k.Value.Available).ThenBy(k => k.Key)) {
                    if (!rightCacheDictionary.TryGetValue(kvp.Key, out var value))
                        outputLists[0].Add(kvp.Value);
                    else
                        tempCenterSet.Add(new Tuple<T, T>(kvp.Value, value));
                }

                foreach (var kvp in rightCacheDictionary.OrderByDescending(k => k.Value.Available).ThenBy(k => k.Key)) {
                    if (!leftCacheDictionary.ContainsKey(kvp.Key))
                        outputLists[3].Add(kvp.Value);
                }

                // sort the combined center lists together (since they must align)

                tempCenterSet.Sort(delegate(Tuple<T, T> a, Tuple<T, T> b) {
                    var availableDiff = (a.Item1.Available || a.Item2.Available).CompareTo(b.Item1.Available || b.Item2.Available);
                    if (availableDiff != 0) return -availableDiff;
                    return string.Compare(a.Item1.Name, b.Item1.Name, StringComparison.Ordinal);
                });
                foreach (var pair in tempCenterSet) {
                    outputLists[1].Add(pair.Item1);
                    outputLists[2].Add(pair.Item2);
                }
            }
        }

        private void UpdateUnfilteredLvIs() {
            _unfilteredSelectedTabObjects = _tabSet[ComparisonTabControl.SelectedIndex];
            IconList.Images.Clear();
            // 0: mod list (no images)
            IconList.ImageSize = ComparisonTabControl.SelectedIndex == 0 ? new Size(1, 1) : new Size(32, 32);

            if (DataCache.UnknownIcon != null)
                IconList.Images.Add(DataCache.UnknownIcon);

            for (var i = 0; i < 4; i++) {
                _unfilteredSelectedTabLvIs[i].Clear();
                // mod -> string type
                if (ComparisonTabControl.SelectedIndex == 0) {
                    foreach (var obj in _unfilteredSelectedTabObjects[i]) {
                        var lvItem = new ListViewItem {
                            Text = (string) obj
                        };
                        lvItem.Tag = lvItem.Text;
                        lvItem.Name = lvItem.Text;
                        lvItem.ForeColor = AvailableTextColor;
                        lvItem.Font = AvailableTextFont;

                        _unfilteredSelectedTabLvIs[i].Add(lvItem);
                    }
                } else { // item, recipe, assembler, miner, beacon, module -> all are DataObjectBase types
                    foreach (var obj in _unfilteredSelectedTabObjects[i]) {
                        var lvItem = new ListViewItem();
                        var doBase = (DataObjectBase) obj;

                        if (doBase.Icon != null) {
                            IconList.Images.Add(doBase.Icon);
                            lvItem.ImageIndex = IconList.Images.Count - 1;
                        } else
                            lvItem.ImageIndex = 0;

                        lvItem.ForeColor = doBase.Available ? AvailableTextColor : UnavailableTextColor;
                        lvItem.Font = doBase.Available ? AvailableTextFont : UnavailableTextFont;

                        lvItem.Text = doBase.FriendlyName;
                        lvItem.Tag = doBase;
                        // we will use this to filter by (cant filter by friendly name as that can cause the middle 2 to desync)
                        lvItem.Name = doBase.Name.ToLower();
                        _unfilteredSelectedTabLvIs[i].Add(lvItem);
                    }
                }
            }

            // now to process the [1] and [2] (left & right) lists of ListViewItems to set the background
            // to white/yellow/red (equal, close enough, different)
            for (var i = 0; i < _unfilteredSelectedTabLvIs[1].Count; i++) {
                var bgColor = Color.White;
                var l = _unfilteredSelectedTabLvIs[1][i];
                var r = _unfilteredSelectedTabLvIs[2][i];
                var similarNames = l.Text.Equals(r.Text, StringComparison.OrdinalIgnoreCase);
                var similarInternals = true;
                switch (ComparisonTabControl.SelectedIndex) {
                    // mods
                    case 0:
                        // if they are different, mark as red.
                        similarInternals = similarNames;
                        break;
                    // items
                    case 1:
                        similarInternals &= ((Item) l.Tag).Available == ((Item) r.Tag).Available;
                        break;

                    case 2: //recipes
                        var lRecipe = (Recipe) l.Tag;
                        var rRecipe = (Recipe) r.Tag;

                        similarInternals = lRecipe.IngredientList.Count == rRecipe.IngredientList.Count &&
                            lRecipe.ProductList.Count == rRecipe.ProductList.Count;
                        similarInternals &= lRecipe.Available == rRecipe.Available;
                        var exactInternals = similarInternals;
                        var scale = rRecipe.Time / lRecipe.Time;
                        if (similarInternals) {
                            foreach (var lIngredient in lRecipe.IngredientList) {
                                var rIngredient = rRecipe.IngredientList.FirstOrDefault(item => item.Name == lIngredient.Name);
                                similarInternals = similarInternals && rIngredient != null;
                                similarInternals = similarInternals &&
                                    Math.Abs(scale * lRecipe.IngredientSet[lIngredient] / rRecipe.IngredientSet[rIngredient] - 1) < 0.001;
                                exactInternals = exactInternals && similarInternals &&
                                    Math.Abs(lRecipe.IngredientSet[lIngredient] - rRecipe.IngredientSet[rIngredient]) < double.Epsilon;
                            }

                            foreach (var lProduct in lRecipe.ProductList) {
                                if (!similarInternals)
                                    continue;

                                var rProduct = rRecipe.ProductList.FirstOrDefault(item => item.Name == lProduct.Name);
                                similarInternals = similarInternals && rProduct != null;
                                similarInternals = similarInternals &&
                                    Math.Abs(scale * lRecipe.ProductSet[lProduct] / rRecipe.ProductSet[rProduct] - 1) < 0.001;
                                exactInternals = exactInternals && similarInternals &&
                                    Math.Abs(lRecipe.ProductSet[lProduct] - rRecipe.ProductSet[rProduct]) < double.Epsilon;
                            }
                        }

                        // for recipes, we want a 'close enough' in situation where the recipe name is different, and/or when the recipe ratio is the same.
                        similarNames = similarNames && exactInternals;
                        // AKA: 1A + 2B -> 3C is considered as similar enough to 2A + 4B -> 6C
                        break;

                    // assemblers
                    case 3:
                    // miners
                    case 4:
                    // power (aka: assemblers)
                    case 5:
                        var lAssembler = (Assembler) l.Tag;
                        var rAssembler = (Assembler) r.Tag;

                        // TODO: QUALITY UPDATE REQUIRED
                        similarInternals = true; // (lAssembler.Speed == rAssembler.Speed && lAssembler.ModuleSlots == rAssembler.ModuleSlots);
                        break;

                    // beacons
                    case 6:
                        var lBeacon = (Beacon) l.Tag;
                        var rBeacon = (Beacon) r.Tag;

                        similarInternals = lBeacon.ModuleSlots == rBeacon.ModuleSlots;
                        break;

                    // modules
                    case 7:
                        var lModule = (Module) l.Tag;
                        var rModule = (Module) r.Tag;

                        similarInternals = Math.Abs(lModule.GetProductivityBonus() - rModule.GetProductivityBonus()) < double.Epsilon &&
                            Math.Abs(lModule.GetSpeedBonus() - rModule.GetSpeedBonus()) < double.Epsilon &&
                            Math.Abs(lModule.GetConsumptionBonus() - rModule.GetConsumptionBonus()) < double.Epsilon &&
                            Math.Abs(lModule.GetSpeedBonus() - rModule.GetSpeedBonus()) < double.Epsilon &&
                            Math.Abs(lModule.GetQualityBonus() - rModule.GetQualityBonus()) < double.Epsilon;

                        break;
                }

                bgColor = similarInternals ? similarNames ? EqualBgColor : CloseEnoughBgColor : DifferentGbColor;
                _unfilteredSelectedTabLvIs[1][i].BackColor = bgColor;
                _unfilteredSelectedTabLvIs[2][i].BackColor = bgColor;
            }
        }

        private void UpdateFilteredLists() {
            var filter = FilterTextBox.Text.ToLower();
            var hideEqual = HideEqualObjectsCheckBox.Checked;
            var hideSimilar = HideSimilarObjectsCheckBox.Checked;
            var showUnavailable = ShowUnavailableCheckBox.Checked;

            // complete filter for LeftOnly and RightOnly sets ([0] and [3])
            // so... for i=0 and i=3 only (Left Only and Right Only)
            for (var i = 0; i < 4; i += 3) {
                _filteredSelectedTabLvIs[i].Clear();

                foreach (var lvItem in _unfilteredSelectedTabLvIs[i])
                    if (showUnavailable || lvItem.Tag is not DataObjectBase dObj || dObj.Available)
                        if (lvItem.Name.Contains(filter) || lvItem.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1)
                            _filteredSelectedTabLvIs[i].Add(lvItem);
            }

            // complete filter for Left&Right sets (have to process at the same time, since if a name fits the filter in one (but not the other),
            // both are still added to maintain parity)
            _filteredSelectedTabLvIs[1].Clear();
            _filteredSelectedTabLvIs[2].Clear();
            // remember: [1] and [2] both have the EXACT same # of items
            for (var j = 0; j < _unfilteredSelectedTabLvIs[1].Count; j++) {
                var leftLvi = _unfilteredSelectedTabLvIs[1][j];
                var rightLvi = _unfilteredSelectedTabLvIs[2][j];

                if (!showUnavailable
                    && (leftLvi.Tag is DataObjectBase ldObj && rightLvi.Tag is DataObjectBase rdObj)
                    && !ldObj.Available
                    && !rdObj.Available) {
                    continue;
                }

                if ((hideEqual && leftLvi.BackColor == EqualBgColor)
                    || (hideSimilar && leftLvi.BackColor == CloseEnoughBgColor)
                    || (!leftLvi.Name.Contains(filter)
                        //&& rightLVI.Name.Contains(filter) //name of [1][j] and [2][j] are the same, don't have to check twice
                        && leftLvi.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) == -1
                        && rightLvi.Text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) == -1)) {
                    continue;
                }

                _filteredSelectedTabLvIs[1].Add(leftLvi);
                _filteredSelectedTabLvIs[2].Add(rightLvi);
            }

            // update listviews

            LeftOnlyListView.VirtualListSize = _filteredSelectedTabLvIs[0].Count;
            LeftListView.VirtualListSize = _filteredSelectedTabLvIs[1].Count;
            RightListView.VirtualListSize = _filteredSelectedTabLvIs[2].Count;
            RightOnlyListView.VirtualListSize = _filteredSelectedTabLvIs[3].Count;
            LeftOnlyListView.Invalidate();
            LeftListView.Invalidate();
            RightListView.Invalidate();
            RightOnlyListView.Invalidate();
        }

        private void ProcessPresetsButton_Click(object sender, EventArgs e) {
            _comparing = !_comparing;
            if (_comparing) {
                ComparePresets();
            } else {
                ClearAllLists();
                _leftCache.Clear();
                _leftCache = null;
                _rightCache.Clear();
                _rightCache = null;

                // we just closed 2 DataCaches... this is pretty large.
                GC.Collect();
            }

            PresetSelectionGroup.Enabled = !_comparing;
            ProcessPresetsButton.Text = _comparing ? "Select Other Presets" : "Read Presets And Compare";
        }

        // either of the two
        private void PresetSelectionBox_SelectedValueChanged(object sender, EventArgs e) {
            ProcessPresetsButton.Enabled = LeftPresetSelectionBox.SelectedIndex != RightPresetSelectionBox.SelectedIndex;
            ProcessPresetsButton.Text = ProcessPresetsButton.Enabled ? "Read Presets And Compare" : "Cant Compare Preset To Itself";
        }

        private void PresetComparatorForm_FormClosed(object sender, FormClosedEventArgs e) {
            if (!_comparing)
                return;

            _comparing = false;
            ClearAllLists();

            _leftCache.Clear();
            _leftCache = null;
            _rightCache.Clear();
            _rightCache = null;

            GC.Collect();
        }

        private void ComparisonTabControl_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateUnfilteredLvIs();
            UpdateFilteredLists();
        }

        private void Filters_Changed(object sender, EventArgs e) {
            UpdateFilteredLists();
        }

        private void LeftOnlyListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredSelectedTabLvIs[0][e.ItemIndex];
        }

        private void LeftListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredSelectedTabLvIs[1][e.ItemIndex];
        }

        private void RightListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredSelectedTabLvIs[2][e.ItemIndex];
        }

        private void RightOnlyListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredSelectedTabLvIs[3][e.ItemIndex];
        }

        private void RightOnlyListView_Resize(object sender, EventArgs e) {
            RightOnlyHeader.Width = RightOnlyListView.Width - 30;
        }

        private void RightListView_Resize(object sender, EventArgs e) {
            RightHeader.Width = RightListView.Width - 30;
        }

        private void LeftListView_Resize(object sender, EventArgs e) {
            LeftHeader.Width = LeftListView.Width - 30;
        }

        private void LeftOnlyListView_Resize(object sender, EventArgs e) {
            LeftOnlyHeader.Width = LeftOnlyListView.Width - 30;
        }

        private void LeftOnlyListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
        } // if (e.IsSelected) e.Item.Selected = false; }

        private void LeftListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
            RightListView.SelectedIndices.Clear();
            RightListView.SelectedIndices.Add(e.ItemIndex);
        }

        private void RightListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
            if (LeftListView.SelectedIndices.Count != 0 && LeftListView.SelectedIndices[0] == e.ItemIndex)
                return;

            LeftListView.SelectedIndices.Clear();
            LeftListView.SelectedIndices.Add(e.ItemIndex);
        }

        private void RightOnlyListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e) {
        } //if (e.IsSelected) e.Item.Selected = false; }

        private void ListView_StartHover(object sender, MouseEventArgs e) {
            var lLvi = ((ListView) sender).GetItemAt(e.Location.X, e.Location.Y);
            if (lLvi == null)
                return;

            var location = new Point(e.X + 15, e.Y);
            ListViewItem rLvi = null;
            var compareTypeTt = sender == LeftListView || sender == RightListView;
            if (compareTypeTt) {
                lLvi = LeftListView.Items[lLvi.Index];
                rLvi = RightListView.Items[lLvi.Index];
            }

            if (lLvi.Tag is Recipe recipe) {
                RecipeToolTip.SetRecipe(recipe, compareTypeTt ? rLvi.Tag as Recipe : null);
                RecipeToolTip.Show((Control) sender, location);
            } else if (lLvi.Tag is Assembler assembler) { // assembler, miner, or power // TODO: QUALITY UPDATE REQUIRED
                var left = assembler.FriendlyName + "\n" +
                    $"   Speed:         {assembler.GetSpeed(assembler.Owner.DefaultQuality)}x\n" +
                    $"   Module Slots:  {assembler.ModuleSlots}";
                var right = "";
                if (compareTypeTt) {
                    var rAssembler = rLvi.Tag as Assembler;
                    right = rAssembler.FriendlyName + "\n" +
                        $"   Speed:         {rAssembler.GetSpeed(assembler.Owner.DefaultQuality)}x\n" +
                        $"   Module Slots:  {rAssembler.ModuleSlots}";
                }

                TextToolTip.SetText(left, right);
                TextToolTip.Show((Control) sender, location);
            } else if (lLvi.Tag is Beacon beacon) {
                var left = beacon.FriendlyName + "\n" +
                    $"   Module Slots:  {beacon.ModuleSlots}";
                var right = "";
                if (compareTypeTt) {
                    var rBeacon = rLvi.Tag as Beacon;
                    right = rBeacon.FriendlyName + "\n" +
                        $"   Module Slots:  {rBeacon.ModuleSlots}";
                }

                TextToolTip.SetText(left, right);
                TextToolTip.Show((Control) sender, location);
            } else if (lLvi.Tag is Module module) {
                var left = module.FriendlyName + "\n" +
                    $"   Productivity bonus: {module.GetProductivityBonus():%0}\n" +
                    $"   Speed bonus:        {module.GetSpeedBonus():%0}\n" +
                    $"   Efficiency bonus:   {(-module.GetConsumptionBonus()):%0}\n" +
                    $"   Pollution bonus:    {module.GetPollutionBonus():%0}" +
                    $"   Quality bonus:      {module.GetQualityBonus():%0}";
                var right = "";
                if (compareTypeTt) {
                    var rModule = rLvi.Tag as Module;
                    right = rModule.FriendlyName + "\n" +
                        $"   Productivity bonus: {rModule.GetProductivityBonus():%0}\n" +
                        $"   Speed bonus:        {rModule.GetSpeedBonus():%0}\n" +
                        $"   Efficiency bonus:   {(-rModule.GetConsumptionBonus()):%0}\n" +
                        $"   Pollution bonus:    {rModule.GetPollutionBonus():%0}" +
                        $"   Quality bonus:      {rModule.GetQualityBonus():%0}";
                }

                TextToolTip.SetText(left, right);
                TextToolTip.Show((Control) sender, location);
            }
        }

        private void ListView_EndHover(object sender, EventArgs e) {
            RecipeToolTip.Hide((Control) sender);
            TextToolTip.Hide((Control) sender);
        }
    }
}