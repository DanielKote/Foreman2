using Foreman.DataCaching;
using Foreman.ProductionGraphView;
using System;

namespace Foreman.Serialization {
    /// <summary>
    /// Graph .fjson pipeline: domain ↔ <see cref="GraphViewerSaveDocument"/> ↔ JSON text.
    /// Application code should use this type only.
    /// Wire format uses System.Text.Json. Increment <see cref="GraphSaveFormat.SaveFormatVersion"/> when necessary.
    /// </summary>
    public static class GraphSaveCodec {

        // --- Build documents from live state ---

        public static ProductionGraphSaveDocument BuildProductionGraph(ProductionGraph graph) =>
            GraphSaveWriter.WriteProductionGraph(graph);

        public static GraphViewerSaveDocument BuildViewer(ProductionGraphViewer viewer) {
            return viewer.DCache is not DataCache cache
                ? throw new InvalidOperationException("Cannot serialize ProductionGraphViewer without a data cache.")
                : GraphSaveWriter.WriteViewer(viewer, cache);
        }

        public static NodeCopyOptionsSaveDocument BuildNodeCopyOptions(NodeCopyOptions options) =>
            GraphSaveWriter.WriteNodeCopyOptions(options);

        public static KeyNodeClipboardSaveData BuildKeyNodeClipboard(bool keyNode, string title) =>
            GraphSaveWriter.WriteKeyNodeClipboard(keyNode, title);

        // --- Parse JSON text to documents ---

        public static ProductionGraphSaveDocument? ReadProductionGraph(string json) =>
            GraphSaveReader.ReadProductionGraph(json);

        public static GraphViewerSaveDocument? ReadViewer(string json) =>
            GraphSaveReader.ReadProductionGraphViewer(json);

        /// <summary>Reads a production graph from a graph fragment, or from a full viewer save file.</summary>
        public static ProductionGraphSaveDocument? ReadGraphPayload(string json) {
            return ReadProductionGraph(json) is ProductionGraphSaveDocument graphDocument
                ? graphDocument
                : ReadViewer(json) is GraphViewerSaveDocument viewerDocument ? viewerDocument.ProductionGraph : null;
        }

        public static NodeCopyOptionsSaveDocument? ReadNodeCopyOptions(string json) =>
            GraphSaveReader.ReadNodeCopyOptions(json);

        public static KeyNodeClipboardSaveData? ReadKeyNodeClipboard(string json) =>
            GraphSaveReader.ReadKeyNodeClipboard(json);

        // --- Write documents to JSON text ---

        public static string WriteProductionGraphDocumentToString(
            ProductionGraphSaveDocument document,
            bool writeIndented = false) =>
            GraphSaveJson.SerializeProductionGraph(document, writeIndented);

        public static string WriteViewerDocumentToString(
            GraphViewerSaveDocument document,
            bool writeIndented = true) =>
            GraphSaveJson.SerializeViewer(document, writeIndented);

        public static string WriteProductionGraphToString(ProductionGraph graph, bool writeIndented = false) =>
            WriteProductionGraphDocumentToString(BuildProductionGraph(graph), writeIndented);

        public static string WriteViewerToString(ProductionGraphViewer viewer, bool writeIndented = true) =>
            WriteViewerDocumentToString(BuildViewer(viewer), writeIndented);

        public static string WriteNodeCopyOptionsToString(NodeCopyOptions options, bool writeIndented = false) =>
            GraphSaveJson.SerializeNodeCopyOptions(BuildNodeCopyOptions(options), writeIndented);

        public static string WriteKeyNodeClipboardToString(bool keyNode, string title, bool writeIndented = false) =>
            GraphSaveJson.SerializeKeyNodeClipboard(BuildKeyNodeClipboard(keyNode, title), writeIndented);
    }
}
