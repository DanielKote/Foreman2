using System;
using System.IO;
using ZstdSharp;

namespace ForemanTest.support {
    /// <summary>Prepares ZSTD-compressed preset assets for tests.</summary>
    internal static class TestPresetAsset {
        public const string PyanodonPresetName = "Factorio 2.0 Pyanodon";
        public const string PyanodonCompressedFileName = PyanodonPresetName + ".pjson.zst";

        public const string SEKrastorioPresetName = "Factorio 2.0 SEKrastorio";
        public const string SEKrastorioCompressedFileName = SEKrastorioPresetName + ".pjson.zst";

        public static bool PyanodonAssetExists =>
            File.Exists(TestAssets.ResolvePath(PyanodonCompressedFileName));

        public static bool SEKrastorioAssetExists =>
            File.Exists(TestAssets.ResolvePath(SEKrastorioCompressedFileName));

        /// <summary>
        /// Decompresses the asset into <c>Presets/{presetName}.pjson</c> under the test output directory
        /// when missing or older than the .zst source. Icon .dat is not required.
        /// </summary>
        public static string EnsurePjsonOnDisk(string presetName, string compressedAssetFileName) {
            string compressedPath = TestAssets.ResolvePath(compressedAssetFileName);
            string presetsDir = Path.Combine(AppContext.BaseDirectory, "Presets");
            Directory.CreateDirectory(presetsDir);
            string pjsonPath = Path.Combine(presetsDir, presetName + ".pjson");

            if (File.Exists(pjsonPath) && File.GetLastWriteTimeUtc(pjsonPath) >= File.GetLastWriteTimeUtc(compressedPath))
                return pjsonPath;

            using var input = File.OpenRead(compressedPath);
            using var decompressor = new DecompressionStream(input);
            using var output = File.Create(pjsonPath);
            decompressor.CopyTo(output);
            return pjsonPath;
        }

        public static string EnsurePyanodonPjsonOnDisk() =>
            EnsurePjsonOnDisk(PyanodonPresetName, PyanodonCompressedFileName);

        public static string EnsureSEKrastorioPjsonOnDisk() =>
            EnsurePjsonOnDisk(SEKrastorioPresetName, SEKrastorioCompressedFileName);
    }
}
