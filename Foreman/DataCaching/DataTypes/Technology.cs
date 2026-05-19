using System.Collections.Generic;

namespace Foreman.DataCaching.DataTypes {
    public interface ITechnology : IDataObjectBase {
        IReadOnlyCollection<ITechnology> Prerequisites { get; }
        IReadOnlyCollection<ITechnology> PostTechs { get; }
        IReadOnlyCollection<IRecipe> UnlockedRecipes { get; }
        IReadOnlyCollection<IQuality> UnlockedQualities { get; }
        IReadOnlyDictionary<IItem, double> SciPackSet { get; }
        IReadOnlyList<IItem> SciPackList { get; }
        double ResearchCost { get; }
        int Tier { get; } //furthest distance from this tech to the starting tech. nice way or ordering technologies
    }

    public class TechnologyPrototype(DataCache dCache, string name, string friendlyName) : DataObjectBasePrototype(dCache, name, friendlyName, "-"), ITechnology {
        public IReadOnlyCollection<ITechnology> Prerequisites { get { return _prerequisites; } }
        public IReadOnlyCollection<ITechnology> PostTechs { get { return _postTechs; } }
        public IReadOnlyCollection<IRecipe> UnlockedRecipes { get { return _unlockedRecipes; } }
        public IReadOnlyCollection<IQuality> UnlockedQualities { get { return _unlockedQualities; } }
        public IReadOnlyDictionary<IItem, double> SciPackSet { get { return _sciPackSet; } }
        public IReadOnlyList<IItem> SciPackList { get { return _sciPackList; } }
        public double ResearchCost { get; set; }
        public int Tier { get; set; }

        private readonly HashSet<TechnologyPrototype> _prerequisites = [];
        internal HashSet<TechnologyPrototype> PrerequisitesInternal => _prerequisites;

        private readonly HashSet<TechnologyPrototype> _postTechs = [];
        internal HashSet<TechnologyPrototype> PostTechsInternal => _postTechs;

        private readonly HashSet<RecipePrototype> _unlockedRecipes = [];
        internal HashSet<RecipePrototype> UnlockedRecipesInternal => _unlockedRecipes;

        private readonly HashSet<QualityPrototype> _unlockedQualities = [];
        internal HashSet<QualityPrototype> UnlockedQualitiesInternal => _unlockedQualities;

        private readonly Dictionary<IItem, double> _sciPackSet = [];
        internal Dictionary<IItem, double> SciPackSetInternal => _sciPackSet;

        private readonly List<IItem> _sciPackList = [];
        internal List<IItem> SciPackListInternal => _sciPackList;

        public void InternalOneWayAddSciPack(ItemPrototype pack, double quantity) {
            if (_sciPackSet.TryGetValue(pack, out double amount))
                _sciPackSet[pack] = amount + quantity;
            else {
                _sciPackSet.Add(pack, quantity);
                _sciPackList.Add(pack);
            }
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public override bool Equals(object? obj) {
            return obj is TechnologyPrototype tp && this == tp;
        }

        public static bool operator ==(TechnologyPrototype? item1, TechnologyPrototype? item2) {
            return ReferenceEquals(item1, item2) || item1?.Name == item2?.Name;
        }

        public static bool operator !=(TechnologyPrototype? item1, TechnologyPrototype? item2) {
            return !(item1 == item2);
        }

        public override string ToString() {
            return string.Format(CultureInfo.InvariantCulture, "Technology: {0}", Name);
        }

    }
}
