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

            foreach (var node in data.Graph.Nodes)
            {
                AssertFloatsAreEqual(0, node.desiredRate);
            }
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

            AssertFloatsAreEqual(0.1, data.SupplyRate("Water"));
        }

        private void AssertFloatsAreEqual(double expected, double actual)
        {
            Assert.AreEqual(expected, actual, 0.0001);
        }
    }
}
