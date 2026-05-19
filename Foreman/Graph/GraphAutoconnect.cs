using Foreman.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman.Graph {
    /// <summary>Connects unlinked node inputs to the nearest compatible output on another node.</summary>
    public static class GraphAutoconnect {
        public static int ConnectDisconnectedInputs(ProductionGraphSession session) {
            ArgumentNullException.ThrowIfNull(session);

            IReadOnlyList<INodeViewModel> nodes = session.View.Nodes;
            var suppliersByItem = new Dictionary<ItemQualityPair, List<NodeId>>();

            foreach (INodeViewModel node in nodes) {
                foreach (ItemQualityPair output in node.Outputs) {
                    if (!suppliersByItem.TryGetValue(output, out List<NodeId>? supplierIds))
                        suppliersByItem[output] = supplierIds = [];
                    supplierIds.Add(node.Id);
                }
            }

            int linksCreated = 0;
            foreach (INodeViewModel consumer in nodes) {
                foreach (ItemQualityPair input in consumer.Inputs
            .Where(input => !consumer.InputLinks.Any(link => link.Item == input))) {
                    if (!suppliersByItem.TryGetValue(input, out List<NodeId>? suppliers))
                        continue;

                    NodeId supplierId = suppliers
                        .Where(id => id != consumer.Id)
                        .OrderBy(id => ManhattanDistance(session, id, consumer.Id))
                        .FirstOrDefault();

                    if (!supplierId.IsValid)
                        continue;

                    session.Editor.CreateLink(supplierId, consumer.Id, input);
                    linksCreated++;
                }
            }

            if (linksCreated > 0)
                session.Graph.UpdateNodeValues();

            return linksCreated;
        }

        private static int ManhattanDistance(ProductionGraphSession session, NodeId a, NodeId b) {
            return !session.View.TryGetNode(a, out INodeViewModel? nodeA) || nodeA is null
                ? int.MaxValue
                : !session.View.TryGetNode(b, out INodeViewModel? nodeB) || nodeB is null
                ? int.MaxValue
                : Math.Abs(nodeA.Location.X - nodeB.Location.X) + Math.Abs(nodeA.Location.Y - nodeB.Location.Y);
        }
    }
}
