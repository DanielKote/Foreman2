namespace Foreman {
    /// <summary>Parses graph save JSON into <see cref="GraphSaveDocuments"/> (use <see cref="GraphSaveCodec"/> from application code).</summary>
    internal static class GraphSaveReader {
        public static ProductionGraphSaveDocument? ReadProductionGraph(string json) {
            if (!GraphSaveMigration.IsCurrentGraph(json))
                return null;
            return GraphSaveJson.DeserializeProductionGraph(json);
        }

        public static GraphViewerSaveDocument? ReadProductionGraphViewer(string json) {
            if (!GraphSaveMigration.IsCurrentViewer(json))
                return null;
            return GraphSaveJson.DeserializeViewer(json);
        }

        public static NodeCopyOptionsSaveDocument? ReadNodeCopyOptions(string json) =>
            GraphSaveJson.DeserializeNodeCopyOptions(json);

        public static KeyNodeClipboardSaveData? ReadKeyNodeClipboard(string json) =>
            GraphSaveJson.DeserializeKeyNodeClipboard(json);
    }
}