using System.Collections.Generic;

namespace Foreman {
    public interface Technology : DataObjectBase {
        IReadOnlyCollection<Technology> Prerequisites { get; }
        IReadOnlyCollection<Technology> PostTechs { get; }
        IReadOnlyCollection<Recipe> UnlockedRecipes { get; }
        IReadOnlyCollection<Recipe> UnlockedQualities { get; }
        IReadOnlyDictionary<Item, double> SciPackSet { get; }
        IReadOnlyList<Item> SciPackList { get; }
        double ResearchCost { get; }
        // furthest distance from this tech to the starting tech. nice way or ordering technologies
        int Tier { get; }
    }

    public class TechnologyPrototype(DataCache dCache, string name, string friendlyName)
        : DataObjectBasePrototype(dCache, name, friendlyName, "-"), Technology {
        public IReadOnlyCollection<Technology> Prerequisites => prerequisites;

        public IReadOnlyCollection<Technology> PostTechs => postTechs;

        public IReadOnlyCollection<Recipe> UnlockedRecipes => unlockedRecipes;

        public IReadOnlyCollection<Recipe> UnlockedQualities => UnlockedQualities;

        public IReadOnlyDictionary<Item, double> SciPackSet => sciPackSet;

        public IReadOnlyList<Item> SciPackList => sciPackList;

        public double ResearchCost { get; set; } = 0;
        public int Tier { get; set; }

        internal HashSet<TechnologyPrototype> prerequisites { get; private set; } = [];
        internal HashSet<TechnologyPrototype> postTechs { get; private set; } = [];
        internal HashSet<RecipePrototype> unlockedRecipes { get; private set; } = [];
        internal HashSet<QualityPrototype> unlockedQualities { get; private set; } = [];
        internal Dictionary<Item, double> sciPackSet { get; private set; } = new();
        internal List<Item> sciPackList { get; private set; } = [];


        public void InternalOneWayAddSciPack(ItemPrototype pack, double quantity) {
            if (sciPackSet.ContainsKey(pack))
                sciPackSet[pack] += quantity;
            else {
                sciPackSet.Add(pack, quantity);
                sciPackList.Add(pack);
            }
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is not TechnologyPrototype prototype) {
                return false;
            }

            return this == prototype;
        }

        public static bool operator ==(TechnologyPrototype item1, TechnologyPrototype item2) {
            if (ReferenceEquals(item1, item2)) {
                return true;
            }

            if ((object) item1 == null || (object) item2 == null) {
                return false;
            }

            return item1.Name == item2.Name;
        }

        public static bool operator !=(TechnologyPrototype item1, TechnologyPrototype item2) {
            return !(item1 == item2);
        }

        public override string ToString() {
            return $"Technology: {Name}";
        }
    }
}