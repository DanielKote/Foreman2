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

			switch(SelectionStyle)
			{
				case Style.Speed:
					bestModule = assembler.ValidModules.Intersect(recipe.ValidModules).Where(m => m.Enabled).OrderBy(m => ((m.SpeedBonus * 1000) - m.ConsumptionBonus)).LastOrDefault();
					break;
				case Style.Productivity:
					bestModule = assembler.ValidModules.Intersect(recipe.ValidModules).Where(m => m.Enabled).OrderBy(m => ((m.ProductivityBonus * 1000) + m.SpeedBonus)).LastOrDefault();
					break;
				case Style.ProductivityOnly:
					bestModule = assembler.ValidModules.Intersect(recipe.ValidModules).Where(m => m.Enabled && m.ProductivityBonus != 0).OrderBy(m => ((m.ProductivityBonus * 1000) + m.SpeedBonus)).LastOrDefault();
					break;
				case Style.Efficiency:
					List<Module> speedModules = assembler.ValidModules.Intersect(recipe.ValidModules).Where(m => m.Enabled && m.SpeedBonus > 0).OrderBy(m => ((m.SpeedBonus * 1000) - m.ConsumptionBonus)).ToList();
					List<Module> efficiencyModules = assembler.ValidModules.Intersect(recipe.ValidModules).Where(m => m.Enabled && m.ConsumptionBonus < 0).OrderBy(m => -((m.ConsumptionBonus * 1000) - m.SpeedBonus)).ToList(); //highest consumption is first! (so worst->best effectivity)

					for(int si = assembler.ModuleSlots - 1; si >= 0; si--)
					{
						foreach(Module speedModule in speedModules)
						{
							foreach(Module efficiencyModule in efficiencyModules)
							{
								if((speedModule.ConsumptionBonus * si) + (efficiencyModule.ConsumptionBonus * (assembler.ModuleSlots - si)) <= -0.8f)
								{
									for (int i = 0; i < si; i++)
										moduleList.Add(speedModule);
									for (int i = 0; i < (assembler.ModuleSlots - si); i++)
										moduleList.Add(efficiencyModule);
									return moduleList;
								}	
							}
						}
					}
					//if we are here, then no module set reaches the -80% efficiency bonus. set the bestModule to the one with lowest consumption bonus and continue.
					bestModule = efficiencyModules.FirstOrDefault();
					break;
				case Style.EfficiencyOnly:
					//slightly harder -> want to add efficiency modules up to the 80% limit
					List<Module> moduleOptions = assembler.ValidModules.Intersect(recipe.ValidModules).Where(m => m.Enabled).OrderBy(m => ((m.ConsumptionBonus * 1000) - m.SpeedBonus)).ToList(); //note: lowest consumption is first!
					foreach(Module moduleOption in moduleOptions)
					{
						if (moduleOption.ConsumptionBonus * assembler.ModuleSlots <= -0.8f) //test for the 80% maximum efficiency gain limit (and use 'worst' module that gets it)
							bestModule = moduleOption;
						else
							break;
					}
					if (bestModule == null) //havent found a limit efficiency -> just use the most efficient one from the options list
						bestModule = moduleOptions.LastOrDefault();

					break;
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
}
