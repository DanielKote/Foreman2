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
		public float Amount;

		private NodeLink(ProductionNode supplier, ProductionNode consumer, Item item, float amount)
		{
			Supplier = supplier;
			Consumer = consumer;
			Item = item;
			Amount = amount;
		}

		public static void Create(ProductionNode supplier, ProductionNode consumer, Item item, float amount)
		{
			if (supplier.OutputLinks.Any(l => l.Consumer == consumer && l.Item == item))
			{
				//A link already exists
				NodeLink existingLink = supplier.OutputLinks.First(l => l.Consumer == consumer && l.Item == item);
				existingLink.Amount += amount;
			}
			else
			{
				NodeLink link = new NodeLink(supplier, consumer, item, amount);
				supplier.OutputLinks.Add(link);
				consumer.InputLinks.Add(link);
				supplier.Graph.InvalidateCaches();
			}
		}

		public void Destroy()
		{
			Supplier.OutputLinks.Remove(this);
			Consumer.InputLinks.Remove(this);
		}
	}
}
