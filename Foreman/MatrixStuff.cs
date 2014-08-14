using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman
{
	static class MatrixStuff
	{
		public static int[,] Multiply(this int[,] a, int[,] b)
		{
			System.Diagnostics.Debug.Assert(a.GetLength(0) == b.GetLength(1));

			int[,] result = new int[b.GetLength(0), a.GetLength(1)];
			
			for (int x = 0; x < result.GetLength(0); x++)
			{
				for (int y = 0; y < result.GetLength(1); y++)
				{
					for (int i = 0; i < a.GetLength(0); i++)
					{
						result[x, y] += a[i, y] * b[x, i];
					}
				}
			}

			return result;
		}

		public static int[,] Add(this int[,] a, int[,] b)
		{
			System.Diagnostics.Debug.Assert(a.GetLength(0) == b.GetLength(0));
			System.Diagnostics.Debug.Assert(a.GetLength(1) == b.GetLength(1));

			int[,] result = new int[a.GetLength(0), a.GetLength(1)];

			for (int x = 0; x < result.GetLength(0); x++)
			{
				for (int y = 0; y < result.GetLength(1); y++)
				{
					result[x, y] = a[x, y] + b[x, y];
				}
			}

			return result;
		}
	}
}
