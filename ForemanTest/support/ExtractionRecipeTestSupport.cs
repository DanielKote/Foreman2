using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ForemanTest.support {
    internal static class ExtractionRecipeTestSupport {
        public static string ExtractionRecipeName(string resourceItemName) =>
            PresetExportTestConstants.ExtractionRecipePrefix + resourceItemName;

        public static bool TryGetExtractionRecipe(
            DataCache cache,
            string resourceItemName,
            out RecipePrototype? recipe) {
            recipe = null;
            if (!cache.Recipes.TryGetValue(ExtractionRecipeName(resourceItemName), out IRecipe? found) ||
                found is not RecipePrototype prototype)
                return false;

            recipe = prototype;
            return true;
        }

        public static RecipePrototype RequireExtractionRecipe(DataCache cache, string resourceItemName) {
            Assert.IsTrue(
                TryGetExtractionRecipe(cache, resourceItemName, out RecipePrototype? recipe),
                $"Missing extraction recipe {ExtractionRecipeName(resourceItemName)}.");
            return recipe!;
        }
    }
}
