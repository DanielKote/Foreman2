using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Foreman
{
	public interface Module : DataObjectBase
	{
		IReadOnlyCollection<Recipe> Recipes { get; }
		IReadOnlyCollection<Assembler> Assemblers { get; }
		IReadOnlyCollection<Beacon> Beacons { get; }
		IReadOnlyCollection<Recipe> AvailableRecipes { get; }

		Item AssociatedItem { get; }

		float SpeedBonus { get; }
		float ProductivityBonus { get; }
		float ConsumptionBonus { get; }
		float PollutionBonus { get; }

		int Tier { get; }

		bool IsMissing { get; }
	}

	public class ModulePrototype : DataObjectBasePrototype, Module
	{
		public IReadOnlyCollection<Recipe> Recipes { get { return recipes; } }
		public IReadOnlyCollection<Assembler> Assemblers { get { return assemblers; } }
		public IReadOnlyCollection<Beacon> Beacons { get { return beacons; } }
		public IReadOnlyCollection<Recipe> AvailableRecipes { get; private set; }
		public Item AssociatedItem { get { return myCache.Items[Name]; } }

		public float SpeedBonus { get; set; }
		public float ProductivityBonus { get; set; }
		public float ConsumptionBonus { get; set; }
		public float PollutionBonus { get; set; }

		public int Tier { get; set; }

		public bool IsMissing { get; private set; }
		public override bool Available { get { return AssociatedItem.Available; } set { } }

		internal HashSet<RecipePrototype> recipes { get; private set; }
		internal HashSet<AssemblerPrototype> assemblers { get; private set; }
		internal HashSet<BeaconPrototype> beacons { get; private set; }

		public ModulePrototype(DataCache dCache, string name, string friendlyName, bool isMissing = false) : base(dCache, name, friendlyName, "-")
		{
			Enabled = true;
			IsMissing = isMissing;

			SpeedBonus = 0;
			ProductivityBonus = 0;
			ConsumptionBonus = 0;
			PollutionBonus = 0;
			recipes = new HashSet<RecipePrototype>();
			assemblers = new HashSet<AssemblerPrototype>();
			beacons = new HashSet<BeaconPrototype>();
		}

		internal void UpdateAvailabilities()
		{
			AvailableRecipes = new HashSet<Recipe>(recipes.Where(r => r.Enabled));
		}

		public override string ToString() { return string.Format("Module: {0}", Name); }
	}
}
