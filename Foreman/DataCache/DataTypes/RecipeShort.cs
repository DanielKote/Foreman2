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

        public RecipeShort(
            string name,
            long recipeId,
            bool missing,
            Dictionary<string, double> ingredients,
            Dictionary<string, double> products) {
            Name = name;
            RecipeID = recipeId;
            isMissing = missing;
            Ingredients = ingredients;
            Products = products;
        }

        public bool Equals(RecipeShort? other) {
            return other is not null &&
                Name == other.Name &&
                Ingredients.Count == other.Ingredients.Count && Ingredients.SequenceEqual(other.Ingredients) &&
                Products.Count == other.Products.Count && Products.SequenceEqual(other.Products);
        }

        public bool Equals(Recipe other) {
            bool similar = Name == other.Name &&
                Ingredients.Count == other.IngredientList.Count && Products.Count == other.ProductList.Count;

            if (similar) {
                foreach (Item ingredient in other.IngredientList)
                    if (!Ingredients.ContainsKey(ingredient.Name) || Ingredients[ingredient.Name] != other.IngredientSet[ingredient])
                        return false;
                foreach (Item ingredient in other.ProductList)
                    if (!Products.ContainsKey(ingredient.Name) || Products[ingredient.Name] != other.ProductSet[ingredient])
                        return false;
            }
            return true;
        }
    }

    public class RecipeShortNaInPrComparer : IEqualityComparer<RecipeShort> //unlike the default recipeshort comparer this one doesnt compare ingredient & product quantities, just names
    {
        public bool Equals(RecipeShort? x, RecipeShort? y) {
            return ReferenceEquals(x, y) ||
                x == y ||
                x is not null && y is not null &&
                x.Name == y.Name &&
                x.Ingredients.Count == y.Ingredients.Count &&
                x.Products.Count == y.Products.Count &&
                x.Ingredients.Keys.All(i => y.Ingredients.ContainsKey(i)) &&
                x.Products.Keys.All(p => y.Products.ContainsKey(p));
        }

        public int GetHashCode(RecipeShort obj) {
            return obj.GetHashCode();
        }

    }
}