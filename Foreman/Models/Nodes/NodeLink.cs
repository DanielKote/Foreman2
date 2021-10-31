using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	public interface NodeLink : ISerializable
	{
		BaseNode Supplier { get; }
		BaseNode Consumer { get; }
		Item Item { get; }
		double Throughput { get; }
		bool IsValid { get; }

		void Delete();
	}


	[Serializable]
	public class NodeLinkPrototype : NodeLink
	{
		protected ProductionGraph MyGraph;

		public BaseNode Supplier { get { return supplier; } }
		public BaseNode Consumer { get { return consumer; } }
		public Item Item { get; private set; }
		public double Throughput { get; set; }
		public bool IsValid { get; private set; }

		internal BaseNodePrototype supplier;
		internal BaseNodePrototype consumer;

		internal NodeLinkPrototype(ProductionGraph myGraph, BaseNodePrototype supplier, BaseNodePrototype consumer, Item item)
		{
			this.MyGraph = myGraph;
			this.supplier = supplier;
			this.consumer = consumer;
			Item = item;

			IsValid = item.IsFluid ? LinkChecker.IsValidTemperatureConnection(Item, supplier, consumer) : LinkChecker.IsPossibleConnection(Item, supplier, consumer); //only need to check once -> item & recipe temperatures cant change.
		}

		public void Delete() { MyGraph.DeleteLink(this); }

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("SupplierID", supplier.NodeID);
			info.AddValue("ConsumerID", consumer.NodeID);
			info.AddValue("Item", Item.Name);
		}

		public override string ToString() { return string.Format("NodeLink for {0} connecting {1} -> {2}", Item.Name, supplier.NodeID, consumer.NodeID); }
	}
}
