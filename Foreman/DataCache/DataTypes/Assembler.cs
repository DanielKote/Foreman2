using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman
{
	public interface Assembler : DataObjectBase
	{
		IReadOnlyCollection<Recipe> Recipes { get; }
		IReadOnlyCollection<Module> Modules { get; }
		IReadOnlyCollection<Item> Fuels { get; }
		IReadOnlyCollection<Recipe> AvailableRecipes { get; }
		IReadOnlyCollection<Item> AvailableFuels { get; }
		IReadOnlyCollection<Item> AssociatedItems { get; }

		bool IsMiner { get; }
		bool IsMissing { get; }

		float Speed { get; }

		float BaseProductivityBonus { get; }
		int ModuleSlots { get; }

		bool IsBurner { get; }
		float EnergyConsumption { get; }
		float EnergyEffectivity { get; }
	}

	public class AssemblerPrototype : DataObjectBasePrototype, Assembler
	{
		public IReadOnlyCollection<Recipe> Recipes { get { return recipes; } }
		public IReadOnlyCollection<Module> Modules { get { return modules; } }
		public IReadOnlyCollection<Item> Fuels { get { return fuels; } }
		public IReadOnlyCollection<Recipe> AvailableRecipes { get; private set; }
		public IReadOnlyCollection<Item> AvailableFuels { get; private set; }
		public IReadOnlyCollection<Item> AssociatedItems { get { return associatedItems; } }

		public bool IsMiner { get; private set; }

		public bool IsMissing { get; private set; }
		public override bool Available { get { return associatedItems.FirstOrDefault(i => i.Available) != null; } set { } }

		public float Speed { get; set; }

		public float BaseProductivityBonus { get; set; }
		public int ModuleSlots { get; set; }

		public bool IsBurner { get; set; }
		public float EnergyConsumption { get; set; }
		public float EnergyEffectivity { get; set; }

		internal HashSet<RecipePrototype> recipes { get; private set; }
		internal HashSet<ModulePrototype> modules { get; private set; }
		internal HashSet<ItemPrototype> fuels { get; private set; }
		internal List<ItemPrototype> associatedItems { get; private set; } //should honestly only be 1, but knowing modders....

		public AssemblerPrototype(DataCache dCache, string name, string friendlyName, bool isMiner, bool isMissing = false) : base(dCache, name, friendlyName, "-")
		{
			IsMiner = isMiner;
			IsMissing = isMissing;

			//just some base defaults -> helps prevent overflow errors during solving if the assembler is a missing entity
			Speed = 1f;
			BaseProductivityBonus = 0;
			ModuleSlots = 0;
			IsBurner = false;
			EnergyConsumption = 1000;
			EnergyEffectivity = 1;

			recipes = new HashSet<RecipePrototype>();
			modules = new HashSet<ModulePrototype>();
			fuels = new HashSet<ItemPrototype>();
			associatedItems = new List<ItemPrototype>();
		}

		internal void UpdateAvailabilities()
		{
			AvailableRecipes = new HashSet<Recipe>(recipes.Where(r => r.Available));
			AvailableFuels = new HashSet<Item>(fuels.Where(i => i.Available));
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
