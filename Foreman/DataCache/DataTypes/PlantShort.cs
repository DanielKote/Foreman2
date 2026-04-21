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
            Products = new Dictionary<string, double>();
        }

        public PlantShort(PlantProcess plantProcess) {
            Name = plantProcess.Name;
            PlantID = plantProcess.PlantID;
            isMissing = plantProcess.IsMissing;

            Products = new Dictionary<string, double>();
            foreach (var kvp in plantProcess.ProductSet)
                Products.Add(kvp.Key.Name, kvp.Value);
        }

        public PlantShort(JToken plantProcess) {
            Name = (string) plantProcess["Name"];
            PlantID = (long) plantProcess["PlantID"];
            isMissing = (bool) plantProcess["isMissing"];

            Products = new Dictionary<string, double>();
            foreach (JProperty ingredient in plantProcess["Products"])
                Products.Add(ingredient.Name, (double) ingredient.Value);
        }

        public static List<PlantShort> GetSetFromJson(JToken jdata) {
            return jdata.Select(recipe => new PlantShort(recipe)).ToList();
        }

        public bool Equals(PlantShort other) {
            return Name == other.Name &&
                Products.Count == other.Products.Count && Products.SequenceEqual(other.Products);
        }

        public bool Equals(PlantProcess other) {
            if (Name != other.Name || Products.Count != other.ProductList.Count)
                return true;

            foreach (var ingredient in other.ProductList) {
                if (!Products.ContainsKey(ingredient.Name) || Math.Abs(Products[ingredient.Name] - other.ProductSet[ingredient]) > double.Epsilon)
                    return false;
            }

            return true;
        }
    }

    // unlike the default plantshort comparer this one doesn't compare product quantities, just names
    public class PlantShortNaInPrComparer : IEqualityComparer<PlantShort> {
        public bool Equals(PlantShort x, PlantShort y) {
            if (x == y)
                return true;

            if (x.Name != y.Name)
                return false;

            return x.Products.Count == y.Products.Count && x.Products.Keys.All(i => y.Products.ContainsKey(i));
        }

        public int GetHashCode(PlantShort obj) {
            return obj.GetHashCode();
        }
    }
}