//#define VERBOSEDEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Foreman
{
	public partial class ProductionGraph
	{
		private int updateCounter = 0;
		public void OptimizeGraphNodeValues()
		{
			foreach (var nodeGroup in GetConnectedComponents())
				OptimiseNodeGroup(nodeGroup);

			Debug.WriteLine("UPDATE #" + updateCounter++);
		}

		private void OptimiseNodeGroup(IEnumerable<BaseNode> nodeGroup)
		{
			double minRatio = 0.1;
			foreach (RecipeNode node in nodeGroup.Where(n => n is RecipeNode))
				minRatio = Math.Min(minRatio, node.GetMinOutputRatio());

			ProductionSolver solver = new ProductionSolver(PullOutputNodes, Math.Pow(10, PullOutputNodesPower), minRatio, Math.Pow(10, LowPriorityPower));

			foreach (BaseNode node in nodeGroup)
				node.AddConstraints(solver);

			var solution = solver.Solve();

#if VERBOSEDEBUG
        Debug.WriteLine(solver.ToString());
#endif

			if (solution == null)
			{
				//Cyclic recipes with 'not enough provided' can lead to no-solution. Cyclic recipes with 'extra left' lead to an over-supply (solution found)
				//using the pulloutputnodes option can result in an unbound solution (also null).

				//ErrorLogging.LogLine(solver.ToString());
				//Console.WriteLine(solver.ToString());
				Console.WriteLine("Solver failed");
			}

			foreach (BaseNode node in nodeGroup)
			{
				node.SetSolvedRate(solution?.ActualRate(node) ?? 0);
				foreach (NodeLink link in node.OutputLinks)
					link.ThroughputPerSec = solution?.Throughput(link) ?? 0;
			}
		}
	}

	// Using partial classes here to group all the constraints related code into this file so it's
	// easy to understand as a whole.
	public abstract partial class BaseNode
	{
		internal virtual void SetSolvedRate(double rate)
		{
			ActualRatePerSec = rate;
			NodeValuesChanged?.Invoke(this, EventArgs.Empty);
		}

		internal void AddConstraints(ProductionSolver solver)
		{
			if (this is RecipeNode rNode)
				solver.AddRecipeNode(rNode, rNode.factoryRate());
			else
				solver.AddNode(this);

			if (this is ConsumerNode cNode && rateType == RateType.Auto)
				solver.AddOutputObjective(cNode);

			if (RateType == RateType.Manual)
				solver.AddTarget(this, DesiredRatePerSec);

			foreach (var itemInputs in InputLinks.GroupBy(x => x.Item))
			{
				Item item = itemInputs.Key;
				solver.AddInputRatio(this, item, itemInputs, inputRateFor(item));
			}

			foreach (var itemOutputs in OutputLinks.GroupBy(x => x.Item))
			{
				Item item = itemOutputs.Key;
				solver.AddOutputRatio(this, item, itemOutputs, outputRateFor(item));
			}
		}

		internal abstract double inputRateFor(Item item);
		internal abstract double outputRateFor(Item item);
	}
}