using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman.DataCaching.DataTypes {
    public class PlantShort : IEquatable<PlantShort> {
        public string Name { get; private set; }
        public long PlantID { get; private set; }
        public bool isMissing { get; private set; }
        public Dictionary<string, double> Products { get; private set; }

        public PlantShort(string name) {
            Name = name;
            PlantID = -1;
            isMissing = false;
            Products = [];
        }

        public PlantShort(IPlantProcess plantProcess) {
            Name = plantProcess.Name;
            PlantID = plantProcess.PlantID;
            isMissing = plantProcess.IsMissing;

            Products = [];
            foreach (var kvp in plantProcess.ProductSet)
                Products.Add(kvp.Key.Name, kvp.Value);
        }

        public PlantShort(string name, long plantId, bool missing, Dictionary<string, double> products) {
            Name = name;
            PlantID = plantId;
            isMissing = missing;
            Products = products;
        }

        public bool Equals(PlantShort? other) {
            return ReferenceEquals(this, other) ||
                Name == other?.Name &&
                Products.Count == other?.Products.Count &&
                Products.SequenceEqual(other.Products);
        }

        public bool Equals(IPlantProcess other) {
            return Name == other.Name &&
                Products.Count == other.ProductList.Count &&
                other.ProductList.All(p => Products.TryGetValue(p.Name, out var prod) && prod == other.ProductSet[p]);
        }

        public override bool Equals(object? obj) {
            return Equals(obj as PlantShort);
        }

        public override int GetHashCode() => HashCode.Combine(Name);
    }

    public class PlantShortNaInPrComparer : IEqualityComparer<PlantShort> //unlike the default plantshort comparer this one doesnt compare product quantities, just names
    {
        public bool Equals(PlantShort? x, PlantShort? y) {
            return ReferenceEquals(x, y) ||
                x == y ||
                x?.Name == y?.Name &&
                x?.Products.Count == y?.Products.Count &&
                x?.Products.Keys.All(k => y?.Products.ContainsKey(k) is true) is true;
        }

        public int GetHashCode(PlantShort obj) {
            return obj.Name.GetHashCode(StringComparison.Ordinal);
        }

    }
}
