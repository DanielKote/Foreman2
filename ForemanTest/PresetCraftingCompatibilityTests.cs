using Foreman;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            CollectionAssert.AreEquivalent(
                new[] { "chemistry", "advanced-chemistry", "smelting" },
                categories);
        }

        [TestMethod]
        public void CollectRecipeCraftingCategories_IncludesPrimaryAndAdditionalWhenNoCraftingCategoriesArray() {
            var recipe = MinimalRecipeJson(
                primaryCategory: "crafting",
                additionalCategories: ["advanced-crafting"]);

            var categories = PresetCraftingCompatibility.CollectRecipeCraftingCategories(recipe, "crafting").ToArray();

            CollectionAssert.AreEquivalent(new[] { "crafting", "advanced-crafting" }, categories);
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