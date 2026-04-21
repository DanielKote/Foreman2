using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman {
    public enum NodeType {
        Supplier,
        Consumer,
        Passthrough,
        Recipe,
        Spoil,
        Plant
    }

    public enum LinkType {
        Input,
        Output
    }

    public class NodeEventArgs(ReadOnlyBaseNode node) : EventArgs {
        public ReadOnlyBaseNode Node = node;
    }

    public class NodeLinkEventArgs(ReadOnlyNodeLink nodeLink) : EventArgs {
        public ReadOnlyNodeLink NodeLink = nodeLink;
    }

    [Serializable]
    public partial class ProductionGraph : ISerializable {
        public class NewNodeCollection {
            public List<ReadOnlyBaseNode> NewNodes { get; private set; } = [];
            public List<ReadOnlyNodeLink> NewLinks { get; private set; } = [];
        }

        //public DataCache DCache { get; private set; }

        public enum RateUnit {
            Per1Sec,
            Per1Min,
            Per5Min,
            Per10Min,
            Per30Min,
            Per1Hour
        }; //, Per6Hour, Per12Hour, Per24Hour }

        public static readonly string[]
            RateUnitNames = ["1 sec", "1 min", "5 min", "10 min", "30 min", "1 hour"]; //, "6 hours", "12 hours", "24 hours" };

        private static readonly float[] RateMultiplier = [1f, 60f, 300f, 600f, 1800f, 3600f]; //, 21600f, 43200f, 86400f };

        public RateUnit SelectedRateUnit { get; set; }

        // the amount of assemblers required will be multiplied by the rate multiplier when displaying.
        public float GetRateMultiplier() {
            return RateMultiplier[(int) SelectedRateUnit];
        }

        public string GetRateName() {
            return RateUnitNames[(int) SelectedRateUnit];
        }

        public NodeDirection DefaultNodeDirection { get; set; } = NodeDirection.Up;
        public bool DefaultToSimplePassthroughNodes { get; set; }

        // 10 million (per second) item flow should be enough for pretty much everything with a generous helping of 'oh god that's way too much!'
        public const double MaxSetFlow = 1e7;

        // 1 million factories should be good enough as well.
        // NOTE: the auto values can go higher, you just cant set more than 1 million on the manual setting.
        public const double MaxFactories = 1e6;

        // 10 million tiles for planting should be good enough
        public const double MaxTiles = 1e7;
        // 1 million inventory slots for spoiling should be good enough
        public const double MaxInventorySlots = 1e6;
        private const int XBorder = 200;
        private const int YBorder = 200;

        public bool PauseUpdates { get; set; }

        // if true, the solver will add a 'pull' for output nodes to prioritize them over lowering factory count.
        // WARNING: this can lead to '0' solutions if there is any production path that can go to infinity
        // (aka: ensure enough nodes are constrained!)
        public bool PullOutputNodes { get; set; }

        public double PullOutputNodesPower { get; set; } = 10;

        // this is the multiplier of the factory cost function for low priority nodes.
        // aka: low priority recipes will be picked if the alternative involves this much more factories
        // (10,000 is a nice value here)
        public double LowPriorityPower { get; set; } =
            1e5;

        public bool EnableExtraProductivityForNonMiners { get; set; }

        public AssemblerSelector AssemblerSelector { get; private set; } = new();
        public ModuleSelector ModuleSelector { get; private set; } = new();
        public FuelSelector FuelSelector { get; private set; } = new();

        public IEnumerable<ReadOnlyBaseNode> Nodes {
            get { return _nodes.Select(node => node.ReadOnlyNode); }
        }

        public IEnumerable<ReadOnlyNodeLink> NodeLinks {
            get { return _nodeLinks.Select(link => link.ReadOnlyLink); }
        }

        // if this isn't null then the serialized production graph will only contain these nodes (and links between them)
        public HashSet<int> SerializeNodeIdSet { get; set; }

        // editing this value will require the entire graph to be updated as any recipe nodes on
        // it will possibly change the number of products and possibly cause a cascade of removed links
        private uint _maxQualitySteps;

        public uint MaxQualitySteps {
            get => _maxQualitySteps;
            set {
                if (value == _maxQualitySteps)
                    return;

                _maxQualitySteps = value;
                foreach (var node in _nodes) {
                    if (node is RecipeNode recipeNode)
                        recipeNode.MaxQualitySteps = _maxQualitySteps;
                }
            }
        }

        public Quality DefaultAssemblerQuality { get; set; }

        public event EventHandler<NodeEventArgs> NodeAdded;
        public event EventHandler<NodeEventArgs> NodeDeleted;
        public event EventHandler<NodeLinkEventArgs> LinkAdded;
        public event EventHandler<NodeLinkEventArgs> LinkDeleted;
        public event EventHandler<EventArgs> NodeValuesUpdated;

        public Rectangle Bounds {
            get {
                if (_nodes.Count == 0)
                    return new Rectangle(0, 0, 0, 0);

                var xMin = int.MaxValue;
                var yMin = int.MaxValue;
                var xMax = int.MinValue;
                var yMax = int.MinValue;
                foreach (var node in _nodes) {
                    xMin = Math.Min(xMin, node.Location.X);
                    xMax = Math.Max(xMax, node.Location.X);
                    yMin = Math.Min(yMin, node.Location.Y);
                    yMax = Math.Max(yMax, node.Location.Y);
                }

                return new Rectangle(xMin - XBorder, yMin - YBorder, xMax - xMin + 2 * XBorder, yMax - yMin + 2 * YBorder);
            }
        }

        private HashSet<BaseNode> _nodes = [];
        private HashSet<NodeLink> _nodeLinks = [];
        private Dictionary<ReadOnlyBaseNode, BaseNode> _roToNode = new();
        private Dictionary<ReadOnlyNodeLink, NodeLink> _roToLink = new();
        private int _lastNodeId;

        public BaseNodeController RequestNodeController(ReadOnlyBaseNode node) {
            return _roToNode.TryGetValue(node, out var value) ? value.Controller : null;
        }

        public ReadOnlyConsumerNode CreateConsumerNode(ItemQualityPair item, Point location) {
            var node = new ConsumerNode(this, _lastNodeId++, item) {
                Location = location,
                NodeDirection = DefaultNodeDirection
            };
            _nodes.Add(node);
            _roToNode.Add(node.ReadOnlyNode, node);
            node.UpdateState();
            NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
            return (ReadOnlyConsumerNode) node.ReadOnlyNode;
        }

        public ReadOnlySupplierNode CreateSupplierNode(ItemQualityPair item, Point location) {
            var node = new SupplierNode(this, _lastNodeId++, item) {
                Location = location,
                NodeDirection = DefaultNodeDirection
            };
            _nodes.Add(node);
            _roToNode.Add(node.ReadOnlyNode, node);
            node.UpdateState();
            NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
            return (ReadOnlySupplierNode) node.ReadOnlyNode;
        }

        public ReadOnlyPassthroughNode CreatePassthroughNode(ItemQualityPair item, Point location) {
            var node = new PassthroughNode(this, _lastNodeId++, item) {
                Location = location,
                NodeDirection = DefaultNodeDirection,
                SimpleDraw = DefaultToSimplePassthroughNodes
            };
            _nodes.Add(node);
            _roToNode.Add(node.ReadOnlyNode, node);
            node.UpdateState();
            NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
            return (ReadOnlyPassthroughNode) node.ReadOnlyNode;
        }

        public ReadOnlySpoilNode CreateSpoilNode(ItemQualityPair inputItem, Item outputItem, Point location) {
            var node = new SpoilNode(this, _lastNodeId++, inputItem, outputItem) {
                Location = location,
                NodeDirection = DefaultNodeDirection
            };
            _nodes.Add(node);
            _roToNode.Add(node.ReadOnlyNode, node);
            node.UpdateState();
            NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
            return (ReadOnlySpoilNode) node.ReadOnlyNode;
        }

        public ReadOnlyPlantNode CreatePlantNode(PlantProcess plantProcess, Quality quality, Point location) {
            var node = new PlantNode(this, _lastNodeId++, plantProcess, quality) {
                Location = location,
                NodeDirection = DefaultNodeDirection
            };
            _nodes.Add(node);
            _roToNode.Add(node.ReadOnlyNode, node);
            node.UpdateState();
            NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
            return (ReadOnlyPlantNode) node.ReadOnlyNode;
        }

        public ReadOnlyRecipeNode CreateRecipeNode(RecipeQualityPair recipe, Point location) {
            return CreateRecipeNode(recipe, location, null);
        }

        // node setup action is used to populate the node prior to informing everything of its creation
        private ReadOnlyRecipeNode CreateRecipeNode(RecipeQualityPair recipe, Point location, Action<RecipeNode> nodeSetupAction) {
            var node = new RecipeNode(this, _lastNodeId++, recipe, DefaultAssemblerQuality) {
                Location = location,
                NodeDirection = DefaultNodeDirection
            };
            nodeSetupAction?.Invoke(node);
            if (nodeSetupAction == null) {
                var rnController = (RecipeNodeController) node.Controller;
                rnController.AutoSetAssembler();
                rnController.AutoSetAssemblerModules();
            }

            _nodes.Add(node);
            _roToNode.Add(node.ReadOnlyNode, node);
            node.UpdateInputsAndOutputs();
            NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
            return (ReadOnlyRecipeNode) node.ReadOnlyNode;
        }

        public ReadOnlyNodeLink CreateLink(ReadOnlyBaseNode supplier, ReadOnlyBaseNode consumer, ItemQualityPair item) {
            if (!_roToNode.ContainsKey(supplier) || !_roToNode.ContainsKey(consumer) || !supplier.Outputs.Contains(item) || !consumer.Inputs.Contains(item))
                Trace.Fail($"Node link creation called with invalid parameters! consumer:{consumer}. supplier:{supplier}. item:{item.ToString()}.");

            // check for an already existing connection
            if (supplier.OutputLinks.Any(l => l.Item == item && l.Consumer == consumer))
                return supplier.OutputLinks.First(l => l.Item == item && l.Consumer == consumer);

            var supplierNode = _roToNode[supplier];
            var consumerNode = _roToNode[consumer];

            var link = new NodeLink(this, supplierNode, consumerNode, item);
            supplierNode.OutputLinks.Add(link);
            consumerNode.InputLinks.Add(link);
            LinkChangeUpdateImpactedNodeStates(link, LinkType.Input);
            LinkChangeUpdateImpactedNodeStates(link, LinkType.Output);

            _nodeLinks.Add(link);
            _roToLink.Add(link.ReadOnlyLink, link);
            LinkAdded?.Invoke(this, new NodeLinkEventArgs(link.ReadOnlyLink));
            return link.ReadOnlyLink;
        }

        public void DeleteNode(ReadOnlyBaseNode node) {
            if (!_roToNode.ContainsKey(node))
                Trace.Fail($"Node deletion called on a node ({node}) that isnt part of the graph!");

            foreach (var link in node.InputLinks.ToList())
                DeleteLink(link);
            foreach (var link in node.OutputLinks.ToList())
                DeleteLink(link);

            _nodes.Remove(_roToNode[node]);
            _roToNode.Remove(node);
            NodeDeleted?.Invoke(this, new NodeEventArgs(node));
        }

        public void DeleteNodes(IEnumerable<ReadOnlyBaseNode> nodes) {
            foreach (var node in nodes)
                DeleteNode(node);
        }

        public void DeleteLink(ReadOnlyNodeLink link) {
            if (!_roToLink.ContainsKey(link) || !_roToNode.ContainsKey(link.Consumer) || !_roToNode.ContainsKey(link.Supplier))
                Trace.Fail(
                    $"Link deletion called with a link ({link}) that isn't part of the graph, or whose node(s) ({link.Consumer}), ({link.Supplier}) is/are not part of the graph!");

            var nodeLink = _roToLink[link];
            nodeLink.ConsumerNode.InputLinks.Remove(nodeLink);
            nodeLink.SupplierNode.OutputLinks.Remove(nodeLink);
            LinkChangeUpdateImpactedNodeStates(nodeLink, LinkType.Input);
            LinkChangeUpdateImpactedNodeStates(nodeLink, LinkType.Output);

            _nodeLinks.Remove(nodeLink);
            _roToLink.Remove(link);
            LinkDeleted?.Invoke(this, new NodeLinkEventArgs(link));
        }

        public void ClearGraph() {
            foreach (var node in _nodes.ToList())
                DeleteNode(node.ReadOnlyNode);

            SerializeNodeIdSet = null;
            _lastNodeId = 0;
        }

        public void UpdateNodeMaxQualities() {
            foreach (var recuipeNode in _nodes.Where(n => n is RecipeNode).Cast<RecipeNode>()) {
                recuipeNode.UpdateInputsAndOutputs(true);
                recuipeNode.UpdateState();
            }
        }

        public void UpdateNodeStates(bool markAllAsDirty) {
            foreach (var node in _nodes)
                node.UpdateState(markAllAsDirty);
        }

        public IEnumerable<ReadOnlyBaseNode> GetSuppliers(ItemQualityPair item) {
            return Nodes.Where(node => node.Outputs.Contains(item));
        }

        public IEnumerable<ReadOnlyBaseNode> GetConsumers(ItemQualityPair item) {
            return Nodes.Where(node => node.Inputs.Contains(item));
        }

        public IEnumerable<IEnumerable<ReadOnlyBaseNode>> GetConnectedNodeGroups(bool includeCleanComponents) {
            return GetConnectedComponents(includeCleanComponents).Select(group => group.Select(node => node.ReadOnlyNode));
        }

        // used to break the graph into groups (in case there are multiple disconnected groups) for simpler solving.
        // Clean components refer to node groups where all the nodes inside the group haven't had any changes since last solve operation
        private IEnumerable<IEnumerable<BaseNode>> GetConnectedComponents(bool includeCleanComponents) {
            // there is an optimized solution for connected components where we keep track of the various groups and modify them
            // as each node/link is added/removed, but testing shows that this calculation below takes under 1ms even for large 1000+ node graphs,
            // so why bother.


            var unvisitedNodes = new HashSet<BaseNode>(_nodes);

            var connectedComponents = new List<HashSet<BaseNode>>();

            while (unvisitedNodes.Any()) {
                var newSet = new HashSet<BaseNode>();
                var allClean = true;

                var toVisitNext = new HashSet<BaseNode> { unvisitedNodes.First() };

                while (toVisitNext.Any()) {
                    var currentNode = toVisitNext.First();
                    allClean &= currentNode.IsClean;

                    foreach (var link in currentNode.InputLinks) {
                        if (unvisitedNodes.Contains(link.SupplierNode))
                            toVisitNext.Add(link.SupplierNode);
                    }

                    foreach (var link in currentNode.OutputLinks) {
                        if (unvisitedNodes.Contains(link.ConsumerNode))
                            toVisitNext.Add(link.ConsumerNode);
                    }

                    newSet.Add(currentNode);
                    toVisitNext.Remove(currentNode);
                    unvisitedNodes.Remove(currentNode);
                }

                if (!allClean || includeCleanComponents)
                    connectedComponents.Add(newSet);
            }

            return connectedComponents;
        }

        public void UpdateNodeValues() {
            if (!PauseUpdates) {
                try {
                    OptimizeGraphNodeValues();
                } catch (OverflowException) {
                    // overflow can theoretically be possible for extremely unbalanced recipes,
                    // but with the limit of double and the artificial limit set on max throughput this should never happen.
                }
            }

            // called even if no changes have been made in order to re-draw the graph
            // (since something required a node value update - link deletion? node addition? whatever)
            NodeValuesUpdated?.Invoke(this, EventArgs.Empty);
        }

        // helper function to update all the impacted nodes after addition/removal of a given link.
        // Basically we want to update any node connected to this link through passthrough nodes (or directly).
        private void LinkChangeUpdateImpactedNodeStates(NodeLink link, LinkType direction) {
            // to prevent a loop
            var visitedLinks = new HashSet<NodeLink>();

            InternalUpdateLinkedNodes(link);
            return;

            void InternalUpdateLinkedNodes(NodeLink iLink) {
                if (!visitedLinks.Add(iLink))
                    return;

                if (direction == LinkType.Output) {
                    iLink.ConsumerNode.UpdateState();
                    if (iLink.ConsumerNode is not PassthroughNode)
                        return;

                    foreach (var secondaryLink in iLink.ConsumerNode.OutputLinks)
                        InternalUpdateLinkedNodes(secondaryLink);
                } else {
                    iLink.SupplierNode.UpdateState();
                    if (iLink.SupplierNode is not PassthroughNode)
                        return;

                    foreach (var secondaryLink in iLink.SupplierNode.InputLinks)
                        InternalUpdateLinkedNodes(secondaryLink);
                }
            }
        }

        //----------------------------------------------Save/Load JSON functions

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            // collect the set of nodes and links to be saved
            // (either entire set, or only that which is bound by the specified serialized node list)

            var includedNodes = _nodes;
            var includedLinks = _nodeLinks;
            if (SerializeNodeIdSet != null) {
                includedNodes = new HashSet<BaseNode>(_nodes.Where(node => SerializeNodeIdSet.Contains(node.NodeId)));
                includedLinks = [];
                foreach (var link in _nodeLinks)
                    if (includedNodes.Contains(link.ConsumerNode) && includedNodes.Contains(link.SupplierNode))
                        includedLinks.Add(link);
            }

            // prepare list of items/assemblers/modules/beacons/recipes that are part of the saved set.
            // RecipesView have to include a missing component due to the possibility of different recipes having same name
            // (ex: regular iron.recipe, missing iron.recipe, missing iron.recipe #2)

            var includedItems = new HashSet<string>();

            var includedAssemblers = new HashSet<string>();
            var includedModules = new HashSet<string>();
            var includedBeacons = new HashSet<string>();

            var includedRecipes = new HashSet<Recipe>();
            // compares by name, ingredients, and products (not amounts, just items)
            var includedMissingRecipes = new HashSet<Recipe>(new RecipeNaInPrComparer());
            var includedPlantProcesses = new HashSet<PlantProcess>();
            var includedMissingPlantProcesses = new HashSet<PlantProcess>(new PlantNaInPrComparer());

            // name,level
            var includedQualities = new HashSet<KeyValuePair<string, int>> {
                new(DefaultAssemblerQuality.Name, DefaultAssemblerQuality.Level)
            };

            foreach (var node in includedNodes) {
                switch (node) {
                    case RecipeNode recipeNode:
                        if (recipeNode.BaseRecipe.Recipe.IsMissing)
                            includedMissingRecipes.Add(recipeNode.BaseRecipe.Recipe);
                        else
                            includedRecipes.Add(recipeNode.BaseRecipe.Recipe);

                        includedAssemblers.Add(recipeNode.SelectedAssembler.Assembler.Name);

                        if (recipeNode.SelectedBeacon)
                            includedBeacons.Add(recipeNode.SelectedBeacon.Beacon.Name);

                        includedModules.UnionWith(recipeNode.AssemblerModules.Select(m => m.Module.Name));
                        includedModules.UnionWith(recipeNode.BeaconModules.Select(m => m.Module.Name));

                        includedQualities.Add(new KeyValuePair<string, int>(recipeNode.BaseRecipe.Quality.Name, recipeNode.BaseRecipe.Quality.Level));
                        includedQualities.Add(new KeyValuePair<string, int>(recipeNode.SelectedAssembler.Quality.Name,
                            recipeNode.SelectedAssembler.Quality.Level));

                        if (recipeNode.SelectedBeacon)
                            includedQualities.Add(new KeyValuePair<string, int>(recipeNode.BaseRecipe.Quality.Name, recipeNode.BaseRecipe.Quality.Level));

                        includedQualities.UnionWith(recipeNode.AssemblerModules.Select(m => new KeyValuePair<string, int>(m.Quality.Name, m.Quality.Level)));
                        includedQualities.UnionWith(recipeNode.BeaconModules.Select(m => new KeyValuePair<string, int>(m.Quality.Name, m.Quality.Level)));
                        break;
                    case PlantNode plantNode:
                        if (plantNode.BasePlantProcess.IsMissing)
                            includedMissingPlantProcesses.Add(plantNode.BasePlantProcess);
                        else
                            includedPlantProcesses.Add(plantNode.BasePlantProcess);
                        includedQualities.Add(new KeyValuePair<string, int>(plantNode.Seed.Quality.Name, plantNode.Seed.Quality.Level));
                        break;
                    case ConsumerNode consumerNode:
                        includedQualities.Add(new KeyValuePair<string, int>(consumerNode.ConsumedItem.Quality.Name, consumerNode.ConsumedItem.Quality.Level));
                        break;
                    case SupplierNode supplierNode:
                        includedQualities.Add(new KeyValuePair<string, int>(supplierNode.SuppliedItem.Quality.Name, supplierNode.SuppliedItem.Quality.Level));
                        break;
                    case PassthroughNode passNode:
                        includedQualities.Add(new KeyValuePair<string, int>(passNode.PassthroughItem.Quality.Name, passNode.PassthroughItem.Quality.Level));
                        break;
                    case SpoilNode spoilNode:
                        includedQualities.Add(new KeyValuePair<string, int>(spoilNode.InputItem.Quality.Name, spoilNode.InputItem.Quality.Level));
                        break;
                }

                // these will process all inputs/outputs -> so fuel/burnt items are included automatically!

                includedItems.UnionWith(node.Inputs.Select(i => i.Item.Name));
                includedItems.UnionWith(node.Outputs.Select(i => i.Item.Name));
            }

            var includedRecipeShorts = includedRecipes.Select(recipe => new RecipeShort(recipe)).ToList();
            // add the missing after the regular, since when we compare saves to preset we will only check 1st recipe of its name (the non-missing kind then)
            includedRecipeShorts.AddRange(includedMissingRecipes.Select(recipe => new RecipeShort(recipe)));
            var includedPlantShorts = includedPlantProcesses.Select(plantProcess => new PlantShort(plantProcess)).ToList();
            // add the missing after the regular, since when we compare saves to preset we will only check 1st recipe of its name (the non-missing kind then)
            includedPlantShorts.AddRange(includedMissingPlantProcesses.Select(plantProcess => new PlantShort(plantProcess)));

            //serialize
            info.AddValue("Version", Properties.Settings.Default.ForemanVersion);
            info.AddValue("Object", "ProductionGraph");

            info.AddValue("EnableExtraProductivityForNonMiners", EnableExtraProductivityForNonMiners);
            info.AddValue("DefaultNodeDirection", (int) DefaultNodeDirection);
            info.AddValue("Solver_PullOutputNodes", PullOutputNodes);
            info.AddValue("Solver_PullOutputNodesPower", PullOutputNodesPower);
            info.AddValue("Solver_LowPriorityPower", LowPriorityPower);
            info.AddValue("MaxQualitySteps", MaxQualitySteps);
            info.AddValue("DefaultQuality", DefaultAssemblerQuality.Name);

            info.AddValue("IncludedItems", includedItems);
            info.AddValue("IncludedRecipes", includedRecipeShorts);
            info.AddValue("IncludedPlantProcesses", includedPlantShorts);
            info.AddValue("IncludedAssemblers", includedAssemblers);
            info.AddValue("IncludedModules", includedModules);
            info.AddValue("IncludedBeacons", includedBeacons);
            info.AddValue("IncludedQualities", includedQualities);

            info.AddValue("Nodes", includedNodes);
            info.AddValue("NodeLinks", includedLinks);
        }

        // cache is necessary since we will possibly be adding to missing items/recipes
        public NewNodeCollection InsertNodesFromJson(DataCache cache, JObject json, bool loadSolverValues) {
            if (json["Version"] == null || (int) json["Version"] != Properties.Settings.Default.ForemanVersion || json["Object"] == null ||
                (string) json["Object"] != "ProductionGraph") {
                json = VersionUpdater.UpdateGraph(json, cache);
                if (json == null)
                    // update failed
                    return new NewNodeCollection();
            }

            var newNodeCollection = new NewNodeCollection();
            // the links between the node index (as imported) and the newly created node (which will now have a different index).
            // Used to link up nodes
            var oldNodeIndices = new Dictionary<int, ReadOnlyBaseNode>();

            // check compliance on all items, assemblers, modules, beacons, and recipes (data-cache will take care of it) -
            // this means add in any missing objects and handle multi-name recipes (there can be multiple versions of a missing recipe, each with identical names)

            try {
                cache.ProcessImportedItemsSet(json["IncludedItems"].Select(t => (string) t));
                var qualityLinks =
                    cache.ProcessImportedQualitiesSet(json["IncludedQualities"]
                        .Select(j => new KeyValuePair<string, int>((string) j["Key"], (int) j["Value"])));
                cache.ProcessImportedAssemblersSet(json["IncludedAssemblers"].Select(t => (string) t));
                cache.ProcessImportedModulesSet(json["IncludedModules"].Select(t => (string) t));
                cache.ProcessImportedBeaconsSet(json["IncludedBeacons"].Select(t => (string) t));
                var recipeLinks = cache.ProcessImportedRecipesSet(RecipeShort.GetSetFromJson(json["IncludedRecipes"]));
                var plantProcessLinks =
                    cache.ProcessImportedPlantProcessesSet(PlantShort.GetSetFromJson(json["IncludedPlantProcesses"]));

                if (loadSolverValues) {
                    EnableExtraProductivityForNonMiners = (bool) json["EnableExtraProductivityForNonMiners"];
                    DefaultNodeDirection = (NodeDirection) (int) json["DefaultNodeDirection"];
                    PullOutputNodes = (bool) json["Solver_PullOutputNodes"];
                    PullOutputNodesPower = (double) json["Solver_PullOutputNodesPower"];
                    LowPriorityPower = (double) json["Solver_LowPriorityPower"];
                    MaxQualitySteps = (uint) json["MaxQualitySteps"];
                    DefaultAssemblerQuality = qualityLinks[(string) json["DefaultQuality"]];
                }

                // add in all the graph nodes

                foreach (var nodeJToken in json["Nodes"].ToList()) {
                    BaseNode newNode;
                    var locationString = ((string) nodeJToken["Location"]).Split(',');
                    var location = new Point(int.Parse(locationString[0]), int.Parse(locationString[1]));
                    // just an early define
                    string itemName;
                    // early define
                    Quality quality;

                    switch ((NodeType) (int) nodeJToken["NodeType"]) {
                        case NodeType.Consumer: {
                            itemName = (string) nodeJToken["Item"];
                            quality = qualityLinks[(string) nodeJToken["BaseQuality"]];
                            newNode = cache.Items.TryGetValue(itemName, out var item)
                                ? _roToNode[CreateConsumerNode(new ItemQualityPair(item, quality), location)]
                                : _roToNode[CreateConsumerNode(new ItemQualityPair(cache.MissingItems[itemName], quality), location)];
                            newNodeCollection.NewNodes.Add(newNode.ReadOnlyNode);
                            break;
                        }

                        case NodeType.Supplier: {
                            itemName = (string) nodeJToken["Item"];
                            quality = qualityLinks[(string) nodeJToken["BaseQuality"]];
                            newNode = cache.Items.TryGetValue(itemName, out var item)
                                ? _roToNode[CreateSupplierNode(new ItemQualityPair(item, quality), location)]
                                : _roToNode[CreateSupplierNode(new ItemQualityPair(cache.MissingItems[itemName], quality), location)];
                            newNodeCollection.NewNodes.Add(newNode.ReadOnlyNode);
                            break;
                        }

                        case NodeType.Passthrough: {
                            itemName = (string) nodeJToken["Item"];
                            quality = qualityLinks[(string) nodeJToken["BaseQuality"]];
                            newNode = cache.Items.TryGetValue(itemName, out var item)
                                ? _roToNode[CreatePassthroughNode(new ItemQualityPair(item, quality), location)]
                                : _roToNode[CreatePassthroughNode(new ItemQualityPair(cache.MissingItems[itemName], quality), location)];
                            ((PassthroughNode) newNode).SimpleDraw = (bool) nodeJToken["SDraw"];
                            newNodeCollection.NewNodes.Add(newNode.ReadOnlyNode);
                            break;
                        }

                        case NodeType.Spoil: {
                            itemName = (string) nodeJToken["InputItem"];
                            var outputItemName = (string) nodeJToken["OutputItem"];
                            quality = qualityLinks[(string) nodeJToken["BaseQuality"]];
                            var inputItem = cache.Items.TryGetValue(itemName, out var iItem)
                                ? iItem
                                : cache.MissingItems[itemName];
                            var outputItem = cache.Items.TryGetValue(outputItemName, out var oItem)
                                ? oItem
                                : cache.MissingItems[outputItemName];
                            newNode = _roToNode[CreateSpoilNode(new ItemQualityPair(inputItem, quality), outputItem, location)];
                            newNodeCollection.NewNodes.Add(newNode.ReadOnlyNode);
                            break;
                        }

                        case NodeType.Plant: {
                            var plantProcessId = (long) nodeJToken["PlantProcessID"];
                            quality = qualityLinks[(string) nodeJToken["BaseQuality"]];
                            newNode = _roToNode[CreatePlantNode(plantProcessLinks[plantProcessId], quality, location)];
                            newNodeCollection.NewNodes.Add(newNode.ReadOnlyNode);
                            break;
                        }

                        case NodeType.Recipe: {
                            var recipeId = (long) nodeJToken["RecipeID"];
                            var recipeQuality = qualityLinks[(string) nodeJToken["RecipeQuality"]];
                            newNode = _roToNode[CreateRecipeNode(new RecipeQualityPair(recipeLinks[recipeId], recipeQuality), location, rNode => {
                                var rNodeController = (RecipeNodeController) rNode.Controller;

                                rNode.LowPriority = nodeJToken["LowPriority"] != null;

                                rNode.NeighbourCount = (double) nodeJToken["Neighbours"];
                                rNode.ExtraProductivityBonus = (double) nodeJToken["ExtraProductivity"];

                                var assemblerName = (string) nodeJToken["Assembler"];
                                var assemblerQuality = qualityLinks[(string) nodeJToken["AssemblerQuality"]];
                                rNodeController.SetAssembler(cache.Assemblers.TryGetValue(assemblerName, out var assembler)
                                    ? new AssemblerQualityPair(assembler, assemblerQuality)
                                    : new AssemblerQualityPair(cache.MissingAssemblers[assemblerName], assemblerQuality));

                                foreach (var module in nodeJToken["AssemblerModules"]) {
                                    var moduleName = (string) module["Name"];
                                    var moduleQuality = qualityLinks[(string) module["Quality"]];
                                    rNodeController.AddAssemblerModule(cache.Modules.TryGetValue(moduleName, out var cacheModule)
                                        ? new ModuleQualityPair(cacheModule, moduleQuality)
                                        : new ModuleQualityPair(cache.MissingModules[moduleName], moduleQuality));
                                }

                                if (nodeJToken["Fuel"] != null) {
                                    rNodeController.SetFuel(cache.Items.ContainsKey((string) nodeJToken["Fuel"])
                                        ? cache.Items[(string) nodeJToken["Fuel"]]
                                        : cache.MissingItems[(string) nodeJToken["Fuel"]]);
                                } else if (rNode.SelectedAssembler.Assembler.IsBurner)
                                    // and fuel is null... well - it's the import. set it as null (and consider it an error)
                                    rNodeController.SetFuel(null);

                                if (nodeJToken["Burnt"] != null) {
                                    var burntItem = cache.Items.ContainsKey((string) nodeJToken["Burnt"])
                                        ? cache.Items[(string) nodeJToken["Burnt"]]
                                        : cache.MissingItems[(string) nodeJToken["Burnt"]];
                                    if (rNode.FuelRemains != burntItem)
                                        rNode.SetBurntOverride(burntItem);
                                } else if (rNode.Fuel is { BurnResult: not null })
                                    // same as above - there should be a burn result, but there isn't...
                                    rNode.SetBurntOverride(null);

                                if (nodeJToken["Beacon"] != null) {
                                    var beaconName = (string) nodeJToken["Beacon"];
                                    var beaconQuality = qualityLinks[(string) nodeJToken["BeaconQuality"]];
                                    rNodeController.SetBeacon(cache.Beacons.TryGetValue(beaconName, out var beacon)
                                        ? new BeaconQualityPair(beacon, beaconQuality)
                                        : new BeaconQualityPair(cache.MissingBeacons[beaconName], beaconQuality));

                                    foreach (var module in nodeJToken["BeaconModules"]) {
                                        var moduleName = (string) module["Name"];
                                        var moduleQuality = qualityLinks[(string) module["Quality"]];
                                        rNodeController.AddBeaconModule(cache.Modules.TryGetValue(moduleName, out var cacheModule)
                                            ? new ModuleQualityPair(cacheModule, moduleQuality)
                                            : new ModuleQualityPair(cache.MissingModules[moduleName], moduleQuality));
                                    }

                                    rNode.BeaconCount = (double) nodeJToken["BeaconCount"];
                                    rNode.BeaconsPerAssembler = (double) nodeJToken["BeaconsPerAssembler"];
                                    rNode.BeaconsConst = (double) nodeJToken["BeaconsConst"];
                                }

                                // done last, to catch any errors above first.
                                newNodeCollection.NewNodes.Add(rNode.ReadOnlyNode);
                            })];
                            break;
                        }

                        default:
                            // we will catch it right away and delete all nodes added in thus far.
                            // Error was most likely in json read, in which case we count it as a corrupt json and not import anything.
                            throw new Exception();
                    }

                    newNode.RateType = (RateType) (int) nodeJToken["RateType"];
                    if (newNode.RateType == RateType.Manual)
                        newNode.DesiredSetValue = (double) nodeJToken["DesiredSetValue"];

                    newNode.NodeDirection = (NodeDirection) (int) nodeJToken["Direction"];

                    if (nodeJToken["KeyNode"] != null) {
                        newNode.KeyNode = true;
                        newNode.KeyNodeTitle = (string) nodeJToken["KeyNode"];
                    }

                    oldNodeIndices.Add((int) nodeJToken["NodeId"], newNode.ReadOnlyNode);
                }

                // link the new nodes

                foreach (var nodeLinkJToken in json["NodeLinks"].ToList()) {
                    var supplier = oldNodeIndices[(int) nodeLinkJToken["SupplierID"]];
                    var consumer = oldNodeIndices[(int) nodeLinkJToken["ConsumerID"]];
                    var quality = qualityLinks[(string) nodeLinkJToken["Quality"]];

                    var itemName = (string) nodeLinkJToken["Item"];
                    var item = cache.Items.TryGetValue(itemName, out var cacheItem)
                        ? new ItemQualityPair(cacheItem, quality)
                        : new ItemQualityPair(cache.MissingItems[itemName], quality);

                    if (LinkChecker.IsPossibleConnection(item, supplier, consumer))
                        // not necessary to test if connection is valid. It must be valid based on json
                        newNodeCollection.NewLinks.Add(CreateLink(supplier, consumer, item));
                }
            } catch (Exception e) {
                // there was something wrong with the json (probably someone edited it by hand, and it didn't link properly). Delete all added nodes and return empty

                ErrorLogging.LogLine($"Error loading nodes into production graph! ERROR: {e}");
                Console.WriteLine(e);
                DeleteNodes(newNodeCollection.NewNodes);
                return new NewNodeCollection();
            }

            return newNodeCollection;
        }
    }
}