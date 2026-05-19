using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.DataCaching.Loading;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ForemanTest {
    /// <summary>Diagnostic audit: preset JSON vs loaded DataCache (writes report to test output).</summary>
    [TestClass]
    public class PyanodonPresetCoverageAuditTests : ForemanTestBase {
        [TestMethod]
        public async Task Pyanodon_Audit_WritePresetVsCacheReport() {
            var snapshot = await PyanodonPresetTestSupport.LoadSnapshotAsync().ConfigureAwait(false);

            WriteSectionCounts(snapshot.Root, snapshot.Cache);
            WriteRecipeCoverage(snapshot.Root, snapshot.Cache);
            WriteResourceCoverage(snapshot.Root, snapshot.Cache);
            WriteAvailabilityBreakdown(snapshot.Cache);
            WriteBarrelFilterImpact(snapshot.Root, snapshot.Cache);
            WriteChooserRelevantGaps(snapshot.Root, snapshot.Cache);

            Assert.IsNotEmpty(snapshot.Cache.Recipes, "Audit requires a loaded cache.");
        }

        private static void WriteSectionCounts(JsonObject root, DataCache cache) {
            Console.WriteLine("=== Preset JSON array sizes vs DataCache ===");
            Console.WriteLine($"mods:           json={CountArray(root, "mods")}  cache_mods={cache.IncludedMods.Count}");
            Console.WriteLine($"groups:         json={CountArray(root, "groups")}  cache={cache.Groups.Count}");
            Console.WriteLine($"subgroups:      json={CountArray(root, "subgroups")}  cache={cache.Subgroups.Count}");
            Console.WriteLine($"qualities:      json={CountArray(root, "qualities")}  cache={cache.Qualities.Count}");
            Console.WriteLine($"fluids:         json={CountArray(root, "fluids")}  cache_fluids={cache.Items.Values.Count(i => i is IFluid)}");
            Console.WriteLine($"items:          json={CountArray(root, "items")}  cache_items={cache.Items.Count}");
            Console.WriteLine($"modules:        json={CountArray(root, "modules")}  cache={cache.Modules.Count}");
            Console.WriteLine($"recipes:        json={CountArray(root, "recipes")}  cache={cache.Recipes.Count}");
            Console.WriteLine($"resources:      json={CountArray(root, "resources")}");
            Console.WriteLine($"water_resources:json={CountArray(root, "water_resources")}");
            Console.WriteLine($"technologies:   json={CountArray(root, "technologies")}  cache={cache.Technologies.Count}");
            Console.WriteLine($"assemblers:     cache={cache.Assemblers.Count}  beacons={cache.Beacons.Count}");
        }

        private static void WriteRecipeCoverage(JsonObject root, DataCache cache) {
            var jsonRecipeNames = PresetJson.EnumerateArray(root, "recipes")
                .Select(r => PresetJson.GetString(r, "name"))
                .Where(n => n is not null)
                .Cast<string>()
                .ToHashSet(StringComparer.Ordinal);

            int jsonExtraction = PyanodonPresetTestSupport.EnumerateAllResources(root).Count();
            int cacheExtraction = cache.Recipes.Keys.Count(k =>
                k.StartsWith(PresetExportTestConstants.ExtractionRecipePrefix, StringComparison.Ordinal));

            var missingFromCache = jsonRecipeNames
                .Where(n => !cache.Recipes.ContainsKey(n))
                .OrderBy(n => n)
                .ToList();

            int available = cache.Recipes.Values.Count(r => r.Available);
            int unavailable = cache.Recipes.Count - available;
            int noAssembler = cache.Recipes.Values
                .OfType<RecipePrototype>()
                .Count(r => r.AssemblersInternal.Count == 0);

            Console.WriteLine();
            Console.WriteLine("=== Recipes ===");
            Console.WriteLine($"JSON crafting recipes: {jsonRecipeNames.Count}");
            Console.WriteLine($"JSON resource slots:   {jsonExtraction} (each should become {PresetExportTestConstants.ExtractionRecipePrefix}…)");
            Console.WriteLine($"Cache extraction:      {cacheExtraction}");
            Console.WriteLine($"Cache total recipes:   {cache.Recipes.Count} (includes generated burn/spoil/rocket/etc.)");
            Console.WriteLine($"Available:             {available}");
            Console.WriteLine($"Unavailable:           {unavailable}");
            Console.WriteLine($"Still in cache w/ 0 assemblers: {noAssembler}");
            Console.WriteLine($"JSON recipes not in cache: {missingFromCache.Count}");
            SampleLines("  missing crafting", missingFromCache, 25);
        }

        private static void WriteResourceCoverage(JsonObject root, DataCache cache) {
            var fluidMining = PyanodonPresetTestSupport.EnumerateFluidMiningResources(root).ToList();
            var missingExtraction = new List<string>();
            var unavailableExtraction = new List<string>();

            foreach (JsonNode resource in PyanodonPresetTestSupport.EnumerateAllResources(root)) {
                if (PresetJson.GetString(resource, "name") is not string name)
                    continue;

                if (!cache.Recipes.TryGetValue(ExtractionRecipeTestSupport.ExtractionRecipeName(name), out IRecipe? recipe)) {
                    missingExtraction.Add(name);
                    continue;
                }
                if (!recipe.Available)
                    unavailableExtraction.Add(name);
            }

            Console.WriteLine();
            Console.WriteLine("=== Resources / extraction ===");
            Console.WriteLine($"JSON resources + water: {PyanodonPresetTestSupport.EnumerateAllResources(root).Count()}");
            Console.WriteLine($"  with required_fluid:  {fluidMining.Count}");
            Console.WriteLine($"Missing extraction in cache: {missingExtraction.Count}");
            SampleLines("  missing", missingExtraction, 20);
            Console.WriteLine($"Extraction present but unavailable: {unavailableExtraction.Count}");
            SampleLines("  unavailable", unavailableExtraction, 20);
        }

        private static void WriteAvailabilityBreakdown(DataCache cache) {
            var unavailable = cache.Recipes.Values
                .OfType<RecipePrototype>()
                .Where(r => !r.Available)
                .ToList();

            int noUnlockTech = unavailable.Count(r => r.MyUnlockTechnologiesInternal.Count == 0);
            int noAssembler = unavailable.Count(r => r.AssemblersInternal.Count == 0);
            int barrelLike = unavailable.Count(r => DataCacheRecipeFilters.BlackList.Any(rx => rx.IsMatch(r.Name)));

            Console.WriteLine();
            Console.WriteLine("=== Unavailable recipe breakdown (chooser hides these) ===");
            Console.WriteLine($"Total unavailable: {unavailable.Count}");
            Console.WriteLine($"  no unlock technologies: {noUnlockTech}");
            Console.WriteLine($"  no assemblers/miners:   {noAssembler}");
            Console.WriteLine($"  name matches barrel BL: {barrelLike}");
        }

        private static void WriteBarrelFilterImpact(JsonObject root, DataCache cache) {
            if (cache.Recipes.Count == 0)
                return;

            var barrelRecipesInJson = PresetJson.EnumerateArray(root, "recipes")
                .Select(r => PresetJson.GetString(r, "name"))
                .Where(n => n is not null && DataCacheRecipeFilters.BlackList.Any(rx => rx.IsMatch(n)))
                .Cast<string>()
                .ToList();

            int barrelUnavailable = barrelRecipesInJson.Count(n =>
                cache.Recipes.TryGetValue(n, out IRecipe? r) && !r.Available);

            Console.WriteLine();
            Console.WriteLine("=== Barreling filter (UseRecipeBWLists=true in tests) ===");
            Console.WriteLine($"JSON recipes matching barrel blacklist patterns: {barrelRecipesInJson.Count}");
            Console.WriteLine($"  of those, unavailable after load: {barrelUnavailable}");
        }

        private static void WriteChooserRelevantGaps(JsonObject root, DataCache cache) {
            int jsonItems = PresetJson.EnumerateArray(root, "items").Count();
            int availableItems = cache.Items.Values.Count(i => i.Available);
            int itemsWithNoAvailableRecipe = cache.Items.Values
                .OfType<ItemPrototype>()
                .Count(i => i.Available &&
                    !i.ProductionRecipes.Any(r => r.Available) &&
                    !i.ConsumptionRecipes.Any(r => r.Available));

            Console.WriteLine();
            Console.WriteLine("=== Item / chooser visibility ===");
            Console.WriteLine($"JSON items: {jsonItems}");
            Console.WriteLine($"Cache items marked Available: {availableItems}");
            Console.WriteLine($"Available items with zero Available production/consumption recipes: {itemsWithNoAvailableRecipe}");
        }

        private static int CountArray(JsonObject root, string name) =>
            PresetJson.EnumerateArray(root, name).Count();

        private static void SampleLines(string label, List<string> names, int max) {
            if (names.Count == 0)
                return;
            Console.WriteLine($"{label}: {string.Join(", ", names.Take(max))}" +
                (names.Count > max ? $" … +{names.Count - max} more" : ""));
        }
    }
}
