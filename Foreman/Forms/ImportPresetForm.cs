using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Foreman
{
	public partial class ImportPresetForm : Form
	{
		private char[] ExtraChars = { '(', ')', '-', '_', '.', ' ' };
		private CancellationTokenSource cts;

		public string NewPresetName { get; private set; }
		public bool ImportStarted { get; private set; }

		public ImportPresetForm()
		{
			NewPresetName = "";
			ImportStarted = false;
			cts = new CancellationTokenSource();
			InitializeComponent();

			//check default folders for a factorio installation (to fill in the path as the 'default')
			List<string> factorioPaths = new List<string>();

			//program files install
			string pfConfigPath = Path.Combine(new string[] { "c:\\", "Program Files", "Factorio", "config-path.cfg" });
			if (File.Exists(pfConfigPath))
				factorioPaths.Add(Path.GetDirectoryName(pfConfigPath));

			//steam
			object steamPathA = Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam", "SteamPath", "");
			object steamPathB = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath", "");
			string steamPath = (steamPathA != null && !string.IsNullOrEmpty((string)steamPathA)) ? (string)steamPathA : (steamPathB != null && !string.IsNullOrEmpty((string)steamPathB)) ? (string)steamPathB : "";
			if (!string.IsNullOrEmpty((string)steamPath))
            {
				string libraryFoldersFilePath = Path.Combine(new string[] { (string)steamPath, "steamapps", "libraryfolders.vdf" });
				if(File.Exists(libraryFoldersFilePath))
                {
					string[] steamLSettings = File.ReadAllLines(libraryFoldersFilePath);
					foreach(string line in steamLSettings)
                    {
						if(line.Contains("\"path\""))
                        {
							string libraryPath = line.Substring(0, line.LastIndexOf("\""));
							libraryPath = libraryPath.Substring(libraryPath.LastIndexOf("\"") + 1);
							string factorioConfigPath = Path.Combine(new string[] { libraryPath, "steamapps", "common", "Factorio", "config-path.cfg" });
							if (File.Exists(factorioConfigPath))
							factorioPaths.Add(Path.GetDirectoryName(factorioConfigPath));
                        }
                    }
                }
            }

			FactorioLocationComboBox.Items.AddRange(factorioPaths.ToArray());
			if (FactorioLocationComboBox.Items.Count > 0)
				FactorioLocationComboBox.SelectedIndex = 0;
		}

		private void EnableProgressBar(bool enabled)
        {
			FactorioLocationGroup.Enabled = !enabled;
			FactorioSettingsGroup.Enabled = !enabled;
			PresetNameGroup.Enabled = !enabled;
			OKButton.Enabled = !enabled;
			//CancelImportButton.Enabled = !enabled;
			ImportProgressBar.Visible = enabled;
		}

		private void BrowseButton_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				if (Directory.Exists(FactorioLocationComboBox.Text))
					dialog.SelectedPath = FactorioLocationComboBox.Text;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					if (File.Exists(Path.Combine(new string[] { dialog.SelectedPath, "bin", "x64", "factorio.exe" })))
						FactorioLocationComboBox.Text = dialog.SelectedPath;
					else if (File.Exists(Path.Combine(dialog.SelectedPath, "factorio.exe")))
						FactorioLocationComboBox.Text = Path.Combine(Path.GetDirectoryName(dialog.SelectedPath), @"..\\..\\");
					else
						MessageBox.Show("Selected directory doesnt seem to be a factorio install folder (it should at the very least have \"bin\" and \"data\" folders, along with a \"config-path.cfg\" file)");
				}
			}
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			cts.Cancel();
			DialogResult = DialogResult.Cancel;
			NewPresetName = "";
			Close();
		}

		private async void OKButton_Click(object sender, EventArgs e)
		{
			NewPresetName = PresetNameTextBox.Text;
			if (!Directory.Exists(FactorioLocationComboBox.Text))
			{
				MessageBox.Show("That directory doesn't seem to exist");
				CleanupFailedImport();
				return;
			}
			if(NewPresetName.Length < 5)
            {
				MessageBox.Show("Preset name has to be longer than 5!");
				CleanupFailedImport();
				return;
			}

			List<Preset> existingPresets = MainForm.GetValidPresetsList();
			if (existingPresets.FirstOrDefault(p => p.Name.ToLower() == NewPresetName.ToLower()) != null)
			{
				if (MessageBox.Show("This preset name is already in use. Do you wish to overwrite?", "Confirm Overwrite", MessageBoxButtons.YesNo) != DialogResult.Yes)
				{
					CleanupFailedImport();
					return;
				}
			}

			EnableProgressBar(true);

			string installPath = FactorioLocationComboBox.Text;
			//quick check to ensure the install path is correct (and accept a direct path to the factorio.exe folder just in case)
			if (!File.Exists(Path.Combine(new string[] { installPath, "bin", "x64", "factorio.exe" })))
				if (File.Exists(Path.Combine(installPath, "factorio.exe")))
					installPath = Path.Combine(Path.GetDirectoryName(installPath), @"..\\..\\");

			if (!File.Exists(Path.Combine(new string[] { installPath, "bin", "x64", "factorio.exe" })))
			{
				EnableProgressBar(false);
				MessageBox.Show("Couldnt find factorio.exe (/bin/x64/factorio.exe) - please select a valid Factorio install location");
				CleanupFailedImport();
				return;
			}

			FileVersionInfo factorioVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(new string[] { installPath, "bin", "x64", "factorio.exe" }));
			if(factorioVersionInfo.ProductMajorPart < 1 || factorioVersionInfo.ProductMinorPart < 1 || (factorioVersionInfo.ProductMinorPart == 1 && factorioVersionInfo.ProductBuildPart < 4))
            {
				EnableProgressBar(false);
				MessageBox.Show("Factorio version ("+factorioVersionInfo.ProductVersion+") can not be used with Foreman. Please use Factorio 1.1.4 or newer.");
				ErrorLogging.LogLine(string.Format("Factorio version was too old. {0} instead of 1.1.4+", factorioVersionInfo.ProductVersion));
				CleanupFailedImport();
				return;
			}

			string userDataPath = GetFactorioUserPath(installPath);

			//we now have the two paths to use - installPath and userDataPath. can begin processing Factorio
			var progress = new Progress<KeyValuePair<int, string>>(value =>
			{
				if (value.Key > ImportProgressBar.Value)
					ImportProgressBar.Value = value.Key;
				if (!String.IsNullOrEmpty(value.Value) && value.Value != ImportProgressBar.CustomText)
					ImportProgressBar.CustomText = value.Value;
			}) as IProgress<KeyValuePair<int, string>>;
			var token = cts.Token;

#if DEBUG
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
#endif
			ImportStarted = true;
			NewPresetName = await ProcessPreset(installPath, userDataPath, progress, token);
#if DEBUG
			Console.WriteLine(string.Format("Preset import time: {0} seconds.", (stopwatch.ElapsedMilliseconds / 1000).ToString("0.0")));
			ErrorLogging.LogLine(string.Format("Preset import time: {0} seconds.", (stopwatch.ElapsedMilliseconds / 1000).ToString("0.0")));
#endif

			if (!string.IsNullOrEmpty(NewPresetName))
			{
				DialogResult = DialogResult.OK;
				Close();
			}
			else
            {
				//CleanupFailedImport(); //should have already been done.
				EnableProgressBar(false);
            }

		}

		private string GetFactorioUserPath(string installPath)
        {
			//find config-path.cfg, read it, and use it to find config.ini
			string configPath = Path.Combine(installPath, "config-path.cfg");
			if (!File.Exists(configPath))
			{
				EnableProgressBar(false);
				MessageBox.Show("config-path.cfg missing from the install location. Maybe run Factorio once to ensure all files are there?\nAlternatively a reinstall might be required.");
				ErrorLogging.LogLine(string.Format("config-path.cfg was not found at {0}. this was supposed to be the install folder", installPath));
				CleanupFailedImport();
				return "";
			}

			string config = File.ReadAllText(configPath);
			string configIniPath = Path.Combine(ProcessPathString(config.Substring(12, config.IndexOf('\n') - 12), installPath), "config.ini");

			//read config.ini file
			if (!File.Exists(configIniPath))
			{
				EnableProgressBar(false);
				MessageBox.Show("config.ini could not be found. Factorio setup is corrupted?");
				ErrorLogging.LogLine(string.Format("config.ini file was not found at {0}. config-path.cfg was at {1} and linked here.", configIniPath, configPath));
				CleanupFailedImport();
				return "";
			}
			string[] configIni = File.ReadAllLines(configIniPath);
			string writePath = "";
			foreach (string line in configIni)
				if (line.IndexOf("write-data") != -1 && line.IndexOf(";") != 0)
					writePath = line.Substring(line.IndexOf("write-data") + 11);

			return ProcessPathString(writePath, installPath);
		}

		private string ProcessPathString(string input, string installPath)
        {
			if (input.StartsWith(".factorio"))
			{
				string path = installPath;
				string folder = (input == ".factorio") ? "" : input.Substring(9).Replace("/", "\\");
				if (folder.Length > 0) folder = folder.Substring(1);
				while (folder.IndexOf("..") != -1)
				{
					path = Path.GetDirectoryName(path);
					folder = folder.Substring(folder.IndexOf("..") + 2);
					if (folder.Length > 0) folder = folder.Substring(1);
				}
				return string.IsNullOrEmpty(folder) ? path : Path.Combine(path, folder);
			}
			else if (input.StartsWith("__PATH__executable__"))
			{
				string path = Path.Combine(new string[] { installPath, "bin", "x64" });
				string folder = input.Equals("__PATH__executable__") ? "" : input.Substring(20).Replace("/", "\\");
				if (folder.Length > 0) folder = folder.Substring(1);
				while (folder.IndexOf("..") != -1)
				{
					path = Path.GetDirectoryName(path);
					folder = folder.Substring(folder.IndexOf("..") + 2);
					if (folder.Length > 0) folder = folder.Substring(1);
				}
				return string.IsNullOrEmpty(folder) ? path : Path.Combine(path, folder);
			}
			else if (input.StartsWith("__PATH__system-write-data__"))
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("/", "\\");
				string folder = input.Equals("__PATH__system-write-data__") ? "" : input.Substring(27).Replace("/", "\\");
				if (folder.Length > 0) folder = folder.Substring(1);
				while (folder.IndexOf("..") != -1)
				{
					path = Path.GetDirectoryName(path);
					folder = folder.Substring(folder.IndexOf("..") + 2);
					if (folder.Length > 0) folder = folder.Substring(1);
				}
				return string.IsNullOrEmpty(folder) ? Path.Combine(path, "Factorio") : Path.Combine(new string[] { path, "Factorio", folder });
			}
			else
				ErrorLogging.LogLine("path string (from one of the config files) did not start as expected (.factorio || __PATH__executable__ || __PATH__system-write-data__). Path string:" + input);

			return installPath; //something weird must have happened to end up here. Honesty these path conversions are a bit of a mess - not enough examples to be sure its correct (works with all case 'I' have...)
		}

		private async Task<string> ProcessPreset(string installPath, string userDataPath, IProgress<KeyValuePair<int, string>> progress, CancellationToken token)
        {
			return await Task.Run(() =>
			{
				//prepare for running factorio
				string modsPath = Path.Combine(userDataPath, "mods");
				string exePath = Path.Combine(new string[] { installPath, "bin", "x64", "factorio.exe" });
				string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", NewPresetName });
				if (!File.Exists(exePath))
				{
					MessageBox.Show("factorio.exe not found..."); //considering that we got here with factorio.exe checks, this is a bit redundant. but whatevs.
					CleanupFailedImport();
					return "";
				}
				if (!Directory.Exists(modsPath))
					Directory.CreateDirectory(modsPath);

				Directory.CreateDirectory(Path.Combine(modsPath, "foremanexport_1.0.0"));
				try
				{
					File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_1.0.0", "info.json" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "info.json" }), true);
					File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_1.0.0", "instrument-after-data.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-after-data.lua" }), true);


					//recipe&technology difficulties each have their own lua script
					if (NormalRecipeRButton.Checked)
					{
						if (NormalTechnologyRButton.Checked)
							File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_1.0.0", "instrument-control - nn.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-control.lua" }), true);
						else
							File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_1.0.0", "instrument-control - ne.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-control.lua" }), true);
					}
					else
					{
						if (NormalTechnologyRButton.Checked)
							File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_1.0.0", "instrument-control - en.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-control.lua" }), true);
						else
							File.Copy(Path.Combine(new string[] { "Mod", "foremanexport_1.0.0", "instrument-control - ee.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-control.lua" }), true);
					}
				}
				catch
				{
					MessageBox.Show("could not copy foreman export mod files (Mod/foremanexport_1.0.0/) to the factorio mods folder. Reinstall foreman?");
					ErrorLogging.LogLine("copying of foreman export mod files failed.");
					CleanupFailedImport(modsPath);
					return "";
				}

				Process process = new Process();
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.StartInfo.FileName = exePath;

				progress.Report(new KeyValuePair<int, string>(10, "Running Factorio - creating test save."));
				process.StartInfo.Arguments = "--create temp-save.zip";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardInput = true;
				process.Start();
				string resultString = "";
				while (!process.HasExited)
				{
					resultString += process.StandardOutput.ReadToEnd();
					if(token.IsCancellationRequested)
                    {
						process.Close();
						CleanupFailedImport(modsPath);
						return "";
                    }
					Thread.Sleep(100);
				}

				if(resultString.IndexOf("Is another instance already running?") != -1)
                {
					MessageBox.Show("Foreman export could not be completed because this instance of Factorio is currently running. Please stop expanding the factory for just a brief moment and let the export commence in peace!");
					CleanupFailedImport(modsPath);
					return "";
				}

				progress.Report(new KeyValuePair<int, string>(20, "Running Factorio - foreman export scripts."));
				process.StartInfo.Arguments = "--instrument-mod foremanexport --benchmark temp-save.zip --benchmark-ticks 1 --benchmark-runs 1";
				process.Start();
				resultString = "";
				while (!process.HasExited)
				{
					resultString += process.StandardOutput.ReadToEnd();
					if (token.IsCancellationRequested)
					{
						process.Close();
						CleanupFailedImport(modsPath);
						return "";
					}
					Thread.Sleep(100);
				}

				if (File.Exists("temp-save.zip"))
					File.Delete("temp-save.zip");
				if (Directory.Exists(Path.Combine(modsPath, "foremanexport_1.0.0")))
					Directory.Delete(Path.Combine(modsPath, "foremanexport_1.0.0"), true);

				progress.Report(new KeyValuePair<int, string>(25, "Processing mod files."));

				if (resultString.IndexOf("Is another instance already running?") != -1)
				{
					MessageBox.Show("Foreman export could not be completed because this instance of Factorio is currently running. Please stop expanding the factory for just a brief moment and let the export commence in peace!");
					CleanupFailedImport(modsPath);
					return "";
				}
				else if (resultString.IndexOf("<<<END-EXPORT-P1>>>") == -1 || resultString.IndexOf("<<<END-EXPORT-P2>>>") == -1)
                {
					MessageBox.Show("Foreman export could not be completed - possible mod conflict detected. Please run factorio and ensure it can successfully load to menu before retrying.");
					ErrorLogging.LogLine("Foreman export failed partway. Consult errorExporting.json for full output (and search for <<<END-EXPORT-P1>>> or <<<END-EXPORT-P2>>>, at least one of which is missing)");
					File.WriteAllText(Path.Combine(Application.StartupPath, "errorExporting.json"), resultString);
					CleanupFailedImport(modsPath);
					return "";
				}

				string iconString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-P1>>>") + 23);
				iconString = iconString.Substring(0, iconString.IndexOf("<<<END-EXPORT-P1>>>") - 2);

				string dataString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-P2>>>") + 23);
				dataString = dataString.Substring(0, dataString.IndexOf("<<<END-EXPORT-P2>>>") - 2);

				char[] delims = new[] { '\r', '\n' };
				string[] dataStringLines = dataString.Split(delims, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < dataStringLines.Length; i++)
					if (dataStringLines[i].StartsWith("Unknown key:"))
					{
						dataStringLines[i] = dataStringLines[i].Substring(dataStringLines[i].LastIndexOf("\"") + 1);
						if (dataStringLines[i].StartsWith(" ")) dataStringLines[i] = dataStringLines[i].Substring(1);
					}
				dataString = string.Concat(dataStringLines);

#if DEBUG
				File.WriteAllText(Path.Combine(Application.StartupPath, "_iconJObjectOut.json"), iconString.ToString());
				File.WriteAllText(Path.Combine(Application.StartupPath, "_dataJObjectOut.json"), dataString.ToString());
#endif
				JObject iconJObject = null;
				JObject dataJObject = null;
				try
				{
					iconJObject = JObject.Parse(iconString); //this is what needs to be parsed to get all the icons
					dataJObject = JObject.Parse(dataString); //this is pretty much the entire json preset - just need to save it.
				}
				catch
                {
					MessageBox.Show("Foreman export could not be completed - unknown json parsing error.\nSorry");
					ErrorLogging.LogLine("json parsing of output failed. This is clearly an error with the export mod (foremanexport_1.0.0). Consult _iconJObjectOut.json and _dataJObjectOut.json and check which one isnt a valid json (and why)");
					File.WriteAllText(Path.Combine(Application.StartupPath, "_iconJObjectOut.json"), iconString.ToString());
					File.WriteAllText(Path.Combine(Application.StartupPath, "_dataJObjectOut.json"), dataString.ToString());
					CleanupFailedImport(modsPath);
					return "";
				}

				File.WriteAllText(Path.Combine(Application.StartupPath, presetPath + ".json"), dataJObject.ToString(Formatting.Indented));
#if DEBUG
				File.WriteAllText(Path.Combine(Application.StartupPath, "_iconJObjectOut.json"), iconJObject.ToString(Formatting.Indented));
				File.WriteAllText(Path.Combine(Application.StartupPath, "_dataJObjectOut.json"), dataJObject.ToString(Formatting.Indented));
#endif
				if (token.IsCancellationRequested)
				{
					process.Close();
					CleanupFailedImport(modsPath);
					return "";
				}

				//now we need to process icons. This is done by the IconProcessor.
				Dictionary<string, string> modSet = new Dictionary<string, string>();
				foreach (var objJToken in dataJObject["mods"].ToList())
					modSet.Add((string)objJToken["name"], (string)objJToken["version"]);

				using (IconCacheProcessor icProcessor = new IconCacheProcessor())
				{
					if (!icProcessor.PrepareModPaths(modSet, modsPath, Path.Combine(installPath, "data"), token))
					{
						if (!token.IsCancellationRequested)
						{
							MessageBox.Show("Mod inconsistency detected. Try to see if launching Factorio gives an error?");
							ErrorLogging.LogLine("Mod parsing failed - the list of mods provided could not be mapped to the existing mod folders & zip files.");
						}
						CleanupFailedImport(modsPath, presetPath);
						return "";
					}

					if (!icProcessor.CreateIconCache(iconJObject, Path.Combine(Application.StartupPath, presetPath + ".dat"), progress, token, 30, 100))
					{
						if (!token.IsCancellationRequested)
						{
							ErrorLogging.LogLine(string.Format("{0}/{1} images were not found while processing icons.", icProcessor.FailedPathCount, icProcessor.TotalPathCount));
							if (MessageBox.Show(string.Format("{0}/{1} images that were processed for icons were not found and thus some icons are likely wrong/empty. Do you still wish to continue with the preset import?", icProcessor.FailedPathCount, icProcessor.TotalPathCount), "Confirm Preset Import", MessageBoxButtons.YesNo) != DialogResult.Yes)
							{
								CleanupFailedImport(modsPath, presetPath);
								return "";
							}
						}
						else
                        {
							CleanupFailedImport(modsPath, presetPath);
							return "";
                        }
					}
				}

				return NewPresetName;
			});
        }

		private void CleanupFailedImport(string modsPath = "", string presetPath = "")
        {
			NewPresetName = "";

			if (File.Exists("temp-save.zip"))
				File.Delete("temp-save.zip");

			if (modsPath != "" && Directory.Exists(Path.Combine(modsPath, "foremanexport_1.0.0")))
				Directory.Delete(Path.Combine(modsPath, "foremanexport_1.0.0"), true);

			if (presetPath != "" && File.Exists(Path.Combine(Application.StartupPath, presetPath + ".json")))
				File.Delete(Path.Combine(Application.StartupPath, presetPath + ".json"));
			if (presetPath != "" && File.Exists(Path.Combine(Application.StartupPath, presetPath + ".dat")))
				File.Delete(Path.Combine(Application.StartupPath, presetPath + ".dat"));
		}

        private void PresetNameTextBox_TextChanged(object sender, EventArgs e)
        {
			int i = PresetNameTextBox.SelectionStart;
			string filteredText = string.Concat(PresetNameTextBox.Text.Where(c => char.IsLetterOrDigit(c) || ExtraChars.Contains(c)));
			if (filteredText != PresetNameTextBox.Text)
			{
				i = Math.Max(i + filteredText.Length - PresetNameTextBox.Text.Length, 0);
				PresetNameTextBox.Text = filteredText;
				PresetNameTextBox.SelectionStart = i;
			}

			List<Preset> existingPresets = MainForm.GetValidPresetsList();
			if (filteredText.Length < 5)
				PresetNameTextBox.BackColor = Color.Moccasin;
			else if (existingPresets.FirstOrDefault(p => p.Name.ToLower() == filteredText.ToLower()) != null)
				PresetNameTextBox.BackColor = Color.Pink;
			else
				PresetNameTextBox.BackColor = Color.LightGreen;
        }
    }
}
