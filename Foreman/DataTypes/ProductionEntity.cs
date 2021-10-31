using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	public class MachinePermutation
	{
		public ProductionEntity assembler;
		public List<Module> modules;

		public double GetAssemblerRate(Recipe recipe, float beaconBonus)
		{
			return (assembler as Assembler).GetRate(recipe, beaconBonus, modules);
		}

		internal double GetAssemblerProductivity()
		{
			return modules
				.Where(x => x != null)
				.Sum(x => x.ProductivityBonus);
		}

		public double GetMinerRate(Resource r)
		{
			return (assembler as Miner).GetRate(r, modules);
		}

		public MachinePermutation(ProductionEntity machine, ICollection<Module> modules)
		{
			assembler = machine;
			this.modules = modules.ToList();
		}
	}

	public abstract class ProductionEntity : DataObjectBase
	{
		public bool Enabled { get; set; }
		public int ModuleSlots { get; set; }
		public float Speed { get; set; }

		public ProductionEntity(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-") { }

		public IEnumerable<MachinePermutation> GetAllPermutations(Recipe recipe)
		{
			yield return new MachinePermutation(this, new List<Module>());

			Module[] currentModules = new Module[ModuleSlots];

			if (ModuleSlots <= 0)
			{
				yield break;
			}

			foreach (Module module in recipe.ValidModules.Where(m => m.Enabled))
			{
				for (int i = 0; i < ModuleSlots; i++)
				{
					currentModules[i] = module;
					yield return new MachinePermutation(this, currentModules);
				}
			}
		}
	}
}
