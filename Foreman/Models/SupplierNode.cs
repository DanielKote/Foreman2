using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public class SupplierNode : ProductionNode
	{
		public Item SuppliedItem { get; private set; }

		protected SupplierNode(Item item, ProductionGraph graph) : base(graph)
		{
			SuppliedItem = item;
		}

		public override IEnumerable<Item> Inputs
		{
			get { return new List<Item>(); }
		}

		public override IEnumerable<Item> Outputs
		{
			get { yield return SuppliedItem; }
		}

		public static SupplierNode Create(Item item, ProductionGraph graph)
		{
			SupplierNode node = new SupplierNode(item, graph);
			node.Graph.Nodes.Add(node);
			node.Graph.InvalidateCaches();
			return node;
		}

		public override string DisplayName
		{
			get { return SuppliedItem.FriendlyName; }
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", "Supply");
			info.AddValue("ItemName", SuppliedItem.Name);
			info.AddValue("RateType", rateType);
			info.AddValue("ActualRate", actualRate);
			if (rateType == RateType.Manual)
			{
				info.AddValue("DesiredRate", desiredRate);
			}
		}

		public override float GetConsumeRate(Item item)
		{
			Trace.Fail(String.Format("{0} supplier does not consume {1}, nothing should be asking for the rate!", SuppliedItem.FriendlyName, item.FriendlyName));
			return 0;
		}

		public override float GetSupplyRate(Item item)
		{
			if (SuppliedItem != item)
				Trace.Fail(String.Format("{0} supplier does not supply {1}, nothing should be asking for the rate!", SuppliedItem.FriendlyName, item.FriendlyName));

			return (float)Math.Round(actualRate, RoundingDP);
		}

		internal override double outputRateFor(Item item)
		{
			return 1;
		}

		internal override double inputRateFor(Item item)
		{
			throw new ArgumentException("Supply node should not have any inputs!");
		}
	}
}
