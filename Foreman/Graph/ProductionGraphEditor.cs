using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using System;
using System.Drawing;

namespace Foreman.Graph {
    internal sealed class ProductionGraphEditor(ProductionGraph graph, GraphEntityRegistry registry) : IProductionGraphEditor {
        private readonly ProductionGraph _graph = graph;
        private readonly GraphEntityRegistry _registry = registry;

        public ProductionGraph Graph => _graph;

        public NodeId CreateSupplierNode(ItemQualityPair item, Point location) =>
            RegisterCreatedNode(_graph.CreateSupplierNode(item, location));

        public NodeId CreateConsumerNode(ItemQualityPair item, Point location) =>
            RegisterCreatedNode(_graph.CreateConsumerNode(item, location));

        public NodeId CreatePassthroughNode(ItemQualityPair item, Point location) =>
            RegisterCreatedNode(_graph.CreatePassthroughNode(item, location));

        public NodeId CreateRecipeNode(RecipeQualityPair recipe, Point location) =>
            RegisterCreatedNode(_graph.CreateRecipeNode(recipe, location));

        public NodeId CreateSpoilNode(ItemQualityPair inputItem, IItem outputItem, Point location) =>
            RegisterCreatedNode(_graph.CreateSpoilNode(inputItem, outputItem, location));

        public NodeId CreatePlantNode(IPlantProcess plantProcess, IQuality quality, Point location) =>
            RegisterCreatedNode(_graph.CreatePlantNode(plantProcess, quality, location));

        public LinkId CreateLink(NodeId supplierId, NodeId consumerId, ItemQualityPair item) {
            if (!_registry.TryGetNode(supplierId, out BaseNode? supplier) || supplier is null)
                throw new InvalidOperationException("Supplier node is not registered.");
            if (!_registry.TryGetNode(consumerId, out BaseNode? consumer) || consumer is null)
                throw new InvalidOperationException("Consumer node is not registered.");

            NodeLink link = _graph.CreateLink(supplier, consumer, item);
            if (!_registry.TryGetLinkId(link, out LinkId id))
                id = _registry.RegisterLink(link);
            return id;
        }

        public void DeleteNode(NodeId id) {
            if (_registry.TryGetNode(id, out BaseNode? node) && node is not null)
                _graph.DeleteNode(node);
        }

        public void DeleteLink(LinkId id) {
            if (_registry.TryGetLink(id, out NodeLink? link) && link is not null)
                _graph.DeleteLink(link);
        }

        public void SetLocation(NodeId id, Point location) =>
            RequestNodeController(id)?.SetLocation(location);

        public void SetDirection(NodeId id, NodeDirection direction) =>
            RequestNodeController(id)?.SetDirection(direction);

        public BaseNodeController? RequestNodeController(NodeId id) {
            return !_registry.TryGetNode(id, out BaseNode? node) || node is null ? null : _graph.RequestNodeController(node);
        }

        private NodeId RegisterCreatedNode(BaseNode node) {
            return _registry.TryGetNodeId(node, out NodeId id) ? id : _registry.RegisterNode(node);
        }
    }
}
