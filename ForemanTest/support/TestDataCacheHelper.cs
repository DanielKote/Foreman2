using Foreman;
using System.Collections.Generic;
using System.Reflection;

namespace ForemanTest.support {
    /// <summary>Registers test-only entries in DataCache's internal store (test assembly reflection only).</summary>
    internal static class TestDataCacheHelper {
        public static FluidPrototype GetOrCreateFluid(DataCache cache, SubgroupPrototype subgroup, string name) {
            var items = GetDictionary<string, Item>(cache, "Items");
            if (items.TryGetValue(name, out var existing) && existing is FluidPrototype existingFluid)
                return existingFluid;

            var fluid = new FluidPrototype(cache, name, name, subgroup, "z") { IsTemperatureDependent = true };
            items[name] = fluid;
            return fluid;
        }

        public static ItemPrototype GetOrCreateItem(DataCache cache, SubgroupPrototype subgroup, string name) {
            var items = GetDictionary<string, Item>(cache, "Items");
            if (items.TryGetValue(name, out var existing))
                return (ItemPrototype)existing;

            var item = new ItemPrototype(cache, name, name, subgroup, "z");
            items[name] = item;
            return item;
        }

        public static void RegisterRecipe(DataCache cache, RecipePrototype recipe) =>
            GetDictionary<string, Recipe>(cache, "Recipes")[recipe.Name] = recipe;

        public static void RegisterAssembler(DataCache cache, Assembler assembler) =>
            GetDictionary<string, Assembler>(cache, "Assemblers")[assembler.Name] = assembler;

        public static void RegisterQuality(DataCache cache, Quality quality) {
            GetDictionary<string, Quality>(cache, "Qualities")[quality.Name] = quality;
            var store = typeof(DataCache).GetField("_store", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(cache)!;
            store.GetType().GetProperty("DefaultQuality")!.SetValue(store, quality);
        }

        private static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(DataCache cache, string dictionaryName)
            where TKey : notnull {
            var store = typeof(DataCache)
                .GetField("_store", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(cache)!;
            return (Dictionary<TKey, TValue>)store.GetType().GetProperty(dictionaryName)!.GetValue(store)!;
        }
    }
}