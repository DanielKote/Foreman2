using System.Text.Json;

namespace Foreman.Serialization {
    /// <summary>Validates that graph save JSON is a supported version. Invalid JSON returns false without logging (caller treats as unsupported format).</summary>
    internal static class GraphSaveMigration {
        private static bool IsReadableVersion(int version) =>
            version >= GraphSaveFormat.MinReadableSaveVersion
            && version <= GraphSaveFormat.SaveFormatVersion;

        internal static bool IsCurrentGraph(string json) =>
            TryGetRootMetadata(json, out int version, out string? objectType)
            && IsReadableVersion(version)
            && objectType == GraphSaveFormat.GraphObject;

        internal static bool IsCurrentViewer(string json) {
            if (!TryGetRootMetadata(json, out int version, out string? objectType)
                || !IsReadableVersion(version)
                || objectType != GraphSaveFormat.ViewerObject)
                return false;

            try {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.TryGetProperty("ProductionGraph", out JsonElement graph)
                    && graph.ValueKind == JsonValueKind.Object && graph.TryGetProperty("Version", out JsonElement graphVersion)
                    && graphVersion.ValueKind == JsonValueKind.Number
                    && IsReadableVersion(graphVersion.GetInt32()) && graph.TryGetProperty("Object", out JsonElement graphObject)
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
                using var doc = JsonDocument.Parse(json);
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
