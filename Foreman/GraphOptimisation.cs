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
                //OptimiseNodeGroup(nodeGroup);				
                OptimiseNodeGroupGLOP(nodeGroup);
            }

            graph.UpdateLinkThroughputs();
        }

        public class OurSolver
        {
            private Objective objective;

            public Solver solver { get; private set; }
            public Dictionary<object, Variable> allVariables;
            private int counter;
            private List<Google.OrTools.LinearSolver.Constraint> allConstraints;

            enum EndpointType { INPUT, OUTPUT }

            public OurSolver()
            {
                this.solver = Solver.CreateSolver("Foreman", "GLOP_LINEAR_PROGRAMMING");
                this.objective = solver.Objective();
                this.allVariables = new Dictionary<object, Variable>();
                this.allConstraints = new List<Google.OrTools.LinearSolver.Constraint>();
                objective.SetMinimization();
            }

            internal void AddTarget(ProductionNode node, float desiredRate)
            {
                Variable nodeVar = variableFor(node);
                var constraint = MakeConstraint(desiredRate, double.PositiveInfinity);
                constraint.SetCoefficient(nodeVar, 1);
            }

            internal int Solve()
            {
                return solver.Solve();
            }

            internal double GetDesiredRate(ProductionNode node)
            {
                return variableFor(node).SolutionValue();
            }

            internal void AddOutputRatio(ProductionNode node, Item item, IEnumerable<NodeLink> itemOutputs, float outputRate)
            {
                // Ensure that the sum of all outputs for this type of item is in relation to the rate of the recipe
                // So for copper wire, the some of every output variable must 2 * rate
                var constraint = MakeConstraint(0, 0);
                var desiredRateVariable = variableFor(node);

                constraint.SetCoefficient(desiredRateVariable, outputRate);
                foreach (var inputLink in itemOutputs)
                {
                    var inputVariable = variableFor(inputLink, EndpointType.INPUT);
                    constraint.SetCoefficient(inputVariable, -1);
                }
            }

            internal void AddInputRatio(ProductionNode node, Item item, IEnumerable<NodeLink> itemInputs, float inputRate)
            {
                // Ensure that the sum of all inputs for this type of item is in relation to the rate of the recipe
                // So for the steel input to a solar panel, the sum of every input variable to this node must equal 5 * rate.
                var constraint = MakeConstraint(0, 0);
                var desiredRateVariable = variableFor(node);

                constraint.SetCoefficient(desiredRateVariable, inputRate);
                foreach (var inputLink in itemInputs)
                {
                    var inputVariable = variableFor(inputLink, EndpointType.OUTPUT);
                    constraint.SetCoefficient(inputVariable, -1);
                }
            }
            internal void AddRecipeInputAllowBackup(ProductionNode node, Item item, IEnumerable<NodeLink> itemInputs, float inputRate)
            {
                // Ensure that for all inputs for this type of item, they don't consume more than is being produced.
                // Consuming less is fine, this represents a backup.
                foreach (var inputLink in itemInputs)
                {
                    var constraint = MakeConstraint(0, double.PositiveInfinity);
                    var supplierVariable = variableFor(inputLink, EndpointType.INPUT);
                    var consumerVariable = variableFor(inputLink, EndpointType.OUTPUT);
                    constraint.SetCoefficient(supplierVariable, 1);
                    constraint.SetCoefficient(consumerVariable, -1);
                }
            }
            internal void AddRecipeInputConsumeAll(ProductionNode node, Item item, IEnumerable<NodeLink> itemInputs, float inputRate)
            {
                // Ensure that for all inputs for this type of item, all must be consumed.
                foreach (var inputLink in itemInputs)
                {
                    var constraint = MakeConstraint(0, 0);
                    var supplierVariable = variableFor(inputLink, EndpointType.INPUT);
                    var consumerVariable = variableFor(inputLink, EndpointType.OUTPUT);
                    constraint.SetCoefficient(supplierVariable, 1);
                    constraint.SetCoefficient(consumerVariable, -1);
                }
            }

            private Google.OrTools.LinearSolver.Constraint MakeConstraint(double low, double high)
            {
                var constraint = solver.MakeConstraint(low, high);
                allConstraints.Add(constraint);
                return constraint;
            }

            private Variable variableFor(NodeLink inputLink, EndpointType type)
            {
                return variableFor(Tuple.Create(inputLink, type), "link:" + type + ":" + inputLink.Consumer.DisplayName + ":" + inputLink.Item);
            }

            private Variable variableFor(ProductionNode node)
            {
                if (node is SupplyNode)
                {
                    return variableFor(node, "supplier:" + node.DisplayName);
                }
                else if (node is ConsumerNode)
                {
                    return variableFor(node, "consumer:" + node.DisplayName);

                }
                else
                {
                    return variableFor(node, "node:" + node.DisplayName);
                }
            }

            private Variable variableFor(object key, String name)
            {
                if (allVariables.ContainsKey(key))
                {
                    return allVariables[key];
                }
                var newVar = solver.MakeNumVar(0.0, double.PositiveInfinity, name + ":" + GetSequence());
                allVariables[key] = newVar;
                return newVar;
            }

            private int GetSequence()
            {
                return this.counter += 1;
            }

            internal void Minimize(ProductionNode node)
            {
                objective.SetCoefficient(variableFor(node), 1);
            }

            public string GetDescription()
            {
                var desc = new StringBuilder();
                foreach (var constraint in this.allConstraints)
                {
                    var line = new List<string>();
                    foreach (var variable in this.allVariables)
                    {
                        var coefficient = constraint.GetCoefficient(variable.Value);
                        if (coefficient != 0.0)
                        {
                            line.Add(coefficient + " * " + variable.Value.Name());
                        }
                    }
                    desc.Append(string.Join(" + ", line));
                    desc.Append(" -> (");
                    desc.Append(constraint.Lb());
                    desc.Append(", ");
                    desc.Append(constraint.Ub());
                    desc.AppendLine(")");
                }
                return desc.ToString();
            }

        }

        public static void OptimiseNodeGroupGLOP(IEnumerable<ProductionNode> nodeGroup)
        {
            OurSolver solver = new OurSolver();

            foreach (var node in nodeGroup)
            {
                if (node is RecipeNode)
                {
                    RecipeNode rNode = (RecipeNode)node;

                    foreach (var itemInputs in rNode.InputLinks.GroupBy(x => x.Item))
                    {
                        var item = itemInputs.Key;

                        solver.AddInputRatio(rNode, item, itemInputs, rNode.BaseRecipe.Ingredients[item]);
                        solver.AddRecipeInputAllowBackup(rNode, item, itemInputs, rNode.BaseRecipe.Ingredients[item]);
                    }

                    foreach (var itemOutputs in rNode.OutputLinks.GroupBy(x => x.Item))
                    {
                        var item = itemOutputs.Key;

                        solver.AddOutputRatio(rNode, item, itemOutputs, rNode.BaseRecipe.Results[item]);
                    }
                } else if (node is SupplyNode)
                {
                    SupplyNode sNode = (SupplyNode)node;

                    solver.AddOutputRatio(sNode, sNode.SuppliedItem, sNode.OutputLinks, 1);
                } else if (node is ConsumerNode)
                {
                    ConsumerNode cNode = (ConsumerNode)node;

                    solver.AddInputRatio(cNode, cNode.ConsumedItem, cNode.InputLinks, 1);
                    solver.AddRecipeInputConsumeAll(cNode, cNode.ConsumedItem, cNode.InputLinks, 1);
                    if (cNode.rateType == RateType.Manual)
                    {
                        solver.AddTarget(cNode, cNode.desiredRate);
                    }
                }
                solver.Minimize(node);
            }

            foreach (var node in nodeGroup)
            {
                if (node.rateType == RateType.Auto)
                {
                    node.desiredRate = (float)solver.GetDesiredRate(node);
                    if (!(node is ConsumerNode))
                    {
                        node.actualRate = (float)solver.GetDesiredRate(node);
                    }
                }
            }
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