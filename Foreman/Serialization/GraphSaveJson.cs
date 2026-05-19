using Foreman.DataCaching;
using System;
using System.Text.Json;

namespace Foreman.Serialization {
    /// <summary>System.Text.Json wire-format for graph save documents.</summary>
    internal static class GraphSaveJson {
        public static string SerializeProductionGraph(ProductionGraphSaveDocument document, bool writeIndented) {
            ProductionGraphWire wire = GraphSaveWireMapper.FromDocument(document);
            return JsonSerializer.Serialize(wire, GraphSaveJsonOptions.Get(writeIndented));
        }

        public static string SerializeViewer(GraphViewerSaveDocument document, bool writeIndented) {
            GraphViewerWire wire = GraphSaveWireMapper.FromDocument(document);
            return JsonSerializer.Serialize(wire, GraphSaveJsonOptions.Get(writeIndented));
        }

        public static string SerializeNodeCopyOptions(NodeCopyOptionsSaveDocument document, bool writeIndented) {
            NodeCopyOptionsWire wire = GraphSaveWireMapper.FromDocument(document);
            return JsonSerializer.Serialize(wire, GraphSaveJsonOptions.Get(writeIndented));
        }

        public static string SerializeKeyNodeClipboard(KeyNodeClipboardSaveData document, bool writeIndented) {
            KeyNodeClipboardWire wire = GraphSaveWireMapper.FromDocument(document);
            return JsonSerializer.Serialize(wire, GraphSaveJsonOptions.Get(writeIndented));
        }

        public static ProductionGraphSaveDocument? DeserializeProductionGraph(string json) {
            ProductionGraphWire? wire = JsonSerializer.Deserialize<ProductionGraphWire>(json, GraphSaveJsonOptions.Get(writeIndented: false));
            return wire is null ? null : GraphSaveWireMapper.ToProductionGraphDocument(wire);
        }

        public static GraphViewerSaveDocument? DeserializeViewer(string json) {
            GraphViewerWire? wire = JsonSerializer.Deserialize<GraphViewerWire>(json, GraphSaveJsonOptions.Get(writeIndented: false));
            if (wire is null)
                return null;
            try {
                return GraphSaveWireMapper.ToViewerDocument(wire);
            } catch (Exception ex) {
                ErrorLogging.LogException(ex, "Failed to map viewer save wire to document");
                return null;
            }
        }

        public static NodeCopyOptionsSaveDocument? DeserializeNodeCopyOptions(string json) {
            NodeCopyOptionsWire? wire = JsonSerializer.Deserialize<NodeCopyOptionsWire>(json, GraphSaveJsonOptions.Get(writeIndented: false));
            return wire is null ? null : GraphSaveWireMapper.ToNodeCopyOptionsDocument(wire);
        }

        public static KeyNodeClipboardSaveData? DeserializeKeyNodeClipboard(string json) {
            KeyNodeClipboardWire? wire = JsonSerializer.Deserialize<KeyNodeClipboardWire>(json, GraphSaveJsonOptions.Get(writeIndented: false));
            return wire is null ? null : GraphSaveWireMapper.ToKeyNodeClipboard(wire);
        }
    }
}
