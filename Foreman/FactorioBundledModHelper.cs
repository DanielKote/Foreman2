using System.IO;

namespace Foreman {
    internal static class FactorioBundledModHelper {
        public static void CopyToModsFolder(string bundledModFolderName, string modsPath, params string[] relativeFiles) {
            string destDir = Path.Combine(modsPath, bundledModFolderName);
            Directory.CreateDirectory(destDir);
            foreach (string file in relativeFiles)
                File.Copy(Path.Combine("Mods", bundledModFolderName, file), Path.Combine(destDir, file), overwrite: true);
        }
    }
}
