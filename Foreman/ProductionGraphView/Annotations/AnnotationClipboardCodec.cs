using Foreman.Serialization;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Foreman.ProductionGraphView.Annotations {
    /// <summary>Reads and writes annotation payloads embedded in graph clipboard JSON.</summary>
    public static class AnnotationClipboardCodec {
        public static IReadOnlyList<AnnotationSaveData>? ReadAnnotations(string json) {
            try {
                var root = JsonNode.Parse(json);
                return AnnotationJson.DeserializeListFromRoot(root);
            } catch (JsonException) {
                return null;
            }
        }

        public static string MergeAnnotationsIntoFragment(
            string productionGraphFragmentJson,
            IEnumerable<AnnotationSaveData> annotations) {
            JsonNode? parsed;
            try {
                parsed = JsonNode.Parse(productionGraphFragmentJson);
            } catch (JsonException) {
                return productionGraphFragmentJson;
            }

            if (parsed is not JsonObject root)
                return productionGraphFragmentJson;

            root["Annotations"] = AnnotationJson.SerializeToArray(annotations);
            return root.ToJsonString(GraphSaveJsonOptions.Get(writeIndented: false));
        }
    }
}
