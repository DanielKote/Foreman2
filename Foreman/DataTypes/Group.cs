using System;
using System.Collections.Generic;

namespace Foreman
{
    public class Group : DataObjectBase
    {
        public List<Subgroup> Subgroups { get; private set; }

        public Group(DataCache dCache, string name, string lname, string order) : base(dCache, name, lname, order)
        {
            Subgroups = new List<Subgroup>();
        }

        public void SortSubgroups() { Subgroups.Sort(); } //sort them by their order string

    }

    public class Subgroup : DataObjectBase
    {
        public Group MyGroup { get; private set; }

        public List<Recipe> Recipes { get; private set; }
        public List<Item> Items { get; private set; }

        public Subgroup(DataCache dCache, string name, string order) : base(dCache, name, name, order)
        {
            Recipes = new List<Recipe>();
            Items = new List<Item>();
        }

        internal void SetGroup(Group myGroup)
        {
            MyGroup = myGroup;
            MyGroup.Subgroups.Add(this);
        }

        public void SortIRs() { Recipes.Sort(); Items.Sort(); } //sort them by their order string
    }
}
