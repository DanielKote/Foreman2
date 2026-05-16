using System.Text.Json;

namespace Foreman {
    /// <summary>Validates that graph save JSON matches <see cref="GraphSaveFormat.SaveFormatVersion"/>. Invalid JSON returns false without logging (caller treats as unsupported format).</summary>
    internal static class GraphSaveMigration {
        internal static bool IsCurrentGraph(string json) =>
            TryGetRootMetadata(json, out int version, out string? objectType)
            && version == GraphSaveFormat.SaveFormatVersion
            && objectType == GraphSaveFormat.GraphObject;

        internal static bool IsCurrentViewer(string json) {
            if (!TryGetRootMetadata(json, out int version, out string? objectType)
                || version != GraphSaveFormat.SaveFormatVersion
                || objectType != GraphSaveFormat.ViewerObject)
                return false;

            try {
                using JsonDocument doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("ProductionGraph", out JsonElement graph)
                    || graph.ValueKind != JsonValueKind.Object)
                    return false;

                if (!graph.TryGetProperty("Version", out JsonElement graphVersion)
                    || graphVersion.ValueKind != JsonValueKind.Number
                    || graphVersion.GetInt32() != GraphSaveFormat.SaveFormatVersion)
                    return false;

                return graph.TryGetProperty("Object", out JsonElement graphObject)
                    && graphObject.ValueKind == JsonValueKind.String
                    && graphObject.GetString() == GraphSaveFormat.GraphObject;
            } catch (JsonException) {
                return false;
            }
        }

        private static bool TryGetRootMetadata(string json, out int version, out string? objectType) {
            version = 0;
            objectType = null;
            try {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                if (!root.TryGetProperty("Version", out JsonElement versionElement)
                    || versionElement.ValueKind != JsonValueKind.Number
                    || !versionElement.TryGetInt32(out version))
                    return false;

                if (!root.TryGetProperty("Object", out JsonElement objectElement)
                    || objectElement.ValueKind != JsonValueKind.String)
                    return false;

                objectType = objectElement.GetString();
                return objectType is not null;
            } catch (JsonException) {
                return false;
            }
        }
    }
}