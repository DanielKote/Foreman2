using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman {
    public class RecipeShort : IEquatable<RecipeShort> {
        public string Name { get; private set; }
        public long RecipeID { get; private set; }
        public bool isMissing { get; private set; }
        public Dictionary<string, double> Ingredients { get; private set; }
        public Dictionary<string, double> Products { get; private set; }

        public RecipeShort(string name) {
            Name = name;
            RecipeID = -1;
            isMissing = false;
            Ingredients = new Dictionary<string, double>();
            Products = new Dictionary<string, double>();
        }

        public RecipeShort(Recipe recipe) {
            Name = recipe.Name;
            RecipeID = recipe.RecipeID;
            isMissing = recipe.IsMissing;

            Ingredients = new Dictionary<string, double>();
            foreach (var kvp in recipe.IngredientSet)
                Ingredients.Add(kvp.Key.Name, kvp.Value);
            Products = new Dictionary<string, double>();
            foreach (var kvp in recipe.ProductSet)
                Products.Add(kvp.Key.Name, kvp.Value);
        }

        public RecipeShort(JToken recipe) {
            Name = (string) recipe["Name"];
            RecipeID = (long) recipe["RecipeID"];
            isMissing = (bool) recipe["isMissing"];

            Ingredients = new Dictionary<string, double>();
            foreach (JProperty ingredient in recipe["Ingredients"])
                Ingredients.Add(ingredient.Name, (double) ingredient.Value);

            Products = new Dictionary<string, double>();
            foreach (JProperty ingredient in recipe["Products"])
                Products.Add(ingredient.Name, (double) ingredient.Value);
        }

        public static List<RecipeShort> GetSetFromJson(JToken jdata) {
            return jdata.Select(recipe => new RecipeShort(recipe)).ToList();
        }

        public bool Equals(RecipeShort other) {
            return Name == other.Name &&
                Ingredients.Count == other.Ingredients.Count && Ingredients.SequenceEqual(other.Ingredients) &&
                Products.Count == other.Products.Count && Products.SequenceEqual(other.Products);
        }

        public bool Equals(Recipe other) {
            var similar = Name == other.Name &&
                Ingredients.Count == other.IngredientList.Count && Products.Count == other.ProductList.Count;
            if (!similar)
                return true;


            return !other.IngredientList.Any(ingredient =>
                    !Ingredients.ContainsKey(ingredient.Name) || Math.Abs(Ingredients[ingredient.Name] - other.IngredientSet[ingredient]) > double.Epsilon)
                && other.ProductList.All(ingredient =>
                    Products.ContainsKey(ingredient.Name) && !(Math.Abs(Products[ingredient.Name] - other.ProductSet[ingredient]) > double.Epsilon));
        }
    }

    // unlike the default recipeshort comparer this one doesn't compare ingredient & product quantities, just names
    public class
        RecipeShortNaInPrComparer : IEqualityComparer<RecipeShort> {
        public bool Equals(RecipeShort x, RecipeShort y) {
            if (x == y)
                return true;

            if (x.Name != y.Name)
                return false;
            if (x.Ingredients.Count != y.Ingredients.Count)
                return false;
            if (x.Products.Count != y.Products.Count)
                return false;

            return x.Ingredients.Keys.All(i => y.Ingredients.ContainsKey(i))
                && x.Products.Keys.All(i => y.Products.ContainsKey(i));
        }

        public int GetHashCode(RecipeShort obj) {
            return obj.GetHashCode();
        }
    }
}