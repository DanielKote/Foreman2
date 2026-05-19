using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Foreman {
    /// <summary>System.Text.Json helpers for preset / export JSON (replaces Newtonsoft.Linq patterns).</summary>
    internal static class PresetJson {
        private static readonly JsonSerializerOptions IndentedWrite = new() { WriteIndented = true };

        public static JsonObject ParseObject(string json) {
            JsonNode? node = JsonNode.Parse(json) ?? throw new JsonException("Preset JSON root was null.");
            return node is not JsonObject obj ? throw new JsonException("Preset JSON root was not an object.") : obj;
        }

        /// <summary>Returns null when JSON is not an object; <see cref="JsonException"/> is expected and not logged.</summary>
        public static JsonObject? TryParseObject(string json) {
            try {
                return JsonNode.Parse(json) as JsonObject;
            } catch (JsonException) {
                return null;
            }
        }

        public static string WriteIndented(JsonNode node) => node.ToJsonString(IndentedWrite);

        public static JsonNode? GetNode(JsonNode? node, params string[] path) {
            foreach (string segment in path) {
                if (node is null)
                    return null;
                node = node[segment];
            }
            return node;
        }

        public static string? GetString(JsonNode? node, params string[] path) => GetStringValue(GetNode(node, path));

        public static int? GetInt32(JsonNode? node, params string[] path) => GetInt32Value(GetNode(node, path));

        public static double? GetDouble(JsonNode? node, params string[] path) => GetDoubleValue(GetNode(node, path));

        public static bool? GetBool(JsonNode? node, params string[] path) => GetBoolValue(GetNode(node, path));

        public static int? GetInt32At(JsonNode? parent, string propertyName, int index) {
            return GetNode(parent, propertyName) is not JsonArray array || index < 0 || index >= array.Count ? null : GetInt32Value(array[index]);
        }

        public static int CountArray(JsonNode? parent, string propertyName) =>
            GetNode(parent, propertyName) is JsonArray array ? array.Count : 0;

        public static string? GetStringValue(JsonNode? node) {
            return node is null
                ? null
                : node.GetValueKind() switch {
                    JsonValueKind.String => node.GetValue<string>(),
                    JsonValueKind.Number => node.ToJsonString(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => null
                };
        }

        public static int? GetInt32Value(JsonNode? node) {
            if (node is null)
                return null;
            if (node is JsonValue value && value.TryGetValue(out int i))
                return i;
            return node is JsonValue value2 && value2.TryGetValue(out long l) && l >= int.MinValue && l <= int.MaxValue
                ? (int)l
                : node is JsonValue value3 && value3.TryGetValue(out double d) ? (int)d : null;
        }

        public static double? GetDoubleValue(JsonNode? node) {
            if (node is null)
                return null;
            if (node is JsonValue value && value.TryGetValue(out double d))
                return d;
            return node is JsonValue value2 && value2.TryGetValue(out long l)
                ? l
                : node is JsonValue value3 && value3.TryGetValue(out int i) ? i : null;
        }

        public static bool? GetBoolValue(JsonNode? node) {
            return node is null ? null : node is JsonValue value && value.TryGetValue(out bool b) ? b : null;
        }

        public static IEnumerable<string> EnumerateStrings(JsonNode? parent, string propertyName) =>
            EnumerateArray(parent, propertyName).Select(GetStringValue).OfType<string>();

        public static IEnumerable<JsonNode> EnumerateArray(JsonNode? parent, string? propertyName = null) {
            JsonNode? node = propertyName is null ? parent : GetNode(parent, propertyName);
            if (node is not JsonArray array)
                yield break;
            foreach (JsonNode? item in array) {
                if (item is not null)
                    yield return item;
            }
        }

        public static IEnumerable<string> GetObjectPropertyNames(JsonNode? node) {
            if (node is not JsonObject obj)
                yield break;
            foreach (KeyValuePair<string, JsonNode?> kvp in obj)
                yield return kvp.Key;
        }

        public static Dictionary<string, double>? GetStringDoubleDictionary(JsonNode? parent, string propertyName) {
            if (GetNode(parent, propertyName) is not JsonObject obj)
                return null;
            var result = new Dictionary<string, double>();
            foreach (KeyValuePair<string, JsonNode?> kvp in obj) {
                if (kvp.Value is not null && GetDoubleValue(kvp.Value) is double amount)
                    result[kvp.Key] = amount;
            }
            return result;
        }

        public static JsonObject? FindObjectInArrayByName(JsonObject root, string arrayProperty, string? name) {
            if (name is null)
                return null;
            foreach (JsonNode item in EnumerateArray(root, arrayProperty)) {
                if (item is JsonObject obj && GetString(obj, "name") == name)
                    return obj;
            }
            return null;
        }

        public static void MergePresetOverlay(JsonObject basePreset, JsonObject customOverlay) {
            foreach (KeyValuePair<string, JsonNode?> groupToken in customOverlay) {
                if (groupToken.Value is not JsonArray customArray)
                    continue;

                JsonArray baseArray = basePreset[groupToken.Key] as JsonArray ?? [];
                if (basePreset[groupToken.Key] is null)
                    basePreset[groupToken.Key] = baseArray;

                foreach (JsonNode? itemNode in customArray) {
                    if (itemNode is not JsonObject itemToken)
                        continue;

                    string? itemName = GetString(itemToken, "name");
                    JsonObject? presetItem = FindObjectInArrayByName(basePreset, groupToken.Key, itemName);
                    if (presetItem is not null) {
                        foreach (KeyValuePair<string, JsonNode?> parameter in itemToken)
                            presetItem[parameter.Key] = parameter.Value?.DeepClone();
                    } else
                        baseArray.Add(itemToken.DeepClone());
                }
            }
        }
    }
}
