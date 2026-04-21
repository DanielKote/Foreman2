using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman {
    public enum RateType {
        Auto,
        Manual
    };

    public enum NodeState {
        Clean,
        MissingLink,
        Warning,
        Error
    }

    public enum NodeDirection {
        Up,
        Down
    }

    [Serializable]
    public abstract partial class BaseNode : ISerializable {
        public abstract BaseNodeController Controller { get; }
        public ReadOnlyBaseNode ReadOnlyNode { get; protected set; }
        public readonly ProductionGraph MyGraph;
        public readonly int NodeId;

        // if true then this node hasn't changed (internal values or links) since last solver solution
        public bool IsClean { get; protected set; }

        public bool KeyNode { get; set; }
        public string KeyNodeTitle { get; set; }

        public Point Location { get; set; }

        private RateType _rateType;

        public RateType RateType {
            get => _rateType;
            set {
                if (_rateType == value)
                    return;

                _rateType = value;
                UpdateState();
            }
        }

        private NodeDirection _nodeDirection;

        public NodeDirection NodeDirection {
            get => _nodeDirection;
            set {
                if (_nodeDirection == value)
                    return;

                _nodeDirection = value;
                OnNodeStateChanged();
            }
        }

        public double ActualRatePerSec { get; private set; }

        private double _desiredRatePerSec;

        public virtual double DesiredRatePerSec {
            get => _desiredRatePerSec;
            set {
                if (Math.Abs(_desiredRatePerSec - value) < double.Epsilon)
                    return;

                _desiredRatePerSec = value;
                UpdateState();
            }
        }

        public double ActualRate => ActualRatePerSec * MyGraph.GetRateMultiplier();

        public double DesiredRate {
            get => DesiredRatePerSec * MyGraph.GetRateMultiplier();
            set => DesiredRatePerSec = value / MyGraph.GetRateMultiplier();
        }

        // 'set value' values below are for flow setting - they are used to set the desired rate of the node regardless of what 'variable' they represent
        // ex: recipe nodes will use this to 'set' the number of assemblers, passthrough/source/sink nodes use this to 'set' flowrate,
        // plant nodes set 'plant tiles' and spoil nodes set 'inventory stacks'
        // in its default form (below) it's used for 'flowrate'
        public virtual double ActualSetValue => ActualRate;

        public virtual double DesiredSetValue {
            get => DesiredRate;
            set => DesiredRate = value;
        }

        public virtual double MaxDesiredSetValue => ProductionGraph.MaxSetFlow;

        public virtual string SetValueDescription => $"Item Flowrate (per {MyGraph.GetRateName()})";

        public abstract IEnumerable<ItemQualityPair> Inputs { get; }
        public abstract IEnumerable<ItemQualityPair> Outputs { get; }

        public List<NodeLink> InputLinks { get; private set; }
        public List<NodeLink> OutputLinks { get; private set; }

        public NodeState State { get; protected set; }

        // includes node state, as well as any changes that may influence the input/output links (ex: switching fuel, assembler, etc.)
        public event EventHandler<EventArgs> NodeStateChanged;

        // includes actual amount / actual rate changes (ex: graph solved), as well as minor updates (ex:beacon numbers, etc.)
        public event EventHandler<EventArgs> NodeValuesChanged;

        internal BaseNode(ProductionGraph graph, int nodeId) {
            MyGraph = graph;
            NodeId = nodeId;

            KeyNode = false;
            KeyNodeTitle = "";

            _rateType = RateType.Auto;
            _nodeDirection = NodeDirection.Up;

            _desiredRatePerSec = 0;
            Location = new Point(0, 0);

            InputLinks = [];
            OutputLinks = [];
        }

        public bool AllLinksValid {
            get { return InputLinks.Count(l => !l.IsValid) + OutputLinks.Count(l => !l.IsValid) == 0; }
        }

        public bool AllLinksConnected {
            get {
                return !Inputs.Any(i => InputLinks.All(l => l.Item != i))
                    && !Outputs.Any(i => OutputLinks.All(l => l.Item != i));
            }
        }

        public void UpdateState(bool makeDirty = true) {
            if (makeDirty)
                IsClean = false;
            var oldState = State;
            State = GetUpdatedState();
            if (oldState != State)
                OnNodeStateChanged();
        }

        internal virtual NodeState GetUpdatedState() {
            return AllLinksValid ? AllLinksConnected ? NodeState.Clean : NodeState.MissingLink : NodeState.Error;
        }

        protected void OnNodeStateChanged() {
            NodeStateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void OnNodeValuesChanged() {
            NodeValuesChanged?.Invoke(this, EventArgs.Empty);
        }

        // calculated rate a given item is consumed by this node (may not match desired amount)
        public abstract double GetConsumeRate(ItemQualityPair item);
        // calculated rate a given item is supplied by this note (may not match desired amount)
        public abstract double GetSupplyRate(ItemQualityPair item);

        public double GetSupplyUsedRate(ItemQualityPair item) {
            return OutputLinks.Where(x => x.Item == item).Sum(x => x.Throughput);
        }

        public bool IsOverproducing() {
            return Outputs.Any(IsOverproducing);
        }

        // supplied & produced > 1 ---> allow for 0.1% error
        // supplied & produced [0.0001 -> 1]  ---> allow for 1% error
        // supplied & produced [0 ->0.0001] ---> allow for any errors (as long as neither are 0)
        // supplied & produced = 0 ---> no errors if both are exactly 0
        public bool IsOverproducing(ItemQualityPair item) {
            var producedRate = GetSupplyRate(item);
            var supplyUsedRate = GetSupplyUsedRate(item);
            if ((producedRate == 0 && supplyUsedRate == 0) || (producedRate < 0.0001 && supplyUsedRate < 0.0001))
                return false;
            if (supplyUsedRate == 0 && producedRate != 0)
                return true;
            return (producedRate - supplyUsedRate) / supplyUsedRate > (producedRate > 1 && supplyUsedRate > 1 ? 0.001f : 0.01f);
        }

        public bool ManualRateNotMet() {
            return RateType == RateType.Manual && Math.Abs(ActualRatePerSec - DesiredRatePerSec) > 0.0001;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("NodeId", NodeId);
            info.AddValue("Location", Location);
            info.AddValue("RateType", RateType);
            info.AddValue("Direction", NodeDirection);

            if (RateType == RateType.Manual)
                info.AddValue("DesiredSetValue", DesiredSetValue);
            if (KeyNode)
                info.AddValue("KeyNode", KeyNodeTitle);
        }
    }

    public abstract class ReadOnlyBaseNode {
        public int NodeId => _myNode.NodeId;
        public Point Location => _myNode.Location;

        public bool KeyNode => _myNode.KeyNode;
        public string KeyNodeTitle => _myNode.KeyNodeTitle;

        public IEnumerable<ItemQualityPair> Inputs => _myNode.Inputs;
        public IEnumerable<ItemQualityPair> Outputs => _myNode.Outputs;

        public IEnumerable<ReadOnlyNodeLink> InputLinks {
            get { return _myNode.InputLinks.Select(nodeLink => nodeLink.ReadOnlyLink); }
        }

        public IEnumerable<ReadOnlyNodeLink> OutputLinks {
            get { return _myNode.OutputLinks.Select(nodeLink => nodeLink.ReadOnlyLink); }
        }

        public RateType RateType => _myNode.RateType;
        public double ActualRate => _myNode.ActualRate;
        public double ActualRatePerSec => _myNode.ActualRatePerSec;
        public double DesiredRate => _myNode.DesiredRate;
        public NodeState State => _myNode.State;

        public double ActualSetValue => _myNode.ActualSetValue;
        public double DesiredSetValue => _myNode.DesiredSetValue;
        public double MaxDesiredSetValue => _myNode.MaxDesiredSetValue;
        public string SetValueDescription => _myNode.SetValueDescription;

        public NodeDirection NodeDirection => _myNode.NodeDirection;

        public abstract List<string> GetErrors();
        public abstract List<string> GetWarnings();

        public double GetConsumeRate(ItemQualityPair item) => _myNode.GetConsumeRate(item);
        public double GetSupplyRate(ItemQualityPair item) => _myNode.GetSupplyRate(item);
        public double GetSupplyUsedRate(ItemQualityPair item) => _myNode.GetSupplyUsedRate(item);
        public bool IsOverproducing() => _myNode.IsOverproducing();
        public bool IsOverproducing(ItemQualityPair item) => _myNode.IsOverproducing(item);
        public bool ManualRateNotMet() => _myNode.ManualRateNotMet();

        private readonly BaseNode _myNode;

        public event EventHandler<EventArgs> NodeStateChanged;
        public event EventHandler<EventArgs> NodeValuesChanged;

        public ReadOnlyBaseNode(BaseNode node) {
            _myNode = node;
            _myNode.NodeStateChanged += Node_NodeStateChanged;
            _myNode.NodeValuesChanged += Node_NodeValuesChanged;
        }

        private void Node_NodeStateChanged(object sender, EventArgs e) {
            NodeStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Node_NodeValuesChanged(object sender, EventArgs e) {
            NodeValuesChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString() {
            return "RO: " + _myNode;
        }
    }


    public abstract class BaseNodeController(BaseNode myNode) {
        public void SetKeyNode(bool keyNode) {
            myNode.KeyNode = keyNode;
            myNode.KeyNodeTitle = keyNode ? myNode.NodeId.ToString() : "";
        }

        public void SetKeyNodeTitle(string title) {
            if (myNode.KeyNode) myNode.KeyNodeTitle = title;
        }

        public void SetLocation(Point location) {
            if (myNode.Location != location) myNode.Location = location;
        }

        public void SetRateType(RateType type) {
            if (myNode.RateType != type) myNode.RateType = type;
        }

        public void SetDesiredSetValue(double value) {
            if (Math.Abs(myNode.DesiredSetValue - value) > double.Epsilon)
                myNode.DesiredSetValue = value;
            myNode.UpdateState();
        }

        public void SetDirection(NodeDirection direction) {
            if (myNode.NodeDirection != direction) myNode.NodeDirection = direction;
        }

        public abstract Dictionary<string, Action> GetErrorResolutions();
        public abstract Dictionary<string, Action> GetWarningResolutions();

        protected Dictionary<string, Action> GetInvalidConnectionResolutions() {
            var resolutions = new Dictionary<string, Action>();
            if (!myNode.AllLinksValid) {
                resolutions.Add("Delete invalid links", () => {
                    foreach (var invalidLink in myNode.InputLinks.Where(l => !l.IsValid).ToList())
                        myNode.MyGraph.DeleteLink(invalidLink.ReadOnlyLink);
                    foreach (var invalidLink in myNode.OutputLinks.Where(l => !l.IsValid).ToList())
                        myNode.MyGraph.DeleteLink(invalidLink.ReadOnlyLink);
                });
            }

            return resolutions;
        }

        public void Delete() {
            myNode.MyGraph.DeleteNode(myNode.ReadOnlyNode);
        }

        public override string ToString() {
            return "C: " + myNode;
        }
    }
}