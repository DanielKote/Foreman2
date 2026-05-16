using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public interface Item : DataObjectBase {

        Subgroup MySubgroup { get; }

        IReadOnlyCollection<Recipe> ProductionRecipes { get; }
        IReadOnlyCollection<Recipe> ConsumptionRecipes { get; }
        IReadOnlyCollection<Technology> ConsumptionTechnologies { get; }

        bool IsMissing { get; }

        int StackSize { get; }

        double Weight { get; }
        double IngredientToWeightCoefficient { get; }
        double FuelValue { get; }
        double PollutionMultiplier { get; }

        Item? BurnResult { get; }
        PlantProcess? PlantResult { get; }
        Item? SpoilResult { get; }

        Item? FuelOrigin { get; }
        IReadOnlyCollection<Item> PlantOrigins { get; }
        IReadOnlyCollection<Item> SpoilOrigins { get; }

        double GetItemSpoilageTime(Quality quality); //seconds

        IReadOnlyCollection<EntityObjectBase> FuelsEntities { get; }

        //spoil ticks are ignored - its assumed that if there is a plant/spoil result then the ticks are at least low enough to make it viable on a world basis
    }

    public class ItemPrototype : DataObjectBasePrototype, Item {
        public class Test { }

        public Subgroup MySubgroup { get { return mySubgroup; } }

        public IReadOnlyCollection<Recipe> ProductionRecipes { get { return productionRecipes; } }
        public IReadOnlyCollection<Recipe> ConsumptionRecipes { get { return consumptionRecipes; } }
        public IReadOnlyCollection<Technology> ConsumptionTechnologies { get { return consumptionTechnologies; } }

        public bool IsMissing { get; private set; }

        public int StackSize { get; set; }

        public double Weight { get; set; }
        public double IngredientToWeightCoefficient { get; set; }
        public double FuelValue { get; internal set; }
        public double PollutionMultiplier { get; internal set; }

        public Item? BurnResult { get; internal set; }
        public PlantProcess? PlantResult { get; internal set; }
        public Item? SpoilResult { get; internal set; }

        public Item? FuelOrigin { get; internal set; }
        public IReadOnlyCollection<Item> PlantOrigins { get { return plantOrigins; } }
        public IReadOnlyCollection<Item> SpoilOrigins { get { return spoilOrigins; } }

        public IReadOnlyCollection<EntityObjectBase> FuelsEntities { get { return fuelsEntities; } }

        public double GetItemSpoilageTime(Quality quality) { return spoilageTimes.ContainsKey(quality) ? spoilageTimes[quality] : 1; }

        internal SubgroupPrototype mySubgroup;

        internal HashSet<RecipePrototype> productionRecipes { get; private set; }
        internal HashSet<RecipePrototype> consumptionRecipes { get; private set; }
        internal HashSet<TechnologyPrototype> consumptionTechnologies { get; private set; }
        internal HashSet<EntityObjectBasePrototype> fuelsEntities { get; private set; }
        internal HashSet<ItemPrototype> plantOrigins { get; private set; }
        internal HashSet<ItemPrototype> spoilOrigins { get; private set; }

        internal Dictionary<Quality, double> spoilageTimes { get; private set; }

        public ItemPrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : base(dCache, name, friendlyName, order) {
            mySubgroup = subgroup;
            subgroup.items.Add(this);

            StackSize = 1;

            productionRecipes = new HashSet<RecipePrototype>();
            consumptionRecipes = new HashSet<RecipePrototype>();
            consumptionTechnologies = new HashSet<TechnologyPrototype>();
            fuelsEntities = new HashSet<EntityObjectBasePrototype>();
            plantOrigins = new HashSet<ItemPrototype>();
            spoilOrigins = new HashSet<ItemPrototype>();
            spoilageTimes = new Dictionary<Quality, double>();

            Weight = 0.01f;
            IngredientToWeightCoefficient = 1f;
            FuelValue = 1f; //useful for preventing overlow issues when using missing items / non-fuel items (loading with wrong mods / importing from alt mod group can cause this)
            PollutionMultiplier = 1f;
            IsMissing = isMissing;
        }

        public override string ToString() { return string.Format("Item: {0}", Name); }
    }
}