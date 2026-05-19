using System.Collections.Generic;
using System.Linq;

namespace Foreman.DataCaching.DataTypes {
    public interface IPlantProcess : IDataObjectBase {

        double GrowTime { get; } //seconds
        long PlantID { get; }
        bool IsMissing { get; }

        IReadOnlyDictionary<IItem, double> ProductSet { get; }
        IReadOnlyList<IItem> ProductList { get; }

        IItem? Seed { get; }
    }

    public class PlantProcessPrototype : DataObjectBasePrototype, IPlantProcess {
        public double GrowTime { get; internal set; }

        public IReadOnlyDictionary<IItem, double> ProductSet { get { return _productSet; } }
        public IReadOnlyList<IItem> ProductList { get { return _productList; } }

        public IItem? Seed { get; internal set; }

        private readonly Dictionary<IItem, double> _productSet;
        internal Dictionary<IItem, double> ProductSetInternal => _productSet;

        private readonly List<ItemPrototype> _productList;
        internal List<ItemPrototype> ProductListInternal => _productList;

        private readonly HashSet<TechnologyPrototype>? _myUnlockTechnologies;
        internal HashSet<TechnologyPrototype>? MyUnlockTechnologiesInternal => _myUnlockTechnologies;

        public bool IsMissing { get; private set; }

        private static long lastPlantID;
        public long PlantID { get; private set; }

        public PlantProcessPrototype(DataCache dCache, string name, bool isMissing = false) : base(dCache, name, name, "-") {
            PlantID = lastPlantID++;

            GrowTime = 0.5f;
            this.Enabled = true;
            this.IsMissing = isMissing;

            _productSet = [];
            _productList = [];
            _myUnlockTechnologies = [];
        }

        public void InternalOneWayAddProduct(ItemPrototype item, double quantity) {
            if (_productSet.ContainsKey(item)) {
                _productSet[item] += quantity;
            } else {
                _productSet.Add(item, quantity);
                _productList.Add(item);
            }
        }

        internal void InternalOneWayDeleteProduct(ItemPrototype item) //only from delete calls
        {
            _productSet.Remove(item);
            _productList.Remove(item);
        }

        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "Planting process: {0} Id:{1}", Name, PlantID); }
    }

    public class PlantNaInPrComparer : IEqualityComparer<IPlantProcess> //compares by name, ingredient names, and product names (but not exact values!)
    {
        public bool Equals(IPlantProcess? x, IPlantProcess? y) {
            return x == y ||
                (x?.Name == y?.Name &&
                x?.ProductList.Count == y?.ProductList.Count &&
                x?.Seed == y?.Seed &&
                x?.ProductList.All(i => y?.ProductSet.ContainsKey(i) is true) is true);
        }

        public int GetHashCode(IPlantProcess obj) {
            return obj.GetHashCode();
        }
    }
}
