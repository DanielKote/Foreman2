using System.Collections.Generic;

namespace Foreman {
    /// <summary>Transient dictionaries used only while parsing a preset JSON document.</summary>
    internal sealed class PresetLoadSession {
        public Dictionary<string, List<RecipePrototype>> CraftingCategories { get; } = new();
        public Dictionary<string, List<ModulePrototype>> ModuleCategories { get; } = new();
        public Dictionary<string, List<RecipePrototype>> ResourceCategories { get; } = new() {
            ["<<foreman_resource_category_water_tile>>"] = new List<RecipePrototype>()
        };
        public Dictionary<string, List<ItemPrototype>> FuelCategories { get; } = new() {
            ["§§fc:liquids"] = new List<ItemPrototype>()
        };
        public Dictionary<Item, string> BurnResults { get; } = new();
        public Dictionary<Item, string> SpoilResults { get; } = new();
        public Dictionary<Quality, string> NextQualities { get; } = new();
        public List<Recipe> MiningWithFluidRecipes { get; } = new();
    }
}