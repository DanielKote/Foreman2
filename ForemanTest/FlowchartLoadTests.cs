using Foreman;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ForemanTest {
    [TestClass]
    public class FlowchartLoadTests : ForemanTestBase {
        [TestInitialize]
        public void TestInitialize() {
            if (!SpaceAgeDataCacheFixture.PresetsAvailable)
                Assert.Inconclusive($"Space Age preset folder not found: {SpaceAgeDataCacheFixture.PresetsDirectory}");
        }

        [TestMethod]
        public async Task Flowchart_Load_EndToEnd_MatchesPresetAndImportsAllNodes() {
            string path = FlowchartSample.ResolvePath();
            GraphViewerSaveDocument? saveDocument = GraphSaveCodec.ReadViewer(File.ReadAllText(path));
            Assert.IsNotNull(saveDocument, "Flowchart.fjson should parse as a viewer save document.");
            Assert.AreEqual(GraphSaveFormat.SaveFormatVersion, saveDocument.Version);
            Assert.AreEqual(FlowchartSample.PresetName, saveDocument.SavedPresetName);

            ProductionGraphSaveDocument productionGraph = saveDocument.ProductionGraph;
            Assert.IsTrue(productionGraph.Nodes.Count > 0, "Flowchart should contain saved nodes.");

            var preset = new Preset(FlowchartSample.PresetName, true, true);
            PresetErrorPackage errors = await PresetProcessor.TestPreset(
                preset,
                new Dictionary<string, string>(saveDocument.IncludedMods),
                productionGraph.IncludedItems.ToList(),
                productionGraph.IncludedAssemblers.ToList(),
                productionGraph.IncludedQualities.Select(q => q.Key).ToList(),
                productionGraph.IncludedRecipes.ToList(),
                productionGraph.IncludedPlantProcesses.ToList());

            Assert.AreEqual(0, errors.MissingRecipes.Count,
                "Missing recipes: " + string.Join(", ", errors.MissingRecipes));
            Assert.AreEqual(0, errors.IncorrectRecipes.Count,
                "Incorrect recipes: " + string.Join(", ", errors.IncorrectRecipes));
            Assert.AreEqual(0, errors.MissingItems.Count,
                "Missing items: " + string.Join(", ", errors.MissingItems));
            Assert.AreEqual(0, errors.MissingMods.Count,
                "Missing mods: " + string.Join(", ", errors.MissingMods));
            Assert.AreEqual(0, errors.WrongVersionMods.Count,
                "Wrong-version mods: " + string.Join(", ", errors.WrongVersionMods));

            DataCache cache = await SpaceAgeDataCacheFixture.GetLoadedAsync();
            var graph = new ProductionGraph();
            GraphSaveTestUi.ApplyViewerUiToGraph(saveDocument, cache, graph);

            ProductionGraph.NewNodeCollection imported = GraphSaveLoader.LoadProductionGraph(
                graph, cache, productionGraph, applySolverSettings: true);

            Assert.AreEqual(
                productionGraph.Nodes.Count,
                imported.newNodes.Count,
                $"Expected every saved node to import; got {imported.newNodes.Count} of {productionGraph.Nodes.Count}.");
            Assert.IsTrue(
                imported.newLinks.Count > 0,
                "Flowchart should import at least one link.");

            graph.UpdateNodeValues();
            Assert.IsTrue(
                graph.Nodes.OfType<RecipeNode>().Any(),
                "Flowchart should contain at least one recipe node after load.");
        }
    }
}