using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman
{

	public class ConsumerNode : ProductionNode
	{
		public Item ConsumedItem { get; private set; }

		public override string DisplayName
		{
			get { return ConsumedItem.FriendlyName; }
		}

		public override IEnumerable<Item> Inputs
		{
			get { yield return ConsumedItem; }
		}

		public override IEnumerable<Item> Outputs
		{
			get { return new List<Item>(); }
		}

		protected ConsumerNode(Item item, ProductionGraph graph)
			: base(graph)
		{
			ConsumedItem = item;
			rateType = RateType.Manual;
			actualRate = 1f;
		}

		public static ConsumerNode Create(Item item, ProductionGraph graph)
		{
			ConsumerNode node = new ConsumerNode(item, graph);
			node.Graph.Nodes.Add(node);
			node.Graph.InvalidateCaches();
			return node;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", "Consumer");
			info.AddValue("ItemName", ConsumedItem.Name);
			info.AddValue("RateType", rateType);
			info.AddValue("ActualRate", actualRate);
			if (rateType == RateType.Manual)
			{
				info.AddValue("DesiredRate", desiredRate);
			}
		}

		public override float GetConsumeRate(Item item)
		{
			if (ConsumedItem != item)
				Trace.Fail(String.Format("{0} consumer does not consume {1}, nothing should be asking for the rate!", ConsumedItem.FriendlyName, item.FriendlyName));

			return (float)Math.Round(actualRate, RoundingDP);
		}

		public override float GetSupplyRate(Item item)
		{
			Trace.Fail(String.Format("{0} consumer does not supply {1}, nothing should be asking for the rate!", ConsumedItem.FriendlyName, item.FriendlyName));

			return 0;
		}

		internal override double outputRateFor(Item item)
		{
			throw new ArgumentException("Consumer should not have outputs!");
		}

		internal override double inputRateFor(Item item)
		{
			return 1;
		}
	}
}
