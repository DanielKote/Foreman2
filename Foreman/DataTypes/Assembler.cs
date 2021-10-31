using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Foreman
{
	public class Assembler : ProductionEntity
	{
		public IReadOnlyCollection<Recipe> ValidRecipes { get { return validRecipes; } }
		public IReadOnlyCollection<Module> ValidModules { get { return validModules; } }
		public Item AssociatedItem { get { return myCache.Items[Name]; } }

		private HashSet<Recipe> validRecipes;
		private HashSet<Module> validModules;

		public Assembler(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName)
		{
			Enabled = true;
			validRecipes = new HashSet<Recipe>();
			validModules = new HashSet<Module>();
		}

		public void AddValidModule(Module module)
        {
			validModules.Add(module);
			module.InternalOneWayAddAssembler(this);
        }

		internal void InternalOneWayRemoveValidModule(Module module) //only from delete calls
        {
			validModules.Remove(module);
        }

		internal void InternalOneWayAddRecipe(Recipe recipe) //should only be called from Recipe (when it adds an assembler)
        {
			validRecipes.Add(recipe);
        }

		internal void InternalOneWayRemoveRecipe(Recipe recipe) //only from delete calls
        {
			validRecipes.Remove(recipe);
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

	public class Module : DataObjectBase
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

		private HashSet<Recipe> validRecipes;
		private HashSet<Assembler> validAssemblers;

		public Module(DataCache dCache, string name, string friendlyName, float speedBonus, float productivityBonus) : base(dCache, name, friendlyName, "-")
		{
			Enabled = true;
			SpeedBonus = speedBonus;
			ProductivityBonus = productivityBonus;
			validRecipes = new HashSet<Recipe>();
			validAssemblers = new HashSet<Assembler>();
		}

		internal void InternalOneWayAddRecipe(Recipe recipe) //should only be called from Recipe (when it adds a valid module)
        {
			validRecipes.Add(recipe);
        }

		internal void InternalOneWayRemoveRecipe(Recipe recipe) //only from delete calls
		{
			validRecipes.Remove(recipe);
        }

		internal void InternalOneWayAddAssembler(Assembler assembler) //should only be called from Assembler (when it adds a valid module)
        {
			validAssemblers.Add(assembler);
        }

		internal void InternalOneWayRemoveAssembler(Assembler assembler) //only from delete calls
		{
			validAssemblers.Remove(assembler);
        }
	}
}
