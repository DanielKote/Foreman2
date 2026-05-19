using Foreman;
using ForemanTest.support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ForemanTest {
    [TestClass]
    public class GraphSaveCodecTests : ForemanTestBase {
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
            Assert.IsTrue(json.GetProperty("IncludedItems").GetArrayLength() >= 2);
        }

        [TestMethod]
        public void GraphSaveCodec_BuildProductionGraph_MatchesJsonRoundTrip() {
            var data = BuildSimpleChain();
            ProductionGraphSaveDocument built = GraphSaveCodec.BuildProductionGraph(data.Graph);
            ProductionGraphSaveDocument? fromJson = GraphSaveCodec.ReadProductionGraph(
                GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: false));

            Assert.IsNotNull(fromJson);
            Assert.AreEqual(built.Nodes.Count, fromJson.Nodes.Count);
            Assert.AreEqual(built.Links.Count, fromJson.Links.Count);
            Assert.AreEqual(built.IncludedItems.Count, fromJson.IncludedItems.Count);
        }

        [TestMethod]
        public void GraphSaveCodec_ReadProductionGraph_MatchesSerializedChain() {
            var data = BuildSimpleChain();
            string json = GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: false);

            ProductionGraphSaveDocument? document = GraphSaveCodec.ReadProductionGraph(json);

            Assert.IsNotNull(document);
            Assert.AreEqual(3, document.Nodes.Count);
            Assert.AreEqual(2, document.Links.Count);
            Assert.IsTrue(document.Nodes.Any(n => n is RecipeNodeSaveData));
            Assert.IsTrue(document.Nodes.Any(n => n is SupplierNodeSaveData));
            Assert.IsTrue(document.Nodes.Any(n => n is ConsumerNodeSaveData));
            Assert.IsNotNull(document.Solver);
            Assert.IsTrue(document.IncludedItems.Contains("Ore"));
            Assert.IsTrue(document.IncludedItems.Contains("Plate"));
        }

        [TestMethod]
        public void GraphSaveCodec_ReadProductionGraph_InvalidObject_ReturnsNull() {
            var data = BuildSimpleChain();
            JsonNode? parsed = JsonNode.Parse(GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: false));
            Assert.IsNotNull(parsed);
            JsonNode json = parsed;
            json["Object"] = "NotAProductionGraph";

            Assert.IsNull(GraphSaveCodec.ReadProductionGraph(json.ToJsonString()));
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
            Assert.AreEqual(3, payload.Nodes.Count);
        }

        [TestMethod]
        public void GraphSaveLoader_LoadFromDocument_MatchesInsertNodesFromFragment() {
            var data = BuildSimpleChain();
            ProductionGraphSaveDocument document = GraphSaveCodec.BuildProductionGraph(data.Graph);

            foreach (var node in data.Graph.Nodes.ToList())
                data.Graph.DeleteNode(node);

            var viaDocument = data.Graph.InsertNodesFromDocument(data.Cache, document, applySolverSettings: true);
            Assert.AreEqual(3, viaDocument.newNodes.Count);
            Assert.AreEqual(2, viaDocument.newLinks.Count);

            foreach (var node in data.Graph.Nodes.ToList())
                data.Graph.DeleteNode(node);

            string fragmentJson = GraphSaveCodec.WriteProductionGraphDocumentToString(document, writeIndented: false);
            var viaFragment = data.Graph.InsertNodesFromFragment(data.Cache, fragmentJson, applySolverSettings: true);
            Assert.AreEqual(3, viaFragment.newNodes.Count);
            Assert.AreEqual(2, viaFragment.newLinks.Count);
        }

        [TestMethod]
        public void SerializeProductionGraph_SecondSerializeMatchesFirst() {
            var data = BuildSimpleChain();
            string first = GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: true);
            string second = GraphSaveCodec.WriteProductionGraphToString(data.Graph, writeIndented: true);
            Assert.AreEqual(first, second);
        }

        [TestMethod]
        public async Task Flowchart_LoadedGraphSerialize_IsStableAndDiffersFromRawFile() {
            string path = FlowchartSample.ResolvePath();
            string disk = File.ReadAllText(path);
            var cache = await SpaceAgeDataCacheFixture.GetLoadedAsync();
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
                    new JsonSerializerOptions { WriteIndented = true })
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

            Assert.AreEqual(3, imported.newNodes.Count);
            Assert.AreEqual(2, imported.newLinks.Count);
            Assert.IsTrue(imported.newNodes.OfType<ConsumerNode>().Any());
            Assert.IsTrue(imported.newNodes.OfType<RecipeNode>().Any());
            Assert.IsTrue(imported.newNodes.OfType<SupplierNode>().Any());
            Assert.IsTrue(data.Graph.PullOutputNodes);
            Assert.AreEqual(42, data.Graph.PullOutputNodesPower);
            Assert.AreEqual(7, data.Graph.LowPriorityPower);
        }

        [TestMethod]
        public void SerializeProductionGraph_SubsetHonorsSerializeNodeIdSet() {
            var data = BuildSimpleChain();
            var recipeNode = data.Graph.Nodes.OfType<RecipeNode>().Single();

            data.Graph.SerializeNodeIdSet = new HashSet<int> { recipeNode.NodeID };
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
            TestDataCacheHelper.RegisterAssembler(data.Cache, recipeNode.SelectedAssembler.Assembler);

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
            TestDataCacheHelper.RegisterAssembler(data.Cache, recipeNode.SelectedAssembler.Assembler);
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