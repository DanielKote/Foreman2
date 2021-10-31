using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public enum RateType { Auto, Manual };
	public enum NodeState { Clean, Warning, Error }

	[Serializable]
	public abstract partial class BaseNode
	{
		public abstract BaseNodeController Controller { get; }
		public ReadOnlyBaseNode ReadOnlyNode { get; protected set; }
		public readonly ProductionGraph MyGraph;
		public readonly int NodeID;

		public Point Location { get; set; }

		public RateType RateType { get; set; }
		public double ActualRatePerSec { get; private set; }
		public virtual double DesiredRatePerSec { get; set; }
		public double ActualRate { get { return ActualRatePerSec * MyGraph.GetRateMultipler(); } }
		public double DesiredRate { get { return ActualRatePerSec * MyGraph.GetRateMultipler(); } }

		public abstract IEnumerable<Item> Inputs { get; }
		public abstract IEnumerable<Item> Outputs { get; }

		public List<NodeLink> InputLinks { get; private set; }
		public List<NodeLink> OutputLinks { get; private set; }

		public NodeState State { get; protected set; }

		public event EventHandler<EventArgs> NodeStateChanged;

		internal BaseNode(ProductionGraph graph, int nodeID)
		{
			MyGraph = graph;
			NodeID = nodeID;

			RateType = RateType.Auto;
			DesiredRatePerSec = 0;
			Location = new Point(0, 0);

			InputLinks = new List<NodeLink>();
			OutputLinks = new List<NodeLink>();

			MyGraph.GraphOptionsChanged += Graph_OnOptionsChanged;
			UpdateState();
		}

		public bool AllLinksValid { get { return (InputLinks.Count(l => !l.IsValid) + OutputLinks.Count(l => !l.IsValid) == 0); } }

		public virtual bool UpdateState()
		{
			if (State == NodeState.Clean && !AllLinksValid)
			{
				State = NodeState.Error;
				return true;
			}
			return false;
		}

		private void Graph_OnOptionsChanged(object graph, EventArgs e)
		{
			if (UpdateState())
				OnNodeStateChanged();
		}

		protected virtual void OnNodeStateChanged() { NodeStateChanged?.Invoke(this, EventArgs.Empty); }

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
			//supply & consume [0.001 -> 1]  ---> allow for 2% error
			//supply & consume [0 ->0.001] ---> allow for any errors (as long as neither are 0)
			//supply & consume = 0 ---> no errors if both are exactly 0

			double consumeRate = GetConsumeRate(item);
			double supplyRate = GetSuppliedRate(item);
			if ((consumeRate == 0 && supplyRate == 0) || (supplyRate < 0.001 && supplyRate < 0.001))
				return false;
			return ((supplyRate - consumeRate) / supplyRate) > ((consumeRate > 1 && supplyRate > 1) ? 0.001f : 0.05f);
		}

		public bool ManualRateNotMet()
		{
			return (RateType == RateType.Manual) && Math.Abs(ActualRatePerSec - DesiredRatePerSec) > 0.0001;
		}

		public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
	}

	public class ReadOnlyBaseNode
	{
		public int NodeID { get { return MyNode.NodeID; } }
		public Point Location { get { return MyNode.Location; } }
		public IEnumerable<Item> Inputs { get { return MyNode.Inputs; } }
		public IEnumerable<Item> Outputs { get { return MyNode.Outputs; } }

		public IEnumerable<ReadOnlyNodeLink> InputLinks { get { foreach (NodeLink nodeLink in MyNode.InputLinks) yield return nodeLink.ReadOnlyLink; } }
		public IEnumerable<ReadOnlyNodeLink> OutputLinks { get { foreach (NodeLink nodeLink in MyNode.OutputLinks) yield return nodeLink.ReadOnlyLink; } }

		public RateType RateType { get { return MyNode.RateType; } }
		public double ActualRate { get { return MyNode.ActualRate; } }
		public double DesiredRate { get { return MyNode.DesiredRate; } }
		public NodeState State { get { return MyNode.State; } }

		public double GetConsumeRate(Item item) => MyNode.GetConsumeRate(item);
		public double GetSupplyRate(Item item) => MyNode.GetSupplyRate(item);
		public double GetSuppliedRate(Item item) => MyNode.GetSuppliedRate(item);
		public bool IsOversupplied() => MyNode.IsOversupplied();
		public bool IsOversupplied(Item item) => MyNode.IsOversupplied(item);
		public bool ManualRateNotMet() => MyNode.ManualRateNotMet();

		private readonly BaseNode MyNode;

		public event EventHandler<EventArgs> NodeStateChanged;

		public ReadOnlyBaseNode(BaseNode node)
		{
			MyNode = node;
			MyNode.NodeStateChanged += Node_NodeStateChanged;
		}

		private void Node_NodeStateChanged(object sender, EventArgs e)
		{
			NodeStateChanged?.Invoke(this, EventArgs.Empty);
		}

		public override string ToString() { return "RO: " + MyNode.ToString(); }
	}


	public abstract class BaseNodeController
	{
		public int NodeID { get { return MyNode.NodeID; } }
		public Point Location { get { return MyNode.Location; } set { if (MyNode.Location != value) { MyNode.Location = value; } } }

		public IEnumerable<Item> Inputs { get { return MyNode.Inputs; } }
		public IEnumerable<Item> Outputs { get { return MyNode.Inputs; } }

		public IEnumerable<NodeLinkController> InputLinks { get { foreach (NodeLink link in MyNode.InputLinks) yield return link.Controller; } }
		public IEnumerable<NodeLinkController> OutputLinks { get { foreach (NodeLink link in MyNode.OutputLinks) yield return link.Controller; } }

		public RateType RateType { get { return MyNode.RateType; } set { if (MyNode.RateType != value) { MyNode.RateType = value; } } }

		public event EventHandler<EventArgs> NodeStateChanged;

		private readonly BaseNode MyNode;

		protected BaseNodeController(BaseNode myNode)
		{
			MyNode = myNode;
			MyNode.NodeStateChanged += MyNode_NodeStateChanged;
		}

		private void MyNode_NodeStateChanged(object sender, EventArgs e)
		{
			NodeStateChanged?.Invoke(this, EventArgs.Empty);
		}

		public abstract List<string> GetErrors();
		public abstract List<string> GetWarnings();
		public abstract Dictionary<string, Action> GetErrorResolutions();
		public abstract Dictionary<string, Action> GetWarningResolutions();

		protected Dictionary<string, Action> GetInvalidConnectionResolutions()
		{
			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
			if (!MyNode.AllLinksValid)
			{
				resolutions.Add("Delete invalid links", new Action(() =>
				{
					foreach (NodeLinkController invalidLink in InputLinks.Where(l => l.IsValid))
						invalidLink.Delete();
					foreach (NodeLinkController invalidLink in OutputLinks.Where(l => l.IsValid))
						invalidLink.Delete();
				}));
			}
			return resolutions;
		}

		public void Delete() { MyNode.MyGraph.DeleteNode(MyNode.ReadOnlyNode); }
		public override string ToString() { return "C: " + MyNode.ToString(); }
	}
}
