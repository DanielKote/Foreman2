using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Foreman {
    public class NodeLink {
        private readonly NodeLinkController controller;
        public NodeLinkController Controller { get { return controller; } }

        public ItemQualityPair Item { get; private set; }
        public double ThroughputPerSec { get; internal set; }
        public double Throughput { get { return ThroughputPerSec * MyGraph.GetRateMultipler(); } }
        public bool IsValid { get; private set; }

        public readonly ProductionGraph MyGraph;

        public readonly BaseNode SupplierNode;
        public readonly BaseNode ConsumerNode;

        internal NodeLink(ProductionGraph myGraph, BaseNode supplier, BaseNode consumer, ItemQualityPair item) {
            MyGraph = myGraph;
            SupplierNode = supplier;
            ConsumerNode = consumer;
            Item = item;

            controller = new NodeLinkController(this);

            IsValid = LinkChecker.IsPossibleConnection(Item, SupplierNode, ConsumerNode); //only need to check once -> item & recipe temperatures cant change.
        }

        public override string ToString() => string.Format("NodeLink for {0} ({1}) connecting {1} -> {2}", Item.Item?.Name, Item.Quality?.Name, SupplierNode.NodeID, ConsumerNode.NodeID);
    }

    public class NodeLinkController {
        private readonly NodeLink MyLink;

        internal NodeLinkController(NodeLink link) { MyLink = link; }

        public void Delete() => MyLink.MyGraph.DeleteLink(MyLink);
        public override string ToString() { return "C: " + MyLink.ToString(); }
    }
}