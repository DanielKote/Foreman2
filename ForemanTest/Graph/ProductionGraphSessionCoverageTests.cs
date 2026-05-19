using Foreman;
using Foreman.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ForemanTest.Graph {
    [TestClass]
    public class ProductionGraphSessionCoverageTests : ForemanTestBase {
        [TestMethod]
        public void Attach_BackfillsExistingDomainNodesAndLinks() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            int domainNodeCount = built.Graph.Nodes.Count();
            int domainLinkCount = built.Graph.NodeLinks.Count();

            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            Assert.AreEqual(domainNodeCount, session.View.Nodes.Count);
            Assert.AreEqual(domainLinkCount, session.View.Links.Count);
            Assert.IsTrue(session.View.Nodes.Any(n => n is IRecipeNodeViewModel));
            Assert.IsTrue(session.View.Nodes.Any(n => n is ISupplierNodeViewModel));
            Assert.IsTrue(session.View.Nodes.Any(n => n is IConsumerNodeViewModel));
        }

        [TestMethod]
        public void Attach_IsIdempotent_SecondAttachDoesNotDuplicateViewModels() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            var session = new ProductionGraphSession(built.Graph);
            session.Attach();
            int countAfterFirst = session.View.Nodes.Count;
            session.Attach();
            Assert.AreEqual(countAfterFirst, session.View.Nodes.Count);
        }

        [TestMethod]
        public void Detach_StopsSynchronizingNewDomainNodes() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var graph = ctx.NewGraph();
            var session = GraphSessionTestHelper.AttachSession(graph);
            session.Detach();

            graph.CreateSupplierNode(ctx.Item("iron"), Point.Empty);

            Assert.AreEqual(1, graph.Nodes.Count());
            Assert.AreEqual(0, session.View.Nodes.Count);
        }

        [TestMethod]
        public void LoadProductionGraphDocument_WithAttachedSession_RegistersAllViewModels() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            ProductionGraphSaveDocument document = GraphSaveCodec.BuildProductionGraph(built.Graph);

            var graph = new ProductionGraph { DefaultAssemblerQuality = built.Graph.DefaultAssemblerQuality };

            var session = GraphSessionTestHelper.AttachSession(graph);
            ProductionGraph.NewNodeCollection imported = GraphSaveLoader.LoadProductionGraph(
                graph, built.Cache, document, applySolverSettings: true);

            Assert.AreEqual(imported.newNodes.Count, session.View.Nodes.Count);
            Assert.AreEqual(imported.newLinks.Count, session.View.Links.Count);
            foreach (BaseNode importedNode in imported.newNodes) {
                INodeViewModel? vm = session.View.Nodes.FirstOrDefault(n => n.Id.Value == importedNode.NodeID);
                Assert.IsNotNull(vm, $"Missing view model for node id {importedNode.NodeID}.");
            }
        }

        [TestMethod]
        public void CreateNodeViewModels_AllPrimaryNodeTypes_UseTypedViewModels() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var graph = ctx.NewGraph();
            var session = GraphSessionTestHelper.AttachSession(graph);

            NodeId supplierId = session.Editor.CreateSupplierNode(ctx.Item("ore"), new Point(0, 0));
            NodeId consumerId = session.Editor.CreateConsumerNode(ctx.Item("ore"), new Point(50, 0));
            NodeId passthroughId = session.Editor.CreatePassthroughNode(ctx.Item("ore"), new Point(100, 0));

            var recipe = new RecipePrototype(ctx.Cache, "test-recipe", "Test Recipe", ctx.Subgroup, "z");
            TestPrototypeFactory.SetRecipeTime(recipe, 1);
            TestPrototypeFactory.LinkRecipeAndAssembler(recipe, TestPrototypeFactory.CreateTestAssembler(ctx.Cache));
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, recipe);
            var oreItem = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "ore");
            var plateItem = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "plate");
            recipe.InternalOneWayAddIngredient(oreItem, 1);
            recipe.InternalOneWayAddProduct(plateItem, 1, 0);
            NodeId recipeId = session.Editor.CreateRecipeNode(new RecipeQualityPair(recipe, ctx.Quality), new Point(150, 0));

            Assert.IsInstanceOfType(GetVm(session, supplierId), typeof(ISupplierNodeViewModel));
            Assert.IsInstanceOfType(GetVm(session, consumerId), typeof(IConsumerNodeViewModel));
            Assert.IsInstanceOfType(GetVm(session, passthroughId), typeof(IPassthroughNodeViewModel));
            Assert.IsInstanceOfType(GetVm(session, recipeId), typeof(IRecipeNodeViewModel));
        }

        [TestMethod]
        public void CreateSpoilNode_RegistersSpoilViewModel() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            var fresh = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "fresh");
            var spoiled = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, "rotten");
            GraphSessionTestHelper.WireSpoilChain(fresh, spoiled, ctx.Quality);

            NodeId spoilId = session.Editor.CreateSpoilNode(ctx.Item("fresh"), spoiled, new Point(0, 0));
            Assert.IsInstanceOfType(GetVm(session, spoilId), typeof(ISpoilNodeViewModel));
        }

        [TestMethod]
        public void CreatePlantNode_RegistersPlantViewModel() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            PlantProcessPrototype plantProcess = GraphSessionTestHelper.CreatePlantProcess(ctx, "seed", "crop");
            NodeId plantId = session.Editor.CreatePlantNode(plantProcess, ctx.Quality, new Point(0, 0));
            Assert.IsInstanceOfType(GetVm(session, plantId), typeof(IPlantNodeViewModel));
        }

        [TestMethod]
        public void TryGetDomainNode_RoundTripsRecipeNodeFromViewModel() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            IRecipeNodeViewModel recipeVm = session.View.Nodes.OfType<IRecipeNodeViewModel>().First();
            Assert.IsTrue(session.TryGetDomainNode(recipeVm.Id, out BaseNode? domainNode));
            Assert.IsInstanceOfType(domainNode, typeof(RecipeNode));

            Recipe? viewRecipe = recipeVm.BaseRecipe.Recipe;
            Assert.IsInstanceOfType(domainNode, typeof(RecipeNode));
            Recipe? domainRecipe = ((RecipeNode)domainNode).BaseRecipe.Recipe;
            Assert.IsNotNull(viewRecipe);
            Assert.IsNotNull(domainRecipe);
            Assert.AreEqual(viewRecipe.Name, domainRecipe.Name);
            Assert.AreEqual(recipeVm.Id.Value, domainNode.NodeID);
        }

        [TestMethod]
        public void Editor_SetLocation_UpdatesViewModelLocation() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            NodeId id = session.Editor.CreateSupplierNode(ctx.Item("iron"), Point.Empty);

            session.Editor.SetLocation(id, new Point(12, 34));
            INodeViewModel vm = GetVm(session, id);

            Assert.AreEqual(new Point(12, 34), vm.Location);
        }

        [TestMethod]
        public void Editor_SetDirection_UpdatesViewModelDirection() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            NodeId id = session.Editor.CreateSupplierNode(ctx.Item("iron"), Point.Empty);

            session.Editor.SetDirection(id, NodeDirection.Down);
            Assert.AreEqual(NodeDirection.Down, GetVm(session, id).NodeDirection);
        }

        [TestMethod]
        public void DeleteLink_RemovesLinkViewModelAndClearsEndpointLinkLists() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            ItemQualityPair item = ctx.Item("iron");
            NodeId supplierId = session.Editor.CreateSupplierNode(item, new Point(0, 0));
            NodeId consumerId = session.Editor.CreateConsumerNode(item, new Point(80, 0));
            LinkId linkId = session.Editor.CreateLink(supplierId, consumerId, item);

            IConsumerNodeViewModel consumerVm = (IConsumerNodeViewModel)GetVm(session, consumerId);
            Assert.AreEqual(1, consumerVm.InputLinks.Count);

            session.Editor.DeleteLink(linkId);

            Assert.IsFalse(session.View.TryGetLink(linkId, out _));
            Assert.AreEqual(0, consumerVm.InputLinks.Count);
            Assert.AreEqual(0, ((ISupplierNodeViewModel)GetVm(session, supplierId)).OutputLinks.Count);
        }

        [TestMethod]
        public void DeleteNode_RemovesConnectedLinkViewModels() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            ItemQualityPair item = ctx.Item("iron");
            NodeId supplierId = session.Editor.CreateSupplierNode(item, Point.Empty);
            NodeId consumerId = session.Editor.CreateConsumerNode(item, new Point(40, 0));
            LinkId linkId = session.Editor.CreateLink(supplierId, consumerId, item);

            session.Editor.DeleteNode(supplierId);

            Assert.IsFalse(session.View.TryGetNode(supplierId, out _));
            Assert.IsFalse(session.View.TryGetLink(linkId, out _));
        }

        [TestMethod]
        public void ClearGraph_InvalidatesPriorNodeIdsAndUsesNewEpoch() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            NodeId firstId = session.Editor.CreateSupplierNode(ctx.Item("iron"), Point.Empty);
            uint firstEpoch = firstId.Epoch;

            session.Graph.ClearGraph();

            Assert.IsFalse(session.View.TryGetNode(firstId, out _));
            Assert.IsFalse(session.TryGetDomainNode(firstId, out _));

            NodeId secondId = session.Editor.CreateSupplierNode(ctx.Item("iron"), Point.Empty);
            Assert.AreNotEqual(firstEpoch, secondId.Epoch);
            Assert.IsTrue(secondId.IsValid);
        }

        [TestMethod]
        public void SessionEvents_FireOnCreateAndDelete() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            ItemQualityPair item = ctx.Item("iron");

            int nodesAdded = 0, nodesRemoved = 0, linksAdded = 0, linksRemoved = 0, cleared = 0;
            session.NodeViewModelAdded += (_, _) => nodesAdded++;
            session.NodeViewModelRemoved += (_, _) => nodesRemoved++;
            session.LinkViewModelAdded += (_, _) => linksAdded++;
            session.LinkViewModelRemoved += (_, _) => linksRemoved++;
            session.GraphCleared += (_, _) => cleared++;

            NodeId supplierId = session.Editor.CreateSupplierNode(item, Point.Empty);
            NodeId consumerId = session.Editor.CreateConsumerNode(item, new Point(30, 0));
            LinkId linkId = session.Editor.CreateLink(supplierId, consumerId, item);

            Assert.AreEqual(2, nodesAdded);
            Assert.AreEqual(1, linksAdded);

            session.Editor.DeleteLink(linkId);
            Assert.AreEqual(1, linksRemoved);

            session.Editor.DeleteNode(supplierId);
            Assert.IsTrue(nodesRemoved >= 1);

            session.Graph.ClearGraph();
            Assert.AreEqual(1, cleared);
            Assert.AreEqual(0, session.View.Nodes.Count);
        }

        [TestMethod]
        public void NodeStateChanged_FiresWhenLinkCompletesConsumer() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            ItemQualityPair item = ctx.Item("iron");
            NodeId supplierId = session.Editor.CreateSupplierNode(item, Point.Empty);
            NodeId consumerId = session.Editor.CreateConsumerNode(item, new Point(40, 0));

            INodeViewModel consumerVm = GetVm(session, consumerId);
            int stateChanges = 0;
            consumerVm.NodeStateChanged += (_, _) => stateChanges++;

            session.Editor.CreateLink(supplierId, consumerId, item);

            Assert.IsTrue(stateChanges >= 1);
            Assert.AreEqual(NodeState.Clean, consumerVm.State);
        }

        [TestMethod]
        public void Flowchart_Load_WithAttachedSession_BackfillsAllViewModels() {
            if (!SpaceAgeDataCacheFixture.PresetsAvailable)
                Assert.Inconclusive($"Space Age preset folder not found: {SpaceAgeDataCacheFixture.PresetsDirectory}");

            string path = FlowchartSample.ResolvePath();
            GraphViewerSaveDocument? saveDocument = GraphSaveCodec.ReadViewer(System.IO.File.ReadAllText(path));
            Assert.IsNotNull(saveDocument);

            var graph = new ProductionGraph();
            GraphSaveTestUi.ApplyViewerUiToGraph(saveDocument, SpaceAgeDataCacheFixture.GetLoadedAsync().GetAwaiter().GetResult(), graph);

            var session = GraphSessionTestHelper.AttachSession(graph);
            ProductionGraph.NewNodeCollection imported = GraphSaveLoader.LoadProductionGraph(
                graph,
                SpaceAgeDataCacheFixture.GetLoadedAsync().GetAwaiter().GetResult(),
                saveDocument.ProductionGraph,
                applySolverSettings: true);

            Assert.AreEqual(imported.newNodes.Count, session.View.Nodes.Count);
            Assert.AreEqual(imported.newLinks.Count, session.View.Links.Count);
            Assert.AreEqual(saveDocument.ProductionGraph.Nodes.Count, session.View.Nodes.Count);
        }

        private static INodeViewModel GetVm(ProductionGraphSession session, NodeId id) {
            Assert.IsTrue(session.View.TryGetNode(id, out INodeViewModel? vm));
            Assert.IsNotNull(vm);
            return vm;
        }
    }
}