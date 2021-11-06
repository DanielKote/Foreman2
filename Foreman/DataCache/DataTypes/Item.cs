using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman
{
	public interface Item : DataObjectBase
	{
		Subgroup MySubgroup { get; }

		IReadOnlyCollection<Recipe> ProductionRecipes { get; }
		IReadOnlyCollection<Recipe> ConsumptionRecipes { get; }
		IReadOnlyCollection<Technology> ConsumptionTechnologies { get; }

		bool IsMissing { get; }

		int StackSize { get; }

		double FuelValue { get; }
		double PollutionMultiplier { get; }
		Item BurnResult { get; }
		Item FuelOrigin { get; }
		IReadOnlyCollection<EntityObjectBase> FuelsEntities { get; }
	}

	public class ItemPrototype : DataObjectBasePrototype, Item
	{
		public Subgroup MySubgroup { get { return mySubgroup; } }

		public IReadOnlyCollection<Recipe> ProductionRecipes { get { return productionRecipes; } }
		public IReadOnlyCollection<Recipe> ConsumptionRecipes { get { return consumptionRecipes; } }
		public IReadOnlyCollection<Technology> ConsumptionTechnologies { get { return consumptionTechnologies; } }

		public bool IsMissing { get; private set; }

		public int StackSize { get; set; }

		public double FuelValue { get; internal set; }
		public double PollutionMultiplier { get; internal set; }
		public Item BurnResult { get; internal set; }
		public Item FuelOrigin { get; internal set; }
		public IReadOnlyCollection<EntityObjectBase> FuelsEntities { get { return fuelsEntities; } }

		internal SubgroupPrototype mySubgroup;

		internal HashSet<RecipePrototype> productionRecipes { get; private set; }
		internal HashSet<RecipePrototype> consumptionRecipes { get; private set; }
		internal HashSet<TechnologyPrototype> consumptionTechnologies { get; private set; }
		internal HashSet<EntityObjectBasePrototype> fuelsEntities { get; private set; }

		public ItemPrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : base(dCache, name, friendlyName, order)
		{
			mySubgroup = subgroup;
			subgroup.items.Add(this);

			StackSize = 1;

			productionRecipes = new HashSet<RecipePrototype>();
			consumptionRecipes = new HashSet<RecipePrototype>();
			consumptionTechnologies = new HashSet<TechnologyPrototype>();
			fuelsEntities = new HashSet<EntityObjectBasePrototype>();

			FuelValue = 1f; //useful for preventing overlow issues when using missing items / non-fuel items (loading with wrong mods / importing from alt mod group can cause this)
			PollutionMultiplier = 1f;
			IsMissing = isMissing;
		}

		public override string ToString() { return string.Format("Item: {0}", Name); }
	}
}
