using Foreman.Graph;
using Foreman.Models;
using Foreman.Models.Nodes;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ForemanTest.Graph {
    [TestClass]
    public class GraphAutoconnectTests : ForemanTestBase {
        [TestMethod]
        public void ConnectDisconnectedInputs_LinksSupplierToConsumer() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var graph = ctx.NewGraph();
            var session = GraphSessionTestHelper.AttachSession(graph);

            ItemQualityPair plate = ctx.Item("plate");
            session.Editor.CreateSupplierNode(plate, new System.Drawing.Point(0, 0));
            NodeId consumerId = session.Editor.CreateConsumerNode(plate, new System.Drawing.Point(100, 0));

            int created = GraphAutoconnect.ConnectDisconnectedInputs(session);

            Assert.AreEqual(1, created);
            session.View.TryGetNode(consumerId, out INodeViewModel? consumer);
            Assert.IsNotNull(consumer);
            Assert.Contains(link => link.Item == plate, consumer.InputLinks);
            Assert.HasCount(1, graph.NodeLinks);
        }

        [TestMethod]
        public void ConnectDisconnectedInputs_DoesNotSelfConnectPassthrough() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var graph = ctx.NewGraph();
            var session = GraphSessionTestHelper.AttachSession(graph);

            ItemQualityPair item = ctx.Item("wire");
            session.Editor.CreatePassthroughNode(item, new System.Drawing.Point(0, 0));

            int created = GraphAutoconnect.ConnectDisconnectedInputs(session);

            Assert.AreEqual(0, created);
            Assert.IsEmpty(graph.NodeLinks);
        }

        [TestMethod]
        public void ConnectDisconnectedInputs_PrefersNearestSupplier() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var graph = ctx.NewGraph();
            var session = GraphSessionTestHelper.AttachSession(graph);

            ItemQualityPair item = ctx.Item("gear");
            NodeId farSupplier = session.Editor.CreateSupplierNode(item, new System.Drawing.Point(0, 0));
            NodeId nearSupplier = session.Editor.CreateSupplierNode(item, new System.Drawing.Point(50, 0));
            NodeId consumer = session.Editor.CreateConsumerNode(item, new System.Drawing.Point(100, 0));
            _ = farSupplier;
            _ = nearSupplier;

            int created = GraphAutoconnect.ConnectDisconnectedInputs(session);

            Assert.AreEqual(1, created);
            NodeLink link = graph.NodeLinks.Single();
            Assert.AreEqual(nearSupplier.Value, link.SupplierNode.NodeID);
            Assert.AreEqual(consumer.Value, link.ConsumerNode.NodeID);
        }
    }
}
