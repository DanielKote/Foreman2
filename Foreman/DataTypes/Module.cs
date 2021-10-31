using System.Collections.Generic;
using System.Drawing;

namespace Foreman
{
	public interface Module : DataObjectBase
	{
		IReadOnlyCollection<Recipe> ValidRecipes { get; }
		IReadOnlyCollection<Assembler> ValidAssemblers { get; }
		 IReadOnlyCollection<Miner> ValidMiners { get; }
		IReadOnlyCollection<Beacon> ValidBeacons { get; }
		Item AssociatedItem { get; }

		float SpeedBonus { get; }
		float ProductivityBonus { get; }
		float EfficiencyBonus { get; }
		float PollutionBonus { get; }
		bool Enabled { get; set; }
	}

	public class ModulePrototype : DataObjectBasePrototype, Module
	{
		public IReadOnlyCollection<Recipe> ValidRecipes { get { return validRecipes; } }
		public IReadOnlyCollection<Assembler> ValidAssemblers { get { return validAssemblers; } }
		public IReadOnlyCollection<Miner> ValidMiners { get { return validMiners; } }
		public IReadOnlyCollection<Beacon> ValidBeacons { get { return validBeacons; } }
		public Item AssociatedItem { get { return myCache.Items[Name]; } }

		public bool Enabled { get; set; }
		public float SpeedBonus { get; set; }
		public float ProductivityBonus { get; set; }
		public float EfficiencyBonus { get; set; }
		public float PollutionBonus { get; set; }

		internal HashSet<RecipePrototype> validRecipes { get; private set; }
		internal HashSet<AssemblerPrototype> validAssemblers { get; private set; }
		internal HashSet<MinerPrototype> validMiners { get; private set; }
		internal HashSet<BeaconPrototype> validBeacons { get; private set; }

		public ModulePrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-")
		{
			Enabled = true;
			SpeedBonus = 0;
			ProductivityBonus = 0;
			EfficiencyBonus = 0;
			PollutionBonus = 0;
			validRecipes = new HashSet<RecipePrototype>();
			validAssemblers = new HashSet<AssemblerPrototype>();
			validMiners = new HashSet<MinerPrototype>();
			validBeacons = new HashSet<BeaconPrototype>();
		}
	}
}
