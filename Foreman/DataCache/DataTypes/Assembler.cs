using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	public interface Assembler : DataObjectBase
	{
		IReadOnlyCollection<Recipe> ValidRecipes { get; }
		IReadOnlyCollection<Module> ValidModules { get; }
		Item AssociatedItem { get; }

		bool IsMiner { get; }
		bool Enabled { get; set; }
		float Speed { get; }

		float BaseProductivityBonus { get; }
		int ModuleSlots { get; }

		bool IsBurner { get; }
		float EnergyConsumption { get; }
		float EnergyEffectivity { get; }
		IReadOnlyCollection<Item> ValidFuels { get; }
	}

	public class AssemblerPrototype : DataObjectBasePrototype, Assembler
	{
		public IReadOnlyCollection<Recipe> ValidRecipes { get { return validRecipes; } }
		public IReadOnlyCollection<Module> ValidModules { get { return validModules; } }
		public Item AssociatedItem { get { return myCache.Items[Name]; } }

		public bool IsMiner { get; private set; }
		public bool Enabled { get; set; }
		public float Speed { get; set; }

		public float BaseProductivityBonus { get; set; }
		public int ModuleSlots { get; set; }

		public bool IsBurner { get; set; }
		public float EnergyConsumption { get; set; }
		public float EnergyEffectivity { get; set; }
		public IReadOnlyCollection<Item> ValidFuels { get { return validFuels; } }

		internal HashSet<RecipePrototype> validRecipes { get; private set; }
		internal HashSet<ModulePrototype> validModules { get; private set; }
		internal HashSet<ItemPrototype> validFuels { get; private set; }

		public AssemblerPrototype(DataCache dCache, string name, string friendlyName, bool isMiner) : base(dCache, name, friendlyName, "-")
		{
			IsMiner = isMiner;
			Enabled = true;

			validRecipes = new HashSet<RecipePrototype>();
			validModules = new HashSet<ModulePrototype>();
			validFuels = new HashSet<ItemPrototype>();
		}

		public override string ToString()
		{
			return String.Format("Assembler: {0}", Name);
		}

		public float GetRate(Recipe recipe, float beaconBonus, IEnumerable<Module> modules = null)
		{
			double finalSpeed = this.Speed;
			if (modules != null)
				foreach (Module module in modules.Where(m => m != null))
					finalSpeed += module.SpeedBonus * this.Speed;
            finalSpeed += beaconBonus * this.Speed;

			double craftingTime = recipe.Time / finalSpeed;
			craftingTime = (float)(Math.Ceiling(craftingTime * 60d) / 60d); //Machines have to wait for a new tick before starting a new item, so round up to the nearest tick

			return (float)(1d / craftingTime);
		}
	}
}
