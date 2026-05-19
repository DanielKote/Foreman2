using System.Collections.Generic;
using System.Linq;

namespace Foreman.DataCaching.DataTypes {
    public interface IRecipe : IDataObjectBase {
        ISubgroup MySubgroup { get; }

        double Time { get; }
        long RecipeID { get; }
        bool IsMissing { get; }

        bool HasProductivityResearch { get; }

        bool AllowConsumptionBonus { get; }
        bool AllowSpeedBonus { get; }
        bool AllowProductivityBonus { get; }
        bool AllowPollutionBonus { get; }
        bool AllowQualityBonus { get; }

        double MaxProductivityBonus { get; }

        IReadOnlyDictionary<IItem, double> ProductSet { get; }
        IReadOnlyDictionary<IItem, double> ProductPSet { get; } //extra productivity amounts [ actual amount = productSet + (productPSet * productivity bonus) ]
        IReadOnlyList<IItem> ProductList { get; }
        IReadOnlyDictionary<IItem, double> ProductTemperatureMap { get; }

        IReadOnlyDictionary<IItem, double> IngredientSet { get; }
        IReadOnlyList<IItem> IngredientList { get; }
        IReadOnlyDictionary<IItem, FRange> IngredientTemperatureMap { get; }

        IReadOnlyCollection<IAssembler> Assemblers { get; }
        IReadOnlyCollection<IModule> AssemblerModules { get; }
        IReadOnlyCollection<IModule> BeaconModules { get; }

        IReadOnlyCollection<ITechnology> MyUnlockTechnologies { get; }
        IReadOnlyList<IReadOnlyList<IItem>> MyUnlockSciencePacks { get; }

        string GetIngredientFriendlyName(IItem item);
        string GetProductFriendlyName(IItem item);
        bool TestIngredientConnection(IRecipe provider, IItem ingredient);

        //trash items (spoiled items from spoiling of items already inside assembler) are ignored
        //planet conditions are ignored
    }

    public class RecipePrototype : DataObjectBasePrototype, IRecipe {
        public ISubgroup MySubgroup { get { return _mySubgroup; } }

        public double Time { get; internal set; }

        public IReadOnlyDictionary<IItem, double> ProductSet { get { return _productSet; } }
        public IReadOnlyDictionary<IItem, double> ProductPSet { get { return _productPSet; } }
        public IReadOnlyList<IItem> ProductList { get { return _productList; } }
        public IReadOnlyDictionary<IItem, double> ProductTemperatureMap { get { return _productTemperatureMap; } }

        public IReadOnlyDictionary<IItem, double> IngredientSet { get { return _ingredientSet; } }
        public IReadOnlyList<IItem> IngredientList { get { return _ingredientList; } }
        public IReadOnlyDictionary<IItem, FRange> IngredientTemperatureMap { get { return _ingredientTemperatureMap; } }

        public IReadOnlyCollection<IAssembler> Assemblers { get { return _assemblers; } }
        public IReadOnlyCollection<IModule> AssemblerModules { get { return _assemblerModules; } }
        public IReadOnlyCollection<IModule> BeaconModules { get { return _beaconModules; } }

        public IReadOnlyCollection<ITechnology> MyUnlockTechnologies { get { return _myUnlockTechnologies; } }
        public IReadOnlyList<IReadOnlyList<IItem>> MyUnlockSciencePacks { get; set; }

        private readonly SubgroupPrototype _mySubgroup;

        internal SubgroupPrototype MySubgroupInternal => _mySubgroup;

        private readonly Dictionary<IItem, double> _productSet;
        internal Dictionary<IItem, double> ProductSetInternal => _productSet;

        private readonly Dictionary<IItem, double> _productPSet;
        internal Dictionary<IItem, double> ProductPSetInternal => _productPSet;

        private readonly Dictionary<IItem, double> _productTemperatureMap;
        internal Dictionary<IItem, double> ProductTemperatureMapInternal => _productTemperatureMap;

        private readonly List<ItemPrototype> _productList;
        internal List<ItemPrototype> ProductListInternal => _productList;

        private readonly Dictionary<IItem, double> _ingredientSet;
        internal Dictionary<IItem, double> IngredientSetInternal => _ingredientSet;

        private readonly Dictionary<IItem, FRange> _ingredientTemperatureMap;
        internal Dictionary<IItem, FRange> IngredientTemperatureMapInternal => _ingredientTemperatureMap;

        private readonly List<ItemPrototype> _ingredientList;
        internal List<ItemPrototype> IngredientListInternal => _ingredientList;

        private readonly HashSet<AssemblerPrototype> _assemblers;
        internal HashSet<AssemblerPrototype> AssemblersInternal => _assemblers;

        private readonly HashSet<ModulePrototype> _assemblerModules;
        internal HashSet<ModulePrototype> AssemblerModulesInternal => _assemblerModules;

        private readonly HashSet<ModulePrototype> _beaconModules;
        internal HashSet<ModulePrototype> BeaconModulesInternal => _beaconModules;

        private readonly HashSet<TechnologyPrototype> _myUnlockTechnologies;
        internal HashSet<TechnologyPrototype> MyUnlockTechnologiesInternal => _myUnlockTechnologies;

        /// <summary>All categories this recipe belongs to (primary, additional, and exported crafting_categories).</summary>
        private readonly List<string> _craftingCategoryKeys = [];
        internal List<string> CraftingCategoryKeysInternal => _craftingCategoryKeys;

        public bool IsMissing { get; private set; }

        public bool AllowConsumptionBonus { get; internal set; }
        public bool AllowSpeedBonus { get; internal set; }
        public bool AllowProductivityBonus { get; internal set; }
        public bool AllowPollutionBonus { get; internal set; }
        public bool AllowQualityBonus { get; internal set; }

        public bool HasProductivityResearch { get; internal set; }

        public double MaxProductivityBonus { get; internal set; }

        private static long lastRecipeID;
        public long RecipeID { get; private set; }

        internal bool HideFromPlayerCrafting { get; set; }

        /// <summary>Factorio <c>recipe.hidden</c> from export — recipe starts disabled; user can enable via chooser.</summary>
        internal bool HiddenInGame { get; set; }

        /// <summary>Export lists at least one entity for one of this recipe's crafting categories.</summary>
        internal bool HasCraftingMachineInPreset { get; set; }

        public RecipePrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : base(dCache, name, friendlyName, order) {
            RecipeID = lastRecipeID++;

            _mySubgroup = subgroup;
            subgroup.RecipesInternal.Add(this);

            Time = 0.5f;
            this.Enabled = true;
            this.IsMissing = isMissing;
            this.HideFromPlayerCrafting = false;
            this.AllowConsumptionBonus = true;
            this.AllowSpeedBonus = true;
            this.AllowProductivityBonus = true;
            this.AllowPollutionBonus = true;
            this.AllowQualityBonus = true;
            this.MaxProductivityBonus = 1000;
            this.HasProductivityResearch = false;

            _ingredientSet = [];
            _ingredientList = [];
            _ingredientTemperatureMap = [];

            _productSet = [];
            _productList = [];
            _productTemperatureMap = [];
            _productPSet = [];

            _assemblers = [];
            _assemblerModules = [];
            _beaconModules = [];
            _myUnlockTechnologies = [];
            MyUnlockSciencePacks = new List<List<IItem>>();
        }

        public string GetIngredientFriendlyName(IItem item) {
            return IngredientSet.ContainsKey(item) && (item is IFluid fluid) && fluid.IsTemperatureDependent
                ? fluid.GetTemperatureRangeFriendlyName(IngredientTemperatureMap[item])
                : item.FriendlyName;
        }

        public string GetProductFriendlyName(IItem item) {
            return _productSet.ContainsKey(item) && (item is IFluid fluid) && (fluid.IsTemperatureDependent || fluid.DefaultTemperature != ProductTemperatureMap[item])
                ? fluid.GetTemperatureFriendlyName(_productTemperatureMap[item])
                : item.FriendlyName;
        }

        public bool TestIngredientConnection(IRecipe provider, IItem ingredient) //checks if the temperature that the ingredient is coming out at fits within the range of temperatures required for this recipe
        {
            return IngredientSet.ContainsKey(ingredient) && provider.ProductSet.ContainsKey(ingredient) && IngredientTemperatureMap[ingredient].Contains(provider.ProductTemperatureMap[ingredient]);
        }

        public void InternalOneWayAddIngredient(ItemPrototype item, double quantity, double minTemp = double.NaN, double maxTemp = double.NaN) {
            if (_ingredientSet.TryGetValue(item, out var existing))
                _ingredientSet[item] = existing + quantity;
            else {
                _ingredientSet.Add(item, quantity);
                _ingredientList.Add(item);

                minTemp = (item is IFluid && double.IsNaN(minTemp) ? double.NegativeInfinity : minTemp);
                maxTemp = (item is IFluid && double.IsNaN(maxTemp) ? double.PositiveInfinity : maxTemp);
                _ingredientTemperatureMap.Add(item, new FRange(minTemp, maxTemp));
            }
        }

        internal void InternalOneWayDeleteIngredient(ItemPrototype item) //only from delete calls
        {
            _ingredientSet.Remove(item);
            _ingredientList.Remove(item);
            _ingredientTemperatureMap.Remove(item);
        }

        public void InternalOneWayAddProduct(ItemPrototype item, double quantity, double pquantity, double temperature = double.NaN) {
            if (_productSet.ContainsKey(item)) {
                _productSet[item] += quantity;
                _productPSet[item] += pquantity;
            } else {
                _productSet.Add(item, quantity);
                _productPSet.Add(item, pquantity);
                _productList.Add(item);

                temperature = (item is IFluid fluid && double.IsNaN(temperature)) ? fluid.DefaultTemperature : temperature;
                _productTemperatureMap.Add(item, temperature);
            }
        }

        internal void InternalOneWayDeleteProduct(ItemPrototype item) //only from delete calls
        {
            _productSet.Remove(item);
            _productPSet.Remove(item);
            _productList.Remove(item);
            _productTemperatureMap.Remove(item);
        }

        public override string ToString() { return string.Format(CultureInfo.InvariantCulture, "Recipe: {0} Id:{1}", Name, RecipeID); }
    }

    public class RecipeNaInPrComparer : IEqualityComparer<IRecipe> //compares by name, ingredient names, and product names
    {
        public bool Equals(IRecipe? x, IRecipe? y) {
            return ReferenceEquals(x, y) ||
                x == y ||
                x?.Name == y?.Name &&
                x?.IngredientList.Count == y?.IngredientList.Count &&
                x?.ProductList.Count == y?.ProductList.Count &&
                x?.IngredientList.All(i => y?.IngredientSet.ContainsKey(i) is true) is true &&
                x?.ProductList.All(p => y?.ProductSet.ContainsKey(p) is true) is true;
        }

        public int GetHashCode(IRecipe obj) {
            return obj.GetHashCode();
        }
    }

    public record struct FRange(double Min, double Max, bool Ignore = false) {
        //NOTE: there is no check for min to be guaranteed to be less than max, and this is BY DESIGN
        //this means that if your range is for example from 10 to 8, (and it isnt ignored), ANY call to Contains methods will return false
        //ex: 2 recipes, one requiring fluid 0->10 degrees, other requiring fluid 20->30 degrees. A proper summation of ranges will result in a vaild range of 20->10 degrees to satisfy both recipes, aka: NO TEMP WILL SATISFY!
        public readonly bool Contains(double value) => Ignore || double.IsNaN(value) || ((double.IsNaN(Min) || value >= Min) && (double.IsNaN(Max) || value <= Max));
        public readonly bool Contains(FRange range) => Ignore || range.Ignore || ((double.IsNaN(this.Min) || double.IsNaN(range.Min) || this.Min <= range.Min) && (double.IsNaN(this.Max) || double.IsNaN(range.Max) || this.Max >= range.Max));
        public readonly bool IsPoint() => Ignore || Min == Max; //true if the range is a single point (min is max, and we arent ignoring it)
    }
}
