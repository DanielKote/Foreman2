using Foreman;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ForemanTest {
    [TestClass]
    public class ItemQualityNodeDiagnosticsTests {
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
            StringAssert.Contains(supplier.GetErrors().Single(), "Quality");
            StringAssert.Contains(consumer.GetErrors().Single(), "Quality");
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
            StringAssert.Contains(supplierWarning, "iron");
            StringAssert.Contains(consumerWarning, "iron");
            Assert.IsFalse(supplierWarning.Contains("Normal"));
            Assert.IsFalse(consumerWarning.Contains("Normal"));
        }

        [TestMethod]
        public void ItemQualityNodeMessages_ItemDisabled_UsesItemFriendlyName() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var item = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "iron-ore");
            var pair = new ItemQualityPair(item, ctx.Quality);
            int warningSet = ItemQualityNodeMessages.ItemDisabled;

            var warnings = ItemQualityNodeMessages.GetWarnings(pair, warningSet);

            Assert.AreEqual(1, warnings.Count);
            StringAssert.Contains(warnings[0], "iron-ore");
            Assert.IsFalse(warnings[0].Contains("Normal"));
        }

        [TestMethod]
        public void Passthrough_ToString_DescribesPassthroughNode() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var node = new PassthroughNode(ctx.NewGraph(), 1, ctx.Item("belt"));

            StringAssert.Contains(node.ToString(), "Passthrough");
            Assert.IsFalse(node.ToString().StartsWith("Supply node"));
        }

        [TestMethod]
        public void Spoil_ToString_IncludesInputOutputAndQualities() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var fresh = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "fresh");
            var rotten = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "rotten");
            GraphSessionTestHelper.WireSpoilChain(fresh, rotten, ctx.Quality);
            var node = new SpoilNode(ctx.NewGraph(), 1, new ItemQualityPair(fresh, ctx.Quality), rotten);

            string text = node.ToString();
            StringAssert.Contains(text, "fresh");
            StringAssert.Contains(text, "rotten");
            StringAssert.Contains(text, "normal");
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

            Assert.IsTrue((node.ErrorSet & SpoilNode.Errors.InvalidSpoilResult) != 0);
            string message = node.GetErrors().Single(e => e.Contains("Spoil result"));
            StringAssert.Contains(message, "doesnt match");
            Assert.IsFalse(message.Contains("doesnt exist in preset"));
        }
    }
}