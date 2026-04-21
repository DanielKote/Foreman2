using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Foreman {
    public partial class GraphSummaryForm : Form {
        protected class ItemCounter(double i, double iu, double o, double ou, double oo, double p, double c) {
            public double Input { get; set; } = i;
            public double InputUnlinked { get; set; } = iu;
            public double Output { get; set; } = o;
            public double OutputUnlinked { get; set; } = ou;
            public double OutputOverflow { get; set; } = oo;
            public double Production { get; set; } = p;
            public double Consumption { get; set; } = c;
        }


        private List<ListViewItem> _unfilteredAssemblerList;
        private List<ListViewItem> _unfilteredMinerList;
        private List<ListViewItem> _unfilteredPowerList;
        private List<ListViewItem> _unfilteredBeaconList;

        private List<ListViewItem> _unfilteredItemsList;
        private List<ListViewItem> _unfilteredFluidsList;

        private List<ListViewItem> _unfilteredKeyNodesList;

        private List<ListViewItem> _filteredAssemblerList;
        private List<ListViewItem> _filteredMinerList;
        private List<ListViewItem> _filteredPowerList;
        private List<ListViewItem> _filteredBeaconList;

        private List<ListViewItem> _filteredItemsList;
        private List<ListViewItem> _filteredFluidsList;

        private List<ListViewItem> _filteredKeyNodesList;

        // int is +ve if sorted down, -ve if sorted up, |value| is the column # (starts from 1 due to 0 not having a sign) of the sort.
        private Dictionary<ListView, int> _lastSortOrder;

        private readonly string _rateString;

        private static readonly Color AvailableObjectColor = Color.White;
        private static readonly Color UnavailableObjectColor = Color.Pink;

        public GraphSummaryForm(IEnumerable<ReadOnlyBaseNode> nodes, IEnumerable<ReadOnlyNodeLink> links, string rateString) {
            InitializeComponent();
            MainForm.SetDoubleBuffered(AssemblerListView);
            MainForm.SetDoubleBuffered(MinerListView);
            MainForm.SetDoubleBuffered(PowerListView);
            MainForm.SetDoubleBuffered(BeaconListView);
            MainForm.SetDoubleBuffered(ItemsListView);
            MainForm.SetDoubleBuffered(FluidsListView);
            MainForm.SetDoubleBuffered(KeyNodesListView);

            _unfilteredAssemblerList = [];
            _unfilteredMinerList = [];
            _unfilteredPowerList = [];
            _unfilteredBeaconList = [];
            _unfilteredItemsList = [];
            _unfilteredFluidsList = [];
            _unfilteredKeyNodesList = [];

            _filteredAssemblerList = [];
            _filteredMinerList = [];
            _filteredPowerList = [];
            _filteredBeaconList = [];
            _filteredItemsList = [];
            _filteredFluidsList = [];
            _filteredKeyNodesList = [];

            _lastSortOrder = new Dictionary<ListView, int> {
                { AssemblerListView, 2 },
                { MinerListView, 2 },
                { PowerListView, 2 },
                { BeaconListView, 2 },
                { ItemsListView, 1 },
                { FluidsListView, 1 },
                { KeyNodesListView, 1 }
            };

            IconList.Images.Clear();
            IconList.Images.Add(DataCache.UnknownIcon);

            ItemsTabPage.Text += $" ( per {rateString})";
            _rateString = rateString;

            // lists

            LoadUnfilteredSelectedAssemblerList(
                nodes.Where(n => n is ReadOnlyRecipeNode rNode && rNode.SelectedAssembler.Assembler.EntityType == EntityType.Assembler)
                    .Select(n => (ReadOnlyRecipeNode) n), _unfilteredAssemblerList);
            LoadUnfilteredSelectedAssemblerList(
                nodes.Where(n => n is ReadOnlyRecipeNode rNode && rNode.SelectedAssembler.Assembler.EntityType is EntityType.Miner or EntityType.OffshorePump)
                    .Select(n => (ReadOnlyRecipeNode) n), _unfilteredMinerList);
            LoadUnfilteredSelectedAssemblerList(
                nodes.Where(n =>
                        n is ReadOnlyRecipeNode rNode &&
                        rNode.SelectedAssembler.Assembler.EntityType is EntityType.Boiler or EntityType.BurnerGenerator or EntityType.Generator
                            or EntityType.Reactor)
                    .Select(n => (ReadOnlyRecipeNode) n), _unfilteredPowerList);

            LoadUnfilteredBeaconList(nodes.Where(n => n is ReadOnlyRecipeNode rNode && rNode.SelectedBeacon).Select(n => (ReadOnlyRecipeNode) n),
                _unfilteredBeaconList);

            LoadUnfilteredItemLists(nodes, links, false, _unfilteredItemsList);
            LoadUnfilteredItemLists(nodes, links, true, _unfilteredFluidsList);

            LoadUnfilteredKeyNodesList(nodes.Where(n => n.KeyNode), _unfilteredKeyNodesList);

            // building totals

            var buildingTotal = nodes.Where(n => n is ReadOnlyRecipeNode).Sum(n => Math.Ceiling(((ReadOnlyRecipeNode) n).ActualSetValue));
            double beaconTotal = nodes.Where(n => n is ReadOnlyRecipeNode).Sum(n => ((ReadOnlyRecipeNode) n).GetTotalBeacons());
            BuildingCountLabel.Text += GraphicsStuff.DoubleToString(buildingTotal);
            BeaconCountLabel.Text += GraphicsStuff.DoubleToString(beaconTotal);

            // power totals

            var powerConsumption = nodes.Where(n => n is ReadOnlyRecipeNode).Sum(n =>
                ((ReadOnlyRecipeNode) n).GetTotalAssemblerElectricalConsumption() + ((ReadOnlyRecipeNode) n).GetTotalBeaconElectricalConsumption());
            var powerProduction = nodes.Where(n => n is ReadOnlyRecipeNode).Sum(n => ((ReadOnlyRecipeNode) n).GetTotalGeneratorElectricalProduction());
            PowerConsumptionLabel.Text += GraphicsStuff.DoubleToEnergy(powerConsumption, "W");
            PowerProductionLabel.Text += GraphicsStuff.DoubleToEnergy(powerProduction, "W");

            // update filtered

            UpdateFilteredBuildingLists();
            UpdateFilteredItemsLists();
            UpdateFilteredKeyNodesList();
        }

        //-------------------------------------------------------------------------------------------------------Initial list initialization

        private void LoadUnfilteredSelectedAssemblerList(IEnumerable<ReadOnlyRecipeNode> origin, List<ListViewItem> lviList) {
            var buildingCounters = new Dictionary<AssemblerQualityPair, int>();
            // power for buildings, power for beacons
            var buildingElectricalPower = new Dictionary<AssemblerQualityPair, Tuple<double, double>>();

            foreach (var recipeNode in origin) {
                if (!buildingCounters.ContainsKey(recipeNode.SelectedAssembler)) {
                    buildingCounters.Add(recipeNode.SelectedAssembler, 0);
                    buildingElectricalPower.Add(recipeNode.SelectedAssembler, new Tuple<double, double>(0, 0));
                }

                // should probably check the validity of ceiling in case of near correct
                // (ex: 1.0001 assemblers should really be counted as 1 instead of 2)
                buildingCounters[recipeNode.SelectedAssembler] += (int) Math.Ceiling(recipeNode.ActualSetValue);
                var oldValues = buildingElectricalPower[recipeNode.SelectedAssembler];
                buildingElectricalPower[recipeNode.SelectedAssembler] = new Tuple<double, double>(
                    oldValues.Item1 + recipeNode.GetTotalGeneratorElectricalProduction() + recipeNode.GetTotalAssemblerElectricalConsumption(),
                    oldValues.Item2 + recipeNode.GetTotalBeaconElectricalConsumption());
            }

            foreach (var assembler in buildingCounters.Keys.OrderByDescending(a => a.Assembler.Available)
                .ThenBy(a => a.Assembler.FriendlyName)
                .ThenBy(a => a.Quality.Level)
                .ThenBy(a => a.Quality.FriendlyName)
            ) {
                var lvItem = new ListViewItem();
                if (assembler.Assembler.Icon != null) {
                    IconList.Images.Add(assembler.Icon);
                    lvItem.ImageIndex = IconList.Images.Count - 1;
                } else {
                    lvItem.ImageIndex = 0;
                }

                lvItem.Text = buildingCounters[assembler] >= 10000000
                    ? buildingCounters[assembler].ToString("0.##e0")
                    : buildingCounters[assembler].ToString("N0");
                lvItem.Tag = assembler;
                // key
                lvItem.Name = assembler.Assembler.Name + ":" + assembler.Quality.Name;
                lvItem.BackColor = assembler.Assembler.Available ? AvailableObjectColor : UnavailableObjectColor;
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = assembler.FriendlyName });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() {
                    Text = buildingElectricalPower[assembler].Item1 == 0 ? "-" : GraphicsStuff.DoubleToEnergy(buildingElectricalPower[assembler].Item1, "W"),
                    Tag = buildingElectricalPower[assembler].Item1
                });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() {
                    Text = buildingElectricalPower[assembler].Item2 == 0 ? "-" : GraphicsStuff.DoubleToEnergy(buildingElectricalPower[assembler].Item2, "W"),
                    Tag = buildingElectricalPower[assembler].Item2
                });
                lviList.Add(lvItem);
            }
        }

        private void LoadUnfilteredBeaconList(IEnumerable<ReadOnlyRecipeNode> origin, List<ListViewItem> lviList) {
            var beaconCounters = new Dictionary<BeaconQualityPair, int>();

            foreach (var recipeNode in origin) {
                if (!recipeNode.SelectedBeacon)
                    continue;

                if (!beaconCounters.ContainsKey(recipeNode.SelectedBeacon))
                    beaconCounters.Add(recipeNode.SelectedBeacon, 0);
                beaconCounters[recipeNode.SelectedBeacon] += recipeNode.GetTotalBeacons();
            }

            foreach (var beacon in beaconCounters.Keys.OrderByDescending(b => b.Beacon.Available)
                .ThenBy(b => b.Beacon.FriendlyName)
                .ThenBy(b => b.Quality.Level)
                .ThenBy(b => b.Quality.FriendlyName)
            ) {
                var lvItem = new ListViewItem();
                if (beacon.Icon != null) {
                    IconList.Images.Add(beacon.Icon);
                    lvItem.ImageIndex = IconList.Images.Count - 1;
                } else {
                    lvItem.ImageIndex = 0;
                }

                lvItem.Text = beaconCounters[beacon].ToString();
                lvItem.Tag = beacon;
                // key
                lvItem.Name = beacon.Beacon.Name + ":" + beacon.Quality.Name;
                lvItem.BackColor = beacon.Beacon.Available ? AvailableObjectColor : UnavailableObjectColor;
                lvItem.SubItems.Add(beacon.FriendlyName);
                // TODO: QUALITY UPDATE REQUIRED
                var beaconPowerConsumption =
                    beaconCounters[beacon] * (beacon.Beacon.GetEnergyConsumption(beacon.Quality) + beacon.Beacon.GetEnergyDrain());
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem()
                    { Text = beaconCounters[beacon] == 0 ? "-" : GraphicsStuff.DoubleToEnergy(beaconPowerConsumption, "W"), Tag = beaconPowerConsumption });
                lviList.Add(lvItem);
            }
        }

        private void LoadUnfilteredItemLists(IEnumerable<ReadOnlyBaseNode> nodes, IEnumerable<ReadOnlyNodeLink> links, bool fluids,
            List<ListViewItem> lviList) {
            // NOTE: throughput is initially calculated as all non-overflow linked input & output of each recipe node. At the end we will add
            var itemCounters = new Dictionary<ItemQualityPair, ItemCounter>();

            foreach (var node in nodes) {
                switch (node) {
                    case ReadOnlyRecipeNode: {
                        foreach (var input in node.Inputs.Where(i => fluids.Equals(i.Item is Fluid))) {
                            if (!itemCounters.ContainsKey(input))
                                itemCounters.Add(input, new ItemCounter(0, 0, 0, 0, 0, 0, 0));

                            var consumeRate = node.GetConsumeRate(input);
                            if (!(consumeRate > 0))
                                continue;

                            if (node.InputLinks.All(l => l.Item != input))
                                itemCounters[input].InputUnlinked += consumeRate;
                            else
                                itemCounters[input].Consumption += consumeRate;
                        }

                        foreach (var output in node.Outputs.Where(i => fluids.Equals(i.Item is Fluid))) {
                            if (!itemCounters.ContainsKey(output))
                                itemCounters.Add(output, new ItemCounter(0, 0, 0, 0, 0, 0, 0));

                            var supplyRate = node.GetSupplyRate(output);
                            var isOverProduced = node.IsOverproducing(output);
                            var supplyUsedRate = isOverProduced ? node.GetSupplyUsedRate(output) : supplyRate;

                            if (!(supplyRate > 0))
                                continue;

                            if (node.OutputLinks.All(l => l.Item != output))
                                itemCounters[output].OutputUnlinked += supplyRate;

                            itemCounters[output].Production += supplyRate;
                            if (isOverProduced)
                                itemCounters[output].OutputOverflow += supplyRate - supplyUsedRate;
                        }

                        break;
                    }

                    case ReadOnlySupplierNode sNode when fluids.Equals(sNode.SuppliedItem.Item is Fluid): {
                        if (!itemCounters.ContainsKey(sNode.SuppliedItem))
                            itemCounters.Add(sNode.SuppliedItem, new ItemCounter(0, 0, 0, 0, 0, 0, 0));
                        itemCounters[sNode.SuppliedItem].Input += sNode.ActualRate;
                        break;
                    }

                    case ReadOnlyConsumerNode cNode when fluids.Equals(cNode.ConsumedItem.Item is Fluid): {
                        if (!itemCounters.ContainsKey(cNode.ConsumedItem))
                            itemCounters.Add(cNode.ConsumedItem, new ItemCounter(0, 0, 0, 0, 0, 0, 0));
                        itemCounters[cNode.ConsumedItem].Output += cNode.ActualRate;
                        break;
                    }
                }
            }

            foreach (var item in itemCounters.Keys.OrderBy(a => a.Item.FriendlyName)
                .ThenBy(a => a.Quality.Level)
                .ThenBy(a => a.Quality.FriendlyName)
            ) {
                var lvItem = new ListViewItem();
                if (item.Icon != null) {
                    IconList.Images.Add(item.Icon);
                    lvItem.ImageIndex = IconList.Images.Count - 1;
                } else {
                    lvItem.ImageIndex = 0;
                }

                lvItem.Text = item.FriendlyName;
                lvItem.Tag = item;
                // key
                lvItem.Name = item.Item.Name + ":" + item.Quality.Name;
                lvItem.BackColor = item.Item.Available ? AvailableObjectColor : UnavailableObjectColor;
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem()
                    { Text = itemCounters[item].Input == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Input), Tag = itemCounters[item].Input });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() {
                    Text = itemCounters[item].InputUnlinked == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].InputUnlinked),
                    Tag = itemCounters[item].InputUnlinked
                });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem()
                    { Text = itemCounters[item].Output == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Output), Tag = itemCounters[item].Output });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() {
                    Text = itemCounters[item].OutputUnlinked == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].OutputUnlinked),
                    Tag = itemCounters[item].OutputUnlinked
                });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() {
                    Text = itemCounters[item].OutputOverflow == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].OutputOverflow),
                    Tag = itemCounters[item].OutputOverflow
                });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() {
                    Text = itemCounters[item].Production == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Production),
                    Tag = itemCounters[item].Production
                });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() {
                    Text = itemCounters[item].Consumption == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Consumption),
                    Tag = itemCounters[item].Consumption
                });
                lviList.Add(lvItem);
            }
        }

        private void LoadUnfilteredKeyNodesList(IEnumerable<ReadOnlyBaseNode> origin, List<ListViewItem> lviList) {
            foreach (var node in origin) {
                var lvItem = new ListViewItem();

                Bitmap icon;
                string nodeText;
                string nodeType;
                switch (node) {
                    case ReadOnlyConsumerNode cNode:
                        icon = cNode.ConsumedItem.Icon;
                        nodeText = cNode.ConsumedItem.FriendlyName;
                        nodeType = "Consumer";
                        break;
                    case ReadOnlySupplierNode sNode:
                        icon = sNode.SuppliedItem.Icon;
                        nodeText = sNode.SuppliedItem.FriendlyName;
                        nodeType = "Supplier";
                        break;
                    case ReadOnlyPassthroughNode pNode:
                        icon = pNode.PassthroughItem.Icon;
                        nodeText = pNode.PassthroughItem.FriendlyName;
                        nodeType = "Passthrough";
                        break;
                    case ReadOnlyRecipeNode rNode:
                        icon = rNode.BaseRecipe.Icon;
                        nodeText = rNode.BaseRecipe.FriendlyName;
                        nodeType = "Recipe";
                        break;
                    case ReadOnlySpoilNode spNode:
                        icon = spNode.InputItem.Icon;
                        nodeText = spNode.InputItem.FriendlyName + " spoiling";
                        nodeType = "Spoil";
                        break;
                    case ReadOnlyPlantNode plNode:
                        icon = plNode.Seed.Icon;
                        nodeText = plNode.Seed.FriendlyName + " planting";
                        nodeType = "Plant";
                        break;
                    default:
                        continue;
                }

                if (icon != null) {
                    IconList.Images.Add(icon);
                    lvItem.ImageIndex = IconList.Images.Count - 1;
                } else {
                    lvItem.ImageIndex = 0;
                }

                lvItem.Text = nodeType;
                lvItem.Tag = node;
                // key
                lvItem.Name = nodeText;
                lvItem.BackColor = AvailableObjectColor;
                lvItem.SubItems.Add(nodeText);
                lvItem.SubItems.Add(node.KeyNodeTitle);

                if (node is ReadOnlyRecipeNode rrNode) {
                    lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = "-", Tag = (double) 0 });
                    lvItem.SubItems.Add(new ListViewItem.ListViewSubItem()
                        { Text = GraphicsStuff.DoubleToString(rrNode.ActualSetValue), Tag = rrNode.ActualSetValue });
                } else {
                    lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = GraphicsStuff.DoubleToString(node.ActualRate), Tag = node.ActualRate });
                    lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = "-", Tag = (double) 0 });
                }

                lviList.Add(lvItem);
            }
        }

        //-------------------------------------------------------------------------------------------------------Filter functions

        private void UpdateFilteredBuildingLists() {
            UpdateFilteredBuildingList(_unfilteredAssemblerList, _filteredAssemblerList, AssemblerListView);
            UpdateFilteredBuildingList(_unfilteredMinerList, _filteredMinerList, MinerListView);
            UpdateFilteredBuildingList(_unfilteredPowerList, _filteredPowerList, PowerListView);
            UpdateFilteredBuildingList(_unfilteredBeaconList, _filteredBeaconList, BeaconListView);
        }

        private void UpdateFilteredBuildingList(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner) {
            var filterString = BuildingsFilterTextBox.Text.ToLower();

            filteredList.Clear();

            foreach (var lvItem in unfilteredList) {
                if (string.IsNullOrEmpty(filterString) || ((DataObjectBase) lvItem.Tag).LFriendlyName.Contains(filterString))
                    filteredList.Add(lvItem);
            }

            owner.VirtualListSize = filteredList.Count;
            owner.Invalidate();
        }

        private void UpdateFilteredItemsLists() {
            UpdateFilteredItemsList(_unfilteredItemsList, _filteredItemsList, ItemsListView);
            UpdateFilteredItemsList(_unfilteredFluidsList, _filteredFluidsList, FluidsListView);
        }

        private void UpdateFilteredItemsList(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner) {
            var filterString = ItemsFilterTextBox.Text.ToLower();
            var includeInputs = ItemFilterInputCheckBox.Checked;
            var includeInputUnlinked = ItemFilterInputUnlinkedCheckBox.Checked;
            var includeOutputs = ItemFilterOutputCheckBox.Checked;
            var includeOutputsUnlinked = ItemFilterOutputUnlinkedCheckBox.Checked;
            var includeOutputsOverflow = ItemFilterOutputOverproducedCheckBox.Checked;
            var includeProduced = ItemFilterProductionCheckBox.Checked;
            var includeConsumed = ItemFilterConsumptionCheckBox.Checked;

            filteredList.Clear();

            foreach (var lvItem in unfilteredList) {
                if (!string.IsNullOrEmpty(filterString) && !((Item) lvItem.Tag).LFriendlyName.Contains(filterString))
                    continue;

                if ((includeInputs && lvItem.SubItems[1].Text != "-") ||
                    (includeInputUnlinked && lvItem.SubItems[2].Text != "-") ||
                    (includeOutputs && lvItem.SubItems[3].Text != "-") ||
                    (includeOutputsUnlinked && lvItem.SubItems[4].Text != "-") ||
                    (includeOutputsOverflow && lvItem.SubItems[5].Text != "-") ||
                    (includeProduced && lvItem.SubItems[6].Text != "-") ||
                    (includeConsumed && lvItem.SubItems[7].Text != "-")) {
                    filteredList.Add(lvItem);
                }
            }

            owner.VirtualListSize = filteredList.Count;
            owner.Invalidate();
        }

        private void UpdateFilteredKeyNodesList() {
            var filterString = KeyNodesFilterTextBox.Text.ToLower();
            var includeSuppliers = SupplierNodeFilterCheckBox.Checked;
            var includeConsumers = ConsumerNodeFilterCheckBox.Checked;
            var includePassthrough = PassthroughNodeFilterCheckBox.Checked;
            var includeRecipe = RecipeNodeFilterCheckBox.Checked;

            _filteredKeyNodesList.Clear();

            foreach (var lvItem in _unfilteredKeyNodesList) {
                if (!string.IsNullOrEmpty(filterString)
                    && !lvItem.Text.ToLower().Contains(filterString)
                    && !lvItem.SubItems[1].Text.ToLower().Contains(filterString)
                    && !lvItem.SubItems[2].Text.ToLower().Contains(filterString)) {
                    continue;
                }

                if ((includeSuppliers && lvItem.Tag is ReadOnlySupplierNode) ||
                    (includeConsumers && lvItem.Tag is ReadOnlyConsumerNode) ||
                    (includePassthrough && lvItem.Tag is ReadOnlyPassthroughNode) ||
                    (includeRecipe && lvItem.Tag is ReadOnlyRecipeNode)) {
                    _filteredKeyNodesList.Add(lvItem);
                }
            }

            KeyNodesListView.VirtualListSize = _filteredKeyNodesList.Count;
            KeyNodesListView.Invalidate();
        }

        //-------------------------------------------------------------------------------------------------------Virtual item retrieval for all list views

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

        private void ItemsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredItemsList[e.ItemIndex];
        }

        private void FluidsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredFluidsList[e.ItemIndex];
        }

        private void KeyNodesListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) {
            e.Item = _filteredKeyNodesList[e.ItemIndex];
        }

        //-------------------------------------------------------------------------------------------------------Filter changed events

        private void BuildingsFilterTextBox_TextChanged(object sender, EventArgs e) {
            UpdateFilteredBuildingLists();
        }

        private void ItemsFilterTextBox_TextChanged(object sender, EventArgs e) {
            UpdateFilteredItemsLists();
        }

        private void ItemFilterCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateFilteredItemsLists();
        }

        private void KeyNodesFilterTextBox_TextChanged(object sender, EventArgs e) {
            UpdateFilteredKeyNodesList();
        }

        private void KeyNodesFilterCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateFilteredKeyNodesList();
        }

        //-------------------------------------------------------------------------------------------------------Column clicked events

        private void AssemblerListView_ColumnClick(object sender, ColumnClickEventArgs e) {
            BuildingListView_ColumnSort(_unfilteredAssemblerList, _filteredAssemblerList, AssemblerListView, e.Column);
        }

        private void MinerListView_ColumnClick(object sender, ColumnClickEventArgs e) {
            BuildingListView_ColumnSort(_unfilteredMinerList, _filteredMinerList, MinerListView, e.Column);
        }

        private void PowerListView_ColumnClick(object sender, ColumnClickEventArgs e) {
            BuildingListView_ColumnSort(_unfilteredPowerList, _filteredPowerList, PowerListView, e.Column);
        }

        private void BeaconListView_ColumnClick(object sender, ColumnClickEventArgs e) {
            BuildingListView_ColumnSort(_unfilteredBeaconList, _filteredBeaconList, BeaconListView, e.Column);
        }

        private void BuildingListView_ColumnSort(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner, int column) {
            // last sort was this very column -> this is now a reverse sort
            var reverseSortLambda = _lastSortOrder[owner] == column + 1 ? -1 : 1;
            _lastSortOrder[owner] = reverseSortLambda * (column + 1);

            unfilteredList.Sort((a, b) => {
                int result;
                if (column == 0)
                    result = -double.Parse(a.Text).CompareTo(double.Parse(b.Text));
                else if (column == 1)
                    result = string.Compare(a.SubItems[1].Text.ToLower(), b.SubItems[1].Text.ToLower(), StringComparison.Ordinal);
                else
                    result = -((double) a.SubItems[column].Tag).CompareTo((double) b.SubItems[column].Tag);

                if (result == 0)
                    result = string.Compare(((DataObjectBase) a.Tag).LFriendlyName, ((DataObjectBase) b.Tag).LFriendlyName, StringComparison.Ordinal);
                if (result == 0)
                    result = string.Compare(((DataObjectBase) a.Tag).Name, ((DataObjectBase) b.Tag).Name, StringComparison.Ordinal);
                return result * reverseSortLambda;
            });

            UpdateFilteredBuildingList(unfilteredList, filteredList, owner);
            owner.Invalidate();
        }

        private void ItemsListView_ColumnClick(object sender, ColumnClickEventArgs e) {
            ItemListView_ColumnSort(_unfilteredItemsList, _filteredItemsList, ItemsListView, e.Column);
        }

        private void FluidsListView_ColumnClick(object sender, ColumnClickEventArgs e) {
            ItemListView_ColumnSort(_unfilteredFluidsList, _filteredFluidsList, FluidsListView, e.Column);
        }

        private void ItemListView_ColumnSort(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner, int column) {
            // last sort was this very column -> this is now a reverse sort
            var reverseSortLambda = _lastSortOrder[owner] == column + 1 ? -1 : 1;
            _lastSortOrder[owner] = reverseSortLambda * (column + 1);

            unfilteredList.Sort((a, b) => {
                int result;
                if (column == 0)
                    result = string.Compare(a.SubItems[0].Text.ToLower(), b.SubItems[0].Text.ToLower(), StringComparison.Ordinal);
                else
                    result = -((double) a.SubItems[column].Tag).CompareTo((double) b.SubItems[column].Tag);

                if (result == 0)
                    result = string.Compare(((DataObjectBase) a.Tag).LFriendlyName, ((DataObjectBase) b.Tag).LFriendlyName, StringComparison.Ordinal);
                if (result == 0)
                    result = string.Compare(((DataObjectBase) a.Tag).Name, ((DataObjectBase) b.Tag).Name, StringComparison.Ordinal);
                return result * reverseSortLambda;
            });

            UpdateFilteredItemsList(unfilteredList, filteredList, owner);
            owner.Invalidate();
        }

        private void KeyNodesListView_ColumnClick(object sender, ColumnClickEventArgs e) {
            const int maxDigits = 20;
            var comparerRegex = new Regex(@"\d+", RegexOptions.Compiled);
            var stringComparerProcessedStrings = new Dictionary<string, string>();

            // last sort was this very column -> this is now a reverse sort
            var reverseSortLambda = _lastSortOrder[KeyNodesListView] == e.Column + 1 ? -1 : 1;
            _lastSortOrder[KeyNodesListView] = reverseSortLambda * (e.Column + 1);

            _unfilteredKeyNodesList.Sort((a, b) => {
                int result;
                if (e.Column == 2)
                    result = NaturalCompareStrings(a.SubItems[2].Text, b.SubItems[2].Text);
                else if (e.Column < 3)
                    result = string.Compare(a.SubItems[e.Column].Text.ToLower(), b.SubItems[e.Column].Text.ToLower(), StringComparison.Ordinal);
                else
                    result = -((double) a.SubItems[e.Column].Tag).CompareTo((double) b.SubItems[e.Column].Tag);

                if (result == 0 && e.Column != 2)
                    result = NaturalCompareStrings(a.SubItems[2].Text, b.SubItems[2].Text);
                if (result == 0 && e.Column != 0)
                    result = string.Compare(a.SubItems[0].Text.ToLower(), b.SubItems[0].Text.ToLower(), StringComparison.Ordinal);
                if (result == 0 && e.Column != 1)
                    result = string.Compare(a.SubItems[1].Text.ToLower(), b.SubItems[1].Text.ToLower(), StringComparison.Ordinal);
                if (result == 0)
                    result = ((ReadOnlyBaseNode) a.Tag).NodeId.CompareTo(((ReadOnlyBaseNode) b.Tag).NodeId);
                return result * reverseSortLambda;
            });

            UpdateFilteredKeyNodesList();
            KeyNodesListView.Invalidate();
            return;

            int NaturalCompareStrings(string a, string b) {
                if (!stringComparerProcessedStrings.ContainsKey(a))
                    stringComparerProcessedStrings.Add(a, comparerRegex.Replace(a.ToLower(), matcha => matcha.Value.PadLeft(maxDigits, '0')));
                if (!stringComparerProcessedStrings.ContainsKey(b))
                    stringComparerProcessedStrings.Add(b, comparerRegex.Replace(b.ToLower(), matcha => matcha.Value.PadLeft(maxDigits, '0')));

                return string.Compare(stringComparerProcessedStrings[a], stringComparerProcessedStrings[b], StringComparison.Ordinal);
            }
        }

        //-------------------------------------------------------------------------------------------------------Export CSV functions

        private void BuildingsExportButton_Click(object sender, EventArgs e) {
            ExportCsv(
                [_filteredAssemblerList, _filteredMinerList, _filteredPowerList, _filteredBeaconList],
                [
                    ["#", "Assembler", "Electrical power consumed by assemblers (in W)", "Electrical power consumed by beacons (in W)"],
                    ["#", "Miner", "Electrical power consumed by assemblers (in W)", "Electrical power consumed by beacons (in W)"],
                    ["#", "Power Building", "Electrical power generated (in W)", "Electrical power consumed (in W)"],
                    ["#", "Beacon", "Electrical power consumed by beacons (in W)"]
                ]);
        }

        private void ItemsExportButton_Click(object sender, EventArgs e) {
            ExportCsv(
                [_filteredItemsList, _filteredFluidsList],
                [
                    [
                        "Item", "Input (per " + _rateString + ")", "Input through un-linked recipe ingredients (per " + _rateString + ")",
                        "Output (per " + _rateString + ")", "Output through un-linked recipe products (per " + _rateString + ")",
                        "Output through overproduction (per " + _rateString + ")", "Produced by recipe nodes (per " + _rateString + ")",
                        "Consumed by recipe nodes (per " + _rateString + ")"
                    ],
                    [
                        "Fluid", "Input (per " + _rateString + ")", "Input through un-linked recipe ingredients (per " + _rateString + ")",
                        "Output (per " + _rateString + ")", "Output through un-linked recipe products (per " + _rateString + ")",
                        "Output through overproduction (per " + _rateString + ")", "Produced by recipe nodes (per " + _rateString + ")",
                        "Consumed by recipe nodes (per " + _rateString + ")"
                    ]
                ]);
        }

        private void keyNodesExportButton_Click(object sender, EventArgs e) {
            ExportCsv(
                [_filteredKeyNodesList],
                [
                    [
                        "Node Type", "Node Details (item / recipe name)", "Node Title", "Throughput (for non-recipe nodes) (per " + _rateString + ")",
                        "Building Count (for recipe nodes)"
                    ]
                ]);
        }

        private void ExportCsv(List<ListViewItem>[] inputList, string[][] columnNames) {
            using var dialog = new SaveFileDialog();

            dialog.AddExtension = true;
            dialog.Filter = "CSV (*.csv)|*.csv";
            dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Exported CSVs");
            if (!Directory.Exists(dialog.InitialDirectory))
                Directory.CreateDirectory(dialog.InitialDirectory);
            dialog.FileName = "foreman data.csv";
            dialog.ValidateNames = true;
            dialog.OverwritePrompt = true;
            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
                return;

            var csvLines = new List<string[]>();

            for (var i = 0; i < inputList.Length; i++) {
                csvLines.Add(columnNames[i]);
                foreach (var lvi in inputList[i]) {
                    var cLine = new string[columnNames[i].Length];
                    for (var j = 0; j < cLine.Length; j++)
                        cLine[j] = (lvi.SubItems[j].Tag ?? lvi.SubItems[j].Text).ToString().Replace(",", "").Replace("\n", "; ").Replace("\t", "");
                    csvLines.Add(cLine);
                }

                csvLines.Add([""]);
            }

            if (csvLines.Count > 0)
                csvLines.RemoveAt(csvLines.Count - 1);

            // export to csv.

            var csvBuilder = new StringBuilder();
            csvLines.ForEach(line => { csvBuilder.AppendLine(string.Join(",", line)); });
            File.WriteAllText(dialog.FileName, csvBuilder.ToString());
        }
    }
}