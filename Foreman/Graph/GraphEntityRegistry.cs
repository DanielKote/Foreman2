using Foreman.Models.Nodes;
using System.Collections.Generic;

namespace Foreman.Graph {
    internal sealed class GraphEntityRegistry {
        private uint _epoch = 1;
        private int _nextLinkValue;
        private readonly Dictionary<NodeId, BaseNode> _nodes = [];
        private readonly Dictionary<LinkId, NodeLink> _links = [];
        private readonly Dictionary<BaseNode, NodeId> _nodeToId = [];
        private readonly Dictionary<NodeLink, LinkId> _linkToId = [];

        public uint Epoch => _epoch;

        public void Reset() {
            _epoch++;
            if (_epoch == 0)
                _epoch = 1;
            _nextLinkValue = 0;
            _nodes.Clear();
            _links.Clear();
            _nodeToId.Clear();
            _linkToId.Clear();
        }

        public NodeId RegisterNode(BaseNode node) {
            var id = new NodeId(node.NodeID, _epoch);
            _nodes[id] = node;
            _nodeToId[node] = id;
            return id;
        }

        public LinkId RegisterLink(NodeLink link) {
            if (_linkToId.TryGetValue(link, out LinkId existing))
                return existing;

            var id = new LinkId(_nextLinkValue++, _epoch);
            _links[id] = link;
            _linkToId[link] = id;
            return id;
        }

        public void UnregisterNode(NodeId id) {
            if (!_nodes.TryGetValue(id, out BaseNode? node))
                return;
            _nodes.Remove(id);
            _nodeToId.Remove(node);
        }

        public void UnregisterLink(LinkId id) {
            if (!_links.TryGetValue(id, out NodeLink? link))
                return;
            _links.Remove(id);
            _linkToId.Remove(link);
        }

        public bool TryGetNode(NodeId id, out BaseNode? node) => _nodes.TryGetValue(id, out node);
        public bool TryGetLink(LinkId id, out NodeLink? link) => _links.TryGetValue(id, out link);
        public bool TryGetNodeId(BaseNode node, out NodeId id) => _nodeToId.TryGetValue(node, out id);
        public bool TryGetLinkId(NodeLink link, out LinkId id) => _linkToId.TryGetValue(link, out id);
    }
}
