using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.DataCaching.Loading;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json.Nodes;

namespace ForemanTest {
    [TestClass]
    public class PresetMissingSubgroupTests : ForemanTestBase {
        [TestMethod]
        public void PresetDataLoader_ItemWithoutSubgroup_UsesMissingSubgroup() {
            var cache = new DataCache(filterRecipes: false);
            DataCacheStore store = TestDataCacheHelper.RequireStore(cache);
            Assert.IsNotNull(store.MissingSubgroup);

            var loader = new PresetDataLoader(cache, store, new PresetLoadSession());
            var itemJson = new JsonObject {
                ["name"] = "item-cam",
                ["localised_name"] = "Item Cam",
                ["order"] = "z[item-cam]",
                ["stack_size"] = 1,
                ["weight"] = 1,
                ["ingredient_to_weight_coefficient"] = 1
            };

            loader.ProcessItem(itemJson, []);

            Assert.IsTrue(cache.Items.TryGetValue("item-cam", out IItem? item));
            Assert.AreSame(store.MissingSubgroup, item!.MySubgroup);
        }

        [TestMethod]
        public void PresetDataLoader_RecipeWithoutSubgroup_UsesMissingSubgroup() {
            var cache = new DataCache(filterRecipes: false);
            DataCacheStore store = TestDataCacheHelper.RequireStore(cache);
            TestDataCacheHelper.GetOrCreateItem(cache, store.MissingSubgroup!, "dummy-product");

            var loader = new PresetDataLoader(cache, store, new PresetLoadSession());
            var recipeJson = new JsonObject {
                ["name"] = "test-recipe-no-sg",
                ["localised_name"] = "Test",
                ["order"] = "a",
                ["category"] = "crafting",
                ["energy"] = 1,
                ["products"] = new JsonArray {
                    new JsonObject { ["name"] = "dummy-product", ["type"] = "item", ["amount"] = 1, ["p_amount"] = 1 }
                },
                ["ingredients"] = new JsonArray()
            };

            loader.ProcessRecipe(recipeJson, []);

            Assert.IsTrue(cache.Recipes.TryGetValue("test-recipe-no-sg", out IRecipe? recipe));
            Assert.AreSame(store.MissingSubgroup, recipe!.MySubgroup);
        }
    }
}
