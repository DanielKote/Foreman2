using System;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman
{
	public class Item : DataObjectBase
	{
		public static string[] itemLocaleCategories = { "item-name", "fluid-name", "entity-name", "equipment-name" };
        public Subgroup MySubgroup { get; protected set; }

		public IReadOnlyCollection<Recipe> ProductionRecipes { get { return productionRecipes; } }
		public IReadOnlyCollection<Recipe> ConsumptionRecipes { get { return consumptionRecipes; } }
		private HashSet<Recipe> productionRecipes;
		private HashSet<Recipe> consumptionRecipes;

		public bool IsMissingItem { get { return DataCache.MissingItems.ContainsKey(Name); } }
		public bool IsFluid { get; private set; }
        public bool IsTemperatureDependent { get; set; } //while building the recipes, if we notice any product fluid NOT producted at its default temperature, we mark that fluid as temperature dependent

		public double Temperature { get; set; } //for liquids

		public new Bitmap Icon
        {
			get
			{
				if (base.Icon == null)
					base.Icon = DataCache.UnknownIcon;
				return base.Icon;
            }
			set { base.Icon = value; }
        }

		public Item(string name, string lname, bool isfluid, Subgroup subgroup, string order) : base(name, lname, order)
		{
			MySubgroup = subgroup;
			MySubgroup.Items.Add(this);
			localeCategories = itemLocaleCategories;
			productionRecipes = new HashSet<Recipe>();
			consumptionRecipes = new HashSet<Recipe>();

			IsFluid = isfluid;
            Temperature = 0;
            IsTemperatureDependent = false;
		}

		internal void InternalAddConsumptionRecipe(Recipe recipe) //should only be called from the Recipe class when it adds an ingredient
        {
			consumptionRecipes.Add(recipe);
        }
		internal void InternalAddProductionRecipe(Recipe recipe) //should only be called from the Recipe class when it adds a product
        {
			productionRecipes.Add(recipe);
        }
		internal void InternalRemoveConsumptionRecipe(Recipe recipe) //should only be called from the Recipe class when it removes an ingredient
		{
			consumptionRecipes.Remove(recipe);
		}
		internal void InternalRemoveProductionRecipe(Recipe recipe) //should only be called from the Recipe class when it removes a product
		{
			productionRecipes.Remove(recipe);
		}

		public override string ToString() { return String.Format("Item: {0}", Name); }

		public static string GetTemperatureRangeFriendlyName(Item item, fRange tempRange)
        {
			if (tempRange.Ignore)
				return item.FriendlyName;

			string name = item.FriendlyName;
			bool includeMin = tempRange.Min >= float.MinValue; //== float.NegativeInfinity;
			bool includeMax = tempRange.Max <= float.MaxValue; //== float.PositiveInfinity;

			if (tempRange.Min == tempRange.Max)
				name += " (" + tempRange.Min.ToString("0") + "°)";
			else if (includeMin && includeMax)
				name += " (" + tempRange.Min.ToString("0") + "°-" + tempRange.Max.ToString("0") + "°)";
			else if (includeMin)
				name += " (min " + tempRange.Min.ToString("0") + "°)";
			else if (includeMax)
				name += " (max " + tempRange.Max.ToString("0") + "°)";
			else
				name += "(any°)";

			return name;
		}
	}
}
