using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
    //public enum NodeType { Recipe, Supply, Consumer };
	public enum RateType { Auto, Manual };

	[Serializable]
	public abstract partial class ProductionNode : ISerializable
	{
		public static readonly int RoundingDP = 4;
		public ProductionGraph Graph { get; protected set; }
		public abstract String DisplayName { get; }
		public abstract IEnumerable<Item> Inputs { get; }
		public abstract IEnumerable<Item> Outputs { get; }
        public double SpeedBonus { get; internal set; }
        public double ProductivityBonus { get; set; }

        public List<NodeLink> InputLinks = new List<NodeLink>();
		public List<NodeLink> OutputLinks = new List<NodeLink>();
		public RateType rateType = RateType.Auto;

        // The rate the solver calculated is appropriate for this node.
        public float actualRate = 0f;

        // If the rateType is manual, this field contains the rate the user desires.
		public float desiredRate = 0f;

        // The calculated rate at which the given item is consumed by this node. This may not match
        // the desired amount!
        public abstract float GetConsumeRate(Item item);

        // The calculated rate at which the given item is consumed by this node. This may not match
        // the desired amount!
        public abstract float GetSupplyRate(Item item);

		protected ProductionNode(ProductionGraph graph)
		{
			Graph = graph;
		}

		public bool CanUltimatelyTakeFrom(ProductionNode node)
		{
			Queue<ProductionNode> Q = new Queue<ProductionNode>();
			HashSet<ProductionNode> V = new HashSet<ProductionNode>();

			V.Add(this);
			Q.Enqueue(this);

			while (Q.Any())
			{
				ProductionNode t = Q.Dequeue();
				if (t == node)
				{
					return true;
				}
				foreach (NodeLink e in t.InputLinks)
				{
					ProductionNode u = e.Supplier;
					if (!V.Contains(u))
					{
						V.Add(u);
						Q.Enqueue(u);
					}
				}
			}
			return false;
		}

		public void Destroy()
		{
			foreach (NodeLink link in InputLinks.ToList().Union(OutputLinks.ToList()))
			{
				link.Destroy();
			}
			Graph.Nodes.Remove(this);
			Graph.InvalidateCaches();
		}

		public abstract void GetObjectData(SerializationInfo info, StreamingContext context);

        public virtual float ProductivityMultiplier()
        {
            return (float)(1.0 + ProductivityBonus);
        }

        internal double GetProductivityBonus()
        {
            return 1.5;
        }

        public float GetSuppliedRate(Item item)
        {
            return (float)InputLinks.Where(x => x.Item == item).Sum(x => x.Throughput);
        }

        internal bool OverSupplied(Item item)
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

		internal bool ManualRateNotMet()
        {
            // TODO: Hard-coded epsilon is gross :(
            return rateType == RateType.Manual && Math.Abs(actualRate - desiredRate) > 0.0001;
        }
    }
}
