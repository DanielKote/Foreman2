using System.Collections.Generic;
using System.Linq;

namespace Foreman {
    public interface PlantProcess : DataObjectBase {
        // seconds
        double GrowTime { get; }
        long PlantID { get; }
        bool IsMissing { get; }

        IReadOnlyDictionary<Item, double> ProductSet { get; }
        IReadOnlyList<Item> ProductList { get; }

        Item Seed { get; }
    }

    public class PlantProcessPrototype : DataObjectBasePrototype, PlantProcess {
        public double GrowTime { get; internal set; }

        public IReadOnlyDictionary<Item, double> ProductSet => productSet;

        public IReadOnlyList<Item> ProductList => productList;

        public Item Seed { get; internal set; }

        internal Dictionary<Item, double> productSet { get; private set; }
        internal List<ItemPrototype> productList { get; private set; }

        internal HashSet<TechnologyPrototype> myUnlockTechnologies { get; private set; }

        public bool IsMissing { get; private set; }

        private static long lastPlantID;
        public long PlantID { get; private set; }

        public PlantProcessPrototype(DataCache dCache, string name, bool isMissing = false) : base(dCache, name, name, "-") {
            PlantID = lastPlantID++;

            GrowTime = 0.5f;
            Enabled = true;
            IsMissing = isMissing;

            productSet = new Dictionary<Item, double>();
            productList = [];
        }

        public void InternalOneWayAddProduct(ItemPrototype item, double quantity) {
            if (productSet.ContainsKey(item)) {
                productSet[item] += quantity;
            } else {
                productSet.Add(item, quantity);
                productList.Add(item);
            }
        }

        // only from delete calls
        internal void InternalOneWayDeleteProduct(ItemPrototype item) {
            productSet.Remove(item);
            productList.Remove(item);
        }

        public override string ToString() {
            return $"Planting process: {Name} Id:{PlantID}";
        }
    }

    // compares by name, ingredient names, and product names
    // (but not exact values!)
    public class PlantNaInPrComparer : IEqualityComparer<PlantProcess> {
        public bool Equals(PlantProcess x, PlantProcess y) {
            if (x == y)
                return true;

            if (x.Name != y.Name)
                return false;
            if (x.ProductList.Count != y.ProductList.Count)
                return false;

            if (x.Seed != y.Seed)
                return false;

            return x.ProductList.All(i => y.ProductSet.ContainsKey(i));
        }

        public int GetHashCode(PlantProcess obj) {
            return obj.GetHashCode();
        }
    }
}