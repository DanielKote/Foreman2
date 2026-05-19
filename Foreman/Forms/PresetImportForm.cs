using Foreman.Controls;
using Foreman.DataCaching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public partial class PresetImportForm : Form {
        private readonly char[] ExtraChars = ['(', ')', '-', '_', '.', ' '];
        private readonly CancellationTokenSource cts;

        public string NewPresetName { get; private set; }
        public bool ImportStarted { get; private set; }

        public PresetImportForm() {
            NewPresetName = "";
            ImportStarted = false;
            cts = new CancellationTokenSource();
            InitializeComponent();
            PresetNameTextBox.Focus();

            FactorioLocationComboBox.Items.AddRange([.. FactorioPathsProcessor.GetFactorioInstallLocations()]);
            if (FactorioLocationComboBox.Items.Count > 0)
                FactorioLocationComboBox.SelectedIndex = 0;
        }

        private void EnableProgressBar(bool enabled) {
            this.SuspendLayout();
            ImportProgressBar.Visible = enabled;
            CancelImportButtonB.Visible = enabled;
            CancelImportButtonB.Focus();

            FactorioLocationGroup.Enabled = !enabled;
            FactorioSettingsGroup.Enabled = !enabled;
            PresetNameGroup.Enabled = !enabled;

            OKButton.Visible = !enabled;
            OKButton.Enabled = !enabled;
            CancelImportButton.Visible = !enabled;
            this.ResumeLayout();
        }

        private void FactorioBrowseButton_Click(object? sender, EventArgs e) {
            using var dialog = new FolderBrowserDialog();
            if (Directory.Exists(FactorioLocationComboBox.Text))
                dialog.SelectedPath = FactorioLocationComboBox.Text;

            if (dialog.ShowDialog() == DialogResult.OK) {
                if (FactorioPathsProcessor.TryNormalizeInstallPath(dialog.SelectedPath, out string installRoot))
                    FactorioLocationComboBox.Text = installRoot;
                else
                    UserMessages.Show("Selected directory doesnt seem to be a factorio install folder (it should at the very least have \"bin\" and \"data\" folders, along with a \"config-path.cfg\" file)");
            }
        }

        private void ModsBrowseButton_Click(object? sender, EventArgs e) {
            using var dialog = new FolderBrowserDialog();
            if (Directory.Exists(ModsLocationComboBox.Text))
                dialog.SelectedPath = ModsLocationComboBox.Text;

            if (dialog.ShowDialog() == DialogResult.OK) {
                if (File.Exists(Path.Combine(dialog.SelectedPath, "mod-list.json")))
                    ModsLocationComboBox.Text = dialog.SelectedPath;
                else
                    UserMessages.Show("Selected directory doesnt seem to be a factorio mods folder (it should at the very least have \"mod-list.json\" file)");
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e) {
            cts.Cancel();
            DialogResult = DialogResult.Cancel;
            NewPresetName = "";
            Close();
        }

        private async void OKButton_Click(object? sender, EventArgs e) {
            NewPresetName = PresetNameTextBox.Text;
            if (!Directory.Exists(FactorioLocationComboBox.Text)) {
                UserMessages.Show("That directory doesn't seem to exist");
                CleanupFailedImport();
                return;
            }
            if (NewPresetName.Length < 5) {
                UserMessages.Show("Preset name has to be longer than 5!");
                CleanupFailedImport();
                return;
            }

            var existingPresets = MainForm.GetValidPresetsList();
            if (string.Equals(NewPresetName, MainForm.DefaultPreset, StringComparison.OrdinalIgnoreCase)) {
                UserMessages.Show("Cant overwrite default preset!", "", MessageBoxButtons.OK);
                CleanupFailedImport();
                return;
            } else if (existingPresets?.Any(p => string.Equals(p.Name, NewPresetName, StringComparison.OrdinalIgnoreCase)) is true) {
                if (UserMessages.Show("This preset name is already in use. Do you wish to overwrite?", "Confirm Overwrite", MessageBoxButtons.YesNo) != DialogResult.Yes) {
                    CleanupFailedImport();
                    return;
                }
            }

            EnableProgressBar(true);

            string installPath = FactorioLocationComboBox.Text;
            //quick check to ensure the install path is correct (and accept a direct path to the factorio.exe folder just in case)
            if (!File.Exists(Path.Combine(installPath, "bin", "x64", "factorio.exe")))
                if (File.Exists(Path.Combine(installPath, "factorio.exe")))
                    installPath = Path.Combine(Path.GetDirectoryName(installPath) ?? "", @"..\\..\\");

            if (!File.Exists(Path.Combine(installPath, "bin", "x64", "factorio.exe"))) {
                EnableProgressBar(false);
                UserMessages.Show("Couldnt find factorio.exe (/bin/x64/factorio.exe) - please select a valid Factorio install location");
                CleanupFailedImport();
                return;
            }

            string factorioExePath = Path.Combine(installPath, "bin", "x64", "factorio.exe");
            if (!FactorioInstallValidator.TryValidateExecutable(factorioExePath, out string? factorioVersionError)) {
                EnableProgressBar(false);
                UserMessages.Show(factorioVersionError);
                CleanupFailedImport();
                return;
            }

            var factorioVersionInfo = FileVersionInfo.GetVersionInfo(factorioExePath);

            string modsPath = ModsLocationComboBox.Text;
            if (string.IsNullOrEmpty(modsPath) || !File.Exists(Path.Combine(modsPath, "mod-list.json"))) {
                string userDataPath = FactorioPathsProcessor.GetFactorioUserPath(installPath, true);
                if (string.IsNullOrEmpty(userDataPath)) {
                    UserMessages.Show("Couldnt auto-locate the mods folder - please manually locate the folder");
                    CleanupFailedImport();
                    return;
                }
                modsPath = Path.Combine(userDataPath, "mods");
            }

            //we now have the two paths to use - installPath and modsPath. can begin processing Factorio
            var progress = new Progress<KeyValuePair<int, string>>(value => {
                if (value.Key > ImportProgressBar.Value)
                    ImportProgressBar.Value = value.Key;
                if (!string.IsNullOrEmpty(value.Value) && value.Value != ImportProgressBar.CustomText)
                    ImportProgressBar.CustomText = value.Value;
            }) as IProgress<KeyValuePair<int, string>>;
            var token = cts.Token;

#if DEBUG
            var stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            ImportStarted = true;
            string foremanModName = "foremanexport_" + factorioVersionInfo.ProductMajorPart + ".0.0";
            NewPresetName = await ProcessPreset(installPath, foremanModName, modsPath, progress, token).ConfigureAwait(false);
#if DEBUG
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Preset import time: {0} seconds.", (stopwatch.ElapsedMilliseconds / 1000).ToString("0.0", CultureInfo.InvariantCulture)));
            ErrorLogging.LogLine(string.Format(CultureInfo.InvariantCulture, "Preset import time: {0} seconds.", (stopwatch.ElapsedMilliseconds / 1000).ToString("0.0", CultureInfo.InvariantCulture)));
#endif

            if (!string.IsNullOrEmpty(NewPresetName)) {
                await this.InvokeOnUiThreadAsync(() => {
                    DialogResult = DialogResult.OK;
                    Close();
                }).ConfigureAwait(false);
            } else {
                await this.InvokeOnUiThreadAsync(() => EnableProgressBar(false)).ConfigureAwait(false);
            }

        }

        private async Task<string> ProcessPreset(string installPath, string foremanModName, string modsPath, IProgress<KeyValuePair<int, string>> progress, CancellationToken token) {
            return await Task.Run(async () => {
                //prepare for running factorio
                string exePath = FactorioPathsProcessor.GetExecutablePath(installPath);
                string presetPath = PresetProcessor.GetPresetPath(NewPresetName, "");
                if (!File.Exists(exePath)) {
                    UserMessages.Show("factorio.exe not found..."); //considering that we got here with factorio.exe checks, this is a bit redundant. but whatevs.
                    CleanupFailedImport();
                    return "";
                }
                //ensure mod path exists and doesnt have the foreman export mod in it
                try {
                    if (!Directory.Exists(modsPath))
                        Directory.CreateDirectory(modsPath);
                    if (Directory.Exists(Path.Combine(modsPath, foremanModName)))
                        Directory.Delete(Path.Combine(modsPath, foremanModName));
                } catch (Exception e) {
                    if (e is UnauthorizedAccessException) {
                        UserMessages.Show("Insufficient access to the factorio mods folder. Please ensure factorio mods are in an accessible folder, or launch Foreman with Administrator privileges.");
                        ErrorLogging.LogException(e, "insufficient access to factorio mods folder");
                    } else {
                        UserMessages.Show("Unknown error trying to access factorio mods folder. Sorry");
                        ErrorLogging.LogException(e, "error while accessing factorio mods folder");
                    }
                    CleanupFailedImport(modsPath);
                    return "";
                }

                string tempSavePath = Path.Combine(Application.StartupPath, "temp-save.zip");

                progress.Report(new KeyValuePair<int, string>(10, "Running Factorio - creating test save."));
                FactorioRunResult createRun = FactorioBenchmarkRunner.Run(
                    exePath,
                    string.Format(CultureInfo.InvariantCulture, "--mod-directory \"{0}\" --create temp-save.zip", modsPath),
                    token,
                    () => CleanupFailedImport(modsPath));

                if (string.IsNullOrEmpty(createRun.Output) && token.IsCancellationRequested)
                    return "";

                if (FactorioBenchmarkRunner.IsAnotherInstanceRunning(createRun.Output)) {
                    UserMessages.Show("Foreman export could not be completed because this instance of Factorio is currently running. Please stop expanding the factory for just a brief moment and let the export commence in peace!");
                    CleanupFailedImport(modsPath);
                    return "";
                }

                if (ReportFactorioCrashIfNeeded(createRun, "creating the test save for preset export", modsPath))
                    return "";

                if (!File.Exists(tempSavePath)) {
                    UserMessages.Show(
                        "Factorio did not create the test save (temp-save.zip) needed for preset export.\n\n" +
                        "Factorio may have crashed or exited early. Check factorio-current.log in your Factorio user data folder " +
                        "and try disabling mods until you can create a new game with the same mod list.");
                    ErrorLogging.LogLine($"Foreman preset export: temp-save.zip missing after --create (exit code {createRun.ExitCode}).");
                    WriteExportFailureLog(createRun.Output);
                    CleanupFailedImport(modsPath);
                    return "";
                }

                FactorioModListHelper.SetModState(modsPath, "foremanexport", enabled: true, removeFromListWhenDisabled: false);

                try {
                    FactorioBundledModHelper.CopyToModsFolder(foremanModName, modsPath, "info.json", "instrument-after-data.lua", "instrument-control.lua");
                } catch (Exception e) {
                    if (e is UnauthorizedAccessException) {
                        UserMessages.Show("Insufficient access to copy foreman export mod files (Mods/" + foremanModName + "/) to the factorio mods folder. Please ensure factorio mods are in an accessible folder, or launch Foreman with Administrator privileges.");
                        ErrorLogging.LogException(e, "copying of foreman export mod files failed - insufficient access");
                    } else {
                        UserMessages.Show("could not copy foreman export mod files (Mods/" + foremanModName + "/) to the factorio mods folder. Reinstall foreman?");
                        ErrorLogging.LogException(e, "copying of foreman export mod files failed");
                    }
                    CleanupFailedImport(modsPath);
                    return "";
                }

                progress.Report(new KeyValuePair<int, string>(20, "Running Factorio - foreman export scripts."));
                FactorioRunResult exportRun = FactorioBenchmarkRunner.Run(
                    exePath,
                    string.Format(CultureInfo.InvariantCulture, "--mod-directory \"{0}\" --instrument-mod foremanexport --benchmark temp-save.zip --benchmark-ticks 1 --benchmark-runs 1", modsPath),
                    token,
                    () => CleanupFailedImport(modsPath));

                string resultString = exportRun.Output;

                if (string.IsNullOrEmpty(resultString) && token.IsCancellationRequested)
                    return "";

                if (ReportFactorioCrashIfNeeded(exportRun, "running the preset export scripts", modsPath))
                    return "";

                if (File.Exists(tempSavePath))
                    File.Delete(tempSavePath);
                if (Directory.Exists(Path.Combine(modsPath, foremanModName)))
                    Directory.Delete(Path.Combine(modsPath, foremanModName), true);

                progress.Report(new KeyValuePair<int, string>(25, "Processing mod files."));

                if (FactorioBenchmarkRunner.IsAnotherInstanceRunning(resultString)) {
                    UserMessages.Show("Foreman export could not be completed because this instance of Factorio is currently running. Please stop expanding the factory for just a brief moment and let the export commence in peace!");
                    CleanupFailedImport(modsPath);
                    return "";
                } else if (!resultString.Contains("<<<END-EXPORT-P1>>>", StringComparison.Ordinal) || !resultString.Contains("<<<END-EXPORT-P2>>>", StringComparison.Ordinal)) {
#if DEBUG
                    Console.WriteLine(resultString);
#endif
                    string failureMessage = resultString.Contains("temp-save.zip does not exist", StringComparison.Ordinal)
                        ? "Foreman export could not finish because Factorio could not load the test save (temp-save.zip). " +
                          "The save may not have been created in the previous step; check factorio-current.log for crashes or errors."
                        : "Foreman export could not be completed - possible mod conflict detected. Please run Factorio and ensure it can successfully load to menu before retrying.";
                    UserMessages.Show(failureMessage);
                    ErrorLogging.LogLine("Foreman export failed partway. Consult errorExporting.json for full output (and search for <<<END-EXPORT-P1>>> or <<<END-EXPORT-P2>>>, at least one of which is missing)");
                    WriteExportFailureLog(resultString);
                    CleanupFailedImport(modsPath);
                    return "";
                }
#if DEBUG
                Utf8File.WriteAllText(Path.Combine(Application.StartupPath, "debugExporting.json"), resultString);
#endif

                string lnamesString = resultString[(resultString.IndexOf("<<<START-EXPORT-LN>>>", StringComparison.Ordinal) + 23)..];
                lnamesString = lnamesString[..(lnamesString.IndexOf("<<<END-EXPORT-LN>>>", StringComparison.Ordinal) - 2)];
                lnamesString = lnamesString.Replace("\n", "").Replace("\r", "").Replace("<#~#>", "\n");

                string iconString = resultString[(resultString.IndexOf("<<<START-EXPORT-P1>>>", StringComparison.Ordinal) + 23)..];
                iconString = iconString[..(iconString.IndexOf("<<<END-EXPORT-P1>>>", StringComparison.Ordinal) - 2)];

                string dataString = resultString[(resultString.IndexOf("<<<START-EXPORT-P2>>>", StringComparison.Ordinal) + 23)..];
                dataString = dataString[..(dataString.IndexOf("<<<END-EXPORT-P2>>>", StringComparison.Ordinal) - 2)];

                string[] lnames = lnamesString.Split('\n'); //keep empties - we know where they are!
                var localisedNames = new Dictionary<string, string>(); //this is the link between the 'lid' property and the localised names in dataString
                for (int i = 0; i < lnames.Length / 2; i++)
                    localisedNames.Add('$' + i.ToString(CultureInfo.InvariantCulture), lnames[(i * 2) + 1].Replace("Unknown key: \"", "").Replace("\"", ""));

#if DEBUG
                Utf8File.WriteAllText(Path.Combine(Application.StartupPath, "_iconJObjectOut.json"), iconString.ToString());
                Utf8File.WriteAllText(Path.Combine(Application.StartupPath, "_dataJObjectOut.json"), dataString.ToString());
#endif
                JsonObject? iconJObject;
                JsonObject? dataJObject;
                try {
                    iconJObject = PresetJson.ParseObject(iconString);
                    dataJObject = PresetJson.ParseObject(dataString);
                } catch (Exception ex) {
                    UserMessages.Show("Foreman export could not be completed - unknown json parsing error.\nSorry");
                    ErrorLogging.LogException(ex, "json parsing of export mod output failed (" + foremanModName + "); consult _iconJObjectOut.json and _dataJObjectOut.json");
                    Utf8File.WriteAllText(Path.Combine(Application.StartupPath, "_iconJObjectOut.json"), iconString.ToString());
                    Utf8File.WriteAllText(Path.Combine(Application.StartupPath, "_dataJObjectOut.json"), dataString.ToString());
                    CleanupFailedImport(modsPath);
                    return "";
                }

                //now to trawl over the dataJObject entities and replace any 'lid' with 'localised_name'
                foreach (string groupName in PresetJson.GetObjectPropertyNames(dataJObject)) {
                    if (dataJObject[groupName] is not JsonArray set)
                        continue;
                    foreach (JsonNode? obj in set) {
                        if (obj is JsonObject jobject && PresetJson.GetString(jobject, "lid") is string lid) {
                            jobject["localised_name"] = localisedNames[lid];
                            jobject.Remove("lid");
                        }
                    }
                }

                //save new preset (data)
                Utf8File.WriteAllText(Path.Combine(Application.StartupPath, presetPath + ".pjson"), PresetJson.WriteIndented(dataJObject));
                File.Copy(Path.Combine(Application.StartupPath, "baseCustom.json"), Path.Combine(Application.StartupPath, presetPath + ".json"), true);
#if DEBUG
                Utf8File.WriteAllText(Path.Combine(Application.StartupPath, "_iconJObjectOut.json"), PresetJson.WriteIndented(iconJObject));
                Utf8File.WriteAllText(Path.Combine(Application.StartupPath, "_dataJObjectOut.json"), PresetJson.WriteIndented(dataJObject));
#endif

                if (token.IsCancellationRequested) {
                    CleanupFailedImport(modsPath);
                    return "";
                }

                //now we need to process icons. This is done by the IconProcessor.
                var modSet = new Dictionary<string, string>();
                foreach (JsonNode objJToken in PresetJson.EnumerateArray(dataJObject, "mods"))
                    if (PresetJson.GetString(objJToken, "name") is string name && PresetJson.GetString(objJToken, "version") is string version)
                        modSet.Add(name.ToLowerInvariant(), version);

                using (var icProcessor = new IconCacheProcessor()) {
                    if (!icProcessor.PrepareModPaths(modSet, modsPath, Path.Combine(installPath, "data"), token)) {
                        if (!token.IsCancellationRequested) {
                            UserMessages.Show("Mod inconsistency detected. Try to see if launching Factorio gives an error?");
                            ErrorLogging.LogLine("Mod parsing failed - the list of mods provided could not be mapped to the existing mod folders & zip files.");
                        }
                        CleanupFailedImport(modsPath, presetPath);
                        return "";
                    }

                    if (!await icProcessor.CreateIconCache(iconJObject, Path.Combine(Application.StartupPath, presetPath + ".dat"), progress, 30, 100, token).ConfigureAwait(false)) {
                        if (!token.IsCancellationRequested) {
                            ErrorLogging.LogLine(string.Format(CultureInfo.InvariantCulture, "{0}/{1} images were not found while processing icons.", icProcessor.FailedPathCount, icProcessor.TotalPathCount));
                            if (UserMessages.Show(string.Format(DisplayCulture.Format, "{0}/{1} images that were processed for icons were not found and thus some icons are likely wrong/empty. Do you still wish to continue with the preset import?", icProcessor.FailedPathCount, icProcessor.TotalPathCount), "Confirm Preset Import", MessageBoxButtons.YesNo) != DialogResult.Yes) {
                                CleanupFailedImport(modsPath, presetPath);
                                return "";
                            }
                        } else {
                            CleanupFailedImport(modsPath, presetPath);
                            return "";
                        }
                    }
                }

                FactorioModListHelper.SetModState(modsPath, "foremanexport", enabled: false, removeFromListWhenDisabled: true);
                return NewPresetName;
            }).ConfigureAwait(false);
        }

        private static void WriteExportFailureLog(string output) =>
            Utf8File.WriteAllText(Path.Combine(Application.StartupPath, "errorExporting.json"), output);

        private bool ReportFactorioCrashIfNeeded(FactorioRunResult run, string phaseDescription, string modsPath) {
            if (!run.Crashed)
                return false;

            UserMessages.Show(
                "Factorio crashed while " + phaseDescription + ".\n\n" +
                "This is usually caused by a bug in one of your enabled mods, not by Foreman. " +
                "Open factorio-current.log in your Factorio user data folder for details, " +
                "then try disabling mods until Factorio can start a new game with the same mod list.");
            ErrorLogging.LogLine("Foreman preset export: Factorio crash during " + phaseDescription + " (exit code " + run.ExitCode + ").");
            WriteExportFailureLog(run.Output);
            CleanupFailedImport(modsPath);
            return true;
        }

        private void CleanupFailedImport(string modsPath = "", string presetPath = "", string foremanModName = "") {
            if (!string.IsNullOrEmpty(modsPath))
                FactorioModListHelper.SetModState(modsPath, "foremanexport", enabled: false, removeFromListWhenDisabled: true);

            NewPresetName = "";

            string tempSavePath = Path.Combine(Application.StartupPath, "temp-save.zip");
            if (File.Exists(tempSavePath))
                File.Delete(tempSavePath);

            if (!string.IsNullOrEmpty(modsPath) && !string.IsNullOrEmpty(foremanModName) && Directory.Exists(Path.Combine(modsPath, foremanModName)))
                Directory.Delete(Path.Combine(modsPath, foremanModName), true);

            if (!string.IsNullOrEmpty(presetPath) && !string.IsNullOrEmpty(foremanModName) && File.Exists(Path.Combine(Application.StartupPath, presetPath + ".pjson")))
                File.Delete(Path.Combine(Application.StartupPath, presetPath + ".pjson"));
            if (!string.IsNullOrEmpty(presetPath) && !string.IsNullOrEmpty(foremanModName) && File.Exists(Path.Combine(Application.StartupPath, presetPath + ".json")))
                File.Delete(Path.Combine(Application.StartupPath, presetPath + ".json"));
            if (!string.IsNullOrEmpty(presetPath) && !string.IsNullOrEmpty(foremanModName) && File.Exists(Path.Combine(Application.StartupPath, presetPath + ".dat")))
                File.Delete(Path.Combine(Application.StartupPath, presetPath + ".dat"));
        }

        private void PresetNameTextBox_TextChanged(object? sender, EventArgs e) {
            int i = PresetNameTextBox.SelectionStart;
            string filteredText = string.Concat(PresetNameTextBox.Text.Where(c => char.IsLetterOrDigit(c) || ExtraChars.Contains(c)));
            if (filteredText != PresetNameTextBox.Text) {
                i = Math.Max(i + filteredText.Length - PresetNameTextBox.Text.Length, 0);
                PresetNameTextBox.Text = filteredText;
                PresetNameTextBox.SelectionStart = i;
            }

            var existingPresets = MainForm.GetValidPresetsList();
            PresetNameTextBox.BackColor = filteredText.Length < 5
                ? Color.Moccasin
                : existingPresets?.Any(p => string.Equals(p.Name, filteredText, StringComparison.OrdinalIgnoreCase)) is true
                ? Color.Pink
                : Color.LightGreen;
        }
    }
}
