using Foreman;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models.Nodes;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace ForemanTest.Graph {
    [TestClass]
    public class ProductionGraphSessionBridgeTests : ForemanTestBase {
        [TestMethod]
        public void TryGetDomainLink_RoundTripsLinkFromViewModel() {
            var ctx = GraphSessionTestHelper.CreateContext();
            var session = GraphSessionTestHelper.AttachSession(ctx.NewGraph());
            var item = ctx.Item("iron");
            NodeId supplierId = session.Editor.CreateSupplierNode(item, System.Drawing.Point.Empty);
            NodeId consumerId = session.Editor.CreateConsumerNode(item, new System.Drawing.Point(40, 0));
            LinkId linkId = session.Editor.CreateLink(supplierId, consumerId, item);

            Assert.IsTrue(session.View.TryGetLink(linkId, out INodeLinkViewModel? linkVm));
            Assert.IsTrue(session.TryGetDomainLink(linkId, out NodeLink? domainLink));

            Assert.IsNotNull(linkVm);
            Assert.IsNotNull(domainLink);
            Assert.AreEqual(linkVm.Item, domainLink.Item);
            Assert.AreEqual(linkVm.SupplierId, supplierId);
            Assert.AreEqual(linkVm.ConsumerId, consumerId);
        }

        [TestMethod]
        public void Session_TryGetDomain_ResolvesEveryViewModel() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            int resolvedNodes = session.View.Nodes.Count(vm => session.TryGetDomainNode(vm.Id, out _));
            int resolvedLinks = session.View.Links.Count(vm => session.TryGetDomainLink(vm.Id, out _));

            Assert.AreEqual(built.Graph.Nodes.Count(), resolvedNodes);
            Assert.AreEqual(built.Graph.NodeLinks.Count(), resolvedLinks);
        }

        [TestMethod]
        public void Editor_RequestNodeController_ReturnsControllersForCreatedNodes() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            INodeViewModel supplierVm = session.View.Nodes.OfType<ISupplierNodeViewModel>().First();
            IRecipeNodeViewModel recipeVm = session.View.Nodes.OfType<IRecipeNodeViewModel>().First();

            Assert.IsInstanceOfType<BaseNodeController>(session.Editor.RequestNodeController(supplierVm.Id));
            Assert.IsInstanceOfType<RecipeNodeController>(session.Editor.RequestNodeController(recipeVm.Id));
        }

        [TestMethod]
        public void RecipeNodeViewModel_CalculationMethods_MatchDomainRecipeNode() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            built.Solve();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            IRecipeNodeViewModel viewModel = session.View.Nodes.OfType<IRecipeNodeViewModel>().First();
            Assert.IsTrue(session.TryGetDomainNode(viewModel.Id, out BaseNode? domainNode));
            Assert.IsInstanceOfType<RecipeNode>(domainNode);
            var recipeNode = (RecipeNode)domainNode;

            const double tolerance = 1e-9;
            Assert.AreEqual(recipeNode.GetAssemblerSpeed(), viewModel.GetAssemblerSpeed(), tolerance);
            Assert.AreEqual(recipeNode.GetTotalCrafts(), viewModel.GetTotalCrafts(), tolerance);
            Assert.AreEqual(recipeNode.GetTotalAssemblerFuelConsumption(), viewModel.GetTotalAssemblerFuelConsumption(), tolerance);
            Assert.AreEqual(recipeNode.GetAssemblerEnergyConsumption(), viewModel.GetAssemblerEnergyConsumption(), tolerance);
            Assert.AreEqual(recipeNode.GetAssemblerPollutionProduction(), viewModel.GetAssemblerPollutionProduction(), tolerance);
            if (viewModel.SelectedAssembler.Assembler.EntityType == EntityType.Generator) {
                Assert.AreEqual(recipeNode.GetGeneratorMinimumTemperature(), viewModel.GetGeneratorMinimumTemperature(), tolerance);
                Assert.AreEqual(recipeNode.GetGeneratorMaximumTemperature(), viewModel.GetGeneratorMaximumTemperature(), tolerance);
                Assert.AreEqual(recipeNode.GetGeneratorEffectivity(), viewModel.GetGeneratorEffectivity(), tolerance);
                Assert.AreEqual(recipeNode.GetGeneratorElectricalProduction(), viewModel.GetGeneratorElectricalProduction(), tolerance);
            }
            Assert.AreEqual(recipeNode.GetBeaconEnergyConsumption(), viewModel.GetBeaconEnergyConsumption(), tolerance);
            Assert.AreEqual(recipeNode.GetTotalAssemblerElectricalConsumption(), viewModel.GetTotalAssemblerElectricalConsumption(), tolerance);
            Assert.AreEqual(recipeNode.GetTotalGeneratorElectricalProduction(), viewModel.GetTotalGeneratorElectricalProduction(), tolerance);
            Assert.AreEqual(recipeNode.GetTotalBeacons(), viewModel.GetTotalBeacons());
            Assert.AreEqual(recipeNode.GetTotalBeaconElectricalConsumption(), viewModel.GetTotalBeaconElectricalConsumption(), tolerance);
            Assert.AreEqual(recipeNode.GetConsumptionMultiplier(), viewModel.GetConsumptionMultiplier(), tolerance);
            Assert.AreEqual(recipeNode.GetSpeedMultiplier(), viewModel.GetSpeedMultiplier(), tolerance);
            Assert.AreEqual(recipeNode.GetProductivityBonus() + 1, viewModel.GetProductivityMultiplier(), tolerance);
        }

        [TestMethod]
        public void RecipeNodeViewModel_BuildingTotals_MatchDomainAggregation() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            built.Solve();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            var viewModels = session.View.Nodes.OfType<IRecipeNodeViewModel>().ToList();
            var recipeNodes = built.Graph.Nodes.OfType<RecipeNode>().ToList();

            double vmBuildings = viewModels.Sum(n => Math.Ceiling(n.ActualSetValue));
            double domainBuildings = recipeNodes.Sum(n => Math.Ceiling(n.ActualSetValue));
            double vmBeacons = viewModels.Sum(n => n.GetTotalBeacons());
            double domainBeacons = recipeNodes.Sum(n => n.GetTotalBeacons());
            double vmPowerIn = viewModels.Sum(n => n.GetTotalAssemblerElectricalConsumption() + n.GetTotalBeaconElectricalConsumption());
            double domainPowerIn = recipeNodes.Sum(n => n.GetTotalAssemblerElectricalConsumption() + n.GetTotalBeaconElectricalConsumption());
            double vmPowerOut = viewModels.Sum(n => n.GetTotalGeneratorElectricalProduction());
            double domainPowerOut = recipeNodes.Sum(n => n.GetTotalGeneratorElectricalProduction());

            Assert.AreEqual(domainBuildings, vmBuildings);
            Assert.AreEqual(domainBeacons, vmBeacons);
            Assert.AreEqual(domainPowerIn, vmPowerIn, 1e-6);
            Assert.AreEqual(domainPowerOut, vmPowerOut, 1e-6);
        }

        [TestMethod]
        public void LinkViewModel_Throughput_MatchesDomainLinkAfterSolve() {
            GraphBuilder.BuiltData built = GraphSessionTestHelper.BuildSimpleChain();
            built.Solve();
            var session = GraphSessionTestHelper.AttachSession(built.Graph);

            foreach (INodeLinkViewModel linkVm in session.View.Links) {
                Assert.IsTrue(session.TryGetDomainLink(linkVm.Id, out NodeLink? domainLink));
                Assert.IsNotNull(domainLink);
                Assert.AreEqual(domainLink.Throughput, linkVm.Throughput, 1e-6);
            }
        }

    }
}
