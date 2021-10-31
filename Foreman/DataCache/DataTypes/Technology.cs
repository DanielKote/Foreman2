using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	public interface Technology : DataObjectBase
	{
		IReadOnlyCollection<Technology> Prerequisites { get; }
		IReadOnlyCollection<Technology> PostTechs { get; }
		IReadOnlyCollection<Recipe> UnlockedRecipes { get; }
		IReadOnlyCollection<Recipe> AvailableUnlockedRecipes { get; }
	}

	public class TechnologyPrototype : DataObjectBasePrototype, Technology
	{
		public IReadOnlyCollection<Technology> Prerequisites { get { return prerequisites; } }
		public IReadOnlyCollection<Technology> PostTechs { get { return postTechs; } }
		public IReadOnlyCollection<Recipe> UnlockedRecipes { get { return unlockedRecipes; } }
		public IReadOnlyCollection<Recipe> AvailableUnlockedRecipes { get; private set; }

		internal HashSet<TechnologyPrototype> prerequisites { get; private set; }
		internal HashSet<TechnologyPrototype> postTechs { get; private set; }
		internal HashSet<RecipePrototype> unlockedRecipes { get; private set; }

		public TechnologyPrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-")
		{
			prerequisites = new HashSet<TechnologyPrototype>();
			postTechs = new HashSet<TechnologyPrototype>();
			unlockedRecipes = new HashSet<RecipePrototype>();
		}

		internal void UpdateAvailabilities()
		{
			AvailableUnlockedRecipes = new HashSet<Recipe>(unlockedRecipes.Where(r => r.Enabled));
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TechnologyPrototype))
			{
				return false;
			}
			return this == (TechnologyPrototype)obj;
		}

		public static bool operator ==(TechnologyPrototype item1, TechnologyPrototype item2)
		{
			if (System.Object.ReferenceEquals(item1, item2))
			{
				return true;
			}
			if ((object)item1 == null || (object)item2 == null)
			{
				return false;
			}

			return item1.Name == item2.Name;
		}

		public static bool operator !=(TechnologyPrototype item1, TechnologyPrototype item2)
		{
			return !(item1 == item2);
		}

		public override string ToString()
		{
			return String.Format("Technology: {0}", Name);
		}

	}
}
