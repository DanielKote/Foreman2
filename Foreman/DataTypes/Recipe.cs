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
        public Subgroup MySubgroup { get; protected set; }

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

		private Dictionary<Item, float> productSet;
        private Dictionary<Item, float> productTemperatureMap;
		private List<Item> productList;

		private Dictionary<Item, float> ingredientSet;
        private Dictionary<Item, fRange> ingredientTemperatureMap;
		private List<Item> ingredientList;

		private HashSet<Assembler> validAssemblers;
		private HashSet<Module> validModules;

		private HashSet<Technology> myUnlockTechnologies;

		public bool IsCyclic { get; set; }
		public bool IsMissingRecipe { get { return myCache.MissingRecipes.ContainsKey(Name); } }
		public bool HasEnabledAssemblers { get { return validAssemblers.FirstOrDefault(a => a.Enabled) != null; } }

		public bool Hidden { get; set; }


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

		public Recipe(DataCache dCache, string name, string friendlyName, Subgroup subgroup, string order) : base(dCache, name, friendlyName, order)
		{
			MySubgroup = subgroup;
			MySubgroup.Recipes.Add(this);

			Time = 0.5f;
			this.Hidden = false;
			this.IsCyclic = false;

			ingredientSet = new Dictionary<Item, float>();
			ingredientList = new List<Item>();
            ingredientTemperatureMap = new Dictionary<Item, fRange>();

			productSet = new Dictionary<Item, float>();
			productList = new List<Item>();
            productTemperatureMap = new Dictionary<Item, float>();

			validAssemblers = new HashSet<Assembler>();
			validModules = new HashSet<Module>();
			myUnlockTechnologies = new HashSet<Technology>();
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
				item.InternalOneWayAddConsumptionRecipe(this);
            }
        }

		internal void InternalOneWayDeleteIngredient(Item item) //only from delete calls
		{
			ingredientSet.Remove(item);
			ingredientList.Remove(item);
            ingredientTemperatureMap.Remove(item);
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
				item.InternalOneWayAddProductionRecipe(this);
			}
		}

		internal void InternalOneWayDeleteProduct(Item item) //only from delete calls
		{
			productSet.Remove(item);
			productList.Remove(item);
            productTemperatureMap.Remove(item);
		}

		public void AddValidAssembler(Assembler assembler)
        {
			validAssemblers.Add(assembler);
			assembler.InternalOneWayAddRecipe(this);
        }

		internal void InternalOneWayRemoveValidAssembler(Assembler assembler) //only from delete calls
		{
			validAssemblers.Remove(assembler);
        }

		public void AddValidModule(Module module)
        {
			validModules.Add(module);
			module.InternalOneWayAddRecipe(this);
        }

		internal void InternalOneWayRemoveValidModule(Module module) //only from delete calls
		{
			validModules.Remove(module);
        }

		public void AddUnlockTechnology(Technology technology)
        {
			myUnlockTechnologies.Add(technology);
			technology.InternalOneWayAddRecipe(this);
        }

		internal void InternalOneWayRemoveUnlockTechnology(Technology technology) //only from delete calls
		{
			myUnlockTechnologies.Remove(technology);
        }

		public override string ToString() { return String.Format("Recipe: {0}", Name); }
	}
}
