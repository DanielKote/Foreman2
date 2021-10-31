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

		public override double GetConsumeRate(Item item) { return (double)Math.Round(ActualRate, RoundingDP); }
		public override double GetSupplyRate(Item item) { return (double)Math.Round(ActualRate, RoundingDP); }

		internal override double inputRateFor(Item item) { return 1; }
		internal override double outputRateFor(Item item) { return 1; }

		public override void UpdateState() { State = (PassthroughItem.IsMissing || !AllLinksValid) ? NodeState.Error : NodeState.Clean; }

		public override List<string> GetErrors()
		{
			List<string> errors = new List<string>();
			if (PassthroughItem.IsMissing)
				errors.Add(string.Format("> Item \"{0}\" doesnt exist in preset!", PassthroughItem.FriendlyName));
			else if (!AllLinksValid)
				errors.Add("> Some links are invalid!");
			return errors;
		}

		public override Dictionary<string, Action> GetErrorResolutions()
		{
			Dictionary<string, Action> resolutions = new Dictionary<string, Action>();
			if(PassthroughItem.IsMissing)
				resolutions.Add("Delete node", new Action(() => this.Delete()));
			else
				foreach (KeyValuePair<string, Action> kvp in GetInvalidConnectionResolutions())
					resolutions.Add(kvp.Key, kvp.Value);
			return resolutions;
		}

		public override List<string> GetWarnings() { Trace.Fail("Passthrough node never has the warning state!"); return null; }
		public override Dictionary<string, Action> GetWarningResolutions() { Trace.Fail("Passthrough node never has the warning state!"); return null; }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", NodeType.Passthrough);
			info.AddValue("NodeID", NodeID);
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
