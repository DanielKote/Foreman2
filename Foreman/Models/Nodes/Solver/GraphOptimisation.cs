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
			foreach (BaseNode node in Nodes.Where(n => n.RateType == RateType.Auto))
				node.ResetSolvedRate();

			foreach (var nodeGroup in GetConnectedComponents())
				OptimiseNodeGroup(nodeGroup);

			Debug.WriteLine("UPDATE #" + updateCounter++);
		}

		private void OptimiseNodeGroup(IEnumerable<BaseNode> nodeGroup)
		{
			double maxRatio = 1;
			foreach (RecipeNode node in nodeGroup.Where(n => n is RecipeNode))
				maxRatio = Math.Max(maxRatio, node.GetMaxIORatio());

			ProductionSolver solver = new ProductionSolver(maxRatio);

			foreach (BaseNode node in nodeGroup)
				node.AddConstraints(solver);

			var solution = solver.Solve();

#if VERBOSEDEBUG
        Debug.WriteLine(solver.ToString());
#endif

			// TODO: Handle BIG NUMBERS
			// TODO: Return error in solution!?
			if (solution == null)
			{
				//Cyclic recipes with 'not enough provided' can lead to no-solution. Cyclic recipes with 'extra left' lead to an over-supply (solution found)
				//have to check for cyclic recipes currently in the graph, and see if we can somehow find which one is causing issues and display it.
				//work in the solver is kind of necessary here....

				//removed the exception raising since this can actually happen now.
				//throw new Exception("Solver failed but that shouldn't happen.\n" + solver.ToString());
				ErrorLogging.LogLine(solver.ToString());
				Console.WriteLine(solver.ToString());
			}

			foreach (BaseNode node in nodeGroup)
			{
				node.SetSolvedRate(solution?.ActualRate(node) ?? 0);
				foreach (NodeLink link in node.OutputLinks)
					link.Throughput = solution?.Throughput(link) ?? 0;
				foreach (NodeLink link in node.InputLinks)
					link.Throughput = solution?.Throughput(link) ?? 0;

			}
		}
	}

	// Using partial classes here to group all the constraints related code into this file so it's
	// easy to understand as a whole.
	public abstract partial class BaseNode
	{
		internal void ResetSolvedRate()
		{
			ActualRatePerSec = 0;
		}

		internal virtual void SetSolvedRate(double rate)
		{
			ActualRatePerSec = rate;
		}

		internal void AddConstraints(ProductionSolver solver)
		{
			solver.AddNode(this);

			if (RateType == RateType.Manual)
				solver.AddTarget(this, DesiredRatePerSec);

			foreach (var itemInputs in InputLinks.GroupBy(x => x.Item))
			{
				var item = itemInputs.Key;

				solver.AddInputRatio(this, item, itemInputs, inputRateFor(item));
				solver.AddInputLink(this, item, itemInputs);
			}

			foreach (var itemOutputs in OutputLinks.GroupBy(x => x.Item))
			{
				var item = itemOutputs.Key;

				solver.AddOutputRatio(this, item, itemOutputs, outputRateFor(item));
				// Output links do not need to constrained, since they are already covered by adding
				// the input link above.
			}
		}

		internal abstract double inputRateFor(Item item);
		internal abstract double outputRateFor(Item item);
	}
}