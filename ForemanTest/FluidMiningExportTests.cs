using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ForemanTest {
    /// <summary>
    /// Fluid-required mining (resource extraction with a fluid ingredient).
    /// Lua export must emit required_fluid as a string; loader must keep recipes available and miner-linked.
    /// </summary>
    [TestClass]
    public class FluidMiningExportTests : ForemanTestBase {
        private const string VanillaUraniumResource = "uranium-ore";
        private const string VanillaUraniumFluid = "sulfuric-acid";

        [TestInitialize]
        public void TestInitialize() {
            if (!VanillaDataCacheFixture.PresetsAvailable)
                Assert.Inconclusive($"Preset folder not found: {VanillaDataCacheFixture.PresetsDirectory}");
        }

        [TestMethod]
        public void VanillaPreset_UraniumOre_RequiredFluidIsExportedAsString() {
            JsonNode root = PresetProcessor.PrepPreset(new Preset(VanillaDataCacheFixture.PresetName, true, true));
            JsonNode? uranium = PresetJson.EnumerateArray(root, "resources")
                .FirstOrDefault(r => PresetJson.GetString(r, "name") == VanillaUraniumResource);

            Assert.IsNotNull(uranium, "Vanilla preset should define uranium-ore as a resource.");
            Assert.AreEqual(
                VanillaUraniumFluid,
                PresetJson.GetString(uranium, "required_fluid"),
                "required_fluid must be a JSON string (fluid name), not an object. Re-export with fixed foremanexport mod if null.");
            Assert.IsGreaterThan(
                0, PresetJson.GetDouble(uranium, "fluid_amount") ?? 0,
                "uranium-ore should declare fluid_amount.");
        }

        [TestMethod]
        public async Task Vanilla_UraniumExtraction_IncludesFluidIngredient() {
            DataCache cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            RecipePrototype recipe = ExtractionRecipeTestSupport.RequireExtractionRecipe(cache, VanillaUraniumResource);

            Assert.Contains(
                i => i.Name == VanillaUraniumFluid, recipe.IngredientListInternal,
                $"Uranium extraction should consume {VanillaUraniumFluid}. " +
                "If missing, preset JSON may lack required_fluid or loader skipped an unknown fluid name.");
        }

        [TestMethod]
        public async Task Vanilla_UraniumExtraction_IsAvailableForNodeSelection() {
            DataCache cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            RecipePrototype recipe = ExtractionRecipeTestSupport.RequireExtractionRecipe(cache, VanillaUraniumResource);

            Assert.IsTrue(
                recipe.Available,
                "Fluid-mining extraction should be Available so it appears in the recipe/node chooser.");
            Assert.IsNotEmpty(
                recipe.MyUnlockTechnologiesInternal,
                "Fluid-mining extraction should be linked to at least one unlock technology (typically via miner entity).");
        }

        [TestMethod]
        public async Task Vanilla_UraniumExtraction_IsLinkedToAMiner() {
            DataCache cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            RecipePrototype recipe = ExtractionRecipeTestSupport.RequireExtractionRecipe(cache, VanillaUraniumResource);

            Assert.IsNotEmpty(
                recipe.AssemblersInternal,
                "Uranium extraction should be assigned to at least one mining entity (electric miner, etc.).");
        }

        [TestMethod]
        public void PyanodonPreset_JsonListsResourcesWithRequiredFluid() {
            JsonObject root = PyanodonPresetTestSupport.LoadPreparedPresetJson();
            Assert.IsNotEmpty(
                PyanodonPresetTestSupport.EnumerateFluidMiningResources(root),
                "Pyanodon preset should include at least one resource with required_fluid after export.");
        }

        [TestMethod]
        public void PyanodonPreset_AllRequiredFluidNamesAreJsonStrings() {
            JsonObject root = PyanodonPresetTestSupport.LoadPreparedPresetJson();
            var bad = PresetJson.EnumerateArray(root, "resources")
                .Select(resource => (
                    Name: PresetJson.GetString(resource, "name"),
                    HasFluidNode: PresetJson.GetNode(resource, "required_fluid") is not null,
                    FluidIsString: PresetJson.GetString(resource, "required_fluid") is not null))
                .Where(x => x.HasFluidNode && !x.FluidIsString)
                .Select(x => x.Name ?? "(unnamed resource)")
                .ToList();

            PyanodonPresetTestSupport.AssertEmpty(
                bad,
                "These resources have required_fluid that is not a string (re-export with fixed foremanexport Lua)");
        }

        [TestMethod]
        public async Task Pyanodon_AllFluidMiningExtractions_HaveFluidIngredients() {
            var (root, cache) = await PyanodonPresetTestSupport.LoadPresetAndCacheAsync().ConfigureAwait(false);
            var expected = PyanodonPresetTestSupport.EnumerateFluidMiningResources(root).ToList();

            var missingRecipe = new List<string>();
            var missingIngredient = new List<string>();

            foreach ((string resourceName, string fluidName) in expected) {
                if (!ExtractionRecipeTestSupport.TryGetExtractionRecipe(cache, resourceName, out RecipePrototype? recipe)) {
                    missingRecipe.Add(resourceName);
                    continue;
                }
                if (!recipe!.IngredientListInternal.Any(i => i.Name == fluidName))
                    missingIngredient.Add($"{resourceName} (expected fluid {fluidName})");
            }

            AssertFluidMiningFailures(expected.Count,
                (missingRecipe, "No extraction recipe in cache"),
                (missingIngredient, "Extraction recipes missing fluid ingredients"));
        }

        [TestMethod]
        public async Task Pyanodon_AllFluidMiningExtractions_AreAvailableForNodeSelection() {
            var (root, cache) = await PyanodonPresetTestSupport.LoadPresetAndCacheAsync().ConfigureAwait(false);
            var expected = PyanodonPresetTestSupport.EnumerateFluidMiningResources(root).ToList();

            var missingRecipe = new List<string>();
            var unavailable = new List<string>();

            foreach ((string resourceName, _) in expected) {
                if (!ExtractionRecipeTestSupport.TryGetExtractionRecipe(cache, resourceName, out RecipePrototype? recipe)) {
                    missingRecipe.Add(resourceName);
                    continue;
                }
                if (!recipe!.Available || recipe.MyUnlockTechnologiesInternal.Count == 0)
                    unavailable.Add(resourceName);
            }

            AssertFluidMiningFailures(expected.Count,
                (missingRecipe, "No extraction recipe in cache"),
                (unavailable, "Recipe not Available or has no unlock technologies"));
        }

        [TestMethod]
        public async Task Pyanodon_AllFluidMiningExtractions_AreLinkedToMiners() {
            var (root, cache) = await PyanodonPresetTestSupport.LoadPresetAndCacheAsync().ConfigureAwait(false);
            var expected = PyanodonPresetTestSupport.EnumerateFluidMiningResources(root).ToList();

            var missingRecipe = new List<string>();
            var unlinked = new List<string>();

            foreach ((string resourceName, _) in expected) {
                if (!ExtractionRecipeTestSupport.TryGetExtractionRecipe(cache, resourceName, out RecipePrototype? recipe)) {
                    missingRecipe.Add(resourceName);
                    continue;
                }
                if (recipe!.AssemblersInternal.Count == 0)
                    unlinked.Add(resourceName);
            }

            AssertFluidMiningFailures(expected.Count,
                (missingRecipe, "No extraction recipe in cache"),
                (unlinked, "Recipes with no mining entities assigned"));
        }

        private static void AssertFluidMiningFailures(
            int expectedCount,
            params (List<string> Names, string Category)[] categories) {
            var lines = categories
                .Where(c => c.Names.Count > 0)
                .Select(c =>
                    $"{c.Category} ({c.Names.Count} of {expectedCount} fluid-mining resources): " +
                    string.Join(", ", c.Names.Take(40)) +
                    (c.Names.Count > 40 ? $" … and {c.Names.Count - 40} more" : ""))
                .ToList();

            Assert.IsEmpty(lines, string.Join(System.Environment.NewLine, lines));
        }
    }
}
