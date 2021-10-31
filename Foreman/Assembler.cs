using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	public class MachinePermutation
	{
		public ProductionEntity assembler;
		public List<Module> modules;

		public double GetAssemblerRate(float recipeTime, float beaconBonus)
		{
			return (assembler as Assembler).GetRate(recipeTime, beaconBonus, modules);
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

	public abstract class ProductionEntity
	{
		public String Name { get; protected set; }
		public string LName;
		public bool Enabled { get; set; }
		public Bitmap Icon { get; set; }
		public int ModuleSlots { get; set; }
		public float Speed { get; set; }
		private String friendlyName;
		public String FriendlyName
		{
			get
			{
				if (!String.IsNullOrEmpty(LName))
					return LName;
				if (!String.IsNullOrWhiteSpace(friendlyName))
				{
					return friendlyName;
				}
				else
				{
					return Name;
				}
			}
			set
			{
				friendlyName = value;
			}
		}
		
		public IEnumerable<MachinePermutation> GetAllPermutations(Recipe recipe)
		{
			yield return new MachinePermutation(this, new List<Module>());

			Module[] currentModules = new Module[ModuleSlots];

			if (ModuleSlots <= 0)
			{
				yield break;
			}

            var allowedModules = DataCache.Modules.Values
                .Where(m => m.Enabled)
                .Where(m => m.AllowedIn(recipe));

			foreach (Module module in allowedModules)
			{
				for (int i = 0; i < ModuleSlots; i++)
				{
					currentModules[i] = module;
					yield return new MachinePermutation(this, currentModules);
				}
			}
		}
	}

	public class Assembler : ProductionEntity
	{
		public List<String> Categories { get; private set; }
		public int MaxIngredients { get; set; }
		public List<string> AllowedEffects { get; private set; }

		public Assembler(String name)
		{
			Enabled = true;
			Name = name;
			Categories = new List<string>();
			AllowedEffects = new List<string>();
		}

		public override string ToString()
		{
			return String.Format("Assembler: {0}", Name);
		}

		public float GetRate(float recipeTime, float beaconBonus, IEnumerable<Module> speedModules = null)
		{
			double finalSpeed = this.Speed;
			if (speedModules != null)
			{
				foreach (Module module in speedModules.Where(m => m != null))
				{
					finalSpeed += module.SpeedBonus * this.Speed;
				}
			}
            finalSpeed += beaconBonus * this.Speed;

			double craftingTime = recipeTime / finalSpeed;
			craftingTime = (float)(Math.Ceiling(craftingTime * 60d) / 60d); //Machines have to wait for a new tick before starting a new item, so round up to the nearest tick

			return (float)(1d / craftingTime);
		}
	}

	public class Module
	{
		public Bitmap Icon
		{
			get
			{
				//For each module there should be a corresponding item with the icon already loaded.
				return DataCache.Items[Name].Icon;
			}
		}
		public bool Enabled { get; set; }
		public float SpeedBonus { get; private set; }
        public float ProductivityBonus { get; private set; }
		public string Name { get; private set; }
		public string LName;
		private String friendlyName;
        private List<string> allowedIn;

        public String FriendlyName
		{
			get
			{
				if (!String.IsNullOrEmpty(LName))
					return LName;
				if (!String.IsNullOrWhiteSpace(friendlyName))
				{
					return friendlyName;
				}
				else
				{
					return Name;
				}
			}
			set
			{
				friendlyName = value;
			}
		}

        public Module(String name, float speedBonus, float productivityBonus, List<string> allowedIn)
		{
			Name = name;
			Enabled = true;
			this.SpeedBonus = speedBonus;
			this.ProductivityBonus = productivityBonus;
			this.allowedIn = allowedIn;
		}

        public bool AllowedIn(Recipe recipe)
        {
            if (allowedIn == null || recipe == null) // TODO: Remove recipe == null case, it's just there as scaffolding.
                return true;

            return allowedIn.Contains(recipe.Name);
        }
	}
}
