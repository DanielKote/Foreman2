using Foreman.Models.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman.Graph {
    public sealed class ProductionGraphSession : IProductionGraphSession {
        private readonly GraphEntityRegistry _registry = new();
        private readonly GraphViewModel _view = new();
        private readonly ProductionGraphEditor _editor;
        private readonly Dictionary<NodeLink, NodeLinkViewModel> _linkViewModels = [];
        private readonly Dictionary<NodeLink, LinkId> _domainLinkIds = [];
        private bool _attached;

        public ProductionGraphSession(ProductionGraph graph) {
            Graph = graph;
            _editor = new ProductionGraphEditor(graph, _registry);
        }

        public ProductionGraph Graph { get; }
        public IGraphViewModel View => _view;
        public IProductionGraphEditor Editor => _editor;

        public bool TryGetDomainNode(NodeId id, out BaseNode? node) => _registry.TryGetNode(id, out node);
        public bool TryGetDomainLink(LinkId id, out NodeLink? link) => _registry.TryGetLink(id, out link);

        public event EventHandler<NodeViewModelEventArgs>? NodeViewModelAdded;
        public event EventHandler<NodeViewModelEventArgs>? NodeViewModelRemoved;
        public event EventHandler<LinkViewModelEventArgs>? LinkViewModelAdded;
        public event EventHandler<LinkViewModelEventArgs>? LinkViewModelRemoved;
        public event EventHandler? GraphCleared;
        public event EventHandler? NodeValuesUpdated;

        public void Attach() {
            if (_attached)
                return;
            _attached = true;
            Graph.NodeAdded += OnNodeAdded;
            Graph.NodeDeleted += OnNodeDeleted;
            Graph.LinkAdded += OnLinkAdded;
            Graph.LinkDeleted += OnLinkDeleted;
            Graph.NodeValuesUpdated += OnNodeValuesUpdated;
            Graph.GraphCleared += OnGraphCleared;
            BackfillExistingGraph();
        }

        public void Detach() {
            if (!_attached)
                return;
            _attached = false;
            Graph.NodeAdded -= OnNodeAdded;
            Graph.NodeDeleted -= OnNodeDeleted;
            Graph.LinkAdded -= OnLinkAdded;
            Graph.LinkDeleted -= OnLinkDeleted;
            Graph.NodeValuesUpdated -= OnNodeValuesUpdated;
            Graph.GraphCleared -= OnGraphCleared;
        }

        internal INodeLinkViewModel GetOrCreateLinkViewModel(NodeLink link) {
            if (_linkViewModels.TryGetValue(link, out NodeLinkViewModel? existing))
                return existing;

            LinkId linkId = _registry.RegisterLink(link);
            var viewModel = new NodeLinkViewModel(linkId, link, _registry);
            _linkViewModels[link] = viewModel;
            _domainLinkIds[link] = linkId;
            _view.AddLink(viewModel);
            RefreshEndpointLinkLists(link);
            LinkViewModelAdded?.Invoke(this, new LinkViewModelEventArgs(viewModel));
            return viewModel;
        }

        private void BackfillExistingGraph() {
            foreach (BaseNode domainNode in Graph.Nodes.ToList())
                EnsureNodeViewModel(domainNode);
            foreach (NodeLink domainLink in Graph.NodeLinks.ToList())
                GetOrCreateLinkViewModel(domainLink);
        }

        private void OnNodeAdded(object? sender, NodeEventArgs e) {
            INodeViewModel viewModel = EnsureNodeViewModel(e.Node);
            NodeViewModelAdded?.Invoke(this, new NodeViewModelEventArgs(viewModel));
        }

        private void OnNodeDeleted(object? sender, NodeEventArgs e) {
            var id = new NodeId(e.Node.NodeID, _registry.Epoch);
            if (!_view.TryGetNode(id, out INodeViewModel? viewModel) || viewModel is null)
                return;
            RemoveNodeViewModel(viewModel.Id);
            NodeViewModelRemoved?.Invoke(this, new NodeViewModelEventArgs(viewModel));
        }

        private void OnLinkAdded(object? sender, NodeLinkEventArgs e) =>
            GetOrCreateLinkViewModel(e.Link);

        private void OnLinkDeleted(object? sender, NodeLinkEventArgs e) {
            if (!_domainLinkIds.TryGetValue(e.Link, out LinkId id))
                return;
            if (!_view.TryGetLink(id, out INodeLinkViewModel? viewModel) || viewModel is null)
                return;
            RemoveLinkViewModel(id, e.Link);
            _domainLinkIds.Remove(e.Link);
            LinkViewModelRemoved?.Invoke(this, new LinkViewModelEventArgs(viewModel));
        }

        private void OnNodeValuesUpdated(object? sender, EventArgs e) {
            foreach (INodeViewModel node in _view.Nodes) {
                if (node is NodeViewModelBase vm)
                    vm.NotifyValuesChanged();
            }
            NodeValuesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnGraphCleared(object? sender, EventArgs e) {
            _registry.Reset();
            _view.Clear();
            _linkViewModels.Clear();
            _domainLinkIds.Clear();
            GraphCleared?.Invoke(this, EventArgs.Empty);
        }

        private INodeViewModel EnsureNodeViewModel(BaseNode domainNode) {
            if (_registry.TryGetNodeId(domainNode, out NodeId existingId) && _view.TryGetNode(existingId, out INodeViewModel? existing) && existing is not null)
                return existing;

            NodeId id = _registry.RegisterNode(domainNode);
            INodeViewModel viewModel = NodeViewModelFactory.Create(id, domainNode, this);
            _view.AddNode(viewModel);
            return viewModel;
        }

        private void RemoveNodeViewModel(NodeId id) {
            if (_view.TryGetNode(id, out INodeViewModel? viewModel) && viewModel is NodeViewModelBase vm) {
                vm.RefreshLinkViewModels();
            }
            _registry.UnregisterNode(id);
            _view.RemoveNode(id);
        }

        private void RemoveLinkViewModel(LinkId id, NodeLink? domainLink) {
            if (domainLink is not null)
                _linkViewModels.Remove(domainLink);
            _registry.UnregisterLink(id);
            _view.RemoveLink(id);
            if (domainLink is not null)
                RefreshEndpointLinkLists(domainLink);
        }

        private void RefreshEndpointLinkLists(NodeLink link) {
            if (_registry.TryGetNodeId(link.SupplierNode, out NodeId supplierId) && _view.TryGetNode(supplierId, out INodeViewModel? supplier) && supplier is NodeViewModelBase supplierVm)
                supplierVm.RefreshLinkViewModels();
            if (_registry.TryGetNodeId(link.ConsumerNode, out NodeId consumerId) && _view.TryGetNode(consumerId, out INodeViewModel? consumer) && consumer is NodeViewModelBase consumerVm)
                consumerVm.RefreshLinkViewModels();
        }

    }
}
