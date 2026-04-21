using Google.OrTools.LinearSolver;
using System.Collections.Generic;
using System.Text;

namespace Foreman {
    // A super thin wrapper around OrTools.LinearSolver to make up for its deficiencies as a generated class.
    public class GoogleSolver {
        private Solver _solver = Solver.CreateSolver("GLOP");
        private List<Variable> _variables = [];
        private List<Constraint> _constraints = [];

        public static GoogleSolver Create() {
            return new GoogleSolver();
        }

        public override string ToString() {
            var desc = new StringBuilder();
            desc.AppendLine("== Constraints");

            foreach (var constraint in _constraints) {
                var line = new List<string>();
                foreach (var variable in _variables) {
                    var coefficient = constraint.GetCoefficient(variable);
                    if (coefficient != 0.0) {
                        line.Add(coefficient + " * " + variable.Name());
                    }
                }

                desc.Append($"{string.Join(" + ", line)} → ({constraint.Lb()}, {constraint.Ub()})\n");
            }

            desc.AppendLine("");
            desc.AppendLine("");
            desc.AppendLine("== Variables");

            foreach (var variable in _variables) {
                desc.Append($"{variable.Name()} = {variable.SolutionValue()}\n");
            }

            return desc.ToString();
        }

        internal Objective Objective() {
            return _solver.Objective();
        }

        internal Solver.ResultStatus Solve() {
            return _solver.Solve();
        }

        internal Constraint MakeConstraint(double low, double high) {
            var constraint = _solver.MakeConstraint(low, high);
            _constraints.Add(constraint);
            return constraint;
        }

        internal Variable MakeNumVar(double low, double high, string name) {
            var variable = _solver.MakeNumVar(low, high, name);
            _variables.Add(variable);
            return variable;
        }
    }
}