//#define VERBOSEDEBUG

using Foreman.DataCaching;
using Foreman.Models;
using Foreman.Models.Nodes;
using Foreman.Models.Solver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Foreman {
    public partial class ProductionGraph {
        private int updateCounter;
        public void OptimizeGraphNodeValues() {
            foreach (var nodeGroup in GetConnectedComponents(false))
                OptimiseNodeGroup(nodeGroup);

            Debug.WriteLine("UPDATE #" + updateCounter++);
        }

        private void OptimiseNodeGroup(IEnumerable<BaseNode> nodeGroup) {
            double minRatio = 0.1;
            foreach (RecipeNode node in nodeGroup.Where(n => n is RecipeNode))
                minRatio = Math.Min(minRatio, node.GetMinOutputRatio());

            var solver = new ProductionSolver(PullOutputNodes, Math.Pow(10, PullOutputNodesPower), minRatio, Math.Pow(10, LowPriorityPower));

            foreach (BaseNode node in nodeGroup)
                node.AddConstraints(solver);

            var solution = solver.Solve();

#if VERBOSEDEBUG
        Debug.WriteLine(solver.ToString());
#endif

            if (solution == null) {
                //Cyclic recipes with 'not enough provided' can lead to no-solution. Cyclic recipes with 'extra left' lead to an over-supply (solution found)
                //using the pulloutputnodes option can result in an unbound solution (also null).

                //ErrorLogging.LogLine(solver.ToString(CultureInfo.InvariantCulture));
                //Console.WriteLine(solver.ToString(CultureInfo.InvariantCulture));
                ErrorLogging.LogLine("Solver failed to find a solution for a connected node group");
            }

            foreach (BaseNode node in nodeGroup) {
                node.SetSolvedRate(solution?.ActualRate(node) ?? 0);
                foreach (NodeLink link in node.OutputLinks)
                    link.ThroughputPerSec = solution?.Throughput(link) ?? 0;
            }
        }
    }

    // Using partial classes here to group all the constraints related code into this file so it's
    // easy to understand as a whole.
    public abstract partial class BaseNode {
        internal virtual void SetSolvedRate(double rate) {
            //this is for all nodes but the recipe node. IRecipe node overwrites this to set the factory count instead (as that is what the solver was solving for)
            ActualRatePerSec = rate;
            NodeValuesChanged?.Invoke(this, EventArgs.Empty);
            IsClean = true;
        }

        internal void AddConstraints(ProductionSolver solver) {
            if (this is RecipeNode rNode)
                solver.AddRecipeNode(rNode, rNode.FactoryRate()); //add node with minimization requirement on number of buildings
            else
                solver.AddNode(this); //add node without any minimization requirements

            if (this is ConsumerNode cNode && rateType == RateType.Auto)
                solver.AddOutputObjective(cNode); //pull up consumer node
            else if (RateType == RateType.Manual)
                solver.AddTarget(this, DesiredRatePerSec); //set manual requrement

            //add in the connections from the inputs of the node to any links connected to those inputs, grouped by item. There is no errors allowed here -> sum of link throughputs MUST equal the amount consumed.
            foreach (var itemInputs in InputLinks.GroupBy(x => x.Item)) {
                ItemQualityPair item = itemInputs.Key;
                solver.AddInputRatio(this, item, itemInputs, inputRateFor(item));
            }

            //add in the connections for the outputs of the node to any links connected to those outputs, grouped by item. Errors are only allowed for recipe nodes (too much produced -> accumulating in node), though it will be marked as 'overproducing'. All other nodes allow no errors (sum of link thorughputs MUST equal the amount produced)
            foreach (var itemOutputs in OutputLinks.GroupBy(x => x.Item)) {
                ItemQualityPair item = itemOutputs.Key;
                solver.AddOutputRatio(this, item, itemOutputs, outputRateFor(item));
            }

            //add in a forced 0 for passthrough nodes that have no inputs or no outputs (prevents such nodes from acting as 'free' inputs or outputs)
            if (this is PassthroughNode pNode && (InputLinks.Count == 0 || OutputLinks.Count == 0))
                solver.SetZero(pNode);
        }

        internal abstract double inputRateFor(ItemQualityPair item);
        internal abstract double outputRateFor(ItemQualityPair item);
    }
}
