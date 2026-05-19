using Foreman;
using Foreman.DataCaching.DataTypes;
using Foreman.Models.Nodes;
using Foreman.ProductionGraphView;
using Foreman.Serialization;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ForemanTest {
    [TestClass]
    public class GraphSaveCodecTests : ForemanTestBase {
        public TestContext? TestContext { get; set; }
        [TestMethod]
        public void SerializeProductionGraph_ProducesExpectedDocumentShape() {
            var data = BuildSimpleChain();
            JsonElement json = JsonDocument.Parse(
                GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: false)).RootElement;

            Assert.AreEqual(GraphSaveFormat.SaveFormatVersion, json.GetProperty("Version").GetInt32());
            Assert.AreEqual(GraphSaveFormat.GraphObject, json.GetProperty("Object").GetString());
            Assert.AreEqual(JsonValueKind.Array, json.GetProperty("Nodes").ValueKind);
            Assert.AreEqual(JsonValueKind.Array, json.GetProperty("NodeLinks").ValueKind);
            Assert.AreEqual(JsonValueKind.Array, json.GetProperty("IncludedItems").ValueKind);
            Assert.IsGreaterThanOrEqualTo(2, json.GetProperty("IncludedItems").GetArrayLength());
        }

        [TestMethod]
        public void GraphSaveCodec_BuildProductionGraph_MatchesJsonRoundTrip() {
            var data = BuildSimpleChain();
            ProductionGraphSaveDocument built = GraphSaveCodec.BuildProductionGraph(data.Graph);
            ProductionGraphSaveDocument? fromJson = GraphSaveCodec.ReadProductionGraph(
                GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: false));

            Assert.IsNotNull(fromJson);
            Assert.HasCount(built.Nodes.Count, fromJson.Nodes);
            Assert.HasCount(built.Links.Count, fromJson.Links);
            Assert.HasCount(built.IncludedItems.Count, fromJson.IncludedItems);
        }

        [TestMethod]
        public void GraphSaveCodec_ReadProductionGraph_MatchesSerializedChain() {
            var data = BuildSimpleChain();
            string json = GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: false);

            ProductionGraphSaveDocument? document = GraphSaveCodec.ReadProductionGraph(json);

            Assert.IsNotNull(document);
            Assert.HasCount(3, document.Nodes);
            Assert.HasCount(2, document.Links);
            Assert.Contains(n => n is RecipeNodeSaveData, document.Nodes);
            Assert.Contains(n => n is SupplierNodeSaveData, document.Nodes);
            Assert.Contains(n => n is ConsumerNodeSaveData, document.Nodes);
            Assert.IsNotNull(document.Solver);
            Assert.Contains("Ore", document.IncludedItems);
            Assert.Contains("Plate", document.IncludedItems);
        }

        [TestMethod]
        public void GraphSaveCodec_ReadProductionGraph_InvalidObject_ReturnsNull() {
            var data = BuildSimpleChain();
            var parsed = JsonNode.Parse(GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: false));
            Assert.IsNotNull(parsed);
            JsonNode json = parsed;
            json["Object"] = "NotAProductionGraph";

            Assert.IsNull(GraphSaveCodec.ReadProductionGraph(json.ToJsonString()));
        }

        [TestMethod]
        public void GraphSaveCodec_Annotations_RoundTripThroughViewerDocument() {
            var data = BuildSimpleChain();
            var annotations = new List<AnnotationSaveData> {
                new TextAnnotationSaveData {
                    X = 100,
                    Y = 200,
                    Width = 150,
                    Height = 40,
                    Text = "Hello",
                    FontFamily = "Segoe UI",
                    FontSize = 14f,
                    TextColor = new ColorSaveData(255, 0, 0, 0),
                    BackColor = new ColorSaveData(0, 255, 255, 255),
                    TextAlign = 1
                },
                new ShapeAnnotationSaveData {
                    X = 50,
                    Y = 60,
                    Width = 200,
                    Height = 100,
                    ShapeType = "Ellipse",
                    FillColor = new ColorSaveData(80, 80, 160, 255),
                    BorderColor = new ColorSaveData(255, 60, 120, 220),
                    BorderWidth = 2
                }
            };
            GraphViewerSaveDocument original = new() {
                Version = GraphSaveFormat.SaveFormatVersion,
                ProductionGraph = GraphSaveCodec.BuildProductionGraph(data.Graph),
                Annotations = annotations,
                AnnotationDpi = 120
            };

            string json = GraphSaveCodec.WriteViewerDocumentToString(original, writeIndented: false);
            GraphViewerSaveDocument? restored = GraphSaveCodec.ReadViewer(json);

            Assert.IsNotNull(restored);
            Assert.AreEqual(120, restored.AnnotationDpi);
            Assert.HasCount(2, restored.Annotations);

            var text = restored.Annotations.OfType<TextAnnotationSaveData>().Single();
            Assert.AreEqual("Hello", text.Text);
            Assert.AreEqual(100, text.X);

            var shape = restored.Annotations.OfType<ShapeAnnotationSaveData>().Single();
            Assert.AreEqual("Ellipse", shape.ShapeType);
            Assert.AreEqual(2, shape.BorderWidth);
        }

        [TestMethod]
        public void GraphSaveCodec_ReadGraphPayload_AcceptsViewerSaveFile() {
            var data = BuildSimpleChain();
            GraphViewerSaveDocument viewerDoc = new() {
                Version = GraphSaveFormat.SaveFormatVersion,
                SavedPresetName = data.Cache.PresetName,
                ProductionGraph = GraphSaveCodec.BuildProductionGraph(data.Graph)
            };
            string json = GraphSaveCodec.WriteViewerDocumentToString(viewerDoc, writeIndented: false);

            ProductionGraphSaveDocument? payload = GraphSaveCodec.ReadGraphPayload(json);
            Assert.IsNotNull(payload);
            Assert.HasCount(3, payload.Nodes);
        }

        [TestMethod]
        public void GraphSaveLoader_LoadFromDocument_MatchesInsertNodesFromFragment() {
            var data = BuildSimpleChain();
            ProductionGraphSaveDocument document = GraphSaveCodec.BuildProductionGraph(data.Graph);

            foreach (var node in data.Graph.Nodes.ToList())
                data.Graph.DeleteNode(node);

            var viaDocument = data.Graph.InsertNodesFromDocument(data.Cache, document, applySolverSettings: true);
            Assert.HasCount(3, viaDocument.NewNodes);
            Assert.HasCount(2, viaDocument.NewLinks);

            foreach (var node in data.Graph.Nodes.ToList())
                data.Graph.DeleteNode(node);

            string fragmentJson = GraphSaveCodec.WriteProductionGraphDocumentToString(document, writeIndented: false);
            var viaFragment = data.Graph.InsertNodesFromFragment(data.Cache, fragmentJson, applySolverSettings: true);
            Assert.HasCount(3, viaFragment.NewNodes);
            Assert.HasCount(2, viaFragment.NewLinks);
        }

        [TestMethod]
        public void SerializeProductionGraph_SecondSerializeMatchesFirst() {
            var data = BuildSimpleChain();
            string first = GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: true);
            string second = GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: true);
            Assert.AreEqual(first, second);
        }

        private static readonly JsonSerializerOptions opts = new() { WriteIndented = true };
        [TestMethod]
        public async Task Flowchart_LoadedGraphSerialize_IsStableAndDiffersFromRawFile() {
            string path = FlowchartSample.ResolvePath();
            Assert.IsNotNull(TestContext);
            string disk = await File.ReadAllTextAsync(path, TestContext.CancellationToken).ConfigureAwait(false);
            var cache = await SpaceAgeDataCacheFixture.GetLoadedAsync().ConfigureAwait(false);
            GraphViewerSaveDocument? saveDocument = GraphSaveCodec.ReadViewer(disk);
            Assert.IsNotNull(saveDocument);

            var graph = new ProductionGraph();
            GraphSaveTestUi.ApplyViewerUiToGraph(saveDocument, cache, graph);
            GraphSaveLoader.LoadProductionGraph(graph, cache, saveDocument.ProductionGraph, applySolverSettings: true);
            graph.UpdateNodeValues();

            string once = GraphSaveCodec.WriteProductionGraphToString(graph, writeIndented: true);
            string twice = GraphSaveCodec.WriteProductionGraphToString(graph, writeIndented: true);
            Assert.AreEqual(once, twice, "In-memory graph serialization should be stable for dirty detection.");

            string diskGraph = saveDocument.ProductionGraph is not null
                ? JsonSerializer.Serialize(
                    JsonDocument.Parse(disk).RootElement.GetProperty("ProductionGraph"),
                    opts)
                : "";
            Assert.AreNotEqual(diskGraph, once,
                "On-disk graph JSON may differ in array ordering from a round-trip; MainForm compares to a post-load baseline, not the raw file.");
        }

        [TestMethod]
        public void SerializeProductionGraph_RoundTrip_RestoresNodesLinksAndSolverSettings() {
            var data = BuildSimpleChain();
            data.Graph.PullOutputNodes = true;
            data.Graph.PullOutputNodesPower = 42;
            data.Graph.LowPriorityPower = 7;

            ProductionGraphSaveDocument document = GraphSaveCodec.BuildProductionGraph(data.Graph);

            foreach (var node in data.Graph.Nodes.ToList())
                data.Graph.DeleteNode(node);

            var imported = data.Graph.InsertNodesFromDocument(data.Cache, document, applySolverSettings: true);

            Assert.HasCount(3, imported.NewNodes);
            Assert.HasCount(2, imported.NewLinks);
            Assert.IsNotEmpty(imported.NewNodes.OfType<ConsumerNode>());
            Assert.IsNotEmpty(imported.NewNodes.OfType<RecipeNode>());
            Assert.IsNotEmpty(imported.NewNodes.OfType<SupplierNode>());
            Assert.IsTrue(data.Graph.PullOutputNodes);
            Assert.AreEqual(42, data.Graph.PullOutputNodesPower);
            Assert.AreEqual(7, data.Graph.LowPriorityPower);
        }

        [TestMethod]
        public void SerializeProductionGraph_SubsetHonorsSerializeNodeIdSet() {
            var data = BuildSimpleChain();
            var recipeNode = data.Graph.Nodes.OfType<RecipeNode>().Single();

            data.Graph.SerializeNodeIdSet = [recipeNode.NodeID];
            JsonElement json = JsonDocument.Parse(
                GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: false)).RootElement;
            data.Graph.SerializeNodeIdSet = null;

            Assert.AreEqual(1, json.GetProperty("Nodes").GetArrayLength());
            Assert.AreEqual(0, json.GetProperty("NodeLinks").GetArrayLength());
        }

        [TestMethod]
        public void GraphSaveCodec_ReadNodeCopyOptions_MatchesSerializedPayload() {
            var data = BuildSimpleChain();
            var recipeNode = data.Graph.Nodes.OfType<RecipeNode>().Single();
            TestDataCacheHelper.RegisterQuality(data.Cache, recipeNode.SelectedAssembler.Quality);
            TestDataCacheHelper.RegisterAssembler(data.Cache, (AssemblerPrototype)recipeNode.SelectedAssembler.Assembler);

            NodeCopyOptionsSaveDocument built = GraphSaveCodec.BuildNodeCopyOptions(new NodeCopyOptions(recipeNode));
            NodeCopyOptionsSaveDocument? document = GraphSaveCodec.ReadNodeCopyOptions(
                GraphSaveCodec.WriteNodeCopyOptionsToString(new NodeCopyOptions(recipeNode)));

            Assert.IsNotNull(document);
            Assert.AreEqual(built.AssemblerName, document.AssemblerName);
            Assert.AreEqual(recipeNode.SelectedAssembler.Assembler.Name, document.AssemblerName);
            Assert.AreEqual(recipeNode.SelectedAssembler.Quality.Name, document.AssemblerQualityName);
        }

        [TestMethod]
        public void SerializeNodeCopyOptions_RoundTrip_RestoresAssemblerAndModules() {
            var data = BuildSimpleChain();
            var recipeNode = data.Graph.Nodes.OfType<RecipeNode>().Single();
            TestDataCacheHelper.RegisterQuality(data.Cache, recipeNode.SelectedAssembler.Quality);
            TestDataCacheHelper.RegisterAssembler(data.Cache, (AssemblerPrototype)recipeNode.SelectedAssembler.Assembler);
            var original = new NodeCopyOptions(recipeNode);

            var restored = NodeCopyOptions.GetNodeCopyOptions(
                GraphSaveCodec.BuildNodeCopyOptions(original),
                data.Cache);

            Assert.IsNotNull(restored);
            Assert.AreEqual(original.Assembler.Assembler.Name, restored.Assembler.Assembler.Name);
            Assert.AreEqual(original.Assembler.Quality.Name, restored.Assembler.Quality.Name);
            Assert.AreEqual(original.NeighbourCount, restored.NeighbourCount);
            Assert.AreEqual(original.ExtraProductivityBonus, restored.ExtraProductivityBonus);
        }

        [TestMethod]
        public void SerializeKeyNodeClipboard_ParsesLegacyTupleKeys() {
            KeyNodeClipboardSaveData? document = GraphSaveCodec.ReadKeyNodeClipboard(
                GraphSaveCodec.WriteKeyNodeClipboardToString(true, "Main bus"));
            Assert.IsNotNull(document);
            Assert.IsTrue(document.KeyNode);
            Assert.AreEqual("Main bus", document.Title);
        }

        [TestMethod]
        public void ReadViewer_LegacySaveVersion_ReturnsNull() {
            string path = LegacySaveSample.ResolvePath();
            JsonElement save = JsonDocument.Parse(File.ReadAllText(path)).RootElement;
            Assert.AreNotEqual(GraphSaveFormat.SaveFormatVersion, save.GetProperty("Version").GetInt32());
            Assert.IsNull(GraphSaveCodec.ReadViewer(File.ReadAllText(path)));
        }

        private static GraphBuilder.BuiltData BuildSimpleChain() {
            var builder = GraphBuilder.Create();
            builder.Link(
                builder.Supply("Ore"),
                builder.Recipe().Input("Ore", 1).Output("Plate", 1),
                builder.Consumer("Plate").Target(10));
            return builder.Build();
        }

    }
}
