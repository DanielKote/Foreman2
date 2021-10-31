//#define VERBOSEDEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Foreman
{
    public static partial class GraphOptimisations
    {
        private static int updateCounter = 0;
        public static void FindOptimalGraphToSatisfyFixedNodes(this ProductionGraph graph)
        {
            foreach (BaseNodePrototype node in graph.Nodes.Where(n => n.RateType == RateType.Auto))
                node.ResetSolvedRate();

            foreach (var nodeGroup in graph.GetConnectedComponents())
                OptimiseNodeGroup(nodeGroup);

            Debug.WriteLine("UPDATE #" + updateCounter++);
        }

        private static void OptimiseNodeGroup(IEnumerable<BaseNode> nodeGroup)
        {
            ProductionSolver solver = new ProductionSolver();

            foreach (BaseNodePrototype node in nodeGroup)
                node.AddConstraints(solver);

            var solution = solver.Solve();

#if VERBOSEDEBUG
            Debug.WriteLine(solver.ToString());
#endif

            // TODO: Handle BIG NUMBERS
            // TODO: Return error in solution!?
            if (solution == null)
                throw new Exception("Solver failed but that shouldn't happen.\n" + solver.ToString());

            foreach (BaseNodePrototype node in nodeGroup)
            {
                node.SetSolvedRate(solution.ActualRate(node));
                foreach (NodeLinkPrototype link in node.outputLinks.Union(node.inputLinks))
                    link.Throughput = solution.Throughput(link);

            }
        }
    }

    // Using partial classes here to group all the constraints related code into this file so it's
    // easy to understand as a whole.
    public abstract partial class BaseNodePrototype
    {
        internal void ResetSolvedRate()
        {
            ActualRate = 0;
        }

        internal virtual void SetSolvedRate(double rate)
        {
            ActualRate = (float)rate;
        }

        internal void AddConstraints(ProductionSolver solver)
        {
            solver.AddNode(this);

            if (RateType == RateType.Manual)
                solver.AddTarget(this, DesiredRate);

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