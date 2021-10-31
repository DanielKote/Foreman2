using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman
{
	public class SupplierNode : BaseNode
	{
		private readonly BaseNodeController controller;
		public override BaseNodeController Controller { get { return controller; } }

		public readonly Item SuppliedItem;

		public override IEnumerable<Item> Inputs { get { return new Item[0]; } }
		public override IEnumerable<Item> Outputs { get { yield return SuppliedItem; } }

		public SupplierNode(ProductionGraph graph, int nodeID, Item item) : base(graph, nodeID)
		{
			SuppliedItem = item;
			controller = SupplierNodeController.GetController(this);
			ReadOnlyNode = new ReadOnlySupplierNode(this);
		}

		public override bool UpdateState()
		{
			NodeState oldState = State;
			State = (SuppliedItem.IsMissing || !AllLinksValid) ? NodeState.Error : NodeState.Clean;
			base.UpdateState();
			if (oldState != State)
			{
				OnNodeStateChanged();
				return true;
			}
			return false;
		}

		public override double GetConsumeRate(Item item) { throw new ArgumentException("Supplier does not consume! nothing should be asking for the consume rate"); }
		public override double GetSupplyRate(Item item) { return ActualRate; }

		internal override double inputRateFor(Item item) { throw new ArgumentException("Supplier should not have outputs!"); }
		internal override double outputRateFor(Item item) { return MyGraph.GetRateMultipler(); }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Supplier);
			info.AddValue("NodeID", NodeID);
			info.AddValue("Location", Location);
			info.AddValue("Item", SuppliedItem.Name);
			info.AddValue("RateType", RateType);
			info.AddValue("ActualRate", ActualRatePerSec);
			if (RateType == RateType.Manual)
				info.AddValue("DesiredRate", DesiredRatePerSec);
		}

		public override string ToString() { return string.Format("Supply node for: {0}", SuppliedItem.Name); }
	}

	public class ReadOnlySupplierNode : ReadOnlyBaseNode
	{
		public Item SuppliedItem { get { return MyNode.SuppliedItem; } }

		private readonly SupplierNode MyNode;

		public ReadOnlySupplierNode(SupplierNode node) : base(node) { MyNode = node; }

	}

	public class SupplierNodeController : BaseNodeController
	{
		public Item SuppliedItem { get { return MyNode.SuppliedItem; } }

		private readonly SupplierNode MyNode;

		protected SupplierNodeController(SupplierNode myNode) : base(myNode) { MyNode = myNode; }

		public static SupplierNodeController GetController(SupplierNode node)
		{
			if (node.Controller != null)
				return (SupplierNodeController)node.Controller;
			return new SupplierNodeController(node);
		}

		public override List<string> GetErrors()
		{
			List<string> errors = new List<string>();
			if (SuppliedItem.IsMissing)
				errors.Add(string.Format("> Item \"{0}\" doesnt exist in preset!", SuppliedItem.FriendlyName));
			else if (!MyNode.AllLinksValid)
				errors.Add("> Some links are invalid!");
			return errors;
		}

		public override Dictionary<string, Action> GetErrorResolutions()
		{
			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
			if (SuppliedItem.IsMissing)
				resolutions.Add("Delete node", new Action(() => this.Delete()));
			else
				foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
					resolutions.Add(kvp.Key, kvp.Value);
			return resolutions;
		}

		public override List<string> GetWarnings() { Trace.Fail("Passthrough node never has the warning state!"); return null; }
		public override Dictionary<string, Action> GetWarningResolutions() { Trace.Fail("Passthrough node never has the warning state!"); return null; }
	}
}
