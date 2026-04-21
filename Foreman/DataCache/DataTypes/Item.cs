using System.Collections.Generic;

namespace Foreman {
    // spoil ticks are ignored - its assumed that
    // if there is a plant/spoil result then the ticks are at least low enough to make it viable on a world basis
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

        Item BurnResult { get; }
        PlantProcess PlantResult { get; }
        Item SpoilResult { get; }

        Item FuelOrigin { get; }
        IReadOnlyCollection<Item> PlantOrigins { get; }
        IReadOnlyCollection<Item> SpoilOrigins { get; }

        // seconds
        double GetItemSpoilageTime(Quality quality);

        IReadOnlyCollection<EntityObjectBase> FuelsEntities { get; }
    }

    public class ItemPrototype : DataObjectBasePrototype, Item {
        public Subgroup MySubgroup => mySubgroup;

        public IReadOnlyCollection<Recipe> ProductionRecipes => productionRecipes;

        public IReadOnlyCollection<Recipe> ConsumptionRecipes => consumptionRecipes;

        public IReadOnlyCollection<Technology> ConsumptionTechnologies => consumptionTechnologies;

        public bool IsMissing { get; private set; }

        public int StackSize { get; set; }

        public double Weight { get; set; }
        public double IngredientToWeightCoefficient { get; set; }
        public double FuelValue { get; internal set; }
        public double PollutionMultiplier { get; internal set; }

        public Item BurnResult { get; internal set; }
        public PlantProcess PlantResult { get; internal set; }
        public Item SpoilResult { get; internal set; }

        public Item FuelOrigin { get; internal set; }

        public IReadOnlyCollection<Item> PlantOrigins => plantOrigins;

        public IReadOnlyCollection<Item> SpoilOrigins => spoilOrigins;

        public IReadOnlyCollection<EntityObjectBase> FuelsEntities => fuelsEntities;

        public double GetItemSpoilageTime(Quality quality) {
            return spoilageTimes.TryGetValue(quality, out var time) ? time : 1;
        }

        internal SubgroupPrototype mySubgroup;

        internal HashSet<RecipePrototype> productionRecipes { get; private set; }
        internal HashSet<RecipePrototype> consumptionRecipes { get; private set; }
        internal HashSet<TechnologyPrototype> consumptionTechnologies { get; private set; }
        internal HashSet<EntityObjectBasePrototype> fuelsEntities { get; private set; }
        internal HashSet<ItemPrototype> plantOrigins { get; private set; }
        internal HashSet<ItemPrototype> spoilOrigins { get; private set; }

        internal Dictionary<Quality, double> spoilageTimes { get; private set; }

        public ItemPrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : base(
            dCache, name, friendlyName, order) {
            mySubgroup = subgroup;
            subgroup.items.Add(this);

            StackSize = 1;

            productionRecipes = [];
            consumptionRecipes = [];
            consumptionTechnologies = [];
            fuelsEntities = [];
            plantOrigins = [];
            spoilOrigins = [];
            spoilageTimes = new Dictionary<Quality, double>();

            Weight = 0.01f;
            IngredientToWeightCoefficient = 1f;
            // useful for preventing overlow issues when using missing items / non-fuel items
            // (loading with wrong mods / importing from alt mod group can cause this)
            FuelValue = 1f;
            PollutionMultiplier = 1f;
            IsMissing = isMissing;
        }

        public override string ToString() {
            return $"Item: {Name}";
        }
    }
}