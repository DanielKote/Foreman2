using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
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
            public double Input { get; set; } = i; public double InputUnlinked { get; set; } = iu; public double Output { get; set; } = o; public double OutputUnlinked { get; set; } = ou; public double OutputOverflow { get; set; } = oo; public double Production { get; set; } = p; public double Consumption { get; set; } = c;
        }


        private readonly List<ListViewItem> unfilteredAssemblerList;
        private readonly List<ListViewItem> unfilteredMinerList;
        private readonly List<ListViewItem> unfilteredPowerList;
        private readonly List<ListViewItem> unfilteredBeaconList;

        private readonly List<ListViewItem> unfilteredItemsList;
        private readonly List<ListViewItem> unfilteredFluidsList;

        private readonly List<ListViewItem> unfilteredKeyNodesList;

        private readonly List<ListViewItem> filteredAssemblerList;
        private readonly List<ListViewItem> filteredMinerList;
        private readonly List<ListViewItem> filteredPowerList;
        private readonly List<ListViewItem> filteredBeaconList;

        private readonly List<ListViewItem> filteredItemsList;
        private readonly List<ListViewItem> filteredFluidsList;

        private readonly List<ListViewItem> filteredKeyNodesList;

        private readonly Dictionary<ListView, int> lastSortOrder; //int is +ve if sorted down, -ve if sorted up, |value| is the column # (starts from 1 due to 0 not having a sign) of the sort.

        private readonly string rateString;

        private static readonly Color AvailableObjectColor = Color.White;
        private static readonly Color UnavailableObjectColor = Color.Pink;

        public GraphSummaryForm(IProductionGraphSession session, string rateString)
            : this(session.View.Nodes, rateString) {
        }

        public GraphSummaryForm(IEnumerable<INodeViewModel> nodes, string rateString) {
            InitializeComponent();
            MainForm.SetDoubleBuffered(AssemblerListView);
            MainForm.SetDoubleBuffered(MinerListView);
            MainForm.SetDoubleBuffered(PowerListView);
            MainForm.SetDoubleBuffered(BeaconListView);
            MainForm.SetDoubleBuffered(IItemsListView);
            MainForm.SetDoubleBuffered(IFluidsListView);
            MainForm.SetDoubleBuffered(KeyNodesListView);

            unfilteredAssemblerList = [];
            unfilteredMinerList = [];
            unfilteredPowerList = [];
            unfilteredBeaconList = [];
            unfilteredItemsList = [];
            unfilteredFluidsList = [];
            unfilteredKeyNodesList = [];

            filteredAssemblerList = [];
            filteredMinerList = [];
            filteredPowerList = [];
            filteredBeaconList = [];
            filteredItemsList = [];
            filteredFluidsList = [];
            filteredKeyNodesList = [];

            lastSortOrder = new Dictionary<ListView, int> {
                { AssemblerListView, 2 },
                { MinerListView, 2 },
                { PowerListView, 2 },
                { BeaconListView, 2 },
                { IItemsListView, 1 },
                { IFluidsListView, 1 },
                { KeyNodesListView, 1 }
            };

            IconList.Images.Clear();
            IconList.Images.Add(DataCache.UnknownIcon);

            IItemsTabPage.Text += " ( per " + rateString + ")";
            this.rateString = rateString;

            IEnumerable<RecipeNodeViewModel> recipeNodes = nodes.OfType<RecipeNodeViewModel>();

            //lists
            LoadUnfilteredSelectedAssemblerList(recipeNodes.Where(r => r.SelectedAssembler.Assembler.EntityType == EntityType.Assembler), unfilteredAssemblerList);
            LoadUnfilteredSelectedAssemblerList(recipeNodes.Where(r => r.SelectedAssembler.Assembler.EntityType == EntityType.Miner || r.SelectedAssembler.Assembler.EntityType == EntityType.OffshorePump), unfilteredMinerList);
            LoadUnfilteredSelectedAssemblerList(recipeNodes.Where(r => r.SelectedAssembler.Assembler.EntityType == EntityType.Boiler || r.SelectedAssembler.Assembler.EntityType == EntityType.BurnerGenerator || r.SelectedAssembler.Assembler.EntityType == EntityType.Generator || r.SelectedAssembler.Assembler.EntityType == EntityType.Reactor), unfilteredPowerList);

            LoadUnfilteredBeaconList(recipeNodes.Where(r => r.SelectedBeacon), unfilteredBeaconList);

            LoadUnfilteredItemLists(nodes, false, unfilteredItemsList);
            LoadUnfilteredItemLists(nodes, true, unfilteredFluidsList);

            LoadUnfilteredKeyNodesList(nodes.Where(n => n.KeyNode), unfilteredKeyNodesList);

            //building totals
            double buildingTotal = recipeNodes.Sum(n => Math.Ceiling(n.ActualSetValue));
            double beaconTotal = recipeNodes.Sum(n => n.GetTotalBeacons());
            BuildingCountLabel.Text += GraphicsStuff.DoubleToString(buildingTotal);
            BeaconCountLabel.Text += GraphicsStuff.DoubleToString(beaconTotal);

            //power totals
            double powerConsumption = recipeNodes.Sum(n => n.GetTotalAssemblerElectricalConsumption() + n.GetTotalBeaconElectricalConsumption());
            double powerProduction = recipeNodes.Sum(n => n.GetTotalGeneratorElectricalProduction());
            PowerConsumptionLabel.Text += GraphicsStuff.DoubleToEnergy(powerConsumption, "W");
            PowerProductionLabel.Text += GraphicsStuff.DoubleToEnergy(powerProduction, "W");
            if (powerConsumption > 0 && powerProduction > 0) {
                PowerNetLabel.Visible = true;
                PowerNetLabel.Text += GraphicsStuff.DoubleToEnergy(powerProduction - powerConsumption, "W");
            }

            //update filtered
            UpdateFilteredBuildingLists();
            UpdateFilteredItemsLists();
            UpdateFilteredKeyNodesList();
        }

        //-------------------------------------------------------------------------------------------------------Initial list initialization

        private void LoadUnfilteredSelectedAssemblerList(IEnumerable<RecipeNodeViewModel> origin, List<ListViewItem> lviList) {
            var buildingCounters = new Dictionary<AssemblerQualityPair, int>();
            var buildingElectricalPower = new Dictionary<AssemblerQualityPair, Tuple<double, double>>(); //power for buildings, power for beacons)

            foreach (RecipeNodeViewModel rnode in origin) {
                if (buildingCounters.TryAdd(rnode.SelectedAssembler, 0))
                    buildingElectricalPower.Add(rnode.SelectedAssembler, new Tuple<double, double>(0, 0));
                buildingCounters[rnode.SelectedAssembler] += (int)Math.Ceiling(rnode.ActualSetValue); //should probably check the validity of ceiling in case of near correct (ex: 1.0001 assemblers should really be counted as 1 instead of 2)
                Tuple<double, double> oldValues = buildingElectricalPower[rnode.SelectedAssembler];
                buildingElectricalPower[rnode.SelectedAssembler] = new Tuple<double, double>(oldValues.Item1 + rnode.GetTotalGeneratorElectricalProduction() + rnode.GetTotalAssemblerElectricalConsumption(), oldValues.Item2 + rnode.GetTotalBeaconElectricalConsumption());
            }

            foreach (AssemblerQualityPair assembler in buildingCounters.Keys.OrderByDescending(a => a.Assembler.Available).ThenBy(a => a.Assembler.FriendlyName).ThenBy(a => a.Quality.Level).ThenBy(a => a.Quality.FriendlyName)) {
                var lvItem = new ListViewItem();
                Bitmap? icon = assembler.Icon;
                if (icon != null) {
                    IconList.Images.Add(icon);
                    lvItem.ImageIndex = IconList.Images.Count - 1;
                } else {
                    lvItem.ImageIndex = 0;
                }

                lvItem.Text = buildingCounters[assembler] >= 10000000 ? buildingCounters[assembler].ToString("0.##e0", DisplayCulture.Format) : buildingCounters[assembler].ToString("N0", DisplayCulture.Format);
                lvItem.Tag = assembler;
                lvItem.Name = assembler.Assembler.Name + ":" + assembler.Quality.Name; //key
                lvItem.BackColor = assembler.Assembler.Available ? AvailableObjectColor : UnavailableObjectColor;
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = assembler.FriendlyName });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = buildingElectricalPower[assembler].Item1 == 0 ? "-" : GraphicsStuff.DoubleToEnergy(buildingElectricalPower[assembler].Item1, "W"), Tag = buildingElectricalPower[assembler].Item1 });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = buildingElectricalPower[assembler].Item2 == 0 ? "-" : GraphicsStuff.DoubleToEnergy(buildingElectricalPower[assembler].Item2, "W"), Tag = buildingElectricalPower[assembler].Item2 });
                lviList.Add(lvItem);
            }
        }

        private void LoadUnfilteredBeaconList(IEnumerable<RecipeNodeViewModel> origin, List<ListViewItem> lviList) {
            var beaconCounters = new Dictionary<BeaconQualityPair, int>();

            foreach (RecipeNodeViewModel rnode in origin) {
                if (!rnode.SelectedBeacon)
                    continue;

                if (!beaconCounters.ContainsKey(rnode.SelectedBeacon))
                    beaconCounters[rnode.SelectedBeacon] = 0;
                beaconCounters[rnode.SelectedBeacon] += rnode.GetTotalBeacons();
            }

            List<(BeaconQualityPair Pair, IBeacon Beacon, IQuality Quality)> sortedBeacons = [];
            foreach (BeaconQualityPair pair in beaconCounters.Keys) {
                if (pair is { Beacon: IBeacon beaconEntity, Quality: IQuality qualityEntity })
                    sortedBeacons.Add((pair, beaconEntity, qualityEntity));
            }
            sortedBeacons.Sort((a, b) => {
                int result = b.Beacon.Available.CompareTo(a.Beacon.Available);
                if (result != 0)
                    return result;
                result = string.Compare(a.Beacon.FriendlyName, b.Beacon.FriendlyName, StringComparison.Ordinal);
                if (result != 0)
                    return result;
                result = a.Quality.Level.CompareTo(b.Quality.Level);
                return result != 0 ? result : string.Compare(a.Quality.FriendlyName, b.Quality.FriendlyName, StringComparison.Ordinal);
            });

            foreach ((BeaconQualityPair beacon, IBeacon beaconEntity, IQuality qualityEntity) in sortedBeacons) {
                var lvItem = new ListViewItem();
                Bitmap? icon = beacon.Icon;
                if (icon != null) {
                    IconList.Images.Add(icon);
                    lvItem.ImageIndex = IconList.Images.Count - 1;
                } else {
                    lvItem.ImageIndex = 0;
                }

                lvItem.Text = beaconCounters[beacon].ToString(DisplayCulture.Format);
                lvItem.Tag = beacon;
                lvItem.Name = beaconEntity.Name + ":" + qualityEntity.Name; //key
                lvItem.BackColor = beaconEntity.Available ? AvailableObjectColor : UnavailableObjectColor;
                lvItem.SubItems.Add(beacon.FriendlyName ?? string.Empty);
                double beaconPowerConsumption = beaconCounters[beacon] * (beaconEntity.GetEnergyConsumption(qualityEntity) + beaconEntity.GetEnergyDrain());  //QUALITY UPDATE REQUIRED
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = beaconCounters[beacon] == 0 ? "-" : GraphicsStuff.DoubleToEnergy(beaconPowerConsumption, "W"), Tag = beaconPowerConsumption });
                lviList.Add(lvItem);
            }
        }

        private void LoadUnfilteredItemLists(IEnumerable<INodeViewModel> nodes, bool fluids, List<ListViewItem> lviList) {
            //NOTE: throughput is initially calculatated as all non-overflow linked input & output of each recipe node. At the end we will add
            var itemCounters = new Dictionary<ItemQualityPair, ItemCounter>();

            foreach (INodeViewModel node in nodes) {
                if (node is RecipeNodeViewModel) {
                    foreach (ItemQualityPair input in node.Inputs.Where(i => fluids.Equals(i.Item is IFluid))) {
                        if (!itemCounters.ContainsKey(input))
                            itemCounters.Add(input, new ItemCounter(0, 0, 0, 0, 0, 0, 0));

                        double consumeRate = node.GetConsumeRate(input);
                        if (consumeRate > 0) {
                            if (!node.InputLinks.Any(l => l.Item == input))
                                itemCounters[input].InputUnlinked += consumeRate;
                            else
                                itemCounters[input].Consumption += consumeRate;
                        }
                    }

                    foreach (ItemQualityPair output in node.Outputs.Where(i => fluids.Equals(i.Item is IFluid))) {
                        if (!itemCounters.ContainsKey(output))
                            itemCounters.Add(output, new ItemCounter(0, 0, 0, 0, 0, 0, 0));

                        double supplyRate = node.GetSupplyRate(output);
                        bool isOverProduced = node.IsOverproducing(output);
                        double supplyUsedRate = isOverProduced ? node.GetSupplyUsedRate(output) : supplyRate;

                        if (supplyRate > 0) {
                            if (!node.OutputLinks.Any(l => l.Item == output))
                                itemCounters[output].OutputUnlinked += supplyRate;

                            itemCounters[output].Production += supplyRate;
                            if (isOverProduced)
                                itemCounters[output].OutputOverflow += supplyRate - supplyUsedRate;
                        }
                    }
                } else if (node is ISupplierNodeViewModel sNode && fluids.Equals(sNode.SuppliedItem.Item is IFluid)) {
                    if (!itemCounters.ContainsKey(sNode.SuppliedItem))
                        itemCounters.Add(sNode.SuppliedItem, new ItemCounter(0, 0, 0, 0, 0, 0, 0));
                    itemCounters[sNode.SuppliedItem].Input += sNode.ActualRate;
                } else if (node is IConsumerNodeViewModel cNode && fluids.Equals(cNode.ConsumedItem.Item is IFluid)) {
                    if (!itemCounters.ContainsKey(cNode.ConsumedItem))
                        itemCounters.Add(cNode.ConsumedItem, new ItemCounter(0, 0, 0, 0, 0, 0, 0));
                    itemCounters[cNode.ConsumedItem].Output += cNode.ActualRate;
                }
            }

            List<(ItemQualityPair Pair, IItem Item, IQuality Quality)> sortedItems = [];
            foreach (ItemQualityPair pair in itemCounters.Keys) {
                if (pair is { Item: IItem itemEntity, Quality: IQuality qualityEntity })
                    sortedItems.Add((pair, itemEntity, qualityEntity));
            }
            sortedItems.Sort((a, b) => {
                int result = string.Compare(a.Item.FriendlyName, b.Item.FriendlyName, StringComparison.Ordinal);
                if (result != 0)
                    return result;
                result = a.Quality.Level.CompareTo(b.Quality.Level);
                return result != 0 ? result : string.Compare(a.Quality.FriendlyName, b.Quality.FriendlyName, StringComparison.Ordinal);
            });

            foreach ((ItemQualityPair item, IItem itemEntity, IQuality qualityEntity) in sortedItems) {
                var lvItem = new ListViewItem();
                Bitmap? icon = item.Icon;
                if (icon != null) {
                    IconList.Images.Add(icon);
                    lvItem.ImageIndex = IconList.Images.Count - 1;
                } else {
                    lvItem.ImageIndex = 0;
                }

                lvItem.Text = item.FriendlyName ?? string.Empty;
                lvItem.Tag = item;
                lvItem.Name = itemEntity.Name + ":" + qualityEntity.Name; //key
                lvItem.BackColor = itemEntity.Available ? AvailableObjectColor : UnavailableObjectColor;
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].Input == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Input), Tag = itemCounters[item].Input });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].InputUnlinked == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].InputUnlinked), Tag = itemCounters[item].InputUnlinked });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].Output == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Output), Tag = itemCounters[item].Output });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].OutputUnlinked == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].OutputUnlinked), Tag = itemCounters[item].OutputUnlinked });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].OutputOverflow == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].OutputOverflow), Tag = itemCounters[item].OutputOverflow });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].Production == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Production), Tag = itemCounters[item].Production });
                lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].Consumption == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Consumption), Tag = itemCounters[item].Consumption });
                lviList.Add(lvItem);
            }
        }

        private void LoadUnfilteredKeyNodesList(IEnumerable<INodeViewModel> origin, List<ListViewItem> lviList) {
            foreach (INodeViewModel node in origin) {
                var lvItem = new ListViewItem();

                Bitmap? icon;
                string? nodeText;
                string nodeType;
                if (node is IConsumerNodeViewModel cNode) {
                    icon = cNode.ConsumedItem.Icon;
                    nodeText = cNode.ConsumedItem.FriendlyName;
                    nodeType = "Consumer";
                } else if (node is ISupplierNodeViewModel sNode) {
                    icon = sNode.SuppliedItem.Icon;
                    nodeText = sNode.SuppliedItem.FriendlyName;
                    nodeType = "Supplier";
                } else if (node is IPassthroughNodeViewModel pNode) {
                    icon = pNode.PassthroughItem.Icon;
                    nodeText = pNode.PassthroughItem.FriendlyName;
                    nodeType = "Passthrough";
                } else if (node is RecipeNodeViewModel rNode) {
                    icon = rNode.BaseRecipe.Icon;
                    nodeText = rNode.BaseRecipe.FriendlyName;
                    nodeType = "Recipe";
                } else if (node is ISpoilNodeViewModel spNode) {
                    icon = spNode.InputItem.Icon;
                    nodeText = spNode.InputItem.FriendlyName + " spoiling";
                    nodeType = "Spoil";
                } else if (node is IPlantNodeViewModel plNode) {
                    icon = plNode.Seed.Icon;
                    nodeText = plNode.Seed.FriendlyName + " planting";
                    nodeType = "Plant";
                } else
                    continue;

                if (icon != null) {
                    IconList.Images.Add(icon);
                    lvItem.ImageIndex = IconList.Images.Count - 1;
                } else {
                    lvItem.ImageIndex = 0;
                }

                lvItem.Text = nodeType;
                lvItem.Tag = node;
                lvItem.Name = nodeText ?? string.Empty; //key
                lvItem.BackColor = AvailableObjectColor;
                lvItem.SubItems.Add(nodeText ?? string.Empty);
                lvItem.SubItems.Add(node.KeyNodeTitle);

                if (node is RecipeNodeViewModel rrNode) {
                    lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = "-", Tag = (double)0 });
                    lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = GraphicsStuff.DoubleToString(rrNode.ActualSetValue), Tag = rrNode.ActualSetValue });
                } else {
                    lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = GraphicsStuff.DoubleToString(node.ActualRate), Tag = node.ActualRate });
                    lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = "-", Tag = (double)0 });
                }
                lviList.Add(lvItem);
            }
        }

        //-------------------------------------------------------------------------------------------------------Filter functions

        /// <summary>
        /// Summary list rows store value-type tags (e.g. AssemblerQualityPair, ItemQualityPair), not IDataObjectBase.
        /// Match the filter against visible text (primary column + subitems).
        /// </summary>
        private static bool ListViewItemRowContainsFilter(ListViewItem lvItem, string filterLower) {
            if (string.IsNullOrEmpty(filterLower))
                return true;
            if (lvItem.Text.Contains(filterLower, StringComparison.OrdinalIgnoreCase))
                return true;
            for (int i = 0; i < lvItem.SubItems.Count; i++) {
                if (lvItem.SubItems[i].Text.Contains(filterLower, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void UpdateFilteredBuildingLists() {
            UpdateFilteredBuildingList(unfilteredAssemblerList, filteredAssemblerList, AssemblerListView);
            UpdateFilteredBuildingList(unfilteredMinerList, filteredMinerList, MinerListView);
            UpdateFilteredBuildingList(unfilteredPowerList, filteredPowerList, PowerListView);
            UpdateFilteredBuildingList(unfilteredBeaconList, filteredBeaconList, BeaconListView);
        }

        private void UpdateFilteredBuildingList(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner) {
            string filterString = BuildingsFilterTextBox.Text.ToLowerInvariant();

            filteredList.Clear();

            foreach (ListViewItem lvItem in unfilteredList)
                if (ListViewItemRowContainsFilter(lvItem, filterString))
                    filteredList.Add(lvItem);

            owner.VirtualListSize = filteredList.Count;
            owner.Invalidate();
        }

        private void UpdateFilteredItemsLists() {
            UpdateFilteredItemsList(unfilteredItemsList, filteredItemsList, IItemsListView);
            UpdateFilteredItemsList(unfilteredFluidsList, filteredFluidsList, IFluidsListView);
        }

        private void UpdateFilteredItemsList(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner) {
            string filterString = IItemsFilterTextBox.Text.ToLowerInvariant();
            bool includeInputs = ItemFilterInputCheckBox.Checked;
            bool includeInputUnlinked = ItemFilterInputUnlinkedCheckBox.Checked;
            bool includeOutputs = ItemFilterOutputCheckBox.Checked;
            bool includeOutputsUnlinked = ItemFilterOutputUnlinkedCheckBox.Checked;
            bool includeOutputsOverflow = ItemFilterOutputOverproducedCheckBox.Checked;
            bool includeProduced = ItemFilterProductionCheckBox.Checked;
            bool includeConsumed = ItemFilterConsumptionCheckBox.Checked;

            filteredList.Clear();

            foreach (ListViewItem lvItem in unfilteredList) {
                if (!ListViewItemRowContainsFilter(lvItem, filterString))
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
            string filterString = KeyNodesFilterTextBox.Text.ToLowerInvariant();
            bool includeSuppliers = SupplierNodeFilterCheckBox.Checked;
            bool includeConsumers = ConsumerNodeFilterCheckBox.Checked;
            bool includePassthrough = PassthroughNodeFilterCheckBox.Checked;
            bool includeRecipe = RecipeNodeFilterCheckBox.Checked;

            filteredKeyNodesList.Clear();

            foreach (ListViewItem lvItem in unfilteredKeyNodesList) {
                if (string.IsNullOrEmpty(filterString) || lvItem.Text.Contains(filterString, StringComparison.OrdinalIgnoreCase) || lvItem.SubItems[1].Text.Contains(filterString, StringComparison.OrdinalIgnoreCase) || lvItem.SubItems[2].Text.Contains(filterString, StringComparison.OrdinalIgnoreCase)) {
                    if ((includeSuppliers && (lvItem.Tag is ISupplierNodeViewModel)) ||
                        (includeConsumers && (lvItem.Tag is IConsumerNodeViewModel)) ||
                        (includePassthrough && (lvItem.Tag is IPassthroughNodeViewModel)) ||
                        (includeRecipe && (lvItem.Tag is RecipeNodeViewModel))) {
                        filteredKeyNodesList.Add(lvItem);
                    }
                }
            }

            KeyNodesListView.VirtualListSize = filteredKeyNodesList.Count;
            KeyNodesListView.Invalidate();
        }

        //-------------------------------------------------------------------------------------------------------Virtual item retrieval for all list views

        private void AssemblerListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredAssemblerList[e.ItemIndex]; }
        private void MinerListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredMinerList[e.ItemIndex]; }
        private void PowerListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredPowerList[e.ItemIndex]; }
        private void BeaconListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredBeaconList[e.ItemIndex]; }
        private void IItemsListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredItemsList[e.ItemIndex]; }
        private void IFluidsListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredFluidsList[e.ItemIndex]; }
        private void KeyNodesListView_RetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredKeyNodesList[e.ItemIndex]; }

        //-------------------------------------------------------------------------------------------------------Filter changed events

        private void BuildingsFilterTextBox_TextChanged(object? sender, EventArgs e) { UpdateFilteredBuildingLists(); }

        private void IItemsFilterTextBox_TextChanged(object? sender, EventArgs e) { UpdateFilteredItemsLists(); }
        private void ItemFilterCheckBox_CheckedChanged(object? sender, EventArgs e) { UpdateFilteredItemsLists(); }

        private void KeyNodesFilterTextBox_TextChanged(object? sender, EventArgs e) { UpdateFilteredKeyNodesList(); }
        private void KeyNodesFilterCheckBox_CheckedChanged(object? sender, EventArgs e) { UpdateFilteredKeyNodesList(); }

        //-------------------------------------------------------------------------------------------------------Column clicked events

        private void AssemblerListView_ColumnClick(object? sender, ColumnClickEventArgs e) { BuildingListView_ColumnSort(unfilteredAssemblerList, filteredAssemblerList, AssemblerListView, e.Column); }
        private void MinerListView_ColumnClick(object? sender, ColumnClickEventArgs e) { BuildingListView_ColumnSort(unfilteredMinerList, filteredMinerList, MinerListView, e.Column); }
        private void PowerListView_ColumnClick(object? sender, ColumnClickEventArgs e) { BuildingListView_ColumnSort(unfilteredPowerList, filteredPowerList, PowerListView, e.Column); }
        private void BeaconListView_ColumnClick(object? sender, ColumnClickEventArgs e) { BuildingListView_ColumnSort(unfilteredBeaconList, filteredBeaconList, BeaconListView, e.Column); }

        private void BuildingListView_ColumnSort(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner, int column) {
            int reverseSortLamda = (lastSortOrder[owner] == column + 1) ? -1 : 1; //last sort was this very column -> this is now a reverse sort
            lastSortOrder[owner] = reverseSortLamda * (column + 1);

            unfilteredList.Sort((a, b) => {
                var result = column == 0
                    ? -double.Parse(a.Text, DisplayCulture.Format).CompareTo(double.Parse(b.Text, DisplayCulture.Format))
                    : column == 1
                    ? string.Compare(a.SubItems[1].Text, b.SubItems[1].Text, StringComparison.OrdinalIgnoreCase)
                    : a.SubItems[column].Tag is double aValue && b.SubItems[column].Tag is double bValue
                        ? -aValue.CompareTo(bValue)
                        : 0;

                if (result == 0) {
                    string nameA = a.SubItems.Count > 1 ? a.SubItems[1].Text : a.Text;
                    string nameB = b.SubItems.Count > 1 ? b.SubItems[1].Text : b.Text;
                    result = string.Compare(nameA, nameB, StringComparison.OrdinalIgnoreCase);
                }
                if (result == 0)
                    result = string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                return result * reverseSortLamda;

            });

            UpdateFilteredBuildingList(unfilteredList, filteredList, owner);
            owner.Invalidate();
        }

        private void IItemsListView_ColumnClick(object? sender, ColumnClickEventArgs e) { ItemListView_ColumnSort(unfilteredItemsList, filteredItemsList, IItemsListView, e.Column); }
        private void IFluidsListView_ColumnClick(object? sender, ColumnClickEventArgs e) { ItemListView_ColumnSort(unfilteredFluidsList, filteredFluidsList, IFluidsListView, e.Column); }

        private void ItemListView_ColumnSort(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner, int column) {
            int reverseSortLamda = (lastSortOrder[owner] == column + 1) ? -1 : 1; //last sort was this very column -> this is now a reverse sort
            lastSortOrder[owner] = reverseSortLamda * (column + 1);

            unfilteredList.Sort((a, b) => {
                var result = column == 0
                    ? string.Compare(a.SubItems[0].Text, b.SubItems[0].Text, StringComparison.OrdinalIgnoreCase)
                    : a.SubItems[column].Tag is double aValue && b.SubItems[column].Tag is double bValue
                        ? -aValue.CompareTo(bValue)
                        : 0;

                if (result == 0)
                    result = string.Compare(a.Text, b.Text, StringComparison.OrdinalIgnoreCase);
                if (result == 0)
                    result = string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                return result * reverseSortLamda;
            });

            UpdateFilteredItemsList(unfilteredList, filteredList, owner);
            owner.Invalidate();
        }

        private void KeyNodesListView_ColumnClick(object? sender, ColumnClickEventArgs e) {
            const int maxDigits = 20;
            var comparerRegex = ComparerRegex();
            var stringComparerProcessedStrings = new Dictionary<string, string>();
            int NaturalCompareStrings(string a, string b) {
                if (!stringComparerProcessedStrings.ContainsKey(a))
                    stringComparerProcessedStrings.Add(a, comparerRegex.Replace(a.ToLowerInvariant(), matcha => matcha.Value.PadLeft(maxDigits, '0')));
                if (!stringComparerProcessedStrings.ContainsKey(b))
                    stringComparerProcessedStrings.Add(b, comparerRegex.Replace(b.ToLowerInvariant(), matcha => matcha.Value.PadLeft(maxDigits, '0')));

                return string.Compare(stringComparerProcessedStrings[a], stringComparerProcessedStrings[b], StringComparison.Ordinal);
            }

            int reverseSortLamda = (lastSortOrder[KeyNodesListView] == e.Column + 1) ? -1 : 1; //last sort was this very column -> this is now a reverse sort
            lastSortOrder[KeyNodesListView] = reverseSortLamda * (e.Column + 1);

            unfilteredKeyNodesList.Sort((a, b) => {
                var result = e.Column == 2
                    ? NaturalCompareStrings(a.SubItems[2].Text, b.SubItems[2].Text)
                    : e.Column < 3
                    ? string.Compare(a.SubItems[e.Column].Text, b.SubItems[e.Column].Text, StringComparison.OrdinalIgnoreCase)
                    : a.SubItems[e.Column].Tag is double aValue && b.SubItems[e.Column].Tag is double bValue
                        ? -aValue.CompareTo(bValue)
                        : 0;

                if (result == 0 && e.Column != 2)
                    result = NaturalCompareStrings(a.SubItems[2].Text, b.SubItems[2].Text);
                if (result == 0 && e.Column != 0)
                    result = string.Compare(a.SubItems[0].Text, b.SubItems[0].Text, StringComparison.OrdinalIgnoreCase);
                if (result == 0 && e.Column != 1)
                    result = string.Compare(a.SubItems[1].Text, b.SubItems[1].Text, StringComparison.OrdinalIgnoreCase);
                if (result == 0)
                    result = a.Tag is INodeViewModel nodeA && b.Tag is INodeViewModel nodeB
                        ? nodeA.Id.Value.CompareTo(nodeB.Id.Value)
                        : 0;
                return result * reverseSortLamda;
            });

            UpdateFilteredKeyNodesList();
            KeyNodesListView.Invalidate();
        }

        //-------------------------------------------------------------------------------------------------------Export CSV functions

        private static readonly string[] BuildingsExportAssemblerHeader = ["#", "Assembler", "Electrical power consumed by assemblers (in W)", "Electrical power consumed by beacons (in W)"];
        private static readonly string[] BuildingsExportMinerHeader = ["#", "Miner", "Electrical power consumed by assemblers (in W)", "Electrical power consumed by beacons (in W)"];
        private static readonly string[] BuildingsExportPowerHeader = ["#", "Power Building", "Electrical power generated (in W)", "Electrical power consumed (in W)"];
        private static readonly string[] BuildingsExportBeaconHeader = ["#", "Beacon", "Electrical power consumed by beacons (in W)"];

        private void BuildingsExportButton_Click(object? sender, EventArgs e) {
            ExportCSV(
                [filteredAssemblerList, filteredMinerList, filteredPowerList, filteredBeaconList],
                [
                    BuildingsExportAssemblerHeader,
                    BuildingsExportMinerHeader,
                    BuildingsExportPowerHeader,
                    BuildingsExportBeaconHeader
                ]);
        }

        private void IItemsExportButton_Click(object? sender, EventArgs e) {
            ExportCSV(
                [filteredItemsList, filteredFluidsList],
                [
                    ["Item", "Input (per "+rateString+")", "Input through un-linked recipe ingredients (per "+rateString+")", "Output (per " + rateString + ")", "Output through un-linked recipe products (per " + rateString + ")", "Output through overproduction (per " + rateString + ")", "Produced by recipe nodes (per " + rateString + ")", "Consumed by recipe nodes (per " + rateString + ")"],
                    ["Fluid", "Input (per "+rateString+")", "Input through un-linked recipe ingredients (per "+rateString+")", "Output (per " + rateString + ")", "Output through un-linked recipe products (per " + rateString + ")", "Output through overproduction (per " + rateString + ")", "Produced by recipe nodes (per " + rateString + ")", "Consumed by recipe nodes (per " + rateString + ")"]
                ]);
        }

        private void KeyNodesExportButton_Click(object? sender, EventArgs e) {
            ExportCSV(
                [filteredKeyNodesList],
                [
                    ["Node Type", "Node Details (item / recipe name)", "Node Title", "Throughput (for non-recipe nodes) (per " + rateString + ")", "Building Count (for recipe nodes)"]
                ]);
        }

        private static void ExportCSV(List<ListViewItem>[] inputList, string[][] columnNames) {
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

            if (result == DialogResult.OK) {
                var csvLines = new List<string[]>();

                for (int i = 0; i < inputList.Length; i++) {
                    csvLines.Add(columnNames[i]);
                    foreach (ListViewItem lvi in inputList[i]) {
                        string[] cLine = new string[columnNames[i].Length];
                        for (int j = 0; j < cLine.Length; j++)
                            cLine[j] = (lvi.SubItems[j].Tag as string ?? lvi.SubItems[j].Text).Replace(",", "").Replace("\n", "; ").Replace("\t", "");
                        csvLines.Add(cLine);
                    }
                    csvLines.Add([""]);
                }
                if (csvLines.Count > 0)
                    csvLines.RemoveAt(csvLines.Count - 1);

                //export to csv.
                var csvBuilder = new StringBuilder();
                csvLines.ForEach(line => { csvBuilder.AppendLine(string.Join(",", line)); });
                Utf8File.WriteAllText(dialog.FileName, csvBuilder.ToString());
            }
        }

        [GeneratedRegex(@"\d+", RegexOptions.Compiled)]
        private static partial Regex ComparerRegex();
    }
}
