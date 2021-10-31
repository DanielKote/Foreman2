using System;
using System.Collections.Generic;

namespace Foreman
{
    public class Group : DataObjectBase
    {
        [ObsoleteAttribute("nested groups not supported", true)]
        public new Group MyGroup { get { throw new Exception("nested groups not supported"); } }
        [ObsoleteAttribute("groups do not belong to subgroups", true)]
        public new Subgroup MySubgroup { get { throw new Exception("groups do not belong to subgroups"); } }

        public List<Subgroup> Subgroups { get; private set; }

        public Group(string name, string lname, string order) : base(name, lname, null, order)
        {
            Subgroups = new List<Subgroup>();
        }

        public void SortSubgroups() { Subgroups.Sort(); } //sort them by their order string

    }

    public class Subgroup : DataObjectBase
    {
        [ObsoleteAttribute("nested subgroups not supported", true)]
        private new Subgroup MySubgroup { get { throw new Exception("nested subgroups not supported"); } }
        [ObsoleteAttribute("subgroups not have friendly names", true)]
        private new string LFriendlyName { get { return ""; } }
        [ObsoleteAttribute("subgroups not have friendly names", true)]
        private new string FriendlyName { get { return ""; } }

        public new Group MyGroup { get; private set; } //hides the reference approach of base

        public List<Recipe> Recipes { get; private set; }
        public List<Item> Items { get; private set; }

        public Subgroup(string name, Group myGroup, string order) : base(name, "", null, order)
        {
            MyGroup = myGroup;
            MyGroup.Subgroups.Add(this);
            Recipes = new List<Recipe>();
            Items = new List<Item>();
        }

        public void SortIRs() { Recipes.Sort(); Items.Sort(); } //sort them by their order string
    }
}
