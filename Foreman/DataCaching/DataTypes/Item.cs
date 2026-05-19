using System.Collections.Generic;

namespace Foreman.DataCaching.DataTypes {
    public interface IItem : IDataObjectBase {

        ISubgroup MySubgroup { get; }

        IReadOnlyCollection<IRecipe> ProductionRecipes { get; }
        IReadOnlyCollection<IRecipe> ConsumptionRecipes { get; }
        IReadOnlyCollection<ITechnology> ConsumptionTechnologies { get; }

        bool IsMissing { get; }

        int StackSize { get; }

        double Weight { get; }
        double IngredientToWeightCoefficient { get; }
        double FuelValue { get; }
        double PollutionMultiplier { get; }

        IItem? BurnResult { get; }
        IPlantProcess? PlantResult { get; }
        IItem? SpoilResult { get; }

        IItem? FuelOrigin { get; }
        IReadOnlyCollection<IItem> PlantOrigins { get; }
        IReadOnlyCollection<IItem> SpoilOrigins { get; }

        double GetItemSpoilageTime(IQuality quality); //seconds

        IReadOnlyCollection<IEntityObjectBase> FuelsEntities { get; }

        //spoil ticks are ignored - its assumed that if there is a plant/spoil result then the ticks are at least low enough to make it viable on a world basis
    }

    public class ItemPrototype : DataObjectBasePrototype, IItem {
        public class Test { }

        public ISubgroup MySubgroup { get { return _mySubgroup; } }

        public IReadOnlyCollection<IRecipe> ProductionRecipes { get { return _productionRecipes; } }
        public IReadOnlyCollection<IRecipe> ConsumptionRecipes { get { return _consumptionRecipes; } }
        public IReadOnlyCollection<ITechnology> ConsumptionTechnologies { get { return _consumptionTechnologies; } }

        public bool IsMissing { get; private set; }

        public int StackSize { get; set; }

        public double Weight { get; set; }
        public double IngredientToWeightCoefficient { get; set; }
        public double FuelValue { get; internal set; }
        public double PollutionMultiplier { get; internal set; }

        public IItem? BurnResult { get; internal set; }
        public IPlantProcess? PlantResult { get; internal set; }
        public IItem? SpoilResult { get; internal set; }

        public IItem? FuelOrigin { get; internal set; }
        public IReadOnlyCollection<IItem> PlantOrigins { get { return _plantOrigins; } }
        public IReadOnlyCollection<IItem> SpoilOrigins { get { return _spoilOrigins; } }

        public IReadOnlyCollection<IEntityObjectBase> FuelsEntities { get { return _fuelsEntities; } }

        public double GetItemSpoilageTime(IQuality quality) => _spoilageTimes.TryGetValue(quality, out double spoilTime) ? spoilTime : 1;

        private readonly SubgroupPrototype _mySubgroup;
        internal SubgroupPrototype MySubgroupInternal => _mySubgroup;

        private readonly HashSet<RecipePrototype> _productionRecipes;
        internal HashSet<RecipePrototype> ProductionRecipesInternal => _productionRecipes;

        private readonly HashSet<RecipePrototype> _consumptionRecipes;
        internal HashSet<RecipePrototype> ConsumptionRecipesInternal => _consumptionRecipes;

        private readonly HashSet<TechnologyPrototype> _consumptionTechnologies;
        internal HashSet<TechnologyPrototype> ConsumptionTechnologiesInternal => _consumptionTechnologies;

        private readonly HashSet<EntityObjectBasePrototype> _fuelsEntities;
        internal HashSet<EntityObjectBasePrototype> FuelsEntitiesInternal => _fuelsEntities;

        private readonly HashSet<ItemPrototype> _plantOrigins;
        internal HashSet<ItemPrototype> PlantOriginsInternal => _plantOrigins;

        private readonly HashSet<ItemPrototype> _spoilOrigins;
        internal HashSet<ItemPrototype> SpoilOriginsInternal => _spoilOrigins;

        private readonly Dictionary<IQuality, double> _spoilageTimes;
        internal Dictionary<IQuality, double> spoilageTimes => _spoilageTimes;

        public ItemPrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : base(dCache, name, friendlyName, order) {
            _mySubgroup = subgroup;
            subgroup.ItemsInternal.Add(this);

            StackSize = 1;

            _productionRecipes = [];
            _consumptionRecipes = [];
            _consumptionTechnologies = [];
            _fuelsEntities = [];
            _plantOrigins = [];
            _spoilOrigins = [];
            _spoilageTimes = [];

            Weight = 0.01f;
            IngredientToWeightCoefficient = 1f;
            FuelValue = 1f; //useful for preventing overlow issues when using missing items / non-fuel items (loading with wrong mods / importing from alt mod group can cause this)
            PollutionMultiplier = 1f;
            IsMissing = isMissing;
        }

        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "Item: {0}", Name); }
    }
}
