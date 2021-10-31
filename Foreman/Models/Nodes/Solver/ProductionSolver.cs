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

		enum EndpointType { SUPPLY, CONSUME, ERROR }

		public ProductionSolver()
		{
			this.solver = GoogleSolver.Create();
			this.objective = solver.Objective();
			this.allVariables = new Dictionary<object, Variable>();
			this.nodes = new List<BaseNode>();
		}

		public void AddNode(BaseNode node)
		{
			var x = variableFor(node);

			this.nodes.Add(node);

			// The rate of all nodes should be minimized.
			//
			// TODO: There is a tension between minimizing supply and minimizing time to produce that
			// I suspect is not explicitly handled well here. That will likely result in "unexpected"
			// results depending on which the user is wanting. Need to figure out concrete examples
			// of where this would happen. With sufficiently large error co-efficients it's probably
			// fine for most real world recipes?
			objective.SetCoefficient(x, 1.0);
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
			if (solver.Solve() != Solver.OPTIMAL)
				return null;

			var nodeSolutions = nodes
				.ToDictionary(x => x, x => solutionFor(Tuple.Create(x, RateType.ACTUAL)));

			// Link throughput is the maximum, i.e. the supply solution. The consumer solution may be
			// less than this if the consumer is buffering.
			var linkSolutions = nodes
				.SelectMany(x => x.OutputLinks)
				.ToDictionary(x => x, x => solutionFor(Tuple.Create(x, EndpointType.SUPPLY)));

			return new Solution(nodeSolutions, linkSolutions);
		}

		public enum RateType { ACTUAL, ERROR, ABS_ERROR }

		// Ensure that the solution has a rate matching desired for this node. Typically there will
		// one of these on the ultimate output node, though multiple are supported, on any node. If
		// there is a conflict, a 'best effort' solution will be returned, where some nodes actual
		// rates will not match the desired asked for here.
		public void AddTarget(BaseNode node, double desiredRate)
		{
			var nodeVar = variableFor(node, RateType.ACTUAL);
			var errorVar = variableFor(node, RateType.ERROR);

			// The sum of the rate for this node, plus an error variable, must be equal to
			// desiredRate. In normal scenarios, the error variable will be zero.
			var constraint = MakeConstraint(desiredRate, desiredRate);
			constraint.SetCoefficient(nodeVar, 1);
			constraint.SetCoefficient(errorVar, 1);

			minimizeError(node, errorVar);
		}

		// Constrain a ratio on the output side of a node
		public void AddOutputRatio(BaseNode node, Item item, IEnumerable<NodeLink> links, double rate)
		{
			Debug.Assert(links.All(x => x.Supplier == node));

			addRatio(node, item, links, rate, EndpointType.SUPPLY);
		}

		// Constrain a ratio on the input side of a node
		public void AddInputRatio(BaseNode node, Item item, IEnumerable<NodeLink> links, double rate)
		{
			Debug.Assert(links.All(x => x.Consumer == node));

			addRatio(node, item, links, rate, EndpointType.CONSUME);
		}

		// Constrain input to a node for a particular item so that the node does not consume more
		// than is being produced by the supplier.
		// 
		// Consuming less than is being produced is fine. This represents a backup.
		public void AddInputLink(BaseNode node, Item item, IEnumerable<NodeLink> links)
		{
			Debug.Assert(links.All(x => x.Consumer == node));

			// Each item input/output to a recipe has one varible per link. These variables should be
			// related to one another using one of the other Ratio methods.
			foreach (var link in links)
			{
				var supplierVariable = variableFor(link, EndpointType.SUPPLY);
				var consumerVariable = variableFor(link, EndpointType.CONSUME);
				var errorVariable = variableFor(link, EndpointType.ERROR);

				{
					// The consuming end of the link must be no greater than the supplying end.
					var constraint = MakeConstraint(0, double.PositiveInfinity);
					constraint.SetCoefficient(supplierVariable, 1);
					constraint.SetCoefficient(consumerVariable, -1);
				}

				// Minimize over-supply. Necessary for unbalanced diamond recipe chains (such as
				// Yuoki smelting - this doesn't occur in Vanilla) where the deficit is made up by an
				// infinite supplier, in order to not just grab everything from that supplier and let
				// produced materials backup. Also, this is needed so that resources don't "pool" in
				// pass-through nodes.
				//
				// TODO: A more correct solution for pass-through would be to forbid over-supply on them.
				{
					var constraint = MakeConstraint(0, 0);
					constraint.SetCoefficient(errorVariable, 1);
					constraint.SetCoefficient(supplierVariable, -1);
					constraint.SetCoefficient(consumerVariable, 1);

					// The cost of over-supply needs to be greater than benefit of minimizing rate,
					// other-wise pure consumption nodes won't consume anything.
					objective.SetCoefficient(errorVariable, 100);
				}
			}
		}

		// Ensure that the sum on the end of all the links is in relation to the rate of the recipe.
		// The given rate is always for a single execution of the recipe, so the ratio is always (X1
		// + X2 + ... + XN)*Rate:1
		//
		// For example, if a copper wire recipe (1 plate makes 2 wires) is connected to two different
		// consumers, then the sum of the wire rate flowing over those two links must be equal to 2
		// time the rate of the recipe.
		private void addRatio(BaseNode node, Item item, IEnumerable<NodeLink> links, double rate, EndpointType type)
		{
			// Ensure that the sum of all inputs for this type of item is in relation to the rate of the recipe
			// So for the steel input to a solar panel, the sum of every input variable to this node must equal 5 * rate.
			var constraint = MakeConstraint(0, 0);
			var rateVariable = variableFor(node);

			constraint.SetCoefficient(rateVariable, rate);
			foreach (var link in links)
			{
				var variable = variableFor(link, type);
				constraint.SetCoefficient(variable, -1);
			}
		}

		private void minimizeError(BaseNode node, Variable errorVar)
		{
			var absErrorVar = variableFor(node, RateType.ABS_ERROR);

			// These constraints translate the minimization of the absolute variable:
			//
			//     min(|e|)
			//
			// To a form that is expressible directly to the solver, by introducing a shadow variable z:
			//
			//     z - e >= 0
			//     z + e >= 0
			//     min(z)
			//
			// This is counter-intuitive at first! Key insight: only one constraint will be relevant,
			// depending on whether e is positive or negative.
			var abs1 = MakeConstraint(0, double.PositiveInfinity);
			abs1.SetCoefficient(absErrorVar, 1);
			abs1.SetCoefficient(errorVar, 1);

			var abs2 = MakeConstraint(0, double.PositiveInfinity);
			abs2.SetCoefficient(absErrorVar, 1);
			abs2.SetCoefficient(errorVar, -1);

			objective.SetCoefficient(absErrorVar, 1000000);
		}

		private Constraint MakeConstraint(double low, double high)
		{
			return solver.MakeConstraint(low, high);
		}

		private Variable variableFor(NodeLink inputLink, EndpointType type)
		{
			return variableFor(Tuple.Create(inputLink, type), makeName("link", type, inputLink.Consumer.DisplayName, inputLink.Item.FriendlyName));
		}

		private string makeName(params object[] components)
		{
			return string.Join(":", components).ToLower().Replace(" ", "-");
		}

		private Variable variableFor(BaseNode node, RateType type = RateType.ACTUAL)
		{
			return variableFor(Tuple.Create(node, type), makeName("node", type, node.DisplayName));
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