using Google.OrTools.LinearSolver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace Foreman {
    public enum NodeType { Supplier, Consumer, Passthrough, Recipe, Spoil, Plant }
    public enum LinkType { Input, Output }

    public class NodeEventArgs : EventArgs {
        public ReadOnlyBaseNode node;
        public NodeEventArgs(ReadOnlyBaseNode node) { this.node = node; }
    }
    public class NodeLinkEventArgs : EventArgs {
        public ReadOnlyNodeLink nodeLink;
        public NodeLinkEventArgs(ReadOnlyNodeLink nodeLink) { this.nodeLink = nodeLink; }
    }

    [Serializable]
    public partial class ProductionGraph : ISerializable {
        public class NewNodeCollection {
            public List<ReadOnlyBaseNode> newNodes { get; private set; }
            public List<ReadOnlyNodeLink> newLinks { get; private set; }
            public NewNodeCollection() { newNodes = new List<ReadOnlyBaseNode>(); newLinks = new List<ReadOnlyNodeLink>(); }
        }

        //public DataCache DCache { get; private set; }

        public enum RateUnit { Per1Sec, Per1Min, Per5Min, Per10Min, Per30Min, Per1Hour };//, Per6Hour, Per12Hour, Per24Hour }
        public static readonly string[] RateUnitNames = new string[] { "1 sec", "1 min", "5 min", "10 min", "30 min", "1 hour" }; //, "6 hours", "12 hours", "24 hours" };
        private static readonly float[] RateMultiplier = new float[] { 1f, 60f, 300f, 600f, 1800f, 3600f }; //, 21600f, 43200f, 86400f };

        public RateUnit SelectedRateUnit { get; set; }
        public float GetRateMultipler() { return RateMultiplier[(int)SelectedRateUnit]; } //the amount of assemblers required will be multipled by the rate multipler when displaying.
        public string GetRateName() { return RateUnitNames[(int)SelectedRateUnit]; }

        public NodeDirection DefaultNodeDirection { get; set; }
        public bool DefaultToSimplePassthroughNodes { get; set; }

        public const double MaxSetFlow = 1e7; //10 million (per second) item flow should be enough for pretty much everything with a generous helping of 'oh god thats way too much!'
        public const double MaxFactories = 1e6; //1 million factories should be good enough as well. NOTE: the auto values can go higher, you just cant set more than 1 million on the manual setting.
        public const double MaxTiles = 1e7; //10 million tiles for planting should be good enough
        public const double MaxInventorySlots = 1e6; // 1 million inventory slots for spoiling should be good enough
        private const int XBorder = 200;
        private const int YBorder = 200;

        public bool PauseUpdates { get; set; }
        public bool PullOutputNodes { get; set; } //if true, the solver will add a 'pull' for output nodes so as to prioritize them over lowering factory count. WARNING: this can lead to '0' solutions if there is any production path that can go to infinity (aka: ensure enough nodes are constrained!)
        public double PullOutputNodesPower { get; set; }
        public double LowPriorityPower { get; set; } //this is the multiplier of the factory cost function for low priority nodes. aka: low priority recipes will be picked if the alternative involves this much more factories (10,000 is a nice value here)
        public bool EnableExtraProductivityForNonMiners { get; set; }

        public AssemblerSelector AssemblerSelector { get; private set; }
        public ModuleSelector ModuleSelector { get; private set; }
        public FuelSelector FuelSelector { get; private set; }

        public IEnumerable<ReadOnlyBaseNode> Nodes => nodes.Select(node => node.ReadOnlyNode).OfType<ReadOnlyBaseNode>();
        public IEnumerable<ReadOnlyNodeLink> NodeLinks => nodeLinks.Select(link => link.ReadOnlyLink);
        public HashSet<int>? SerializeNodeIdSet { get; set; } //if this isnt null then the serialized production graph will only contain these nodes (and links between them)

        //editing this value will require the entire graph to be updated as any recipe nodes on it will possibly change the number of products and possibly cause a cascade of removed links
        private uint maxQualitySteps;
        public uint MaxQualitySteps {
            get { return maxQualitySteps; }
            set {
                if (value != maxQualitySteps) {
                    maxQualitySteps = value;
                    foreach (BaseNode node in nodes) {
                        if (node is RecipeNode rnode)
                            rnode.MaxQualitySteps = maxQualitySteps;
                    }
                }
            }
        }

        public Quality? DefaultAssemblerQuality { get; set; }

        public event EventHandler<NodeEventArgs>? NodeAdded;
        public event EventHandler<NodeEventArgs>? NodeDeleted;
        public event EventHandler<NodeLinkEventArgs>? LinkAdded;
        public event EventHandler<NodeLinkEventArgs>? LinkDeleted;
        public event EventHandler<EventArgs>? NodeValuesUpdated;

        public Rectangle Bounds {
            get {
                if (nodes.Count == 0)
                    return new Rectangle(0, 0, 0, 0);

                int xMin = int.MaxValue;
                int yMin = int.MaxValue;
                int xMax = int.MinValue;
                int yMax = int.MinValue;
                foreach (BaseNode node in nodes) {
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

        public ProductionGraph() {
            DefaultNodeDirection = NodeDirection.Up;
            PullOutputNodes = false;
            PullOutputNodesPower = 10;
            LowPriorityPower = 1e5;

            nodes = new HashSet<BaseNode>();
            nodeLinks = new HashSet<NodeLink>();
            roToNode = new Dictionary<ReadOnlyBaseNode, BaseNode>();
            roToLink = new Dictionary<ReadOnlyNodeLink, NodeLink>();
            lastNodeID = 0;

            AssemblerSelector = new AssemblerSelector();
            ModuleSelector = new ModuleSelector();
            FuelSelector = new FuelSelector();
        }

        public BaseNodeController? RequestNodeController(ReadOnlyBaseNode node) => roToNode.TryGetValue(node, out var bn) ? bn.Controller : null;

        private T SetupNodeOfType<T>(BaseNode node, Point location)
            where T : ReadOnlyBaseNode {
            node.Location = location;
            node.NodeDirection = DefaultNodeDirection;
            if (node.ReadOnlyNode is not T ret)
                throw new ArgumentNullException(nameof(node.ReadOnlyNode));
            nodes.Add(node);
            roToNode.Add(node.ReadOnlyNode, node);
            node.UpdateState();
            NodeAdded?.Invoke(this, new NodeEventArgs(node.ReadOnlyNode));
            return ret;
        }

        public ReadOnlyConsumerNode CreateConsumerNode(ItemQualityPair item, Point location) {
            return SetupNodeOfType<ReadOnlyConsumerNode>(new ConsumerNode(this, lastNodeID++, item), location);
        }

        public ReadOnlySupplierNode CreateSupplierNode(ItemQualityPair item, Point location) {
            return SetupNodeOfType<ReadOnlySupplierNode>(new SupplierNode(this, lastNodeID++, item), location);
        }

        public ReadOnlyPassthroughNode CreatePassthroughNode(ItemQualityPair item, Point location) {
            return SetupNodeOfType<ReadOnlyPassthroughNode>(new PassthroughNode(this, lastNodeID++, item), location);
        }

        public ReadOnlySpoilNode CreateSpoilNode(ItemQualityPair inputItem, Item outputItem, Point location) {
            return SetupNodeOfType<ReadOnlySpoilNode>(new SpoilNode(this, lastNodeID++, inputItem, outputItem), location);
        }

        public ReadOnlyPlantNode CreatePlantNode(PlantProcess plantProcess, Quality quality, Point location) {
            return SetupNodeOfType<ReadOnlyPlantNode>(new PlantNode(this, lastNodeID++, plantProcess, quality), location);
        }

        public ReadOnlyRecipeNode CreateRecipeNode(RecipeQualityPair recipe, Point location) => CreateRecipeNode(recipe, location, null);
        private ReadOnlyRecipeNode CreateRecipeNode(RecipeQualityPair recipe, Point location, Action<RecipeNode>? nodeSetupAction) //node setup action is used to populate the node prior to informing everyone of its creation
        {
            if (DefaultAssemblerQuality is null)
                throw new NullReferenceException(nameof(DefaultAssemblerQuality));
            RecipeNode node = new RecipeNode(this, lastNodeID++, recipe, DefaultAssemblerQuality);
            node.Location = location;
            node.NodeDirection = DefaultNodeDirection;
            if (node.ReadOnlyNode is not ReadOnlyRecipeNode ret)
                throw new NullReferenceException(nameof(node.ReadOnlyNode));
            nodeSetupAction?.Invoke(node);
            if (nodeSetupAction == null) {
                RecipeNodeController rnController = (RecipeNodeController)node.Controller;
                rnController.AutoSetAssembler();
                rnController.AutoSetAssemblerModules();
            }
            nodes.Add(node);
            roToNode.Add(ret, node);
            node.UpdateInputsAndOutputs();
            NodeAdded?.Invoke(this, new NodeEventArgs(ret));
            return ret;
        }

        public ReadOnlyNodeLink CreateLink(ReadOnlyBaseNode supplier, ReadOnlyBaseNode consumer, ItemQualityPair item) {
            if (!roToNode.ContainsKey(supplier) || !roToNode.ContainsKey(consumer) || !supplier.Outputs.Contains(item) || !consumer.Inputs.Contains(item))
                Trace.Fail(string.Format("Node link creation called with invalid parameters! consumer:{0}. supplier:{1}. item:{2}.", consumer.ToString(), supplier.ToString(), item.ToString()));
            if (supplier.OutputLinks.Any(l => l.Item == item && l.Consumer == consumer)) //check for an already existing connection
                return supplier.OutputLinks.First(l => l.Item == item && l.Consumer == consumer);

            BaseNode supplierNode = roToNode[supplier];
            BaseNode consumerNode = roToNode[consumer];

            NodeLink link = new NodeLink(this, supplierNode, consumerNode, item);
            supplierNode.OutputLinks.Add(link);
            consumerNode.InputLinks.Add(link);
            LinkChangeUpdateImpactedNodeStates(link, LinkType.Input);
            LinkChangeUpdateImpactedNodeStates(link, LinkType.Output);

            nodeLinks.Add(link);
            roToLink.Add(link.ReadOnlyLink, link);
            LinkAdded?.Invoke(this, new NodeLinkEventArgs(link.ReadOnlyLink));
            return link.ReadOnlyLink;
        }

        public void DeleteNode(ReadOnlyBaseNode node) {
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

        public void DeleteNodes(IEnumerable<ReadOnlyBaseNode> nodes) {
            foreach (ReadOnlyBaseNode node in nodes)
                DeleteNode(node);
        }

        public void DeleteLink(ReadOnlyNodeLink link) {
            if (!roToLink.ContainsKey(link) || link.Consumer is null || link.Supplier is null || !roToNode.ContainsKey(link.Consumer) || !roToNode.ContainsKey(link.Supplier))
                Trace.Fail(string.Format("Link deletion called with a link ({0}) that isnt part of the graph, or whose node(s) ({1}), ({2}) is/are not part of the graph!", link.ToString(), link.Consumer?.ToString(), link.Supplier?.ToString()));

            NodeLink nodeLink = roToLink[link];
            nodeLink.ConsumerNode.InputLinks.Remove(nodeLink);
            nodeLink.SupplierNode.OutputLinks.Remove(nodeLink);
            LinkChangeUpdateImpactedNodeStates(nodeLink, LinkType.Input);
            LinkChangeUpdateImpactedNodeStates(nodeLink, LinkType.Output);

            nodeLinks.Remove(nodeLink);
            roToLink.Remove(link);
            LinkDeleted?.Invoke(this, new NodeLinkEventArgs(link));
        }

        public void ClearGraph() {
            foreach (var node in nodes.Select(node => node.ReadOnlyNode).OfType<ReadOnlyBaseNode>())
                DeleteNode(node);

            SerializeNodeIdSet = null;
            lastNodeID = 0;
        }

        public void UpdateNodeMaxQualities() {
            foreach (var rnode in nodes.OfType<RecipeNode>()) {
                rnode.UpdateInputsAndOutputs(true);
                rnode.UpdateState();
            }
        }

        public void UpdateNodeStates(bool markAllAsDirty) {
            foreach (BaseNode node in nodes)
                node.UpdateState(markAllAsDirty);
        }

        public IEnumerable<ReadOnlyBaseNode> GetSuppliers(ItemQualityPair item) {
            foreach (ReadOnlyBaseNode node in Nodes)
                if (node.Outputs.Contains(item))
                    yield return node;
        }

        public IEnumerable<ReadOnlyBaseNode> GetConsumers(ItemQualityPair item) {
            foreach (ReadOnlyBaseNode node in Nodes)
                if (node.Inputs.Contains(item))
                    yield return node;
        }

        public IEnumerable<IEnumerable<ReadOnlyBaseNode>> GetConnectedNodeGroups(bool includeCleanComponents) {
            foreach (IEnumerable<BaseNode> group in GetConnectedComponents(includeCleanComponents))
                yield return group.Select(node => node.ReadOnlyNode).OfType<ReadOnlyBaseNode>();
        }

        private IEnumerable<IEnumerable<BaseNode>> GetConnectedComponents(bool includeCleanComponents) //used to break the graph into groups (in case there are multiple disconnected groups) for simpler solving. Clean components refer to node groups where all the nodes inside the group havent had any changes since last solve operation
        {
            //there is an optimized solution for connected components where we keep track of the various groups and modify them as each node/link is added/removed, but testing shows that this calculation below takes under 1ms even for larg 1000+ node graphs, so why bother.


            HashSet<BaseNode> unvisitedNodes = [.. nodes];

            List<HashSet<BaseNode>> connectedComponents = [];

            while (unvisitedNodes.Any()) {
                HashSet<BaseNode> newSet = [];
                bool allClean = true;

                HashSet<BaseNode> toVisitNext = [unvisitedNodes.First()];

                while (toVisitNext.Any()) {
                    BaseNode currentNode = toVisitNext.First();
                    allClean &= currentNode.IsClean;

                    foreach (NodeLink link in currentNode.InputLinks)
                        if (unvisitedNodes.Contains(link.SupplierNode))
                            toVisitNext.Add(link.SupplierNode);

                    foreach (NodeLink link in currentNode.OutputLinks)
                        if (unvisitedNodes.Contains(link.ConsumerNode))
                            toVisitNext.Add(link.ConsumerNode);

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
                try { OptimizeGraphNodeValues(); } catch (OverflowException) { } //overflow can theoretically be possible for extremely unbalanced recipes, but with the limit of double and the artificial limit set on max throughput this should never happen.
            }
            NodeValuesUpdated?.Invoke(this, EventArgs.Empty); //called even if no changes have been made in order to re-draw the graph (since something required a node value update - link deletion? node addition? whatever)
        }

        private void LinkChangeUpdateImpactedNodeStates(NodeLink link, LinkType direction) //helper function to update all the impacted nodes after addition/removal of a given link. Basically we want to update any node connected to this link through passthrough nodes (or directly).
        {
            HashSet<NodeLink> visitedLinks = new HashSet<NodeLink>(); //to prevent a loop
            void Internal_UpdateLinkedNodes(NodeLink ilink) {
                if (visitedLinks.Contains(ilink))
                    return;
                visitedLinks.Add(ilink);

                if (direction == LinkType.Output) {
                    ilink.ConsumerNode.UpdateState();
                    if (ilink.ConsumerNode is PassthroughNode)
                        foreach (NodeLink secondaryLink in ilink.ConsumerNode.OutputLinks)
                            Internal_UpdateLinkedNodes(secondaryLink);
                } else {
                    ilink.SupplierNode.UpdateState();
                    if (ilink.SupplierNode is PassthroughNode)
                        foreach (NodeLink secondaryLink in ilink.SupplierNode.InputLinks)
                            Internal_UpdateLinkedNodes(secondaryLink);

                }
            }

            Internal_UpdateLinkedNodes(link);
        }

        //----------------------------------------------Save/Load JSON functions

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //collect the set of nodes and links to be saved (either entire set, or only that which is bound by the specified serialized node list)
            HashSet<BaseNode> includedNodes = nodes;
            HashSet<NodeLink> includedLinks = nodeLinks;
            if (SerializeNodeIdSet != null) {
                includedNodes = new HashSet<BaseNode>(nodes.Where(node => SerializeNodeIdSet.Contains(node.NodeID)));
                includedLinks = new HashSet<NodeLink>();
                foreach (NodeLink link in nodeLinks)
                    if (includedNodes.Contains(link.ConsumerNode) && includedNodes.Contains(link.SupplierNode))
                        includedLinks.Add(link);
            }

            //prepare list of items/assemblers/modules/beacons/recipes that are part of the saved set. Recipes have to include a missing component due to the possibility of different recipes having same name (ex: regular iron.recipe, missing iron.recipe, missing iron.recipe #2)
            HashSet<string> includedItems = [];

            HashSet<string> includedAssemblers = [];
            HashSet<string> includedModules = [];
            HashSet<string> includedBeacons = [];

            HashSet<Recipe> includedRecipes = [];
            HashSet<Recipe> includedMissingRecipes = new(new RecipeNaInPrComparer()); //compares by name, ingredients, and products (not amounts, just items)
            HashSet<PlantProcess> includedPlantProcesses = [];
            HashSet<PlantProcess> includedMissingPlantProcesses = new(new PlantNaInPrComparer());

            HashSet<KeyValuePair<string, int>> includedQualities = new(); //name,level
            if (DefaultAssemblerQuality is not null)
                includedQualities.Add(new KeyValuePair<string, int>(DefaultAssemblerQuality.Name, DefaultAssemblerQuality.Level));

            foreach (BaseNode node in includedNodes) {
                switch (node) {
                    case RecipeNode rnode:
                        if (rnode.BaseRecipe.Recipe?.IsMissing is true)
                            includedMissingRecipes.Add(rnode.BaseRecipe.Recipe);
                        else if (rnode.BaseRecipe.Recipe is not null)
                            includedRecipes.Add(rnode.BaseRecipe.Recipe);

                        includedAssemblers.Add(rnode.SelectedAssembler.Assembler.Name);

                        if (rnode.SelectedBeacon && rnode.SelectedBeacon.Beacon is not null)
                            includedBeacons.Add(rnode.SelectedBeacon.Beacon.Name);

                        includedModules.UnionWith(rnode.AssemblerModules.Select(m => m.Module.Name));
                        includedModules.UnionWith(rnode.BeaconModules.Select(m => m.Module.Name));

                        if (rnode.BaseRecipe.Quality is not null)
                            includedQualities.Add(new(rnode.BaseRecipe.Quality.Name, rnode.BaseRecipe.Quality.Level));
                        includedQualities.Add(new(rnode.SelectedAssembler.Quality.Name, rnode.SelectedAssembler.Quality.Level));

                        if (rnode.SelectedBeacon && rnode.BaseRecipe.Quality is not null)
                            includedQualities.Add(new(rnode.BaseRecipe.Quality.Name, rnode.BaseRecipe.Quality.Level));

                        includedQualities.UnionWith(rnode.AssemblerModules.Select(m => new KeyValuePair<string, int>(m.Quality.Name, m.Quality.Level)));
                        includedQualities.UnionWith(rnode.BeaconModules.Select(m => new KeyValuePair<string, int>(m.Quality.Name, m.Quality.Level)));
                        break;
                    case PlantNode pnode:
                        if (pnode.BasePlantProcess.IsMissing)
                            includedMissingPlantProcesses.Add(pnode.BasePlantProcess);
                        else
                            includedPlantProcesses.Add(pnode.BasePlantProcess);
                        if (pnode.Seed.Quality is not null)
                            includedQualities.Add(new(pnode.Seed.Quality.Name, pnode.Seed.Quality.Level));
                        break;
                    case ConsumerNode cnode:
                        if (cnode.ConsumedItem.Quality is not null)
                            includedQualities.Add(new(cnode.ConsumedItem.Quality.Name, cnode.ConsumedItem.Quality.Level));
                        break;
                    case SupplierNode snode:
                        if (snode.SuppliedItem.Quality is not null)
                            includedQualities.Add(new(snode.SuppliedItem.Quality.Name, snode.SuppliedItem.Quality.Level));
                        break;
                    case PassthroughNode passnode:
                        if (passnode.PassthroughItem.Quality is not null)
                            includedQualities.Add(new KeyValuePair<string, int>(passnode.PassthroughItem.Quality.Name, passnode.PassthroughItem.Quality.Level));
                        break;
                    case SpoilNode spoilnode:
                        if (spoilnode.InputItem.Quality is not null)
                            includedQualities.Add(new KeyValuePair<string, int>(spoilnode.InputItem.Quality.Name, spoilnode.InputItem.Quality.Level));
                        break;
                }

                //these will process all inputs/outputs -> so fuel/burnt items are included automatically!
                includedItems.UnionWith(node.Inputs.Select(i => i.Item?.Name).OfType<string>());
                includedItems.UnionWith(node.Outputs.Select(i => i.Item?.Name).OfType<string>());
            }
            var includedRecipeShorts = includedRecipes.Select(recipe => new RecipeShort(recipe)).ToList();
            includedRecipeShorts.AddRange(includedMissingRecipes.Select(recipe => new RecipeShort(recipe))); //add the missing after the regular, since when we compare saves to preset we will only check 1st recipe of its name (the non-missing kind then)
            var includedPlantShorts = includedPlantProcesses.Select(pprocess => new PlantShort(pprocess)).ToList();
            includedPlantShorts.AddRange(includedMissingPlantProcesses.Select(pprocess => new PlantShort(pprocess))); //add the missing after the regular, since when we compare saves to preset we will only check 1st recipe of its name (the non-missing kind then)

            //serialize
            info.AddValue("Version", Properties.Settings.Default.ForemanVersion);
            info.AddValue("Object", "ProductionGraph");

            info.AddValue("EnableExtraProductivityForNonMiners", EnableExtraProductivityForNonMiners);
            info.AddValue("DefaultNodeDirection", (int)DefaultNodeDirection);
            info.AddValue("Solver_PullOutputNodes", PullOutputNodes);
            info.AddValue("Solver_PullOutputNodesPower", PullOutputNodesPower);
            info.AddValue("Solver_LowPriorityPower", LowPriorityPower);
            info.AddValue("MaxQualitySteps", MaxQualitySteps);
            info.AddValue("DefaultQuality", DefaultAssemblerQuality?.Name ?? "normal");

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

        public NewNodeCollection InsertNodesFromJson(DataCache cache, JObject json, bool loadSolverValues) //cache is necessary since we will possibly be adding to mssing items/recipes
        {
            if ((int?)json["Version"] != Properties.Settings.Default.ForemanVersion ||
                (string?)json["Object"] != "ProductionGraph") {
                JObject? migrated = VersionUpdater.UpdateGraph(json, cache);
                if (migrated is null) //update failed
                    return new NewNodeCollection();
                json = migrated;
            }

            NewNodeCollection newNodeCollection = new NewNodeCollection();
            Dictionary<int, ReadOnlyBaseNode> oldNodeIndices = new Dictionary<int, ReadOnlyBaseNode>(); //the links between the node index (as imported) and the newly created node (which will now have a different index). Used to link up nodes

            try {
                //check compliance on all items, assemblers, modules, beacons, and recipes (data-cache will take care of it) - this means add in any missing objects and handle multi-name recipes (there can be multiple versions of a missing recipe, each with identical names)
                cache.ProcessImportedItemsSet(json["IncludedItems"]?.Select(t => (string?)t).OfType<string>() ?? []);
                var qualityLinks = cache.ProcessImportedQualitiesSet(json["IncludedQualities"]
                    ?.Select(j => (string?)j["Key"] is string key && (int?)j["Value"] is int value ? new KeyValuePair<string, int>(key, value) : (KeyValuePair<string, int>?)null).OfType<KeyValuePair<string, int>>() ?? []);
                cache.ProcessImportedAssemblersSet(json["IncludedAssemblers"]?.Select(t => (string?)t).OfType<string>() ?? []);
                cache.ProcessImportedModulesSet(json["IncludedModules"]?.Select(t => (string?)t).OfType<string>() ?? []);
                cache.ProcessImportedBeaconsSet(json["IncludedBeacons"]?.Select(t => (string?)t).OfType<string>() ?? []);
                Dictionary<long, Recipe> recipeLinks = cache.ProcessImportedRecipesSet(RecipeShort.GetSetFromJson(json["IncludedRecipes"]));
                Dictionary<long, PlantProcess> plantProcessLinks = cache.ProcessImportedPlantProcessesSet(PlantShort.GetSetFromJson(json["IncludedPlantProcesses"]));

                if (loadSolverValues) {
                    EnableExtraProductivityForNonMiners = (bool?)json["EnableExtraProductivityForNonMiners"] is true;
                    DefaultNodeDirection = (int?)json["DefaultNodeDirection"] is int i ? (NodeDirection)i : NodeDirection.Up;
                    PullOutputNodes = (bool?)json["Solver_PullOutputNodes"] is true;
                    PullOutputNodesPower = (double?)json["Solver_PullOutputNodesPower"] ?? default;
                    LowPriorityPower = (double?)json["Solver_LowPriorityPower"] ?? default;
                    MaxQualitySteps = (uint?)json["MaxQualitySteps"] ?? default;
                    DefaultAssemblerQuality = qualityLinks[(string?)json["DefaultQuality"] ?? "normal"];
                }

                //add in all the graph nodes
                foreach (JToken nodeJToken in json["Nodes"]?.AsEnumerable() ?? []) {
                    BaseNode? newNode = null;
                    string[] locationString = ((string?)nodeJToken["Location"])?.Split(',') ?? [];
                    Point location = new Point(int.Parse(locationString[0]), int.Parse(locationString[1]));
                    string? itemName = null; //just an early define
                    Quality? quality = null; //early define

                    if ((int?)nodeJToken["NodeType"] is not int nt)
                        continue;

                    switch ((NodeType)nt) {
                        case NodeType.Consumer:
                            itemName = (string?)nodeJToken["Item"];
                            if ((string?)nodeJToken["BaseQuality"] is string bq)
                                qualityLinks.TryGetValue(bq, out quality);
                            if (itemName is not null && quality is not null && cache.Items.TryGetValue(itemName, out var value))
                                newNode = roToNode[CreateConsumerNode(new ItemQualityPair(value, quality), location)];
                            else if (itemName is not null && quality is not null)
                                newNode = roToNode[CreateConsumerNode(new ItemQualityPair(cache.MissingItems[itemName], quality), location)];
                            if (newNode?.ReadOnlyNode is not null)
                                newNodeCollection.newNodes.Add(newNode.ReadOnlyNode);
                            break;
                        case NodeType.Supplier:
                            itemName = (string?)nodeJToken["Item"];
                            if ((string?)nodeJToken["BaseQuality"] is string bq2)
                                qualityLinks.TryGetValue(bq2, out quality);
                            if (itemName is not null && quality is not null && cache.Items.TryGetValue(itemName, out var value2))
                                newNode = roToNode[CreateSupplierNode(new ItemQualityPair(value2, quality), location)];
                            else if (itemName is not null && quality is not null)
                                newNode = roToNode[CreateSupplierNode(new ItemQualityPair(cache.MissingItems[itemName], quality), location)];
                            if (newNode?.ReadOnlyNode is not null)
                                newNodeCollection.newNodes.Add(newNode.ReadOnlyNode);
                            break;
                        case NodeType.Passthrough:
                            itemName = (string?)nodeJToken["Item"];
                            if ((string?)nodeJToken["BaseQuality"] is string bq3)
                                qualityLinks.TryGetValue(bq3, out quality);
                            if (itemName is not null && quality is not null && cache.Items.TryGetValue(itemName, out var value3))
                                newNode = roToNode[CreatePassthroughNode(new ItemQualityPair(value3, quality), location)];
                            else if (itemName is not null && quality is not null)
                                newNode = roToNode[CreatePassthroughNode(new ItemQualityPair(cache.MissingItems[itemName], quality), location)];
                            (newNode as PassthroughNode)?.SimpleDraw = (bool?)nodeJToken["SDraw"] is true;
                            if (newNode?.ReadOnlyNode is not null)
                                newNodeCollection.newNodes.Add(newNode.ReadOnlyNode);
                            break;
                        case NodeType.Spoil:
                            itemName = (string?)nodeJToken["InputItem"];
                            var outputItemName = (string?)nodeJToken["OutputItem"];
                            if ((string?)nodeJToken["BaseQuality"] is string bq4)
                                qualityLinks.TryGetValue(bq4, out quality);
                            var inputItem = itemName is not null ? (cache.Items.ContainsKey(itemName) ? cache.Items[itemName] : cache.MissingItems[itemName]) : default;
                            var outputItem = outputItemName is not null ? (cache.Items.ContainsKey(outputItemName) ? cache.Items[outputItemName] : cache.MissingItems[outputItemName]) : default;
                            if (inputItem is not null && quality is not null && outputItem is not null)
                                newNode = roToNode[CreateSpoilNode(new ItemQualityPair(inputItem, quality), outputItem, location)];
                            if (newNode?.ReadOnlyNode is not null)
                                newNodeCollection.newNodes.Add(newNode.ReadOnlyNode);
                            break;
                        case NodeType.Plant:
                            long pprocessID = (long?)nodeJToken["PlantProcessID"] ?? default;
                            if ((string?)nodeJToken["BaseQuality"] is string bq5)
                                qualityLinks.TryGetValue(bq5, out quality);
                            if (quality is not null)
                                newNode = roToNode[CreatePlantNode(plantProcessLinks[pprocessID], quality, location)];
                            if (newNode?.ReadOnlyNode is not null)
                                newNodeCollection.newNodes.Add(newNode.ReadOnlyNode);
                            break;
                        case NodeType.Recipe:
                            long recipeID = (long?)nodeJToken["RecipeID"] ?? default;
                            Quality? recipeQuality = null;
                            if ((string?)nodeJToken["RecipeQuality"] is string rq)
                                qualityLinks.TryGetValue(rq, out recipeQuality);
                            if (recipeQuality is not null)
                                newNode = roToNode[CreateRecipeNode(new RecipeQualityPair(recipeLinks[recipeID], recipeQuality), location, (rNode) => {
                                    RecipeNodeController rNodeController = (RecipeNodeController)rNode.Controller;

                                    rNode.LowPriority = (nodeJToken["LowPriority"] != null);

                                    rNode.NeighbourCount = (double?)nodeJToken["Neighbours"] ?? default;
                                    rNode.ExtraProductivityBonus = (double?)nodeJToken["ExtraProductivity"] ?? default;

                                    var assemblerName = (string?)nodeJToken["Assembler"];
                                    Quality? assemblerQuality = null;
                                    if ((string?)nodeJToken["AssemblerQuality"] is string aq)
                                        qualityLinks.TryGetValue(aq, out assemblerQuality);
                                    if (assemblerName is not null && assemblerQuality is not null && cache.Assemblers.TryGetValue(assemblerName, out var assembler))
                                        rNodeController.SetAssembler(new AssemblerQualityPair(assembler, assemblerQuality));
                                    else if (assemblerName is not null && assemblerQuality is not null && cache.MissingAssemblers.TryGetValue(assemblerName, out var assembler2))
                                        rNodeController.SetAssembler(new AssemblerQualityPair(assembler2, assemblerQuality));

                                    foreach (JToken module in nodeJToken["AssemblerModules"]?.AsEnumerable() ?? []) {
                                        var moduleName = (string?)module["Name"];
                                        Quality? moduleQuality = null;
                                        if ((string?)module["Quality"] is string quality)
                                            qualityLinks.TryGetValue(quality, out moduleQuality);
                                        if (moduleName is not null && moduleQuality is not null && cache.Modules.TryGetValue(moduleName, out var module2))
                                            rNodeController.AddAssemblerModule(new ModuleQualityPair(module2, moduleQuality));
                                        else if (moduleName is not null && moduleQuality is not null && cache.MissingModules.TryGetValue(moduleName, out module2))
                                            rNodeController.AddAssemblerModule(new ModuleQualityPair(module2, moduleQuality));
                                    }

                                    if (nodeJToken["Fuel"] != null) {
                                        var s = (string?)nodeJToken["Fuel"];
                                        if (s is not null && cache.Items.TryGetValue(s, out var item))
                                            rNodeController.SetFuel(item);
                                        else if (s is not null && cache.MissingItems.TryGetValue(s, out var item2))
                                            rNodeController.SetFuel(item2);
                                    } else if (rNode.SelectedAssembler.Assembler.IsBurner) //and fuel is null... well - its the import. set it as null (and consider it an error)
                                        rNodeController.SetFuel(null);

                                    if ((string?)nodeJToken["Burnt"] is string burntStr) {
                                        Item? burntItem;
                                        if (!cache.Items.TryGetValue(burntStr, out burntItem))
                                            cache.MissingItems.TryGetValue(burntStr, out burntItem);
                                        if (rNode.FuelRemains != burntItem)
                                            rNode.SetBurntOverride(burntItem);
                                    } else if (rNode.Fuel != null && rNode.Fuel.BurnResult != null) //same as above - there should be a burn result, but there isnt...
                                        rNode.SetBurntOverride(null);

                                    if ((string?)nodeJToken["Beacon"] is string beaconName) {
                                        Quality? beaconQuality = null;
                                        if ((string?)nodeJToken["BeaconQuality"] is string beaconQualityStr)
                                            qualityLinks.TryGetValue(beaconQualityStr, out beaconQuality);

                                        if (beaconQuality is not null && cache.Beacons.ContainsKey(beaconName))
                                            rNodeController.SetBeacon(new BeaconQualityPair(cache.Beacons[beaconName], beaconQuality));
                                        else if (beaconQuality is not null)
                                            rNodeController.SetBeacon(new BeaconQualityPair(cache.MissingBeacons[beaconName], beaconQuality));

                                        foreach (JToken module in nodeJToken["BeaconModules"]?.AsEnumerable() ?? []) {
                                            var moduleName = (string?)module["Name"];
                                            Quality? moduleQuality = null;
                                            if ((string?)module["Quality"] is string q)
                                                qualityLinks.TryGetValue(q, out moduleQuality);

                                            if (moduleName is not null && moduleQuality is not null && cache.Modules.TryGetValue(moduleName, out var module2))
                                                rNodeController.AddBeaconModule(new ModuleQualityPair(module2, moduleQuality));
                                            else if (moduleName is not null && moduleQuality is not null && cache.MissingModules.TryGetValue(moduleName, out module2))
                                                rNodeController.AddBeaconModule(new ModuleQualityPair(module2, moduleQuality));
                                        }

                                        rNode.BeaconCount = (double?)nodeJToken["BeaconCount"] ?? default;
                                        rNode.BeaconsPerAssembler = (double?)nodeJToken["BeaconsPerAssembler"] ?? default;
                                        rNode.BeaconsConst = (double?)nodeJToken["BeaconsConst"] ?? default;
                                    }

                                    if (rNode.ReadOnlyNode is not null)
                                        newNodeCollection.newNodes.Add(rNode.ReadOnlyNode); //done last, so as to catch any errors above first.
                                })];
                            break;
                        default:
                            throw new Exception(); //we will catch it right away and delete all nodes added in thus far. Error was most likely in json read, in which case we count it as a corrupt json and not import anything.
                    }

                    if ((int?)nodeJToken["RateType"] is int i)
                        newNode?.RateType = (RateType)i;
                    if (newNode?.RateType == RateType.Manual)
                        newNode.DesiredSetValue = (double?)nodeJToken["DesiredSetValue"] ?? default;

                    newNode?.NodeDirection = (int?)nodeJToken["Direction"] is int j ? (NodeDirection)j : NodeDirection.Up;

                    if ((string?)nodeJToken["KeyNode"] is string keyNode) {
                        newNode?.KeyNode = true;
                        newNode?.KeyNodeTitle = keyNode;
                    }

                    if ((int?)nodeJToken["NodeID"] is int nodeId && newNode?.ReadOnlyNode is not null)
                        oldNodeIndices.Add(nodeId, newNode.ReadOnlyNode);
                }

                //link the new nodes
                foreach (JToken nodeLinkJToken in json["NodeLinks"]?.AsEnumerable() ?? []) {
                    if ((int?)nodeLinkJToken["SupplierID"] is not int supplierId ||
                        (int?)nodeLinkJToken["ConsumerID"] is not int consumerId ||
                        (string?)nodeLinkJToken["Quality"] is not string qualityStr ||
                        (string?)nodeLinkJToken["Item"] is not string itemName)
                        continue;
                    ReadOnlyBaseNode supplier = oldNodeIndices[supplierId];
                    ReadOnlyBaseNode consumer = oldNodeIndices[consumerId];
                    ItemQualityPair item;
                    var quality = qualityLinks[qualityStr];
                    if (quality is null)
                        continue;

                    if (cache.Items.ContainsKey(itemName))
                        item = new ItemQualityPair(cache.Items[itemName], quality);
                    else
                        item = new ItemQualityPair(cache.MissingItems[itemName], quality);

                    if (LinkChecker.IsPossibleConnection(item, supplier, consumer)) //not necessary to test if connection is valid. It must be valid based on json
                        newNodeCollection.newLinks.Add(CreateLink(supplier, consumer, item));
                }
            } catch (Exception e) //there was something wrong with the json (probably someone edited it by hand and it didnt link properly). Delete all added nodes and return empty
              {
                ErrorLogging.LogLine(string.Format("Error loading nodes into producton graph! ERROR: {0}", e));
                Console.WriteLine(e);
                DeleteNodes(newNodeCollection.newNodes);
                return new NewNodeCollection();
            }
            return newNodeCollection;
        }
    }
}