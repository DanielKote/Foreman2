using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ForemanTest {
    [TestClass]
    public partial class DataCacheTests : ForemanTestBase {
        private const string VanillaPresetName = VanillaDataCacheFixture.PresetName;

        public TestContext? TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize() {
            if (!VanillaDataCacheFixture.PresetsAvailable)
                Assert.Inconclusive($"Preset folder not found: {VanillaDataCacheFixture.PresetsDirectory}");
        }

        private static string PresetPath(string fileName) =>
            Path.Combine(VanillaDataCacheFixture.PresetsDirectory, fileName);

        // --- preset files / read path (no full load) ---

        [TestMethod]
        public void VanillaPreset_FilesExist() {
            Assert.IsTrue(File.Exists(PresetPath(VanillaPresetName + ".pjson")));
            Assert.IsTrue(File.Exists(PresetPath(VanillaPresetName + ".dat")));
        }

        [TestMethod]
        public async Task IconCacheFiles_BundledPresets_AreValidFoic() {
            if (!VanillaDataCacheFixture.PresetsAvailable)
                Assert.Inconclusive($"Preset folder not found: {VanillaDataCacheFixture.PresetsDirectory}");

            string[] presetNames =
            [
                VanillaPresetName,
                SpaceAgeDataCacheFixture.PresetName
            ];

            foreach (string presetName in presetNames) {
                string path = Path.Combine(VanillaDataCacheFixture.PresetsDirectory, presetName + ".dat");
                Assert.IsTrue(File.Exists(path), $"Missing icon cache: {path}");
                Assert.IsTrue(
                    ForemanIconCacheFile.IsFoicFile(path),
                    $"{presetName}.dat is not FOIC format. Re-import the preset to regenerate icon caches before running tests.");

                Assert.IsNotNull(TestContext);
                var icons = await ForemanIconCacheFile.ReadAsync(path, cancellationToken: TestContext.CancellationToken).ConfigureAwait(false);
                Assert.IsGreaterThan(100, icons.Count, $"{presetName}: expected a large icon set, got {icons.Count}.");
                Assert.IsTrue(icons.ContainsKey("icon.i.iron-plate"), $"{presetName}: missing icon.i.iron-plate.");
                Assert.IsNotNull(icons["icon.i.iron-plate"].Icon, $"{presetName}: iron-plate icon bitmap is null.");
            }
        }

        [TestMethod]
        public void ReadPresetInfo_Vanilla_IncludesBaseMod() {
            var info = PresetProcessor.ReadPresetInfo(new Preset(VanillaPresetName, true, true));
            Assert.IsNotNull(info.ModList);
            Assert.IsTrue(info.ModList.ContainsKey("base"), "Expected vanilla preset to list the base mod.");
        }

        [TestMethod]
        public void PrepPreset_Vanilla_ContainsIronPlateItem() {
            var json = PresetProcessor.PrepPreset(new Preset(VanillaPresetName, true, true));
            bool hasIronPlate = json["items"] is JsonArray items &&
                items.Any(t => t?["name"]?.GetValue<string>() == "iron-plate");
            Assert.IsTrue(hasIronPlate, "Vanilla preset JSON should contain iron-plate.");
        }

        [TestMethod]
        public void PrepPreset_Vanilla_ModCountMatchesReadPresetInfo() {
            var preset = new Preset(VanillaPresetName, true, true);
            var info = PresetProcessor.ReadPresetInfo(preset);
            var json = PresetProcessor.PrepPreset(preset);
            int jsonModCount = json["mods"] is JsonArray mods ? mods.Count : 0;
            Assert.AreEqual(jsonModCount, info.ModList?.Count ?? 0);
        }

        [TestMethod]
        public async Task TestPreset_Vanilla_ReturnsComparableErrorPackage() {
            var preset = new Preset(VanillaPresetName, true, true);
            var json = PresetProcessor.PrepPreset(preset);
            var modList = PresetProcessor.ReadPresetInfo(preset).ModList ?? [];
            var itemNames = (json["items"] as JsonArray)?.Select(t => t?["name"]?.GetValue<string>()).OfType<string>().Take(50).ToList() ?? [];
            var entityNames = (json["entities"] as JsonArray)?.Select(t => t?["name"]?.GetValue<string>()).OfType<string>().Take(50).ToList() ?? [];
            var qualityNames = (json["qualities"] as JsonArray)?.Select(t => t?["name"]?.GetValue<string>()).OfType<string>().ToList() ?? [];
            var recipeShorts = (json["recipes"] as JsonArray)?
                .Select(t => t?["name"]?.GetValue<string>())
                .OfType<string>()
                .Take(50)
                .Select(name => new RecipeShort(name))
                .ToList() ?? [];

            var errors = await PresetProcessor.TestPreset(
                preset, modList, itemNames, qualityNames, recipeShorts, []).ConfigureAwait(false);

            Assert.AreEqual(VanillaPresetName, errors.Preset.Name);
            Assert.IsGreaterThanOrEqualTo(0, errors.ErrorCount);
        }

        [TestMethod]
        public async Task TestPreset_Vanilla_KnownRecipeNotReportedMissing() {
            var preset = new Preset(VanillaPresetName, true, true);
            var modList = PresetProcessor.ReadPresetInfo(preset).ModList ?? [];
            var errors = await PresetProcessor.TestPreset(
                preset, modList, [], [], [new RecipeShort("iron-plate")], []).ConfigureAwait(false);

            Assert.DoesNotContain("iron-plate", errors.MissingRecipes, "iron-plate should exist in the vanilla preset recipe set.");
        }

        [TestMethod]
        public async Task TestPreset_Vanilla_BoilerPseudoRecipeMatchesDataCacheAmounts() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsTrue(cache.Recipes.TryGetValue("§§r:b:water:steam:165", out IRecipe? boilerRecipe));
            var fromCache = new RecipeShort(boilerRecipe);

            var preset = new Preset(VanillaPresetName, true, true);
            var modList = PresetProcessor.ReadPresetInfo(preset).ModList ?? [];
            var errors = await PresetProcessor.TestPreset(
                preset, modList, [], [], [fromCache], []).ConfigureAwait(false);

            Assert.IsEmpty(errors.IncorrectRecipes,
                "Incorrect recipes: " + string.Join(", ", errors.IncorrectRecipes));
            Assert.AreEqual(600, fromCache.Products["steam"]);
        }

        // --- import placeholders (isolated cache, no preset load) ---

        [TestMethod]
        public void ProcessImportedItemsSet_AddsMissingItemPlaceholder() {
            var cache = new DataCache(filterRecipes: false);
            const string itemName = "nonexistent-test-item-xyzzy";
            cache.ProcessImportedItemsSet([itemName]);
            Assert.IsTrue(cache.MissingItems.ContainsKey(itemName));
        }

        [TestMethod]
        public void ProcessImportedItemsSet_SkipsExistingAndDuplicateNames() {
            var cache = new DataCache(filterRecipes: false);
            const string itemName = "import-a";
            cache.ProcessImportedItemsSet([itemName, itemName]);
            Assert.HasCount(1, cache.MissingItems);
            cache.ProcessImportedItemsSet([itemName]);
            Assert.HasCount(1, cache.MissingItems);
        }

        [TestMethod]
        public void ProcessImportedAssemblersSet_AddsMissingAssembler() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedAssemblersSet(["test-missing-assembler-xyzzy"]);
            Assert.IsTrue(cache.MissingAssemblers.ContainsKey("test-missing-assembler-xyzzy"));
        }

        [TestMethod]
        public void ProcessImportedModulesSet_AddsMissingModule() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedModulesSet(["test-missing-module-xyzzy"]);
            Assert.IsTrue(cache.MissingModules.ContainsKey("test-missing-module-xyzzy"));
        }

        [TestMethod]
        public void ProcessImportedBeaconsSet_AddsMissingBeacon() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedBeaconsSet(["test-missing-beacon-xyzzy"]);
            Assert.IsTrue(cache.MissingBeacons.ContainsKey("test-missing-beacon-xyzzy"));
        }

        [TestMethod]
        public async Task ProcessImportedQualitiesSet_MapsKnownQualityByLevel() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            var map = cache.ProcessImportedQualitiesSet(
            [
                new KeyValuePair<string, int>("normal", 0)
            ]);
            Assert.IsTrue(map.ContainsKey("normal"));
            Assert.IsTrue(map.TryGetValue("normal", out IQuality? normal));
            Assert.IsNotNull(normal);
            Assert.AreEqual(0, normal.Level);
        }

        [TestMethod]
        public void Clear_KeepsForemanHelperObjects() {
            var cache = new DataCache(filterRecipes: true);
            Assert.IsTrue(cache.Recipes.ContainsKey("§§r:h:heat-generation"));
            Assert.IsTrue(cache.Items.ContainsKey("§§i:heat"));
            Assert.IsTrue(cache.Groups.ContainsKey("§§g:extra_group"));

            cache.Clear();

            Assert.IsTrue(cache.Recipes.ContainsKey("§§r:h:heat-generation"));
            Assert.IsTrue(cache.Items.ContainsKey("§§i:heat"));
            Assert.IsTrue(cache.Groups.ContainsKey("§§g:extra_group"));
            Assert.AreEqual(0, cache.Items.Count(i => !i.Key.StartsWith("§§", StringComparison.Ordinal)));
        }

        [TestMethod]
        public async Task Clear_AfterLoad_RemovesQualitiesSciencePacksAndMissingSets() {
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false).ConfigureAwait(false);
            Assert.IsTrue(cache.Qualities.ContainsKey("normal"));
            Assert.IsNotEmpty(cache.SciencePacks);

            cache.Clear();

            Assert.IsEmpty(cache.Qualities);
            Assert.IsEmpty(cache.MissingQualities);
            Assert.IsEmpty(cache.SciencePacks);
            Assert.IsEmpty(cache.SciencePackPrerequisites);
            Assert.IsEmpty(cache.MissingRecipes);
            Assert.IsEmpty(cache.MissingPlantProcesses);
        }

        // --- full vanilla load (shared fixture) ---

        [TestMethod]
        public async Task LoadAllData_Vanilla_LoadsCoreItemsWithoutIcons() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);

            Assert.AreEqual(VanillaPresetName, cache.PresetName);
            Assert.IsTrue(cache.Items.ContainsKey("iron-plate"));
            Assert.IsTrue(cache.Items.ContainsKey("copper-plate"));
            Assert.IsTrue(cache.Recipes.ContainsKey("iron-plate"));
            Assert.IsNotNull(cache.DefaultQuality);
            Assert.IsNotNull(cache.PlayerAssembler);
            Assert.IsTrue(cache.Technologies.ContainsKey("automation"));
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_IronPlateRecipeHasExpectedIoAndAssemblers() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);

            var recipe = cache.Recipes["iron-plate"];
            Assert.IsTrue(recipe.IngredientSet.ContainsKey(cache.Items["iron-ore"]));
            Assert.IsTrue(recipe.ProductSet.ContainsKey(cache.Items["iron-plate"]));
            Assert.IsGreaterThan(0, recipe.Assemblers.Count, "iron-plate should be craftable in at least one assembler.");
            Assert.IsTrue(cache.Assemblers.ContainsKey("stone-furnace"));
            Assert.Contains(cache.Assemblers["stone-furnace"], recipe.Assemblers);
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_SteamFluidAndWaterExist() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsTrue(cache.Items.ContainsKey("steam"));
            Assert.IsTrue(cache.Items["steam"] is IFluid);
            Assert.IsTrue(cache.Items.ContainsKey("water"));
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_NuclearReactorAndSteamTurbineEnergyValues() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsNotNull(cache.DefaultQuality);
            var quality = cache.DefaultQuality;
            var reactor = cache.Assemblers["nuclear-reactor"];
            var turbine = cache.Assemblers["steam-turbine"];

            Assert.AreEqual(40_000_000, reactor.GetEnergyConsumption(quality));
            Assert.AreEqual(40, reactor.GetSpeed(quality), 1e-6);
            Assert.AreEqual(5_820_000, turbine.GetEnergyProduction(quality));
            Assert.AreEqual(0, turbine.GetEnergyConsumption(quality));
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_CollectionsHaveExpectedScale() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsGreaterThan(100, cache.Items.Count, $"Vanilla item count was {cache.Items.Count}.");
            Assert.IsGreaterThan(100, cache.Recipes.Count, $"Vanilla recipe count was {cache.Recipes.Count}.");
            Assert.IsGreaterThan(15, cache.Assemblers.Count, $"Vanilla assembler count was {cache.Assemblers.Count}.");
            Assert.IsGreaterThan(25, cache.Technologies.Count, $"Vanilla technology count was {cache.Technologies.Count}.");
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_SciencePacksPopulatedAfterPostLoad() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsNotEmpty(cache.SciencePacks);
            var automation = cache.Technologies["automation"];
            Assert.IsNotEmpty(automation.SciPackList);
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_AvailableRecipesAreSubsetOfAllRecipes() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            var availableNames = cache.AvailableRecipes.Select(r => r.Name).ToHashSet();
            foreach (var name in availableNames)
                Assert.IsTrue(cache.Recipes.ContainsKey(name));
            Assert.IsLessThan(cache.Recipes.Count, availableNames.Count,
                "Some recipes should be marked unavailable after post-processing.");
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_BarrelRecipesFilteredWhenRecipeListsEnabled() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync(filterRecipes: true).ConfigureAwait(false);
            var barrelSuffix = BarrelSuffixRegex();
            foreach (var recipe in cache.Recipes.Values) {
                if (recipe.Name == "empty-barrel")
                    continue;
                if (barrelSuffix.IsMatch(recipe.Name))
                    Assert.IsFalse(recipe.Available,
                        $"Recipe {recipe.Name} should be unavailable when recipe filter lists are enabled.");
            }
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_ReloadOnFreshCacheReplacesPresetName() {
            VanillaDataCacheFixture.Reset();
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false).ConfigureAwait(false);
            Assert.AreEqual(VanillaPresetName, cache.PresetName);
            Assert.IsTrue(cache.Items.ContainsKey("iron-plate"));

            cache.Clear();
            Assert.AreEqual(0, cache.Items.Count(i => !i.Key.StartsWith("§§", StringComparison.Ordinal)));
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false).ConfigureAwait(false);
            Assert.IsTrue(cache.Items.ContainsKey("iron-plate"));
        }

        [TestMethod]
        public async Task ProcessImportedQualitiesSet_CreatesMissingQualityWhenUnknown() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            var map = cache.ProcessImportedQualitiesSet(
            [
                new KeyValuePair<string, int>("save-only-quality-tier-3", 3)
            ]);

            Assert.IsTrue(map.ContainsKey("save-only-quality-tier-3"));
            Assert.IsTrue(map.TryGetValue("save-only-quality-tier-3", out IQuality? saveOnlyQuality));
            Assert.IsNotNull(saveOnlyQuality);
            Assert.IsTrue(saveOnlyQuality.IsMissing);
            Assert.AreEqual(3, saveOnlyQuality.Level);
            Assert.Contains(q => q.Level == 3, cache.MissingQualities.Values);
        }

        [TestMethod]
        public async Task ProcessImportedRecipesSet_LinksExistingRecipeByNameAndIo() {
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false).ConfigureAwait(false);
            var ironPlate = cache.Recipes["iron-plate"];
            const long linkId = 424242L;
            var shortWithId = new RecipeShort(
                ironPlate.Name,
                linkId,
                missing: false,
                new Dictionary<string, double> { ["iron-ore"] = 1.0 },
                new Dictionary<string, double> { ["iron-plate"] = 1.0 });

            var links = cache.ProcessImportedRecipesSet([shortWithId]);

            Assert.HasCount(1, links);
            Assert.AreSame(ironPlate, links[linkId]);
            Assert.IsEmpty(cache.MissingRecipes);
        }

        [TestMethod]
        public async Task ProcessImportedRecipesSet_CreatesMissingRecipeWithKnownAndMissingItems() {
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false).ConfigureAwait(false);
            cache.ProcessImportedItemsSet(["save-only-ingredient-xyzzy"]);

            var shortFromSave = new RecipeShort(
                "save-only-recipe-xyzzy",
                9001L,
                missing: true,
                new Dictionary<string, double> { ["save-only-ingredient-xyzzy"] = 2.0, ["iron-ore"] = 1.0 },
                new Dictionary<string, double> { ["iron-plate"] = 1.0 });

            var links = cache.ProcessImportedRecipesSet([shortFromSave]);

            Assert.IsTrue(links.TryGetValue(9001L, out IRecipe? linkedRecipe));
            Assert.IsNotNull(linkedRecipe);
            var missing = (RecipePrototype)linkedRecipe;
            Assert.IsTrue(missing.IsMissing);
            Assert.IsTrue(cache.MissingRecipes.ContainsKey(shortFromSave));
            Assert.IsTrue(missing.IngredientSet.ContainsKey(cache.Items["iron-ore"]));
            Assert.IsTrue(missing.IngredientSet.ContainsKey(cache.MissingItems["save-only-ingredient-xyzzy"]));
            Assert.HasCount(1, missing.Assemblers);
            Assert.IsTrue(missing.Assemblers.First().IsMissing);
        }

        [TestMethod]
        public async Task ProcessImportedRecipesSet_ReusesExistingMissingRecipeEntry() {
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false).ConfigureAwait(false);
            var shortFromSave = new RecipeShort(
                "save-only-recipe-dedupe",
                77L,
                missing: true,
                new Dictionary<string, double> { ["iron-ore"] = 1.0 },
                new Dictionary<string, double> { ["iron-plate"] = 1.0 });

            var first = cache.ProcessImportedRecipesSet([shortFromSave]);
            var second = cache.ProcessImportedRecipesSet([shortFromSave]);

            Assert.HasCount(1, cache.MissingRecipes);
            Assert.AreSame(first[77L], second[77L]);
        }

        [TestMethod]
        public void ProcessImportedPlantProcessesSet_CreatesMissingWhenUnknown() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedItemsSet(["iron-plate"]);

            var shortFromSave = new PlantShort(
                "save-only-plant-xyzzy",
                55L,
                missing: true,
                new Dictionary<string, double> { ["iron-plate"] = 1.0 });

            var links = cache.ProcessImportedPlantProcessesSet([shortFromSave]);

            Assert.AreEqual(55L, links.Single().Key);
            Assert.IsTrue(cache.MissingPlantProcesses.ContainsKey(shortFromSave));
            Assert.IsTrue(links.TryGetValue(55L, out IPlantProcess? linkedPlant));
            Assert.IsNotNull(linkedPlant);
            Assert.IsTrue(linkedPlant.ProductSet.ContainsKey(cache.MissingItems["iron-plate"]));
        }

        [TestMethod]
        public async Task LoadAllData_WithoutRecipeFilter_BarrelRecipeStaysAvailable() {
            VanillaDataCacheFixture.Reset();
            var cache = new DataCache(filterRecipes: false);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false).ConfigureAwait(false);

            Assert.IsTrue(cache.Recipes.ContainsKey("crude-oil-barrel"));
            Assert.IsTrue(cache.Recipes["crude-oil-barrel"].Available,
                "Barrel recipes should remain available when recipe filter lists are disabled.");
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_DefaultQualityIsNormal() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsNotNull(cache.DefaultQuality);
            Assert.AreEqual("normal", cache.DefaultQuality.Name);
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_DefaultQualityDisplaysAsNormal() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsNotNull(cache.DefaultQuality);
            Assert.AreEqual("Normal", cache.DefaultQuality.FriendlyName);
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_PlayerAssemblerRegisteredAfterEntityLoad() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsNotNull(cache.PlayerAssembler);
            Assert.IsTrue(cache.Assemblers.ContainsKey(cache.PlayerAssembler.Name));
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_RocketLaunchRecipeUsesRocketAssembler() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsNotNull(cache.RocketAssembler);
            var launchRecipe = cache.Recipes.Values
                .FirstOrDefault(r => r.Name.StartsWith("§§r:rl:launch-", StringComparison.Ordinal));
            Assert.IsNotNull(launchRecipe, "PresetDataLoader should create rocket launch recipes after entity load.");
            Assert.IsNotNull(cache.RocketAssembler);
            Assert.Contains(cache.RocketAssembler, launchRecipe.Assemblers);
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_SatelliteRocketLaunchProductsAndIngredients() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsTrue(cache.Recipes.TryGetValue("§§r:rl:launch-satellite", out IRecipe? launchRecipe));
            Assert.IsNotNull(launchRecipe);
            Assert.IsTrue(cache.Items.TryGetValue("satellite", out IItem? satellite));
            Assert.IsTrue(cache.Items.TryGetValue("space-science-pack", out IItem? spaceScience));
            Assert.IsTrue(cache.Items.TryGetValue("rocket-part", out IItem? rocketPart));

            Assert.AreEqual(1, launchRecipe.IngredientSet[satellite]);
            Assert.AreEqual(100, launchRecipe.IngredientSet[rocketPart]);
            Assert.AreEqual(1000, launchRecipe.ProductSet[spaceScience]);
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_ProductivityModuleLinkedToCraftingRecipes() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            Assert.IsTrue(cache.Modules.ContainsKey("productivity-module-3"));
            var module = (ModulePrototype)cache.Modules["productivity-module-3"];
            Assert.IsNotEmpty(module.Recipes, "Entity/module processing should attach productivity modules to eligible recipes.");
            Assert.Contains(r => r.Name == "electronic-circuit", module.Recipes);
        }

        [GeneratedRegex("-barrel$")]
        private static partial Regex BarrelSuffixRegex();
    }
}
