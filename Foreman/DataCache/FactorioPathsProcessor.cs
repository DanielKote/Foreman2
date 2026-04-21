using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Foreman {
    public static class FactorioPathsProcessor {
        // check default folders for a factorio installation (to fill in the path as the 'default')
        public static List<string> GetFactorioInstallLocations() {
            var factorioPaths = new List<string>();

            // program files install

            var pfConfigPath = Path.Combine(["c:\\", "Program Files", "Factorio", "config-path.cfg"]);
            if (File.Exists(pfConfigPath))
                factorioPaths.Add(Path.GetDirectoryName(pfConfigPath));

            // steam

            var steamPathA = Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "SteamPath", "");
            var steamPathB = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath", "");
            var steamPath = steamPathA != null && !string.IsNullOrEmpty((string) steamPathA) ? (string) steamPathA :
                steamPathB != null && !string.IsNullOrEmpty((string) steamPathB) ? (string) steamPathB : "";

            if (string.IsNullOrEmpty(steamPath))
                return factorioPaths;

            var libraryFoldersFilePath = Path.Combine([steamPath, "steamapps", "libraryfolders.vdf"]);
            if (!File.Exists(libraryFoldersFilePath))
                return factorioPaths;

            var steamLSettings = File.ReadAllLines(libraryFoldersFilePath);
            foreach (var line in steamLSettings) {
                if (!line.Contains("\"path\""))
                    continue;

                var libraryPath = line.Substring(0, line.LastIndexOf("\"", StringComparison.Ordinal));
                libraryPath = libraryPath.Substring(libraryPath.LastIndexOf("\"", StringComparison.Ordinal) + 1);
                var factorioConfigPath = Path.Combine(libraryPath, "steamapps", "common", "Factorio", "config-path.cfg");
                if (File.Exists(factorioConfigPath))
                    factorioPaths.Add(Path.GetDirectoryName(factorioConfigPath));
            }

            return factorioPaths;
        }

        public static string GetFactorioUserPath(string installPath, bool verboseFail = false) {
            // find config-path.cfg, read it, and use it to find config.ini

            var configPath = Path.Combine(installPath, "config-path.cfg");
            if (!File.Exists(configPath)) {
                if (verboseFail)
                    MessageBox.Show(
                        "config-path.cfg missing from the install location. Maybe run Factorio once to ensure all files are there?\nAlternatively a reinstall might be required.");
                ErrorLogging.LogLine($"config-path.cfg was not found at {installPath}. this was supposed to be the install folder");
                return "";
            }

            var config = File.ReadAllText(configPath);
            var configIniPath = Path.Combine(ProcessPathString(config.Substring(12, config.IndexOf('\n') - 12), installPath), "config.ini");

            // read config.ini file

            if (!File.Exists(configIniPath)) {
                if (verboseFail)
                    MessageBox.Show("config.ini could not be found. Factorio setup is corrupted?");
                ErrorLogging.LogLine($"config.ini file was not found at {configIniPath}. config-path.cfg was at {configPath} and linked here.");
                return "";
            }

            var configIni = File.ReadAllLines(configIniPath);
            var writePath = "";
            foreach (var line in configIni)
                if (line.IndexOf("write-data", StringComparison.Ordinal) != -1 && line.IndexOf(";", StringComparison.Ordinal) != 0)
                    writePath = line.Substring(line.IndexOf("write-data", StringComparison.Ordinal) + 11);

            return ProcessPathString(writePath, installPath);
        }

        private static string ProcessPathString(string input, string installPath) {
            if (input.StartsWith(".factorio")) {
                var path = installPath;
                var folder = input == ".factorio" ? "" : input.Substring(9).Replace("/", "\\");
                if (folder.Length > 0) folder = folder.Substring(1);
                while (folder.IndexOf("..", StringComparison.Ordinal) != -1) {
                    path = Path.GetDirectoryName(path);
                    folder = folder.Substring(folder.IndexOf("..", StringComparison.Ordinal) + 2);
                    if (folder.Length > 0) folder = folder.Substring(1);
                }

                return string.IsNullOrEmpty(folder) ? path : Path.Combine(path, folder);
            }

            if (input.StartsWith("__PATH__executable__")) {
                var path = Path.Combine([installPath, "bin", "x64"]);
                var folder = input.Equals("__PATH__executable__") ? "" : input.Substring(20).Replace("/", "\\");
                if (folder.Length > 0) folder = folder.Substring(1);
                while (folder.IndexOf("..", StringComparison.Ordinal) != -1) {
                    path = Path.GetDirectoryName(path);
                    folder = folder.Substring(folder.IndexOf("..", StringComparison.Ordinal) + 2);
                    if (folder.Length > 0) folder = folder.Substring(1);
                }

                return string.IsNullOrEmpty(folder) ? path : Path.Combine(path, folder);
            }

            if (input.StartsWith("__PATH__system-write-data__")) {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("/", "\\");
                var folder = input.Equals("__PATH__system-write-data__") ? "" : input.Substring(27).Replace("/", "\\");
                if (folder.Length > 0) folder = folder.Substring(1);
                while (folder.IndexOf("..", StringComparison.Ordinal) != -1) {
                    path = Path.GetDirectoryName(path);
                    folder = folder.Substring(folder.IndexOf("..", StringComparison.Ordinal) + 2);
                    if (folder.Length > 0) folder = folder.Substring(1);
                }

                return string.IsNullOrEmpty(folder) ? Path.Combine(path, "Factorio") : Path.Combine([path, "Factorio", folder]);
            }

            ErrorLogging.LogLine(
                "path string (from one of the config files) did not start as expected (.factorio || __PATH__executable__ || __PATH__system-write-data__). Path string:" +
                input);

            // something weird must have happened to end up here.
            // Honestly these path conversions are a bit of a mess - not enough examples to be sure its correct
            // (works with all case 'I' have...)
            return installPath;
        }
    }
}