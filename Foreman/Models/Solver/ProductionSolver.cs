using Foreman.Models.Nodes;
using Google.OrTools.LinearSolver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Solver_t = Google.OrTools.LinearSolver.Solver;

namespace Foreman.Models.Solver {
    // A wrapper around Google's Optimization Tools, specifically the Linear Programming library. We
    // can express a factory as a system of linear constraints, and this library takes care of
    // solving them for us.
    //
    // Google also provides a library for Flow Algorithms which sounds like an appealing and
    // potentially simpler alternative, though it's not obvious to me that the problem maps exactly.
    //
    // https://developers.google.com/optimization/lp/glop
    public class ProductionSolver {
        public double LowPriorityMultiplier { get; set; }

        private readonly double outputObjectiveCoefficient; //we want to maximize the output of each automatic consumer node.
        private readonly double factoryObjectiveCoefficient; //we want to minimize the number of buildings (of all recipe nodes), but not at the expense of oversupply or errors

        private readonly double overflowObjectiveCoefficient; //cost of oversupply needs to be great enough that the solver doesnt choose to 0 all recipe nodes and swallow any produced items as 'oversupply'. This needs to take into account the current nodes output ratios (ex: if item is produced extremely slowly, this value needs to be high enough for the solver not to decide to 0 its use)
        private readonly double errorObjectiveCoefficient; //errors should be avoided at all cost (if possible)

        public class Solution(Dictionary<BaseNode, double> nodes, Dictionary<NodeLink, double> links) {
            public Dictionary<BaseNode, double> Nodes { get; private set; } = nodes;
            public Dictionary<NodeLink, double> Links { get; private set; } = links;

            public double ActualRate(BaseNode node) {
                return Nodes[node];
            }

            public double Throughput(NodeLink link) {
                return Links[link];
            }
        }

        private readonly Objective objective;

        private readonly GoogleSolver solver;

        // There is no way to generate a unique string/name for nodes, so instead store a map so they
        // can be uniquely associated.
        private readonly Dictionary<object, Variable> allVariables;

        // We only keep track of constraints as we create them for debugging purposes. OrTools
        // doesn't provide a method for listing all constraints on a solver, which is unfortunate.

        // Keep track of nodes as they are added to ensure the solution contains all of them, even if
        // there are no links.
        private readonly List<BaseNode> nodes;

        // Used to ensure uniqueness of variables names
        private int counter;

        private enum LinkType { LINK, ERROR }
        private enum RateType { ACTUAL, ERROR }

        public ProductionSolver(bool pullOutputNodes, double pullPower, double minRecipeOutRate, double lowPriorityMultiplier) : this(pullOutputNodes, pullPower, 1e-2, 1e-1 / Math.Min(1e-3, minRecipeOutRate), 1e2 / Math.Min(1e-3, minRecipeOutRate), lowPriorityMultiplier) { } //io ratio is the maximum output imbalance (ex: 1 deuterium cell (highest nuclear in seablock) is enough to produce 120,000 MJ of heat and thus is consumed at around 1/120000 per sec, so the minRecipeOutRate should be 1/120000)

        public ProductionSolver(bool pullOutputNodes, double outputObjectiveC, double factoryObjectiveC, double overflowObjectiveC, double errorObjectiveC, double lowPriorityMultiplier) {
            LowPriorityMultiplier = lowPriorityMultiplier;
            outputObjectiveCoefficient = pullOutputNodes ? outputObjectiveC : 0;
            factoryObjectiveCoefficient = factoryObjectiveC;
            overflowObjectiveCoefficient = overflowObjectiveC;
            errorObjectiveCoefficient = errorObjectiveC;

            solver = GoogleSolver.Create();
            objective = solver.Objective();
            allVariables = [];
            nodes = [];
        }

        public void AddNode(BaseNode node) {
            _ = VariableFor(node); // TODO: If this has no side-effects, delete it.
            nodes.Add(node);
        }

        //we want to minimize the number of buildings (so recipe nodes only). For all other nodes we dont care about the rates, since their flows will be dictated by other factors.
        //this does mean that we prefer paths with least number of buildings, which may mean more source items consumed (ex: a base oil process with speed modules will be prefered over an advanced oil process without speed modules)
        //however since there is a cost associated with providing those items (through more buildings for resource extraction), this should be OK for most use-cases.
        public void AddRecipeNode(RecipeNode node, double factoryRateCoefficient) {
            Variable nodeRate = VariableFor(node);
            nodes.Add(node);
            objective.SetCoefficient(nodeRate, factoryObjectiveCoefficient * factoryRateCoefficient * (node.LowPriority ? LowPriorityMultiplier : 1));
        }

        // Returns null if no optimal solution can be found. Technically GLOP can return non-optimal
        // solutions, but since I don't have any inputs that generate those I'm opting on the side of
        // safety by treating those as non-solutions.
        public Solution? Solve() {
            if (nodes.Count == 0)
                return new Solution([], []); //no nodes mean empty solution (no errors)

            objective.SetMinimization();

            //solver.Solve(); //<<---------------------------------- Cyclic recipes with 'not enough provided' can lead to no-solution. Cyclic recipes with 'extra left' lead to an over-supply (solution found)
            //ex: coal liquifaction produces more heavy oil than is required (25->90) -> solution will be found (if it is connected back to itself only), but there will be an over-production of heavy oil.
            //    Kovarex enrichment produces less Uranium 238 than is required (5->2) -> solution will be 0 (if it is connected back to itself only), as there is no way to satisfy the inputs. In a more complicated case with multiple nodes (instead of one recipe looped back to itself), this can lead to a null-solution (error)
            if (solver.Solve() != Solver_t.ResultStatus.OPTIMAL)
                return null; //error solution -> sets all values to 0 and records the error

            var nodeSolutions = nodes
                .ToDictionary(x => x, x => SolutionFor(Tuple.Create(x, RateType.ACTUAL)));

            var linkSolutions = nodes
                .SelectMany(x => x.OutputLinks)
                .ToDictionary(x => x, x => SolutionFor(x));

            return new Solution(nodeSolutions, linkSolutions);
        }

        // Ensure that the solution has a rate matching desired for this node. Typically there will
        // one of these on the ultimate output node, though multiple are supported, on any node. If
        // there is a conflict, a 'best effort' solution will be returned, where some nodes actual
        // rates will be less than the desired asked for here.
        public void AddTarget(BaseNode node, double desiredRate) {
            Variable nodeVar = VariableFor(node, RateType.ACTUAL);
            Variable errorVar = VariableFor(node, RateType.ERROR);

            // The sum of the rate for this node, plus an error variable, must be equal to
            // desiredRate. In normal scenarios, the error variable will be zero. In error scenarios the error variable will be +ve non-zero.
            Constraint constraint = MakeConstraint(desiredRate, desiredRate);
            constraint.SetCoefficient(nodeVar, 1);
            constraint.SetCoefficient(errorVar, 1);

            objective.SetCoefficient(errorVar, errorObjectiveCoefficient);
        }

        //we want to maximize the amount of output items, so we add a negative weight to the objective for the given consumer node.
        //Only done if asked for, and can easily lead to unbound solutions (where due to -ve weighting an 'infinite' amount of output at the expense of 'infinite' number
        //of factories is considered the 'optimal' solution
        public void AddOutputObjective(ConsumerNode node) {
            if (outputObjectiveCoefficient > 0)
                objective.SetCoefficient(VariableFor(node), -outputObjectiveCoefficient);
        }

        //set the node to be zero (used for passthrough nodes with missing input or output)
        public void SetZero(PassthroughNode node) {
            Variable nodeVar = VariableFor(node);
            Constraint constraint = MakeConstraint(0, 0);
            constraint.SetCoefficient(nodeVar, 1);
        }

        // Constrain a ratio on the output side of a node. This is done for each unique item, and constrains the producted item (based on the node rate) to be equal to the amount of the item transported away by the links
        // Due to the possibility of an overflow, we introduce an 'overflow' variable here that accounts for any extra items produced that cant be consumed by the nodes above.
        //	BUT! this is done only for recipe nodes! all other nodes cant have overflows!
        public void AddOutputRatio(BaseNode node, ItemQualityPair item, IEnumerable<NodeLink> links, double rate) {
            Debug.Assert(links.All(x => x.SupplierNode == node));
            AddIORatio(node, item, links, rate, node is RecipeNode || node is SpoilNode || node is PlantNode);
        }

        // Constrain a ratio on the input side of a node. Done for each unique item, and constrains the consumed item (based on the node rate) to be equal to the amount of the item provided by the links.
        // unlike with the outputs, we dont have any error/overflow variables here. the numbers MUST equal
        public void AddInputRatio(BaseNode node, ItemQualityPair item, IEnumerable<NodeLink> links, double rate) {
            Debug.Assert(links.All(x => x.ConsumerNode == node));
            AddIORatio(node, item, links, rate, false);
        }

        private void AddIORatio(BaseNode node, ItemQualityPair item, IEnumerable<NodeLink> links, double rate, bool includeErrorVariable) {
            Constraint constraint = MakeConstraint(0, 0);
            Variable rateVariable = VariableFor(node);

            constraint.SetCoefficient(rateVariable, rate);
            foreach (var link in links) {
                Variable variable = VariableFor(link);
                constraint.SetCoefficient(variable, -1);
            }

            if (includeErrorVariable) {
                Variable errorVariable = VariableForOverflow(node, item);
                constraint.SetCoefficient(errorVariable, -1);
                objective.SetCoefficient(errorVariable, overflowObjectiveCoefficient);
            }
        }

        private Constraint MakeConstraint(double low, double high) {
            return solver.MakeConstraint(low, high);
        }

        private Variable VariableFor(NodeLink inputLink) {
            return VariableFor(inputLink, MakeName("link", "S(" + inputLink.ConsumerNode.NodeID + ")", "C(" + inputLink.ConsumerNode.NodeID + ")", inputLink.Item.ToString()));
        }

        private Variable VariableFor(BaseNode node, RateType type = RateType.ACTUAL) {
            return VariableFor(Tuple.Create(node, type), MakeName("node", type, node.NodeID, node.ToString() ?? "ERR"));
        }

        private Variable VariableForOverflow(BaseNode node, ItemQualityPair item) {
            return VariableFor(Tuple.Create(node, item), MakeName("node-overflow", node.NodeID, node.ToString() ?? "ERR", item.ToString()));
        }

        private Variable VariableFor(object key, string name) {
            if (allVariables.TryGetValue(key, out Variable? existing))
                return existing;

            Variable newVar = solver.MakeNumVar(0.0, double.PositiveInfinity, name + ":" + GetSequence());
            allVariables[key] = newVar;
            return newVar;
        }

        private double SolutionFor(object key) {
            return allVariables.TryGetValue(key, out Variable? variable) ? variable.SolutionValue() : 0.0;
        }

        private int GetSequence() {
            return this.counter += 1;
        }

        private static string MakeName(params object[] components) {
            return string.Join(":", components).ToLowerInvariant().Replace(" ", "-");
        }

        // A human-readable description of the constraints. Useful for debugging.
        public override string ToString() {
            return solver.ToString();
        }
    }
}
