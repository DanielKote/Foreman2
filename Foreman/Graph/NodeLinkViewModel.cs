using Foreman.Models;
using Foreman.Models.Nodes;

namespace Foreman.Graph {
    internal sealed class NodeLinkViewModel : INodeLinkViewModel {
        private readonly GraphEntityRegistry _registry;
        private readonly NodeLink _link;

        public NodeLinkViewModel(LinkId id, NodeLink link, GraphEntityRegistry registry) {
            Id = id;
            _link = link;
            _registry = registry;
            SupplierId = _registry.TryGetNodeId(link.SupplierNode, out NodeId supplierId) ? supplierId : NodeId.Invalid;
            ConsumerId = _registry.TryGetNodeId(link.ConsumerNode, out NodeId consumerId) ? consumerId : NodeId.Invalid;
        }

        public LinkId Id { get; }
        public NodeId SupplierId { get; }
        public NodeId ConsumerId { get; }
        public ItemQualityPair Item => _link.Item;
        public double Throughput => _link.Throughput;
        public NodeDirection SupplierDirection => _link.SupplierNode.NodeDirection;
        public NodeDirection ConsumerDirection => _link.ConsumerNode.NodeDirection;
        public bool IsValid => _link.IsValid;
    }
}
