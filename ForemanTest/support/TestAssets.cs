using System;
using System.IO;

namespace ForemanTest.support {
    internal static class TestAssets {
        private const string AssetsFolder = "assets";

        public static string ResolvePath(string fileName) {
            string path = Path.Combine(AppContext.BaseDirectory, AssetsFolder, fileName);
            return !File.Exists(path)
                ? throw new FileNotFoundException(
                    $"Test asset not found: {path}. Ensure ForemanTest.csproj copies assets\\{fileName} to the output directory.",
                    path)
                : path;
        }
    }
}
