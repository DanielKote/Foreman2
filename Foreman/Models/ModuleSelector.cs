using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	public class ModuleSelector
	{
		public enum Style { None, Speed, Productivity, ProductivityOnly, Efficiency, EfficiencyOnly }
		public static readonly string[] StyleNames = new string[] { "None", "Speed", "Productivity", "Productivity Only", "Efficiency", "Efficiency Only" };

		public Style DefaultSelectionStyle { get; set; }

		public ModuleSelector() { DefaultSelectionStyle = Style.None; }

		public List<Module> GetModules(Assembler assembler, Recipe recipe) { return GetModules(assembler, recipe, DefaultSelectionStyle); }
		public List<Module> GetModules(Assembler assembler, Recipe recipe, Style style)
		{
			List<Module> moduleList = new List<Module>();
			Module bestModule = null;
			if (assembler == null || assembler.ModuleSlots == 0)
				return moduleList;


			switch (style)
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
					List<ModulePermutator.Permutation> modulePermutations = ModulePermutator.GetOptimalEfficiencyPermutations(speedModules, efficiencyModules, assembler.ModuleSlots);

					//return best module permutation that has the lowest consumption (max -80%), and the highest speed.
					if (modulePermutations.Count > 0)
						return modulePermutations.OrderByDescending(p => p.ConsumptionBonus).ThenBy(p => p.SpeedBonus).ThenByDescending(p => p.SquaredTierValue).Last().Modules.Where(m => m != null).OrderBy(m => m.FriendlyName).ToList();
					return moduleList; //empty
				case Style.EfficiencyOnly:
					List<Module> moduleOptions = assembler.Modules.Intersect(recipe.Modules).Where(m => m.Enabled && m.ConsumptionBonus < 0).OrderByDescending(m => ((m.ConsumptionBonus * 1000) - m.SpeedBonus)).ToList();
					List<ModulePermutator.Permutation> modulePermutationsB = ModulePermutator.GetOptimalEfficiencyPermutations(new List<Module>(), moduleOptions, assembler.ModuleSlots);

					//return best module permutation that has the lowest consumption (max -80%), and the lowest tier cost
					if (modulePermutationsB.Count > 0)
						return modulePermutationsB.OrderByDescending(p => p.ConsumptionBonus).ThenByDescending(p => p.SquaredTierValue).Last().Modules.Where(m => m != null).OrderBy(m => m.FriendlyName).ToList();
					return moduleList; //empty
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
			public double SpeedBonus;
			public double ProductivityBonus;
			public double ConsumptionBonus;
			public double PollutionBonus;
			public int SquaredTierValue; //total of all added modules tiers squared (ex: T1+T2+T3 would have a value of 1+4+9) ->usefull for solving for 'cheapest' option

			public Permutation(Module[] modules)
			{
				Modules = modules.ToArray();
				SpeedBonus = 0;
				ProductivityBonus = 0;
				ConsumptionBonus = 0;
				PollutionBonus = 0;
				SquaredTierValue = 0;

				foreach (Module m in modules)
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

		public static List<Permutation> GetOptimalEfficiencyPermutations(List<Module> speedModules, List<Module> efficiencyModules, int moduleSlots) //the original approach of brute forcing things runs into a ceiling when using over 12 modules or 12 slots (I mean, its a combination -> factorials everywhere)
		{
			//doe by a 'smart' approach: fill in the set with i of one type speed and (module slot - i) of one type efficiency, then refine by changing 1 of the speeds to a different module and 1 of the efficiencies to a different module.
			//so assume the ideal solution will be in the form (i - 1)* speedModule A + (1)* speedModuleB + (module slots - i - 1)* efficiencyModule A + (1) * efficiencyModuleB where speedModuleA and speedModuleB can be equal (same for efficiencyA and B)
			List<Permutation> permutations = new List<Permutation>();
			Module[] permutation = new Module[moduleSlots];


			//permutation will be in the form [ 1 speedModuleB, n-1 amounts of speedModuleA, moduleslots-n-1 amounts of efficiencyModuleA, 1 efficiencyModuleB ] where n is between 1 and moduleslots - 1.
			//thus 3 speed modules + 1 efficiency is valid. 1 speed module and 3 efficiency is valid. 4 speed only is not (neither is 4 efficiency)
			//will do nothing if there isnt at least 3 slots
			for (int sfA = 0; sfA < speedModules.Count; sfA++)
			{
				for (int efA = 0; efA < efficiencyModules.Count; efA++)
				{
					for (int border = 1; border < moduleSlots; border++)
					{
						for (int i = 1; i < border; i++)
							permutation[i] = speedModules[sfA];
						for (int i = border; i < moduleSlots - 1; i++)
							permutation[i] = efficiencyModules[efA];
						for (int sfB = sfA; sfB < speedModules.Count; sfB++)
						{
							permutation[0] = speedModules[sfB];
							for (int efB = efA; efB < efficiencyModules.Count; efB++)
							{
								permutation[moduleSlots - 1] = efficiencyModules[efB];
								permutations.Add(new Permutation(permutation));
							}
						}
					}
				}
			}

			//efficiency only permutations -> in the form of [n efficiency moduleA, x efficiency moduleB, moduleSlots-n-x null modules], with at least 1 of moduleA and 1 of moduleB
			//will do nothing if there arent at least 2 slots
			for (int efA = 0; efA < efficiencyModules.Count; efA++)
			{
				for (int efB = efA; efB < efficiencyModules.Count; efB++) //prevents double counts where A and B are switched (still double counts at A=B, but thats minor enough)
				{
					for (int n = 1; n < moduleSlots; n++)
					{
						for (int x = n + 1; x < moduleSlots; x++)
						{
							for (int i = 0; i < n; i++)
								permutation[i] = efficiencyModules[efA];
							for (int i = n; i < x; i++)
								permutation[i] = efficiencyModules[efB];
							for (int i = x; i < moduleSlots; i++)
								permutation[i] = null;
							permutations.Add(new Permutation(permutation));
						}
					}
				}
			}

			//last efficiency only -> 0 or 1 modules
			//at least 1 slot
			for (int i = 0; i < moduleSlots; i++)
				permutation[i] = null;
			permutations.Add(new Permutation(permutation)); //no modules
			foreach (Module module in efficiencyModules)
			{
				permutation[0] = module;
				permutations.Add(new Permutation(permutation)); //1 module
			}

			//0 slots (empty permutation)
			if (moduleSlots == 0)
				permutations.Add(new Permutation(permutation));

			return permutations;
		}
	}
}
