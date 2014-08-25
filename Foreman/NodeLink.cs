using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	public class NodeLink
	{
		public ProductionNode Supplier;
		public ProductionNode Consumer;
		public Item Item;
		public float Demand
		{
			get
			{
				return Consumer.GetRequiredInput(Item) / Consumer.InputLinks.Where(l => l.Item == Item).Count();
			}
		}
		public float Amount
		{
			get
			{
				return Math.Min(Supplier.GetTotalOutput(Item), Demand);
			}
		}

		private NodeLink(ProductionNode supplier, ProductionNode consumer, Item item, float maxAmount = float.PositiveInfinity)
		{
			Supplier = supplier;
			Consumer = consumer;
			Item = item;
		}

		public static NodeLink Create(ProductionNode supplier, ProductionNode consumer, Item item, float maxAmount = float.PositiveInfinity)
		{
			NodeLink link = new NodeLink(supplier, consumer, item, maxAmount);
			supplier.OutputLinks.Add(link);
			consumer.InputLinks.Add(link);
			supplier.Graph.InvalidateCaches();
			return link;
		}

		public void Destroy()
		{
			Supplier.OutputLinks.Remove(this);
			Consumer.InputLinks.Remove(this);
		}
	}
}
