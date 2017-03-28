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

            Assert.AreEqual(10, data.Supply("Plate").desiredRate);
        }

        [TestMethod]
        public void TestBasicSolveWithRecipe()
        {
            var builder = GraphBuilder.Create();

            builder.Link(
                builder.Supply("Plate"),
                builder.Recipe().Input("Plate", 1).Output("Wire", 2),
                builder.Consumer("Wire").Target(10)
            );

            var data = builder.Build();

            GraphOptimisations.FindOptimalGraphToSatisfyFixedNodes(data.Graph);

            Assert.AreEqual(5, data.Supply("Plate").desiredRate);
        }
    }
}
