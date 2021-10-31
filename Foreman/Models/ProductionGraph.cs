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
		public BaseNode node;
		public NodeEventArgs(BaseNode node) { this.node = node; }
	}
	public class NodeLinkEventArgs : EventArgs
	{
		public NodeLink nodeLink;
		public NodeLinkEventArgs(NodeLink nodeLink) { this.nodeLink = nodeLink; }
	}

	[Serializable]
	public class ProductionGraph : ISerializable
	{
		public class NewNodeCollection
		{
			public List<BaseNode> newNodes { get; private set; }
			public List<NodeLink> newLinks { get; private set; }
			public NewNodeCollection() { newNodes = new List<BaseNode>(); newLinks = new List<NodeLink>(); }
		}

		public const double MaxSetFlow = 10000000000000; //10 trillion should be enough for pretty much everything with a generous helping of 'oh god thats way too much!'
		private const int XBorder = 200;
		private const int YBorder = 100;

		public bool PauseUpdates { get; set; }

		public AssemblerSelector AssemblerSelector { get; private set; }
		public ModuleSelector ModuleSelector { get; private set; }
		public FuelSelector FuelSelector { get; private set; }

		public IReadOnlyCollection<BaseNode> Nodes { get { return nodes; } }
		public IReadOnlyCollection<NodeLink> NodeLinks { get { return nodeLinks; } }
		public List<BaseNode> SerializeNodeList { get; set; } //if this isnt null then the serialized production graph will only contain these nodes (and links between them)

		public event EventHandler<NodeEventArgs> NodeAdded;
		public event EventHandler<NodeEventArgs> NodeDeleted;
		public event EventHandler<NodeLinkEventArgs> LinkAdded;
		public event EventHandler<NodeLinkEventArgs> LinkDeleted;
		public event EventHandler<EventArgs> NodeValuesUpdated;

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

		private HashSet<BaseNodePrototype> nodes;
		private HashSet<NodeLinkPrototype> nodeLinks;
		private int lastNodeID;

		public ProductionGraph()
		{
			nodes = new HashSet<BaseNodePrototype>();
			nodeLinks = new HashSet<NodeLinkPrototype>();
			lastNodeID = 0;

			AssemblerSelector = new AssemblerSelector();
			ModuleSelector = new ModuleSelector();
			FuelSelector = new FuelSelector();
		}

		public ConsumerNode CreateConsumerNode(Item item, Point location)
		{
			ConsumerNodePrototype node = new ConsumerNodePrototype(this, lastNodeID++, item);
			node.Location = location;
			nodes.Add(node);
			NodeAdded?.Invoke(this, new NodeEventArgs(node));
			return node;
		}

		public SupplierNode CreateSupplierNode(Item item, Point location)
		{
			SupplierNodePrototype node = new SupplierNodePrototype(this, lastNodeID++, item);
			node.Location = location;
			nodes.Add(node);
			NodeAdded?.Invoke(this, new NodeEventArgs(node));
			return node;
		}

		public PassthroughNode CreatePassthroughNode(Item item, Point location)
		{
			PassthroughNodePrototype node = new PassthroughNodePrototype(this, lastNodeID++, item);
			node.Location = location;
			nodes.Add(node);
			NodeAdded?.Invoke(this, new NodeEventArgs(node));
			return node;
		}

		public RecipeNode CreateRecipeNode(Recipe recipe, Point location) { return CreateRecipeNode(recipe, location, null); }
		private RecipeNode CreateRecipeNode(Recipe recipe, Point location, Action<RecipeNodePrototype> nodeSetupAction) //node setup action is used to populate the node prior to informing everyone of its creation
		{
			RecipeNodePrototype node = new RecipeNodePrototype(this, lastNodeID++, recipe, nodeSetupAction == null);
			node.Location = location;
			nodes.Add(node);
			nodeSetupAction?.Invoke(node);
			NodeAdded?.Invoke(this, new NodeEventArgs(node));
			return node;
		}

		public NodeLink CreateLink(BaseNode supplier, BaseNode consumer, Item item)
		{
			BaseNodePrototype supplierP = (BaseNodePrototype)supplier;
			BaseNodePrototype consumerP = (BaseNodePrototype)consumer;
			if (supplier.OutputLinks.Any(l => l.Item == item && l.Consumer == consumer)) //check for an already existing connection
				return null;

			NodeLinkPrototype link = new NodeLinkPrototype(this, supplierP, consumerP, item);
			supplierP.outputLinks.Add(link);
			consumerP.inputLinks.Add(link);
			nodeLinks.Add(link);
			LinkAdded?.Invoke(this, new NodeLinkEventArgs(link));
			return link;
		}

		public void DeleteNode(BaseNode node)
		{
			foreach (NodeLinkPrototype link in node.InputLinks.ToList())
				DeleteLink(link);
			foreach (NodeLinkPrototype link in node.OutputLinks.ToList())
				DeleteLink(link);

			nodes.Remove((BaseNodePrototype)node);
			NodeDeleted?.Invoke(this, new NodeEventArgs(node));
		}

		public void DeleteNodes(IEnumerable<BaseNode> nodes)
		{
			foreach (BaseNode node in nodes)
				DeleteNode(node);
		}

		public void DeleteLink(NodeLink link)
		{
			NodeLinkPrototype linkP = (NodeLinkPrototype)link;
			linkP.consumer.inputLinks.Remove(linkP);
			linkP.supplier.outputLinks.Remove(linkP);
			nodeLinks.Remove(linkP);
			LinkDeleted?.Invoke(this, new NodeLinkEventArgs(link));
		}

		public void ClearGraph()
		{
			nodes.Clear();
			nodeLinks.Clear();
			SerializeNodeList = null;
			lastNodeID = 0;
		}

		public void UpdateNodeStates()
		{
			foreach (BaseNode node in nodes)
				node.UpdateState();
		}

		public IEnumerable<BaseNode> GetSuppliers(Item item)
		{
			foreach (BaseNode node in Nodes)
				if (node.Outputs.Contains(item))
					yield return node;
		}

		public IEnumerable<BaseNode> GetConsumers(Item item)
		{
			foreach (BaseNode node in Nodes)
				if (node.Inputs.Contains(item))
					yield return node;
		}

		public IEnumerable<IEnumerable<BaseNode>> GetConnectedComponents() //used to break the graph into groups (in case there are multiple disconnected groups) for simpler solving
		{
			HashSet<BaseNode> unvisitedNodes = new HashSet<BaseNode>(Nodes);

			List<HashSet<BaseNode>> connectedComponents = new List<HashSet<BaseNode>>();

			while (unvisitedNodes.Any())
			{
				connectedComponents.Add(new HashSet<BaseNode>());
				HashSet<BaseNode> toVisitNext = new HashSet<BaseNode>();
				toVisitNext.Add(unvisitedNodes.First());

				while (toVisitNext.Any())
				{
					BaseNode currentNode = toVisitNext.First();

					foreach (NodeLink link in currentNode.InputLinks)
						if (unvisitedNodes.Contains(link.Supplier))
							toVisitNext.Add(link.Supplier);

					foreach (NodeLink link in currentNode.OutputLinks)
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
				try { this.FindOptimalGraphToSatisfyFixedNodes(); }
				catch (OverflowException) { }
				//If the numbers here are so big they're causing an overflow, there's not much I can do about it. It's already pretty clear in the UI that the values are unusable.
				//At least this way it doesn't crash...
			}
			NodeValuesUpdated?.Invoke(this, EventArgs.Empty); //called even if no changes have been made in order to re-draw the graph (since something required a node value update - link deletion? node addition? whatever)
		}

		public NewNodeCollection InsertNodesFromJson(DataCache cache, JToken json) //cache is necessary since we will possibly be adding to mssing items/recipes
		{
			NewNodeCollection newNodeCollection = new NewNodeCollection();
			Dictionary<int, BaseNode> oldNodeIndices = new Dictionary<int, BaseNode>(); //the links between the node index (as imported) and the newly created node (which will now have a different index). Used to link up nodes

			//check compliance on all items, assemblers, modules, beacons, and recipes (data-cache will take care of it) - this means add in any missing objects and handle multi-name recipes (there can be multiple versions of a missing recipe, each with identical names)
			cache.ProcessImportedItemsSet(json["IncludedItems"].Select(t => (string)t));
			cache.ProcessImportedAssemblersSet(json["IncludedAssemblers"].Select(t => (string)t));
			cache.ProcessImportedModulesSet(json["IncludedModules"].Select(t => (string)t));
			cache.ProcessImportedBeaconsSet(json["IncludedBeacons"].Select(t => (string)t));
			Dictionary<long, Recipe> recipeLinks = cache.ProcessImportedRecipesSet(RecipeShort.GetSetFromJson(json["IncludedRecipes"]));

			//add in all the graph nodes
			foreach (JToken nodeJToken in json["Nodes"].ToList())
			{
				BaseNodePrototype newNode = null;
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
								newNode = (BaseNodePrototype)CreateConsumerNode(cache.Items[itemName], location);
							else
								newNode = (BaseNodePrototype)CreateConsumerNode(cache.MissingItems[itemName], location);
							newNodeCollection.newNodes.Add(newNode);
							break;
						case NodeType.Supplier:
							itemName = (string)nodeJToken["Item"];
							if (cache.Items.ContainsKey(itemName))
								newNode = (BaseNodePrototype)CreateSupplierNode(cache.Items[itemName], location);
							else
								newNode = (BaseNodePrototype)CreateSupplierNode(cache.MissingItems[itemName], location);
							newNodeCollection.newNodes.Add(newNode);
							break;
						case NodeType.Passthrough:
							itemName = (string)nodeJToken["Item"];
							if (cache.Items.ContainsKey(itemName))
								newNode = (BaseNodePrototype)CreatePassthroughNode(cache.Items[itemName], location);
							else
								newNode = (BaseNodePrototype)CreatePassthroughNode(cache.MissingItems[itemName], location);
							newNodeCollection.newNodes.Add(newNode);
							break;
						case NodeType.Recipe:
							long recipeID = (long)nodeJToken["RecipeID"];
							newNode = (RecipeNodePrototype)CreateRecipeNode(recipeLinks[recipeID], location, (rNode) => {
								newNodeCollection.newNodes.Add(rNode);

								rNode.NeighbourCount = (double)nodeJToken["Neighbours"];

								string assemblerName = (string)nodeJToken["Assembler"];
								if (cache.Assemblers.ContainsKey(assemblerName))
									rNode.SetAssembler(cache.Assemblers[assemblerName]);
								else
									rNode.SetAssembler(cache.MissingAssemblers[assemblerName]);

								foreach (string moduleName in nodeJToken["AssemblerModules"].Select(t => (string)t).ToList())
								{
									if (cache.Modules.ContainsKey(moduleName))
										rNode.AddAssemblerModule(cache.Modules[moduleName]);
									else
										rNode.AddAssemblerModule(cache.MissingModules[moduleName]);
								}

								if (nodeJToken["Fuel"] != null)
								{
									if (cache.Items.ContainsKey((string)nodeJToken["Fuel"]))
										rNode.SetFuel(cache.Items[(string)nodeJToken["Fuel"]]);
									else
										rNode.SetFuel(cache.MissingItems[(string)nodeJToken["Fuel"]]);
								}
								else if (rNode.SelectedAssembler.IsBurner) //and fuel is null :/
									rNode.SetFuel(FuelSelector.GetFuel(rNode.SelectedAssembler));

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
										rNode.SetBeacon(cache.Beacons[beaconName]);
									else
										rNode.SetBeacon(cache.MissingBeacons[beaconName]);

									foreach (string moduleName in nodeJToken["BeaconModules"].Select(t => (string)t).ToList())
									{
										if (cache.Modules.ContainsKey(moduleName))
											rNode.AddBeaconModule(cache.Modules[moduleName]);
										else
											rNode.AddBeaconModule(cache.MissingModules[moduleName]);
									}

									rNode.BeaconCount = (double)nodeJToken["BeaconCount"];
									rNode.BeaconsPerAssembler = (double)nodeJToken["BeaconsPerAssembler"];
									rNode.BeaconsConst = (double)nodeJToken["BeaconsConst"];
								}
							});
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
							newNode.DesiredRate = (double)nodeJToken["DesiredRate"];
					}

					oldNodeIndices.Add((int)nodeJToken["NodeID"], newNode);
				}
				catch //there was something wrong with the json (probably someone edited it by hand and it didnt link properly). Delete all added nodes and return empty
				{
					foreach (BaseNodePrototype node in newNodeCollection.newNodes)
						node.Delete();
					newNodeCollection.newNodes.Clear();
					return newNodeCollection;
				}
			}

			//link the new nodes
			foreach (JToken nodeLinkJToken in json["NodeLinks"].ToList())
			{
				BaseNode supplier = oldNodeIndices[(int)nodeLinkJToken["SupplierID"]];
				BaseNode consumer = oldNodeIndices[(int)nodeLinkJToken["ConsumerID"]];
				Item item;

				string itemName = (string)nodeLinkJToken["Item"];
				if (cache.Items.ContainsKey(itemName))
					item = cache.Items[itemName];
				else
					item = cache.MissingItems[itemName];

				if(LinkChecker.IsPossibleConnection(item, supplier, consumer)) //not necessary to test if connection is valid. It must be valid based on json
					newNodeCollection.newLinks.Add(CreateLink(supplier, consumer, item));
			}
			return newNodeCollection;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//collect the set of nodes and links to be saved (either entire set, or only that which is bound by the specified serialized node list)
			IEnumerable<BaseNode> includedNodes = nodes;
			IEnumerable<NodeLink> includedLinks = nodeLinks;
			if (SerializeNodeList != null)
			{
				HashSet<BaseNode> nodeSet = new HashSet<BaseNode>();
				foreach (BaseNode node in SerializeNodeList.Where(n => nodes.Contains(n)))
					nodeSet.Add(node);
				HashSet<NodeLink> linkSet = new HashSet<NodeLink>();
				foreach (NodeLink link in nodeLinks)
					if (nodeSet.Contains(link.Consumer) && nodeSet.Contains(link.Supplier))
						linkSet.Add(link);
				includedNodes = nodeSet;
				includedLinks = linkSet;
			}

			//prepare list of items/assemblers/modules/beacons/recipes that are part of the saved set. Recipes have to include a missing component due to the possibility of different recipes having same name (ex: regular iron.recipe, missing iron.recipe, missing iron.recipe #2)
			HashSet<string> includedItems = new HashSet<string>();
			HashSet<string> includedAssemblers = new HashSet<string>();
			HashSet<string> includedModules = new HashSet<string>();
			HashSet<string> includedBeacons = new HashSet<string>();
			
			HashSet<Recipe> includedRecipes = new HashSet<Recipe>();
			HashSet<Recipe> includedMissingRecipes = new HashSet<Recipe>(new RecipeNaInPrComparer()); //compares by name, ingredients, and products (not amounts, just items)


			foreach (BaseNode node in Nodes)
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