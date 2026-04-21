using System;
using System.Collections.Generic;
using System.Linq;

namespace Foreman {
    // trash items (spoiled items from spoiling of items already inside assembler) are ignored
    // planet conditions are ignored
    public interface Recipe : DataObjectBase {
        Subgroup MySubgroup { get; }

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

        IReadOnlyDictionary<Item, double> ProductSet { get; }
        // extra productivity amounts [actual amount = productSet + (productPSet * productivity bonus)]
        IReadOnlyDictionary<Item, double> ProductPSet { get; }
        IReadOnlyList<Item> ProductList { get; }
        IReadOnlyDictionary<Item, double> ProductTemperatureMap { get; }

        IReadOnlyDictionary<Item, double> IngredientSet { get; }
        IReadOnlyList<Item> IngredientList { get; }
        IReadOnlyDictionary<Item, FRange> IngredientTemperatureMap { get; }

        IReadOnlyCollection<Assembler> Assemblers { get; }
        IReadOnlyCollection<Module> AssemblerModules { get; }
        IReadOnlyCollection<Module> BeaconModules { get; }

        IReadOnlyCollection<Technology> MyUnlockTechnologies { get; }
        IReadOnlyList<IReadOnlyList<Item>> MyUnlockSciencePacks { get; }

        string GetIngredientFriendlyName(Item item);
        string GetProductFriendlyName(Item item);
        bool TestIngredientConnection(Recipe provider, Item ingredient);
    }

    public class RecipePrototype : DataObjectBasePrototype, Recipe {
        public Subgroup MySubgroup => mySubgroup;

        public double Time { get; internal set; }

        public IReadOnlyDictionary<Item, double> ProductSet => productSet;

        public IReadOnlyDictionary<Item, double> ProductPSet => productPSet;

        public IReadOnlyList<Item> ProductList => productList;

        public IReadOnlyDictionary<Item, double> ProductTemperatureMap => productTemperatureMap;

        public IReadOnlyDictionary<Item, double> IngredientSet => ingredientSet;

        public IReadOnlyList<Item> IngredientList => ingredientList;

        public IReadOnlyDictionary<Item, FRange> IngredientTemperatureMap => ingredientTemperatureMap;

        public IReadOnlyCollection<Assembler> Assemblers => assemblers;

        public IReadOnlyCollection<Module> AssemblerModules => assemblerModules;

        public IReadOnlyCollection<Module> BeaconModules => beaconModules;

        public IReadOnlyCollection<Technology> MyUnlockTechnologies => myUnlockTechnologies;

        public IReadOnlyList<IReadOnlyList<Item>> MyUnlockSciencePacks { get; set; }

        internal SubgroupPrototype mySubgroup;

        internal Dictionary<Item, double> productSet { get; private set; }
        internal Dictionary<Item, double> productPSet { get; private set; }
        internal Dictionary<Item, double> productTemperatureMap { get; private set; }
        internal List<ItemPrototype> productList { get; private set; }

        internal Dictionary<Item, double> ingredientSet { get; private set; }
        internal Dictionary<Item, FRange> ingredientTemperatureMap { get; private set; }
        internal List<ItemPrototype> ingredientList { get; private set; }

        internal HashSet<AssemblerPrototype> assemblers { get; private set; }
        internal HashSet<ModulePrototype> assemblerModules { get; private set; }
        internal HashSet<ModulePrototype> beaconModules { get; private set; }

        internal HashSet<TechnologyPrototype> myUnlockTechnologies { get; private set; }

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

        public RecipePrototype(DataCache dCache, string name, string friendlyName, SubgroupPrototype subgroup, string order, bool isMissing = false) : base(
            dCache, name, friendlyName, order) {
            RecipeID = lastRecipeID++;

            mySubgroup = subgroup;
            subgroup.recipes.Add(this);

            Time = 0.5f;
            Enabled = true;
            IsMissing = isMissing;
            HideFromPlayerCrafting = false;
            AllowConsumptionBonus = true;
            AllowSpeedBonus = true;
            AllowProductivityBonus = true;
            AllowPollutionBonus = true;
            AllowQualityBonus = true;
            MaxProductivityBonus = 1000;
            HasProductivityResearch = false;

            ingredientSet = new Dictionary<Item, double>();
            ingredientList = [];
            ingredientTemperatureMap = new Dictionary<Item, FRange>();

            productSet = new Dictionary<Item, double>();
            productList = [];
            productTemperatureMap = new Dictionary<Item, double>();
            productPSet = new Dictionary<Item, double>();

            assemblers = [];
            assemblerModules = [];
            beaconModules = [];
            myUnlockTechnologies = [];
            MyUnlockSciencePacks = new List<List<Item>>();
        }

        public string GetIngredientFriendlyName(Item item) {
            if (IngredientSet.ContainsKey(item) && item is Fluid { IsTemperatureDependent: true } fluid)
                return fluid.GetTemperatureRangeFriendlyName(IngredientTemperatureMap[item]);
            return item.FriendlyName;
        }

        public string GetProductFriendlyName(Item item) {
            if (productSet.ContainsKey(item) && item is Fluid fluid &&
                (fluid.IsTemperatureDependent || Math.Abs(fluid.DefaultTemperature - ProductTemperatureMap[item]) > double.Epsilon))
                return fluid.GetTemperatureFriendlyName(productTemperatureMap[item]);
            return item.FriendlyName;
        }

        // checks if the temperature that the ingredient is coming out at fits within the range of temperatures required for this recipe
        public bool TestIngredientConnection(Recipe provider, Item ingredient) {
            if (!IngredientSet.ContainsKey(ingredient) || !provider.ProductSet.ContainsKey(ingredient))
                return false;

            return IngredientTemperatureMap[ingredient].Contains(provider.ProductTemperatureMap[ingredient]);
        }

        public void InternalOneWayAddIngredient(ItemPrototype item, double quantity, double minTemp = double.NaN, double maxTemp = double.NaN) {
            if (IngredientSet.ContainsKey(item))
                ingredientSet[item] += quantity;
            else {
                ingredientSet.Add(item, quantity);
                ingredientList.Add(item);

                minTemp = item is Fluid && double.IsNaN(minTemp) ? double.NegativeInfinity : minTemp;
                maxTemp = item is Fluid && double.IsNaN(maxTemp) ? double.PositiveInfinity : maxTemp;
                ingredientTemperatureMap.Add(item, new FRange(minTemp, maxTemp));
            }
        }

        // only from delete calls
        internal void InternalOneWayDeleteIngredient(ItemPrototype item) {
            ingredientSet.Remove(item);
            ingredientList.Remove(item);
            ingredientTemperatureMap.Remove(item);
        }

        public void InternalOneWayAddProduct(ItemPrototype item, double quantity, double pQuantity, double temperature = double.NaN) {
            if (productSet.ContainsKey(item)) {
                productSet[item] += quantity;
                productPSet[item] += pQuantity;
            } else {
                productSet.Add(item, quantity);
                productPSet.Add(item, pQuantity);
                productList.Add(item);

                temperature = item is Fluid fluid && double.IsNaN(temperature) ? fluid.DefaultTemperature : temperature;
                productTemperatureMap.Add(item, temperature);
            }
        }

        // only from delete calls
        internal void InternalOneWayDeleteProduct(ItemPrototype item) {
            productSet.Remove(item);
            productPSet.Remove(item);
            productList.Remove(item);
            productTemperatureMap.Remove(item);
        }

        public override string ToString() {
            return $"Recipe: {Name} Id:{RecipeID}";
        }
    }

    // compares by name, ingredient names, and product names
    public class RecipeNaInPrComparer : IEqualityComparer<Recipe> {
        public bool Equals(Recipe x, Recipe y) {
            if (x == y)
                return true;

            if (x.Name != y.Name)
                return false;
            if (x.IngredientList.Count != y.IngredientList.Count)
                return false;
            if (x.ProductList.Count != y.ProductList.Count)
                return false;

            return x.IngredientList.All(i => y.IngredientSet.ContainsKey(i))
                && x.ProductList.All(i => y.ProductSet.ContainsKey(i));
        }

        public int GetHashCode(Recipe obj) {
            return obj.GetHashCode();
        }
    }

    // NOTE: there is no check for min to be guaranteed to be less than max, and this is BY DESIGN
    // this means that if your range is for example from 10 to 8, (and it isn't ignored), ANY call to Contains methods will return false
    // ex: 2 recipes, one requiring fluid 0->10 degrees, other requiring fluid 20->30 degrees.
    // A proper summation of ranges will result in a valid range of 20->10 degrees to satisfy both recipes, aka: NO TEMP WILL SATISFY!
    public struct FRange(double min, double max, bool ignore = false) {
        public double Min = min;
        public double Max = max;
        public bool Ignore = ignore;

        public bool Contains(double value) {
            return Ignore || double.IsNaN(value) || ((double.IsNaN(Min) || value >= Min) && (double.IsNaN(Max) || value <= Max));
        }

        public bool Contains(FRange range) {
            return Ignore || range.Ignore || ((double.IsNaN(Min) || double.IsNaN(range.Min) || Min <= range.Min) &&
                (double.IsNaN(Max) || double.IsNaN(range.Max) || Max >= range.Max));
        }

        // true if the range is a single point (min is max, and we aren't ignoring it)
        public bool IsPoint() {
            return Ignore || Math.Abs(Min - Max) < double.Epsilon;
        }
    }
}