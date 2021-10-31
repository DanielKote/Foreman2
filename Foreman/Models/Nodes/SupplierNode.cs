using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Foreman
{
	public interface SupplierNode : BaseNode
	{
		Item SuppliedItem { get; }
	}

	public class SupplierNodePrototype : BaseNodePrototype, SupplierNode
	{
		public Item SuppliedItem { get; private set; }

		public override string DisplayName { get { return SuppliedItem.FriendlyName; } }
		public override IEnumerable<Item> Inputs { get { return new Item[0]; } }
		public override IEnumerable<Item> Outputs { get { yield return SuppliedItem; } }

		public SupplierNodePrototype(ProductionGraph graph, int nodeID, Item item) : base(graph, nodeID)
		{
			SuppliedItem = item;
		}

		public override float GetConsumeRate(Item item) { throw new ArgumentException("Supplier does not consume! nothing should be asking for the consume rate"); }
		public override float GetSupplyRate(Item item) { return (float)Math.Round(ActualRate, RoundingDP); }

		internal override double inputRateFor(Item item) { throw new ArgumentException("Supplier should not have outputs!"); }
		internal override double outputRateFor(Item item) { return 1; }

		public override bool IsValid { get { return !SuppliedItem.IsMissing; } }
		public override string GetErrors()
		{
			if (!IsValid)
				return string.Format("Item \"{0}\" doesnt exist in preset!", SuppliedItem.FriendlyName);
			return null;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Supplier);
			info.AddValue("NodeID", NodeID);
			info.AddValue("Location", Location);
			info.AddValue("Item", SuppliedItem.Name);
			info.AddValue("RateType", RateType);
			info.AddValue("ActualRate", ActualRate);
			if (RateType == RateType.Manual)
				info.AddValue("DesiredRate", DesiredRate);
		}

		public override string ToString() { return string.Format("Supply node for: {0}", SuppliedItem.Name); }
	}
}
