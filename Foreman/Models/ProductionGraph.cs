using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using Foreman.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public enum NodeType { Supplier, Consumer, Passthrough, Recipe, Spoil, Plant }
    public enum LinkType { Input, Output }

    public class NodeEventArgs(BaseNode node) : EventArgs {
        public BaseNode Node { get; } = node;
    }
    public class NodeLinkEventArgs(NodeLink link) : EventArgs {
        public NodeLink Link { get; } = link;
    }

    public partial class ProductionGraph {
        public class NewNodeBatch {
            public List<BaseNode> NewNodes { get; } = [];
            public List<NodeLink> NewLinks { get; } = [];
        }

        //public DataCache DCache { get; private set; }

        public enum RateUnit { Per1Sec, Per1Min, Per5Min, Per10Min, Per30Min, Per1Hour };//, Per6Hour, Per12Hour, Per24Hour }
        public static readonly string[] RateUnitNames = ["1 sec", "1 min", "5 min", "10 min", "30 min", "1 hour"]; //, "6 hours", "12 hours", "24 hours" };
        private static readonly float[] RateMultiplier = [1f, 60f, 300f, 600f, 1800f, 3600f]; //, 21600f, 43200f, 86400f };

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

        public IEnumerable<BaseNode> Nodes => nodes;
        public IEnumerable<NodeLink> NodeLinks => nodeLinks;
        public HashSet<int>? SerializeNodeIdSet { get; set; } //if this isnt null then the serialized production graph will only contain these nodes (and links between them)

        internal (IReadOnlyCollection<BaseNode> nodes, IReadOnlyCollection<NodeLink> links) GetFragmentForSerialization() {
            if (SerializeNodeIdSet is null)
                return (nodes, nodeLinks);
            var includedNodes = new HashSet<BaseNode>(nodes.Where(node => SerializeNodeIdSet.Contains(node.NodeID)));
            var includedLinks = new HashSet<NodeLink>();
            foreach (NodeLink link in nodeLinks) {
                if (includedNodes.Contains(link.ConsumerNode) && includedNodes.Contains(link.SupplierNode))
                    includedLinks.Add(link);
            }
            return (includedNodes, includedLinks);
        }

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

        public IQuality? DefaultAssemblerQuality { get; set; }

        public event EventHandler<NodeEventArgs>? NodeAdded;
        public event EventHandler<NodeEventArgs>? NodeDeleted;
        public event EventHandler<NodeLinkEventArgs>? LinkAdded;
        public event EventHandler<NodeLinkEventArgs>? LinkDeleted;
        public event EventHandler<EventArgs>? NodeValuesUpdated;
        public event EventHandler? GraphCleared;

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

        private readonly HashSet<BaseNode> nodes;
        private readonly HashSet<NodeLink> nodeLinks;
        private int lastNodeID;

        public ProductionGraph() {
            DefaultNodeDirection = NodeDirection.Up;
            PullOutputNodes = false;
            PullOutputNodesPower = 10;
            LowPriorityPower = 1e5;

            nodes = [];
            nodeLinks = [];
            lastNodeID = 0;

            AssemblerSelector = new AssemblerSelector();
            ModuleSelector = new ModuleSelector();
            FuelSelector = new FuelSelector();
        }

        public BaseNodeController? RequestNodeController(BaseNode node) => nodes.Contains(node) ? node.Controller : null;

        private BaseNode SetupNodeOfType(BaseNode node, Point location) {
            node.Location = location;
            node.NodeDirection = DefaultNodeDirection;
            nodes.Add(node);
            node.UpdateState();
            NodeAdded?.Invoke(this, new NodeEventArgs(node));
            return node;
        }

        public ConsumerNode CreateConsumerNode(ItemQualityPair item, Point location) =>
            (ConsumerNode)SetupNodeOfType(new ConsumerNode(this, lastNodeID++, item), location);

        public SupplierNode CreateSupplierNode(ItemQualityPair item, Point location) =>
            (SupplierNode)SetupNodeOfType(new SupplierNode(this, lastNodeID++, item), location);

        public PassthroughNode CreatePassthroughNode(ItemQualityPair item, Point location) =>
            (PassthroughNode)SetupNodeOfType(new PassthroughNode(this, lastNodeID++, item), location);

        public SpoilNode CreateSpoilNode(ItemQualityPair inputItem, IItem outputItem, Point location) =>
            (SpoilNode)SetupNodeOfType(new SpoilNode(this, lastNodeID++, inputItem, outputItem), location);

        public PlantNode CreatePlantNode(IPlantProcess plantProcess, IQuality quality, Point location) =>
            (PlantNode)SetupNodeOfType(new PlantNode(this, lastNodeID++, plantProcess, quality), location);

        public RecipeNode CreateRecipeNode(RecipeQualityPair recipe, Point location) =>
            CreateRecipeNode(recipe, location, null);

        private RecipeNode CreateRecipeNode(RecipeQualityPair recipe, Point location, Action<RecipeNode>? nodeSetupAction) {
            if (DefaultAssemblerQuality is null)
                throw new InvalidOperationException(nameof(DefaultAssemblerQuality));
            var node = new RecipeNode(this, lastNodeID++, recipe, DefaultAssemblerQuality) {
                Location = location,
                NodeDirection = DefaultNodeDirection
            };
            nodeSetupAction?.Invoke(node);
            if (nodeSetupAction == null) {
                var rnController = (RecipeNodeController)node.Controller;
                rnController.AutoSetAssembler();
                rnController.AutoSetAssemblerModules();
            }
            nodes.Add(node);
            node.UpdateInputsAndOutputs();
            NodeAdded?.Invoke(this, new NodeEventArgs(node));
            return node;
        }

        public NodeLink CreateLink(BaseNode supplier, BaseNode consumer, ItemQualityPair item) {
            if (!nodes.Contains(supplier) || !nodes.Contains(consumer) || !supplier.Outputs.Contains(item) || !consumer.Inputs.Contains(item))
                Trace.Fail(string.Format(CultureInfo.InvariantCulture, "Node link creation called with invalid parameters! consumer:{0}. supplier:{1}. item:{2}.", consumer, supplier, item));
            if (supplier.OutputLinks.Any(l => l.Item == item && l.ConsumerNode == consumer))
                return supplier.OutputLinks.First(l => l.Item == item && l.ConsumerNode == consumer);

            var link = new NodeLink(this, supplier, consumer, item);
            supplier.OutputLinks.Add(link);
            consumer.InputLinks.Add(link);
            LinkChangeUpdateImpactedNodeStates(link, LinkType.Input);
            LinkChangeUpdateImpactedNodeStates(link, LinkType.Output);

            nodeLinks.Add(link);
            LinkAdded?.Invoke(this, new NodeLinkEventArgs(link));
            return link;
        }

        public void DeleteNode(BaseNode node) {
            if (!nodes.Contains(node))
                Trace.Fail(string.Format(CultureInfo.InvariantCulture, "Node deletion called on a node ({0}) that isnt part of the graph!", node));

            foreach (NodeLink link in node.InputLinks.ToList())
                DeleteLink(link);
            foreach (NodeLink link in node.OutputLinks.ToList())
                DeleteLink(link);

            nodes.Remove(node);
            NodeDeleted?.Invoke(this, new NodeEventArgs(node));
        }

        public void DeleteNodes(IEnumerable<BaseNode> nodesToDelete) {
            foreach (BaseNode node in nodesToDelete.ToList())
                DeleteNode(node);
        }

        public void DeleteLink(NodeLink link) {
            if (!nodeLinks.Contains(link))
                Trace.Fail(string.Format(CultureInfo.InvariantCulture, "Link deletion called with a link that isnt part of the graph!"));

            link.ConsumerNode.InputLinks.Remove(link);
            link.SupplierNode.OutputLinks.Remove(link);
            LinkChangeUpdateImpactedNodeStates(link, LinkType.Input);
            LinkChangeUpdateImpactedNodeStates(link, LinkType.Output);

            nodeLinks.Remove(link);
            LinkDeleted?.Invoke(this, new NodeLinkEventArgs(link));
        }

        public void ClearGraph() {
            foreach (BaseNode node in nodes.ToList())
                DeleteNode(node);

            SerializeNodeIdSet = null;
            lastNodeID = 0;
            GraphCleared?.Invoke(this, EventArgs.Empty);
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

        public IEnumerable<BaseNode> GetSuppliers(ItemQualityPair item) {
            foreach (BaseNode node in Nodes)
                if (node.Outputs.Contains(item))
                    yield return node;
        }

        public IEnumerable<BaseNode> GetConsumers(ItemQualityPair item) {
            foreach (BaseNode node in Nodes)
                if (node.Inputs.Contains(item))
                    yield return node;
        }

        public IEnumerable<IEnumerable<BaseNode>> GetConnectedNodeGroups(bool includeCleanComponents) =>
            GetConnectedComponents(includeCleanComponents);

        private List<HashSet<BaseNode>> GetConnectedComponents(bool includeCleanComponents) //used to break the graph into groups (in case there are multiple disconnected groups) for simpler solving. Clean components refer to node groups where all the nodes inside the group havent had any changes since last solve operation
        {
            //there is an optimized solution for connected components where we keep track of the various groups and modify them as each node/link is added/removed, but testing shows that this calculation below takes under 1ms even for larg 1000+ node graphs, so why bother.


            HashSet<BaseNode> unvisitedNodes = [.. nodes];

            List<HashSet<BaseNode>> connectedComponents = [];

            while (unvisitedNodes.Count > 0) {
                HashSet<BaseNode> newSet = [];
                bool allClean = true;

                HashSet<BaseNode> toVisitNext = [unvisitedNodes.First()];

                while (toVisitNext.Count > 0) {
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
                try { OptimizeGraphNodeValues(); } catch (OverflowException ex) {
                    ErrorLogging.LogException(ex, "OptimizeGraphNodeValues overflow");
                }
            }
            NodeValuesUpdated?.Invoke(this, EventArgs.Empty); //called even if no changes have been made in order to re-draw the graph (since something required a node value update - link deletion? node addition? whatever)
        }

        private static void LinkChangeUpdateImpactedNodeStates(NodeLink link, LinkType direction) //helper function to update all the impacted nodes after addition/removal of a given link. Basically we want to update any node connected to this link through passthrough nodes (or directly).
                {
            var visitedLinks = new HashSet<NodeLink>(); //to prevent a loop
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

        public NewNodeBatch InsertNodesFromDocument(
            DataCache cache,
            ProductionGraphSaveDocument document,
            bool applySolverSettings) =>
            GraphSaveLoader.LoadProductionGraph(this, cache, document, applySolverSettings);

        public NewNodeBatch InsertNodesFromFragment(DataCache cache, string json, bool applySolverSettings) {
            ProductionGraphSaveDocument? document = GraphSaveCodec.ReadGraphPayload(json);
            return document is null ? new NewNodeBatch() : InsertNodesFromDocument(cache, document, applySolverSettings);
        }

        internal RecipeNode CreateRecipeNodeWithSetup(
            RecipeQualityPair recipe,
            Point location,
            Action<RecipeNode>? nodeSetupAction) =>
            CreateRecipeNode(recipe, location, nodeSetupAction);
    }
}
