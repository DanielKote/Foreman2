using System;
using Google.OrTools.LinearSolver;
using System.Collections.Generic;
using System.Text;

namespace Foreman
{
	// A super thin wrapper around OrTools.LinearSolver to make up for its deficiences as a generated class.
	public class GoogleSolver
	{
		private Solver solver;
		private List<Variable> variables;
		private List<Constraint> constraints;

		public static GoogleSolver Create()
		{
			return new GoogleSolver();
		}

		public GoogleSolver()
		{
			this.solver = Solver.CreateSolver("Foreman", "GLOP_LINEAR_PROGRAMMING");
			this.variables = new List<Variable>();
			this.constraints = new List<Constraint>();
		}

		public override string ToString()
		{
			var desc = new StringBuilder();
			desc.AppendLine("== Constraints");

			foreach (var constraint in this.constraints)
			{
				var line = new List<string>();
				foreach (var variable in this.variables)
				{
					var coefficient = constraint.GetCoefficient(variable);
					if (coefficient != 0.0)
					{
						line.Add(coefficient + " * " + variable.Name());
					}
				}
				desc.AppendFormat("{0} → ({1}, {2})\n", string.Join(" + ", line), constraint.Lb(), constraint.Ub());
			}
			desc.AppendLine("");
			desc.AppendLine("");
			desc.AppendLine("== Variables");

			foreach (var variable in this.variables)
			{
				desc.AppendFormat("{0} = {1}\n", variable.Name(), variable.SolutionValue());
			}

			return desc.ToString();
		}

		internal Objective Objective()
		{
			return solver.Objective();
		}

		internal int Solve()
		{
			return solver.Solve();
		}

		internal Constraint MakeConstraint(double low, double high)
		{
			var constraint = solver.MakeConstraint(low, high);
			this.constraints.Add(constraint);
			return constraint;
		}

		internal Variable MakeNumVar(double low, double high, string name)
		{
			var variable = solver.MakeNumVar(low, high, name);
			this.variables.Add(variable);
			return variable;
		}
	}
}