using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman
{
	public interface ConsumerNode : BaseNode
	{
		Item ConsumedItem { get; }
	}

	public class ConsumerNodePrototype : BaseNodePrototype, ConsumerNode
	{
		public Item ConsumedItem { get; private set; }

		public override string DisplayName { get { return ConsumedItem.FriendlyName; } }
		public override IEnumerable<Item> Inputs { get { yield return ConsumedItem; } }
		public override IEnumerable<Item> Outputs { get { return new Item[0]; } }

		public ConsumerNodePrototype(ProductionGraph graph, int nodeID, Item item) : base(graph, nodeID)
		{
			ConsumedItem = item;
			RateType = RateType.Manual;
			ActualRate = 1f;
		}

		public override float GetConsumeRate(Item item) { return (float)Math.Round(ActualRate, RoundingDP); }
		public override float GetSupplyRate(Item item) { throw new ArgumentException("Consumer does not supply! nothing should be asking for the supply rate"); }

		internal override double inputRateFor(Item item) { return 1; }
		internal override double outputRateFor(Item item) { throw new ArgumentException("Consumer should not have outputs!"); }

		public override bool IsValid { get { return !ConsumedItem.IsMissing; } }
		public override string GetErrors()
		{
			if (!IsValid)
				return string.Format("Item \"{0}\" doesnt exist in preset!", ConsumedItem.FriendlyName);
			return null;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Consumer);
			info.AddValue("NodeID", NodeID);
			info.AddValue("Location", Location);
			info.AddValue("Item", ConsumedItem.Name);
			info.AddValue("RateType", RateType);
			info.AddValue("ActualRate", ActualRate);
			if (RateType == RateType.Manual)
				info.AddValue("DesiredRate", DesiredRate);
		}

		public override string ToString() { return string.Format("Consumption node for: {0}", ConsumedItem.Name); }
	}
}
