using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Foreman {
    public static class FactorioPathsProcessor {
        public static List<string> GetFactorioInstallLocations() {
            //check default folders for a factorio installation (to fill in the path as the 'default')
            List<string> factorioPaths = new List<string>();

            //program files install
            string pfConfigPath = Path.Combine("c:\\", "Program Files", "Factorio", "config-path.cfg");
            if (File.Exists(pfConfigPath) && Path.GetDirectoryName(pfConfigPath) is string dir)
                factorioPaths.Add(dir);

            //steam
            var steamPath = Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "SteamPath", null) switch {
                string { Length: > 0 } s => s,
                _ => null,
            } ??
            Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath", null) switch {
                string { Length: > 0 } s => s,
                _ => null,
            };
            if (steamPath is not null) {
                string libraryFoldersFilePath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
                if (File.Exists(libraryFoldersFilePath)) {
                    string[] steamLSettings = File.ReadAllLines(libraryFoldersFilePath);
                    foreach (string line in steamLSettings) {
                        if (line.Contains("\"path\"")) {
                            string libraryPath = line.Substring(0, line.LastIndexOf("\""));
                            libraryPath = libraryPath.Substring(libraryPath.LastIndexOf("\"") + 1);
                            string factorioConfigPath = Path.Combine(libraryPath, "steamapps", "common", "Factorio", "config-path.cfg");
                            if (File.Exists(factorioConfigPath) && Path.GetDirectoryName(factorioConfigPath) is string dirPath)
                                factorioPaths.Add(dirPath);
                        }
                    }
                }
            }

            return factorioPaths;
        }

        public static string GetFactorioUserPath(string installPath, bool verboseFail = false) {
            //find config-path.cfg, read it, and use it to find config.ini
            string configPath = Path.Combine(installPath, "config-path.cfg");
            if (!File.Exists(configPath)) {
                if (verboseFail)
                    MessageBox.Show("config-path.cfg missing from the install location. Maybe run Factorio once to ensure all files are there?\nAlternatively a reinstall might be required.");
                ErrorLogging.LogLine(string.Format("config-path.cfg was not found at {0}. this was supposed to be the install folder", installPath));
                return "";
            }

            string config = File.ReadAllText(configPath);
            string configIniPath = Path.Combine(ProcessPathString(config.Substring(12, config.IndexOf('\n') - 12), installPath), "config.ini");

            //read config.ini file
            if (!File.Exists(configIniPath)) {
                if (verboseFail)
                    MessageBox.Show("config.ini could not be found. Factorio setup is corrupted?");
                ErrorLogging.LogLine(string.Format("config.ini file was not found at {0}. config-path.cfg was at {1} and linked here.", configIniPath, configPath));
                return "";
            }
            string[] configIni = File.ReadAllLines(configIniPath);
            string writePath = "";
            foreach (string line in configIni)
                if (line.IndexOf("write-data") != -1 && line.IndexOf(";") != 0)
                    writePath = line.Substring(line.IndexOf("write-data") + 11);

            return ProcessPathString(writePath, installPath);
        }

        private static string ProcessPathString(string input, string installPath) {
            if (input.StartsWith(".factorio")) {
                string path = installPath;
                string folder = (input == ".factorio") ? "" : input.Substring(9).Replace("/", "\\");
                if (folder.Length > 0)
                    folder = folder.Substring(1);
                while (folder.IndexOf("..") != -1) {
                    path = Path.GetDirectoryName(path) ?? path;
                    folder = folder.Substring(folder.IndexOf("..") + 2);
                    if (folder.Length > 0)
                        folder = folder.Substring(1);
                }
                return string.IsNullOrEmpty(folder) ? path : Path.Combine(path, folder);
            } else if (input.StartsWith("__PATH__executable__")) {
                string path = Path.Combine(installPath, "bin", "x64");
                string folder = input.Equals("__PATH__executable__") ? "" : input.Substring(20).Replace("/", "\\");
                if (folder.Length > 0)
                    folder = folder.Substring(1);
                while (folder.IndexOf("..") != -1) {
                    path = Path.GetDirectoryName(path) ?? path;
                    folder = folder.Substring(folder.IndexOf("..") + 2);
                    if (folder.Length > 0)
                        folder = folder.Substring(1);
                }
                return string.IsNullOrEmpty(folder) ? path : Path.Combine(path, folder);
            } else if (input.StartsWith("__PATH__system-write-data__")) {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("/", "\\");
                string folder = input.Equals("__PATH__system-write-data__") ? "" : input.Substring(27).Replace("/", "\\");
                if (folder.Length > 0)
                    folder = folder.Substring(1);
                while (folder.IndexOf("..") != -1) {
                    path = Path.GetDirectoryName(path) ?? path;
                    folder = folder.Substring(folder.IndexOf("..") + 2);
                    if (folder.Length > 0)
                        folder = folder.Substring(1);
                }
                return string.IsNullOrEmpty(folder) ? Path.Combine(path, "Factorio") : Path.Combine(path, "Factorio", folder);
            } else
                ErrorLogging.LogLine("path string (from one of the config files) did not start as expected (.factorio || __PATH__executable__ || __PATH__system-write-data__). Path string:" + input);

            return installPath; //something weird must have happened to end up here. Honesty these path conversions are a bit of a mess - not enough examples to be sure its correct (works with all case 'I' have...)
        }

        public static string GetExecutablePath(string installPath) =>
            Path.Combine(installPath, "bin", "x64", "factorio.exe");

        public static bool TryNormalizeInstallPath(string selectedPath, out string installRoot) {
            installRoot = selectedPath;
            if (File.Exists(GetExecutablePath(selectedPath)))
                return true;
            if (File.Exists(Path.Combine(selectedPath, "x64", "factorio.exe"))) {
                installRoot = Path.GetDirectoryName(selectedPath) ?? selectedPath;
                return true;
            }
            if (File.Exists(Path.Combine(selectedPath, "factorio.exe"))) {
                installRoot = Path.GetDirectoryName(Path.GetDirectoryName(selectedPath) ?? selectedPath) ?? selectedPath;
                return true;
            }
            return false;
        }
    }
}