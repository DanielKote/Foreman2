using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Foreman {
    /// <summary>Null-safe helpers for reading Foreman save / preset JSON.</summary>
    internal static class JsonTokens {
        public static string? AsString(JToken? token) =>
            token is { Type: JTokenType.String } ? token.Value<string>() : null;

        public static int? AsInt32(JToken? token) => token is null ? null : (int?)token;

        public static double? AsDouble(JToken? token) => token is null ? null : (double?)token;

        public static bool? AsBoolean(JToken? token) => token is null ? null : (bool?)token;

        public static float? AsSingle(JToken? token) => token is null ? null : (float?)token;

        public static IEnumerable<string> EnumerateStrings(JToken? token) {
            if (token is not JArray array)
                yield break;
            foreach (JToken entry in array) {
                if (entry.Type == JTokenType.String && entry.Value<string>() is string s)
                    yield return s;
            }
        }

        public static List<string> ToStringList(JToken? token) => [.. EnumerateStrings(token)];

        public static Dictionary<string, string> ParseModList(JToken? includedModsToken) {
            Dictionary<string, string> modSet = new Dictionary<string, string>();
            foreach (string entry in EnumerateStrings(includedModsToken)) {
                string[] mod = entry.Split('|');
                if (mod.Length >= 2)
                    modSet[mod[0]] = mod[1];
            }
            return modSet;
        }

        public static List<string> EnumerateQualityKeys(JToken? includedQualitiesToken) {
            List<string> names = new List<string>();
            if (includedQualitiesToken is not JArray array)
                return names;
            foreach (JToken entry in array) {
                if (entry is JObject obj && AsString(obj["Key"]) is string key)
                    names.Add(key);
            }
            return names;
        }
    }
}