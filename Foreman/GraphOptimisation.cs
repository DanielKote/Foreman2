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
            // TODO: Return link throughput in solution
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
    public partial class ProductionNode
    {
        internal virtual void AddConstraints(ProductionSolver solver)
        {
            solver.AddNode(this);
            if (rateType == RateType.Manual)
            {
                solver.AddTarget(this, desiredRate);
            }
        }

        internal void ResetSolvedRate()
        {
            actualRate = 0;
        }

        internal virtual void SetSolvedRate(double rate)
        {
            actualRate = (float)rate;
        }
    }

    public partial class SupplyNode : ProductionNode
    {
        internal override void AddConstraints(ProductionSolver solver)
        {
            base.AddConstraints(solver);

            // Supply nodes are effectively recipe nodes with no inputs that produce one item
            // per recipe rate.
            solver.AddOutputRatio(this, SuppliedItem, OutputLinks, 1);
        }
    }

    public partial class ConsumerNode : ProductionNode
    {
        internal override void AddConstraints(ProductionSolver solver)
        {
            base.AddConstraints(solver);

            // Unlike recipe nodes, consumer nodes need to consume all of their input. This
            // is needed for both accurate automatically calculated output rates (when not
            // fixed), and also to properly consume multiple inputs.
            solver.AddInputRatio(this, ConsumedItem, InputLinks, 1);
            solver.AddInputConsumeAll(this, ConsumedItem, InputLinks, 1);
        }
    }

    public partial class RecipeNode : ProductionNode
    {
        internal override void AddConstraints(ProductionSolver solver)
        {
            base.AddConstraints(solver);

            foreach (var itemInputs in InputLinks.GroupBy(x => x.Item))
            {
                var item = itemInputs.Key;

                // Inputs to a recipe node are allowed to back up
                solver.AddInputRatio(this, item, itemInputs, BaseRecipe.Ingredients[item]);
                solver.AddInputAllowBackup(this, item, itemInputs, BaseRecipe.Ingredients[item]);
            }

            foreach (var itemOutputs in OutputLinks.GroupBy(x => x.Item))
            {
                var item = itemOutputs.Key;

                solver.AddOutputRatio(this, item, itemOutputs, BaseRecipe.Results[item]);
            }
        }
    }
}