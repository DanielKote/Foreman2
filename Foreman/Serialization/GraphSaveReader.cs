namespace Foreman.Serialization {
    /// <summary>Parses graph save JSON into <see cref="ProductionGraphSaveDocument"/> (use <see cref="GraphSaveCodec"/> from application code).</summary>
    internal static class GraphSaveReader {
        public static ProductionGraphSaveDocument? ReadProductionGraph(string json) {
            return !GraphSaveMigration.IsCurrentGraph(json) ? null : GraphSaveJson.DeserializeProductionGraph(json);
        }

        public static GraphViewerSaveDocument? ReadProductionGraphViewer(string json) {
            return !GraphSaveMigration.IsCurrentViewer(json) ? null : GraphSaveJson.DeserializeViewer(json);
        }

        public static NodeCopyOptionsSaveDocument? ReadNodeCopyOptions(string json) =>
            GraphSaveJson.DeserializeNodeCopyOptions(json);

        public static KeyNodeClipboardSaveData? ReadKeyNodeClipboard(string json) =>
            GraphSaveJson.DeserializeKeyNodeClipboard(json);
    }
}
