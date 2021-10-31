using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	public interface Assembler : ProductionEntity
	{
		IReadOnlyCollection<Recipe> ValidRecipes { get; }
		Item AssociatedItem { get; }
	}

	public class AssemblerPrototype : ProductionEntityPrototype, Assembler
	{
		public IReadOnlyCollection<Recipe> ValidRecipes { get { return validRecipes; } }
		public Item AssociatedItem { get { return myCache.Items[Name]; } }

		internal HashSet<RecipePrototype> validRecipes { get; private set; }

		public AssemblerPrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName)
		{
			Enabled = true;
			validRecipes = new HashSet<RecipePrototype>();
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
