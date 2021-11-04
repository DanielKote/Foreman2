using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman
{
	public class ConsumerNode : BaseNode
	{
		private readonly BaseNodeController controller;
		public override BaseNodeController Controller { get{ return controller; } }

		public readonly Item ConsumedItem;

		public override IEnumerable<Item> Inputs { get { yield return ConsumedItem; } }
		public override IEnumerable<Item> Outputs { get { return new Item[0]; } }

		public ConsumerNode(ProductionGraph graph, int nodeID, Item item) : base(graph, nodeID)
		{
			ConsumedItem = item;
			controller = ConsumerNodeController.GetController(this);
			ReadOnlyNode = new ReadOnlyConsumerNode(this);
		}

		public override void UpdateState()
		{
			NodeState oldState = State;
			State = (ConsumedItem.IsMissing || !AllLinksValid) ? NodeState.Error : NodeState.Clean;
			if (oldState != State)
				OnNodeStateChanged();
		}

		public override double GetConsumeRate(Item item) { return ActualRate; }
		public override double GetSupplyRate(Item item) { throw new ArgumentException("Consumer does not supply! nothing should be asking for the supply rate"); }

		internal override double inputRateFor(Item item) { return 1; }
		internal override double outputRateFor(Item item) { throw new ArgumentException("Consumer should not have outputs!"); }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Consumer);
			info.AddValue("NodeID", NodeID);
			info.AddValue("Location", Location);
			info.AddValue("Item", ConsumedItem.Name);
			info.AddValue("RateType", RateType);
			if (RateType == RateType.Manual)
				info.AddValue("DesiredRate", DesiredRatePerSec);
		}

		public override string ToString() { return string.Format("Consumption node for: {0}", ConsumedItem.Name); }
	}

	public class ReadOnlyConsumerNode : ReadOnlyBaseNode
	{
		public Item ConsumedItem => MyNode.ConsumedItem;

		private readonly ConsumerNode MyNode;

		public ReadOnlyConsumerNode(ConsumerNode node) : base(node) { MyNode = node; }

		public override List<string> GetErrors()
		{
			List<string> errors = new List<string>();
			if (ConsumedItem.IsMissing)
				errors.Add(string.Format("> Item \"{0}\" doesnt exist in preset!", ConsumedItem.FriendlyName));
			else if (!MyNode.AllLinksValid)
				errors.Add("> Some links are invalid!");
			return errors;
		}

		public override List<string> GetWarnings() { Trace.Fail("Consumer node never has the warning state!"); return null; }
	}

	public class ConsumerNodeController : BaseNodeController
	{
		private readonly ConsumerNode MyNode;

		protected ConsumerNodeController(ConsumerNode myNode) : base(myNode) { MyNode = myNode; }

		public static ConsumerNodeController GetController(ConsumerNode node)
		{
			if (node.Controller != null)
				return (ConsumerNodeController)node.Controller;
			return new ConsumerNodeController(node);
		}

		public override Dictionary<string, Action> GetErrorResolutions()
		{
			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
			if (MyNode.ConsumedItem.IsMissing)
				resolutions.Add("Delete node", new Action(() => this.Delete()));
			else
				foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
					resolutions.Add(kvp.Key, kvp.Value);
			return resolutions;
		}

		public override Dictionary<string, Action> GetWarningResolutions() { Trace.Fail("Consumer node never has the warning state!"); return null; }
	}
}
