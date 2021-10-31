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

		public override double GetConsumeRate(Item item) { throw new ArgumentException("Supplier does not consume! nothing should be asking for the consume rate"); }
		public override double GetSupplyRate(Item item) { return (double)Math.Round(ActualRate, RoundingDP); }

		internal override double inputRateFor(Item item) { throw new ArgumentException("Supplier should not have outputs!"); }
		internal override double outputRateFor(Item item) { return 1; }

		public override void UpdateState() { State = (SuppliedItem.IsMissing || !AllLinksValid) ? NodeState.Error : NodeState.Clean; }

		public override List<string> GetErrors()
		{
			List<string> errors = new List<string>();
			if (SuppliedItem.IsMissing)
				errors.Add(string.Format("> Item \"{0}\" doesnt exist in preset!", SuppliedItem.FriendlyName));
			else if (!AllLinksValid)
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

		public override List<string> GetWarnings() { Trace.Fail("Supplier node never has the warning state!"); return null; }
		public override Dictionary<string, Action> GetWarningResolutions() { Trace.Fail("Supplier node never has the warning state!"); return null; }

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
