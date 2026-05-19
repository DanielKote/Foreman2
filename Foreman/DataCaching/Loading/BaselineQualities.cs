using Foreman.DataCaching.DataTypes;
using System.Collections.Generic;

namespace Foreman.DataCaching.Loading {
    /// <summary>
    /// Factorio presets without the quality feature still need a default tier for Foreman.
    /// These names match the synthetic qualities injected by the export mod when <c>prototypes.quality</c> is absent.
    /// </summary>
    internal static class BaselineQualities {
        internal const string NormalName = "normal";
        internal const string UnknownName = "quality-unknown";
        internal const string NormalDisplayName = "Normal";
        internal const string UnknownDisplayName = "Unknown";

        internal static string? GetDisplayName(string qualityName) => qualityName switch {
            NormalName => NormalDisplayName,
            UnknownName => UnknownDisplayName,
            _ => null
        };

        internal static void EnsurePresent(DataCache owner, DataCacheStore store, IReadOnlyDictionary<string, IconColorPair> iconCache) {
            if (store.Qualities.ContainsKey(NormalName))
                return;

            AddQuality(owner, store, iconCache, NormalName, NormalDisplayName, "a", hidden: true);
            AddQuality(owner, store, iconCache, UnknownName, UnknownDisplayName, "z", hidden: true);
        }

        private static void AddQuality(
            DataCache owner,
            DataCacheStore store,
            IReadOnlyDictionary<string, IconColorPair> iconCache,
            string name,
            string displayName,
            string order,
            bool hidden) {
            QualityPrototype quality = new(owner, name, displayName, order) {
                Level = 0,
                BeaconPowerMultiplier = 1,
                MiningDrillResourceDrainMultiplier = 1,
                Available = !hidden,
                Enabled = true
            };

            if (iconCache.TryGetValue("icon.q." + name, out var icon))
                quality.SetIconAndColor(icon);

            store.Qualities.Add(name, quality);
        }
    }
}
