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
		IReadOnlyDictionary<Item, double> SciPackSet { get; }
		IReadOnlyList<Item> SciPackList { get; }
		double ResearchCost { get; }
		int Tier { get; } //furthest distance from this tech to the starting tech. nice way or ordering technologies
	}

	public class TechnologyPrototype : DataObjectBasePrototype, Technology
	{
		public IReadOnlyCollection<Technology> Prerequisites { get { return prerequisites; } }
		public IReadOnlyCollection<Technology> PostTechs { get { return postTechs; } }
		public IReadOnlyCollection<Recipe> UnlockedRecipes { get { return unlockedRecipes; } }
		public IReadOnlyDictionary<Item, double> SciPackSet { get { return sciPackSet; } }
		public IReadOnlyList<Item> SciPackList { get { return sciPackList; } }
		public double ResearchCost { get; set; }
		public int Tier { get; set; }

		internal HashSet<TechnologyPrototype> prerequisites { get; private set; }
		internal HashSet<TechnologyPrototype> postTechs { get; private set; }
		internal HashSet<RecipePrototype> unlockedRecipes { get; private set; }
		internal Dictionary<Item, double> sciPackSet { get; private set; }
		internal List<Item> sciPackList { get; private set; }


		public TechnologyPrototype(DataCache dCache, string name, string friendlyName) : base(dCache, name, friendlyName, "-")
		{
			prerequisites = new HashSet<TechnologyPrototype>();
			postTechs = new HashSet<TechnologyPrototype>();
			unlockedRecipes = new HashSet<RecipePrototype>();
			sciPackSet = new Dictionary<Item, double>();
			sciPackList = new List<Item>();
			ResearchCost = 0;
		}

		public void InternalOneWayAddSciPack(ItemPrototype pack, double quantity)
		{
			if (sciPackSet.ContainsKey(pack))
				sciPackSet[pack] += quantity;
			else
			{
				sciPackSet.Add(pack, quantity);
				sciPackList.Add(pack);
			}
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
