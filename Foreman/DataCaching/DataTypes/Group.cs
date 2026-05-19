using System.Collections.Generic;

namespace Foreman.DataCaching.DataTypes {
    public interface IGroup : IDataObjectBase {
        IReadOnlyList<ISubgroup> Subgroups { get; }
    }

    public interface ISubgroup : IDataObjectBase {
        IGroup? MyGroup { get; }
        IReadOnlyList<IRecipe> Recipes { get; }
        IReadOnlyList<IItem> Items { get; }
    }


    public class GroupPrototype(DataCache dCache, string name, string lname, string order) : DataObjectBasePrototype(dCache, name, lname, order), IGroup {
        public IReadOnlyList<ISubgroup> Subgroups { get { return _subgroups; } }

        private readonly List<SubgroupPrototype> _subgroups = [];

        internal List<SubgroupPrototype> SubgroupsInternal => _subgroups;

        public void SortSubgroups() { _subgroups.Sort(); } //sort them by their order string

        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "Group: {0}", Name); }
    }

    public class SubgroupPrototype(DataCache dCache, string name, string order) : DataObjectBasePrototype(dCache, name, name, order), ISubgroup {
        public IGroup? MyGroup => _myGroup;

        public IReadOnlyList<IRecipe> Recipes { get { return _recipes; } }
        public IReadOnlyList<IItem> Items { get { return _items; } }

        private GroupPrototype? _myGroup;

        internal GroupPrototype? MyGroupInternal { get => _myGroup; set => _myGroup = value; }

        private readonly List<RecipePrototype> _recipes = [];
        internal List<RecipePrototype> RecipesInternal => _recipes;

        private readonly List<ItemPrototype> _items = [];
        internal List<ItemPrototype> ItemsInternal => _items;

        public void SortIRs() { _recipes.Sort(); _items.Sort(); } //sort them by their order string

        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "Subgroup: {0}", Name); }
    }
}
