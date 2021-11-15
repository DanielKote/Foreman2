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
	public partial class GraphSummaryForm : Form
	{
		protected class ItemCounter
		{
			public double Input { get; set; }
			public double InputUnlinked { get; set; }
			public double Output { get; set; }
			public double OutputUnlinked { get; set; }
			public double OutputOverflow { get; set; }
			public double Production { get; set; }
			public double Consumption { get; set; }

			public ItemCounter(double i, double iu, double o, double ou, double oo, double p, double c) { Input = i; InputUnlinked = iu; Output = o; OutputUnlinked = ou; OutputOverflow = oo; Production = p; Consumption = c; }
		}


		private List<ListViewItem> unfilteredAssemblerList;
		private List<ListViewItem> unfilteredMinerList;
		private List<ListViewItem> unfilteredPowerList;
		private List<ListViewItem> unfilteredBeaconList;

		private List<ListViewItem> unfilteredItemsList;
		private List<ListViewItem> unfilteredFluidsList;

		private List<ListViewItem> unfilteredKeyNodesList;

		private List<ListViewItem> filteredAssemblerList;
		private List<ListViewItem> filteredMinerList;
		private List<ListViewItem> filteredPowerList;
		private List<ListViewItem> filteredBeaconList;

		private List<ListViewItem> filteredItemsList;
		private List<ListViewItem> filteredFluidsList;

		private List<ListViewItem> filteredKeyNodesList;

		private Dictionary<ListView, int> lastSortOrder; //int is +ve if sorted down, -ve if sorted up, |value| is the column # (starts from 1 due to 0 not having a sign) of the sort.

		private readonly string rateString;

		private static readonly Color AvailableObjectColor = Color.White;
		private static readonly Color UnavailableObjectColor = Color.Pink;

		public GraphSummaryForm(IEnumerable<ReadOnlyBaseNode> nodes, IEnumerable<ReadOnlyNodeLink> links, string rateString)
		{
			InitializeComponent();
			MainForm.SetDoubleBuffered(AssemblerListView);
			MainForm.SetDoubleBuffered(MinerListView);
			MainForm.SetDoubleBuffered(PowerListView);
			MainForm.SetDoubleBuffered(BeaconListView);
			MainForm.SetDoubleBuffered(ItemsListView);
			MainForm.SetDoubleBuffered(FluidsListView);
			MainForm.SetDoubleBuffered(KeyNodesListView);

			unfilteredAssemblerList = new List<ListViewItem>();
			unfilteredMinerList = new List<ListViewItem>();
			unfilteredPowerList = new List<ListViewItem>();
			unfilteredBeaconList = new List<ListViewItem>();
			unfilteredItemsList = new List<ListViewItem>();
			unfilteredFluidsList = new List<ListViewItem>();
			unfilteredKeyNodesList = new List<ListViewItem>();

			filteredAssemblerList = new List<ListViewItem>();
			filteredMinerList = new List<ListViewItem>();
			filteredPowerList = new List<ListViewItem>();
			filteredBeaconList = new List<ListViewItem>();
			filteredItemsList = new List<ListViewItem>();
			filteredFluidsList = new List<ListViewItem>();
			filteredKeyNodesList = new List<ListViewItem>();

			lastSortOrder = new Dictionary<ListView, int>();
			lastSortOrder.Add(AssemblerListView, 2);
			lastSortOrder.Add(MinerListView, 2);
			lastSortOrder.Add(PowerListView, 2);
			lastSortOrder.Add(BeaconListView, 2);
			lastSortOrder.Add(ItemsListView, 1);
			lastSortOrder.Add(FluidsListView, 1);
			lastSortOrder.Add(KeyNodesListView, 1);

			IconList.Images.Clear();
			IconList.Images.Add(DataCache.UnknownIcon);

			ItemsTabPage.Text += " ( per " + rateString + ")";
			this.rateString = rateString;

			//lists
			LoadUnfilteredSelectedAssemblerList(nodes.Where(n => n is ReadOnlyRecipeNode rNode && rNode.SelectedAssembler.EntityType == EntityType.Assembler).Select(n => (ReadOnlyRecipeNode)n), unfilteredAssemblerList);
			LoadUnfilteredSelectedAssemblerList(nodes.Where(n => n is ReadOnlyRecipeNode rNode && (rNode.SelectedAssembler.EntityType == EntityType.Miner || rNode.SelectedAssembler.EntityType == EntityType.OffshorePump)).Select(n => (ReadOnlyRecipeNode)n), unfilteredMinerList);
			LoadUnfilteredSelectedAssemblerList(nodes.Where(n => n is ReadOnlyRecipeNode rNode && (rNode.SelectedAssembler.EntityType == EntityType.Boiler || rNode.SelectedAssembler.EntityType == EntityType.BurnerGenerator || rNode.SelectedAssembler.EntityType == EntityType.Generator || rNode.SelectedAssembler.EntityType == EntityType.Reactor)).Select(n => (ReadOnlyRecipeNode)n), unfilteredPowerList);

			LoadUnfilteredBeaconList(nodes.Where(n => n is ReadOnlyRecipeNode rNode && rNode.SelectedBeacon != null).Select(n => (ReadOnlyRecipeNode)n), unfilteredBeaconList);

			LoadUnfilteredItemLists(nodes, links, false, unfilteredItemsList);
			LoadUnfilteredItemLists(nodes, links, true, unfilteredFluidsList);

			LoadUnfilteredKeyNodesList(nodes.Where(n => n.KeyNode), unfilteredKeyNodesList);

			//building totals
			double buildingTotal = nodes.Where(n => n is ReadOnlyRecipeNode).Sum(n => Math.Ceiling(((ReadOnlyRecipeNode)n).ActualAssemblerCount));
			double beaconTotal = nodes.Where(n => n is ReadOnlyRecipeNode).Sum(n => ((ReadOnlyRecipeNode)n).GetTotalBeacons());
			BuildingCountLabel.Text += GraphicsStuff.DoubleToString(buildingTotal);
			BeaconCountLabel.Text += GraphicsStuff.DoubleToString(beaconTotal);

			//power totals
			double powerConsumption = nodes.Where(n => n is ReadOnlyRecipeNode).Sum(n => ((ReadOnlyRecipeNode)n).GetTotalAssemblerElectricalConsumption() + ((ReadOnlyRecipeNode)n).GetTotalBeaconElectricalConsumption());
			double powerProduction = nodes.Where(n => n is ReadOnlyRecipeNode).Sum(n => ((ReadOnlyRecipeNode)n).GetTotalGeneratorElectricalProduction());
			PowerConsumptionLabel.Text += GraphicsStuff.DoubleToEnergy(powerConsumption, "W");
			PowerProductionLabel.Text += GraphicsStuff.DoubleToEnergy(powerProduction, "W");

			//update filtered
			UpdateFilteredBuildingLists();
			UpdateFilteredItemsLists();
			UpdateFilteredKeyNodesList();
		}

		//-------------------------------------------------------------------------------------------------------Initial list initialization

		private void LoadUnfilteredSelectedAssemblerList(IEnumerable<ReadOnlyRecipeNode> origin, List<ListViewItem> lviList)
		{
			Dictionary<Assembler, int> buildingCounters = new Dictionary<Assembler, int>();
			Dictionary<Assembler, Tuple<double, double>> buildingElectricalPower = new Dictionary<Assembler, Tuple<double, double>>(); //power for buildings, power for beacons)

			foreach(ReadOnlyRecipeNode rnode in origin)
			{
				if (!buildingCounters.ContainsKey(rnode.SelectedAssembler))
				{
					buildingCounters.Add(rnode.SelectedAssembler, 0);
					buildingElectricalPower.Add(rnode.SelectedAssembler, new Tuple<double, double>(0,0));
				}
				buildingCounters[rnode.SelectedAssembler] += (int)Math.Ceiling(rnode.ActualAssemblerCount); //should probably check the validity of ceiling in case of near correct (ex: 1.0001 assemblers should really be counted as 1 instead of 2)
				Tuple<double, double> oldValues = buildingElectricalPower[rnode.SelectedAssembler];
				buildingElectricalPower[rnode.SelectedAssembler] = new Tuple<double,double>(oldValues.Item1 + rnode.GetTotalGeneratorElectricalProduction() + rnode.GetTotalAssemblerElectricalConsumption(), oldValues.Item2 + rnode.GetTotalBeaconElectricalConsumption());
			}

			foreach (Assembler assembler in buildingCounters.Keys.OrderByDescending(a => a.Available).ThenBy(a => a.FriendlyName))
			{
				ListViewItem lvItem = new ListViewItem();
				if (assembler.Icon != null)
				{
					IconList.Images.Add(assembler.Icon);
					lvItem.ImageIndex = IconList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}

				lvItem.Text = buildingCounters[assembler] >= 10000000? buildingCounters[assembler].ToString("0.##e0") : buildingCounters[assembler].ToString("N0");
				lvItem.Tag = assembler;
				lvItem.Name = assembler.Name; //key
				lvItem.BackColor = assembler.Available ? AvailableObjectColor : UnavailableObjectColor;
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = assembler.FriendlyName });
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = buildingElectricalPower[assembler].Item1 == 0 ? "-" : GraphicsStuff.DoubleToEnergy(buildingElectricalPower[assembler].Item1, "W"), Tag = buildingElectricalPower[assembler].Item1 });
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = buildingElectricalPower[assembler].Item2 == 0 ? "-" : GraphicsStuff.DoubleToEnergy(buildingElectricalPower[assembler].Item2, "W"), Tag = buildingElectricalPower[assembler].Item2 });
				lviList.Add(lvItem);
			}
		}

		private void LoadUnfilteredBeaconList(IEnumerable<ReadOnlyRecipeNode> origin, List<ListViewItem> lviList)
		{
			Dictionary<Beacon, int> beaconCounters = new Dictionary<Beacon, int>();

			foreach (ReadOnlyRecipeNode rnode in origin)
			{
				if (!beaconCounters.ContainsKey(rnode.SelectedBeacon))
					beaconCounters.Add(rnode.SelectedBeacon, 0);
				beaconCounters[rnode.SelectedBeacon] += rnode.GetTotalBeacons();
			}

			foreach (Beacon beacon in beaconCounters.Keys.OrderByDescending(b => b.Available).ThenBy(b => b.FriendlyName))
			{
				ListViewItem lvItem = new ListViewItem();
				if (beacon.Icon != null)
				{
					IconList.Images.Add(beacon.Icon);
					lvItem.ImageIndex = IconList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}

				lvItem.Text = beaconCounters[beacon].ToString();
				lvItem.Tag = beacon;
				lvItem.Name = beacon.Name; //key
				lvItem.BackColor = beacon.Available ? AvailableObjectColor : UnavailableObjectColor;
				lvItem.SubItems.Add(beacon.FriendlyName);
				double beaconPowerConsumption = beaconCounters[beacon] * (beacon.EnergyConsumption + beacon.EnergyDrain);
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = beaconCounters[beacon] == 0 ? "-" : GraphicsStuff.DoubleToEnergy(beaconPowerConsumption, "W"), Tag = beaconPowerConsumption });
				lviList.Add(lvItem);
			}
		}

		private void LoadUnfilteredItemLists(IEnumerable<ReadOnlyBaseNode> nodes, IEnumerable<ReadOnlyNodeLink> links, bool fluids, List<ListViewItem> lviList)
		{
			//NOTE: throughput is initially calculatated as all non-overflow linked input & output of each recipe node. At the end we will add
			Dictionary<Item, ItemCounter> itemCounters = new Dictionary<Item, ItemCounter>();

			foreach (ReadOnlyBaseNode node in nodes)
			{
				if (node is ReadOnlyRecipeNode)
				{
					foreach (Item input in node.Inputs.Where(i => fluids.Equals(i is Fluid)))
					{
						if (!itemCounters.ContainsKey(input))
							itemCounters.Add(input, new ItemCounter(0, 0, 0, 0, 0, 0, 0));

						double consumeRate = node.GetConsumeRate(input);
						if (consumeRate > 0)
						{
							if (!node.InputLinks.Any(l => l.Item == input))
								itemCounters[input].InputUnlinked += consumeRate;
							else
								itemCounters[input].Consumption += consumeRate;
						}
					}

					foreach (Item output in node.Outputs.Where(i => fluids.Equals(i is Fluid)))
					{
						if (!itemCounters.ContainsKey(output))
							itemCounters.Add(output, new ItemCounter(0, 0, 0, 0, 0, 0, 0));

						double supplyRate = node.GetSupplyRate(output);
						bool isOverProduced = node.IsOverproducing(output);
						double supplyUsedRate = isOverProduced ? node.GetSupplyUsedRate(output) : supplyRate;

						if (supplyRate > 0)
						{
							if (!node.OutputLinks.Any(l => l.Item == output))
								itemCounters[output].OutputUnlinked += supplyRate;

							itemCounters[output].Production += supplyRate;
							if (isOverProduced)
								itemCounters[output].OutputOverflow += supplyRate - supplyUsedRate;
						}
					}
				}

				else if(node is ReadOnlySupplierNode sNode && fluids.Equals(sNode.SuppliedItem is Fluid))
				{
					if (!itemCounters.ContainsKey(sNode.SuppliedItem))
						itemCounters.Add(sNode.SuppliedItem, new ItemCounter(0, 0, 0, 0, 0, 0, 0));
					itemCounters[sNode.SuppliedItem].Input += sNode.ActualRate;
				}

				else if(node is ReadOnlyConsumerNode cNode && fluids.Equals(cNode.ConsumedItem is Fluid))
				{
					if (!itemCounters.ContainsKey(cNode.ConsumedItem))
						itemCounters.Add(cNode.ConsumedItem, new ItemCounter(0, 0, 0, 0, 0, 0, 0));
					itemCounters[cNode.ConsumedItem].Output += cNode.ActualRate;
				}
			}

			foreach (Item item in itemCounters.Keys.OrderBy(a => a.FriendlyName))
			{
				ListViewItem lvItem = new ListViewItem();
				if (item.Icon != null)
				{
					IconList.Images.Add(item.Icon);
					lvItem.ImageIndex = IconList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}

				lvItem.Text = item.FriendlyName;
				lvItem.Tag = item;
				lvItem.Name = item.Name; //key
				lvItem.BackColor = item.Available ? AvailableObjectColor : UnavailableObjectColor;
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].Input == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Input), Tag = itemCounters[item].Input });
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].InputUnlinked == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].InputUnlinked), Tag = itemCounters[item].InputUnlinked});
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].Output == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Output), Tag = itemCounters[item].Output });
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].OutputUnlinked == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].OutputUnlinked), Tag = itemCounters[item].OutputUnlinked });
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].OutputOverflow == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].OutputOverflow), Tag = itemCounters[item].OutputOverflow });
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].Production == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Production), Tag = itemCounters[item].Production });
				lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = itemCounters[item].Consumption == 0 ? "-" : GraphicsStuff.DoubleToString(itemCounters[item].Consumption), Tag = itemCounters[item].Consumption });
				lviList.Add(lvItem);
			}
		}

		private void LoadUnfilteredKeyNodesList(IEnumerable<ReadOnlyBaseNode> origin, List<ListViewItem> lviList)
		{
			foreach (ReadOnlyBaseNode node in origin)
			{
				ListViewItem lvItem = new ListViewItem();

				Bitmap icon;
				string nodeText;
				string nodeType;
				if (node is ReadOnlyConsumerNode cNode)
				{
					icon = cNode.ConsumedItem.Icon;
					nodeText = cNode.ConsumedItem.FriendlyName;
					nodeType = "Consumer";
				}
				else if (node is ReadOnlySupplierNode sNode)
				{
					icon = sNode.SuppliedItem.Icon;
					nodeText = sNode.SuppliedItem.FriendlyName;
					nodeType = "Supplier";
				}
				else if (node is ReadOnlyPassthroughNode pNode)
				{
					icon = pNode.PassthroughItem.Icon;
					nodeText = pNode.PassthroughItem.FriendlyName;
					nodeType = "Passthrough";
				}
				else if (node is ReadOnlyRecipeNode rNode)
				{
					icon = rNode.BaseRecipe.Icon;
					nodeText = rNode.BaseRecipe.FriendlyName;
					nodeType = "Recipe";
				}
				else
					continue;

				if (icon != null)
				{
					IconList.Images.Add(icon);
					lvItem.ImageIndex = IconList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}

				lvItem.Text = nodeType;
				lvItem.Tag = node;
				lvItem.Name = nodeText; //key
				lvItem.BackColor = AvailableObjectColor;
				lvItem.SubItems.Add(nodeText);
				lvItem.SubItems.Add(node.KeyNodeTitle);

				if(node is ReadOnlyRecipeNode rrNode)
				{
					lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = "-", Tag = (double)0 });
					lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = GraphicsStuff.DoubleToString(rrNode.ActualAssemblerCount), Tag = rrNode.ActualAssemblerCount });
				}
				else
				{
					lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = GraphicsStuff.DoubleToString(node.ActualRate), Tag = node.ActualRate });
					lvItem.SubItems.Add(new ListViewItem.ListViewSubItem() { Text = "-", Tag = (double)0 });
				}
				lviList.Add(lvItem);
			}
		}

		//-------------------------------------------------------------------------------------------------------Filter functions

		private void UpdateFilteredBuildingLists()
		{
			UpdateFilteredBuildingList(unfilteredAssemblerList, filteredAssemblerList, AssemblerListView);
			UpdateFilteredBuildingList(unfilteredMinerList, filteredMinerList, MinerListView);
			UpdateFilteredBuildingList(unfilteredPowerList, filteredPowerList, PowerListView);
			UpdateFilteredBuildingList(unfilteredBeaconList, filteredBeaconList, BeaconListView);
		}

		private void UpdateFilteredBuildingList(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner)
		{
			string filterString = BuildingsFilterTextBox.Text.ToLower();

			filteredList.Clear();

			foreach (ListViewItem lvItem in unfilteredList)
				if (string.IsNullOrEmpty(filterString) || ((DataObjectBase)lvItem.Tag).LFriendlyName.Contains(filterString))
					filteredList.Add(lvItem);

			owner.VirtualListSize = filteredList.Count;
			owner.Invalidate();
		}

		private void UpdateFilteredItemsLists()
		{
			UpdateFilteredItemsList(unfilteredItemsList, filteredItemsList, ItemsListView);
			UpdateFilteredItemsList(unfilteredFluidsList, filteredFluidsList, FluidsListView);
		}

		private void UpdateFilteredItemsList(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner)
		{
			string filterString = ItemsFilterTextBox.Text.ToLower();
			bool includeInputs = ItemFilterInputCheckBox.Checked;
			bool includeInputUnlinked = ItemFilterInputUnlinkedCheckBox.Checked;
			bool includeOutputs = ItemFilterOutputCheckBox.Checked;
			bool includeOutputsUnlinked = ItemFilterOutputUnlinkedCheckBox.Checked;
			bool includeOutputsOverflow = ItemFilterOutputOverproducedCheckBox.Checked;
			bool includeProduced = ItemFilterProductionCheckBox.Checked;
			bool includeConsumed = ItemFilterConsumptionCheckBox.Checked;

			filteredList.Clear();

			foreach (ListViewItem lvItem in unfilteredList)
			{
				if (string.IsNullOrEmpty(filterString) || ((Item)lvItem.Tag).LFriendlyName.Contains(filterString))
				{
					if ((includeInputs && lvItem.SubItems[1].Text != "-") ||
						(includeInputUnlinked && lvItem.SubItems[2].Text != "-") ||
						(includeOutputs && lvItem.SubItems[3].Text != "-") ||
						(includeOutputsUnlinked && lvItem.SubItems[4].Text != "-") ||
						(includeOutputsOverflow && lvItem.SubItems[5].Text != "-") ||
						(includeProduced && lvItem.SubItems[6].Text != "-") ||
						(includeConsumed && lvItem.SubItems[7].Text != "-"))
					{
						filteredList.Add(lvItem);
					}
				}
			}

			owner.VirtualListSize = filteredList.Count;
			owner.Invalidate();
		}

		private void UpdateFilteredKeyNodesList()
		{
			string filterString = KeyNodesFilterTextBox.Text.ToLower();
			bool includeSuppliers = SupplierNodeFilterCheckBox.Checked;
			bool includeConsumers = ConsumerNodeFilterCheckBox.Checked;
			bool includePassthrough = PassthroughNodeFilterCheckBox.Checked;
			bool includeRecipe = RecipeNodeFilterCheckBox.Checked;

			filteredKeyNodesList.Clear();

			foreach (ListViewItem lvItem in unfilteredKeyNodesList)
			{
				if (string.IsNullOrEmpty(filterString) || lvItem.Text.ToLower().Contains(filterString) || lvItem.SubItems[1].Text.ToLower().Contains(filterString) || lvItem.SubItems[2].Text.ToLower().Contains(filterString))
				{
					if ((includeSuppliers && (lvItem.Tag is ReadOnlySupplierNode)) ||
						(includeConsumers && (lvItem.Tag is ReadOnlyConsumerNode)) ||
						(includePassthrough && (lvItem.Tag is ReadOnlyPassthroughNode)) ||
						(includeRecipe && (lvItem.Tag is ReadOnlyRecipeNode)))
					{
						filteredKeyNodesList.Add(lvItem);
					}
				}
			}

			KeyNodesListView.VirtualListSize = filteredKeyNodesList.Count;
			KeyNodesListView.Invalidate();
		}

		//-------------------------------------------------------------------------------------------------------Virtual item retrieval for all list views

		private void AssemblerListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredAssemblerList[e.ItemIndex]; }
		private void MinerListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredMinerList[e.ItemIndex]; }
		private void PowerListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredPowerList[e.ItemIndex]; }
		private void BeaconListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredBeaconList[e.ItemIndex]; }
		private void ItemsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredItemsList[e.ItemIndex]; }
		private void FluidsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredFluidsList[e.ItemIndex]; }
		private void KeyNodesListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredKeyNodesList[e.ItemIndex]; }

		//-------------------------------------------------------------------------------------------------------Filter changed events

		private void BuildingsFilterTextBox_TextChanged(object sender, EventArgs e) { UpdateFilteredBuildingLists(); }

		private void ItemsFilterTextBox_TextChanged(object sender, EventArgs e) { UpdateFilteredItemsLists(); }
		private void ItemFilterCheckBox_CheckedChanged(object sender, EventArgs e) { UpdateFilteredItemsLists(); }

		private void KeyNodesFilterTextBox_TextChanged(object sender, EventArgs e) { UpdateFilteredKeyNodesList(); }
		private void KeyNodesFilterCheckBox_CheckedChanged(object sender, EventArgs e) { UpdateFilteredKeyNodesList(); }

		//-------------------------------------------------------------------------------------------------------Column clicked events

		private void AssemblerListView_ColumnClick(object sender, ColumnClickEventArgs e) { BuildingListView_ColumnSort(unfilteredAssemblerList, filteredAssemblerList, AssemblerListView, e.Column); }
		private void MinerListView_ColumnClick(object sender, ColumnClickEventArgs e) { BuildingListView_ColumnSort(unfilteredMinerList, filteredMinerList, MinerListView, e.Column); }
		private void PowerListView_ColumnClick(object sender, ColumnClickEventArgs e) { BuildingListView_ColumnSort(unfilteredPowerList, filteredPowerList, PowerListView, e.Column); }
		private void BeaconListView_ColumnClick(object sender, ColumnClickEventArgs e) { BuildingListView_ColumnSort(unfilteredBeaconList, filteredBeaconList, BeaconListView, e.Column); }

		private void BuildingListView_ColumnSort(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner, int column)
		{
			int reverseSortLamda = (lastSortOrder[owner] == column + 1) ? -1 : 1; //last sort was this very column -> this is now a reverse sort
			lastSortOrder[owner] = reverseSortLamda * (column + 1);

			unfilteredList.Sort((a, b) =>
			{
				if (column == 0)
					return reverseSortLamda * -double.Parse(a.Text).CompareTo(double.Parse(b.Text));
				else if (column == 1)
					return reverseSortLamda * a.SubItems[1].Text.ToLower().CompareTo(b.SubItems[1].Text.ToLower());
				else
					return reverseSortLamda * -((double)a.SubItems[column].Tag).CompareTo((double)b.SubItems[column].Tag);
			});

			UpdateFilteredBuildingList(unfilteredList, filteredList, owner);
			owner.Invalidate();
		}

		private void ItemsListView_ColumnClick(object sender, ColumnClickEventArgs e) { ItemListView_ColumnSort(unfilteredItemsList, filteredItemsList, ItemsListView, e.Column); }
		private void FluidsListView_ColumnClick(object sender, ColumnClickEventArgs e) { ItemListView_ColumnSort(unfilteredFluidsList, filteredFluidsList, FluidsListView, e.Column); }

		private void ItemListView_ColumnSort(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner, int column)
		{
			int reverseSortLamda = (lastSortOrder[owner] == column + 1) ? -1 : 1; //last sort was this very column -> this is now a reverse sort
			lastSortOrder[owner] = reverseSortLamda * (column + 1);

			unfilteredList.Sort((a, b) =>
			{
				if (column == 0)
					return reverseSortLamda * a.SubItems[0].Text.ToLower().CompareTo(b.SubItems[0].Text.ToLower());
				else
					return reverseSortLamda * -((double)a.SubItems[column].Tag).CompareTo((double)b.SubItems[column].Tag);
			});

			UpdateFilteredItemsList(unfilteredList, filteredList, owner);
			owner.Invalidate();
		}

		private void KeyNodesListView_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			int reverseSortLamda = (lastSortOrder[KeyNodesListView] == e.Column + 1) ? -1 : 1; //last sort was this very column -> this is now a reverse sort
			lastSortOrder[KeyNodesListView] = reverseSortLamda * (e.Column + 1);

			unfilteredKeyNodesList.Sort((a, b) =>
			{
				if (e.Column < 3)
					return reverseSortLamda * a.SubItems[0].Text.ToLower().CompareTo(b.SubItems[0].Text.ToLower());
				else
					return reverseSortLamda * -((double)a.SubItems[e.Column].Tag).CompareTo((double)b.SubItems[e.Column].Tag);
			});

			UpdateFilteredKeyNodesList();
			KeyNodesListView.Invalidate();
		}

		//-------------------------------------------------------------------------------------------------------Export CSV functions

		private void BuildingsExportButton_Click(object sender, EventArgs e)
		{
			ExportCSV(
				new List<ListViewItem>[] { filteredAssemblerList, filteredMinerList, filteredPowerList, filteredBeaconList },
				new string[][] { 
					new string[] { "#", "Assembler", "Electrical power consumed by assemblers (in W)", "Electrical power consumed by beacons (in W)" }, 
					new string[] { "#", "Miner", "Electrical power consumed by assemblers (in W)", "Electrical power consumed by beacons (in W)" }, 
					new string[] { "#", "Power Building", "Electrical power generated (in W)", "Electrical power consumed (in W)" }, 
					new string[] { "#", "Beacon", "Electrical power consumed by beacons (in W)" }
				});
		}

		private void ItemsExportButton_Click(object sender, EventArgs e)
		{
			ExportCSV(
				new List<ListViewItem>[] { filteredItemsList, filteredFluidsList },
				new string[][]
				{
					new string[] {"Item", "Input (per "+rateString+")", "Input through un-linked recipe ingredients (per "+rateString+")", "Output (per " + rateString + ")", "Output through un-linked recipe products (per " + rateString + ")", "Output through overproduction (per " + rateString + ")", "Produced by recipe nodes (per " + rateString + ")", "Consumed by recipe nodes (per " + rateString + ")" },
					new string[] {"Fluid", "Input (per "+rateString+")", "Input through un-linked recipe ingredients (per "+rateString+")", "Output (per " + rateString + ")", "Output through un-linked recipe products (per " + rateString + ")", "Output through overproduction (per " + rateString + ")", "Produced by recipe nodes (per " + rateString + ")", "Consumed by recipe nodes (per " + rateString + ")" }
				});
		}

		private void keyNodesExportButton_Click(object sender, EventArgs e)
		{
			ExportCSV(
				new List<ListViewItem>[] { filteredKeyNodesList },
				new string[][]
				{
					new string[] {"Node Type", "Node Details (item / recipe name)", "Node Title", "Throughput (for non-recipe nodes) (per " + rateString + ")", "Building Count (for recipe nodes)" }
				});
		}

		private void ExportCSV(List<ListViewItem>[] inputList, string[][] columnNames)
		{
			using (SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.AddExtension = true;
				dialog.Filter = "CSV (*.csv)|*.csv";
				dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Exported CSVs");
				if (!Directory.Exists(dialog.InitialDirectory))
					Directory.CreateDirectory(dialog.InitialDirectory);
				dialog.FileName = "foreman data.csv";
				dialog.ValidateNames = true;
				dialog.OverwritePrompt = true;
				var result = dialog.ShowDialog();

				if (result == DialogResult.OK)
				{
					List<string[]> csvLines = new List<string[]>();

					for(int i = 0; i < inputList.Length; i++)
					{
						csvLines.Add(columnNames[i]);
						foreach (ListViewItem lvi in inputList[i])
						{
							string[] cLine = new string[columnNames[i].Length];
							for (int j = 0; j < cLine.Length; j++)
								cLine[j] = (lvi.SubItems[j].Tag?? lvi.SubItems[j].Text).ToString().Replace(",", "").Replace("\n", "; ").Replace("\t", "");
							csvLines.Add(cLine);
						}
						csvLines.Add(new string[] { "" });
					}
					if (csvLines.Count > 0)
						csvLines.RemoveAt(csvLines.Count - 1);

					//export to csv.
					StringBuilder csvBuilder = new StringBuilder();
					csvLines.ForEach(line => { csvBuilder.AppendLine(string.Join(",", line)); });
					File.WriteAllText(dialog.FileName, csvBuilder.ToString());
				}
			}
		}
	}
}
