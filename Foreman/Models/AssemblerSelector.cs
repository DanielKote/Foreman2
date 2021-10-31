using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman
{
	public class AssemblerSelector
	{
		public enum Style { Worst, WorstNonBurner, WorstBurner, Best, BestNonBurner, BestBurner, MostModules }
		public static readonly string[] StyleNames = new string[] { "Worst", "Worst Non-Burner", "Worst Burner", "Best", "Best Non-Burner", "Best Burner", "Most Modules" };

		public Style DefaultSelectionStyle { get; set; }

		public AssemblerSelector() { DefaultSelectionStyle = Style.WorstNonBurner; }


		public Assembler GetAssembler(Recipe recipe) { return GetAssembler(recipe, DefaultSelectionStyle); }
		public Assembler GetAssembler(Recipe recipe, Style style) { return GetOrderedAssemblerList(recipe, style).First(); }
		public List<Assembler> GetOrderedAssemblerList(Recipe recipe) { return GetOrderedAssemblerList(recipe, DefaultSelectionStyle); }

		public List<Assembler> GetOrderedAssemblerList(Recipe recipe, Style style)
		{ 
			if (style == Style.MostModules)
			{
				return recipe.Assemblers
					.OrderByDescending(a => a.Enabled)
					.ThenByDescending(a => a.Available)
					.ThenByDescending(a => ((a.ModuleSlots * 1000000) + a.Speed))
					.ToList();
			}
			else
			{
				int orderDirection;
				bool includeNonBurners;
				bool includeBurners;

				switch (style)
				{
					case Style.Worst:
						orderDirection = -1;
						includeNonBurners = true;
						includeBurners = true;
						break;
					case Style.WorstBurner:
						orderDirection = -1;
						includeNonBurners = false;
						includeBurners = true;
						break;
					case Style.WorstNonBurner:
						orderDirection = -1;
						includeNonBurners = true;
						includeBurners = false;
						break;
					case Style.Best:
						orderDirection = 1;
						includeNonBurners = true;
						includeBurners = true;
						break;
					case Style.BestBurner:
						orderDirection = 1;
						includeNonBurners = false;
						includeBurners = true;
						break;
					case Style.BestNonBurner:
					default:
						orderDirection = 1;
						includeNonBurners = true;
						includeBurners = false;
						break;
				}

				Console.WriteLine("ASSEMBLERS:");
				foreach (Assembler aa in recipe.Assemblers
					.OrderByDescending(a => a.Enabled)
					.ThenByDescending(a => (a.IsBurner && includeBurners) || (!a.IsBurner && includeNonBurners))
					.ThenByDescending(a => a.Available)
					.ThenByDescending(a => orderDirection * ((a.Speed * 1000000) + a.ModuleSlots))
					.ToList())
					Console.WriteLine(aa);

				return recipe.Assemblers
					.OrderByDescending(a => a.Enabled)
					.ThenByDescending(a => (a.IsBurner && includeBurners) || (!a.IsBurner && includeNonBurners))
					.ThenByDescending(a => a.Available)
					.ThenByDescending(a => orderDirection * ((a.Speed * 1000000) + a.ModuleSlots))
					.ToList();
			}
		}
	}
}
