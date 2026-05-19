using Foreman.DataCaching.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foreman.DataCaching.Loading {
    /// <summary>
    /// Propagates recipe/item availability (step 4 of post-load). Items that only exist as spoil/plant
    /// results stay available when their origin is available; cyclic enable/disable is detected and resolved.
    /// </summary>
    internal static class ItemAvailabilityFixpoint {
        internal static int LastIterationCount { get; private set; }
        internal static bool LastRunDetectedCycle { get; private set; }

        internal static void Run(DataCacheStore store) {
            LastIterationCount = 0;
            LastRunDetectedCycle = false;

            var seenSignatures = new HashSet<string>(StringComparer.Ordinal);
            bool clean = false;

            while (!clean) {
                clean = true;
                LastIterationCount++;

                foreach (RecipePrototype recipe in store.Recipes.Values.Where(r =>
                    r.Available &&
                    !r.Assemblers.Any(a =>
                        a.Available ||
                        ReferenceEquals(a, store.PlayerAssembler) ||
                        ReferenceEquals(a, store.RocketAssembler)))) {
                    recipe.Available = false;
                    clean = false;
                }

                foreach (ItemPrototype item in store.Items.Values.Where(i =>
                    i.Available &&
                    !i.ProductionRecipes.Any(r =>
                        r.Available && !(r.IngredientList.Count == 1 && r.IngredientList[0] == i))).Cast<ItemPrototype>()) {
                    if (ShouldRemainAvailable(item))
                        continue;

                    item.Available = false;
                    clean = false;
                    foreach (RecipePrototype r in item.ConsumptionRecipesInternal)
                        r.Available = false;
                }

                foreach (ItemPrototype item in store.Items.Values.Where(i => !i.Available).Cast<ItemPrototype>()) {
                    bool shouldBeAvailable = IsUsefulViaSpoilOrPlant(item);
                    if (item.Available == shouldBeAvailable)
                        continue;
                    item.Available = shouldBeAvailable;
                    clean = false;
                }

                if (clean)
                    break;

                string signature = BuildAvailabilitySignature(store);
                if (!seenSignatures.Add(signature)) {
                    LastRunDetectedCycle = true;
                    ErrorLogging.LogLine(
                        "Item availability fixpoint cycle detected after " + LastIterationCount +
                        " iterations; applying stable spoil/plant and consumption rules.");
                    ApplyStableItemAvailability(store);
                    break;
                }
            }
        }

        internal static bool HasUsefulConsumption(ItemPrototype item) =>
            item.ConsumptionRecipesInternal
                .Where(r => r.Available)
                .Any(r =>
                    r.IngredientListInternal.Count > 1 ||
                    r.ProductListInternal.Count > 1 ||
                    (r.ProductListInternal.Count == 1 && r.ProductListInternal[0] != item));

        internal static bool IsUsefulViaSpoilOrPlant(ItemPrototype item) =>
            item.SpoilOriginsInternal.Any(origin => origin.Available) ||
            item.PlantOriginsInternal.Any(origin => origin.Available);

        internal static bool ShouldRemainAvailable(ItemPrototype item) =>
            item.Name.StartsWith("§§", StringComparison.Ordinal) ||
            HasUsefulConsumption(item) ||
            IsUsefulViaSpoilOrPlant(item);

        private static void ApplyStableItemAvailability(DataCacheStore store) {
            foreach (ItemPrototype item in store.Items.Values.Cast<ItemPrototype>()) {
                bool hasViableProduction = item.ProductionRecipes.Any(r =>
                    r.Available && !(r.IngredientList.Count == 1 && ReferenceEquals(r.IngredientList[0], item)));
                item.Available = hasViableProduction || ShouldRemainAvailable(item);
            }
        }

        private static string BuildAvailabilitySignature(DataCacheStore store) {
            var builder = new StringBuilder(store.Items.Count * 16);
            foreach (ItemPrototype item in store.Items.Values.Cast<ItemPrototype>().OrderBy(i => i.Name, StringComparer.Ordinal))
                builder.Append(item.Name).Append(item.Available ? '1' : '0').Append('\n');
            return builder.ToString();
        }
    }
}
