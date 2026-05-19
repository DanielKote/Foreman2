using Foreman;
using Foreman.DataCaching;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ForemanTest.support {
    /// <summary>
    /// Loads the Pyanodon preset from a ZSTD asset in <c>assets/</c> (no .dat required).
    /// </summary>
    internal static class PyanodonDataCacheFixture {
        public static string PresetName => TestPresetAsset.PyanodonPresetName;

        private static readonly SemaphoreSlim Gate = new(1, 1);
        private static DataCache? cache;

        public static bool PresetAssetExists => TestPresetAsset.PyanodonAssetExists;

        public static async Task<DataCache> GetLoadedAsync(bool filterRecipes = true) {
            if (!PresetAssetExists)
                throw new InvalidOperationException(
                    $"Pyanodon preset asset not found. Add assets/{TestPresetAsset.PyanodonCompressedFileName} to ForemanTest.");

            TestPresetAsset.EnsurePyanodonPjsonOnDisk();

            if (cache is not null && cache.PresetName == PresetName)
                return cache;

            await Gate.WaitAsync().ConfigureAwait(false);
            try {
                if (cache is null || cache.PresetName != PresetName) {
                    cache = new DataCache(filterRecipes);
                    await cache.LoadAllData(
                        new Preset(PresetName, true, true),
                        NullProgress.Instance,
                        loadIcons: false).ConfigureAwait(false);
                }
                return cache;
            } finally {
                Gate.Release();
            }
        }

        public static void Reset() => cache = null;
    }
}
