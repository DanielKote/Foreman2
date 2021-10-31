using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
    public partial class PresetComparatorForm : Form
    {
        private bool Comparing; //true means we loaded the presets and are displaying the comparison (preset switching disabled), false means we are selecting presets
        private DataCache LeftCache;
        private DataCache RightCache;

        //all of these are of array size 4 (representing the 4 lists) : Left Only (from LeftCache), Left (from LeftCache), Right(from RightCache), Right Only (from RightCache)
        //Left and Right ([1] and [2]) have the exact same length.
        //the base lists are populated during initial cache loading and comparison and include the full lists.
        //the unfiltered selected tab list is set to equal one of the base lists based on which tab is selected.
        //the filtered selected tab list is further updated from the unfiltered tab list based on the filter string (and is the one used to populate the 4 item-lists)
        private List<object>[] unfilteredSelectedTabObjects;
        private List<ListViewItem>[] unfilteredSelectedTabLVIs;
        private List<ListViewItem>[] filteredSelectedTabLVIs;

        private List<object>[] unfilteredModTabObjects; //strings
        private List<object>[] unfilteredItemTabObjects; //Items
        private List<object>[] unfilteredRecipeTabObjects; //Recipes
        private List<object>[] unfilteredAssemblerTabObjects; //Assemblers
        private List<object>[] unfilteredMinerTabObjects; //Miners
        private List<object>[] unfilteredModuleTabObjects; //Modules
        private List<object>[][] tabSet; //just a helper array to set unfilteredSelectedTabObjects to the correct value without having to if/switch

        public PresetComparatorForm()
        {
            Comparing = false;

            InitializeComponent();
            RightOnlyHeader.Width = RightOnlyListView.Width - 30;
            RightHeader.Width = RightListView.Width - 30;
            LeftHeader.Width = LeftListView.Width - 30;
            LeftOnlyHeader.Width = LeftOnlyListView.Width - 30;
            LoadPresetOptions();

            unfilteredModTabObjects = new List<object>[] { new List<object>(), new List<object>(), new List<object>(), new List<object>() };
            unfilteredItemTabObjects = new List<object>[] { new List<object>(), new List<object>(), new List<object>(), new List<object>() };
            unfilteredRecipeTabObjects = new List<object>[] { new List<object>(), new List<object>(), new List<object>(), new List<object>() };
            unfilteredAssemblerTabObjects = new List<object>[] { new List<object>(), new List<object>(), new List<object>(), new List<object>() };
            unfilteredMinerTabObjects = new List<object>[] { new List<object>(), new List<object>(), new List<object>(), new List<object>() };
            unfilteredModuleTabObjects = new List<object>[] { new List<object>(), new List<object>(), new List<object>(), new List<object>() };
            tabSet = new List<object>[][] { unfilteredModTabObjects, unfilteredItemTabObjects, unfilteredRecipeTabObjects, unfilteredAssemblerTabObjects, unfilteredMinerTabObjects, unfilteredModuleTabObjects };

            unfilteredSelectedTabObjects = tabSet[0];

            unfilteredSelectedTabLVIs = new List<ListViewItem>[] { new List<ListViewItem>(), new List<ListViewItem>(), new List<ListViewItem>(), new List<ListViewItem>() };
            filteredSelectedTabLVIs = new List<ListViewItem>[] { new List<ListViewItem>(), new List<ListViewItem>(), new List<ListViewItem>(), new List<ListViewItem>() };

        }

        private void LoadPresetOptions()
        {
            List<string> existingPresetFiles = new List<string>();
            foreach (string presetFile in Directory.GetFiles(Path.Combine(Application.StartupPath, "Presets"), "*.json"))
                if (File.Exists(Path.ChangeExtension(presetFile, "dat")))
                    existingPresetFiles.Add(Path.GetFileNameWithoutExtension(presetFile));
            existingPresetFiles.Sort();
            List<Preset> Presets = new List<Preset>();
            foreach (string presetFile in existingPresetFiles)
                Presets.Add(new Preset(presetFile, false, false)); //we dont care about default or selected states here.

            if (existingPresetFiles.Count < 2)
                this.Close();

            LeftPresetSelectionBox.Items.AddRange(Presets.ToArray());
            RightPresetSelectionBox.Items.AddRange(Presets.ToArray());
            LeftPresetSelectionBox.SelectedIndex = 0;
            RightPresetSelectionBox.SelectedIndex = 1;
        }

        private void ClearAllLists()
        {
            LeftOnlyListView.VirtualListSize = 0;
            LeftListView.VirtualListSize = 0;
            RightListView.VirtualListSize = 0;
            RightOnlyListView.VirtualListSize = 0;

            for (int i = 0; i < 4; i++)
            {
                unfilteredModTabObjects[i].Clear();
                unfilteredItemTabObjects[i].Clear();
                unfilteredRecipeTabObjects[i].Clear();
                unfilteredAssemblerTabObjects[i].Clear();
                unfilteredMinerTabObjects[i].Clear();
                unfilteredModuleTabObjects[i].Clear();

                filteredSelectedTabLVIs[i].Clear();
                unfilteredSelectedTabLVIs[i].Clear();
            }
        }

        private void ComparePresets()
        {
            //helpful inner function to process items, recipes, assemblers, miners, and modules (so... everything but mods)
            void ProcessObject<T>(IReadOnlyDictionary<string, T> leftCacheDictionary, IReadOnlyDictionary<string, T> rightCacheDictionary, List<object>[] outputLists) where T : DataObjectBase
            {
                foreach (var kvp in leftCacheDictionary)
                {
                    if (rightCacheDictionary.ContainsKey(kvp.Key))
                        outputLists[1].Add(kvp.Value);
                    else
                        outputLists[0].Add(kvp.Value);
                }
                foreach (var kvp in rightCacheDictionary)
                {
                    if (leftCacheDictionary.ContainsKey(kvp.Key))
                        outputLists[2].Add(kvp.Value);
                    else
                        outputLists[3].Add(kvp.Value);
                }
                for (int i = 0; i < 4; i++) outputLists[i].Sort(delegate (object a, object b) { return ((DataObjectBase)a).Name.CompareTo(((DataObjectBase)b).Name); });
            }

            //step 1: load in left and right caches
            using (DataLoadForm form = new DataLoadForm(LeftPresetSelectionBox.SelectedItem as Preset))
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Left = this.Left + 150;
                form.Top = this.Top + 100;
                form.ShowDialog(); //LOAD FACTORIO DATA for left preset
                LeftCache = form.GetDataCache();
            }
            using (DataLoadForm form = new DataLoadForm(RightPresetSelectionBox.SelectedItem as Preset))
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Left = this.Left + 150;
                form.Top = this.Top + 100;
                form.ShowDialog(); //LOAD FACTORIO DATA for left preset
                RightCache = form.GetDataCache();
            }

            //step 2: fill in the unfiltered tab lists

            //2.1: mods
            foreach (var kvp in LeftCache.IncludedMods)
            {
                if (RightCache.IncludedMods.ContainsKey(kvp.Key))
                    unfilteredModTabObjects[1].Add(kvp.Key + "_" + kvp.Value);
                else
                    unfilteredModTabObjects[0].Add(kvp.Key + "_" + kvp.Value);
            }
            foreach (var kvp in RightCache.IncludedMods)
            {
                if (LeftCache.IncludedMods.ContainsKey(kvp.Key))
                    unfilteredModTabObjects[2].Add(kvp.Key + "_" + kvp.Value);
                else
                    unfilteredModTabObjects[3].Add(kvp.Key + "_" + kvp.Value);
            }
            for (int i = 0; i < 4; i++) unfilteredModTabObjects[i].Sort(delegate (object a, object b) { return ((string)a).CompareTo((string)b); });

            //2.2: items, recipes, assemblers, miners, and modules
            ProcessObject(LeftCache.Items, RightCache.Items, unfilteredItemTabObjects);
            ProcessObject(LeftCache.Recipes, RightCache.Recipes, unfilteredRecipeTabObjects);
            ProcessObject(LeftCache.Assemblers, RightCache.Assemblers, unfilteredAssemblerTabObjects);
            ProcessObject(LeftCache.Miners, RightCache.Miners, unfilteredMinerTabObjects);
            ProcessObject(LeftCache.Modules, RightCache.Modules, unfilteredModuleTabObjects);

            //process the tab (for the first time) - it will also populate the actual lists.
            UpdateUnfilteredLVIs();
            UpdateFilteredLists();
        }

        private void UpdateUnfilteredLVIs()
        {
            unfilteredSelectedTabObjects = tabSet[ComparisonTabControl.SelectedIndex];
            IconList.Images.Clear();
            IconList.ImageSize = (ComparisonTabControl.SelectedIndex == 0 ? new Size(1, 1) : new Size(32, 32)); //0: mod list (no images)
            if (DataCache.UnknownIcon != null)
                IconList.Images.Add(DataCache.UnknownIcon);

            for (int i = 0; i < 4; i++)
            {
                unfilteredSelectedTabLVIs[i].Clear();
                if (ComparisonTabControl.SelectedIndex == 0) //mod -> string type
                {
                    foreach (object obj in unfilteredSelectedTabObjects[i])
                    {
                        ListViewItem lvItem = new ListViewItem();
                        lvItem.Text = (string)obj;
                        lvItem.Tag = lvItem.Text;
                        lvItem.Name = lvItem.Text;
                        unfilteredSelectedTabLVIs[i].Add(lvItem);
                    }
                }
                else //item,recipe,assembler,miner,module -> all are DataObjectBase types
                {
                    foreach (object obj in unfilteredSelectedTabObjects[i])
                    {
                        ListViewItem lvItem = new ListViewItem();
                        DataObjectBase doBase = (DataObjectBase)obj;

                        if (doBase.Icon != null)
                        {
                            IconList.Images.Add(doBase.Icon);
                            lvItem.ImageIndex = IconList.Images.Count - 1;
                        }
                        else
                            lvItem.ImageIndex = 0;

                        lvItem.Text = doBase.FriendlyName;
                        lvItem.Tag = doBase;
                        lvItem.Name = doBase.Name.ToLower(); //we will use this to filter by (cant filter by friendly name as that can cause the middle 2 to desync)
                        if (doBase is Recipe recipe)
                        {
                            //add in the tooltip here for recipes
                        }
                        unfilteredSelectedTabLVIs[i].Add(lvItem);
                    }
                }
            }
        }

        private void UpdateFilteredLists()
        {
            string filter = FilterTextBox.Text.ToLower();

            for (int i = 0; i < 4; i++)
            {
                filteredSelectedTabLVIs[i].Clear();

                foreach (ListViewItem lvItem in unfilteredSelectedTabLVIs[i])
                    if (lvItem.Name.Contains(filter))
                        filteredSelectedTabLVIs[i].Add(lvItem);
            }

            LeftOnlyListView.VirtualListSize = filteredSelectedTabLVIs[0].Count;
            LeftListView.VirtualListSize = filteredSelectedTabLVIs[1].Count;
            RightListView.VirtualListSize = filteredSelectedTabLVIs[2].Count;
            RightOnlyListView.VirtualListSize = filteredSelectedTabLVIs[3].Count;
            LeftOnlyListView.Invalidate();
            LeftListView.Invalidate();
            RightListView.Invalidate();
            RightOnlyListView.Invalidate();
        }

        private void ProcessPresetsButton_Click(object sender, EventArgs e)
        {
            Comparing = !Comparing;
            if(Comparing)
            {
                ComparePresets();
            }
            else
            {
                ClearAllLists();
                LeftCache.Clear();
                LeftCache = null;
                RightCache.Clear();
                RightCache = null;

                GC.Collect(); //we just closed 2 DataCaches... this is pretty large.
            }
            PresetSelectionGroup.Enabled = !Comparing;
            ProcessPresetsButton.Text = Comparing ? "Select Other Presets" : "Read Presets And Compare";
        }

        private void PresetSelectionBox_SelectedValueChanged(object sender, EventArgs e) //either of the two
        {
            ProcessPresetsButton.Enabled = (LeftPresetSelectionBox.SelectedIndex != RightPresetSelectionBox.SelectedIndex);
            ProcessPresetsButton.Text = ProcessPresetsButton.Enabled ? "Read Presets And Compare" : "Cant Compare Preset To Itself";
        }

        private void PresetComparatorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(Comparing)
            {
                Comparing = false;
                ClearAllLists();

                LeftCache.Clear();
                LeftCache = null;
                RightCache.Clear();
                RightCache = null;

                GC.Collect();
            }
        }

        private void ComparisonTabControl_SelectedIndexChanged(object sender, EventArgs e) { UpdateUnfilteredLVIs(); UpdateFilteredLists(); }
        private void FilterTextBox_TextChanged(object sender, EventArgs e) { UpdateFilteredLists(); }

        private void LeftOnlyListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredSelectedTabLVIs[0][e.ItemIndex]; }
        private void LeftListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredSelectedTabLVIs[1][e.ItemIndex]; }
        private void RightListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredSelectedTabLVIs[2][e.ItemIndex]; }
        private void RightOnlyListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredSelectedTabLVIs[3][e.ItemIndex]; }

        private void RightOnlyListView_Resize(object sender, EventArgs e) { RightOnlyHeader.Width = RightOnlyListView.Width - 30; }
        private void RightListView_Resize(object sender, EventArgs e) { RightHeader.Width = RightListView.Width - 30; }
        private void LeftListView_Resize(object sender, EventArgs e) { LeftHeader.Width = LeftListView.Width - 30; }
        private void LeftOnlyListView_Resize(object sender, EventArgs e) { LeftOnlyHeader.Width = LeftOnlyListView.Width - 30; }
    }
}
