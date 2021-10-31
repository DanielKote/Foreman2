using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public enum RateType { Auto, Manual };

	public interface BaseNode : ISerializable
    {
		string DisplayName { get; }
		int NodeID { get; }
		Point Location { get; set; }

		IEnumerable<Item> Inputs { get; }
		IEnumerable<Item> Outputs { get; }
		
		IReadOnlyList<NodeLink> InputLinks { get; }
		IReadOnlyList<NodeLink> OutputLinks { get; }

		RateType RateType { get; set; }
		float ActualRate { get; }
		float DesiredRate { get; set; }

		float GetConsumeRate(Item item); //calculated rate a given item is consumed by this node (may not match desired amount)
		float GetSupplyRate(Item item); //calculated rate a given item is supplied by this note (may not match desired amount)
		float GetSuppliedRate(Item item); //actual rate an item is supplied to this node (sum of all links for a given item to this node)

		bool IsOversupplied(Item item); //true if GetConsumeRate < GetSuppliedRate
		bool ManualRateNotMet(); //true if ActualRate < DesiredRate (and RateType is manual)

		float GetSpeedMultiplier();
		float GetProductivityMultiplier();
	}

	[Serializable]
	public abstract partial class BaseNodePrototype : BaseNode
	{
		protected static readonly int RoundingDP = 4;
		protected ProductionGraph MyGraph;

		public int NodeID { get; private set; }
		public Point Location { get; set; }
		public RateType RateType { get; set; }

		public abstract string DisplayName { get; }
		public abstract IEnumerable<Item> Inputs { get; }
		public abstract IEnumerable<Item> Outputs { get; }

		public IReadOnlyList<NodeLink> InputLinks { get { return inputLinks; } }
		public IReadOnlyList<NodeLink> OutputLinks { get { return outputLinks; } }

        public float ActualRate { get; protected set; } // The rate the solver calculated is appropriate for this node.
		public float DesiredRate { get; set; } // If the rateType is manual, this field contains the rate the user desires.

        internal List<NodeLinkPrototype> inputLinks { get; private set; }
		internal List<NodeLinkPrototype> outputLinks { get; private set; }
     
        public abstract float GetConsumeRate(Item item); //calculated rate a given item is consumed by this node
        public abstract float GetSupplyRate(Item item); //calculated rate a given item is supplied by this node

		public virtual float GetSpeedMultiplier() { return 1; }
		public virtual float GetProductivityMultiplier() { return 1; }

		protected BaseNodePrototype(ProductionGraph graph, int nodeID)
		{
			RateType = RateType.Auto;
			MyGraph = graph;
			NodeID = nodeID;
			Location = new Point(0, 0);

			inputLinks = new List<NodeLinkPrototype>();
			outputLinks = new List<NodeLinkPrototype>();
		}

		public abstract void GetObjectData(SerializationInfo info, StreamingContext context);

        public float GetSuppliedRate(Item item)
        {
            return (float)InputLinks.Where(x => x.Item == item).Sum(x => x.Throughput);
        }

        public bool IsOversupplied(Item item)
        {
			//supply & consume > 1 ---> allow for 0.1% error
			//supply & consume [0.001 -> 1]  ---> allow for 2% error
			//supply & consume [0 ->0.001] ---> allow for any errors (as long as neither are 0)
			//supply & consume = 0 ---> no errors if both are exactly 0

			float consumeRate = GetConsumeRate(item);
			float supplyRate = GetSuppliedRate(item);
			if ((consumeRate == 0 && supplyRate == 0) || (supplyRate < 0.001 && supplyRate < 0.001))
				return false;
			return ((supplyRate - consumeRate) / supplyRate) > ((consumeRate > 1 && supplyRate > 1) ? 0.001f : 0.05f);
		}

		public bool ManualRateNotMet()
        {
            return (RateType == RateType.Manual) && Math.Abs(ActualRate - DesiredRate) > 0.0001;
        }
    }
}
