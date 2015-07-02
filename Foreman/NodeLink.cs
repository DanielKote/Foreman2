using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Foreman
{
	[Serializable]
	public class NodeLink: ISerializable
	{
		public ProductionNode Supplier;
		public ProductionNode Consumer;
		public Item Item;
		public float Demand
		{
			get
			{
				return Consumer.GetTotalDemand(Item) / Consumer.InputLinks.Where(l => l.Item == Item).Count();
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

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Supplier", Supplier.Graph.Nodes.IndexOf(Supplier));
			info.AddValue("Consumer", Consumer.Graph.Nodes.IndexOf(Consumer));
			info.AddValue("Item", Item.Name);
		}
	}
}
