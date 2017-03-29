using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
    public static partial class GraphOptimisations
    {
        public static void FindOptimalGraphToSatisfyFixedNodes(this ProductionGraph graph)
        {
            foreach (ProductionNode node in graph.Nodes.Where(n => n.rateType == RateType.Auto))
            {
                if (node.rateType == RateType.Auto)
                {
                    node.actualRate = node.desiredRate = 0;
                }
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
                if (node.rateType == RateType.Manual)
                {
                    solver.AddTarget(node, node.desiredRate);
                }

                if (node is RecipeNode)
                {
                    RecipeNode rNode = (RecipeNode)node;

                    foreach (var itemInputs in rNode.InputLinks.GroupBy(x => x.Item))
                    {
                        var item = itemInputs.Key;

                        // Inputs to a recipe node are allowed to back up
                        solver.AddInputRatio(rNode, item, itemInputs, rNode.BaseRecipe.Ingredients[item]);
                        solver.AddInputAllowBackup(rNode, item, itemInputs, rNode.BaseRecipe.Ingredients[item]);
                    }

                    foreach (var itemOutputs in rNode.OutputLinks.GroupBy(x => x.Item))
                    {
                        var item = itemOutputs.Key;

                        solver.AddOutputRatio(rNode, item, itemOutputs, rNode.BaseRecipe.Results[item]);
                    }
                }
                else if (node is SupplyNode)
                {
                    SupplyNode sNode = (SupplyNode)node;

                    // Supply nodes are effectively recipe nodes with no inputs that produce one item
                    // per recipe rate.
                    solver.AddOutputRatio(sNode, sNode.SuppliedItem, sNode.OutputLinks, 1);
                }
                else if (node is ConsumerNode)
                {
                    ConsumerNode cNode = (ConsumerNode)node;

                    // Unlike recipe nodes, consumer nodes need to consume all of their input. This
                    // is needed for both accurate automatically calculated output rates (when not
                    // fixed), and also to properly consume multiple inputs.
                    solver.AddInputRatio(cNode, cNode.ConsumedItem, cNode.InputLinks, 1);
                    solver.AddInputConsumeAll(cNode, cNode.ConsumedItem, cNode.InputLinks, 1);
                }
            }

            var solution = solver.Solve();

            if (solution == null)
                return;

            foreach (var node in nodeGroup)
            {
                if (node.rateType == RateType.Auto)
                {
                    node.desiredRate = (float)solution[node];
                    // TODO: Polymorphic this.
                    // TODO: Why this restriction? In case of over-production actual could be different from desired.
                    if (!(node is ConsumerNode))
                    {
                        node.actualRate = (float)solution[node];
                    }
                }
            }
        }
    }
}