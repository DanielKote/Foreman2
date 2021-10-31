using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman
{
	public class AssemblerSelector
	{
		public enum Style { Worst, WorstNonBurner, Best, BestNonBurner, MostModules }
		public static readonly string[] StyleNames = new string[] { "Worst", "Worst non-Burer", "Best", "Best non-Burner", "Most Modules" };

		public Style SelectionStyle { get; set; }

		public AssemblerSelector() { SelectionStyle = Style.WorstNonBurner; }

		public Assembler GetAssembler(Recipe recipe)
		{
			if (SelectionStyle == Style.MostModules)
			{
				return recipe.Assemblers
					.OrderBy(a => a.Enabled)
					.ThenBy(a => a.Available)
					.ThenBy(a => ((a.ModuleSlots * 1000000) + a.Speed))
					.LastOrDefault();
			}
			else
			{
				int orderDirection;
				bool noBurners;

				switch (SelectionStyle)
				{
					case Style.Worst:
						orderDirection = -1;
						noBurners = false;
						break;
					case Style.WorstNonBurner:
						orderDirection = -1;
						noBurners = true;
						break;
					case Style.Best:
						orderDirection = 1;
						noBurners = false;
						break;
					case Style.BestNonBurner:
					default:
						orderDirection = 1;
						noBurners = true;
						break;
				}

				return recipe.Assemblers.Where(a => !(noBurners && a.IsBurner))
					.OrderBy(a => a.Enabled)
					.ThenBy(a => a.Available)
					.ThenBy(a => orderDirection * ((a.Speed * 1000000) + a.ModuleSlots))
					.LastOrDefault();
			}
		}
	}
}
