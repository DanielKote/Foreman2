using Foreman.DataCaching.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Foreman.DataCaching.Loading {
    /// <summary>Preset JSON crafting categories and recipe↔machine linking helpers.</summary>
    internal static class PresetCraftingCompatibility {
        public static IEnumerable<string> CollectRecipeCraftingCategories(JsonNode recipeJson, string primaryCategory) {
            var categories = new HashSet<string>(StringComparer.Ordinal) { primaryCategory };
            foreach (string category in PresetJson.EnumerateStrings(recipeJson, "crafting_categories"))
                categories.Add(category);
            foreach (string category in PresetJson.EnumerateStrings(recipeJson, "additional_categories"))
                categories.Add(category);
            return categories;
        }

        public static Dictionary<string, HashSet<string>> BuildCraftingCategoryMachines(JsonObject jsonData) {
            var map = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

            if (jsonData["crafting_category_machines"] is JsonObject exported) {
                foreach (var property in exported) {
                    if (property.Key is not string category)
                        continue;
                    HashSet<string> names = GetOrCreate(map, category);
                    foreach (JsonNode entityNode in PresetJson.EnumerateArray(property.Value)) {
                        if (PresetJson.GetStringValue(entityNode) is string entityName)
                            names.Add(entityName);
                    }
                }
            }

            foreach (JsonNode entityNode in PresetJson.EnumerateArray(jsonData, "entities")) {
                if (PresetJson.GetString(entityNode, "name") is not string entityName)
                    continue;
                foreach (string category in PresetJson.EnumerateStrings(entityNode, "crafting_categories"))
                    GetOrCreate(map, category).Add(entityName);
            }

            return map;
        }

        public static void CopyCraftingCategoryMachines(
            JsonObject jsonData,
            Dictionary<string, HashSet<string>> target) {
            foreach (var entry in BuildCraftingCategoryMachines(jsonData))
                target[entry.Key] = entry.Value;
        }

        public static bool RecipeHasCraftingMachine(
            JsonNode recipeJson,
            string primaryCategory,
            IReadOnlyDictionary<string, HashSet<string>> craftingCategoryMachines) =>
            RecipeHasCraftingMachine(CollectRecipeCraftingCategories(recipeJson, primaryCategory), craftingCategoryMachines);

        public static bool RecipeHasCraftingMachine(
            IEnumerable<string> craftingCategories,
            IReadOnlyDictionary<string, HashSet<string>> craftingCategoryMachines) =>
            craftingCategories.Any(category =>
                craftingCategoryMachines.TryGetValue(category, out HashSet<string>? names) && names.Count > 0);

        /// <summary>After entity load: drop player assembler from machine-only recipes; flag craftability for post-load pruning.</summary>
        public static void FinalizeRecipeCraftingLinks(DataCacheStore store, PresetLoadSession session) {
            UnlinkPlayerAssemblerFromMachineOnlyRecipes(store);
            MarkRecipeCraftingMachineAvailability(store, session);
        }

        private static void UnlinkPlayerAssemblerFromMachineOnlyRecipes(DataCacheStore store) {
            if (store.PlayerAssembler is not AssemblerPrototype player)
                return;

            foreach (RecipePrototype recipe in store.Recipes.Values.OfType<RecipePrototype>()) {
                if (!recipe.HideFromPlayerCrafting)
                    continue;
                if (recipe.AssemblersInternal.Remove(player))
                    player.RecipesInternal.Remove(recipe);
            }
        }

        private static void MarkRecipeCraftingMachineAvailability(DataCacheStore store, PresetLoadSession session) {
            foreach (RecipePrototype recipe in store.Recipes.Values.OfType<RecipePrototype>())
                recipe.HasCraftingMachineInPreset = RecipeHasCraftingMachine(recipe.CraftingCategoryKeysInternal, session.CraftingCategoryMachines);
        }

        private static HashSet<string> GetOrCreate(Dictionary<string, HashSet<string>> map, string category) {
            if (!map.TryGetValue(category, out HashSet<string>? names)) {
                names = new HashSet<string>(StringComparer.Ordinal);
                map.Add(category, names);
            }
            return names;
        }
    }
}
