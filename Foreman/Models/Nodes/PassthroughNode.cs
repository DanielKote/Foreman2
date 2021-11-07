using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman
{
	public class PassthroughNode : BaseNode
	{
		private readonly BaseNodeController controller;
		public override BaseNodeController Controller { get { return controller; } }

		public readonly Item PassthroughItem;

		public override IEnumerable<Item> Inputs { get { yield return PassthroughItem; } }
		public override IEnumerable<Item> Outputs { get { yield return PassthroughItem; } }

		public PassthroughNode(ProductionGraph graph, int nodeID, Item item) : base(graph, nodeID)
		{
			PassthroughItem = item;
			controller = PassthroughNodeController.GetController(this);
			ReadOnlyNode = new ReadOnlyPassthroughNode(this);
		}

		public override void UpdateState()
		{
			NodeState oldState = State;
			State = (!PassthroughItem.IsMissing && AllLinksValid) ? AllLinksConnected ? NodeState.Clean : NodeState.MissingLink : NodeState.Error;
			if (oldState != State)
				OnNodeStateChanged();
		}

		public override double GetConsumeRate(Item item) { return ActualRate; }
		public override double GetSupplyRate(Item item) { return ActualRate; }

		internal override double inputRateFor(Item item) { return 1; }
		internal override double outputRateFor(Item item) { return 1; }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Passthrough);
			info.AddValue("NodeID", NodeID);
			info.AddValue("Location", Location);
			info.AddValue("Item", PassthroughItem.Name);
			info.AddValue("RateType", RateType);
			if (RateType == RateType.Manual)
				info.AddValue("DesiredRate", DesiredRatePerSec);
			info.AddValue("IsFlipped", IsFlipped);
		}

		public override string ToString() { return string.Format("Passthrough node for: {0}", PassthroughItem.Name); }
	}

	public class ReadOnlyPassthroughNode : ReadOnlyBaseNode
	{
		public Item PassthroughItem => MyNode.PassthroughItem;

		private readonly PassthroughNode MyNode;

		public ReadOnlyPassthroughNode(PassthroughNode node) : base(node) { MyNode = node; }

		public override List<string> GetErrors()
		{
			List<string> errors = new List<string>();
			if (PassthroughItem.IsMissing)
				errors.Add(string.Format("> Item \"{0}\" doesnt exist in preset!", PassthroughItem.FriendlyName));
			else if (!MyNode.AllLinksValid)
				errors.Add("> Some links are invalid!");
			return errors;
		}

		public override List<string> GetWarnings() { Trace.Fail("Passthrough node never has the warning state!"); return null; }
	}

	public class PassthroughNodeController : BaseNodeController
	{
		private readonly PassthroughNode MyNode;

		protected PassthroughNodeController(PassthroughNode myNode) : base(myNode) { MyNode = myNode; }

		public static PassthroughNodeController GetController(PassthroughNode node)
		{
			if (node.Controller != null)
				return (PassthroughNodeController)node.Controller;
			return new PassthroughNodeController(node);
		}

		public override Dictionary<string, Action> GetErrorResolutions()
		{
			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
			if (MyNode.PassthroughItem.IsMissing)
				resolutions.Add("Delete node", new Action(() => this.Delete()));
			else
				foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
					resolutions.Add(kvp.Key, kvp.Value);
			return resolutions;
		}

		public override Dictionary<string, Action> GetWarningResolutions() { Trace.Fail("Passthrough node never has the warning state!"); return null; }
	}
}
