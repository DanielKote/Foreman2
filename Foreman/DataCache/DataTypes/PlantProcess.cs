using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public interface PlantProcess : DataObjectBase {

        double GrowTime { get; } //seconds
        long PlantID { get; }
        bool IsMissing { get; }

        IReadOnlyDictionary<Item, double> ProductSet { get; }
        IReadOnlyList<Item> ProductList { get; }

        Item? Seed { get; }
    }

    public class PlantProcessPrototype : DataObjectBasePrototype, PlantProcess {
        public double GrowTime { get; internal set; }

        public IReadOnlyDictionary<Item, double> ProductSet { get { return productSet; } }
        public IReadOnlyList<Item> ProductList { get { return productList; } }

        public Item? Seed { get; internal set; }

        internal Dictionary<Item, double> productSet { get; private set; }
        internal List<ItemPrototype> productList { get; private set; }

        internal HashSet<TechnologyPrototype>? myUnlockTechnologies { get; private set; }

        public bool IsMissing { get; private set; }

        private static long lastPlantID = 0;
        public long PlantID { get; private set; }

        public PlantProcessPrototype(DataCache dCache, string name, bool isMissing = false) : base(dCache, name, name, "-") {
            PlantID = lastPlantID++;

            GrowTime = 0.5f;
            this.Enabled = true;
            this.IsMissing = isMissing;

            productSet = new Dictionary<Item, double>();
            productList = new List<ItemPrototype>();
        }

        public void InternalOneWayAddProduct(ItemPrototype item, double quantity) {
            if (productSet.ContainsKey(item)) {
                productSet[item] += quantity;
            } else {
                productSet.Add(item, quantity);
                productList.Add(item);
            }
        }

        internal void InternalOneWayDeleteProduct(ItemPrototype item) //only from delete calls
        {
            productSet.Remove(item);
            productList.Remove(item);
        }

        public override string ToString() { return String.Format("Planting process: {0} Id:{1}", Name, PlantID); }
    }

    public class PlantNaInPrComparer : IEqualityComparer<PlantProcess> //compares by name, ingredient names, and product names (but not exact values!)
    {
        public bool Equals(PlantProcess? x, PlantProcess? y) {
            return x == y ||
                (x?.Name == y?.Name &&
                x?.ProductList.Count == y?.ProductList.Count &&
                x?.Seed == y?.Seed &&
                x?.ProductList.All(i => y?.ProductSet.ContainsKey(i) is true) is true);
        }

        public int GetHashCode(PlantProcess obj) {
            return obj.GetHashCode();
        }
    }
}