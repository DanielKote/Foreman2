using System;
using System.Runtime.Serialization;

namespace Foreman {
    [Serializable]
    public class NodeLink : ISerializable {
        private readonly NodeLinkController _controller;

        public NodeLinkController Controller => _controller;

        public ReadOnlyNodeLink ReadOnlyLink { get; protected set; }

        public ItemQualityPair Item { get; private set; }
        public double ThroughputPerSec { get; internal set; }

        public double Throughput => ThroughputPerSec * MyGraph.GetRateMultiplier();

        public bool IsValid { get; private set; }

        public readonly ProductionGraph MyGraph;

        public readonly BaseNode SupplierNode;
        public readonly BaseNode ConsumerNode;

        internal NodeLink(ProductionGraph myGraph, BaseNode supplier, BaseNode consumer, ItemQualityPair item) {
            MyGraph = myGraph;
            SupplierNode = supplier;
            ConsumerNode = consumer;
            Item = item;

            _controller = NodeLinkController.GetController(this);
            ReadOnlyLink = new ReadOnlyNodeLink(this);

            // only need to check once -> item & recipe temperatures cant change.
            IsValid = LinkChecker.IsPossibleConnection(Item, SupplierNode.ReadOnlyNode, ConsumerNode.ReadOnlyNode);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("SupplierID", SupplierNode.NodeId);
            info.AddValue("ConsumerID", ConsumerNode.NodeId);
            info.AddValue("Item", Item.Item.Name);
            info.AddValue("Quality", Item.Quality.Name);
        }

        public override string ToString() {
            return $"NodeLink for {Item.Item.Name} ({Item.Quality.Name}) connecting {Item.Quality.Name} -> {SupplierNode.NodeId}";
        }
    }

    public class ReadOnlyNodeLink(NodeLink link) {
        public ReadOnlyBaseNode Supplier => link.SupplierNode.ReadOnlyNode;
        public ReadOnlyBaseNode Consumer => link.ConsumerNode.ReadOnlyNode;

        public NodeDirection SupplierDirection => link.SupplierNode.NodeDirection;
        public NodeDirection ConsumerDirection => link.ConsumerNode.NodeDirection;

        public ItemQualityPair Item => link.Item;
        public double Throughput => link.Throughput;
        public bool IsValid => link.IsValid;

        public override string ToString() {
            return "RO: " + link;
        }
    }

    public class NodeLinkController {
        private readonly NodeLink _myLink;

        protected NodeLinkController(NodeLink link) {
            _myLink = link;
        }

        public static NodeLinkController GetController(NodeLink link) {
            return link.Controller ?? new NodeLinkController(link);
        }

        public void Delete() {
            _myLink.MyGraph.DeleteLink(_myLink.ReadOnlyLink);
        }

        public override string ToString() {
            return "C: " + _myLink;
        }
    }
}