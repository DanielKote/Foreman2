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
    }


	[Serializable]
	public class NodeLinkPrototype: NodeLink
	{
		public BaseNode Supplier { get { return supplier; } }
		public BaseNode Consumer { get { return consumer; } }
		public Item Item { get; private set; }
        public double Throughput { get; set; }

		internal BaseNodePrototype supplier;
		internal BaseNodePrototype consumer;

		internal NodeLinkPrototype(BaseNodePrototype supplier, BaseNodePrototype consumer, Item item)
		{
			this.supplier = supplier;
			this.consumer = consumer;
			Item = item;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("SupplierID", supplier.NodeID);
			info.AddValue("ConsumerID", consumer.NodeID);
			info.AddValue("Item", Item.Name);
		}

		public override string ToString() { return string.Format("NodeLink for {0} connecting {1} -> {2}", Item.Name, supplier.NodeID, consumer.NodeID); }
	}
}
