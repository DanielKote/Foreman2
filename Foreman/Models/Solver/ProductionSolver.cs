using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;
using System.Diagnostics;

namespace Foreman
{
	// A wrapper around Google's Optimization Tools, specifically the Linear Programming library. We
	// can express a factory as a system of linear constraints, and this library takes care of
	// solving them for us.
	//
	// Google also provides a library for Flow Algorithms which sounds like an appealing and
	// potentially simpler alternative, though it's not obvious to me that the problem maps exactly.
	//
	// https://developers.google.com/optimization/lp/glop
	public class ProductionSolver
	{
		public double LowPriorityMultiplier { get; set; }

		private double outputObjectiveCoefficient; //we want to maximize the output of each automatic consumer node.
		private double factoryObjectiveCoefficient; //we want to minimize the number of buildings (of all recipe nodes), but not at the expense of oversupply or errors

		private double overflowObjectiveCoefficient; //cost of oversupply needs to be great enough that the solver doesnt choose to 0 all recipe nodes and swallow any produced items as 'oversupply'. This needs to take into account the current nodes output ratios (ex: if item is produced extremely slowly, this value needs to be high enough for the solver not to decide to 0 its use)
		private double absErrorObjectiveCoefficient; //errors should be avoided at all cost (if possible)

		public class Solution
		{
			public Solution(Dictionary<BaseNode, double> nodes, Dictionary<NodeLink, double> links)
			{
				this.Nodes = nodes;
				this.Links = links;
			}

			public Dictionary<BaseNode, double> Nodes { get; private set; }
			public Dictionary<NodeLink, double> Links { get; private set; }

			public double ActualRate(BaseNode node)
			{
				return Nodes[node];
			}

			public double Throughput(NodeLink link)
			{
				return Links[link];
			}
		}

		private Objective objective;

		private GoogleSolver solver;

		// There is no way to generate a unique string/name for nodes, so instead store a map so they
		// can be uniquely associated.
		private Dictionary<object, Variable> allVariables;

		// We only keep track of constraints as we create them for debugging purposes. OrTools
		// doesn't provide a method for listing all constraints on a solver, which is unfortunate.

		// Keep track of nodes as they are added to ensure the solution contains all of them, even if
		// there are no links.
		private List<BaseNode> nodes;

		// Used to ensure uniqueness of variables names
		private int counter;

		enum LinkType { LINK, ERROR }
		enum RateType { ACTUAL, ERROR }

		public ProductionSolver(bool pullOutputNodes, double minRecipeOutRate = 1e-3) : this(pullOutputNodes, 5, 1e-2, 1e1 / minRecipeOutRate, 1e4 / minRecipeOutRate) { } //io ratio is the maximum output imbalance (ex: 1 deuterium cell (highest nuclear in seablock) is enough to produce 120,000 MJ of heat and thus is consumed at around 1/1200 per sec?, so the minRecipeOutRate should be 1/1200)

		public ProductionSolver(bool pullOutputNodes, double outputObjectiveC, double rateObjectiveC, double supplyObjectiveC, double errorObjectiveC)
		{
			LowPriorityMultiplier = 10000;
			outputObjectiveCoefficient =  pullOutputNodes? outputObjectiveC : 0;
			factoryObjectiveCoefficient = rateObjectiveC;
			overflowObjectiveCoefficient = supplyObjectiveC;
			absErrorObjectiveCoefficient = errorObjectiveC;

			this.solver = GoogleSolver.Create();
			this.objective = solver.Objective();
			this.allVariables = new Dictionary<object, Variable>();
			this.nodes = new List<BaseNode>();
		}

		public void AddNode(BaseNode node)
		{
			var nodeRate = variableFor(node);
			this.nodes.Add(node);
		}

		//we want to minimize the number of buildings (so recipe nodes only). For all other nodes we dont care about the rates, since their flows will be dictated by other factors.
		//this does mean that we prefer paths with least number of buildings, which may mean more source items consumed (ex: a base oil process with speed modules will be prefered over an advanced oil process without speed modules)
		//however since there is a cost associated with providing those items (through more buildings for resource extraction), this should be OK for most use-cases.
		public void AddRecipeNode(RecipeNode node, double factoryRateCoefficient)
		{
			var nodeRate = variableFor(node);
			this.nodes.Add(node);

			if (!node.OutputLinks.Any() && !node.BaseRecipe.Name.StartsWith("§§")) // pure consume node -> we assume this is part of the void recipe node groups and try to give it a higher 'cost' per building to decentivise its use.
				objective.SetCoefficient(nodeRate, factoryObjectiveCoefficient * factoryRateCoefficient * LowPriorityMultiplier);
			else
				objective.SetCoefficient(nodeRate, factoryObjectiveCoefficient * factoryRateCoefficient);
		}

		// Returns null if no optimal solution can be found. Technically GLOP can return non-optimal
		// solutions, but since I don't have any inputs that generate those I'm opting on the side of
		// safety by treating those as non-solutions.
		public Solution Solve()
		{
			// TODO: Can we return an empty solution instead?
			if (nodes.Count == 0)
				return null;

			objective.SetMinimization();

			//solver.Solve(); //<<------------------------------------------------------------------------------------------------------------- Cyclic recipes with 'not enough provided' can lead to no-solution. Cyclic recipes with 'extra left' lead to an over-supply (solution found)
			if (solver.Solve() != Solver.ResultStatus.OPTIMAL)
				return null;

			var nodeSolutions = nodes
				.ToDictionary(x => x, x => solutionFor(Tuple.Create(x, RateType.ACTUAL)));

			var linkSolutions = nodes
				.SelectMany(x => x.OutputLinks)
				.ToDictionary(x => x, x => solutionFor(x));

			return new Solution(nodeSolutions, linkSolutions);
		}

		// Ensure that the solution has a rate matching desired for this node. Typically there will
		// one of these on the ultimate output node, though multiple are supported, on any node. If
		// there is a conflict, a 'best effort' solution will be returned, where some nodes actual
		// rates will be less than the desired asked for here.
		public void AddTarget(BaseNode node, double desiredRate)
		{
			var nodeVar = variableFor(node, RateType.ACTUAL);
			var errorVar = variableFor(node, RateType.ERROR);

			// The sum of the rate for this node, plus an error variable, must be equal to
			// desiredRate. In normal scenarios, the error variable will be zero. In error scenarios the error variable will be +ve non-zero.
			var constraint = MakeConstraint(desiredRate, desiredRate);
			constraint.SetCoefficient(nodeVar, 1);
			constraint.SetCoefficient(errorVar, 1);

			objective.SetCoefficient(errorVar, absErrorObjectiveCoefficient);
		}

		//we want to maximize the amount of output items, so we add a negative weight to the objective for the given consumer node. Only done if asked for.
		public void AddOutputObjective(ConsumerNode node)
		{
			if(outputObjectiveCoefficient > 0)
				objective.SetCoefficient(variableFor(node), -outputObjectiveCoefficient);
		}

		// Constrain a ratio on the output side of a node. This is done for each unique item, and constrains the producted item (based on the node rate) to be equal to the amount of the item transported away by the links
		// this is only done if there are any links -> in the case of 0 links we leave it unbound.
		// Due to the possibility of an overflow, we introduce an 'overflow' variable here that accounts for any extra items produced that cant be consumed by the nodes above.
		//	BUT! this is done only for recipe nodes! all other nodes cant have overflows!
		public void AddOutputRatio(BaseNode node, Item item, IEnumerable<NodeLink> links, double rate)
		{
			if (links.Any())
			{
				Debug.Assert(links.All(x => x.SupplierNode == node));
				AddIORatio(node, item, links, rate, node is RecipeNode);
			}
		}

		// Constrain a ratio on the input side of a node. Done for each unique item, and constrains the consumed item (based on the node rate) to be equal to the amount of the item provided by the links.
		// as with outputs, this is only done if there are any links -> in the case of 0 links we leave it unbound.
		// unlike with the outputs, we dont have any error/overflow variables here. the numbers MUST equal
		public void AddInputRatio(BaseNode node, Item item, IEnumerable<NodeLink> links, double rate)
		{
			if (links.Any())
			{
				Debug.Assert(links.All(x => x.ConsumerNode == node));
				AddIORatio(node, item, links, rate, false);
			}
		}

		private void AddIORatio(BaseNode node, Item item, IEnumerable<NodeLink> links, double rate, bool includeErrorVariable)
		{
			var constraint = MakeConstraint(0, 0);
			var rateVariable = variableFor(node);

			constraint.SetCoefficient(rateVariable, rate);
			foreach (var link in links)
			{
				var variable = variableFor(link);
				constraint.SetCoefficient(variable, -1);
			}

			if (includeErrorVariable)
			{
				var errorVariable = VariableForOverflow(node, item);
				constraint.SetCoefficient(errorVariable, -1);
				objective.SetCoefficient(errorVariable, overflowObjectiveCoefficient);
			}
		}

		private Constraint MakeConstraint(double low, double high)
		{
			return solver.MakeConstraint(low, high);
		}

		private Variable variableFor(NodeLink inputLink)
		{
			return variableFor(inputLink, makeName("link", "S(" + inputLink.ConsumerNode.NodeID + ")", "C(" + inputLink.ConsumerNode.NodeID + ")", inputLink.Item.FriendlyName));
		}

		private string makeName(params object[] components)
		{
			return string.Join(":", components).ToLower().Replace(" ", "-");
		}

		private Variable variableFor(BaseNode node, RateType type = RateType.ACTUAL)
		{
			return variableFor(Tuple.Create(node, type), makeName("node", type, node.NodeID, node.ToString()));
		}

		private Variable VariableForOverflow(BaseNode node, Item item)
		{
			return variableFor(Tuple.Create(node, item), makeName("node-overflow", node.NodeID, node.ToString(), item.ToString()));
		}

		private Variable variableFor(object key, string name)
		{
			if (allVariables.ContainsKey(key))
			{
				return allVariables[key];
			}
			var newVar = solver.MakeNumVar(0.0, double.PositiveInfinity, name + ":" + GetSequence());
			allVariables[key] = newVar;
			return newVar;
		}

		private double solutionFor(object key)
		{
			if (allVariables.ContainsKey(key))
			{
				return allVariables[key].SolutionValue();
			}
			else
			{
				return 0.0;
			}
		}

		private int GetSequence()
		{
			return this.counter += 1;
		}

		// A human-readable description of the constraints. Useful for debugging.
		public override string ToString()
		{
			return solver.ToString();
		}
	}
}