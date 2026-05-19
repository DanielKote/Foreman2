using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class PresetComparatorForm : Form {
        private bool Comparing; //true means we loaded the presets and are displaying the comparison (preset switching disabled), false means we are selecting presets
        private DataCache? LeftCache;
        private DataCache? RightCache;
        private readonly MouseHoverDetector MouseHoverDetector;

        //all of these are of array size 4 (representing the 4 lists) : Left Only (from LeftCache), Left (from LeftCache), Right(from RightCache), Right Only (from RightCache)
        //Left and Right ([1] and [2]) have the exact same length.
        //the base lists are populated during initial cache loading and comparison and include the full lists.
        //the unfiltered selected tab list is set to equal one of the base lists based on which tab is selected.
        //the filtered selected tab list is further updated from the unfiltered tab list based on the filter string (and is the one used to populate the 4 item-lists)
        private List<object>[] unfilteredSelectedTabObjects;
        private readonly List<ListViewItem>[] unfilteredSelectedTabLVIs;
        private readonly List<ListViewItem>[] filteredSelectedTabLVIs;

        private readonly List<object>[] unfilteredModTabObjects; //strings
        private readonly List<object>[] unfilteredItemTabObjects; //Items
        private readonly List<object>[] unfilteredRecipeTabObjects; //Recipes
        private readonly List<object>[] unfilteredAssemblerTabObjects; //Assemblers
        private readonly List<object>[] unfilteredMinerTabObjects; //Assemblers (miners)
        private readonly List<object>[] unfilteredPowerTabObjects; //Assemblers (power generation)
        private readonly List<object>[] unfilteredBeaconTabObjects; //Beacons
        private readonly List<object>[] unfilteredModuleTabObjects; //Modules
        private readonly List<object>[][] tabSet; //just a helper array to set unfilteredSelectedTabObjects to the correct value without having to if/switch

        private static readonly Color EqualBGColor = Color.White;
        private static readonly Color CloseEnoughBGColor = Color.Khaki;
        private static readonly Color DifferentGBColor = Color.Pink;
        private static readonly Color AvailableTextColor = Color.Black;
        private static readonly Color UnavailableTextColor = Color.DarkRed;
        private static readonly Font AvailableTextFont = new(FontFamily.GenericSansSerif, 7.8f, FontStyle.Regular);
        private static readonly Font UnavailableTextFont = new(FontFamily.GenericSansSerif, 7.8f, FontStyle.Italic);

        public PresetComparatorForm() {
            Comparing = false;

            InitializeComponent();
            RightOnlyHeader.Width = RightOnlyListView.Width - 30;
            RightHeader.Width = RightListView.Width - 30;
            LeftHeader.Width = LeftListView.Width - 30;
            LeftOnlyHeader.Width = LeftOnlyListView.Width - 30;
            this.Size = new Size(1000, 700); //scrolling issues if we set it directly, so we set it to the min allowable size and set it to the preferred size here

            TextToolTip.TextFont = new Font(FontFamily.GenericMonospace, 7.8f, FontStyle.Regular);

            MouseHoverDetector = new MouseHoverDetector(100, 200);
            MouseHoverDetector.Add(LeftOnlyListView, ListView_StartHover, ListView_EndHover);
            MouseHoverDetector.Add(LeftListView, ListView_StartHover, ListView_EndHover);
            MouseHoverDetector.Add(RightListView, ListView_StartHover, ListView_EndHover);
            MouseHoverDetector.Add(RightOnlyListView, ListView_StartHover, ListView_EndHover);

            LoadPresetOptions();

            unfilteredModTabObjects = [[], [], [], []];
            unfilteredItemTabObjects = [[], [], [], []];
            unfilteredRecipeTabObjects = [[], [], [], []];
            unfilteredAssemblerTabObjects = [[], [], [], []];
            unfilteredMinerTabObjects = [[], [], [], []];
            unfilteredPowerTabObjects = [[], [], [], []];
            unfilteredBeaconTabObjects = [[], [], [], []];
            unfilteredModuleTabObjects = [[], [], [], []];

            tabSet = [
                unfilteredModTabObjects,
                unfilteredItemTabObjects,
                unfilteredRecipeTabObjects,
                unfilteredAssemblerTabObjects,
                unfilteredMinerTabObjects,
                unfilteredPowerTabObjects,
                unfilteredBeaconTabObjects,
                unfilteredModuleTabObjects
            ];

            unfilteredSelectedTabObjects = tabSet[0];

            unfilteredSelectedTabLVIs = [[], [], [], []];
            filteredSelectedTabLVIs = [[], [], [], []];

        }

        private void LoadPresetOptions() {
            var existingPresetFiles = new List<string>();
            foreach (string presetFile in Directory.GetFiles(Path.Combine(Application.StartupPath, "Presets"), "*.pjson"))
                if (File.Exists(Path.ChangeExtension(presetFile, "dat")))
                    existingPresetFiles.Add(Path.GetFileNameWithoutExtension(presetFile));
            existingPresetFiles.Sort();
            var Presets = new List<Preset>();
            foreach (string presetFile in existingPresetFiles)
                Presets.Add(new Preset(presetFile, false, false)); //we dont care about default or selected states here.

            if (existingPresetFiles.Count < 2)
                this.Close();

            LeftPresetSelectionBox.Items.AddRange([.. Presets]);
            RightPresetSelectionBox.Items.AddRange([.. Presets]);
            LeftPresetSelectionBox.SelectedIndex = 0;
            RightPresetSelectionBox.SelectedIndex = 1;
        }

        private void ClearAllLists() {
            LeftOnlyListView.VirtualListSize = 0;
            LeftListView.VirtualListSize = 0;
            RightListView.VirtualListSize = 0;
            RightOnlyListView.VirtualListSize = 0;

            for (int i = 0; i < 4; i++) {
                unfilteredModTabObjects[i].Clear();
                unfilteredItemTabObjects[i].Clear();
                unfilteredRecipeTabObjects[i].Clear();
                unfilteredAssemblerTabObjects[i].Clear();
                unfilteredMinerTabObjects[i].Clear();
                unfilteredPowerTabObjects[i].Clear();
                unfilteredBeaconTabObjects[i].Clear();
                unfilteredModuleTabObjects[i].Clear();

                filteredSelectedTabLVIs[i].Clear();
                unfilteredSelectedTabLVIs[i].Clear();
            }
        }

        private void ComparePresets() {
            //helpful inner function to process items, recipes, assemblers, miners, and modules (so... everything but mods)
            static void ProcessObject<T>(IReadOnlyDictionary<string, T>? leftCacheDictionary, IReadOnlyDictionary<string, T>? rightCacheDictionary, List<object>[]? outputLists) where T : IDataObjectBase {
                if (leftCacheDictionary is null || rightCacheDictionary is null || outputLists is null)
                    return;
                var tempCenterSet = new List<Tuple<T, T>>();
                foreach (var kvp in leftCacheDictionary.OrderByDescending(k => ((IDataObjectBase)k.Value).Available).ThenBy(k => k.Key)) {
                    if (!rightCacheDictionary.ContainsKey(kvp.Key))
                        outputLists[0].Add(kvp.Value);
                    else
                        tempCenterSet.Add(new Tuple<T, T>(kvp.Value, rightCacheDictionary[kvp.Key]));
                }
                foreach (var kvp in rightCacheDictionary.OrderByDescending(k => ((IDataObjectBase)k.Value).Available).ThenBy(k => k.Key)) {
                    if (!leftCacheDictionary.ContainsKey(kvp.Key))
                        outputLists[3].Add(kvp.Value);
                }

                //sort the combined center lists together (since they must align)
                tempCenterSet.Sort(delegate (Tuple<T, T> a, Tuple<T, T> b) {
                    int availableDiff = (a.Item1.Available || a.Item2.Available).CompareTo((b.Item1.Available || b.Item2.Available));
                    return availableDiff != 0 ? -availableDiff : string.Compare(a.Item1.Name, b.Item1.Name, StringComparison.Ordinal);
                });
                foreach (Tuple<T, T> pair in tempCenterSet) {
                    outputLists[1].Add(pair.Item1);
                    outputLists[2].Add(pair.Item2);
                }
            }

            if (LeftPresetSelectionBox.SelectedItem is not Preset leftPreset ||
                RightPresetSelectionBox.SelectedItem is not Preset rightPreset)
                return;

            //step 1: load in left and right caches
            using (var form = new DataLoadForm(leftPreset)) {
                form.StartPosition = FormStartPosition.Manual;
                form.Left = this.Left + 150;
                form.Top = this.Top + 100;
                form.ShowDialog(); //LOAD FACTORIO DATA for left preset
                LeftCache = form.GetDataCache();
            }
            using (var form = new DataLoadForm(rightPreset)) {
                form.StartPosition = FormStartPosition.Manual;
                form.Left = this.Left + 150;
                form.Top = this.Top + 100;
                form.ShowDialog(); //LOAD FACTORIO DATA for left preset
                RightCache = form.GetDataCache();
            }

            //step 2: fill in the unfiltered tab lists

            //2.1: mods
            foreach (var kvp in LeftCache?.IncludedMods.AsEnumerable() ?? []) {
                if (RightCache?.IncludedMods.ContainsKey(kvp.Key) is true)
                    unfilteredModTabObjects[1].Add(kvp.Key + "_" + kvp.Value);
                else
                    unfilteredModTabObjects[0].Add(kvp.Key + "_" + kvp.Value);
            }
            foreach (var kvp in RightCache?.IncludedMods.AsEnumerable() ?? []) {
                if (LeftCache?.IncludedMods.ContainsKey(kvp.Key) is true)
                    unfilteredModTabObjects[2].Add(kvp.Key + "_" + kvp.Value);
                else
                    unfilteredModTabObjects[3].Add(kvp.Key + "_" + kvp.Value);
            }
            for (int i = 0; i < 4; i++)
                unfilteredModTabObjects[i].Sort(delegate (object a, object b) { return string.Compare((string)a, (string)b, StringComparison.Ordinal); });

            //2.2: items, recipes, assemblers, miners, and modules
            ProcessObject(LeftCache?.Items, RightCache?.Items, unfilteredItemTabObjects);
            ProcessObject(LeftCache?.Recipes, RightCache?.Recipes, unfilteredRecipeTabObjects);
            ProcessObject(LeftCache?.Assemblers.Values.Where(a => a.EntityType == EntityType.Assembler).ToDictionary(a => a.Name), RightCache?.Assemblers.Values.Where(a => a.EntityType == EntityType.Assembler).ToDictionary(a => a.Name), unfilteredAssemblerTabObjects);
            ProcessObject(LeftCache?.Assemblers.Values.Where(a => a.EntityType == EntityType.Miner || a.EntityType == EntityType.OffshorePump).ToDictionary(a => a.Name), RightCache?.Assemblers.Values.Where(a => a.EntityType == EntityType.Miner || a.EntityType == EntityType.OffshorePump).ToDictionary(a => a.Name), unfilteredMinerTabObjects);
            ProcessObject(LeftCache?.Assemblers.Values.Where(a => a.EntityType == EntityType.Boiler || a.EntityType == EntityType.BurnerGenerator || a.EntityType == EntityType.Generator || a.EntityType == EntityType.Reactor).ToDictionary(a => a.Name), RightCache?.Assemblers.Values.Where(a => a.EntityType == EntityType.Boiler || a.EntityType == EntityType.BurnerGenerator || a.EntityType == EntityType.Generator || a.EntityType == EntityType.Reactor).ToDictionary(a => a.Name), unfilteredPowerTabObjects);
            ProcessObject(LeftCache?.Beacons.Values.ToDictionary(a => a.Name), RightCache?.Beacons.Values.ToDictionary(a => a.Name), unfilteredBeaconTabObjects);
            ProcessObject(LeftCache?.Modules, RightCache?.Modules, unfilteredModuleTabObjects);

            //process the tab (for the first time) - it will also populate the actual lists.
            UpdateUnfilteredLVIs();
            UpdateFilteredLists();
        }

        private void UpdateUnfilteredLVIs() {
            unfilteredSelectedTabObjects = tabSet[ComparisonTabControl.SelectedIndex];
            IconList.Images.Clear();
            IconList.ImageSize = (ComparisonTabControl.SelectedIndex == 0 ? new Size(1, 1) : new Size(32, 32)); //0: mod list (no images)

            if (DataCache.UnknownIcon != null)
                IconList.Images.Add(DataCache.UnknownIcon);

            for (int i = 0; i < 4; i++) {
                unfilteredSelectedTabLVIs[i].Clear();
                if (ComparisonTabControl.SelectedIndex == 0) //mod -> string type
                {
                    foreach (object obj in unfilteredSelectedTabObjects[i]) {
                        var lvItem = new ListViewItem {
                            Text = (string)obj
                        };
                        lvItem.Tag = lvItem.Text;
                        lvItem.Name = lvItem.Text;
                        lvItem.ForeColor = AvailableTextColor;
                        lvItem.Font = AvailableTextFont;

                        unfilteredSelectedTabLVIs[i].Add(lvItem);
                    }
                } else //item,recipe,assembler,miner,beacon,module -> all are IDataObjectBase types
                  {
                    foreach (object obj in unfilteredSelectedTabObjects[i]) {
                        var lvItem = new ListViewItem();
                        var doBase = (IDataObjectBase)obj;

                        if (doBase.Icon != null) {
                            IconList.Images.Add(doBase.Icon);
                            lvItem.ImageIndex = IconList.Images.Count - 1;
                        } else
                            lvItem.ImageIndex = 0;

                        lvItem.ForeColor = doBase.Available ? AvailableTextColor : UnavailableTextColor;
                        lvItem.Font = doBase.Available ? AvailableTextFont : UnavailableTextFont;

                        lvItem.Text = doBase.FriendlyName;
                        lvItem.Tag = doBase;
                        lvItem.Name = doBase.Name.ToLowerInvariant(); //we will use this to filter by (cant filter by friendly name as that can cause the middle 2 to desync)
                        unfilteredSelectedTabLVIs[i].Add(lvItem);
                    }
                }
            }

            //now to process the [1] and [2] (left & right) lists of ListViewItems to set the background to white/yellow/red (equal, close enough, different)
            for (int i = 0; i < unfilteredSelectedTabLVIs[1].Count; i++) {
                Color bgColor = Color.White;
                ListViewItem l = unfilteredSelectedTabLVIs[1][i];
                ListViewItem r = unfilteredSelectedTabLVIs[2][i];
                bool similarNames = l.Text.Equals(r.Text, StringComparison.OrdinalIgnoreCase);
                bool similarInternals = true;
                switch (ComparisonTabControl.SelectedIndex) {
                    case 0: //mods
                        similarInternals = similarNames; //if the are different, mark as red.
                        break;
                    case 1: //items
                        similarInternals &= (l.Tag as IItem)?.Available == (r.Tag as IItem)?.Available;
                        break;

                    case 2: //recipes
                        var lRecipe = l.Tag as IRecipe;
                        var rRecipe = r.Tag as IRecipe;

                        similarInternals = (lRecipe?.IngredientList.Count == rRecipe?.IngredientList.Count) && (lRecipe?.ProductList.Count == rRecipe?.ProductList.Count);
                        similarInternals &= (lRecipe?.Available == rRecipe?.Available);
                        bool exactInternals = similarInternals;
                        double scale = (rRecipe?.Time / lRecipe?.Time) ?? 0;
                        if (similarInternals) {
                            foreach (IItem lingredient in lRecipe?.IngredientList ?? []) {
                                var ringredient = rRecipe?.IngredientList.FirstOrDefault(item => item.Name == lingredient.Name);
                                similarInternals = similarInternals && ringredient is not null && lRecipe is not null && rRecipe is not null &&
                                    (Math.Abs((scale * lRecipe.IngredientSet[lingredient] / rRecipe.IngredientSet[ringredient]) - 1) < 0.001);
                                // Roslyn isn't smart enough to figure out ringredient from similarInternals. Just check it again.
                                exactInternals = exactInternals && similarInternals && ringredient is not null && (lRecipe?.IngredientSet[lingredient] == rRecipe?.IngredientSet[ringredient]);
                            }
                            foreach (IItem lproduct in lRecipe?.ProductList ?? []) {
                                if (similarInternals) {
                                    var rproduct = rRecipe?.ProductList.FirstOrDefault(item => item.Name == lproduct.Name);
                                    similarInternals = similarInternals && (rproduct != null) && lRecipe is not null && rRecipe is not null &&
                                        (Math.Abs((scale * lRecipe.ProductSet[lproduct] / rRecipe.ProductSet[rproduct]) - 1) < 0.001);
                                    exactInternals = exactInternals && similarInternals && rproduct is not null && (lRecipe?.ProductSet[lproduct] == rRecipe?.ProductSet[rproduct]);
                                }
                            }
                        }
                        similarNames = similarNames && exactInternals; //for recipes, we want a 'close enough' in situation where the recipe name is different, and/or when the recipe ratio is the same.
                                                                       //AKA: 1A+2B->3C is considered as similar enough to 2A+4B->6C
                        break;

                    case 3: //assemblers
                    case 4: //miners
                    case 5: //power (aka: assemblers)
                        var lAssembler = l.Tag as IAssembler;
                        var rAssembler = r.Tag as IAssembler;

                        similarInternals = true; // (lAssembler.Speed == rAssembler.Speed && lAssembler.ModuleSlots == rAssembler.ModuleSlots);  //QUALITY UPDATE REQUIRED
                        break;
                    case 6: //beacons
                        var lBeacon = l.Tag as IBeacon;
                        var rBeacon = r.Tag as IBeacon;

                        similarInternals = (lBeacon?.ModuleSlots == rBeacon?.ModuleSlots);
                        break;
                    case 7: //modules
                        var lModule = l.Tag as IModule;
                        var rModule = r.Tag as IModule;

                        similarInternals = lModule is not null && rModule is not null &&
                            lModule.GetProductivityBonus() == rModule.GetProductivityBonus() &&
                            lModule.GetSpeedBonus() == rModule.GetSpeedBonus() &&
                            lModule.GetConsumptionBonus() == rModule.GetConsumptionBonus() &&
                            lModule.GetSpeedBonus() == rModule.GetSpeedBonus() &&
                            lModule.GetQualityBonus() == rModule.GetQualityBonus();

                        break;
                }

                bgColor = similarInternals ? (similarNames ? EqualBGColor : CloseEnoughBGColor) : DifferentGBColor;
                unfilteredSelectedTabLVIs[1][i].BackColor = bgColor;
                unfilteredSelectedTabLVIs[2][i].BackColor = bgColor;
            }

        }

        private void UpdateFilteredLists() {
            string filter = FilterTextBox.Text.ToLowerInvariant();
            bool hideEqual = HideEqualObjectsCheckBox.Checked;
            bool hideSimilar = HideSimilarObjectsCheckBox.Checked;
            bool showUnavailable = ShowUnavailableCheckBox.Checked;

            //complete filter for LeftOnly and RightOnly sets ([0] and [3])
            for (int i = 0; i < 4; i += 3) //so... for i=0 and i=3 only (Left Only and Right Only)
            {
                filteredSelectedTabLVIs[i].Clear();

                foreach (ListViewItem lvItem in unfilteredSelectedTabLVIs[i])
                    if (showUnavailable || lvItem.Tag is not IDataObjectBase dObj || dObj.Available)
                        if (lvItem.Name.Contains(filter) || lvItem.Text.Contains(filter, StringComparison.OrdinalIgnoreCase))
                            filteredSelectedTabLVIs[i].Add(lvItem);
            }

            //complete filter for Left&Right sets (have to process at the same time, since if a name fits the filter in one (but not the other), both are still added to maintain parity)
            filteredSelectedTabLVIs[1].Clear();
            filteredSelectedTabLVIs[2].Clear();
            for (int j = 0; j < unfilteredSelectedTabLVIs[1].Count; j++) //remember: [1] and [2] both have the EXACT same # of items)
            {
                var leftLVI = (ListViewItem)unfilteredSelectedTabLVIs[1][j];
                var rightLVI = (ListViewItem)unfilteredSelectedTabLVIs[2][j];

                if (showUnavailable || !(leftLVI.Tag is IDataObjectBase ldObj && rightLVI.Tag is IDataObjectBase rdObj) || ldObj.Available || rdObj.Available) {

                    if (!(hideEqual && leftLVI.BackColor == EqualBGColor) && !(hideSimilar && leftLVI.BackColor == CloseEnoughBGColor) && (
                    leftLVI.Name.Contains(filter) ||
                    //rightLVI.Name.Contains(filter) //name of [1][j] and [2][j] are the same, dont have to check twice
                    leftLVI.Text.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    rightLVI.Text.Contains(filter, StringComparison.OrdinalIgnoreCase))) {
                        filteredSelectedTabLVIs[1].Add(leftLVI);
                        filteredSelectedTabLVIs[2].Add(rightLVI);
                    }
                }
            }

            //update listviews
            LeftOnlyListView.VirtualListSize = filteredSelectedTabLVIs[0].Count;
            LeftListView.VirtualListSize = filteredSelectedTabLVIs[1].Count;
            RightListView.VirtualListSize = filteredSelectedTabLVIs[2].Count;
            RightOnlyListView.VirtualListSize = filteredSelectedTabLVIs[3].Count;
            LeftOnlyListView.Invalidate();
            LeftListView.Invalidate();
            RightListView.Invalidate();
            RightOnlyListView.Invalidate();
        }

        private void ProcessPresetsButton_Click(object? sender, EventArgs e) {
            Comparing = !Comparing;
            if (Comparing) {
                ComparePresets();
            } else {
                ClearAllLists();
                LeftCache?.Clear();
                LeftCache = null;
                RightCache?.Clear();
                RightCache = null;

                GC.Collect(); //we just closed 2 DataCaches... this is pretty large.
            }
            PresetSelectionGroup.Enabled = !Comparing;
            ProcessPresetsButton.Text = Comparing ? "Select Other Presets" : "Read Presets And Compare";
        }

        private void PresetSelectionBox_SelectedValueChanged(object? sender, EventArgs e) //either of the two
        {
            ProcessPresetsButton.Enabled = (LeftPresetSelectionBox.SelectedIndex != RightPresetSelectionBox.SelectedIndex);
            ProcessPresetsButton.Text = ProcessPresetsButton.Enabled ? "Read Presets And Compare" : "Cant Compare Preset To Itself";
        }

        private void PresetComparatorForm_FormClosed(object? sender, FormClosedEventArgs e) {
            if (Comparing) {
                Comparing = false;
                ClearAllLists();

                LeftCache?.Clear();
                LeftCache = null;
                RightCache?.Clear();
                RightCache = null;

                GC.Collect();
            }
        }

        private void ComparisonTabControl_SelectedIndexChanged(object? sender, EventArgs e) { UpdateUnfilteredLVIs(); UpdateFilteredLists(); }
        private void Filters_Changed(object? sender, EventArgs e) { UpdateFilteredLists(); }

        private void LeftOnlyListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredSelectedTabLVIs[0][e.ItemIndex]; }
        private void LeftListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredSelectedTabLVIs[1][e.ItemIndex]; }
        private void RightListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredSelectedTabLVIs[2][e.ItemIndex]; }
        private void RightOnlyListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredSelectedTabLVIs[3][e.ItemIndex]; }

        private void RightOnlyListView_Resize(object? sender, EventArgs e) { RightOnlyHeader.Width = RightOnlyListView.Width - 30; }
        private void RightListView_Resize(object? sender, EventArgs e) { RightHeader.Width = RightListView.Width - 30; }
        private void LeftListView_Resize(object? sender, EventArgs e) { LeftHeader.Width = LeftListView.Width - 30; }
        private void LeftOnlyListView_Resize(object? sender, EventArgs e) { LeftOnlyHeader.Width = LeftOnlyListView.Width - 30; }

        private void LeftOnlyListView_ItemSelectionChanged(object? sender, ListViewItemSelectionChangedEventArgs e) { }// if (e.IsSelected) e.Item.Selected = false; }
        private void LeftListView_ItemSelectionChanged(object? sender, ListViewItemSelectionChangedEventArgs e) {
            RightListView.SelectedIndices.Clear();
            RightListView.SelectedIndices.Add(e.ItemIndex);
        }
        private void RightListView_ItemSelectionChanged(object? sender, ListViewItemSelectionChangedEventArgs e) {
            if (LeftListView.SelectedIndices.Count == 0 || LeftListView.SelectedIndices[0] != e.ItemIndex) {
                LeftListView.SelectedIndices.Clear();
                LeftListView.SelectedIndices.Add(e.ItemIndex);
            }
        }
        private void RightOnlyListView_ItemSelectionChanged(object? sender, ListViewItemSelectionChangedEventArgs e) { }//if (e.IsSelected) e.Item.Selected = false; }

        private void ListView_StartHover(object? sender, MouseEventArgs e) {
            if (sender is ListView lv && lv.GetItemAt(e.Location.X, e.Location.Y) is ListViewItem lLVI) {
                var location = new Point(e.X + 15, e.Y);
                ListViewItem? rLVI = null;
                bool compareTypeTT = (sender == LeftListView || sender == RightListView);
                if (compareTypeTT) {
                    lLVI = LeftListView.Items[lLVI.Index];
                    rLVI = RightListView.Items[lLVI.Index];
                }

                if (lLVI.Tag is IRecipe recipe) {
                    RecipeToolTip.SetRecipe(recipe, compareTypeTT ? (rLVI?.Tag as IRecipe) : null);
                    RecipeToolTip.Show(lv, location);
                } else if (lLVI.Tag is IAssembler assembler) //assembler, miner, or power
                  {
                    string left = assembler.FriendlyName + "\n" +
                        string.Format(DisplayCulture.Format, "   Speed:         {0}x\n", assembler.Owner.DefaultQuality is IQuality q ? assembler.GetSpeed(q) : -12345) +  //QUALITY UPDATE REQUIRED
                        string.Format(DisplayCulture.Format, "   Module Slots:  {0}", assembler.ModuleSlots);
                    string right = "";
                    if (compareTypeTT) {
                        var rassembler = rLVI?.Tag as IAssembler;
                        right = rassembler?.FriendlyName + "\n" +
                        string.Format(DisplayCulture.Format, "   Speed:         {0}x\n", assembler.Owner.DefaultQuality is IQuality q2 ? rassembler?.GetSpeed(q2) : -12345) +  //QUALITY UPDATE REQUIRED
                        string.Format(DisplayCulture.Format, "   Module Slots:  {0}", rassembler?.ModuleSlots);
                    }

                    TextToolTip.SetText(left, right);
                    TextToolTip.Show(lv, location);
                } else if (lLVI.Tag is IBeacon beacon) {
                    string left = beacon.FriendlyName + "\n" +
                        string.Format(DisplayCulture.Format, "   Module Slots:  {0}", beacon.ModuleSlots);
                    string right = "";
                    if (compareTypeTT) {
                        var rbeacon = rLVI?.Tag as IBeacon;
                        right = rbeacon?.FriendlyName + "\n" +
                            string.Format(DisplayCulture.Format, "   Module Slots:  {0}", rbeacon?.ModuleSlots);
                    }

                    TextToolTip.SetText(left, right);
                    TextToolTip.Show((Control)sender, location);
                } else if (lLVI.Tag is IModule module) {
                    string left = module.FriendlyName + "\n" +
                        string.Format(DisplayCulture.Format, "   Productivity bonus: {0}\n", module.GetProductivityBonus().ToString("%0", DisplayCulture.Format)) +
                        string.Format(DisplayCulture.Format, "   Speed bonus:        {0}\n", module.GetSpeedBonus().ToString("%0", DisplayCulture.Format)) +
                        string.Format(DisplayCulture.Format, "   Efficiency bonus:   {0}\n", (-module.GetConsumptionBonus()).ToString("%0", DisplayCulture.Format)) +
                        string.Format(DisplayCulture.Format, "   Pollution bonus:    {0}", module.GetPolutionBonus().ToString("%0", DisplayCulture.Format)) +
                        string.Format(DisplayCulture.Format, "   Quality bonus:      {0}", module.GetQualityBonus().ToString("%0", DisplayCulture.Format));
                    string right = "";
                    if (compareTypeTT) {
                        var rmodule = rLVI?.Tag as IModule;
                        right = rmodule?.FriendlyName + "\n" +
                        string.Format(DisplayCulture.Format, "   Productivity bonus: {0}\n", rmodule?.GetProductivityBonus().ToString("%0", DisplayCulture.Format)) +
                        string.Format(DisplayCulture.Format, "   Speed bonus:        {0}\n", rmodule?.GetSpeedBonus().ToString("%0", DisplayCulture.Format)) +
                        string.Format(DisplayCulture.Format, "   Efficiency bonus:   {0}\n", (-rmodule?.GetConsumptionBonus() ?? 0).ToString("%0", DisplayCulture.Format)) +
                        string.Format(DisplayCulture.Format, "   Pollution bonus:    {0}", rmodule?.GetPolutionBonus().ToString("%0", DisplayCulture.Format)) +
                        string.Format(DisplayCulture.Format, "   Quality bonus:      {0}", rmodule?.GetQualityBonus().ToString("%0", DisplayCulture.Format));
                    }
                    TextToolTip.SetText(left, right);
                    TextToolTip.Show(lv, location);
                }
            }

        }

        private void ListView_EndHover(object? sender, EventArgs e) {
            if (sender is not Control ctrl)
                return;
            RecipeToolTip.Hide(ctrl);
            TextToolTip.Hide(ctrl);
        }
    }
}
