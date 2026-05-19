using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Foreman.Serialization {
    /// <summary>JSON serialization for <see cref="AnnotationSaveData"/> (save files and clipboard).</summary>
    public static class AnnotationJson {
        public static AnnotationSaveData? Deserialize(JsonNode node) {
            try {
                return node.Deserialize<AnnotationSaveData>(GraphSaveJsonOptions.Get(writeIndented: false));
            } catch (JsonException) {
                return null;
            }
        }

        public static IReadOnlyList<AnnotationSaveData> DeserializeList(JsonArray array) =>
            [.. array.OfType<JsonNode>().Select(static item => Deserialize(item)).OfType<AnnotationSaveData>()];

        public static IReadOnlyList<AnnotationSaveData>? DeserializeListFromRoot(JsonNode? root, string propertyName = "Annotations") {
            if (root?[propertyName] is not JsonArray array || array.Count == 0)
                return null;
            IReadOnlyList<AnnotationSaveData> list = DeserializeList(array);
            return list.Count > 0 ? list : null;
        }

        public static JsonNode? SerializeToNode(AnnotationSaveData data) =>
            JsonSerializer.SerializeToNode(data, GraphSaveJsonOptions.Get(writeIndented: false));

        public static JsonArray SerializeToArray(IEnumerable<AnnotationSaveData> annotations) =>
            new([.. annotations.Select(SerializeToNode).OfType<JsonNode>()]);
    }
}
