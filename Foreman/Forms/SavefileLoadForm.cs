using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public partial class SaveFileLoadForm : Form {
        private readonly DataCache DCache;
        private readonly HashSet<IDataObjectBase> EnabledObjects;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SaveFileInfo? SaveFileInfo { get; set; }
        private readonly CancellationTokenSource cts;

        private readonly string DefaultSaveFileLocation;
        private string saveFilePath;
        private string factorioPath;

        public SaveFileLoadForm(DataCache cache, HashSet<IDataObjectBase> enabledObjects) {
            DCache = cache;
            EnabledObjects = enabledObjects;
            SaveFileInfo = null;

            cts = new CancellationTokenSource();

            factorioPath = "";
            saveFilePath = "";

            InitializeComponent();

            //check for previous save file location and its validity (or set to "")
            DefaultSaveFileLocation = Properties.Settings.Default.LastSaveFileLocation;
            if (string.IsNullOrEmpty(DefaultSaveFileLocation))
                DefaultSaveFileLocation = "";
            string? tempUDirectory = DefaultSaveFileLocation;
            while (!string.IsNullOrEmpty(tempUDirectory) && !string.Equals(Path.GetFileName(tempUDirectory), "saves", StringComparison.OrdinalIgnoreCase))
                tempUDirectory = Path.GetDirectoryName(tempUDirectory);
            if (!string.IsNullOrEmpty(tempUDirectory))
                tempUDirectory = Path.GetDirectoryName(tempUDirectory); //done one more time to get the actual user directory, not the saves folder
            if (!File.Exists(Path.Combine(tempUDirectory ?? "", "factorio-current.log")))
                DefaultSaveFileLocation = "";

            //check default folders for a factorio installation (to fill in the path as the 'default')
            //program files install
            if (string.IsNullOrEmpty(DefaultSaveFileLocation)) {
                List<string> factorioInstallLocations = FactorioPathsProcessor.GetFactorioInstallLocations();
                if (factorioInstallLocations.Count > 0) {
                    string userPath = FactorioPathsProcessor.GetFactorioUserPath(factorioInstallLocations[0], false);
                    if (!string.IsNullOrEmpty(userPath))
                        DefaultSaveFileLocation = Path.Combine(userPath, "saves");
                }
            }
        }

        private async void ProgressForm_Load(object? sender, EventArgs e) {
#if DEBUG
            DateTime startTime = DateTime.Now;
#endif
            using (var dialog = new OpenFileDialog()) {
                dialog.InitialDirectory = DefaultSaveFileLocation;
                dialog.Filter = "factorio saves (*.zip)|*.zip";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                    saveFilePath = dialog.FileName;
                else {
                    DialogResult = DialogResult.Cancel;
                    SaveFileInfo = null;
                    Close();
                    return;
                }
            }

            var token = cts.Token;
            DialogResult loadResult = await LoadSaveFile(token).ConfigureAwait(false); //OK: all good, data loaded, ABORT: error during loading, display error message, CANCEL: local error prior to load (message already displayed)
            await this.InvokeOnUiThreadAsync(() => {
                DialogResult = loadResult;
                if (DialogResult == DialogResult.OK)
                    ProcessSaveData();
                Close();
            }).ConfigureAwait(false);

#if DEBUG
            TimeSpan diff = DateTime.Now.Subtract(startTime);
            Console.WriteLine("Save file load time: " + Math.Round(diff.TotalSeconds, 2) + " seconds.");
            ErrorLogging.LogLine("Save file load time: " + Math.Round(diff.TotalSeconds, 2) + " seconds.");
#endif
        }

        private async Task<DialogResult> LoadSaveFile(CancellationToken token) {
            return await Task.Run(() => {
                string modsPath = "";
                try {
                    //get factorio path
                    string? userDataPath = saveFilePath;
                    while (!string.IsNullOrEmpty(userDataPath) && !string.Equals(Path.GetFileName(userDataPath), "saves", StringComparison.OrdinalIgnoreCase))
                        userDataPath = Path.GetDirectoryName(userDataPath);
                    userDataPath = Path.GetDirectoryName(userDataPath); //done one more time to get the actual user directory, not the saves folder

                    string currentLog = Path.Combine(userDataPath ?? "", "factorio-current.log");
                    string[] currentLogLines = Utf8File.ReadAllLines(currentLog);
                    foreach (string line in currentLogLines) {
                        if (line.Contains("Program arguments", StringComparison.OrdinalIgnoreCase)) {
                            factorioPath = line[(line.IndexOf('"') + 1)..];
                            factorioPath = factorioPath[..factorioPath.IndexOf('"')];
                        }
                    }

                    if (!FactorioInstallValidator.TryValidateExecutable(factorioPath, out string? factorioVersionError)) {
                        UserMessages.Show(factorioVersionError);
                        return DialogResult.Cancel;
                    }

                    //copy the save reader mod to the mods folder
                    modsPath = Path.Combine(userDataPath ?? "", "mods");
                    if (!Directory.Exists(modsPath))
                        Directory.CreateDirectory(modsPath);
                    try {
                        FactorioBundledModHelper.CopyToModsFolder("foremansavereader_2.0.0", modsPath, "info.json", "instrument-control.lua");
                    } catch (Exception ex) {
                        UserMessages.Show("could not copy foreman save reader mod files (Mods/foremansavereader_2.0.0/) to the factorio mods folder. Reinstall foreman?");
                        ErrorLogging.LogException(ex, "copying of foreman save reader mod files failed");
                        return DialogResult.Abort;
                    }

                    FactorioModListHelper.SetModState(modsPath, "foremansavereader", enabled: true);

                    FactorioRunResult readRun = FactorioBenchmarkRunner.Run(
                        factorioPath,
                        string.Format(CultureInfo.InvariantCulture, "--instrument-mod foremansavereader --benchmark \"{0}\" --benchmark-ticks 1 --benchmark-runs 1", Path.GetFileName(saveFilePath)),
                        token,
                        () => {
                            if (Directory.Exists(Path.Combine(modsPath, "foremansavereader_2.0.0")))
                                Directory.Delete(Path.Combine(modsPath, "foremansavereader_2.0.0"), true);
                        });

                    string resultString = readRun.Output;

                    if (string.IsNullOrEmpty(resultString) && token.IsCancellationRequested)
                        return DialogResult.Cancel;

                    if (Directory.Exists(Path.Combine(modsPath, "foremansavereader_2.0.0")))
                        Directory.Delete(Path.Combine(modsPath, "foremansavereader_2.0.0"), true);

                    if (FactorioBenchmarkRunner.IsAnotherInstanceRunning(resultString)) {
                        UserMessages.Show("File read could not be completed because this instance of Factorio is currently running. Please stop expanding the factory for just a brief moment...");
                        return DialogResult.Cancel;
                    } else if (readRun.Crashed) {
                        UserMessages.Show(
                            "Factorio crashed while reading the save file.\n\n" +
                            "This is usually caused by a mod bug. See factorio-current.log in your Factorio user data folder.");
                        ErrorLogging.LogLine("Foreman save read: Factorio crash (exit code " + readRun.ExitCode + ").");
                        return DialogResult.Abort;
                    } else if (!resultString.Contains("<<<END-EXPORT-P0>>>", StringComparison.Ordinal)) {
#if DEBUG
                        Console.WriteLine(resultString);
#endif
                        ErrorLogging.LogLine("could not process save file due to export not completing. Mod issue?");
                        return DialogResult.Abort;
                    }
                    //parse output
                    string exportString = resultString[(resultString.IndexOf("<<<START-EXPORT-P0>>>", StringComparison.Ordinal) + 23)..];
                    exportString = exportString[..(exportString.IndexOf("<<<END-EXPORT-P0>>>", StringComparison.Ordinal) - 1)];
                    JsonObject export = PresetJson.ParseObject(exportString);

                    SaveFileInfo = new SaveFileInfo();
                    foreach (JsonNode objJToken in PresetJson.EnumerateArray(export, "mods"))
                        if (PresetJson.GetString(objJToken, "name") is string name && PresetJson.GetString(objJToken, "version") is string version)
                            SaveFileInfo.Mods.Add(name, version);
                    foreach (JsonNode objJToken in PresetJson.EnumerateArray(export, "technologies"))
                        if (PresetJson.GetString(objJToken, "name") is string name && PresetJson.GetBool(objJToken, "enabled") is bool enabled)
                            SaveFileInfo.Technologies.Add(name, enabled);
                    foreach (JsonNode objJToken in PresetJson.EnumerateArray(export, "recipes"))
                        if (PresetJson.GetString(objJToken, "name") is string name && PresetJson.GetBool(objJToken, "enabled") is bool enabled)
                            SaveFileInfo.Recipes.Add(name, enabled);

                    Properties.Settings.Default.LastSaveFileLocation = Path.GetDirectoryName(saveFilePath);
                    Properties.Settings.Default.Save();
                    return DialogResult.OK;
                } catch (Exception ex) {
                    ErrorLogging.LogException(ex, string.Format(CultureInfo.InvariantCulture, "Error reading save file '{0}'", saveFilePath));
                    if (!string.IsNullOrEmpty(modsPath) && Directory.Exists(Path.Combine(modsPath, "foremansavereader_2.0.0")))
                        Directory.Delete(Path.Combine(modsPath, "foremansavereader_2.0.0"), true);
                    SaveFileInfo = null;
                    return DialogResult.Abort;
                }
            }).ConfigureAwait(false);
        }

        private void ProcessSaveData() {
            int totalMods = DCache.IncludedMods.Count;
            string missingMods = "\nMissing Mods: ";
            string wrongVersionMods = "\nWrong Version Mods: ";
            string newMods = "\nAdded Mods: ";

            foreach (var mod in DCache.IncludedMods) {
                if (mod.Key == "foremanexport" || mod.Key == "foremansavereader" || mod.Key == "core")
                    continue;

                if (SaveFileInfo?.Mods.ContainsKey(mod.Key) is false)
                    missingMods += mod.Key + ", ";
                else if (SaveFileInfo?.Mods[mod.Key] != mod.Value)
                    wrongVersionMods += mod.Key + ", ";
            }
            foreach (var mod in SaveFileInfo?.Mods ?? []) {
                if (mod.Key == "foremanexport" || mod.Key == "foremansavereader" || mod.Key == "core")
                    continue;

                if (!DCache.IncludedMods.ContainsKey(mod.Key))
                    newMods += mod.Key + ", ";
            }
            missingMods = missingMods[..^2];
            if (missingMods == "\nMissing Mods")
                missingMods = "";
            wrongVersionMods = wrongVersionMods[..^2];
            if (wrongVersionMods == "\nWrong Version Mods")
                wrongVersionMods = "";
            newMods = newMods[..^2];
            if (newMods == "\nAdded Mods")
                newMods = "";

            if (!string.IsNullOrEmpty(missingMods) || !string.IsNullOrEmpty(wrongVersionMods) || !string.IsNullOrEmpty(newMods))
                if (UserMessages.Show("selected save file mods do not match preset mods; out of {0} mods:" + missingMods + wrongVersionMods + newMods + "\nAre you sure you wish to use this save file?", "Save file mod inconsistencies found!", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;

            //we will not be updating technology based on the read data. we will instead be updating the recipes based on their enabled status. This is due to the possibility that a recipe was 'manually' enabled outside of the default technology unlocks. Is this possible? I dont know.
            EnabledObjects.Clear();
            if (DCache.PlayerAssembler is not null)
                EnabledObjects.Add(DCache.PlayerAssembler);

            foreach (IRecipe recipe in DCache.Recipes.Values)
                if (recipe.Name.StartsWith("§§", StringComparison.Ordinal) || (SaveFileInfo?.Recipes.ContainsKey(recipe.Name) is true && SaveFileInfo.Recipes[recipe.Name]))
                    EnabledObjects.Add(recipe);

            //go through all the assemblers, beacons, and modules and add them to the enabled set if at least one of their associated items has at least one production recipe that is in the enabled set.
            foreach (IAssembler assembler in DCache.Assemblers.Values) {
                bool enabled = false;
                foreach (IReadOnlyCollection<IRecipe> recipes in assembler.AssociatedItems.Select(item => item.ProductionRecipes))
                    foreach (IRecipe recipe in recipes)
                        enabled |= EnabledObjects.Contains(recipe);
                if (enabled)
                    EnabledObjects.Add(assembler);
            }

            foreach (IBeacon beacon in DCache.Beacons.Values) {
                bool enabled = false;
                foreach (IReadOnlyCollection<IRecipe> recipes in beacon.AssociatedItems.Select(item => item.ProductionRecipes))
                    foreach (IRecipe recipe in recipes)
                        enabled |= EnabledObjects.Contains(recipe);
                if (enabled)
                    EnabledObjects.Add(beacon);
            }

            foreach (IModule module in DCache.Modules.Values) {
                bool enabled = false;
                foreach (IRecipe recipe in module.AssociatedItem.ProductionRecipes)
                    enabled |= EnabledObjects.Contains(recipe);
                if (enabled)
                    EnabledObjects.Add(module);
            }

        }

        private void CancellationButton_Click(object? sender, EventArgs e) {
            cts.Cancel();
            DialogResult = DialogResult.Cancel;
            SaveFileInfo = null;
            Close();
        }
    }
}
