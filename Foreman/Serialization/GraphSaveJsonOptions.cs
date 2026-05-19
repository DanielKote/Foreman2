using System.Text.Json;
using System.Text.Json.Serialization;

namespace Foreman.Serialization {
    internal static class GraphSaveJsonOptions {
        private static readonly JsonSerializerOptions CompactOptions = Create(writeIndented: false);
        private static readonly JsonSerializerOptions IndentedOptions = Create(writeIndented: true);

        public static JsonSerializerOptions Get(bool writeIndented) =>
            writeIndented ? IndentedOptions : CompactOptions;

        private static JsonSerializerOptions Create(bool writeIndented) => new() {
            WriteIndented = writeIndented,
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }
}
