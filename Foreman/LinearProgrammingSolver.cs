using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	class LinearProgrammingSolver
	{
		List<LinearEquation> rows = new List<LinearEquation>();
		List<Constraint> startingConstraints = new List<Constraint>();
		decimal[] objectiveFunctionCoefficients;

		public void AddConstraint(Constraint constraint)
		{
			startingConstraints.Add(constraint);
		}

		public void SetObjectiveFunction(decimal[] coefficients, ObjectiveFunctionType type)
		{
			objectiveFunctionCoefficients = coefficients.ToArray();

			if (type == ObjectiveFunctionType.Minimise)
			{
				for (int i = 0; i < coefficients.Count(); i++)
				{
					objectiveFunctionCoefficients[i] = -objectiveFunctionCoefficients[i];
				}
			}
		}

		public decimal[] solve()
		{
			for (int i = 0; i < startingConstraints.Count(); i++)
			{
				rows.Add(ConvertConstraintToEquation(startingConstraints[i], i));
			}

			Array.Resize(ref objectiveFunctionCoefficients, NumCoefficients);
			rows.Add(new LinearEquation(0M, objectiveFunctionCoefficients.Select(c => -c).ToArray()));  //Objective function row

			while (RHSHasNegatives()) //Tableau is non-standard and needs to be standardised
			{
				int indicatorRow = findNSIndicatorRow();
				if (!rows[indicatorRow].HasNegatives)
				{
					//Problem is unsolvable
					return new decimal[NumCoefficients];
				}

				int pivotColumn = rows[indicatorRow].IndexOfMostNegative;
				int pivotRow = choosePivotRow(pivotColumn);

				doPivotTransformations(pivotRow, pivotColumn);
			}

			//Now it's standard!
			int infiniteStopper = 1000;
			while (rows.Last().HasNegatives && infiniteStopper-- > 0)
			{
				int pivotColumn = rows.Last().IndexOfMostNegative;
				int pivotRow = choosePivotRow(pivotColumn);

				doPivotTransformations(pivotRow, pivotColumn);
			}

			decimal[] solutions = new decimal[objectiveFunctionCoefficients.Count()];

			for (int column = 0; column < objectiveFunctionCoefficients.Count(); column++)
			{
				int nonZeroCount = 0;
				int nonZeroRow = -1;
				for (int row = 0; row < rows.Count - 1; row++)
				{
					if (rows[row][column] != 0)
					{
						nonZeroCount++;
						nonZeroRow = row;
					}
				}

				if (nonZeroCount == 1)
				{
					solutions[column] = rows[nonZeroRow].RHS;
				}
			}

			return solutions;
		}

		private void doPivotTransformations(int pivotRow, int pivotColumn)
		{
			for (int i = 0; i < rows.Count(); i++)
			{
				if (i != pivotRow)
				{
					rows[i].AddCoefficients(rows[pivotRow], rows[i][pivotColumn] / -rows[pivotRow][pivotColumn]);
				}
			}
			rows[pivotRow].MultiplyCoefficients(1M / rows[pivotRow][pivotColumn]);
		}

		private int choosePivotRow(int pivotColumn)
		{
			decimal[] ratios = new decimal[rows.Count];
			ratios[ratios.Count() - 1] = -1;
			for (int i = 0; i < rows.Count - 1; i++)
			{
				if (rows[i].RHS == 0 && rows[i][pivotColumn] < 0)
				{
					ratios[i] = -1; //Because we need to retain the sign when dividing zero by a negative number
				}
				else
				{
					try
					{
						ratios[i] = rows[i].RHS / rows[i][pivotColumn];
					}
					catch (DivideByZeroException)
					{
						ratios[i] = Decimal.MaxValue;
					}
				}
			}

			int indexOfLowestRatio = -1;
			for (int i = 0; i < ratios.Count(); i++)
			{
				if (ratios[i] >= 0 && (indexOfLowestRatio == -1 || ratios[i] < ratios[indexOfLowestRatio]))
				{
					indexOfLowestRatio = i;
				}
			}

			return indexOfLowestRatio;
		}

		private bool RHSHasNegatives()  //Not including objective row
		{
			for (int i = 0; i < rows.Count() - 1; i++)
			{
				if (rows[i].RHS < 0)
				{
					return true;
				}
			}
			return false;
		}

		private int findNSIndicatorRow()
		{
			decimal lowestValue = 0;
			int lowestRow = rows.Count - 1;
			for (int i = 0; i < rows.Count() - 1; i++)
			{
				if (rows[i].RHS < lowestValue)
				{
					lowestRow = i;
				}
			}

			return lowestRow;
		}

		private LinearEquation ConvertConstraintToEquation(Constraint constraint, int index)
		{
			decimal multiplier = 1M;
			if (constraint.Type == ConstraintType.GreaterThan)	//Equations are assumed to be less-than by the solver
			{
				multiplier = -1M;
			}
			LinearEquation result = new LinearEquation(constraint.RHS * multiplier);
			decimal[] equationCoefficients = new decimal[constraint.Coefficients.Count() + ConstraintCount];
			for (int i = 0; i < constraint.Coefficients.Count(); i++)
			{
				equationCoefficients[i] = constraint.Coefficients[i] * multiplier;
			}
			equationCoefficients[constraint.Coefficients.Count() + index] = 1M;	//Add slack variable

			result.Coefficients = equationCoefficients;
			return result;
		}

		public int ConstraintCount
		{
			get
			{
				return startingConstraints.Count();
			}
		}

		public int NumCoefficients
		{
			get
			{
				if (!rows.Any())
				{
					return 0;
				}
				else
				{
					return rows[0].Coefficients.Count();
				}
			}
		}
	}

	class Constraint
	{
		public ConstraintType Type;
		public decimal RHS;
		public decimal[] Coefficients;

		public Constraint(decimal rhs, ConstraintType type, params decimal[] coefficients)
		{
			this.RHS = rhs;
			this.Type = type;
			this.Coefficients = coefficients;
		}

		public override string ToString()
		{
			string output = "";
			int aChar = 97;

			for (int i = 0; i < Coefficients.Count(); i++)
			{
				if (Coefficients[i] != 0)
				{
					output += Coefficients[i].ToString("+#;-#") + (char)(aChar + i) + " ";
				}
			}

			if (Type == ConstraintType.GreaterThan)
			{
				output += ">= ";
			}
			else
			{
				output += "<= ";
			}

			output += RHS.ToString();

			return output;
		}
	}

	enum ConstraintType { GreaterThan, LessThan };
	enum ObjectiveFunctionType { Maximise, Minimise };

	class LinearEquation
	{
		public decimal[] Coefficients;
		public decimal RHS;

		public LinearEquation(decimal result, params decimal[] coefficients)
		{
			this.Coefficients = coefficients;
			this.RHS = result;
		}

		public decimal this[int i]
		{
			get { return Coefficients[i]; }
			set { Coefficients[i] = value; }
		}

		public void AddCoefficients(LinearEquation pivotEquation, decimal factor)
		{
			for (int i = 0; i < this.Coefficients.Count(); i++)
			{
				this[i] += pivotEquation[i] * factor;
				if (Math.Abs(this[i]) < 0.000000001M)    //Because sometimes rounding errors mean it's not quite zero, and it needs to be
				{
					this[i] = 0;
				}
			}

			this.RHS += pivotEquation.RHS * factor;
		}

		public void MultiplyCoefficients(decimal factor)
		{
			for (int i = 0; i < Coefficients.Count(); i++)
			{
				Coefficients[i] *= factor;
			}
			RHS *= factor;
		}

		public int IndexOfFirstNonZero
		{
			get
			{
				for (int i = 0; i < Coefficients.Count(); i++)
				{
					if (this[i] != 0) return i;
				}
				return -1;
			}
		}

		public int IndexOfMostNegative
		{
			get
			{
				int lowest = 0;
				for (int i = 0; i < Coefficients.Count(); i++)
				{
					if (Coefficients[i] < Coefficients[lowest])
					{
						lowest = i;
					}
				}
				return lowest;
			}
		}

		public bool HasNegatives
		{
			get
			{
				foreach (decimal coefficient in Coefficients)
				{
					if (coefficient < 0)
					{
						return true;
					}
				}

				return false;
			}
		}

		public int NonZeroCount
		{
			get
			{
				int count = 0;
				for (int i = 0; i < Coefficients.Count(); i++)
				{
					if (this[i] != 0) count++;
				}
				return count;
			}
		}

		public override string ToString()
		{
			string output = "";
			foreach (decimal value in Coefficients)
			{
				output += String.Format("{0,-8:0}", decimal.Round(value, 4)) + " ";
			}
			output += " | " + decimal.Round(RHS, 4);

			return output;
		}
	}
}
