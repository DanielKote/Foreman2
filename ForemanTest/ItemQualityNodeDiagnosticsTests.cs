using Foreman;
using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Linq;

namespace ForemanTest {
    [TestClass]
    public class ItemQualityNodeDiagnosticsTests : ForemanTestBase {
        [TestMethod]
        public void SupplierAndConsumer_SameErrorState_WhenQualityNotAvailable() {
            var ctx = GraphSessionTestHelper.CreateContext();
            ((QualityPrototype)ctx.Quality).Available = false;
            var graph = ctx.NewGraph();
            var supplier = new SupplierNode(graph, 1, ctx.Item("iron"));
            var consumer = new ConsumerNode(graph, 2, ctx.Item("iron"));

            supplier.UpdateState();
            consumer.UpdateState();

            Assert.AreEqual(NodeState.Error, supplier.State);
            Assert.AreEqual(NodeState.Error, consumer.State);
            Assert.AreEqual(supplier.ErrorSet, (SupplierNode.Errors)(int)consumer.ErrorSet);
            Assert.Contains("Quality", supplier.GetErrors().Single());
            Assert.Contains("Quality", consumer.GetErrors().Single());
        }

        [TestMethod]
        public void SupplierAndConsumer_SameWarningState_WhenItemDisabled() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var item = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "iron");
            item.Enabled = false;
            var pair = ctx.Item("iron");
            var graph = ctx.NewGraph();
            var supplier = new SupplierNode(graph, 1, pair);
            var consumer = new ConsumerNode(graph, 2, pair);

            supplier.UpdateState();
            consumer.UpdateState();

            Assert.AreEqual(NodeState.Warning, supplier.State);
            Assert.AreEqual(NodeState.Warning, consumer.State);
            Assert.AreEqual(supplier.WarningSet, (SupplierNode.Warnings)(int)consumer.WarningSet);

            string supplierWarning = supplier.GetWarnings().Single(w => w.Contains("Item"));
            string consumerWarning = consumer.GetWarnings().Single(w => w.Contains("Item"));
            Assert.Contains("iron", supplierWarning);
            Assert.Contains("iron", consumerWarning);
            Assert.DoesNotContain("Normal", supplierWarning);
            Assert.DoesNotContain("Normal", consumerWarning);
        }

        [TestMethod]
        public void ItemQualityNodeMessages_ItemDisabled_UsesItemFriendlyName() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var item = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "iron-ore");
            var pair = new ItemQualityPair(item, ctx.Quality);
            int warningSet = ItemQualityNodeMessages.ItemDisabled;

            var warnings = ItemQualityNodeMessages.GetWarnings(pair, warningSet);

            Assert.HasCount(1, warnings);
            Assert.Contains("iron-ore", warnings[0]);
            Assert.DoesNotContain("Normal", warnings[0]);
        }

        [TestMethod]
        public void Passthrough_ToString_DescribesPassthroughNode() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var node = new PassthroughNode(ctx.NewGraph(), 1, ctx.Item("belt"));

            Assert.Contains("Passthrough", node.ToString());
            Assert.IsFalse(node.ToString().StartsWith("Supply node", StringComparison.Ordinal));
        }

        [TestMethod]
        public void Passthrough_NewNode_UsesGraphDefaultSimpleDraw() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var graph = ctx.NewGraph();

            graph.DefaultToSimplePassthroughNodes = false;
            PassthroughNode plain = graph.CreatePassthroughNode(ctx.Item("belt"), Point.Empty);
            Assert.IsFalse(plain.SimpleDraw);

            graph.DefaultToSimplePassthroughNodes = true;
            PassthroughNode simplified = graph.CreatePassthroughNode(ctx.Item("iron"), Point.Empty);
            Assert.IsTrue(simplified.SimpleDraw);
        }

        [TestMethod]
        public void Spoil_ToString_IncludesInputOutputAndQualities() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var fresh = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "fresh");
            var rotten = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "rotten");
            GraphSessionTestHelper.WireSpoilChain(fresh, rotten, ctx.Quality);
            var node = new SpoilNode(ctx.NewGraph(), 1, new ItemQualityPair(fresh, ctx.Quality), rotten);

            string text = node.ToString();
            Assert.Contains("fresh", text);
            Assert.Contains("rotten", text);
            Assert.Contains("normal", text);
        }

        [TestMethod]
        public void Spoil_InvalidSpoilResult_MessageDescribesMismatch() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var fresh = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "fresh");
            var rotten = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "rotten");
            var other = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "other");
            GraphSessionTestHelper.WireSpoilChain(fresh, rotten, ctx.Quality);
            var node = new SpoilNode(ctx.NewGraph(), 1, new ItemQualityPair(fresh, ctx.Quality), other);
            node.UpdateState();

            Assert.AreNotEqual(SpoilNode.Errors.Clean, node.ErrorSet & SpoilNode.Errors.InvalidSpoilResult);
            string message = node.GetErrors().Single(e => e.Contains("Spoil result"));
            Assert.Contains("doesnt match", message);
            Assert.DoesNotContain("doesnt exist in preset", message);
        }
    }
}
