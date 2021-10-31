using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman
{
	[Serializable]
	public class NodeLink
	{
		private readonly NodeLinkController controller;
		public NodeLinkController Controller { get { return controller; } }
		public ReadOnlyNodeLink ReadOnlyLink { get; protected set; }

		public Item Item { get; private set; }
		public double Throughput { get; internal set; }
		public bool IsValid { get; private set; }

		public readonly ProductionGraph MyGraph;

		public readonly BaseNode SupplierNode;
		public readonly BaseNode ConsumerNode;

		internal NodeLink(ProductionGraph myGraph, BaseNode supplier, BaseNode consumer, Item item)
		{
			MyGraph = myGraph;
			SupplierNode = supplier;
			ConsumerNode = consumer;
			Item = item;

			controller = NodeLinkController.GetController(this);
			ReadOnlyLink = new ReadOnlyNodeLink(this);

			IsValid = Item.IsFluid ? LinkChecker.IsValidTemperatureConnection(Item, SupplierNode.ReadOnlyNode, ConsumerNode.ReadOnlyNode) : LinkChecker.IsPossibleConnection(Item, SupplierNode.ReadOnlyNode, ConsumerNode.ReadOnlyNode); //only need to check once -> item & recipe temperatures cant change.
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("SupplierID", SupplierNode.NodeID);
			info.AddValue("ConsumerID", ConsumerNode.NodeID);
			info.AddValue("Item", Item.Name);
		}

		public override string ToString() { return string.Format("NodeLink for {0} connecting {1} -> {2}", Item.Name, SupplierNode.NodeID, ConsumerNode.NodeID); }
	}

	public class ReadOnlyNodeLink
	{
		public ReadOnlyBaseNode Supplier { get { return MyLink.ConsumerNode.ReadOnlyNode; } }
		public ReadOnlyBaseNode Consumer { get { return MyLink.ConsumerNode.ReadOnlyNode; } }
		public Item Item { get { return MyLink.Item; } }
		public double Throughput { get { return MyLink.Throughput; } }
		public bool IsValid { get { return MyLink.IsValid; } }

		private readonly NodeLink MyLink;

		public ReadOnlyNodeLink(NodeLink link) { MyLink = link; }

		public override string ToString() { return "RO: " + MyLink.ToString(); }
	}

	public class NodeLinkController
	{
		public ReadOnlyBaseNode Supplier { get; }
		public ReadOnlyBaseNode Consumer { get; }
		public Item Item { get; }
		public double Throughput { get; }
		public bool IsValid { get; }

		private readonly NodeLink MyLink;

		protected NodeLinkController(NodeLink link) { MyLink = link; }

		public static NodeLinkController GetController(NodeLink link)
		{
			if (link.Controller != null)
				return (NodeLinkController)link.Controller;
			return new NodeLinkController(link);
		}

		public void Delete() { MyLink.MyGraph.DeleteLink(MyLink.ReadOnlyLink); }
		public override string ToString() { return "C: " + MyLink.ToString(); }
	}
}
