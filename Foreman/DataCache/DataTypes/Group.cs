using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	public interface Group : DataObjectBase
	{
		IReadOnlyList<Subgroup> Subgroups { get; }
	}

	public interface Subgroup : DataObjectBase
	{
		Group MyGroup { get; }
		IReadOnlyList<Recipe> Recipes { get; }
		IReadOnlyList<Item> Items { get; }
	}


	public class GroupPrototype : DataObjectBasePrototype, Group
	{
		public IReadOnlyList<Subgroup> Subgroups { get { return subgroups; } }

		internal List<SubgroupPrototype> subgroups;

		public GroupPrototype(DataCache dCache, string name, string lname, string order) : base(dCache, name, lname, order)
		{
			subgroups = new List<SubgroupPrototype>();
		}

		public void SortSubgroups() { subgroups.Sort(); } //sort them by their order string

		public override string ToString() { return String.Format("Group: {0}", Name); }
	}

	public class SubgroupPrototype : DataObjectBasePrototype, Subgroup
	{
		public Group MyGroup { get { return myGroup; } }

		public IReadOnlyList<Recipe> Recipes { get { return recipes; } }
		public IReadOnlyList<Item> Items { get { return items; } }

		internal GroupPrototype myGroup;

		internal List<RecipePrototype> recipes;
		internal List<ItemPrototype> items;

		public SubgroupPrototype(DataCache dCache, string name, string order) : base(dCache, name, name, order)
		{
			recipes = new List<RecipePrototype>();
			items = new List<ItemPrototype>();
		}

		public void SortIRs() { recipes.Sort(); items.Sort(); } //sort them by their order string

		public override string ToString() { return String.Format("Subgroup: {0}", Name); }
	}
}
