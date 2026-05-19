using Foreman.DataCaching;
using System;
using System.IO;
using System.Text.Json.Nodes;

namespace Foreman {
    internal static class FactorioModListHelper {
        public static void SetModState(string modsPath, string modName, bool enabled, bool removeFromListWhenDisabled = false) {
            string modListPath = Path.Combine(modsPath, "mod-list.json");
            JsonObject modlist = File.Exists(modListPath) ? PresetJson.ParseObject(Utf8File.ReadAllText(modListPath)) : [];
            if (modlist["mods"] is not JsonArray modsArray) {
                modsArray = [];
                modlist["mods"] = modsArray;
            }

            JsonObject? modToken = PresetJson.FindObjectInArrayByName(modlist, "mods", modName);
            if (enabled) {
                if (modToken is null)
                    modsArray.Add(new JsonObject { ["name"] = modName, ["enabled"] = true });
                else
                    modToken["enabled"] = true;
            } else if (modToken is not null) {
                if (removeFromListWhenDisabled)
                    modsArray.Remove(modToken);
                else
                    modToken["enabled"] = false;
            }

            try {
                Utf8File.WriteAllText(modListPath, PresetJson.WriteIndented(modlist));
            } catch (Exception ex) {
                ErrorLogging.LogException(ex, string.Format(CultureInfo.InvariantCulture, "Failed to update mod-list.json at {0}", modListPath));
            }
        }
    }
}
