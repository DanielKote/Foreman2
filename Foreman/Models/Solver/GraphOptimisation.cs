using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman
{
    public static partial class GraphOptimisations
    {
        public static void FindOptimalGraphToSatisfyFixedNodes(this ProductionGraph graph)
        {
            foreach (ProductionNode node in graph.Nodes.Where(n => n.rateType == RateType.Auto))
            {
                node.ResetSolvedRate();
            }

            foreach (var nodeGroup in graph.GetConnectedComponents())
            {
                OptimiseNodeGroup(nodeGroup);
            }

            graph.UpdateLinkThroughputs();
        }

        public static void OptimiseNodeGroup(IEnumerable<ProductionNode> nodeGroup)
        {
            ProductionSolver solver = new ProductionSolver();

            foreach (var node in nodeGroup)
            {
                node.AddConstraints(solver);
            }

            var solution = solver.Solve();

            Debug.WriteLine(solver.ToString());

            // TODO: Handle BIG NUMBERS
            // TODO: Return error in solution!?
            if (solution == null)
                throw new Exception("Solver failed but that shouldn't happen.\n" + solver.ToString());

            foreach (var node in nodeGroup)
            {
                node.SetSolvedRate(solution.ActualRate(node));
                foreach (var link in node.OutputLinks.Union(node.InputLinks))
                {
                    link.Throughput = solution.Throughput(link);
                }
            }
        }
    }

    // Using partial classes here to group all the constraints related code into this file so it's
    // easy to understand as a whole.
    public abstract partial class ProductionNode
    {
        internal void ResetSolvedRate()
        {
            actualRate = 0;
        }

        internal virtual void SetSolvedRate(double rate)
        {
            actualRate = (float)rate;
        }

        internal void AddConstraints(ProductionSolver solver)
        {
            solver.AddNode(this);

            if (rateType == RateType.Manual)
            {
                solver.AddTarget(this, desiredRate);
            }

            foreach (var itemInputs in InputLinks.GroupBy(x => x.Item))
            {
                var item = itemInputs.Key;

                solver.AddInputRatio(this, item, itemInputs, inputRateFor(item));
                solver.AddInputLink(this, item, itemInputs, inputRateFor(item));
            }

            foreach (var itemOutputs in OutputLinks.GroupBy(x => x.Item))
            {
                var item = itemOutputs.Key;

                solver.AddOutputRatio(this, item, itemOutputs, outputRateFor(item));
                // Output links do not need to constrained, since they are already covered by adding
                // the input link above.
            }
        }

        internal abstract double outputRateFor(Item item);
        internal abstract double inputRateFor(Item item);
    }
}