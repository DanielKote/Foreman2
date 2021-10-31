using System;
using System.Collections.Generic;

namespace Foreman
{
    public class Group : DataObjectBase
    {
        public IReadOnlyList<Subgroup> Subgroups { get { return subgroups; } }

        private List<Subgroup> subgroups;

        public Group(DataCache dCache, string name, string lname, string order) : base(dCache, name, lname, order)
        {
            subgroups = new List<Subgroup>();
        }

        public void SortSubgroups() { subgroups.Sort(); } //sort them by their order string

        internal void InternalOneWayAddSubgroup(Subgroup sgroup)
        {
            subgroups.Add(sgroup);
        }

        internal void InternalOneWayRemoveSubgroup(Subgroup sgroup)
        {
            subgroups.Remove(sgroup);
        }

    }

    public class Subgroup : DataObjectBase
    {
        public Group MyGroup { get; private set; }

        public IReadOnlyList<Recipe> Recipes { get { return recipes; } }
        public IReadOnlyList<Item> Items { get { return items; } }

        private List<Recipe> recipes;
        private List<Item> items;

        public Subgroup(DataCache dCache, string name, string order) : base(dCache, name, name, order)
        {
            recipes = new List<Recipe>();
            items = new List<Item>();
        }

        internal void SetGroup(Group myGroup)
        {
            MyGroup = myGroup;
            MyGroup.InternalOneWayAddSubgroup(this);
        }

        public void SortIRs() { recipes.Sort(); items.Sort(); } //sort them by their order string

        internal void InternalOneWayAddRecipe(Recipe recipe)
        {
            recipes.Add(recipe);
        }
        internal void InternalOneWayAddItem(Item item)
        {
            items.Add(item);
        }
        internal void InternalOneWayRemoveRecipe(Recipe recipe)
        {
            recipes.Remove(recipe);
        }
        internal void InternalOneWayRemoveItem(Item item)
        {
            items.Remove(item);
        }
    }
}
