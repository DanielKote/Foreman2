using Foreman.Models;
using Foreman.Models.Nodes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public enum RateType { Auto, Manual };
    public enum NodeState { Clean, MissingLink, Warning, Error }
    public enum NodeDirection { Up, Down }

    public abstract partial class BaseNode {
        public abstract BaseNodeController Controller { get; }
        public ProductionGraph MyGraph { get; }
        public int NodeID { get; }
        public bool IsClean { get; protected set; } //if true then this node hasnt changed (internal values or links) since last solver solution

        private bool keyNode;
        private string keyNodeTitle = "";

        public bool KeyNode {
            get => keyNode;
            set {
                if (keyNode == value)
                    return;
                keyNode = value;
                OnNodeStateChanged();
            }
        }

        public string KeyNodeTitle {
            get => keyNodeTitle;
            set {
                if (keyNodeTitle == value)
                    return;
                keyNodeTitle = value;
                OnNodeStateChanged();
            }
        }

        public Point Location { get; set; }

        private RateType rateType;
        public RateType RateType { get { return rateType; } set { if (rateType != value) { rateType = value; UpdateState(); } } }

        private NodeDirection nodeDirection;
        public NodeDirection NodeDirection { get { return nodeDirection; } set { if (nodeDirection != value) { nodeDirection = value; OnNodeStateChanged(); } } }

        public double ActualRatePerSec { get; private set; }

        private double desiredRatePerSec;
        public virtual double DesiredRatePerSec { get { return desiredRatePerSec; } set { if (desiredRatePerSec != value) { desiredRatePerSec = value; UpdateState(); } } }

        public double ActualRate { get { return ActualRatePerSec * MyGraph.GetRateMultipler(); } }
        public double DesiredRate { get { return DesiredRatePerSec * MyGraph.GetRateMultipler(); } set { DesiredRatePerSec = value / MyGraph.GetRateMultipler(); } }

        //'set value' values below are for flow setting - they are used to set the desired rate of the node regardless of what 'variable' they represent
        //ex: recipe nodes will use this to 'set' the number of assemblers, passthrough/source/sink nodes use this to 'set' flowrate, plant nodes set 'plant tiles' and spoil nodes set 'inventory stacks'
        //in its default form (below) its used for 'flowrate'
        public virtual double ActualSetValue { get { return ActualRate; } }
        public virtual double DesiredSetValue { get { return DesiredRate; } set { DesiredRate = value; } }
        public virtual double MaxDesiredSetValue { get { return ProductionGraph.MaxSetFlow; } }
        public virtual string SetValueDescription { get { return string.Format(DisplayCulture.Format, "Item Flowrate (per {0})", MyGraph.GetRateName()); } }

        public abstract IEnumerable<ItemQualityPair> Inputs { get; }
        public abstract IEnumerable<ItemQualityPair> Outputs { get; }

        public List<NodeLink> InputLinks { get; private set; }
        public List<NodeLink> OutputLinks { get; private set; }

        public NodeState State { get; protected set; }

        public event EventHandler<EventArgs>? NodeStateChanged; //includes node state, as well as any changes that may influence the input/output links (ex: switching fuel, assembler, etc.)
        public event EventHandler<EventArgs>? NodeValuesChanged; //includes actual amount / actual rate changes (ex: graph solved), as well as minor updates (ex:beacon numbers, etc.)

        internal BaseNode(ProductionGraph graph, int nodeID) {
            MyGraph = graph;
            NodeID = nodeID;

            KeyNode = false;
            KeyNodeTitle = "";

            rateType = RateType.Auto;
            nodeDirection = NodeDirection.Up;

            desiredRatePerSec = 0;
            Location = new Point(0, 0);

            InputLinks = [];
            OutputLinks = [];
        }

        public bool AllLinksValid { get { return (InputLinks.Count(l => !l.IsValid) + OutputLinks.Count(l => !l.IsValid) == 0); } }
        public bool AllLinksConnected { get { return !Inputs.Any(i => !InputLinks.Any(l => l.Item == i)) && !Outputs.Any(i => !OutputLinks.Any(l => l.Item == i)); } }

        public void UpdateState(bool makeDirty = true) {
            if (makeDirty)
                IsClean = false;
            NodeState oldState = State;
            State = GetUpdatedState();
            if (oldState != State)
                OnNodeStateChanged();
        }

        internal virtual NodeState GetUpdatedState() {
            return (AllLinksValid ? (AllLinksConnected ? NodeState.Clean : NodeState.MissingLink) : NodeState.Error);
        }

        protected virtual void OnNodeStateChanged() { NodeStateChanged?.Invoke(this, EventArgs.Empty); }
        protected virtual void OnNodeValuesChanged() { NodeValuesChanged?.Invoke(this, EventArgs.Empty); }

        public abstract double GetConsumeRate(ItemQualityPair item); //calculated rate a given item is consumed by this node (may not match desired amount)
        public abstract double GetSupplyRate(ItemQualityPair item); //calculated rate a given item is supplied by this note (may not match desired amount)

        public double GetSupplyUsedRate(ItemQualityPair item) {
            return (double)OutputLinks.Where(x => x.Item == item).Sum(x => x.Throughput);
        }

        public bool IsOverproducing() {
            foreach (ItemQualityPair item in Outputs)
                if (IsOverproducing(item))
                    return true;
            return false;
        }

        public bool IsOverproducing(ItemQualityPair item) {
            //supplied & produced > 1 ---> allow for 0.1% error
            //supplied & produced [0.0001 -> 1]  ---> allow for 1% error
            //supplied & produced [0 ->0.0001] ---> allow for any errors (as long as neither are 0)
            //supplied & produced = 0 ---> no errors if both are exactly 0

            double producedRate = GetSupplyRate(item);
            double supplyUsedRate = GetSupplyUsedRate(item);
            return (producedRate != 0 || supplyUsedRate != 0) && (producedRate >= 0.0001 || supplyUsedRate >= 0.0001) && (supplyUsedRate == 0 && producedRate != 0 || ((producedRate - supplyUsedRate) / supplyUsedRate) > ((producedRate > 1 && supplyUsedRate > 1) ? 0.001f : 0.01f));
        }

        public bool ManualRateNotMet() {
            return (RateType == RateType.Manual) && Math.Abs(ActualRatePerSec - DesiredRatePerSec) > 0.0001;
        }

        public virtual List<string> GetErrors() => [];
        public virtual List<string> GetWarnings() => [];

    }

    public abstract class BaseNodeController(BaseNode myNode) {
        private readonly BaseNode MyNode = myNode;

        public void SetKeyNode(bool keyNode) { MyNode.KeyNode = keyNode; MyNode.KeyNodeTitle = keyNode ? MyNode.NodeID.ToString(DisplayCulture.Format) : ""; }
        public void SetKeyNodeTitle(string title) { if (MyNode.KeyNode) MyNode.KeyNodeTitle = title; }

        public void SetLocation(Point location) { if (MyNode.Location != location) MyNode.Location = location; }

        public void SetRateType(RateType type) { if (MyNode.RateType != type) MyNode.RateType = type; }

        public void SetDesiredSetValue(double value) { if (MyNode.DesiredSetValue != value) MyNode.DesiredSetValue = value; MyNode.UpdateState(); }

        public void SetDirection(NodeDirection direction) { if (MyNode.NodeDirection != direction) MyNode.NodeDirection = direction; }

        public abstract Dictionary<string, Action> GetErrorResolutions();
        public virtual Dictionary<string, Action> GetWarningResolutions() => [];

        protected Dictionary<string, Action> ErrorResolutionsDeleteOrFixLinks(bool deleteNode) {
            return deleteNode ? new Dictionary<string, Action> { ["Delete node"] = Delete } : GetInvalidConnectionResolutions();
        }

        protected Dictionary<string, Action> ErrorResolutionsWithFixOrLinks(bool deleteNode, string? fixLabel, Action? fixAction) {
            var resolutions = new Dictionary<string, Action>();
            if (deleteNode)
                resolutions["Delete node"] = Delete;
            if (fixLabel is not null && fixAction is not null)
                resolutions[fixLabel] = fixAction;
            else
                foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
                    resolutions.Add(kvp.Key, kvp.Value);
            return resolutions;
        }

        protected Dictionary<string, Action> GetInvalidConnectionResolutions() {
            var resolutions = new Dictionary<string, Action>();
            if (!MyNode.AllLinksValid) {
                resolutions.Add("Delete invalid links", new Action(() => {
                    foreach (NodeLink invalidLink in MyNode.InputLinks.Where(l => !l.IsValid).ToList())
                        MyNode.MyGraph.DeleteLink(invalidLink);
                    foreach (NodeLink invalidLink in MyNode.OutputLinks.Where(l => !l.IsValid).ToList())
                        MyNode.MyGraph.DeleteLink(invalidLink);
                }));
            }
            return resolutions;
        }

        public void Delete() => MyNode.MyGraph.DeleteNode(MyNode);
        public override string ToString() => "C: " + MyNode.ToString();
    }
}
