using System.Collections.Generic;

namespace Foreman.Graph {
    internal sealed class GraphViewModel : IGraphViewModel {
        private readonly Dictionary<NodeId, INodeViewModel> _nodes = [];
        private readonly Dictionary<LinkId, INodeLinkViewModel> _links = [];

        public IReadOnlyList<INodeViewModel> Nodes => [.. _nodes.Values];
        public IReadOnlyList<INodeLinkViewModel> Links => [.. _links.Values];

        internal void AddNode(INodeViewModel viewModel) => _nodes[viewModel.Id] = viewModel;
        internal void RemoveNode(NodeId id) => _nodes.Remove(id);
        internal void AddLink(INodeLinkViewModel viewModel) => _links[viewModel.Id] = viewModel;
        internal void RemoveLink(LinkId id) => _links.Remove(id);
        internal void Clear() {
            _nodes.Clear();
            _links.Clear();
        }

        public bool TryGetNode(NodeId id, out INodeViewModel? viewModel) => _nodes.TryGetValue(id, out viewModel);
        public bool TryGetLink(LinkId id, out INodeLinkViewModel? viewModel) => _links.TryGetValue(id, out viewModel);
    }
}
