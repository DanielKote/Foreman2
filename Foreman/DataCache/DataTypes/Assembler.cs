using System;
using System.Collections.Generic;
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

		internal HashSet<RecipePrototype> recipes { get; private set; }

		public AssemblerPrototype(DataCache dCache, string name, string friendlyName, EntityType type, EnergySource source, bool isMissing = false) : base(dCache, name, friendlyName, type, source, isMissing)
		{
			recipes = new HashSet<RecipePrototype>();
		}

		public override string ToString()
		{
			return String.Format("Assembler: {0}", Name);
		}
	}
}
