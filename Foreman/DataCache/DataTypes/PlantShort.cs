using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman {
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

        public PlantShort(PlantProcess plantProcess) {
            Name = plantProcess.Name;
            PlantID = plantProcess.PlantID;
            isMissing = plantProcess.IsMissing;

            Products = [];
            foreach (var kvp in plantProcess.ProductSet)
                Products.Add(kvp.Key.Name, kvp.Value);
        }

        public PlantShort(JToken plantProcess) {
            Name = (string?)plantProcess["Name"] ?? "JSON ERROR";
            PlantID = (long?)plantProcess["PlantID"] ?? default;
            isMissing = (bool?)plantProcess["isMissing"] is true;

            Products = [];
            foreach (var ingredient in plantProcess["Products"]
                ?.Select(p => p as JProperty)
                .OfType<JProperty>()
                .Select(p => (double?)p.Value is double val ? (p.Name, val) : ((string, double)?)null)
                .OfType<(string Name, double Value)>() ?? [])
                Products.Add(ingredient.Name, ingredient.Value);
        }

        public static List<PlantShort> GetSetFromJson(JToken? jdata) {
            List<PlantShort> resultList = new List<PlantShort>();
            foreach (JToken recipe in jdata?.AsEnumerable() ?? [])
                resultList.Add(new PlantShort(recipe));
            return resultList;
        }

        public bool Equals(PlantShort? other) {
            return ReferenceEquals(this, other) ||
                Name == other?.Name &&
                Products.Count == other?.Products.Count &&
                Products.SequenceEqual(other.Products);
        }

        public bool Equals(PlantProcess other) {
            return Name == other.Name &&
                Products.Count == other.ProductList.Count &&
                other.ProductList.All(p => Products.TryGetValue(p.Name, out var prod) && prod == other.ProductSet[p]);

            //TODO: Surely the old logic is just wrong. Returns true if not similar?!?
            /*
			bool similar = Name == other.Name && Products.Count == other.ProductList.Count;

			if (similar)
			{
				foreach (Item ingredient in other.ProductList)
					if (!Products.ContainsKey(ingredient.Name) || Products[ingredient.Name] != other.ProductSet[ingredient])
						return false;
			}
			return true;
			*/
        }
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
            return obj.GetHashCode();
        }

    }
}