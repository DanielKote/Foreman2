using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Foreman
{
	public interface Assembler : ProductionEntity
	{
		IReadOnlyCollection<Recipe> ValidRecipes { get; }
		IReadOnlyCollection<Module> ValidModules { get; }
		Item AssociatedItem { get; }
	}

	public interface Module : DataObjectBase
	{
		IReadOnlyCollection<Recipe> ValidRecipes { get; }
		IReadOnlyCollection<Assembler> ValidAssemblers { get; }
		Item AssociatedItem { get; }

		float SpeedBonus { get; }
		float ProductivityBonus { get; }
		bool Enabled { get; set; }
	}


	public class AssemblerPrototype : ProductionEntityPrototype, Assembler
	{
		public IReadOnlyCollection<Recipe> ValidRecipes { get { return validRecipes; } }
		public IReadOnlyCollection<Module> ValidModules { get { return validModules; } }
		public Item AssociatedItem { get { return myCache.Items[Name]; } }

		internal HashSet<RecipePrototype> validRecipes;
		internal HashSet<ModulePrototype> validModules;

		public AssemblerPrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName)
		{
			Enabled = true;
			validRecipes = new HashSet<RecipePrototype>();
			validModules = new HashSet<ModulePrototype>();
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

	public class ModulePrototype : DataObjectBasePrototype, Module
	{
		public override Bitmap Icon { get { return myCache.Items[Name].Icon; } }
        public override Color AverageColor { get { return myCache.Items[Name].AverageColor; } }
        public override string FriendlyName { get { return myCache.Items[Name].FriendlyName; } }
        public override string LFriendlyName { get { return myCache.Items[Name].LFriendlyName; } }

		public IReadOnlyCollection<Recipe> ValidRecipes { get { return validRecipes; } }
		public IReadOnlyCollection<Assembler> ValidAssemblers { get { return validAssemblers; } }
		public Item AssociatedItem { get { return myCache.Items[Name]; } }

        public bool Enabled { get; set; }
		public float SpeedBonus { get; private set; }
        public float ProductivityBonus { get; private set; }

		internal HashSet<RecipePrototype> validRecipes { get; private set; }
		internal HashSet<AssemblerPrototype> validAssemblers { get; private set; }

		public ModulePrototype(DataCache dCache, string name, string friendlyName, float speedBonus, float productivityBonus) : base(dCache, name, friendlyName, "-")
		{
			Enabled = true;
			SpeedBonus = speedBonus;
			ProductivityBonus = productivityBonus;
			validRecipes = new HashSet<RecipePrototype>();
			validAssemblers = new HashSet<AssemblerPrototype>();
		}
	}
}
