using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	public class ModuleSelector
	{
		public enum Style { None, Speed, Productivity, ProductivityOnly, Efficiency, EfficiencyOnly }
		public static readonly string[] StyleNames = new string[] { "None", "Speed", "Productivity", "Productivity Only", "Efficiency", "Efficiency Only" };

		public Style SelectionStyle { get; set; }

		public ModuleSelector() { SelectionStyle = Style.None; }

		public List<Module> GetModules(Assembler assembler, Recipe recipe)
		{
			List<Module> moduleList = new List<Module>();
			Module bestModule = null;
			if (assembler == null || assembler.ModuleSlots == 0)
				return moduleList;


			switch (SelectionStyle)
			{
				//speed, productivity, and productivity only have no max limits, so the 'best' option will always involve identical modules. just pick the best module and fill the module slots.
				//efficiency however is capped at -80% consumption bonus, so we have to get a permutation and pick the 'best'
				case Style.Speed:
					bestModule = assembler.Modules.Intersect(recipe.Modules).Where(m => m.Enabled).OrderBy(m => ((m.SpeedBonus * 1000) - m.ConsumptionBonus)).LastOrDefault();
					break;
				case Style.Productivity:
					bestModule = assembler.Modules.Intersect(recipe.Modules).Where(m => m.Enabled).OrderBy(m => ((m.ProductivityBonus * 1000) + m.SpeedBonus)).LastOrDefault();
					break;
				case Style.ProductivityOnly:
					bestModule = assembler.Modules.Intersect(recipe.Modules).Where(m => m.Enabled && m.ProductivityBonus != 0).OrderBy(m => ((m.ProductivityBonus * 1000) + m.SpeedBonus)).LastOrDefault();
					break;
				case Style.Efficiency:
					List<Module> speedModules = assembler.Modules.Intersect(recipe.Modules).Where(m => m.Enabled && m.SpeedBonus > 0).OrderByDescending(m => ((m.SpeedBonus * 1000) - m.ConsumptionBonus)).ToList(); //highest speed is first!
					List<Module> efficiencyModules = assembler.Modules.Intersect(recipe.Modules).Where(m => m.Enabled && m.ConsumptionBonus < 0).OrderByDescending(m => ((m.ConsumptionBonus * 1000) + m.SpeedBonus)).ToList(); //highest consumption is first! (so worst->best effectivity)
					List<Module> combinedModules = speedModules.Concat(efficiencyModules).ToList();

					//return best module permutation that has the lowest consumption (max -80%), and the highest speed.
					List<ModulePermutator.Permutation> modulePermutations = ModulePermutator.GetModulePermutations(combinedModules, assembler.ModuleSlots);
					return modulePermutations.OrderByDescending(p => p.ConsumptionBonus).ThenBy(p => p.SpeedBonus).ThenByDescending(p => p.SquaredTierValue).Last().Modules.Where(m => m != null).ToList();
				case Style.EfficiencyOnly:
					List<Module> moduleOptions = assembler.Modules.Intersect(recipe.Modules).Where(m => m.Enabled).OrderByDescending(m => ((m.ConsumptionBonus * 1000) - m.SpeedBonus)).ToList();

					//return best module permutation that has the lowest consumption (max -80%), and the lowest tier cost
					List<ModulePermutator.Permutation> modulePermutationsB = ModulePermutator.GetModulePermutations(moduleOptions, assembler.ModuleSlots);
					return modulePermutationsB.OrderByDescending(p => p.ConsumptionBonus).ThenByDescending(p => p.SquaredTierValue).Last().Modules.Where(m => m != null).ToList();
				case Style.None:
				default:
					break; //return the empty module list
			}

			if (bestModule != null)
				for (int i = 0; i < assembler.ModuleSlots; i++)
					moduleList.Add(bestModule);
			return moduleList;
		}
	}

	public static class ModulePermutator
	{
		public struct Permutation
		{
			public Module[] Modules; //NOTE: a null module is possible! this means that this particular permutation isnt using all slots.
			public float SpeedBonus;
			public float ProductivityBonus;
			public float ConsumptionBonus;
			public float PollutionBonus;
			public int SquaredTierValue; //total of all added modules tiers squared (ex: T1+T2+T3 would have a value of 1+4+9) ->usefull for solving for 'cheapest' option

			public Permutation(Module[] modules)
			{
				Modules = modules.ToArray();
				SpeedBonus = 0;
				ProductivityBonus = 0;
				ConsumptionBonus = 0;
				PollutionBonus = 0;
				SquaredTierValue = 0;

				foreach(Module m in modules)
				{
					if (m != null)
					{
						SpeedBonus += m.SpeedBonus;
						ProductivityBonus += m.ProductivityBonus;
						ConsumptionBonus += m.ConsumptionBonus;
						PollutionBonus += m.PollutionBonus;
						SquaredTierValue += m.Tier * m.Tier;
					}
				}

				ConsumptionBonus = Math.Max(-0.8f, ConsumptionBonus);
			}

			public override string ToString()
			{
				string str = "Speed: " + SpeedBonus.ToString("P") + ", Productivity: " + ProductivityBonus.ToString("P") + ", Energy: " + ConsumptionBonus.ToString("P") + ", Pollution: " + PollutionBonus.ToString("P") + ", SqTierValue: " + SquaredTierValue + ", Modules: ";
				foreach (Module m in Modules)
				{
					if (m != null)
						str += m + ", ";
					else
						str += "---, ";
				}
				return str;
			}
		}

		public static List<Permutation> GetModulePermutations(List<Module> moduleOptions, int moduleSlots) //keep in mind that this can get quite large; with 462 permutations of 6 modules into 6 slots, 12,376 of 12 modules into 6 slots, and 100,947 of 18 modules into 6 slots.
		{
			List<Permutation> permutations = new List<Permutation>();
			Module[] permutation = new Module[moduleSlots];
			AddModule(0, 0);
			return permutations;

			void AddModule(int itemIndex, int startingIndex) //depth first insertion of modules brute forcing all options.
			{
				if (itemIndex == permutation.Length)
					permutations.Add(new Permutation(permutation));
				else
				{
					for (int i = startingIndex; i < moduleOptions.Count; i++)
					{
						permutation[itemIndex] = moduleOptions[i];
						AddModule(itemIndex + 1, i);
					}
					permutation[itemIndex] = null;
					AddModule(itemIndex + 1, moduleOptions.Count); //the 'no module' option
				}
			}

		}
	}
}
