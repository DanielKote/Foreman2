using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Serialization;
using System.Collections.Generic;

namespace ForemanTest.support {
    internal static class GraphSaveTestUi {
        public static void ApplyViewerUiToGraph(GraphViewerSaveDocument saveDocument, DataCache cache, ProductionGraph graph) {
            GraphViewerUiSaveData ui = saveDocument.Ui ?? new GraphViewerUiSaveData();
            graph.SelectedRateUnit = ui.Unit;
            graph.AssemblerSelector.DefaultSelectionStyle = ui.AssemblerSelectorStyle;
            graph.ModuleSelector.DefaultSelectionStyle = ui.ModuleSelectorStyle;
            graph.EnableExtraProductivityForNonMiners = ui.ExtraProdForNonMiners;

            foreach (string fuelType in ui.FuelPriorityList) {
                if (cache.Items.TryGetValue(fuelType, out IItem? fuelItem) && fuelItem is not null)
                    graph.FuelSelector.UseFuel(fuelItem);
            }

            ApplyEnabledList(cache.Beacons.Values, cache.Beacons, ui.EnabledBeacons, (b, e) => b.Enabled = e);
            ApplyEnabledList(cache.Assemblers.Values, cache.Assemblers, ui.EnabledAssemblers, (a, e) => a.Enabled = e);
            cache.RocketAssembler?.Enabled = cache.Assemblers.TryGetValue("rocket-silo", out IAssembler? silo) && silo?.Enabled == true;
            ApplyEnabledList(cache.Modules.Values, cache.Modules, ui.EnabledModules, (m, e) => m.Enabled = e);
            ApplyEnabledList(cache.Recipes.Values, cache.Recipes, ui.EnabledRecipes, (r, e) => r.Enabled = e);
        }

        private static void ApplyEnabledList<T>(
            IEnumerable<T> all,
            IReadOnlyDictionary<string, T> byName,
            IReadOnlyList<string> enabledNames,
            System.Action<T, bool> setEnabled) where T : class {
            foreach (T item in all)
                setEnabled(item, false);
            foreach (string name in enabledNames) {
                if (byName.TryGetValue(name, out T? entry))
                    setEnabled(entry, true);
            }
        }
    }
}
