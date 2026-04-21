using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public partial class SaveFileLoadForm : Form {
        private readonly DataCache _dCache;
        private readonly HashSet<DataObjectBase> _enabledObjects;
        public SaveFileInfo SaveFileInfo;

        private CancellationTokenSource _cts;

        private string _defaultSaveFileLocation;
        private string _saveFilePath;
        private string _factorioPath;

        public SaveFileLoadForm(DataCache cache, HashSet<DataObjectBase> enabledObjects) {
            _dCache = cache;
            _enabledObjects = enabledObjects;
            SaveFileInfo = null;

            _cts = new CancellationTokenSource();

            _factorioPath = "";
            _saveFilePath = "";

            InitializeComponent();

            // check for previous save file location and its validity (or set to "")

            _defaultSaveFileLocation = Properties.Settings.Default.LastSaveFileLocation;
            if (string.IsNullOrEmpty(_defaultSaveFileLocation))
                _defaultSaveFileLocation = "";
            var tempUDirectory = _defaultSaveFileLocation;
            while (!string.IsNullOrEmpty(tempUDirectory) && Path.GetFileName(tempUDirectory).ToLower() != "saves")
                tempUDirectory = Path.GetDirectoryName(tempUDirectory);
            if (!string.IsNullOrEmpty(tempUDirectory))
                tempUDirectory = Path.GetDirectoryName(tempUDirectory); //done one more time to get the actual user directory, not the saves folder
            if (!File.Exists(Path.Combine(tempUDirectory, "factorio-current.log")))
                _defaultSaveFileLocation = "";

            // check default folders for a factorio installation (to fill in the path as the 'default')
            // program files install

            if (!string.IsNullOrEmpty(_defaultSaveFileLocation))
                return;

            var factorioInstallLocations = FactorioPathsProcessor.GetFactorioInstallLocations();
            if (factorioInstallLocations.Count <= 0)
                return;

            var userPath = FactorioPathsProcessor.GetFactorioUserPath(factorioInstallLocations[0]);
            if (!string.IsNullOrEmpty(userPath))
                _defaultSaveFileLocation = Path.Combine(userPath, "saves");
        }

        private async void ProgressForm_Load(object sender, EventArgs e) {
#if DEBUG
            var startTime = DateTime.Now;
#endif
            using (var dialog = new OpenFileDialog()) {
                dialog.InitialDirectory = _defaultSaveFileLocation;
                dialog.Filter = "factorio saves (*.zip)|*.zip";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                    _saveFilePath = dialog.FileName;
                else {
                    DialogResult = DialogResult.Cancel;
                    SaveFileInfo = null;
                    Close();
                    return;
                }
            }

            // OK: all good, data loaded,
            // ABORT: error during loading, display error message,
            // CANCEL: local error prior to load (message already displayed)

            var token = _cts.Token;
            DialogResult = await LoadSaveFile(token);
            if (DialogResult == DialogResult.OK)
                ProcessSaveData();
            Close();

#if DEBUG
            var diff = DateTime.Now.Subtract(startTime);
            Console.WriteLine($"Save file load time: {Math.Round(diff.TotalSeconds, 2)} seconds.");
            ErrorLogging.LogLine($"Save file load time: {Math.Round(diff.TotalSeconds, 2)} seconds.");
#endif
        }

        private async Task<DialogResult> LoadSaveFile(CancellationToken token) {
            return await Task.Run(() => {
                var modsPath = "";
                try {
                    // get factorio path

                    var userDataPath = _saveFilePath;
                    while (!string.IsNullOrEmpty(userDataPath) && Path.GetFileName(userDataPath).ToLower() != "saves")
                        userDataPath = Path.GetDirectoryName(userDataPath);

                    // done one more time to get the actual user directory, not the saves folder

                    userDataPath = Path.GetDirectoryName(userDataPath);

                    var currentLog = Path.Combine(userDataPath, "factorio-current.log");
                    var currentLogLines = File.ReadAllLines(currentLog);
                    foreach (var line in currentLogLines) {
                        if (!line.Contains("Program arguments"))
                            continue;

                        _factorioPath = line.Substring(line.IndexOf("\"", StringComparison.Ordinal) + 1);
                        _factorioPath = _factorioPath.Substring(0, _factorioPath.IndexOf("\"", StringComparison.Ordinal));
                    }

                    // test factorio version

                    var factorioVersionInfo = FileVersionInfo.GetVersionInfo(_factorioPath);
                    if (factorioVersionInfo.ProductMajorPart < 2) {
                        MessageBox.Show(
                            "Factorio Version below 2.0 can not be used with this version of Foreman. Please use Factorio 2.0 or newer. Alternatively download dev.13 or under of foreman 2.0 for pre factorio 2.0.");
                        ErrorLogging.LogLine(string.Format(
                            "Factorio version 0.x or 1.x instead of 2.x - use Foreman dev.13 or below for these factorio installs.",
                            factorioVersionInfo.ProductVersion));
                        return DialogResult.Cancel;
                    } else if (factorioVersionInfo.ProductMajorPart > 2) {
                        MessageBox.Show(
                            "Factorio Version 3.x+ can not be used with this version of Foreman. Sit tight and wait for update...\nYou can also try to msg me on discord (u\\DanielKotes) if for some reason I am not already aware of this.");
                        ErrorLogging.LogLine(string.Format("Factorio version 3.x+ isn't supported.", factorioVersionInfo.ProductVersion));
                        return DialogResult.Cancel;
                    } else if (factorioVersionInfo.ProductMinorPart < 0 ||
                        (factorioVersionInfo.ProductMinorPart == 0 && factorioVersionInfo.ProductBuildPart < 7)) {
                        MessageBox.Show("Factorio version (" + factorioVersionInfo.ProductVersion +
                            ") can not be used with Foreman. Please use Factorio 2.0.7 or newer.");
                        ErrorLogging.LogLine($"Factorio version was too old. {factorioVersionInfo.ProductVersion} instead of 2.0.7+");
                        return DialogResult.Cancel;
                    }

                    // copy the save reader mod to the mods folder

                    modsPath = Path.Combine(userDataPath, "mods");
                    if (!Directory.Exists(modsPath))
                        Directory.CreateDirectory(modsPath);
                    Directory.CreateDirectory(Path.Combine(modsPath, "foremansavereader_2.0.0"));
                    try {
                        File.Copy(Path.Combine(["Mods", "foremansavereader_2.0.0", "info.json"]),
                            Path.Combine([modsPath, "foremansavereader_2.0.0", "info.json"]), true);
                        File.Copy(Path.Combine(["Mods", "foremansavereader_2.0.0", "instrument-control.lua"]),
                            Path.Combine([modsPath, "foremansavereader_2.0.0", "instrument-control.lua"]), true);
                    } catch {
                        MessageBox.Show(
                            "could not copy foreman save reader mod files (Mods/foremansavereader_2.0.0/) to the factorio mods folder. Reinstall foreman?");
                        ErrorLogging.LogLine("copying of foreman save reader mod files failed.");
                        return DialogResult.Abort;
                    }

                    // ensure that the foreman save reader mod is correctly added to the mod-list and is enabled

                    var modListPath = Path.Combine(modsPath, "mod-list.json");
                    var modlist = !File.Exists(modListPath) ? new JObject() : JObject.Parse(File.ReadAllText(modListPath));
                    if (modlist["mods"] == null)
                        modlist.Add("mods", new JArray());

                    var foremansavereaderModToken =
                        modlist["mods"].ToList().FirstOrDefault(t => t["name"] != null && (string) t["name"] == "foremansavereader");
                    if (foremansavereaderModToken == null)
                        ((JArray) modlist["mods"]).Add(new JObject() { { "name", "foremansavereader" }, { "enabled", true } });
                    else
                        foremansavereaderModToken["enabled"] = true;
                    File.WriteAllText(modListPath, modlist.ToString(Formatting.Indented));

                    // open the map with factorio and read the save file info (mods, technology, recipes)

                    var process = new Process();
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = _factorioPath;
                    process.StartInfo.Arguments =
                        $"--instrument-mod foremansavereader --benchmark \"{Path.GetFileName(_saveFilePath)}\" --benchmark-ticks 1 --benchmark-runs 1";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.Start();
                    var resultString = "";
                    while (!process.HasExited) {
                        resultString += process.StandardOutput.ReadToEnd();
                        if (token.IsCancellationRequested) {
                            process.Close();
                            if (Directory.Exists(Path.Combine(modsPath, "foremansavereader_2.0.0")))
                                Directory.Delete(Path.Combine(modsPath, "foremansavereader_2.0.0"), true);
                            return DialogResult.Cancel;
                        }

                        Thread.Sleep(100);
                    }

                    if (Directory.Exists(Path.Combine(modsPath, "foremansavereader_2.0.0")))
                        Directory.Delete(Path.Combine(modsPath, "foremansavereader_2.0.0"), true);

                    if (resultString.IndexOf("Is another instance already running?", StringComparison.Ordinal) != -1) {
                        MessageBox.Show(
                            "File read could not be completed because this instance of Factorio is currently running. Please stop expanding the factory for just a brief moment...");
                        return DialogResult.Cancel;
                    } else if (resultString.IndexOf("<<<END-EXPORT-P0>>>", StringComparison.Ordinal) == -1) {
#if DEBUG
                        Console.WriteLine(resultString);
#endif
                        ErrorLogging.LogLine("could not process save file due to export not completing. Mod issue?");
                        return DialogResult.Abort;
                    }

                    //parse output 

                    var exportString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-P0>>>", StringComparison.Ordinal) + 23);
                    exportString = exportString.Substring(0, exportString.IndexOf("<<<END-EXPORT-P0>>>", StringComparison.Ordinal) - 1);
                    var export = JObject.Parse(exportString);

                    SaveFileInfo = new SaveFileInfo();
                    foreach (var objJToken in export["mods"].ToList())
                        SaveFileInfo.Mods.Add((string) objJToken["name"], (string) objJToken["version"]);
                    foreach (var objJToken in export["technologies"].ToList())
                        SaveFileInfo.Technologies.Add((string) objJToken["name"], (bool) objJToken["enabled"]);
                    foreach (var objJToken in export["recipes"].ToList())
                        SaveFileInfo.Recipes.Add((string) objJToken["name"], (bool) objJToken["enabled"]);

                    Properties.Settings.Default.LastSaveFileLocation = Path.GetDirectoryName(_saveFilePath);
                    Properties.Settings.Default.Save();
                    return DialogResult.OK;
                } catch {
                    if (!string.IsNullOrEmpty(modsPath) && Directory.Exists(Path.Combine(modsPath, "foremanexport_2.0.0")))
                        Directory.Delete(Path.Combine(modsPath, "foremanexport_2.0.0"), true);
                    SaveFileInfo = null;
                    return DialogResult.Abort;
                }
            }, token);
        }

        private void ProcessSaveData() {
            var totalMods = _dCache.IncludedMods.Count;
            var missingMods = "\nMissing Mods: ";
            var wrongVersionMods = "\nWrong Version Mods: ";
            var newMods = "\nAdded Mods: ";

            foreach (var mod in _dCache.IncludedMods) {
                if (mod.Key is "foremanexport" or "foremansavereader" or "core")
                    continue;

                if (!SaveFileInfo.Mods.TryGetValue(mod.Key, out var tempMod))
                    missingMods += mod.Key + ", ";
                else if (tempMod != mod.Value)
                    wrongVersionMods += mod.Key + ", ";
            }

            foreach (var mod in SaveFileInfo.Mods) {
                if (mod.Key is "foremanexport" or "foremansavereader" or "core")
                    continue;

                if (!_dCache.IncludedMods.ContainsKey(mod.Key))
                    newMods += mod.Key + ", ";
            }

            missingMods = missingMods.Substring(0, missingMods.Length - 2);
            if (missingMods == "\nMissing Mods") missingMods = "";
            wrongVersionMods = wrongVersionMods.Substring(0, wrongVersionMods.Length - 2);
            if (wrongVersionMods == "\nWrong Version Mods") wrongVersionMods = "";
            newMods = newMods.Substring(0, newMods.Length - 2);
            if (newMods == "\nAdded Mods") newMods = "";

            if (missingMods != "" || wrongVersionMods != "" || newMods != "")
                if (MessageBox.Show(
                        "selected save file mods do not match preset mods; out of {0} mods:" + missingMods + wrongVersionMods + newMods +
                        "\nAre you sure you wish to use this save file?", "Save file mod inconsistencies found!", MessageBoxButtons.OKCancel) ==
                    DialogResult.Cancel)
                    return;

            // we will not be updating technology based on the read data. we will instead be updating the recipes based on their enabled status.
            // This is due to the possibility that a recipe was 'manually' enabled outside the default technology unlocks. Is this possible? I don't know.

            _enabledObjects.Clear();
            _enabledObjects.Add(_dCache.PlayerAssembler);

            foreach (var recipe in _dCache.Recipes.Values)
                if (recipe.Name.StartsWith("§§") || (SaveFileInfo.Recipes.ContainsKey(recipe.Name) && SaveFileInfo.Recipes[recipe.Name]))
                    _enabledObjects.Add(recipe);

            // go through all the assemblers, beacons, and modules and add them to the enabled set
            // if at least one of their associated items has at least one production recipe that is in the enabled set.

            foreach (var assembler in _dCache.Assemblers.Values) {
                var enabled = false;
                foreach (var recipes in assembler.AssociatedItems.Select(item => item.ProductionRecipes))
                foreach (var recipe in recipes)
                    enabled |= _enabledObjects.Contains(recipe);
                if (enabled)
                    _enabledObjects.Add(assembler);
            }

            foreach (var beacon in _dCache.Beacons.Values) {
                var enabled = false;
                foreach (var recipes in beacon.AssociatedItems.Select(item => item.ProductionRecipes))
                foreach (var recipe in recipes)
                    enabled |= _enabledObjects.Contains(recipe);
                if (enabled)
                    _enabledObjects.Add(beacon);
            }

            foreach (var module in _dCache.Modules.Values) {
                var enabled = false;
                foreach (var recipe in module.AssociatedItem.ProductionRecipes)
                    enabled |= _enabledObjects.Contains(recipe);
                if (enabled)
                    _enabledObjects.Add(module);
            }
        }

        private void CancellationButton_Click(object sender, EventArgs e) {
            _cts.Cancel();
            DialogResult = DialogResult.Cancel;
            SaveFileInfo = null;
            Close();
        }
    }
}