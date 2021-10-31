using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman
{
	public class RecipeShort : IEquatable<RecipeShort>
	{
		public string Name { get; private set; }
		public long RecipeID { get; private set; }
		public bool isMissing { get; private set; }
		public Dictionary<string, double> Ingredients { get; private set; }
		public Dictionary<string, double> Products { get; private set; }

		public RecipeShort(string name)
		{
			Name = name;
			RecipeID = -1;
			isMissing = false;
			Ingredients = new Dictionary<string, double>();
			Products = new Dictionary<string, double>();
		}

		public RecipeShort(Recipe recipe)
		{
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

		public RecipeShort(JToken recipe)
		{
			Name = (string)recipe["Name"];
			RecipeID = (long)recipe["RecipeID"];
			isMissing = (bool)recipe["isMissing"];

			Ingredients = new Dictionary<string, double>();
			foreach (JProperty ingredient in recipe["Ingredients"])
				Ingredients.Add((string)ingredient.Name, (double)ingredient.Value);

			Products = new Dictionary<string, double>();
			foreach (JProperty ingredient in recipe["Products"])
				Products.Add((string)ingredient.Name, (double)ingredient.Value);
		}

		public static List<RecipeShort> GetSetFromJson(JToken jdata)
		{
			List<RecipeShort> resultList = new List<RecipeShort>();
			foreach (JToken recipe in jdata)
				resultList.Add(new RecipeShort(recipe));
			return resultList;
		}

		public bool Equals(RecipeShort other)
		{
			return this.Name == other.Name &&
				this.Ingredients.Count == other.Ingredients.Count && this.Ingredients.SequenceEqual(other.Ingredients) &&
				this.Products.Count == other.Products.Count && this.Products.SequenceEqual(other.Products);
		}

		public bool Equals(Recipe other)
		{
			bool similar = this.Name == other.Name &&
				this.Ingredients.Count == other.IngredientList.Count && this.Products.Count == other.ProductList.Count;

			if (similar)
			{
				foreach (Item ingredient in other.IngredientList)
					if (!this.Ingredients.ContainsKey(ingredient.Name) || this.Ingredients[ingredient.Name] != other.IngredientSet[ingredient])
						return false;
				foreach (Item ingredient in other.ProductList)
					if (!this.Products.ContainsKey(ingredient.Name) || this.Products[ingredient.Name] != other.ProductSet[ingredient])
						return false;
			}
			return true;
		}
	}

	public class RecipeShortNaInPrComparer : IEqualityComparer<RecipeShort> //unlike the default recipeshort comparer this one doesnt compare ingredient & product quantities, just names
	{
		public bool Equals(RecipeShort x, RecipeShort y)
		{
			if (x == y)
				return true;

			if (x.Name != y.Name)
				return false;
			if (x.Ingredients.Count != y.Ingredients.Count)
				return false;
			if (x.Products.Count != y.Products.Count)
				return false;

			foreach (string i in x.Ingredients.Keys)
				if (!y.Ingredients.ContainsKey(i))
					return false;
			foreach (string i in x.Products.Keys)
				if (!y.Products.ContainsKey(i))
					return false;

			return true;
		}

		public int GetHashCode(RecipeShort obj)
		{
			return obj.GetHashCode();
		}

	}
}
