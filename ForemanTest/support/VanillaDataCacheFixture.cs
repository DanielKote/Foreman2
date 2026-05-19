using Foreman;
using Foreman.DataCaching;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ForemanTest.support {
    /// <summary>Loads the vanilla preset once per test run (expensive).</summary>
    internal static class VanillaDataCacheFixture {
        public const string PresetName = "Factorio 2.0 Vanilla";

        private static readonly SemaphoreSlim Gate = new(1, 1);
        private static DataCache? cache;

        public static string PresetsDirectory =>
            Path.Combine(AppContext.BaseDirectory, "Presets");

        public static bool PresetsAvailable => Directory.Exists(PresetsDirectory);

        public static async Task<DataCache> GetLoadedAsync(bool filterRecipes = true) {
            if (cache is not null && cache.PresetName == PresetName)
                return cache;

            await Gate.WaitAsync().ConfigureAwait(false);
            try {
                if (cache is null || cache.PresetName != PresetName) {
                    var next = new DataCache(filterRecipes);
                    await next.LoadAllData(
                        new Preset(PresetName, true, true),
                        NullProgress.Instance,
                        loadIcons: false).ConfigureAwait(false);
                    cache = next;
                }
                return cache;
            } finally {
                Gate.Release();
            }
        }

        public static void Reset() => cache = null;
    }
}
