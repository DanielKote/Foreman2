using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Foreman
{
	public enum NodeType { Supplier, Consumer, Passthrough, Recipe }
	public enum LinkType { Input, Output }

	public class NodeEventArgs : EventArgs
	{
		public ReadOnlyBaseNode node;
		public NodeEventArgs(ReadOnlyBaseNode node) { this.node = node; }
	}
	public class NodeLinkEventArgs : EventArgs
	{
		public ReadOnlyNodeLink nodeLink;
		public NodeLinkEventArgs(ReadOnlyNodeLink nodeLink) { this.nodeLink = nodeLink; }
	}

	[Serializable]
	public partial class ProductionGraph : ISerializable
	{
		public class NewNodeCollection
		{
			public List<ReadOnlyBaseNode> newNodes { get; private set; }
			public List<ReadOnlyNodeLink> newLinks { get; private set; }
			public NewNodeCollection() { newNodes = new List<ReadOnlyBaseNode>(); newLinks = new List<ReadOnlyNodeLink>(); }
		}

		public enum RateUnit { Per1Sec, Per1Min, Per5Min, Per10Min, Per30Min, Per1Hour };//, Per6Hour, Per12Hour, Per24Hour }
		public static readonly string[] RateUnitNames = new string[] { "1 sec", "1 min", "5 min", "10 min", "30 min", "1 hour" }; //, "6 hours", "12 hours", "24 hours" };
		private static readonly float[] RateMultiplier = new float[] { 1f, 60f, 300f, 600f, 1800f, 3600f }; //, 21600f, 43200f, 86400f };

		public RateUnit SelectedRateUnit { get; set; }
		public float GetRateMultipler() { return RateMultiplier[(int)SelectedRateUnit]; } //the amount of assemblers required will be multipled by the rate multipler when displaying.
		public string GetRateName() { return RateUnitNames[(int)SelectedRateUnit]; }

		public const double MaxSetFlow = 10000000000000; //10 trillion should be enough for pretty much everything with a generous helping of 'oh god thats way too much!'
		private const int XBorder = 200;
		private const int YBorder = 100;

		public bool PauseUpdates { get; set; }

		public AssemblerSelector AssemblerSelector { get; private set; }
		public ModuleSelector ModuleSelector { get; private set; }
		public FuelSelector FuelSelector { get; private set; }

		public IEnumerable<ReadOnlyBaseNode> Nodes { get { return nodes.Select(node => node.ReadOnlyNode); } }
		public IEnumerable<ReadOnlyNodeLink> NodeLinks { get { return nodeLinks.Select(link => link.ReadOnlyLink); } }
		public HashSet<int> SerializeNodeIdSet { get; set; } //if this isnt null then the serialized production graph will only contain these nodes (and links between them)

		public event EventHandler<NodeEventArgs> NodeAdded;
		public event EventHandler<NodeEventArgs> NodeDeleted;
		public event EventHandler<NodeLinkEventArgs> LinkAdded;
		public event EventHandler<NodeLinkEventArgs> LinkDeleted;
		public event EventHandler<EventArgs> NodeValuesUpdated;

		public event EventHandler<EventArgs> GraphOptionsChanged;

		public Rectangle Bounds
		{
			get
			{
				if (nodes.Count == 0)
					return new Rectangle(0, 0, 0, 0);

				int xMin = int.MaxValue;
				int yMin = int.MaxValue;
				int xMax = int.MinValue;
				int yMax = int.MinValue;
				foreach (BaseNode node in nodes)
				{
					xMin = Math.Min(xMin, node.Location.X);
					xMax = Math.Max(xMax, node.Location.X);
					yMin = Math.Min(yMin, node.Location.Y);
					yMax = Math.Max(yMax, node.Location.Y);
				}

				return new Rectangle(xMin - XBorder, yMin - YBorder, xMax - xMin + (2 * XBorder), yMax - yMin + (2 * YBorder));
			}
		}

		private HashSet<BaseNode> nodes;
		private HashSet<NodeLink> nodeLinks;
		private Dictionary<ReadOnlyBaseNode, BaseNode> roToNode;
		private Dictionary<ReadOnlyNodeLink, NodeLink> roToLink;
		private int lastNodeID;

		public ProductionGraph()
		{
			nodes = new HashSet<BaseNode>();
			nodeLinks = new HashSet<NodeLink>();
			roToNode = new Dictionary<ReadOnlyBaseNode, BaseNode>();
			roToLink = new Dictionary<ReadOnlyNodeLink, NodeLink>();
			lastNodeID = 0;

			AssemblerSelector = new AssemblerSelector();
			ModuleSelector = new ModuleSelector();
			FuelSelector = new FuelSelector();
		}

		public BaseNodeController RequestNodeController(ReadOnlyBaseNode node) { if(roToNode.ContainsKey(node)) return roToNode[node].Controller; return null; }

		public ReadOnlyConsumerNode CreateConsumerNode(Item item, Point location)
		{
			ConsumerNode node = new ConsumerNode(this, lastNodeID++, item);
			node.Location = location;
			nodes.Add(node);
			roToNode.Add(node.ReadOnlyNode, node);
			NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
			return (ReadOnlyConsumerNode)node.ReadOnlyNode;
		}

		public ReadOnlySupplierNode CreateSupplierNode(Item item, Point location)
		{
			SupplierNode node = new SupplierNode(this, lastNodeID++, item);
			node.Location = location;
			nodes.Add(node);
			roToNode.Add(node.ReadOnlyNode, node);
			NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
			return (ReadOnlySupplierNode)node.ReadOnlyNode;
		}

		public ReadOnlyPassthroughNode CreatePassthroughNode(Item item, Point location)
		{
			PassthroughNode node = new PassthroughNode(this, lastNodeID++, item);
			node.Location = location;
			nodes.Add(node);
			roToNode.Add(node.ReadOnlyNode, node);
			NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
			return (ReadOnlyPassthroughNode)node.ReadOnlyNode;
		}

		public ReadOnlyRecipeNode CreateRecipeNode(Recipe recipe, Point location) { return CreateRecipeNode(recipe, location, null); }
		private ReadOnlyRecipeNode CreateRecipeNode(Recipe recipe, Point location, Action<RecipeNode> nodeSetupAction) //node setup action is used to populate the node prior to informing everyone of its creation
		{
			RecipeNode node = new RecipeNode(this, lastNodeID++, recipe);
			node.Location = location;
			nodes.Add(node);
			roToNode.Add(node.ReadOnlyNode, node);
			nodeSetupAction?.Invoke(node);
			if(nodeSetupAction == null)
			{
				RecipeNodeController rnController = (RecipeNodeController)node.Controller;
				rnController.AutoSetAssembler();
				rnController.AutoSetAssemblerModules();
			}
			NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
			return (ReadOnlyRecipeNode)node.ReadOnlyNode;
		}

		public ReadOnlyNodeLink CreateLink(ReadOnlyBaseNode supplier, ReadOnlyBaseNode consumer, Item item)
		{
			if (!roToNode.ContainsKey(supplier) || !roToNode.ContainsKey(consumer) || !supplier.Outputs.Contains(item) || !consumer.Inputs.Contains(item))
				Trace.Fail(string.Format("Node link creation called with invalid parameters! consumer:{0}. supplier:{1}. item:{2}.", consumer.ToString(), supplier.ToString(), item.ToString()));
			if (supplier.OutputLinks.Any(l => l.Item == item && l.Consumer == consumer)) //check for an already existing connection
				return null;

			BaseNode supplierNode = roToNode[supplier];
			BaseNode consumerNode = roToNode[consumer];

			NodeLink link = new NodeLink(this, supplierNode, consumerNode, item);
			supplierNode.OutputLinks.Add(link);
			consumerNode.InputLinks.Add(link);
			nodeLinks.Add(link);
			roToLink.Add(link.ReadOnlyLink, link);
			LinkAdded?.Invoke(this, new NodeLinkEventArgs(link.ReadOnlyLink));
			return link.ReadOnlyLink;
		}

		public void DeleteNode(ReadOnlyBaseNode node)
		{
			if (!roToNode.ContainsKey(node))
				Trace.Fail(string.Format("Node deletion called on a node ({0}) that isnt part of the graph!", node.ToString()));

			foreach (ReadOnlyNodeLink link in node.InputLinks.ToList())
				DeleteLink(link);
			foreach (ReadOnlyNodeLink link in node.OutputLinks.ToList())
				DeleteLink(link);

			nodes.Remove(roToNode[node]);
			roToNode.Remove(node);
			NodeDeleted?.Invoke(this, new NodeEventArgs(node));
		}

		public void DeleteNodes(IEnumerable<ReadOnlyBaseNode> nodes)
		{
			foreach (ReadOnlyBaseNode node in nodes)
				DeleteNode(node);
		}

		public void DeleteLink(ReadOnlyNodeLink link)
		{
			if (!roToLink.ContainsKey(link) || !roToNode.ContainsKey(link.Consumer) || !roToNode.ContainsKey(link.Supplier))
				Trace.Fail(string.Format("Link deletion called with a link ({0}) that isnt part of the graph, or whose node(s) ({1}), ({2}) is/are not part of the graph!", link.ToString(), link.Consumer.ToString(), link.Supplier.ToString()));

			NodeLink nodeLink = roToLink[link];
			nodeLink.ConsumerNode.InputLinks.Remove(nodeLink);
			nodeLink.SupplierNode.OutputLinks.Remove(nodeLink);

			nodeLinks.Remove(nodeLink);
			roToLink.Remove(link);
			LinkDeleted?.Invoke(this, new NodeLinkEventArgs(link));
		}

		public void ClearGraph(bool thorough = false)
		{
			if(thorough)
				foreach (BaseNode node in nodes)
					DeleteNode(node.ReadOnlyNode);

			nodes.Clear();
			nodeLinks.Clear();
			roToNode.Clear();
			roToLink.Clear();
			SerializeNodeIdSet = null;
			lastNodeID = 0;
		}

		public void UpdateNodeStates()
		{
			foreach (BaseNode node in nodes)
				node.UpdateState();
		}

		public IEnumerable<ReadOnlyBaseNode> GetSuppliers(Item item)
		{
			foreach (ReadOnlyBaseNode node in Nodes)
				if (node.Outputs.Contains(item))
					yield return node;
		}

		public IEnumerable<ReadOnlyBaseNode> GetConsumers(Item item)
		{
			foreach (ReadOnlyBaseNode node in Nodes)
				if (node.Inputs.Contains(item))
					yield return node;
		}

		public IEnumerable<IEnumerable<ReadOnlyBaseNode>> GetConnectedComponents() //used to break the graph into groups (in case there are multiple disconnected groups) for simpler solving
		{
			HashSet<ReadOnlyBaseNode> unvisitedNodes = new HashSet<ReadOnlyBaseNode>(Nodes);

			List<HashSet<ReadOnlyBaseNode>> connectedComponents = new List<HashSet<ReadOnlyBaseNode>>();

			while (unvisitedNodes.Any())
			{
				connectedComponents.Add(new HashSet<ReadOnlyBaseNode>());
				HashSet<ReadOnlyBaseNode> toVisitNext = new HashSet<ReadOnlyBaseNode>();
				toVisitNext.Add(unvisitedNodes.First());

				while (toVisitNext.Any())
				{
					ReadOnlyBaseNode currentNode = toVisitNext.First();

					foreach (ReadOnlyNodeLink link in currentNode.InputLinks)
						if (unvisitedNodes.Contains(link.Supplier))
							toVisitNext.Add(link.Supplier);

					foreach (ReadOnlyNodeLink link in currentNode.OutputLinks)
						if (unvisitedNodes.Contains(link.Consumer))
							toVisitNext.Add(link.Consumer);

					connectedComponents.Last().Add(currentNode);
					toVisitNext.Remove(currentNode);
					unvisitedNodes.Remove(currentNode);
				}
			}

			return connectedComponents;
		}

		public void UpdateNodeValues()
		{
			if (!PauseUpdates)
			{
				try { OptimizeGraphNodeValues(); }
				catch (OverflowException) { }
				//If the numbers here are so big they're causing an overflow, there's not much I can do about it. It's already pretty clear in the UI that the values are unusable.
				//At least this way it doesn't crash...
			}
			NodeValuesUpdated?.Invoke(this, EventArgs.Empty); //called even if no changes have been made in order to re-draw the graph (since something required a node value update - link deletion? node addition? whatever)
		}

		public NewNodeCollection InsertNodesFromJson(DataCache cache, JToken json) //cache is necessary since we will possibly be adding to mssing items/recipes
		{
			NewNodeCollection newNodeCollection = new NewNodeCollection();
			Dictionary<int, ReadOnlyBaseNode> oldNodeIndices = new Dictionary<int, ReadOnlyBaseNode>(); //the links between the node index (as imported) and the newly created node (which will now have a different index). Used to link up nodes

			//check compliance on all items, assemblers, modules, beacons, and recipes (data-cache will take care of it) - this means add in any missing objects and handle multi-name recipes (there can be multiple versions of a missing recipe, each with identical names)
			cache.ProcessImportedItemsSet(json["IncludedItems"].Select(t => (string)t));
			cache.ProcessImportedAssemblersSet(json["IncludedAssemblers"].Select(t => (string)t));
			cache.ProcessImportedModulesSet(json["IncludedModules"].Select(t => (string)t));
			cache.ProcessImportedBeaconsSet(json["IncludedBeacons"].Select(t => (string)t));
			Dictionary<long, Recipe> recipeLinks = cache.ProcessImportedRecipesSet(RecipeShort.GetSetFromJson(json["IncludedRecipes"]));

			//add in all the graph nodes
			foreach (JToken nodeJToken in json["Nodes"].ToList())
			{
				BaseNode newNode = null;
				string[] locationString = ((string)nodeJToken["Location"]).Split(',');
				Point location = new Point(int.Parse(locationString[0]), int.Parse(locationString[1]));
				string itemName; //just an early define

				try
				{
					switch ((NodeType)(int)nodeJToken["NodeType"])
					{
						case NodeType.Consumer:
							itemName = (string)nodeJToken["Item"];
							if (cache.Items.ContainsKey(itemName))
								newNode = roToNode[CreateConsumerNode(cache.Items[itemName], location)];
							else
								newNode = roToNode[CreateConsumerNode(cache.MissingItems[itemName], location)];
							newNodeCollection.newNodes.Add(newNode.ReadOnlyNode);
							break;
						case NodeType.Supplier:
							itemName = (string)nodeJToken["Item"];
							if (cache.Items.ContainsKey(itemName))
								newNode = roToNode[CreateSupplierNode(cache.Items[itemName], location)];
							else
								newNode = roToNode[CreateSupplierNode(cache.MissingItems[itemName], location)];
							newNodeCollection.newNodes.Add(newNode.ReadOnlyNode);
							break;
						case NodeType.Passthrough:
							itemName = (string)nodeJToken["Item"];
							if (cache.Items.ContainsKey(itemName))
								newNode = roToNode[CreatePassthroughNode(cache.Items[itemName], location)];
							else
								newNode = roToNode[CreatePassthroughNode(cache.MissingItems[itemName], location)];
							newNodeCollection.newNodes.Add(newNode.ReadOnlyNode);
							break;
						case NodeType.Recipe:
							long recipeID = (long)nodeJToken["RecipeID"];
							newNode = roToNode[CreateRecipeNode(recipeLinks[recipeID], location, (rNode) =>
							{
								newNodeCollection.newNodes.Add(rNode.ReadOnlyNode);
								RecipeNodeController rNodeController = (RecipeNodeController)rNode.Controller;

								rNode.NeighbourCount = (double)nodeJToken["Neighbours"];

								string assemblerName = (string)nodeJToken["Assembler"];
								if (cache.Assemblers.ContainsKey(assemblerName))
									rNodeController.SetAssembler(cache.Assemblers[assemblerName]);
								else
									rNodeController.SetAssembler(cache.MissingAssemblers[assemblerName]);

								foreach (string moduleName in nodeJToken["AssemblerModules"].Select(t => (string)t).ToList())
								{
									if (cache.Modules.ContainsKey(moduleName))
										rNodeController.AddAssemblerModule(cache.Modules[moduleName]);
									else
										rNodeController.AddAssemblerModule(cache.MissingModules[moduleName]);
								}

								if (nodeJToken["Fuel"] != null)
								{
									if (cache.Items.ContainsKey((string)nodeJToken["Fuel"]))
										rNodeController.SetFuel(cache.Items[(string)nodeJToken["Fuel"]]);
									else
										rNodeController.SetFuel(cache.MissingItems[(string)nodeJToken["Fuel"]]);
								}
								else if (rNode.SelectedAssembler.IsBurner) //and fuel is null :/
									rNodeController.SetFuel(FuelSelector.GetFuel(rNode.SelectedAssembler));

								if (nodeJToken["Burnt"] != null)
								{
									Item burntItem;
									if (cache.Items.ContainsKey((string)nodeJToken["Burnt"]))
										burntItem = cache.Items[(string)nodeJToken["Burnt"]];
									else
										burntItem = cache.MissingItems[(string)nodeJToken["Burnt"]];
									if (rNode.FuelRemains != burntItem)
										rNode.SetBurntOverride(burntItem);
								}

								if (nodeJToken["Beacon"] != null)
								{
									string beaconName = (string)nodeJToken["Beacon"];
									if (cache.Beacons.ContainsKey(beaconName))
										rNodeController.SetBeacon(cache.Beacons[beaconName]);
									else
										rNodeController.SetBeacon(cache.MissingBeacons[beaconName]);

									foreach (string moduleName in nodeJToken["BeaconModules"].Select(t => (string)t).ToList())
									{
										if (cache.Modules.ContainsKey(moduleName))
											rNodeController.AddBeaconModule(cache.Modules[moduleName]);
										else
											rNodeController.AddBeaconModule(cache.MissingModules[moduleName]);
									}

									rNode.BeaconCount = (double)nodeJToken["BeaconCount"];
									rNode.BeaconsPerAssembler = (double)nodeJToken["BeaconsPerAssembler"];
									rNode.BeaconsConst = (double)nodeJToken["BeaconsConst"];
								}
							})];
							break;
						default:
							throw new Exception();
					}

					newNode.RateType = (RateType)(int)nodeJToken["RateType"];
					if (newNode.RateType == RateType.Manual)
					{
						if (newNode is RecipeNode rnewNode)
							rnewNode.DesiredAssemblerCount = (double)nodeJToken["DesiredAssemblers"];
						else
							newNode.DesiredRatePerSec = (double)nodeJToken["DesiredRate"];
					}

					oldNodeIndices.Add((int)nodeJToken["NodeID"], newNode.ReadOnlyNode);
				}
				catch //there was something wrong with the json (probably someone edited it by hand and it didnt link properly). Delete all added nodes and return empty
				{
					DeleteNodes(newNodeCollection.newNodes);
					newNodeCollection.newNodes.Clear();
					return newNodeCollection;
				}
			}

			//link the new nodes
			foreach (JToken nodeLinkJToken in json["NodeLinks"].ToList())
			{
				ReadOnlyBaseNode supplier = oldNodeIndices[(int)nodeLinkJToken["SupplierID"]];
				ReadOnlyBaseNode consumer = oldNodeIndices[(int)nodeLinkJToken["ConsumerID"]];
				Item item;

				string itemName = (string)nodeLinkJToken["Item"];
				if (cache.Items.ContainsKey(itemName))
					item = cache.Items[itemName];
				else
					item = cache.MissingItems[itemName];

				if (LinkChecker.IsPossibleConnection(item, supplier, consumer)) //not necessary to test if connection is valid. It must be valid based on json
					newNodeCollection.newLinks.Add(CreateLink(supplier, consumer, item));
			}
			return newNodeCollection;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//collect the set of nodes and links to be saved (either entire set, or only that which is bound by the specified serialized node list)
			HashSet<BaseNode> includedNodes = nodes;
			HashSet<NodeLink> includedLinks = nodeLinks;
			if (SerializeNodeIdSet != null)
			{
				includedNodes = new HashSet<BaseNode>(nodes.Where(node => SerializeNodeIdSet.Contains(node.NodeID)));
				includedLinks = new HashSet<NodeLink>();
				foreach (NodeLink link in nodeLinks)
					if (includedNodes.Contains(link.ConsumerNode) && includedNodes.Contains(link.SupplierNode))
						includedLinks.Add(link);
			}

			//prepare list of items/assemblers/modules/beacons/recipes that are part of the saved set. Recipes have to include a missing component due to the possibility of different recipes having same name (ex: regular iron.recipe, missing iron.recipe, missing iron.recipe #2)
			HashSet<string> includedItems = new HashSet<string>();
			HashSet<string> includedAssemblers = new HashSet<string>();
			HashSet<string> includedModules = new HashSet<string>();
			HashSet<string> includedBeacons = new HashSet<string>();

			HashSet<Recipe> includedRecipes = new HashSet<Recipe>();
			HashSet<Recipe> includedMissingRecipes = new HashSet<Recipe>(new RecipeNaInPrComparer()); //compares by name, ingredients, and products (not amounts, just items)

			foreach (BaseNode node in includedNodes)
			{
				if (node is RecipeNode rnode)
				{
					if (rnode.BaseRecipe.IsMissing)
						includedMissingRecipes.Add(rnode.BaseRecipe);
					else
						includedRecipes.Add(rnode.BaseRecipe);

					if (rnode.SelectedAssembler != null)
						includedAssemblers.Add(rnode.SelectedAssembler.Name);
					if (rnode.SelectedBeacon != null)
						includedBeacons.Add(rnode.SelectedBeacon.Name);
					foreach (Module module in rnode.AssemblerModules)
						includedModules.Add(module.Name);
					foreach (Module module in rnode.BeaconModules)
						includedModules.Add(module.Name);
				}

				//these will process all inputs/outputs -> so fuel/burnt items are included automatically!
				foreach (Item input in node.Inputs)
					includedItems.Add(input.Name);
				foreach (Item output in node.Outputs)
					includedItems.Add(output.Name);
			}
			List<RecipeShort> includedRecipeShorts = includedRecipes.Select(recipe => new RecipeShort(recipe)).ToList();
			includedRecipeShorts.AddRange(includedMissingRecipes.Select(recipe => new RecipeShort(recipe))); //add the missing after the regular, since when we compare saves to preset we will only check 1st recipe of its name (the non-missing kind then)

			//serialize
			info.AddValue("IncludedItems", includedItems);
			info.AddValue("IncludedRecipes", includedRecipeShorts);
			info.AddValue("IncludedAssemblers", includedAssemblers);
			info.AddValue("IncludedModules", includedModules);
			info.AddValue("IncludedBeacons", includedBeacons);

			info.AddValue("Nodes", includedNodes);
			info.AddValue("NodeLinks", includedLinks);
		}
	}
}