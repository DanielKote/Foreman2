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
            object store = GetDataCacheStore(cache);
            ReflectionTestHelper.RequireProperty(store.GetType(), "DefaultQuality", BindingFlags.Instance | BindingFlags.Public)
                .SetValue(store, quality);
        }

        public static void SetPresetName(DataCache cache, string presetName) {
            MethodInfo setter = ReflectionTestHelper.Require(
                typeof(DataCache).GetProperty(nameof(DataCache.PresetName))?.GetSetMethod(nonPublic: true),
                "DataCache.PresetName setter was not found.");
            setter.Invoke(cache, new object[] { presetName });
        }

        private static object GetDataCacheStore(DataCache cache) =>
            ReflectionTestHelper.RequireInstance(
                ReflectionTestHelper.RequireField(typeof(DataCache), "_store", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(cache),
                "DataCache._store was null.");

        private static Dictionary<TKey, TValue> GetDictionary<TKey, TValue>(DataCache cache, string dictionaryName)
            where TKey : notnull {
            object store = GetDataCacheStore(cache);
            object? dictionary = ReflectionTestHelper.RequireProperty(store.GetType(), dictionaryName, BindingFlags.Instance | BindingFlags.Public)
                .GetValue(store);
            return (Dictionary<TKey, TValue>)ReflectionTestHelper.RequireInstance(
                dictionary,
                $"DataCache store property {dictionaryName} was null.");
        }
    }
}