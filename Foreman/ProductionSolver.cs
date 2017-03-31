using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private Objective objective;

        private Solver solver;

        // There is no way to generate a unique string/name for nodes, so instead store a map so they
        // can be uniquely associated.
        private Dictionary<object, Variable> allVariables;

        // We only keep track of constraints as we create them for debugging purposes. OrTools
        // doesn't provide a method for listing all constraints on a solver, which is unfortunate.
        private List<Constraint> allConstraints;

        // Used to ensure uniqueness of variables names
        private int counter;

        enum EndpointType { SUPPLY, CONSUME }

        public ProductionSolver()
        {
            this.solver = Solver.CreateSolver("Foreman", "GLOP_LINEAR_PROGRAMMING");
            this.objective = solver.Objective();
            this.allVariables = new Dictionary<object, Variable>();
            this.allConstraints = new List<Constraint>();
        }

        // Returns null if no optimal solution can be found. Technically GLOP can return non-optimal
        // solutions, but since I don't have any inputs that generate those I'm opting on the side of
        // safety by treating those as non-solutions.
        public Dictionary<ProductionNode, double> Solve()
        {
            var nodes = new List<ProductionNode>();

            foreach (var variable in allVariables)
            {
                // This upcasting is pretty gross, but it saves us having to keep track of them
                // in yet another field.
                if (variable.Key is ProductionNode)
                {
                    var node = (ProductionNode)variable.Key;
                    nodes.Add(node);
                    objective.SetCoefficient(variableFor(node), 1);
                }
            }

            if (nodes.Count == 0)
                return null;

            objective.SetMinimization();

            if (solver.Solve() != Solver.OPTIMAL)
                return null;

            return nodes.ToDictionary(x => x, x => variableFor(x).SolutionValue());
        }

        // Constrain the rate for the given node to be at least the desired rate. It's possible that
        // it will be more than this, in the case of overproduction.
        public void AddTarget(ProductionNode node, float desiredRate)
        {
            Variable nodeVar = variableFor(node);
            var constraint = MakeConstraint(desiredRate, double.PositiveInfinity);
            constraint.SetCoefficient(nodeVar, 1);
        }

        // Constrain a ratio on the output side of a node
        public void AddOutputRatio(ProductionNode node, Item item, IEnumerable<NodeLink> links, float rate)
        {
            Debug.Assert(links.All(x => x.Supplier == node));

            AddRatio(node, item, links, rate, EndpointType.SUPPLY);
        }

        // Constrain a ratio on the input side of a node
        public void AddInputRatio(ProductionNode node, Item item, IEnumerable<NodeLink> links, float rate)
        {
            Debug.Assert(links.All(x => x.Consumer == node));

            AddRatio(node, item, links, rate, EndpointType.CONSUME);
        }

        // Constrain input to a node for a particular item so that the node does not consume more
        // than is being produced by the supplier.
        // 
        // Consuming less than is being produced is fine. This represents a backup.
        public void AddInputAllowBackup(ProductionNode node, Item item, IEnumerable<NodeLink> links, float inputRate)
        {
            AddInput(node, item, links, inputRate, double.PositiveInfinity);
        }

        // Constrain input to a node for a particular item so that the node consumes everything that is being produced.
        public void AddInputConsumeAll(ProductionNode node, Item item, IEnumerable<NodeLink> links, float inputRate)
        {
            AddInput(node, item, links, inputRate, 0.0);
        }

        // Each item input/output to a recipe has one varible per link. These variables should be
        // related to one another using one of the other Ratio methods.
        private void AddInput(ProductionNode node, Item item, IEnumerable<NodeLink> links, float inputRate, double upperBound)
        {
            Debug.Assert(links.All(x => x.Consumer == node));

            foreach (var link in links)
            {
                var constraint = MakeConstraint(0, upperBound);
                var supplierVariable = variableFor(link, EndpointType.SUPPLY);
                var consumerVariable = variableFor(link, EndpointType.CONSUME);
                constraint.SetCoefficient(supplierVariable, 1);
                constraint.SetCoefficient(consumerVariable, -1);
            }
        }

        // Ensure that the sum on the end of all the links is in relation to the rate of the recipe.
        // The given rate is always for a single execution of the recipe, so the ratio is always (X1
        // + X2 + ... + XN)*Rate:1
        //
        // For example, if a copper wire recipe (1 plate makes 2 wires) is connected to two different
        // consumers, then the sum of the wire rate flowing over those two links must be equal to 2
        // time the rate of the recipe.
        private void AddRatio(ProductionNode node, Item item, IEnumerable<NodeLink> links, float rate, EndpointType type)
        {
            // Ensure that the sum of all inputs for this type of item is in relation to the rate of the recipe
            // So for the steel input to a solar panel, the sum of every input variable to this node must equal 5 * rate.
            var constraint = MakeConstraint(0, 0);
            var desiredRateVariable = variableFor(node);

            constraint.SetCoefficient(desiredRateVariable, rate);
            foreach (var link in links)
            {
                var variable = variableFor(link, type);
                constraint.SetCoefficient(variable, -1);
            }
        }

        private Constraint MakeConstraint(double low, double high)
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
                return variableFor(node, "supplier:" + node.DisplayName.ToLower());
            }
            else if (node is ConsumerNode)
            {
                return variableFor(node, "consumer:" + node.DisplayName.ToLower());

            }
            else
            {
                return variableFor(node, "node:" + node.DisplayName.ToLower());
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

        // A human-readable description of the constraints. Useful for debugging.
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
}