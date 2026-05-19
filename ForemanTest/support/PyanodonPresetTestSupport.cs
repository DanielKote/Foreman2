using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.Loading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ForemanTest.support {
    /// <summary>Shared setup and assertions for tests against the Pyanodon ZSTD preset asset.</summary>
    internal static class PyanodonPresetTestSupport {
        public static void AssertAssetPresent() {
            if (!PyanodonDataCacheFixture.PresetAssetExists)
                Assert.Inconclusive($"Missing assets/{TestPresetAsset.PyanodonCompressedFileName}.");
        }

        public static JsonObject LoadPreparedPresetJson() {
            AssertAssetPresent();
            TestPresetAsset.EnsurePyanodonPjsonOnDisk();
            return PresetProcessor.PrepPreset(new Preset(PyanodonDataCacheFixture.PresetName, true, true));
        }

        public static Dictionary<string, HashSet<string>> BuildCategoryMachines(JsonObject root) =>
            PresetCraftingCompatibility.BuildCraftingCategoryMachines(root);

        public static async Task<(JsonObject Root, DataCache Cache)> LoadPresetAndCacheAsync() {
            JsonObject root = LoadPreparedPresetJson();
            DataCache cache = await PyanodonDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            return (root, cache);
        }

        public static async Task<PyanodonPresetSnapshot> LoadSnapshotAsync() {
            JsonObject root = LoadPreparedPresetJson();
            return new PyanodonPresetSnapshot(
                root,
                BuildCategoryMachines(root),
                await PyanodonDataCacheFixture.GetLoadedAsync().ConfigureAwait(false));
        }

        public static void AssertEmpty(
            IReadOnlyCollection<string> failures,
            string message,
            int maxSample = 20) {
            if (failures.Count == 0)
                return;

            string sample = string.Join(", ", failures.Take(maxSample));
            if (failures.Count > maxSample)
                sample += $" … +{failures.Count - maxSample} more";

            Assert.Fail($"{message}: {sample}");
        }

        public static IEnumerable<(string ResourceName, string FluidName)> EnumerateFluidMiningResources(JsonNode root) {
            foreach (JsonNode resource in PresetJson.EnumerateArray(root, "resources")) {
                string? resourceName = PresetJson.GetString(resource, "name");
                string? fluidName = PresetJson.GetString(resource, "required_fluid");
                double? amount = PresetJson.GetDouble(resource, "fluid_amount");
                if (resourceName is not null && fluidName is not null && amount is > 0)
                    yield return (resourceName, fluidName);
            }
        }

        public static IEnumerable<JsonNode> EnumerateAllResources(JsonNode root) =>
            PresetJson.EnumerateArray(root, "resources").Concat(PresetJson.EnumerateArray(root, "water_resources"));

        public static bool TryGetRecipeNode(JsonNode recipeNode, out string name, out string category) {
            name = PresetJson.GetString(recipeNode, "name") ?? "";
            category = PresetJson.GetString(recipeNode, "category") ?? "";
            return name.Length > 0 && category.Length > 0;
        }

        public static JsonNode? FindEntity(JsonNode root, string entityName) {
            foreach (JsonNode entity in PresetJson.EnumerateArray(root, "entities")) {
                if (PresetJson.GetString(entity, "name") == entityName)
                    return entity;
            }
            return null;
        }
    }

    internal readonly struct PyanodonPresetSnapshot(
        JsonObject root,
        Dictionary<string, HashSet<string>> categoryMachines,
        DataCache cache) {
        public JsonObject Root { get; } = root;
        public Dictionary<string, HashSet<string>> CategoryMachines { get; } = categoryMachines;
        public DataCache Cache { get; } = cache;
    }
}
