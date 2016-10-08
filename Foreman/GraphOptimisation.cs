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
			//graph.AddAllPossibleRecipeNodesForInputs();

			List<RecipeNode> recipeNodes = graph.Nodes.OfType<RecipeNode>().ToList();
			int nodeCount = recipeNodes.Count();
			HashSet<Item> presentItems = new HashSet<Item>();

			foreach (var node in graph.Nodes)//.Where(n => n.rateType == RateType.Manual))
			{
				foreach (Item item in node.Inputs.Concat(node.Outputs))
				{
					presentItems.Add(item);
				}
			}

			int itemCount = presentItems.Count();
			List<Item> itemList = presentItems.ToList();

			LinearProgrammingSolver solver = new LinearProgrammingSolver();
			foreach (Item item in itemList)
			{
				decimal itemTotalOutput = 0M;
				decimal[] equationCoefficients = new decimal[nodeCount];
				for (int i = 0; i < recipeNodes.Count(); i++)
				{
					RecipeNode node = recipeNodes[i];
					if (node.Inputs.Contains(item))
					{
						equationCoefficients[i] -= (decimal)node.BaseRecipe.Ingredients[item];
					}
					if (node.Outputs.Contains(item))
					{
						equationCoefficients[i] += (decimal)node.BaseRecipe.Results[item];
					}
				}

				foreach (SupplyNode node in graph.Nodes.OfType<SupplyNode>().Where(n => n.Outputs.Contains(item) && n.rateType == RateType.Manual))
				{
					itemTotalOutput -= (decimal)node.desiredRate;
				}

				foreach (ConsumerNode node in graph.Nodes.OfType<ConsumerNode>().Where(n => n.Inputs.Contains(item) && n.rateType == RateType.Manual))
				{
					itemTotalOutput += (decimal)node.desiredRate;
				}

				solver.AddConstraint(new Constraint(itemTotalOutput, ConstraintType.GreaterThan, equationCoefficients));
			}

			HashSet<Item> baseItems = new HashSet<Item>() { DataCache.Items["iron-ore"], DataCache.Items["copper-ore"], DataCache.Items["crude-oil"] };
			decimal[] objectiveFunctionCoefficients = new decimal[nodeCount];

			{
				int i = 0;
				foreach (RecipeNode node in graph.Nodes.Where(n => n is RecipeNode))
				{
					foreach (Item item in node.Inputs)
					{
						if (baseItems.Contains(item))
						{
							objectiveFunctionCoefficients[i] += (decimal)node.BaseRecipe.Ingredients[item];
						}
					}
					foreach (Item item in node.Outputs)
					{
						if (baseItems.Contains(item))
						{
							objectiveFunctionCoefficients[i] -= (decimal)node.BaseRecipe.Results[item];
						}
					}

					i++;
				}
			}

			solver.SetObjectiveFunction(objectiveFunctionCoefficients, ObjectiveFunctionType.Minimise);

			var solution = solver.solve();
			for (int i = 0; i < recipeNodes.Count(); i++)
			{
				recipeNodes[i].actualRate = Convert.ToSingle(solution[i]);
			}
		}

		public static void AddAllPossibleRecipeNodesForInputs(this ProductionGraph graph)
		{
			foreach (ProductionNode node in graph.Nodes.ToList())
			{
				foreach (Item input in node.Inputs)
				{
					foreach (Recipe recipe in DataCache.Recipes.Values.Where(r => r.Results.Keys.Contains(input)))
					{
						if (!graph.Nodes.Any(n => n is RecipeNode && (n as RecipeNode).BaseRecipe == recipe))
						{
							var newNode = RecipeNode.Create(recipe, graph);
							NodeLink.Create(newNode, node, input);
						}
					}
				}
			}

			graph.LinkUpAllInputs();
			graph.LinkUpAllInputs();
			graph.LinkUpAllInputs();
			graph.LinkUpAllInputs();
			graph.LinkUpAllInputs();
			graph.LinkUpAllInputs();
			graph.LinkUpAllInputs();
			graph.LinkUpAllInputs();
		}
	}
}