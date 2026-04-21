using System.Collections.Generic;

namespace Foreman {
    public interface Group : DataObjectBase {
        IReadOnlyList<Subgroup> Subgroups { get; }
    }

    public interface Subgroup : DataObjectBase {
        Group MyGroup { get; }
        IReadOnlyList<Recipe> Recipes { get; }
        IReadOnlyList<Item> Items { get; }
    }


    public class GroupPrototype(DataCache dCache, string name, string lname, string order) :
        DataObjectBasePrototype(dCache, name, lname, order), Group {
        public IReadOnlyList<Subgroup> Subgroups => subgroups;

        internal List<SubgroupPrototype> subgroups = [];

        // sort them by their order string
        public void SortSubgroups() {
            subgroups.Sort();
        }

        public override string ToString() {
            return $"Group: {Name}";
        }
    }

    public class SubgroupPrototype(DataCache dCache, string name, string order) :
        DataObjectBasePrototype(dCache, name, name, order), Subgroup {
        public Group MyGroup => myGroup;

        public IReadOnlyList<Recipe> Recipes => recipes;

        public IReadOnlyList<Item> Items => items;

        internal GroupPrototype myGroup;

        internal List<RecipePrototype> recipes = [];
        internal List<ItemPrototype> items = [];

        // sort them by their order string
        public void SortIRs() {
            recipes.Sort();
            items.Sort();
        }

        public override string ToString() {
            return $"Subgroup: {Name}";
        }
    }
}