using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Foreman
{
	public interface Recipe : DataObjectBase
	{
		Subgroup MySubgroup { get; }

		double Time { get; set; }
		long RecipeID { get; }
		bool IsMissing { get; }

		IReadOnlyDictionary<Item, double> ProductSet { get; }
		IReadOnlyList<Item> ProductList { get; }
		IReadOnlyDictionary<Item, double> ProductTemperatureMap { get; }

		IReadOnlyDictionary<Item, double> IngredientSet { get; }
		IReadOnlyList<Item> IngredientList { get; }
		IReadOnlyDictionary<Item, fRange> IngredientTemperatureMap { get; }

		IReadOnlyCollection<Assembler> Assemblers { get; }
		IReadOnlyCollection<Module> Modules { get; }

		IReadOnlyCollection<Technology> MyUnlockTechnologies { get; }
		IReadOnlyList<IReadOnlyList<Item>> MyUnlockSciencePacks { get; }

		string GetIngredientFriendlyName(Item item);
		string GetProductFriendlyName(Item item);
		bool TestIngredientConnection(Recipe provider, Item ingredient);

	}

	public class RecipePrototype : DataObjectBasePrototype, Recipe
	{
		public Subgroup MySubgroup { get { return mySubgroup; } }

		public double Time { get; set; }

		public IReadOnlyDictionary<Item, double> ProductSet { get { return productSet; } }
		public IReadOnlyList<Item> ProductList { get { return productList; } }
		public IReadOnlyDictionary<Item, double> ProductTemperatureMap { get { return productTemperatureMap; } }

		public IReadOnlyDictionary<Item, double> IngredientSet { get { return ingredientSet; } }
		public IReadOnlyList<Item> IngredientList { get { return ingredientList; } }
		public IReadOnlyDictionary<Item, fRange> IngredientTemperatureMap { get { return ingredientTemperatureMap; } }

		public IReadOnlyCollection<Assembler> Assemblers { get { return assemblers; } }
		public IReadOnlyCollection<Module> Modules { get { return modules; } }

		public IReadOnlyCollection<Technology> MyUnlockTechnologies { get { return myUnlockTechnologies; } }
		public IReadOnlyList<IReadOnlyList<Item>> MyUnlockSciencePacks { get; set; }

		internal SubgroupPrototype mySubgroup;

		internal Dictionary<Item, double> productSet { get; private set; }
		internal Dictionary<Item, double> productTemperatureMap { get; private set; }
		internal List<ItemPrototype> productList { get; private set; }

		internal Dictionary<Item, double> ingredientSet { get; private set; }
		internal Dictionary<Item, fRange> ingredientTemperatureMap { get; private set; }
		internal List<ItemPrototype> ingredientList { get; private set; }

		internal HashSet<AssemblerPrototype> assemblers { get; private set; }
		internal HashSet<ModulePrototype> modules { get; private set; }

		internal HashSet<TechnologyPrototype> myUnlockTechnologies { get; private set; }

		public bool IsMissing { get; private set; }

		private static long lastRecipeID = 0;
		public long RecipeID { get; private set; }

		public RecipePrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : base(dCache, name, friendlyName, order)
		{
			RecipeID = lastRecipeID++;

			mySubgroup = subgroup;
			subgroup.recipes.Add(this);

			Time = 0.5f;
			this.Enabled = true;
			this.IsMissing = isMissing;

			ingredientSet = new Dictionary<Item, double>();
			ingredientList = new List<ItemPrototype>();
			ingredientTemperatureMap = new Dictionary<Item, fRange>();

			productSet = new Dictionary<Item, double>();
			productList = new List<ItemPrototype>();
			productTemperatureMap = new Dictionary<Item, double>();

			assemblers = new HashSet<AssemblerPrototype>();
			modules = new HashSet<ModulePrototype>();
			myUnlockTechnologies = new HashSet<TechnologyPrototype>();
		}

		public string GetIngredientFriendlyName(Item item)
		{
			if (IngredientSet.ContainsKey(item) && (item is Fluid fluid) && fluid.IsTemperatureDependent)
				return fluid.GetTemperatureRangeFriendlyName(IngredientTemperatureMap[item]);
			return item.FriendlyName;
		}

		public string GetProductFriendlyName(Item item)
		{
			if (productSet.ContainsKey(item) && (item is Fluid fluid) && (fluid.IsTemperatureDependent || fluid.DefaultTemperature != ProductTemperatureMap[item]))
				return fluid.GetTemperatureFriendlyName(productTemperatureMap[item]);
			return item.FriendlyName;
		}

		public bool TestIngredientConnection(Recipe provider, Item ingredient) //checks if the temperature that the ingredient is coming out at fits within the range of temperatures required for this recipe
		{
			if (!IngredientSet.ContainsKey(ingredient) || !provider.ProductSet.ContainsKey(ingredient))
				return false;

			return IngredientTemperatureMap[ingredient].Contains(provider.ProductTemperatureMap[ingredient]);
		}

		public void InternalOneWayAddIngredient(ItemPrototype item, double quantity, double minTemp = double.NaN, double maxTemp = double.NaN)
		{
			if (IngredientSet.ContainsKey(item))
				ingredientSet[item] += quantity;
			else
			{
				ingredientSet.Add(item, quantity);
				ingredientList.Add(item);

				minTemp = (item is Fluid && double.IsNaN(minTemp) ? double.NegativeInfinity : minTemp);
				maxTemp = (item is Fluid && double.IsNaN(maxTemp) ? double.PositiveInfinity : maxTemp);
				ingredientTemperatureMap.Add(item, new fRange(minTemp, maxTemp));
			}
		}

		internal void InternalOneWayDeleteIngredient(ItemPrototype item) //only from delete calls
		{
			ingredientSet.Remove(item);
			ingredientList.Remove(item);
			ingredientTemperatureMap.Remove(item);
		}

		public void InternalOneWayAddProduct(ItemPrototype item, double quantity, double temperature = double.NaN)
		{
			if (productSet.ContainsKey(item))
				productSet[item] += quantity;
			else
			{
				productSet.Add(item, quantity);
				productList.Add(item);

				temperature = (item is Fluid fluid && temperature == double.NaN) ? fluid.DefaultTemperature : temperature;
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

	public class RecipeNaInPrComparer : IEqualityComparer<Recipe> //compares by name, ingredient names, and product names
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

	public struct fRange
	{
		//NOTE: there is no check for min to be guaranteed to be less than max, and this is BY DESIGN
		//this means that if your range is for example from 10 to 8, (and it isnt ignored), ANY call to Contains methods will return false
		//ex: 2 recipes, one requiring fluid 0->10 degrees, other requiring fluid 20->30 degrees. A proper summation of ranges will result in a vaild range of 20->10 degrees to satisfy both recipes, aka: NO TEMP WILL SATISFY!
		public double Min;
		public double Max;
		public bool Ignore;

		public fRange(double min, double max, bool ignore = false) { Min = min; Max = max; Ignore = ignore; }
		public bool Contains(double value) { return Ignore || double.IsNaN(value) || ((double.IsNaN(Min) || value >= Min) && (double.IsNaN(Max) || value <= Max)); }
		public bool Contains(fRange range) { return Ignore || range.Ignore || ((double.IsNaN(this.Min) || double.IsNaN(range.Min) || this.Min <= range.Min) && (double.IsNaN(this.Max) || double.IsNaN(range.Max) || this.Max >= range.Max)); }
		public bool IsPoint() { return Ignore || Min == Max; } //true if the range is a single point (min is max, and we arent ignoring it)
	}
}
