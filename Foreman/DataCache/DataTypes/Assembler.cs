using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Foreman
{
	public interface Assembler : EntityObjectBase
	{
		IReadOnlyCollection<Recipe> Recipes { get; }
		double BaseProductivityBonus { get; }
	}

	internal class AssemblerPrototype : EntityObjectBasePrototype, Assembler
	{
		public IReadOnlyCollection<Recipe> Recipes { get { return recipes; } }
		public double BaseProductivityBonus { get; set; }

		public override bool Available { get { return associatedItems.FirstOrDefault(i => i.Available) != null; } set { } }


		internal HashSet<RecipePrototype> recipes { get; private set; }

		public AssemblerPrototype(DataCache dCache, string name, string friendlyName, EntityType type, EnergySource source, bool isMissing = false) : base(dCache, name, friendlyName, type, source, isMissing)
		{
			recipes = new HashSet<RecipePrototype>();
		}

		public override string ToString()
		{
			return String.Format("Assembler: {0}", Name);
		}

		public double GetRate(Recipe recipe, double beaconBonus, IEnumerable<Module> modules = null)
		{
			double finalSpeed = this.Speed;
			if (modules != null)
				foreach (Module module in modules.Where(m => m != null))
					finalSpeed += module.SpeedBonus * this.Speed;
			finalSpeed += beaconBonus * this.Speed;

			double craftingTime = recipe.Time / finalSpeed;
			craftingTime = (double)(Math.Ceiling(craftingTime * 60d) / 60d); //Machines have to wait for a new tick before starting a new item, so round up to the nearest tick

			return (double)(1d / craftingTime);
		}
	}
}
