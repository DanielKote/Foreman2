using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForemanTest.Graph {
    [TestClass]
    public class ProductionGraphSessionTests : ForemanTestBase {
        private static ItemQualityPair TestItem() {
            var cache = new DataCache(filterRecipes: true);
            var subgroup = new SubgroupPrototype(cache, "§§test:subgroup", "z");
            var quality = new QualityPrototype(cache, "normal", "Normal", "a");
            return new ItemQualityPair(TestDataCacheHelper.GetOrCreateItem(cache, subgroup, "iron"), quality);
        }

        [TestMethod]
        public void CreateSupplierNode_RegistersViewModelWithMatchingEpoch() {
            var graph = new ProductionGraph();
            var session = new ProductionGraphSession(graph);
            session.Attach();

            ItemQualityPair iron = TestItem();
            NodeId id = session.Editor.CreateSupplierNode(iron, new System.Drawing.Point(10, 20));

            Assert.IsTrue(id.IsValid);
            Assert.IsTrue(session.View.TryGetNode(id, out INodeViewModel? vm));
            Assert.IsNotNull(vm);
            Assert.AreEqual(NodeType.Supplier, vm.NodeType);
            Assert.AreEqual(new System.Drawing.Point(10, 20), vm.Location);
            Assert.IsInstanceOfType<ISupplierNodeViewModel>(vm);
        }

        [TestMethod]
        public void CreateLink_RegistersLinkViewModelBetweenNodes() {
            var graph = new ProductionGraph();
            var session = new ProductionGraphSession(graph);
            session.Attach();

            ItemQualityPair iron = TestItem();
            NodeId supplierId = session.Editor.CreateSupplierNode(iron, new System.Drawing.Point(0, 0));
            NodeId consumerId = session.Editor.CreateConsumerNode(iron, new System.Drawing.Point(100, 0));
            LinkId linkId = session.Editor.CreateLink(supplierId, consumerId, iron);

            Assert.IsTrue(linkId.IsValid);
            Assert.IsTrue(session.View.TryGetLink(linkId, out INodeLinkViewModel? linkVm));
            Assert.IsNotNull(linkVm);
            Assert.AreEqual(supplierId, linkVm.SupplierId);
            Assert.AreEqual(consumerId, linkVm.ConsumerId);
        }

        [TestMethod]
        public void DeleteNode_RemovesViewModelFromSession() {
            var graph = new ProductionGraph();
            var session = new ProductionGraphSession(graph);
            session.Attach();

            ItemQualityPair iron = TestItem();
            NodeId id = session.Editor.CreateSupplierNode(iron, new System.Drawing.Point(0, 0));
            session.Editor.DeleteNode(id);

            Assert.IsFalse(session.View.TryGetNode(id, out _));
        }

        [TestMethod]
        public void ClearGraph_ResetsSessionViewModels() {
            var graph = new ProductionGraph();
            var session = new ProductionGraphSession(graph);
            session.Attach();

            ItemQualityPair iron = TestItem();
            session.Editor.CreateSupplierNode(iron, new System.Drawing.Point(0, 0));
            graph.ClearGraph();

            Assert.IsEmpty(session.View.Nodes);
            Assert.IsEmpty(session.View.Links);
        }

        [TestMethod]
        public void NodeValuesUpdated_NotifiesViewModelListeners() {
            var graph = new ProductionGraph();
            var session = new ProductionGraphSession(graph);
            session.Attach();

            ItemQualityPair iron = TestItem();
            NodeId id = session.Editor.CreateSupplierNode(iron, new System.Drawing.Point(0, 0));
            Assert.IsTrue(session.View.TryGetNode(id, out INodeViewModel? vm));
            Assert.IsNotNull(vm);

            int valuesChangedCount = 0;
            vm.NodeValuesChanged += (_, _) => valuesChangedCount++;

            graph.UpdateNodeValues();

            Assert.IsGreaterThanOrEqualTo(1, valuesChangedCount);
        }
    }
}
