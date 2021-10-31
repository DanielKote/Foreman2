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
			Assembler assembler = GetAssembler(recipe, SelectionStyle, !recipe.HasEnabledAssemblers); //get assembler based on style
			if (assembler == null) //if we didnt find one, loosen the parameters (if trying for non-burner, accept a burner)
			{
				if (SelectionStyle == Style.WorstNonBurner)
					assembler = GetAssembler(recipe, Style.Worst, !recipe.HasEnabledAssemblers);
				else if (SelectionStyle == Style.BestNonBurner)
					assembler = GetAssembler(recipe, Style.Best, !recipe.HasEnabledAssemblers);
			}

			return assembler;
		}

		private Assembler GetAssembler(Recipe recipe, Style style, bool allowDisabled)
		{
			switch (SelectionStyle)
			{
				case Style.Worst:
					return recipe.Assemblers.Where(a => (allowDisabled || a.Enabled)).OrderBy(a => (-(a.Speed * 1000000) - a.ModuleSlots)).LastOrDefault();
				case Style.WorstNonBurner:
					return recipe.Assemblers.Where(a => (allowDisabled || a.Enabled) && !a.IsBurner).OrderBy(a => (-(a.Speed * 1000000) - a.ModuleSlots)).LastOrDefault();
				case Style.Best:
					return recipe.Assemblers.Where(a => (allowDisabled || a.Enabled)).OrderBy(a => ((a.Speed * 1000000) + a.ModuleSlots)).LastOrDefault();
				case Style.BestNonBurner:
					return recipe.Assemblers.Where(a => (allowDisabled || a.Enabled) && !a.IsBurner).OrderBy(a => ((a.Speed * 1000000) + a.ModuleSlots)).LastOrDefault();
				case Style.MostModules:
					return recipe.Assemblers.Where(a => (allowDisabled || a.Enabled)).OrderBy(a => ((float)(a.ModuleSlots * 1000000) + a.Speed)).LastOrDefault();
			}
			return null; //shouldnt happen
		}
	}
}
