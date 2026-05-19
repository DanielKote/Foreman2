using Foreman.DataCaching.Loading;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace ForemanTest {
    [TestClass]
    public class PresetCraftingCompatibilityTests : ForemanTestBase {
        [TestMethod]
        public void CollectRecipeCraftingCategories_MergesExportedCategoryArrays() {
            var recipe = MinimalRecipeJson(
                primaryCategory: "chemistry",
                craftingCategories: ["chemistry", "advanced-chemistry"],
                additionalCategories: ["smelting"]);

            var categories = PresetCraftingCompatibility.CollectRecipeCraftingCategories(recipe, "chemistry").ToArray();
            ICollection<string> expected = ["chemistry", "advanced-chemistry", "smelting"];
            CollectionAssert.AreEquivalent(expected as ICollection, categories);
        }

        [TestMethod]
        public void CollectRecipeCraftingCategories_IncludesPrimaryAndAdditionalWhenNoCraftingCategoriesArray() {
            var recipe = MinimalRecipeJson(
                primaryCategory: "crafting",
                additionalCategories: ["advanced-crafting"]);

            var categories = PresetCraftingCompatibility.CollectRecipeCraftingCategories(recipe, "crafting").ToArray();
            ICollection<string> expected = ["crafting", "advanced-crafting"];
            CollectionAssert.AreEquivalent(expected as ICollection, categories);
        }

        private static JsonObject MinimalRecipeJson(
            string primaryCategory,
            string[]? craftingCategories = null,
            string[]? additionalCategories = null) {
            var recipe = new JsonObject {
                ["name"] = "test-recipe",
                ["localised_name"] = "Test",
                ["subgroup"] = "raw-resource",
                ["order"] = "a",
                ["category"] = primaryCategory,
                ["energy"] = 1,
                ["ingredients"] = new JsonArray(),
                ["products"] = new JsonArray(),
            };

            if (craftingCategories is { Length: > 0 })
                recipe["crafting_categories"] = new JsonArray(craftingCategories.Select(c => JsonValue.Create(c)).ToArray());
            if (additionalCategories is { Length: > 0 })
                recipe["additional_categories"] = new JsonArray(additionalCategories.Select(c => JsonValue.Create(c)).ToArray());

            return recipe;
        }
    }
}
