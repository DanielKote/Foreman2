using Foreman;
using Foreman.DataCaching;
using Foreman.Serialization;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ForemanTest {
    [TestClass]
    public class FlowchartLoadTests : ForemanTestBase {
        public TestContext? TestContext { get; set; }
        [TestInitialize]
        public void TestInitialize() {
            if (!SpaceAgeDataCacheFixture.PresetsAvailable)
                Assert.Inconclusive($"Space Age preset folder not found: {SpaceAgeDataCacheFixture.PresetsDirectory}");
        }

        [TestMethod]
        public async Task Flowchart_Load_EndToEnd_MatchesPresetAndImportsAllNodes() {
            string path = FlowchartSample.ResolvePath();

            Assert.IsNotNull(TestContext);
            GraphViewerSaveDocument? saveDocument = GraphSaveCodec.ReadViewer(await File.ReadAllTextAsync(path, TestContext.CancellationToken).ConfigureAwait(false));
            Assert.IsNotNull(saveDocument, "Flowchart.fjson should parse as a viewer save document.");
            Assert.AreEqual(GraphSaveFormat.SaveFormatVersion, saveDocument.Version);
            Assert.AreEqual(FlowchartSample.PresetName, saveDocument.SavedPresetName);

            ProductionGraphSaveDocument productionGraph = saveDocument.ProductionGraph;
            Assert.IsNotEmpty(productionGraph.Nodes, "Flowchart should contain saved nodes.");

            var preset = new Preset(FlowchartSample.PresetName, true, true);
            PresetErrorPackage errors = await PresetProcessor.TestPreset(
                preset,
                new Dictionary<string, string>(saveDocument.IncludedMods),
                [.. productionGraph.IncludedItems],
                [.. productionGraph.IncludedQualities.Select(q => q.Key)],
                [.. productionGraph.IncludedRecipes],
                [.. productionGraph.IncludedPlantProcesses]).ConfigureAwait(false);

            Assert.IsEmpty(errors.MissingRecipes,
                "Missing recipes: " + string.Join(", ", errors.MissingRecipes));
            Assert.IsEmpty(errors.IncorrectRecipes,
                "Incorrect recipes: " + string.Join(", ", errors.IncorrectRecipes));
            Assert.IsEmpty(errors.MissingItems,
                "Missing items: " + string.Join(", ", errors.MissingItems));
            Assert.IsEmpty(errors.MissingMods,
                "Missing mods: " + string.Join(", ", errors.MissingMods));
            Assert.IsEmpty(errors.WrongVersionMods,
                "Wrong-version mods: " + string.Join(", ", errors.WrongVersionMods));

            DataCache cache = await SpaceAgeDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            var graph = new ProductionGraph();
            GraphSaveTestUi.ApplyViewerUiToGraph(saveDocument, cache, graph);

            ProductionGraph.NewNodeBatch imported = GraphSaveLoader.LoadProductionGraph(
                graph, cache, productionGraph, applySolverSettings: true);

            Assert.HasCount(
                productionGraph.Nodes.Count,
                imported.NewNodes,
                $"Expected every saved node to import; got {imported.NewNodes.Count} of {productionGraph.Nodes.Count}.");
            Assert.IsNotEmpty(
                imported.NewLinks,
                "Flowchart should import at least one link.");

            graph.UpdateNodeValues();
            Assert.IsNotEmpty(
                graph.Nodes.OfType<RecipeNode>(),
                "Flowchart should contain at least one recipe node after load.");
        }
    }
}
