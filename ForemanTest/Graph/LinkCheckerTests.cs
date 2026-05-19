using Foreman;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Linq;

namespace ForemanTest.Graph {
    [TestClass]
    public class LinkCheckerTests : ForemanTestBase {
        [TestMethod]
        public void IsPossibleConnection_ValidItemFlow_ReturnsTrue() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            ISupplierNodeViewModel supplier = session.View.Nodes.OfType<ISupplierNodeViewModel>().First();
            IRecipeNodeViewModel recipe = session.View.Nodes.OfType<IRecipeNodeViewModel>().First();

            ItemQualityPair ore = supplier.SuppliedItem;
            Assert.IsTrue(LinkChecker.IsPossibleConnection(ore, supplier, recipe, session));
        }

        [TestMethod]
        public void IsPossibleConnection_WrongItem_ReturnsFalse() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            NodeId supplierId = session.Editor.CreateSupplierNode(ctx.Item("iron"), System.Drawing.Point.Empty);
            NodeId consumerId = session.Editor.CreateConsumerNode(ctx.Item("copper"), new System.Drawing.Point(40, 0));

            INodeViewModel supplier = session.View.Nodes.First(n => n.Id == supplierId);
            INodeViewModel consumer = session.View.Nodes.First(n => n.Id == consumerId);

            Assert.IsFalse(LinkChecker.IsPossibleConnection(ctx.Item("iron"), supplier, consumer, session));
        }

        [TestMethod]
        public void IsPossibleConnection_ViewModelOverload_MatchesDomainNodes() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            ISupplierNodeViewModel supplierVm = session.View.Nodes.OfType<ISupplierNodeViewModel>().First();
            IRecipeNodeViewModel recipeVm = session.View.Nodes.OfType<IRecipeNodeViewModel>().First();
            Assert.IsTrue(session.TryGetDomainNode(supplierVm.Id, out BaseNode? supplierNode));
            Assert.IsTrue(session.TryGetDomainNode(recipeVm.Id, out BaseNode? recipeNode));
            Assert.IsNotNull(supplierNode);
            Assert.IsNotNull(recipeNode);

            ItemQualityPair ore = supplierVm.SuppliedItem;
            bool viaViewModels = LinkChecker.IsPossibleConnection(ore, supplierVm, recipeVm, session);
            bool viaDomain = LinkChecker.IsPossibleConnection(ore, supplierNode, recipeNode);

            Assert.AreEqual(viaDomain, viaViewModels);
        }

        [TestMethod]
        public void IsPossibleConnection_IncompatibleFluidTemperatures_ReturnsFalse() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var graph = ctx.NewGraph();
            var session = GraphSessionTestHelper.AttachSession(graph);
            FluidPrototype steam = TestDataCacheHelper.GetOrCreateFluid(ctx.Cache, ctx.Subgroup, "steam");

            RecipePrototype producer = CreateRecipe(ctx, "produce-steam");
            producer.InternalOneWayAddProduct(steam, 1, 0, 500);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, producer);

            RecipePrototype consumer = CreateRecipe(ctx, "consume-steam");
            consumer.InternalOneWayAddIngredient(steam, 1, 0, 100);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, consumer);

            NodeId producerId = session.Editor.CreateRecipeNode(new RecipeQualityPair(producer, ctx.Quality), System.Drawing.Point.Empty);
            NodeId consumerId = session.Editor.CreateRecipeNode(new RecipeQualityPair(consumer, ctx.Quality), new System.Drawing.Point(120, 0));
            var steamPair = new ItemQualityPair(steam, ctx.Quality);

            INodeViewModel producerVm = session.View.Nodes.First(n => n.Id == producerId);
            INodeViewModel consumerVm = session.View.Nodes.First(n => n.Id == consumerId);

            Assert.IsFalse(LinkChecker.IsPossibleConnection(steamPair, producerVm, consumerVm, session));
        }

        [TestMethod]
        public void IsPossibleConnection_CompatibleFluidTemperatures_ReturnsTrue() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var graph = ctx.NewGraph();
            var session = GraphSessionTestHelper.AttachSession(graph);
            FluidPrototype steam = TestDataCacheHelper.GetOrCreateFluid(ctx.Cache, ctx.Subgroup, "steam");

            RecipePrototype producer = CreateRecipe(ctx, "produce-steam-ok");
            producer.InternalOneWayAddProduct(steam, 1, 0, 100);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, producer);

            RecipePrototype consumer = CreateRecipe(ctx, "consume-steam-ok");
            consumer.InternalOneWayAddIngredient(steam, 1, 0, 500);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, consumer);

            NodeId producerId = session.Editor.CreateRecipeNode(new RecipeQualityPair(producer, ctx.Quality), System.Drawing.Point.Empty);
            NodeId consumerId = session.Editor.CreateRecipeNode(new RecipeQualityPair(consumer, ctx.Quality), new System.Drawing.Point(120, 0));
            var steamPair = new ItemQualityPair(steam, ctx.Quality);

            INodeViewModel producerVm = session.View.Nodes.First(n => n.Id == producerId);
            INodeViewModel consumerVm = session.View.Nodes.First(n => n.Id == consumerId);

            Assert.IsTrue(LinkChecker.IsPossibleConnection(steamPair, producerVm, consumerVm, session));
        }

        [TestMethod]
        public void GetTemperatureRange_ViewModelOverload_MatchesDomainNode() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);
            IRecipeNodeViewModel recipeVm = session.View.Nodes.OfType<IRecipeNodeViewModel>().First();
            Assert.IsTrue(session.TryGetDomainNode(recipeVm.Id, out BaseNode? recipeNode));

            FRange viaVm = LinkChecker.GetTemperatureRange(null, recipeVm, LinkType.Output, true, session);
            FRange viaDomain = LinkChecker.GetTemperatureRange(null, recipeNode, LinkType.Output, true);

            Assert.AreEqual(viaDomain.Ignore, viaVm.Ignore);
            Assert.AreEqual(viaDomain.Min, viaVm.Min, 1e-9);
            Assert.AreEqual(viaDomain.Max, viaVm.Max, 1e-9);
        }

        [TestMethod]
        public void CreateLink_InvalidTemperature_MarksLinkInvalid() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            FluidPrototype steam = TestDataCacheHelper.GetOrCreateFluid(ctx.Cache, ctx.Subgroup, "steam");

            RecipePrototype producer = CreateRecipe(ctx, "produce-steam-link");
            producer.InternalOneWayAddProduct(steam, 1, 0, 500);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, producer);

            RecipePrototype consumer = CreateRecipe(ctx, "consume-steam-link");
            consumer.InternalOneWayAddIngredient(steam, 1, 0, 100);
            TestDataCacheHelper.RegisterRecipe(ctx.Cache, consumer);

            NodeId producerId = session.Editor.CreateRecipeNode(new RecipeQualityPair(producer, ctx.Quality), System.Drawing.Point.Empty);
            NodeId consumerId = session.Editor.CreateRecipeNode(new RecipeQualityPair(consumer, ctx.Quality), new System.Drawing.Point(120, 0));
            var steamPair = new ItemQualityPair(steam, ctx.Quality);

            LinkId linkId = session.Editor.CreateLink(producerId, consumerId, steamPair);
            Assert.IsTrue(session.View.TryGetLink(linkId, out INodeLinkViewModel? linkVm));
            Assert.IsNotNull(linkVm);
            Assert.IsFalse(linkVm.IsValid);
        }

        private static RecipePrototype CreateRecipe(GraphSessionTestHelper.TestContext ctx, string name) {
            var recipe = new RecipePrototype(ctx.Cache, name, name, ctx.Subgroup, "z");
            TestPrototypeFactory.SetRecipeTime(recipe, 1);
            TestPrototypeFactory.LinkRecipeAndAssembler(recipe, TestPrototypeFactory.CreateTestAssembler(ctx.Cache));
            return recipe;
        }
    }

    [TestClass]
    public class NodeViewModelPropertyChangedTests : ForemanTestBase {
        [TestMethod]
        public void KeyNode_PropertyChanged_FiresWhenControllerSetsKeyNode() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);
            IRecipeNodeViewModel recipeVm = session.View.Nodes.OfType<IRecipeNodeViewModel>().First();

            int keyNodeChanges = 0;
            int keyNodeTitleChanges = 0;
            ((INotifyPropertyChanged)recipeVm).PropertyChanged += (_, e) => {
                if (e.PropertyName == nameof(INodeViewModel.KeyNode))
                    keyNodeChanges++;
                if (e.PropertyName == nameof(INodeViewModel.KeyNodeTitle))
                    keyNodeTitleChanges++;
            };

            RecipeNodeController controller = session.Editor.RequestNodeController(recipeVm.Id) as RecipeNodeController
                ?? throw new AssertFailedException("Recipe node should have a RecipeNodeController.");
            controller.SetKeyNode(true);

            Assert.IsTrue(recipeVm.KeyNode);
            Assert.IsGreaterThanOrEqualTo(1, keyNodeChanges);
            Assert.IsGreaterThanOrEqualTo(1, keyNodeTitleChanges);
        }

        [TestMethod]
        public void KeyNodeTitle_PropertyChanged_FiresWhenControllerSetsTitle() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);
            IRecipeNodeViewModel recipeVm = session.View.Nodes.OfType<IRecipeNodeViewModel>().First();
            RecipeNodeController controller = session.Editor.RequestNodeController(recipeVm.Id) as RecipeNodeController
                ?? throw new AssertFailedException("Recipe node should have a RecipeNodeController.");
            controller.SetKeyNode(true);

            int titleChanges = 0;
            ((INotifyPropertyChanged)recipeVm).PropertyChanged += (_, e) => {
                if (e.PropertyName == nameof(INodeViewModel.KeyNodeTitle))
                    titleChanges++;
            };

            controller.SetKeyNodeTitle("Main bus");

            Assert.AreEqual("Main bus", recipeVm.KeyNodeTitle);
            Assert.IsGreaterThanOrEqualTo(1, titleChanges);
        }
    }
}
