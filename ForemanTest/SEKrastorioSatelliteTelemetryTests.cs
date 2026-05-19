using Foreman;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Models;
using Foreman.Models.Nodes;
using ForemanTest.Graph;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Threading.Tasks;

namespace ForemanTest {
    [TestClass]
    public class SEKrastorioSatelliteTelemetryTests : ForemanTestBase {
        private static async Task<DataCache> LoadSEKrastorioCacheAsync() {
            if (!TestPresetAsset.SEKrastorioAssetExists)
                Assert.Inconclusive($"Missing assets/{TestPresetAsset.SEKrastorioCompressedFileName}.");

            TestPresetAsset.EnsureSEKrastorioPjsonOnDisk();
            var cache = new DataCache(filterRecipes: true);
            await cache.LoadAllData(new Preset(TestPresetAsset.SEKrastorioPresetName, true, true), NullProgress.Instance, loadIcons: false).ConfigureAwait(false);
            return cache;
        }

        [TestMethod]
        public async Task LoadAllData_SEKrastorio_SatelliteLaunchProducesTelemetry() {
            DataCache cache = await LoadSEKrastorioCacheAsync().ConfigureAwait(false);

            Assert.IsTrue(cache.Recipes.TryGetValue("§§r:rl:launch-satellite", out IRecipe? launchRecipe),
                "Navigation satellite should have a rocket launch recipe.");
            Assert.IsTrue(cache.Items.TryGetValue("se-satellite-telemetry", out IItem? telemetry));
            Assert.IsTrue(cache.Items.TryGetValue("satellite", out IItem? satellite));
            Assert.IsTrue(cache.Items.TryGetValue("rocket-part", out IItem? rocketPart));

            Assert.IsGreaterThan(0, launchRecipe!.IngredientSet[satellite],
                "Launch recipe must consume at least one satellite per launch.");
            Assert.IsGreaterThan(0, launchRecipe.ProductSet[telemetry],
                "Launch recipe must produce telemetry (SE gives 200 per satellite).");
            Assert.AreEqual(100, launchRecipe.IngredientSet[rocketPart]);
        }

        [TestMethod]
        public async Task SEKrastorio_SatelliteTelemetrySink_SolvesNonZeroRates() {
            DataCache cache = await LoadSEKrastorioCacheAsync().ConfigureAwait(false);
            Assert.IsTrue(cache.Recipes.TryGetValue("§§r:rl:launch-satellite", out IRecipe? launchRecipe));
            Assert.IsTrue(cache.Items.TryGetValue("se-satellite-telemetry", out IItem? telemetryItem));

            var ctx = GraphSessionTestHelper.CreateContext(cache);
            ProductionGraph graph = ctx.NewGraph();

            RecipeQualityPair launchPair = new(launchRecipe!, ctx.Quality);
            ItemQualityPair telemetryPair = new(telemetryItem!, ctx.Quality);

            RecipeNode launchNode = graph.CreateRecipeNode(launchPair, Point.Empty);

            ConsumerNode sink = graph.CreateConsumerNode(telemetryPair, new Point(200, 0));
            if (graph.RequestNodeController(sink) is BaseNodeController sinkController) {
                sinkController.SetRateType(RateType.Manual);
                sinkController.SetDesiredSetValue(1);
            }

            graph.CreateLink(launchNode, sink, telemetryPair);
            graph.UpdateNodeValues();

            Assert.IsGreaterThan(0, launchNode.ActualSetValue,
                "Rocket launch node should run when feeding a 1/s telemetry sink.");
            Assert.IsGreaterThan(0.9, sink.ActualRatePerSec,
                $"Telemetry sink should receive about 1/s, got {sink.ActualRatePerSec}.");
            Assert.AreEqual(200, launchRecipe!.ProductSet[telemetryItem!],
                "Each satellite launch should yield 200 telemetry in the recipe definition.");
        }
    }
}
