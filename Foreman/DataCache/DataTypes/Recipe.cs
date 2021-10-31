using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Newtonsoft.Json.Linq;

namespace Foreman
{
	public interface Recipe : DataObjectBase
    {
		Subgroup MySubgroup { get; }

		float Time { get; set; }
		long RecipeID { get; }
		bool IsMissingRecipe { get; }
		bool IsCyclic { get; }
		bool HasEnabledAssemblers { get; }
		bool Enabled { get; set; }

		IReadOnlyDictionary<Item, float> ProductSet { get; }
		IReadOnlyList<Item> ProductList { get; }
		IReadOnlyDictionary<Item, float> ProductTemperatureMap { get; }

		IReadOnlyDictionary<Item, float> IngredientSet { get; }
		IReadOnlyList<Item> IngredientList { get; }
		IReadOnlyDictionary<Item, fRange> IngredientTemperatureMap { get; }

		IReadOnlyCollection<Assembler> ValidAssemblers { get; }
		IReadOnlyCollection<Module> ValidModules { get; }

		IReadOnlyCollection<Technology> MyUnlockTechnologies { get; }

		string GetIngredientFriendlyName(Item item);
		string GetProductFriendlyName(Item item);
		bool TestIngredientConnection(Recipe provider, Item ingredient);

	}

	public class RecipePrototype : DataObjectBasePrototype, Recipe
    {
        public Subgroup MySubgroup { get { return mySubgroup; } }

        public float Time { get; set; }

        public IReadOnlyDictionary<Item, float> ProductSet { get { return productSet; } }
		public IReadOnlyList<Item> ProductList { get { return productList; } }
        public IReadOnlyDictionary<Item, float> ProductTemperatureMap { get { return productTemperatureMap; } }

        public IReadOnlyDictionary<Item, float> IngredientSet { get { return ingredientSet; } }
		public IReadOnlyList<Item> IngredientList { get { return ingredientList; } }
        public IReadOnlyDictionary<Item, fRange> IngredientTemperatureMap { get { return ingredientTemperatureMap; } }

		public IReadOnlyCollection<Assembler> ValidAssemblers { get { return validAssemblers; } }
		public IReadOnlyCollection<Module> ValidModules { get { return validModules; } }

		public IReadOnlyCollection<Technology> MyUnlockTechnologies { get { return myUnlockTechnologies; } }

		internal SubgroupPrototype mySubgroup;

		internal Dictionary<Item, float> productSet { get; private set; }
		internal Dictionary<Item, float> productTemperatureMap { get; private set; }
		internal List<ItemPrototype> productList { get; private set; }

		internal Dictionary<Item, float> ingredientSet { get; private set; }
		internal Dictionary<Item, fRange> ingredientTemperatureMap { get; private set; }
		internal List<ItemPrototype> ingredientList { get; private set; }

		internal HashSet<AssemblerPrototype> validAssemblers { get; private set; }
		internal HashSet<ModulePrototype> validModules { get; private set; }

		internal HashSet<TechnologyPrototype> myUnlockTechnologies { get; private set; }

		public bool IsMissingRecipe { get; private set; }
		public bool IsCyclic { get; set; }
		public bool HasEnabledAssemblers { get { return validAssemblers.FirstOrDefault(a => a.Enabled) != null; } }
		public bool Enabled { get; set; }

		private static long lastRecipeID = 0;
		public long RecipeID { get; private set; }

		public RecipePrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : base(dCache, name, friendlyName, order)
		{
			RecipeID = lastRecipeID++;

			mySubgroup = subgroup;
			subgroup.recipes.Add(this);

			Time = 0.5f;
			this.Enabled = true;
			this.IsCyclic = false;
			this.IsMissingRecipe = isMissing;

			ingredientSet = new Dictionary<Item, float>();
			ingredientList = new List<ItemPrototype>();
            ingredientTemperatureMap = new Dictionary<Item, fRange>();

			productSet = new Dictionary<Item, float>();
			productList = new List<ItemPrototype>();
            productTemperatureMap = new Dictionary<Item, float>();

			validAssemblers = new HashSet<AssemblerPrototype>();
			validModules = new HashSet<ModulePrototype>();
			myUnlockTechnologies = new HashSet<TechnologyPrototype>();
		}

		public string GetIngredientFriendlyName(Item item)
		{
			if (!IngredientSet.ContainsKey(item))
				return item.FriendlyName;
			if (!item.IsTemperatureDependent)
				return item.FriendlyName;
			return item.GetTemperatureRangeFriendlyName(IngredientTemperatureMap[item]);
        }

        public string GetProductFriendlyName(Item item)
        {
            if (!ProductSet.ContainsKey(item))
                return item.FriendlyName;

            string name = item.FriendlyName;
            if (item.IsTemperatureDependent)
				name += " (" + ProductTemperatureMap[item].ToString("0") + "°)";

            return name;
        }

        public bool TestIngredientConnection(Recipe provider, Item ingredient) //checks if the temperature that the ingredient is coming out at fits within the range of temperatures required for this recipe
        {
            if (!IngredientSet.ContainsKey(ingredient) || !provider.ProductSet.ContainsKey(ingredient))
                return false;

            return IngredientTemperatureMap[ingredient].Contains(provider.ProductTemperatureMap[ingredient]);
        }

        public void InternalOneWayAddIngredient(ItemPrototype item, float quantity, float minTemp = float.NegativeInfinity, float maxTemp = float.PositiveInfinity)
        {
			if (IngredientSet.ContainsKey(item))
				ingredientSet[item] += quantity;
			else
            {
				ingredientSet.Add(item, quantity);
				ingredientList.Add(item);
                ingredientTemperatureMap.Add(item, new fRange(minTemp, maxTemp));
            }
        }

		internal void InternalOneWayDeleteIngredient(ItemPrototype item) //only from delete calls
		{
			ingredientSet.Remove(item);
			ingredientList.Remove(item);
            ingredientTemperatureMap.Remove(item);
        }

        public void InternalOneWayAddProduct(ItemPrototype item, float quantity, float temperature = 0)
		{
			if (productSet.ContainsKey(item))
				productSet[item] += quantity;
			else
			{
				productSet.Add(item, quantity);
				productList.Add(item);
                productTemperatureMap.Add(item, temperature);
			}
		}

		internal void InternalOneWayDeleteProduct(ItemPrototype item) //only from delete calls
		{
			productSet.Remove(item);
			productList.Remove(item);
            productTemperatureMap.Remove(item);
		}

		public override string ToString() { return String.Format("Recipe: {0} Id:{1}", Name, RecipeID); }
	}

    public class MissingRecipeComparer : IEqualityComparer<Recipe>
    {
        public bool Equals(Recipe x, Recipe y)
        {
			if (x == y)
				return true;

			if (x.Name != y.Name)
				return false;
			if (x.IngredientList.Count != y.IngredientList.Count)
				return false;
			if (x.ProductList.Count != y.ProductList.Count)
				return false;

			foreach (Item i in x.IngredientList)
				if (!y.IngredientSet.ContainsKey(i))
					return false;
			foreach (Item i in x.ProductList)
				if (!y.ProductSet.ContainsKey(i))
					return false;

			return true;
		}

        public int GetHashCode(Recipe obj)
        {
			return obj.GetHashCode();
        }
    }

    public class RecipeShort : IEquatable<RecipeShort>
	{
		public string Name { get; private set; }
		public long RecipeID { get; private set; }
		public bool isMissing { get; private set; }
		public Dictionary<string, float> Ingredients { get; private set; }
		public Dictionary<string, float> Products { get; private set; }

		public RecipeShort(string name)
		{
			Name = name;
			RecipeID = -1;
			isMissing = false;
			Ingredients = new Dictionary<string, float>();
			Products = new Dictionary<string, float>();
		}

		public RecipeShort(Recipe recipe)
		{
			Name = recipe.Name;
			RecipeID = recipe.RecipeID;
			isMissing = recipe.IsMissingRecipe;

			Ingredients = new Dictionary<string, float>();
			foreach (var kvp in recipe.IngredientSet)
				Ingredients.Add(kvp.Key.Name, kvp.Value);
			Products = new Dictionary<string, float>();
			foreach (var kvp in recipe.ProductSet)
				Products.Add(kvp.Key.Name, kvp.Value);
		}

		public RecipeShort(JToken recipe)
		{
			Name = (string)recipe["Name"];
			RecipeID = (long)recipe["RecipeID"];
			isMissing = (bool)recipe["isMissing"];

			Ingredients = new Dictionary<string, float>();
			foreach (JProperty ingredient in recipe["Ingredients"])
				Ingredients.Add((string)ingredient.Name, (float)ingredient.Value);

			Products = new Dictionary<string, float>();
			foreach (JProperty ingredient in recipe["Products"])
				Products.Add((string)ingredient.Name, (float)ingredient.Value);
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
	}

	public struct fRange
	{
		//NOTE: there is no check for min to be guaranteed to be less than max, and this is BY DESIGN
		//this means that if your range is for example from 10 to 8, (and it isnt ignored), ANY call to Contains methods will return false
		//ex: 2 recipes, one requiring fluid 0->10 degrees, other requiring fluid 20->30 degrees. A proper summation of ranges will result in a vaild range of 20->10 degrees to satisfy both recipes, aka: NO TEMP WILL SATISFY!
		public float Min;
		public float Max;
		public bool Ignore;

		public fRange(float min, float max, bool ignore = false) { Min = min; Max = max; Ignore = ignore; }
		public bool Contains(float value) { return Ignore || (value >= Min && value <= Max); }
		public bool Contains(fRange range) { return Ignore || (this.Min <= range.Min && this.Max >= range.Max); }
		public bool IsContainedIn(fRange range) { return Ignore || (range.Min <= this.Min && range.Max >= this.Max); } //Ignore counts only for this! not for the provided range
	}
}
