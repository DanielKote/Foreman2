using System;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman
{
	public class Item : DataObjectBase
	{
        public Subgroup MySubgroup { get; protected set; }

		public IReadOnlyCollection<Recipe> ProductionRecipes { get { return productionRecipes; } }
		public IReadOnlyCollection<Recipe> ConsumptionRecipes { get { return consumptionRecipes; } }
		public IReadOnlyCollection<Resource> MiningResources { get { return miningResources; } }

		private HashSet<Recipe> productionRecipes;
		private HashSet<Recipe> consumptionRecipes;
		private HashSet<Resource> miningResources;

		public bool IsMissingItem { get; private set; }

		public bool IsFluid { get; private set; }
        public bool IsTemperatureDependent { get; set; } //while building the recipes, if we notice any product fluid NOT producted at its default temperature, we mark that fluid as temperature dependent

		public double Temperature { get; set; } //for liquids

		public Item(DataCache dCache, string name, string friendlyName, bool isfluid, Subgroup subgroup, string order, bool isMissing = false) : base(dCache, name, friendlyName, order)
		{
			MySubgroup = subgroup;
			MySubgroup.InternalOneWayAddItem(this);
			productionRecipes = new HashSet<Recipe>();
			consumptionRecipes = new HashSet<Recipe>();
			miningResources = new HashSet<Resource>();

			IsFluid = isfluid;
            Temperature = 0;
            IsTemperatureDependent = false;
			IsMissingItem = isMissing;
			if (isMissing)
				myCache.MissingItems.Add(this.Name, this);
		}

		internal void InternalOneWayAddConsumptionRecipe(Recipe recipe) //should only be called from the Recipe class when it adds an ingredient
        {
			consumptionRecipes.Add(recipe);
        }
		internal void InternalOneWayAddProductionRecipe(Recipe recipe) //should only be called from the Recipe class when it adds a product
        {
			productionRecipes.Add(recipe);
        }
		internal void InternalOneWayRemoveConsumptionRecipe(Recipe recipe) //only from delete calls
		{
			consumptionRecipes.Remove(recipe);
		}
		internal void InternalOneWayRemoveProductionRecipe(Recipe recipe) //only from delete calls
		{
			productionRecipes.Remove(recipe);
		}

		internal void InternalOneWayAddMiningResource(Resource resource) //should only be called from Resource class
        {
			miningResources.Add(resource);
        }

		internal void InternalOneWayRemoveMiningResource(Resource resource) //only from delete calls
        {
			miningResources.Remove(resource);
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
