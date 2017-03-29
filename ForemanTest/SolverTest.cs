using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Foreman;

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

            Assert.AreEqual(10, data.SupplyRate("Plate"));
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

            Assert.AreEqual(10, data.SupplyRate("Ore"));
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

            Assert.AreEqual(50, data.SupplyRate("Plate"));
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

            Assert.AreEqual(5, data.SupplyRate("Plate"));
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

            Assert.AreEqual(8, data.SupplyRate("Plate"));
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

            Assert.AreEqual(15, data.SupplyRate("Plate"));
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

            Assert.AreEqual(10, data.ConsumedRate("Plate"));
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

            Assert.AreEqual(10, data.SupplyRate("Ore"));
            Assert.AreEqual(10, data.ConsumedRate("Plate"));
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
                Assert.AreEqual(0, node.desiredRate);
            }
        }
    }
}
