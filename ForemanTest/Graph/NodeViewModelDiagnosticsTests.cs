using Foreman;
using Foreman.Graph;
using Foreman.Models.Nodes;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ForemanTest.Graph {
    [TestClass]
    public class NodeViewModelDiagnosticsTests : ForemanTestBase {
        [TestMethod]
        public void ViewModel_GetErrorsAndWarnings_MatchDomainNodes_ForSimpleChain() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            built.Solve();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            foreach (INodeViewModel vm in session.View.Nodes) {
                Assert.IsTrue(session.TryGetDomainNode(vm.Id, out BaseNode? domainNode));
                Assert.IsNotNull(domainNode);
                CollectionAssert.AreEqual(domainNode.GetErrors(), vm.GetErrors());
                CollectionAssert.AreEqual(domainNode.GetWarnings(), vm.GetWarnings());
            }
        }

        [TestMethod]
        public void RecipeNodeViewModel_GetErrorsAndWarnings_MatchDomainRecipeNode() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            built.Solve();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            IRecipeNodeViewModel recipeVm = session.View.Nodes.OfType<IRecipeNodeViewModel>().First();
            Assert.IsTrue(session.TryGetDomainNode(recipeVm.Id, out BaseNode? domainNode));
            Assert.IsInstanceOfType<RecipeNode>(domainNode);
            var recipeNode = (RecipeNode)domainNode;

            CollectionAssert.AreEqual(recipeNode.GetErrors(), recipeVm.GetErrors());
            CollectionAssert.AreEqual(recipeNode.GetWarnings(), recipeVm.GetWarnings());
        }

        [TestMethod]
        public void SupplierNodeViewModel_GetErrorsAndWarnings_MatchDomainSupplierNode() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            NodeId supplierId = session.Editor.CreateSupplierNode(ctx.Item("iron"), System.Drawing.Point.Empty);

            var supplierVm = (ISupplierNodeViewModel)session.View.Nodes.First(n => n.Id == supplierId);
            Assert.IsTrue(session.TryGetDomainNode(supplierId, out BaseNode? domainNode));
            Assert.IsInstanceOfType<SupplierNode>(domainNode);
            var supplierNode = (SupplierNode)domainNode;

            CollectionAssert.AreEqual(supplierNode.GetErrors(), supplierVm.GetErrors());
            CollectionAssert.AreEqual(supplierNode.GetWarnings(), supplierVm.GetWarnings());
        }
    }
}
