using System.Collections.Generic;
using System.Drawing;

namespace Foreman
{
	public interface Module : DataObjectBase
	{
		IReadOnlyCollection<Recipe> ValidRecipes { get; }
		IReadOnlyCollection<Assembler> ValidAssemblers { get; }
		Item AssociatedItem { get; }

		float SpeedBonus { get; }
		float ProductivityBonus { get; }
		float EfficiencyBonus { get; }
		float PollutionBonus { get; }
		bool Enabled { get; set; }
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
		public float SpeedBonus { get; set; }
		public float ProductivityBonus { get; set; }
		public float EfficiencyBonus { get; set; }
		public float PollutionBonus { get; set; }

		internal HashSet<RecipePrototype> validRecipes { get; private set; }
		internal HashSet<AssemblerPrototype> validAssemblers { get; private set; }

		public ModulePrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-")
		{
			Enabled = true;
			SpeedBonus = 0;
			ProductivityBonus = 0;
			EfficiencyBonus = 0;
			PollutionBonus = 0;
			validRecipes = new HashSet<RecipePrototype>();
			validAssemblers = new HashSet<AssemblerPrototype>();
		}
	}
}
