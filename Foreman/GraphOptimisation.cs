using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.OrTools.LinearSolver;

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
				OptimiseNodeGroupGLOP(nodeGroup);
                Console.Out.Flush();
			}

			graph.UpdateLinkThroughputs();
		}

        public class OurSolver
        {
            private Objective objective;

            public Solver solver { get; private set; }
            public HashSet<string> allVariables;
            enum EndpointType { INPUT, OUTPUT }

            public OurSolver()
            {
                this.solver = Solver.CreateSolver("Foreman", "GLOP_LINEAR_PROGRAMMING");
                this.objective = solver.Objective();
                objective.SetMinimization();
            }

            internal void AddInputGoal(ProductionNode node, Item item, IEnumerable<ProductionNode> suppliers, float desiredRate)
            {
                {
                    Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(desiredRate, double.PositiveInfinity);
                    Variable variable = varFor(node, item, EndpointType.INPUT);
                    constraint.SetCoefficient(variable, 1.0);
                }

                {
                    // Don't overproduce!
                    //Google.OrTools.LinearSolver.Constraint productionConstraint = solver.MakeConstraint(0, double.PositiveInfinity);
                    //Google.OrTools.LinearSolver.Constraint productionConstraint = solver.MakeConstraint(double.NegativeInfinity, 0);
                    Google.OrTools.LinearSolver.Constraint productionConstraint = solver.MakeConstraint(0, 0);

                    Variable nodeVar = varFor(node, item, EndpointType.INPUT);
                    productionConstraint.SetCoefficient(nodeVar, 1);
                    foreach (var supplier in suppliers)
                    {
                        productionConstraint.SetCoefficient(varFor(supplier, item, EndpointType.OUTPUT), -1.0);
                    }
                }

            }

            private Variable varFor(ProductionNode node, Item item, EndpointType type)
            {
                string n = nameFor(node, item, type);
                
                Variable existing = solver.LookupVariableOrNull(n);
                if (existing == null)
                {
                    existing = solver.MakeNumVar(0.0, double.PositiveInfinity, n);
                }
                return existing;
            }

            private string nameFor(ProductionNode node, Item item, EndpointType type)
            {
                return node.GetHashCode() + ":" + type.ToString() + ":" + item.ToString();
            }

            internal void AddRecipeConstraint(ProductionNode node, Item input, float inputAmount, Item output, float outputAmount)
            {
                Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(double.NegativeInfinity, 0);

                Variable inputVar = varFor(node, input, EndpointType.INPUT);
                Variable outputVar = varFor(node, output, EndpointType.OUTPUT);

                // TODO: Comment this
                constraint.SetCoefficient(inputVar, -outputAmount);
                constraint.SetCoefficient(outputVar, inputAmount);
            }

            internal void AddProductionConstraint(ProductionNode node, Item item, IEnumerable<ProductionNode> consumers)
            {
                Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(0, double.PositiveInfinity);

                Variable nodeVar = varFor(node, item, EndpointType.OUTPUT);
                constraint.SetCoefficient(nodeVar, 1);
                foreach (var consumer in consumers)
                {
                    constraint.SetCoefficient(varFor(consumer, item, EndpointType.INPUT), -1.0);
                }
            }

            internal void AddObjective(ProductionNode node, Item item)
            {
                objective.SetCoefficient(varFor(node, item, EndpointType.OUTPUT), 1);
            }

            internal void Solve()
            {
                solver.Solve();
            }

            internal void GetSolution(ProductionNode node, Item item, bool input)
            {
                Variable v = varFor(node, item, input ? EndpointType.INPUT : EndpointType.OUTPUT);
                
                System.Diagnostics.Debug.WriteLine(v.Name() + ": " + v.SolutionValue());
            }
        }

        public static void OptimiseNodeGroupGLOP(IEnumerable<ProductionNode> nodeGroup)
        {
            OurSolver solver = new OurSolver();

            foreach (var node in nodeGroup)
            {
                if (node is ConsumerNode)
                {
                    ConsumerNode consumerNode = (ConsumerNode)node;
                    // TODO: This only works for ConsumerNodes
                    if (node.rateType == RateType.Manual)
                    {
                        foreach (Item item in node.Inputs) //.Concat(node.Outputs))
                        {
                            IEnumerable<ProductionNode> suppliers = node.InputLinks
                                .Where(x => x.Item == item)
                                .Select(x => x.Supplier);

                            solver.AddInputGoal(node, item, suppliers, node.desiredRate);
                        }
                    } else
                    {
                        // Handle waste
                    }
                } else
                {
                    // Set up internal node constraints
                    if (node is RecipeNode)
                    {
                        RecipeNode rNode = (RecipeNode)node;
                        foreach (var input in node.Inputs)
                        {
                            foreach (var output in node.Outputs)
                            {

                                Recipe recipe = rNode.BaseRecipe;
                                solver.AddRecipeConstraint(rNode, input, recipe.Ingredients[input], output, recipe.Results[output]);
                            }
                        }
                        // TODO: Link output ratios
                    }

                    // Set up constraints for nodes this one supplies
                    foreach (var item in node.Outputs)
                    {
                        IEnumerable<ProductionNode> supplied = node.OutputLinks
                            .Where(x => x.Item == item)
                            .Select(x => x.Consumer);

                        solver.AddProductionConstraint(node, item, supplied);

                        // TODO: Or recipes with no inputs?
                        if (node is SupplyNode)
                        {
                            solver.AddObjective(node, item);
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("Solving it");
            solver.Solve();

            foreach (var node in nodeGroup)
            {
                foreach (var item in node.Inputs)
                {
                    solver.GetSolution(node, item, true);
                    // TODO: Probably only need first input/output
                }
                foreach (var item in node.Outputs)
                {
                    solver.GetSolution(node, item, false);
                    // TODO: Probably only need first input/output
                }
            }
            System.Diagnostics.Debug.WriteLine("Done");

        }

		public static void OptimiseNodeGroup3(IEnumerable<ProductionNode> nodeGroup)
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

            Solver solver = Solver.CreateSolver("Foreman", "GLOP_LINEAR_PROGRAMMING");

			foreach (Item item in itemRequirements.Keys)
			{
				decimal[] equationCoefficients = new decimal[nodeCount];
                Dictionary<ProductionNode, decimal> coefficients = new Dictionary<ProductionNode, decimal>();


				for (int i = 0; i < nodes.Count(); i++)
				{
					ProductionNode node = nodes[i];
                    Variable existing = solver.LookupVariableOrNull(node.ToString());
                    if (existing == null)
                    {
                        solver.MakeNumVar(0.0, double.PositiveInfinity, node.ToString());
                    }

					if (node is SupplyNode && node.Outputs.Contains(item))
					{
						equationCoefficients[i] = 1;
                        coefficients.Add(node, 1);
					} else if (node is ConsumerNode && node.Inputs.Contains(item))
					{
						equationCoefficients[i] = -1;
                        coefficients.Add(node, -1);
					}
					else if (node is RecipeNode)
					{
						if (node.Inputs.Contains(item))
						{
                            decimal x;
                            coefficients.TryGetValue(node, out x);
                            coefficients.Add(node, x - (decimal)((RecipeNode)node).BaseRecipe.Ingredients[item]);
							equationCoefficients[i] -= (decimal)((RecipeNode)node).BaseRecipe.Ingredients[item];
						}
						if (node.Outputs.Contains(item))
						{
                            decimal x;
                            coefficients.TryGetValue(node, out x);
                            coefficients.Add(node, x + (decimal)((RecipeNode)node).BaseRecipe.Results[item]);
							equationCoefficients[i] += (decimal)((RecipeNode)node).BaseRecipe.Results[item];
						}
					}
				}

				if (itemRequirements[item] < 0)
				{
                    Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(double.NegativeInfinity, -(double)itemRequirements[item]);
                    foreach (KeyValuePair<ProductionNode, decimal> entry in coefficients)
                    {
                        constraint.SetCoefficient(solver.LookupVariableOrNull(entry.Key.ToString()), (double)entry.Value);
                    }
				}
				else
				{
                    Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint((double)itemRequirements[item], double.PositiveInfinity);
                    foreach (KeyValuePair<ProductionNode, decimal> entry in coefficients)
                    {
                        constraint.SetCoefficient(solver.LookupVariableOrNull(entry.Key.ToString()), (double)entry.Value);
                    }
				}
			}

			decimal[] objectiveFunctionCoefficients = new decimal[nodeCount];
            Objective objective = solver.Objective();
            objective.SetMinimization();

			int j = 0;
			foreach (ProductionNode node in nodes)
			{
				if (node is SupplyNode)
				{
					if (((SupplyNode)node).Outputs.Contains(DataCache.Items["water"]))
					{
                        objective.SetCoefficient(solver.LookupVariableOrNull(node.ToString()), 0.0);
						//objectiveFunctionCoefficients[j] = 0M;
					}
					else
					{
                        objective.SetCoefficient(solver.LookupVariableOrNull(node.ToString()), 1.0);
					}
				}
				else if (node is ConsumerNode)
				{
					//objectiveFunctionCoefficients[j] = -1M;
                    objective.SetCoefficient(solver.LookupVariableOrNull(node.ToString()), -1.0);
				}
				j++;
			}

			//solver.SetObjectiveFunction(objectiveFunctionCoefficients, ObjectiveFunctionType.Minimise);

			//var solution = solver.solve();
            solver.Solve();
			for (int i = 0; i < nodes.Count(); i++)
			{
                ProductionNode node = nodes[i];
				if (node.rateType == RateType.Auto)
				{
                    node.desiredRate = Convert.ToSingle(solver.LookupVariableOrNull(node.ToString()).SolutionValue());//  Convert.ToSingle(solution[i]);
				}
				if (!(node is ConsumerNode) && node.rateType == RateType.Auto)
				{
                    node.actualRate = Convert.ToSingle(solver.LookupVariableOrNull(node.ToString()).SolutionValue());
					//nodes[i].actualRate = Convert.ToSingle(solution[i]);
				}
			}

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
							equationCoefficients[i] += (decimal)(((RecipeNode)node).BaseRecipe.Results[item]);
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
                    /*
					if (((SupplyNode)node).Outputs.Contains(DataCache.Items["water"]))
					{
						objectiveFunctionCoefficients[j] = 0M;
					}
					else
                    */
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