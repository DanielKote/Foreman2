using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	public static class GraphOptimisations
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
			List<ProductionNode> nodes = nodeGroup.ToList();

			int nodeCount = nodes.Count();
			Dictionary<Item, Decimal> itemRequirements = new Dictionary<Item, decimal>();

			foreach (var node in nodes)
			{
				foreach (Item item in node.Inputs.Union(node.Outputs))
				{
					itemRequirements[item] = 0M;
				}
			}

			foreach (var node in nodes.Where(n => n.rateType == RateType.Manual))
			{
				foreach (Item item in node.Inputs.Concat(node.Outputs))
				{
					itemRequirements[item] += (decimal)node.GetTotalDemand(item) - (decimal)node.GetDesiredOutput(item);
				}
			}

			LinearProgrammingSolver solver = new LinearProgrammingSolver();
			foreach (Item item in itemRequirements.Keys)
			{
				decimal[] equationCoefficients = new decimal[nodeCount];
				for (int i = 0; i < nodes.Count(); i++)
				{
					ProductionNode node = nodes[i];
					if (node is SupplyNode && node.Outputs.Contains(item))
					{
						equationCoefficients[i] = 1;
					} else if (node is ConsumerNode && node.Inputs.Contains(item))
					{
						equationCoefficients[i] = -1;
					}
					else if (node is RecipeNode)
					{
						if (node.Inputs.Contains(item))
						{
							equationCoefficients[i] -= (decimal)((RecipeNode)node).BaseRecipe.Ingredients[item];
						}
						if (node.Outputs.Contains(item))
						{
							equationCoefficients[i] += (decimal)((RecipeNode)node).BaseRecipe.Results[item];
						}
					}
				}

				if (itemRequirements[item] < 0)
				{
					solver.AddConstraint(new Constraint(-itemRequirements[item], ConstraintType.LessThan, equationCoefficients.Select(c => -c).ToArray()));
				}
				else
				{
					solver.AddConstraint(new Constraint(itemRequirements[item], ConstraintType.GreaterThan, equationCoefficients));
				}
			}

			decimal[] objectiveFunctionCoefficients = new decimal[nodeCount];

			int j = 0;
			foreach (ProductionNode node in nodes)
			{
				if (node is SupplyNode)
				{
					if (((SupplyNode)node).Outputs.Contains(DataCache.Items["water"]))
					{
						objectiveFunctionCoefficients[j] = 0M;
					}
					else
					{
						objectiveFunctionCoefficients[j] = 1M;
					}
				}
				else if (node is ConsumerNode)
				{
					objectiveFunctionCoefficients[j] = -1M;
				}
				j++;
			}

			solver.SetObjectiveFunction(objectiveFunctionCoefficients, ObjectiveFunctionType.Minimise);

			var solution = solver.solve();
			for (int i = 0; i < nodes.Count(); i++)
			{
				if (nodes[i].rateType == RateType.Auto)
				{
					nodes[i].desiredRate = Convert.ToSingle(solution[i]);
				}
				if (!(nodes[i] is ConsumerNode) && nodes[i].rateType == RateType.Auto)
				{
					nodes[i].actualRate = Convert.ToSingle(solution[i]);
				}
			}
		}
	}
}