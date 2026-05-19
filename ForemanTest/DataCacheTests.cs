using Foreman;
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
    public class DataCacheTests : ForemanTestBase {
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

                var icons = await ForemanIconCacheFile.ReadAsync(path);
                Assert.IsTrue(icons.Count > 100, $"{presetName}: expected a large icon set, got {icons.Count}.");
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
            var modList = PresetProcessor.ReadPresetInfo(preset).ModList ?? new Dictionary<string, string>();
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
                preset, modList, itemNames, entityNames, qualityNames, recipeShorts, []);

            Assert.AreEqual(VanillaPresetName, errors.Preset.Name);
            Assert.IsTrue(errors.ErrorCount >= 0);
        }

        [TestMethod]
        public async Task TestPreset_Vanilla_KnownRecipeNotReportedMissing() {
            var preset = new Preset(VanillaPresetName, true, true);
            var modList = PresetProcessor.ReadPresetInfo(preset).ModList ?? new Dictionary<string, string>();
            var errors = await PresetProcessor.TestPreset(
                preset, modList, [], [], [], [new RecipeShort("iron-plate")], []);

            Assert.IsFalse(errors.MissingRecipes.Contains("iron-plate"), "iron-plate should exist in the vanilla preset recipe set.");
        }

        [TestMethod]
        public async Task TestPreset_Vanilla_BoilerPseudoRecipeMatchesDataCacheAmounts() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            Assert.IsTrue(cache.Recipes.TryGetValue("§§r:b:water:steam:165", out Recipe? boilerRecipe));
            var fromCache = new RecipeShort(boilerRecipe);

            var preset = new Preset(VanillaPresetName, true, true);
            var modList = PresetProcessor.ReadPresetInfo(preset).ModList ?? new Dictionary<string, string>();
            var errors = await PresetProcessor.TestPreset(
                preset, modList, [], [], [], [fromCache], []);

            Assert.AreEqual(0, errors.IncorrectRecipes.Count,
                "Incorrect recipes: " + string.Join(", ", errors.IncorrectRecipes));
            Assert.AreEqual(600, fromCache.Products["steam"]);
        }

        // --- import placeholders (isolated cache, no preset load) ---

        [TestMethod]
        public void ProcessImportedItemsSet_AddsMissingItemPlaceholder() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedItemsSet(new[] { "nonexistent-test-item-xyzzy" });
            Assert.IsTrue(cache.MissingItems.ContainsKey("nonexistent-test-item-xyzzy"));
        }

        [TestMethod]
        public void ProcessImportedItemsSet_SkipsExistingAndDuplicateNames() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedItemsSet(new[] { "import-a", "import-a" });
            Assert.AreEqual(1, cache.MissingItems.Count);
            cache.ProcessImportedItemsSet(new[] { "import-a" });
            Assert.AreEqual(1, cache.MissingItems.Count);
        }

        [TestMethod]
        public void ProcessImportedAssemblersSet_AddsMissingAssembler() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedAssemblersSet(new[] { "test-missing-assembler-xyzzy" });
            Assert.IsTrue(cache.MissingAssemblers.ContainsKey("test-missing-assembler-xyzzy"));
        }

        [TestMethod]
        public void ProcessImportedModulesSet_AddsMissingModule() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedModulesSet(new[] { "test-missing-module-xyzzy" });
            Assert.IsTrue(cache.MissingModules.ContainsKey("test-missing-module-xyzzy"));
        }

        [TestMethod]
        public void ProcessImportedBeaconsSet_AddsMissingBeacon() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedBeaconsSet(new[] { "test-missing-beacon-xyzzy" });
            Assert.IsTrue(cache.MissingBeacons.ContainsKey("test-missing-beacon-xyzzy"));
        }

        [TestMethod]
        public async Task ProcessImportedQualitiesSet_MapsKnownQualityByLevel() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            var map = cache.ProcessImportedQualitiesSet(new[]
            {
                new KeyValuePair<string, int>("normal", 0)
            });
            Assert.IsTrue(map.ContainsKey("normal"));
            Assert.IsTrue(map.TryGetValue("normal", out Quality? normal));
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
            Assert.AreEqual(0, cache.Items.Count(i => !i.Key.StartsWith("§§")));
        }

        [TestMethod]
        public async Task Clear_AfterLoad_RemovesQualitiesSciencePacksAndMissingSets() {
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false);
            Assert.IsTrue(cache.Qualities.ContainsKey("normal"));
            Assert.IsTrue(cache.SciencePacks.Count > 0);

            cache.Clear();

            Assert.AreEqual(0, cache.Qualities.Count);
            Assert.AreEqual(0, cache.MissingQualities.Count);
            Assert.AreEqual(0, cache.SciencePacks.Count);
            Assert.AreEqual(0, cache.SciencePackPrerequisites.Count);
            Assert.AreEqual(0, cache.MissingRecipes.Count);
            Assert.AreEqual(0, cache.MissingPlantProcesses.Count);
        }

        // --- full vanilla load (shared fixture) ---

        [TestMethod]
        public async Task LoadAllData_Vanilla_LoadsCoreItemsWithoutIcons() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();

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
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();

            var recipe = cache.Recipes["iron-plate"];
            Assert.IsTrue(recipe.IngredientSet.ContainsKey(cache.Items["iron-ore"]));
            Assert.IsTrue(recipe.ProductSet.ContainsKey(cache.Items["iron-plate"]));
            Assert.IsTrue(recipe.Assemblers.Any(), "iron-plate should be craftable in at least one assembler.");
            Assert.IsTrue(cache.Assemblers.ContainsKey("stone-furnace"));
            Assert.IsTrue(recipe.Assemblers.Contains(cache.Assemblers["stone-furnace"]));
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_SteamFluidAndWaterExist() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            Assert.IsTrue(cache.Items.ContainsKey("steam"));
            Assert.IsTrue(cache.Items["steam"] is Fluid);
            Assert.IsTrue(cache.Items.ContainsKey("water"));
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_NuclearReactorAndSteamTurbineEnergyValues() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
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
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            Assert.IsTrue(cache.Items.Count > 100, $"Vanilla item count was {cache.Items.Count}.");
            Assert.IsTrue(cache.Recipes.Count > 100, $"Vanilla recipe count was {cache.Recipes.Count}.");
            Assert.IsTrue(cache.Assemblers.Count > 15, $"Vanilla assembler count was {cache.Assemblers.Count}.");
            Assert.IsTrue(cache.Technologies.Count > 25, $"Vanilla technology count was {cache.Technologies.Count}.");
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_SciencePacksPopulatedAfterPostLoad() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            Assert.IsTrue(cache.SciencePacks.Count > 0);
            var automation = cache.Technologies["automation"];
            Assert.IsTrue(automation.SciPackList.Count > 0);
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_AvailableRecipesAreSubsetOfAllRecipes() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            var availableNames = cache.AvailableRecipes.Select(r => r.Name).ToHashSet();
            foreach (var name in availableNames)
                Assert.IsTrue(cache.Recipes.ContainsKey(name));
            Assert.IsTrue(availableNames.Count < cache.Recipes.Count,
                "Some recipes should be marked unavailable after post-processing.");
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_BarrelRecipesFilteredWhenRecipeListsEnabled() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync(filterRecipes: true);
            var barrelSuffix = new Regex("-barrel$");
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
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false);
            Assert.AreEqual(VanillaPresetName, cache.PresetName);
            Assert.IsTrue(cache.Items.ContainsKey("iron-plate"));

            cache.Clear();
            Assert.AreEqual(0, cache.Items.Count(i => !i.Key.StartsWith("§§")));
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false);
            Assert.IsTrue(cache.Items.ContainsKey("iron-plate"));
        }

        [TestMethod]
        public async Task ProcessImportedQualitiesSet_CreatesMissingQualityWhenUnknown() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            var map = cache.ProcessImportedQualitiesSet(new[]
            {
                new KeyValuePair<string, int>("save-only-quality-tier-3", 3)
            });

            Assert.IsTrue(map.ContainsKey("save-only-quality-tier-3"));
            Assert.IsTrue(map.TryGetValue("save-only-quality-tier-3", out Quality? saveOnlyQuality));
            Assert.IsNotNull(saveOnlyQuality);
            Assert.IsTrue(saveOnlyQuality.IsMissing);
            Assert.AreEqual(3, saveOnlyQuality.Level);
            Assert.IsTrue(cache.MissingQualities.Values.Any(q => q.Level == 3));
        }

        [TestMethod]
        public async Task ProcessImportedRecipesSet_LinksExistingRecipeByNameAndIo() {
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false);
            var ironPlate = cache.Recipes["iron-plate"];
            const long linkId = 424242L;
            var shortWithId = new RecipeShort(
                ironPlate.Name,
                linkId,
                missing: false,
                new Dictionary<string, double> { ["iron-ore"] = 1.0 },
                new Dictionary<string, double> { ["iron-plate"] = 1.0 });

            var links = cache.ProcessImportedRecipesSet(new[] { shortWithId });

            Assert.AreEqual(1, links.Count);
            Assert.AreSame(ironPlate, links[linkId]);
            Assert.AreEqual(0, cache.MissingRecipes.Count);
        }

        [TestMethod]
        public async Task ProcessImportedRecipesSet_CreatesMissingRecipeWithKnownAndMissingItems() {
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false);
            cache.ProcessImportedItemsSet(new[] { "save-only-ingredient-xyzzy" });

            var shortFromSave = new RecipeShort(
                "save-only-recipe-xyzzy",
                9001L,
                missing: true,
                new Dictionary<string, double> { ["save-only-ingredient-xyzzy"] = 2.0, ["iron-ore"] = 1.0 },
                new Dictionary<string, double> { ["iron-plate"] = 1.0 });

            var links = cache.ProcessImportedRecipesSet(new[] { shortFromSave });

            Assert.IsTrue(links.TryGetValue(9001L, out Recipe? linkedRecipe));
            Assert.IsNotNull(linkedRecipe);
            var missing = (RecipePrototype)linkedRecipe;
            Assert.IsTrue(missing.IsMissing);
            Assert.IsTrue(cache.MissingRecipes.ContainsKey(shortFromSave));
            Assert.IsTrue(missing.IngredientSet.ContainsKey(cache.Items["iron-ore"]));
            Assert.IsTrue(missing.IngredientSet.ContainsKey(cache.MissingItems["save-only-ingredient-xyzzy"]));
            Assert.AreEqual(1, missing.Assemblers.Count);
            Assert.IsTrue(missing.Assemblers.First().IsMissing);
        }

        [TestMethod]
        public async Task ProcessImportedRecipesSet_ReusesExistingMissingRecipeEntry() {
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false);
            var shortFromSave = new RecipeShort(
                "save-only-recipe-dedupe",
                77L,
                missing: true,
                new Dictionary<string, double> { ["iron-ore"] = 1.0 },
                new Dictionary<string, double> { ["iron-plate"] = 1.0 });

            var first = cache.ProcessImportedRecipesSet(new[] { shortFromSave });
            var second = cache.ProcessImportedRecipesSet(new[] { shortFromSave });

            Assert.AreEqual(1, cache.MissingRecipes.Count);
            Assert.AreSame(first[77L], second[77L]);
        }

        [TestMethod]
        public void ProcessImportedPlantProcessesSet_CreatesMissingWhenUnknown() {
            var cache = new DataCache(filterRecipes: false);
            cache.ProcessImportedItemsSet(new[] { "iron-plate" });

            var shortFromSave = new PlantShort(
                "save-only-plant-xyzzy",
                55L,
                missing: true,
                new Dictionary<string, double> { ["iron-plate"] = 1.0 });

            var links = cache.ProcessImportedPlantProcessesSet(new[] { shortFromSave });

            Assert.AreEqual(55L, links.Single().Key);
            Assert.IsTrue(cache.MissingPlantProcesses.ContainsKey(shortFromSave));
            Assert.IsTrue(links.TryGetValue(55L, out PlantProcess? linkedPlant));
            Assert.IsNotNull(linkedPlant);
            Assert.IsTrue(linkedPlant.ProductSet.ContainsKey(cache.MissingItems["iron-plate"]));
        }

        [TestMethod]
        public async Task LoadAllData_WithoutRecipeFilter_BarrelRecipeStaysAvailable() {
            VanillaDataCacheFixture.Reset();
            var cache = new DataCache(filterRecipes: false);
            await cache.LoadAllData(new Preset(VanillaPresetName, true, true), NullProgress.Instance, loadIcons: false);

            Assert.IsTrue(cache.Recipes.ContainsKey("crude-oil-barrel"));
            Assert.IsTrue(cache.Recipes["crude-oil-barrel"].Available,
                "Barrel recipes should remain available when recipe filter lists are disabled.");
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_DefaultQualityIsNormal() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            Assert.IsNotNull(cache.DefaultQuality);
            Assert.AreEqual("normal", cache.DefaultQuality.Name);
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_PlayerAssemblerRegisteredAfterEntityLoad() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            Assert.IsNotNull(cache.PlayerAssembler);
            Assert.IsTrue(cache.Assemblers.ContainsKey(cache.PlayerAssembler.Name));
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_RocketLaunchRecipeUsesRocketAssembler() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            Assert.IsNotNull(cache.RocketAssembler);
            var launchRecipe = cache.Recipes.Values
                .FirstOrDefault(r => r.Name.StartsWith("§§r:rl:launch-", StringComparison.Ordinal));
            Assert.IsNotNull(launchRecipe, "PresetDataLoader should create rocket launch recipes after entity load.");
            Assert.IsNotNull(cache.RocketAssembler);
            Assert.IsTrue(launchRecipe.Assemblers.Contains(cache.RocketAssembler));
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_SatelliteRocketLaunchProductsAndIngredients() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            Assert.IsTrue(cache.Recipes.TryGetValue("§§r:rl:launch-satellite", out Recipe? launchRecipe));
            Assert.IsNotNull(launchRecipe);
            Assert.IsTrue(cache.Items.TryGetValue("satellite", out Item? satellite));
            Assert.IsTrue(cache.Items.TryGetValue("space-science-pack", out Item? spaceScience));
            Assert.IsTrue(cache.Items.TryGetValue("rocket-part", out Item? rocketPart));

            Assert.AreEqual(1, launchRecipe.IngredientSet[satellite]);
            Assert.AreEqual(100, launchRecipe.IngredientSet[rocketPart]);
            Assert.AreEqual(1000, launchRecipe.ProductSet[spaceScience]);
        }

        [TestMethod]
        public async Task LoadAllData_Vanilla_ProductivityModuleLinkedToCraftingRecipes() {
            var cache = await VanillaDataCacheFixture.GetLoadedAsync();
            Assert.IsTrue(cache.Modules.ContainsKey("productivity-module-3"));
            var module = (ModulePrototype)cache.Modules["productivity-module-3"];
            Assert.IsTrue(module.Recipes.Count > 0, "Entity/module processing should attach productivity modules to eligible recipes.");
            Assert.IsTrue(module.Recipes.Any(r => r.Name == "electronic-circuit"));
        }
    }
}