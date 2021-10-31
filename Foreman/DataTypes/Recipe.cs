using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Foreman
{
	public class RecipeShort
	{
		public string Name;
		public List<string> Ingredients;
		public List<string> Products;

		public RecipeShort(Recipe recipe)
		{
			Name = recipe.Name;
			Ingredients = recipe.IngredientSet.Keys.Select(item => item.Name).ToList();
			Products = recipe.ProductSet.Keys.Select(item => item.Name).ToList();
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

    public class Recipe : DataObjectBase
    {
        public static string[] recipeLocaleCategories = { "recipe-name" };

        public Subgroup MySubgroup { get; protected set; }

        public float Time { get; set; }
        public string Category { get; set; }

        public IReadOnlyDictionary<Item, float> ProductSet { get { return productSet; } }
		public IReadOnlyList<Item> ProductList { get { return productList; } }
        public IReadOnlyDictionary<Item, float> ProductTemperatureMap { get { return productTemperatureMap; } }

        public IReadOnlyDictionary<Item, float> IngredientSet { get { return ingredientSet; } }
		public IReadOnlyList<Item> IngredientList { get { return ingredientList; } }
        public IReadOnlyDictionary<Item, fRange> IngredientTemperatureMap { get { return ingredientTemperatureMap; } }

		private Dictionary<Item, float> productSet;
        private Dictionary<Item, float> productTemperatureMap;
		private List<Item> productList;
		private Dictionary<Item, float> ingredientSet;
        private Dictionary<Item, fRange> ingredientTemperatureMap;
		private List<Item> ingredientList;

		public bool IsAvailableAtStart { get; set; }
		public bool IsCyclic { get; set; }
		public bool IsMissingRecipe { get { return DataCache.MissingRecipes.ContainsKey(Name); } }

		public bool Hidden { get; set; }

		public bool HasEnabledAssemblers { get; set; }

		public new Bitmap Icon
		{
			get
			{
				if (base.Icon == null)
				{
					if (ProductSet.Count == 1)
						base.Icon = ProductSet.Keys.First().Icon;
					else
						base.Icon = DataCache.UnknownIcon;
				}
				return base.Icon;
			}
			set { base.Icon = value; }
		}

		public Recipe(string name, string lname, Subgroup subgroup, string order) : base(name, lname, order)
		{
			localeCategories = recipeLocaleCategories;

			MySubgroup = subgroup;
			MySubgroup.Recipes.Add(this);

			this.Time = 0.5f;
			this.ingredientSet = new Dictionary<Item, float>();
			this.ingredientList = new List<Item>();
            this.ingredientTemperatureMap = new Dictionary<Item, fRange>();
			this.productSet = new Dictionary<Item, float>();
			this.productList = new List<Item>();
            this.productTemperatureMap = new Dictionary<Item, float>();
			this.HasEnabledAssemblers = false;

			this.Hidden = false;
			this.IsAvailableAtStart = false;
			this.IsCyclic = false;
		}

		public string GetIngredientFriendlyName(Item item)
		{
			if (!ingredientSet.ContainsKey(item))
				return item.FriendlyName;
			if (!item.IsTemperatureDependent)
				return item.FriendlyName;
			return Item.GetTemperatureRangeFriendlyName(item, IngredientTemperatureMap[item]);
        }

        public string GetProductFriendlyName(Item item)
        {
            if (!productSet.ContainsKey(item))
                return item.FriendlyName;

            string name = item.FriendlyName;
            if (item.IsTemperatureDependent)
				name += " (" + ProductTemperatureMap[item].ToString("0") + "°)";

            return name;
        }

        public bool TestIngredientConnection(Recipe provider, Item ingredient) //checks if the temperature that the ingredient is coming out at fits within the range of temperatures required for this recipe
        {
            if (!ingredientSet.ContainsKey(ingredient) || !provider.productSet.ContainsKey(ingredient))
                return false;

            return ingredientTemperatureMap[ingredient].Contains(provider.productTemperatureMap[ingredient]);
        }

        public void AddIngredient(Item item, float quantity, float minTemp = float.NegativeInfinity, float maxTemp = float.PositiveInfinity)
        {
			if (ingredientSet.ContainsKey(item))
				ingredientSet[item] += quantity;
			else
            {
				ingredientSet.Add(item, quantity);
				ingredientList.Add(item);
                ingredientTemperatureMap.Add(item, new fRange(minTemp, maxTemp));
				item.InternalAddConsumptionRecipe(this);
            }
        }

		public bool DeleteIngredient(Item item)
        {
			if (!ingredientSet.ContainsKey(item))
				return false;
			ingredientSet.Remove(item);
			ingredientList.Remove(item);
            ingredientTemperatureMap.Remove(item);
			item.InternalRemoveConsumptionRecipe(this);
			return true;
        }

        public void AddProduct(Item item, float quantity, float temperature = 0)
		{
			if (productSet.ContainsKey(item))
				productSet[item] += quantity;
			else
			{
				productSet.Add(item, quantity);
				productList.Add(item);
                productTemperatureMap.Add(item, temperature);
				item.InternalAddProductionRecipe(this);
			}
		}

		public bool DeleteProduct(Item item)
		{
			if (!productSet.ContainsKey(item))
				return false;
			productSet.Remove(item);
			productList.Remove(item);
            productTemperatureMap.Remove(item);
			item.InternalRemoveProductionRecipe(this);
			return true;
		}

		public override string ToString() { return String.Format("Recipe: {0}", Name); }
	}
}
