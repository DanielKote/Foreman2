using System;
using System.Collections.Generic;

namespace Foreman
{
    public class Group : DataObjectBase
    {
        public List<Subgroup> Subgroups { get; private set; }

        public Group(string name, string lname, string order) : base(name, lname, order)
        {
            Subgroups = new List<Subgroup>();
        }

        public void SortSubgroups() { Subgroups.Sort(); } //sort them by their order string

    }

    public class Subgroup : DataObjectBase
    {
        public Group MyGroup { get; protected set; }

        public List<Recipe> Recipes { get; private set; }
        public List<Item> Items { get; private set; }

        public Subgroup(string name, Group myGroup, string order) : base(name, name, order)
        {
            MyGroup = myGroup;
            MyGroup.Subgroups.Add(this);
            Recipes = new List<Recipe>();
            Items = new List<Item>();
        }

        public void SortIRs() { Recipes.Sort(); Items.Sort(); } //sort them by their order string
    }
}
