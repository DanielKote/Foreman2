using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public class PassthroughNode : ProductionNode
	{
		public Item PassedItem;

		protected PassthroughNode(Item item, ProductionGraph graph)
			: base(graph)
		{
			this.PassedItem = item;
		}

		public override IEnumerable<Item> Inputs
		{
			get { return Enumerable.Repeat(PassedItem, 1); }
		}

		public override IEnumerable<Item> Outputs
		{
			get { return Inputs; }
		}

		public static PassthroughNode Create(Item item, ProductionGraph graph)
		{
			PassthroughNode node = new PassthroughNode(item, graph);
			node.Graph.Nodes.Add(node);
			node.Graph.InvalidateCaches();
			return node;
		}

		//If the graph is showing amounts rather than rates, round up all fractions (because it doesn't make sense to do half a recipe, for example)
		private float ValidateRecipeRate(float amount)
		{
			if (Graph.SelectedAmountType == AmountType.FixedAmount)
			{
				return (float)Math.Ceiling(Math.Round(amount, RoundingDP)); //Subtracting a very small number stops the amount from getting rounded up due to FP errors. It's a bit hacky but it works for now.
			}
			else
			{
				return (float)Math.Round(amount, RoundingDP);
			}
		}

		public override string DisplayName
		{
			get { return PassedItem.FriendlyName; }
		}

		public override string ToString()
		{
			return String.Format("Pass-through Tree Node: {0}", PassedItem.Name);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("NodeType", "PassThrough");
			info.AddValue("ItemName", PassedItem.Name);
			info.AddValue("RateType", rateType);
			info.AddValue("ActualRate", actualRate);
			if (rateType == RateType.Manual)
			{
				info.AddValue("DesiredRate", desiredRate);
			}
		}

		public override float GetConsumeRate(Item item)
		{
			return (float)Math.Round(actualRate, RoundingDP);
		}

		public override float GetSupplyRate(Item item)
		{
			return (float)Math.Round(actualRate, RoundingDP);
		}

		internal override double outputRateFor(Item item)
		{
			return 1;
		}

		internal override double inputRateFor(Item item)
		{
			return 1;
		}

		public override float ProductivityMultiplier()
		{
			return 1;
		}
	}
}
