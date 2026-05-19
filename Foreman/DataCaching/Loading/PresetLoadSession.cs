using Foreman.DataCaching.DataTypes;
using System;
using System.Collections.Generic;

namespace Foreman.DataCaching.Loading {
    /// <summary>Transient dictionaries used only while parsing a preset JSON document.</summary>
    internal sealed class PresetLoadSession {
        public Dictionary<string, List<RecipePrototype>> CraftingCategories { get; } = [];
        public Dictionary<string, List<ModulePrototype>> ModuleCategories { get; } = [];
        public Dictionary<string, List<RecipePrototype>> ResourceCategories { get; } = new() {
            ["<<foreman_resource_category_water_tile>>"] = []
        };
        public Dictionary<string, List<ItemPrototype>> FuelCategories { get; } = new() {
            ["§§fc:liquids"] = []
        };
        public Dictionary<IItem, string> BurnResults { get; } = [];
        public Dictionary<IItem, string> SpoilResults { get; } = [];
        public Dictionary<IQuality, string> NextQualities { get; } = [];
        public List<IRecipe> MiningWithFluidRecipes { get; } = [];

        /// <summary>IRecipe category → entity names that can craft it (from export or entity crafting_categories).</summary>
        public Dictionary<string, HashSet<string>> CraftingCategoryMachines { get; } = new(StringComparer.Ordinal);
    }
}
