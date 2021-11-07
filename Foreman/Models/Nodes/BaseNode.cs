using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public enum RateType { Auto, Manual };
	public enum NodeState { Clean, MissingLink, Warning, Error }

	[Serializable]
	public abstract partial class BaseNode : ISerializable
	{
		public abstract BaseNodeController Controller { get; }
		public ReadOnlyBaseNode ReadOnlyNode { get; protected set; }
		public readonly ProductionGraph MyGraph;
		public readonly int NodeID;

		public Point Location { get; set; }

		private RateType rateType;
		public RateType RateType { get { return rateType; } set { if (rateType != value) { rateType = value; UpdateState(); } } }

		public bool IsFlipped { get; set; }

		public double ActualRatePerSec { get; private set; }

		private double desiredRatePerSec;
		public virtual double DesiredRatePerSec { get { return desiredRatePerSec; } set { if (desiredRatePerSec != value) { desiredRatePerSec = value; UpdateState(); } } }

		public double ActualRate { get { return ActualRatePerSec * MyGraph.GetRateMultipler(); } }
		public double DesiredRate { get { return DesiredRatePerSec * MyGraph.GetRateMultipler(); } set { DesiredRatePerSec = value / MyGraph.GetRateMultipler(); } }

		public abstract IEnumerable<Item> Inputs { get; }
		public abstract IEnumerable<Item> Outputs { get; }

		public List<NodeLink> InputLinks { get; private set; }
		public List<NodeLink> OutputLinks { get; private set; }

		public NodeState State { get; protected set; }

		public event EventHandler<EventArgs> NodeStateChanged; //includes node state, as well as any changes that may influence the input/output links (ex: switching fuel, assembler, etc.)
		public event EventHandler<EventArgs> NodeValuesChanged; //includes actual amount / actual rate changes (ex: graph solved), as well as minor updates (ex:beacon numbers, etc.)

		internal BaseNode(ProductionGraph graph, int nodeID)
		{
			MyGraph = graph;
			NodeID = nodeID;

			rateType = RateType.Auto;
			desiredRatePerSec = 0;
			Location = new Point(0, 0);

			InputLinks = new List<NodeLink>();
			OutputLinks = new List<NodeLink>();
		}

		public bool AllLinksValid { get { return (InputLinks.Count(l => !l.IsValid) + OutputLinks.Count(l => !l.IsValid) == 0); } }
		public bool AllLinksConnected { get { return !Inputs.Any(i => !InputLinks.Any(l => l.Item == i)) && !Outputs.Any(i => !OutputLinks.Any(l => l.Item == i)); } }

		public virtual void UpdateState()
		{
			NodeState originalState = State;
			State = AllLinksValid ? AllLinksConnected? NodeState.Clean : NodeState.MissingLink : NodeState.Error;
			if (State != originalState)
				OnNodeStateChanged();
		}

		protected virtual void OnNodeStateChanged() { NodeStateChanged?.Invoke(this, EventArgs.Empty); }
		protected virtual void OnNodeValuesChanged() { NodeValuesChanged?.Invoke(this, EventArgs.Empty); }

		public abstract double GetConsumeRate(Item item); //calculated rate a given item is consumed by this node (may not match desired amount)
		public abstract double GetSupplyRate(Item item); //calculated rate a given item is supplied by this note (may not match desired amount)

		public double GetSuppliedRate(Item item)
		{
			return (double)InputLinks.Where(x => x.Item == item).Sum(x => x.Throughput);
		}

		public bool IsOversupplied()
		{
			foreach (Item item in Inputs)
				if (IsOversupplied(item))
					return true;
			return false;
		}

		public bool IsOversupplied(Item item)
		{
			//supply & consume > 1 ---> allow for 0.1% error
			//supply & consume [0.001 -> 1]  ---> allow for 1% error
			//supply & consume [0 ->0.001] ---> allow for any errors (as long as neither are 0)
			//supply & consume = 0 ---> no errors if both are exactly 0

			double consumeRate = GetConsumeRate(item);
			double supplyRate = GetSuppliedRate(item);
			if ((consumeRate == 0 && supplyRate == 0) || (supplyRate < 0.001 && supplyRate < 0.001))
				return false;
			return ((supplyRate - consumeRate) / supplyRate) > ((consumeRate > 1 && supplyRate > 1) ? 0.001f : 0.01f);
		}

		public bool ManualRateNotMet()
		{
			return (RateType == RateType.Manual) && Math.Abs(ActualRatePerSec - DesiredRatePerSec) > 0.0001;
		}

		public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
	}

	public abstract class ReadOnlyBaseNode
	{
		public int NodeID => MyNode.NodeID;
		public Point Location => MyNode.Location;
		public IEnumerable<Item> Inputs => MyNode.Inputs;
		public IEnumerable<Item> Outputs => MyNode.Outputs;

		public IEnumerable<ReadOnlyNodeLink> InputLinks { get { foreach (NodeLink nodeLink in MyNode.InputLinks) yield return nodeLink.ReadOnlyLink; } }
		public IEnumerable<ReadOnlyNodeLink> OutputLinks { get { foreach (NodeLink nodeLink in MyNode.OutputLinks) yield return nodeLink.ReadOnlyLink; } }

		public RateType RateType => MyNode.RateType;
		public double ActualRate => MyNode.ActualRate;
		public double DesiredRate => MyNode.DesiredRate;
		public NodeState State => MyNode.State;

		public bool IsFlipped => MyNode.IsFlipped;

		public abstract List<string> GetErrors();
		public abstract List<string> GetWarnings();

		public double GetConsumeRate(Item item) => MyNode.GetConsumeRate(item);
		public double GetSupplyRate(Item item) => MyNode.GetSupplyRate(item);
		public double GetSuppliedRate(Item item) => MyNode.GetSuppliedRate(item);
		public bool IsOversupplied() => MyNode.IsOversupplied();
		public bool IsOversupplied(Item item) => MyNode.IsOversupplied(item);
		public bool ManualRateNotMet() => MyNode.ManualRateNotMet();

		private readonly BaseNode MyNode;

		public event EventHandler<EventArgs> NodeStateChanged;
		public event EventHandler<EventArgs> NodeValuesChanged;

		public ReadOnlyBaseNode(BaseNode node)
		{
			MyNode = node;
			MyNode.NodeStateChanged += Node_NodeStateChanged;
			MyNode.NodeValuesChanged += Node_NodeValuesChanged;
		}

		private void Node_NodeStateChanged(object sender, EventArgs e) { NodeStateChanged?.Invoke(this, EventArgs.Empty); }
		private void Node_NodeValuesChanged(object sender, EventArgs e) { NodeValuesChanged?.Invoke(this, EventArgs.Empty); }

		public override string ToString() { return "RO: " + MyNode.ToString(); }
	}


	public abstract class BaseNodeController
	{
		private readonly BaseNode MyNode;

		protected BaseNodeController(BaseNode myNode)
		{
			MyNode = myNode;
		}

		public void SetLocation(Point location) { if(MyNode.Location != location) MyNode.Location = location; }

		public void SetRateType(RateType type) { if (MyNode.RateType != type) MyNode.RateType = type; }
		public virtual void SetDesiredRate(double rate) { if (MyNode.DesiredRate != rate) MyNode.DesiredRate = rate; }

		public abstract Dictionary<string, Action> GetErrorResolutions();
		public abstract Dictionary<string, Action> GetWarningResolutions();

		protected Dictionary<string, Action> GetInvalidConnectionResolutions()
		{
			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
			if (!MyNode.AllLinksValid)
			{
				resolutions.Add("Delete invalid links", new Action(() =>
				{
					foreach (NodeLink invalidLink in MyNode.InputLinks.Where(l => !l.IsValid).ToList())
						MyNode.MyGraph.DeleteLink(invalidLink.ReadOnlyLink);
					foreach (NodeLink invalidLink in MyNode.OutputLinks.Where(l => !l.IsValid).ToList())
						MyNode.MyGraph.DeleteLink(invalidLink.ReadOnlyLink);
				}));
			}
			return resolutions;
		}

		public void Delete() { MyNode.MyGraph.DeleteNode(MyNode.ReadOnlyNode); }
		public override string ToString() { return "C: " + MyNode.ToString(); }
	}
}
