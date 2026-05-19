using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
using ForemanTest.support;
using System;
using System.Linq;

namespace ForemanTest.Graph {
    internal static class GraphSessionTestHelper {
        internal sealed class TestContext {
            public required DataCache Cache { get; init; }
            public required IQuality Quality { get; init; }
            public required SubgroupPrototype Subgroup { get; init; }

            public ItemQualityPair Item(string name) =>
                new(TestDataCacheHelper.GetOrCreateItem(Cache, Subgroup, name), Quality);

            public ProductionGraph NewGraph() => new() { DefaultAssemblerQuality = Quality };
        }

        internal static TestContext CreateContext() {
            var cache = new DataCache(filterRecipes: true);
            var subgroup = new SubgroupPrototype(cache, "§§test:subgroup", "z");
            var quality = new QualityPrototype(cache, "normal", "Normal", "a");
            TestDataCacheHelper.RegisterQuality(cache, quality);
            return new TestContext { Cache = cache, Quality = quality, Subgroup = subgroup };
        }

        internal static TestContext CreateContext(DataCache cache) {
            if (cache.DefaultQuality is null)
                throw new InvalidOperationException("DataCache must have DefaultQuality loaded.");
            SubgroupPrototype subgroup = cache.Subgroups.Values.OfType<SubgroupPrototype>().FirstOrDefault()
                ?? new SubgroupPrototype(cache, "§§test:subgroup", "z");
            return new TestContext { Cache = cache, Quality = cache.DefaultQuality, Subgroup = subgroup };
        }

        internal static ProductionGraphSession AttachSession(ProductionGraph graph) {
            var session = new ProductionGraphSession(graph);
            session.Attach();
            return session;
        }

        internal static GraphBuilder.BuiltData BuildSimpleChain() {
            var builder = GraphBuilder.Create();
            builder.Link(
                builder.Supply("Ore"),
                builder.Recipe().Input("Ore", 1).Output("Plate", 1),
                builder.Consumer("Plate").Target(10));
            return builder.Build();
        }

        internal static void WireSpoilChain(ItemPrototype input, ItemPrototype output, IQuality quality) {
            input.SpoilResult = output;
            output.SpoilOriginsInternal.Add(input);
            input.spoilageTimes[quality] = 60;
        }

        internal static PlantProcessPrototype CreatePlantProcess(TestContext ctx, string seedName, string productName) {
            var seed = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, seedName);
            var product = TestDataCacheHelper.GetOrCreateItem(ctx.Cache, ctx.Subgroup, productName);

            var process = new PlantProcessPrototype(ctx.Cache, "test-plant-" + seedName);
            process.InternalOneWayAddProduct(product, 1);
            process.Seed = seed;
            seed.PlantResult = process;
            return process;
        }
    }
}
