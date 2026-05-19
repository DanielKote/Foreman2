using Google.OrTools.LinearSolver;
using System.Collections.Generic;
using System.Text;

using Solver_t = Google.OrTools.LinearSolver.Solver;

namespace Foreman.Models.Solver {
    // A super thin wrapper around OrTools.LinearSolver to make up for its deficiences as a generated class.
    public class GoogleSolver {
        private readonly Solver_t solver;
        private readonly List<Variable> variables;
        private readonly List<Constraint> constraints;

        public static GoogleSolver Create() {
            return new GoogleSolver();
        }

        public GoogleSolver() {
            solver = Solver_t.CreateSolver("GLOP");
            variables = [];
            constraints = [];
        }

        public override string ToString() {
            var desc = new StringBuilder();
            desc.AppendLine("== Constraints");

            foreach (var constraint in constraints) {
                var line = new List<string>();
                foreach (var variable in variables) {
                    var coefficient = constraint.GetCoefficient(variable);
                    if (coefficient != 0.0) {
                        line.Add(coefficient + " * " + variable.Name());
                    }
                }
                desc.AppendFormat(CultureInfo.InvariantCulture, "{0} → ({1}, {2})\n", string.Join(" + ", line), constraint.Lb(), constraint.Ub());
            }
            desc.AppendLine("");
            desc.AppendLine("");
            desc.AppendLine("== Variables");

            foreach (var variable in variables) {
                desc.AppendFormat(CultureInfo.InvariantCulture, "{0} = {1}\n", variable.Name(), variable.SolutionValue());
            }

            return desc.ToString();
        }

        internal Objective Objective() {
            return solver.Objective();
        }

        internal Solver_t.ResultStatus Solve() {
            return solver.Solve();
        }

        internal Constraint MakeConstraint(double low, double high) {
            var constraint = solver.MakeConstraint(low, high);
            constraints.Add(constraint);
            return constraint;
        }

        internal Variable MakeNumVar(double low, double high, string name) {
            var variable = solver.MakeNumVar(low, high, name);
            variables.Add(variable);
            return variable;
        }
    }
}
