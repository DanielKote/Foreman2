using Foreman;
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
    /// <summary>
    /// Ensures the Pyanodon test preset (Lua export → JSON → DataCache) does not drop utilizable game content.
    /// Hidden recipes remain in the cache (disabled by default; user can enable via Show Hidden).
    /// Recipes with no crafting machine in the export are omitted (never craftable in this mod set).
    /// </summary>
    [TestClass]
    public class PyanodonPresetIntegrityTests : ForemanTestBase {
        [TestMethod]
        public async Task Pyanodon_AllExportedCraftingRecipes_AreLoadedOrNeverCraftableInGame() {
            var snapshot = await PyanodonPresetTestSupport.LoadSnapshotAsync().ConfigureAwait(false);

            var missingCraftable = new List<string>();
            var missingNeverCraftable = new List<string>();

            foreach (JsonNode recipeNode in PresetJson.EnumerateArray(snapshot.Root, "recipes")) {
                if (!PyanodonPresetTestSupport.TryGetRecipeNode(recipeNode, out string name, out string category))
                    continue;

                bool couldCraft = PresetCraftingCompatibility.RecipeHasCraftingMachine(
                    recipeNode, category, snapshot.CategoryMachines);
                if (snapshot.Cache.Recipes.ContainsKey(name))
                    continue;

                if (couldCraft)
                    missingCraftable.Add(name);
                else
                    missingNeverCraftable.Add(name);
            }

            PyanodonPresetTestSupport.AssertEmpty(
                missingCraftable,
                "Exported recipes with crafting machines must be in DataCache");

            Assert.IsNotEmpty(
                missingNeverCraftable,
                "Expected some never-craftable exported recipes (e.g. legacy combustion) to be omitted from cache.");
            Assert.IsTrue(
                missingNeverCraftable.All(n => n.Contains("combustion", StringComparison.Ordinal)),
                "Unexpected never-craftable recipe omissions: " +
                string.Join(", ", missingNeverCraftable.Where(n => !n.Contains("combustion", StringComparison.Ordinal)).Take(15)));
        }

        [TestMethod]
        public async Task Pyanodon_CraftableExportedRecipes_HaveAssemblers() {
            var snapshot = await PyanodonPresetTestSupport.LoadSnapshotAsync().ConfigureAwait(false);
            var unlinked = new List<string>();

            foreach (JsonNode recipeNode in PresetJson.EnumerateArray(snapshot.Root, "recipes")) {
                if (!PyanodonPresetTestSupport.TryGetRecipeNode(recipeNode, out string name, out string category))
                    continue;
                if (!PresetCraftingCompatibility.RecipeHasCraftingMachine(recipeNode, category, snapshot.CategoryMachines))
                    continue;
                if (!snapshot.Cache.Recipes.TryGetValue(name, out IRecipe? recipe) || recipe is not RecipePrototype prototype)
                    continue;
                if (prototype.AssemblersInternal.Count == 0)
                    unlinked.Add(name);
            }

            PyanodonPresetTestSupport.AssertEmpty(
                unlinked,
                "Craftable exported recipes must link to at least one assembler");
        }

        [TestMethod]
        public async Task Pyanodon_HiddenExportedRecipes_WithMachines_AreInCache_DisabledByDefault() {
            var snapshot = await PyanodonPresetTestSupport.LoadSnapshotAsync().ConfigureAwait(false);

            int hiddenWithMachines = 0;
            int missing = 0;
            int notDisabled = 0;

            foreach (JsonNode recipeNode in PresetJson.EnumerateArray(snapshot.Root, "recipes")) {
                if (PresetJson.GetBool(recipeNode, "hidden") is not true)
                    continue;
                if (!PyanodonPresetTestSupport.TryGetRecipeNode(recipeNode, out string name, out string category))
                    continue;
                if (!PresetCraftingCompatibility.RecipeHasCraftingMachine(recipeNode, category, snapshot.CategoryMachines))
                    continue;

                hiddenWithMachines++;
                if (!snapshot.Cache.Recipes.TryGetValue(name, out IRecipe? recipe) || recipe is not RecipePrototype prototype) {
                    missing++;
                    continue;
                }
                if (prototype.Enabled)
                    notDisabled++;
            }

            Assert.IsGreaterThan(1000, hiddenWithMachines, "Pyanodon export should include many hidden-but-craftable recipes.");
            Assert.AreEqual(0, missing, "Hidden recipes with crafting machines must remain in DataCache.");
            Assert.AreEqual(0, notDisabled, "Hidden recipes should start disabled (enable via Show Hidden).");
        }

        [TestMethod]
        public async Task Pyanodon_AllExportedItemsAndFluids_AreInCache() {
            var snapshot = await PyanodonPresetTestSupport.LoadSnapshotAsync().ConfigureAwait(false);

            var missingItems = PresetJson.EnumerateArray(snapshot.Root, "items")
                .Select(i => PresetJson.GetString(i, "name"))
                .Where(n => n is not null && !snapshot.Cache.Items.ContainsKey(n))
                .Cast<string>()
                .ToList();
            var missingFluids = PresetJson.EnumerateArray(snapshot.Root, "fluids")
                .Select(f => PresetJson.GetString(f, "name"))
                .Where(n => n is not null && !snapshot.Cache.Items.ContainsKey(n))
                .Cast<string>()
                .ToList();

            PyanodonPresetTestSupport.AssertEmpty(missingItems, "Missing items");
            PyanodonPresetTestSupport.AssertEmpty(missingFluids, "Missing fluids");
        }

        [TestMethod]
        public async Task Pyanodon_AllExportedTechnologies_AreInCache() {
            var snapshot = await PyanodonPresetTestSupport.LoadSnapshotAsync().ConfigureAwait(false);

            var missing = PresetJson.EnumerateArray(snapshot.Root, "technologies")
                .Select(t => PresetJson.GetString(t, "name"))
                .Where(n => n is not null && !snapshot.Cache.Technologies.ContainsKey(n))
                .Cast<string>()
                .ToList();

            PyanodonPresetTestSupport.AssertEmpty(missing, "Missing technologies");
        }

        [TestMethod]
        public async Task Pyanodon_HideFromPlayerCrafting_RecipesExcludePlayerAssembler() {
            var snapshot = await PyanodonPresetTestSupport.LoadSnapshotAsync().ConfigureAwait(false);
            IAssembler player = snapshot.Cache.PlayerAssembler ?? throw new AssertFailedException("Player assembler is required.");

            var wronglyLinked = PresetJson.EnumerateArray(snapshot.Root, "recipes")
                .Where(r => PresetJson.GetBool(r, "hide_from_player_crafting") is true)
                .Select(r => PresetJson.GetString(r, "name"))
                .Where(n => n is not null &&
                    snapshot.Cache.Recipes.TryGetValue(n, out IRecipe? recipe) &&
                    recipe is RecipePrototype prototype &&
                    prototype.AssemblersInternal.Any(a => ReferenceEquals(a, player)))
                .Cast<string>()
                .ToList();

            PyanodonPresetTestSupport.AssertEmpty(
                wronglyLinked,
                "Machine-only recipes must not list the player assembler");
        }

        [TestMethod]
        public async Task Pyanodon_AllResources_HaveExtractionRecipesInCache() {
            var snapshot = await PyanodonPresetTestSupport.LoadSnapshotAsync().ConfigureAwait(false);

            var missing = PyanodonPresetTestSupport.EnumerateAllResources(snapshot.Root)
                .Select(r => PresetJson.GetString(r, "name"))
                .Where(n => n is not null &&
                    !snapshot.Cache.Recipes.ContainsKey(ExtractionRecipeTestSupport.ExtractionRecipeName(n)))
                .Cast<string>()
                .ToList();

            PyanodonPresetTestSupport.AssertEmpty(missing, "Missing extraction recipes");
        }
    }
}
