using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Foreman;
using System;

namespace ForemanTest
{
    [TestClass]
    public class SolverTest
    {
        [TestMethod]
        public void TestBasicSolve()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Plate"),
                builder.Consumer("Plate").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.SupplyRate("Plate"));
        }

        [TestMethod]
        public void TestBasicRecipe()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Ore"),
                builder.Recipe().Input("Ore", 1).Output("Plate", 1),
                builder.Consumer("Plate").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.SupplyRate("Ore"));
        }

        [TestMethod]
        public void TestRecipeThatReducesItems()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Plate"),
                builder.Recipe().Input("Plate", 5).Output("Steel", 1),
                builder.Consumer("Steel").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(50, data.SupplyRate("Plate"));
        }

        [TestMethod]
        public void TestRecipeThatMultipliesItems()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Plate"),
                builder.Recipe().Input("Plate", 1).Output("Wire", 2),
                builder.Consumer("Wire").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(5, data.SupplyRate("Plate"));
        }

        [TestMethod]
        public void TestRecipeWithUnevenInputOutputRatio()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Plate"),
                builder.Recipe().Input("Plate", 2).Output("Wire", 3),
                builder.Consumer("Wire").Target(12)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(8, data.SupplyRate("Plate"));
        }

        [TestMethod]
        public void TestMultipleInput()
        {
            var builder = GraphBuilder.Create();
            var consumer = builder.Consumer("Plate").Target(10);

            builder.Link(
                builder.Supply("Plate"),
                consumer
            );
            builder.Link(
                builder.Supply("Plate"),
                consumer
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            var production = data.SupplyRate("Plate");
            Assert.IsTrue(production == 10, "Suppliers misproduced: " + production);
        }

        [TestMethod]
        public void TestMultipleOutput()
        {
            var builder = GraphBuilder.Create();
            var supplier = builder.Supply("Plate");

            builder.Link(
                supplier,
                builder.Consumer("Plate").Target(10)
            );
            builder.Link(
                supplier,
                builder.Consumer("Plate").Target(5)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(15, data.SupplyRate("Plate"));
        }

        [TestMethod]
        public void TestFixedSupply()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Plate").Target(10),
                builder.Consumer("Plate")
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.ConsumedRate("Plate"));
        }

        [TestMethod]
        public void TestFixedRecipe()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Ore"),
                builder.Recipe().Input("Ore", 1).Output("Plate", 1).Target(10),
                builder.Consumer("Plate")
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.SupplyRate("Ore"));
            AssertFloatsAreEqual(10, data.ConsumedRate("Plate"));
        }

        [TestMethod]
        public void TestPartialGraph()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Recipe().Input("Ore", 1).Output("Plate", 1)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(0, data.Graph.Nodes[0].actualRate);
        }

        [TestMethod]
        public void TestSingleFixedRecipe()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Recipe().Input("Ore", 1).Output("Plate", 1).Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.Graph.Nodes[0].actualRate);
        }

        [TestMethod]
        public void TestDisjointGraph()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Plate"),
                builder.Consumer("Plate").Target(10)
            );

            builder.Link(
                builder.Supply("Ore"),
                builder.Consumer("Ore").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.SupplyRate("Plate"));
            AssertFloatsAreEqual(10, data.SupplyRate("Ore"));
        }

        [TestMethod]
        public void TestRecipeLinkedToSelf()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Ore"),
                builder.Recipe().Input("Ore", 1).Input("Tool", 1).Output("Plate", 1).Output("Tool", 1),
                builder.Consumer("Plate").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.SupplyRate("Ore"));
        }

        [TestMethod]
        public void TestCycle()
        {
            var builder = GraphBuilder.Create();

            var fixer = builder.Recipe("fixer").Input("Broken", 5).Output("Fixed", 5);
            var breaker = builder.Recipe("breaker").Input("Fixed", 1).Output("Broken", 1).Target(10);
            builder.Link(fixer, breaker);
            builder.Link(breaker, fixer);

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(2, data.RecipeRate("fixer"));
        }

        [TestMethod]
        public void TestNoInputRecipe()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Recipe().Output("Water", 100),
                builder.Recipe().Input("Water", 1).Output("Ice", 1),
                builder.Consumer("Ice").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.ConsumedRate("Ice"));
        }

        [TestMethod]
        public void TestRecipeWithEfficiencyBonus()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Ore"),
                builder.Recipe().Input("Ore", 1).Output("Plate", 1).Efficiency(0.25),
                builder.Consumer("Plate").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(8, data.SupplyRate("Ore"));
        }

        [TestMethod]
        public void TestRecipeCanBeOverSupplied()
        {
            var builder = GraphBuilder.Create();
            var recipe = builder.Recipe("furnace").Input("Ore", 1).Output("Plate", 1);
            var dualOreSupplier = builder.Supply("Ore").Target(10);

            builder.Link(
                builder.Supply("Ore").Target(20),
                recipe,
                builder.Consumer("Plate").Target(10)
            );
            builder.Link(
                dualOreSupplier,
                recipe
            );
            builder.Link(
                dualOreSupplier,
                builder.Consumer("Ore").Target(5)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.ConsumedRate("Plate"));
            AssertFloatsAreEqual(25, data.RecipeInputRate("furnace", "Ore"));
        }

        [TestMethod]
        public void TestMinimizeOverSupply()
        {
            var builder = GraphBuilder.Create();
            var recipe = builder.Recipe("furnace").Input("Ore", 1).Output("Plate", 1);
            var dualOreSupplier = builder.Supply("Ore").Target(25);

            builder.Link(
                dualOreSupplier,
                recipe,
                builder.Consumer("Plate").Target(10)
            );

            builder.Link(
                dualOreSupplier,
                builder.Consumer("Ore")
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(15, data.ConsumedRate("Ore"));
            AssertFloatsAreEqual(10, data.ConsumedRate("Plate"));
        }

        [TestMethod]
        public void TestOverProduction()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Ore"),
                builder.Recipe().Input("Ore", 1).Output("Plate", 1).Target(15),
                builder.Consumer("Plate").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(data.SupplyRate("Ore"), data.ConsumedRate("Plate"));
        }

        [TestMethod]
        public void TestUnderProduction()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Ore"),
                builder.Recipe().Input("Ore", 1).Output("Plate", 1).Target(5),
                builder.Consumer("Plate").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(data.SupplyRate("Ore"), data.ConsumedRate("Plate"));
        }

        [TestMethod]
        public void TestPreferUseOfOverSupplyThanFromSuppliers()
        {
            var builder = GraphBuilder.Create();

            var recipe = builder.Recipe().Output("Plate", 1).Output("Waste", 1);
            var acid = builder.Recipe().Output("Acid", 1).Input("Waste", 5);
            var target = builder.Recipe().Output("Battery", 1).Input("Plate", 1).Input("Acid", 1);

            builder.Link(
                recipe,
                target,
                builder.Consumer("Battery").Target(10)
            );

            builder.Link(
                builder.Supply("Acid"),
                target
            );

            builder.Link(
                recipe,
                acid,
                target
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            AssertFloatsAreEqual(10, data.ConsumedRate("Battery"));

            // 10 plate generates 2 acid, so 8 should come from supply
            AssertFloatsAreEqual(8, data.SupplyRate("Acid"));
        }

        private void AssertFloatsAreEqual(double expected, double actual)
        {
            Assert.AreEqual(expected, actual, 0.0001);
        }
    }
}
