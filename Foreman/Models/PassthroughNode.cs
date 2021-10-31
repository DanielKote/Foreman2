using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman
{
	public interface PassthroughNode : BaseNode
    {
		Item PassthroughItem { get; }
    }

	public class PassthroughNodePrototype : BaseNodePrototype, PassthroughNode
	{
		public Item PassthroughItem { get; private set; }

		public override string DisplayName { get { return PassthroughItem.FriendlyName; } }
		public override IEnumerable<Item> Inputs { get { yield return PassthroughItem; } }
		public override IEnumerable<Item> Outputs { get { yield return PassthroughItem; } }

		public PassthroughNodePrototype(ProductionGraph graph, int nodeID, Item item) : base(graph, nodeID)
		{
			PassthroughItem = item;
		}

		public override float GetConsumeRate(Item item) { return (float)Math.Round(ActualRate, RoundingDP); }
		public override float GetSupplyRate(Item item) { return (float)Math.Round(ActualRate, RoundingDP); }

		internal override double outputRateFor(Item item) { return 1; }
		internal override double inputRateFor(Item item) { return 1; }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Passthrough);
			info.AddValue("NodeID:", NodeID);
			info.AddValue("Location", Location);
			info.AddValue("Item", PassthroughItem.Name);
			info.AddValue("RateType", RateType);
			info.AddValue("ActualRate", ActualRate);
			if (RateType == RateType.Manual)
				info.AddValue("DesiredRate", DesiredRate);
		}

		public override string ToString() { return string.Format("Passthrough node for: {0}", PassthroughItem.Name); }
	}
}
