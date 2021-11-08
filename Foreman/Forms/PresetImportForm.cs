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
using System.Text.RegularExpressions;

namespace Foreman
{
	public partial class PresetImportForm : Form
	{
		private char[] ExtraChars = { '(', ')', '-', '_', '.', ' ' };
		private CancellationTokenSource cts;

		public string NewPresetName { get; private set; }
		public bool ImportStarted { get; private set; }

		public PresetImportForm()
		{
			NewPresetName = "";
			ImportStarted = false;
			cts = new CancellationTokenSource();
			InitializeComponent();
			PresetNameTextBox.Focus();

			FactorioLocationComboBox.Items.AddRange(FactorioPathsProcessor.GetFactorioInstallLocations().ToArray());
			if (FactorioLocationComboBox.Items.Count > 0)
				FactorioLocationComboBox.SelectedIndex = 0;
		}

		private void EnableProgressBar(bool enabled)
		{
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

		private void FactorioBrowseButton_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				if (Directory.Exists(FactorioLocationComboBox.Text))
					dialog.SelectedPath = FactorioLocationComboBox.Text;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					if (File.Exists(Path.Combine(new string[] { dialog.SelectedPath, "bin", "x64", "factorio.exe" })))
						FactorioLocationComboBox.Text = dialog.SelectedPath;
					else if(File.Exists(Path.Combine(new string[] {dialog.SelectedPath, "x64", "factorio.exe"})))
						FactorioLocationComboBox.Text = Path.GetDirectoryName(dialog.SelectedPath);
					else if (File.Exists(Path.Combine(dialog.SelectedPath, "factorio.exe")))
						FactorioLocationComboBox.Text = Path.GetDirectoryName(Path.GetDirectoryName(dialog.SelectedPath));
					else
						MessageBox.Show("Selected directory doesnt seem to be a factorio install folder (it should at the very least have \"bin\" and \"data\" folders, along with a \"config-path.cfg\" file)");
				}
			}
		}

		private void ModsBrowseButton_Click(object sender, EventArgs e)
		{
						using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				if (Directory.Exists(ModsLocationComboBox.Text))
					dialog.SelectedPath = ModsLocationComboBox.Text;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					if (File.Exists(Path.Combine(dialog.SelectedPath, "mod-list.json")))
						ModsLocationComboBox.Text = dialog.SelectedPath;
					else
						MessageBox.Show("Selected directory doesnt seem to be a factorio mods folder (it should at the very least have \"mod-list.json\" file)");
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
			if (NewPresetName.Length < 5)
			{
				MessageBox.Show("Preset name has to be longer than 5!");
				CleanupFailedImport();
				return;
			}

			List<Preset> existingPresets = MainForm.GetValidPresetsList();
			if(NewPresetName.ToLower() == MainForm.DefaultPreset.ToLower())
			{
				MessageBox.Show("Cant overwrite default preset!", "", MessageBoxButtons.OK);
				CleanupFailedImport();
				return;
			}
			else if (existingPresets.Any(p => p.Name.ToLower() == NewPresetName.ToLower()))
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
			if (factorioVersionInfo.ProductMajorPart < 1 || factorioVersionInfo.ProductMinorPart < 1 || (factorioVersionInfo.ProductMinorPart == 1 && factorioVersionInfo.ProductBuildPart < 4))
			{
				EnableProgressBar(false);
				MessageBox.Show("Factorio version (" + factorioVersionInfo.ProductVersion + ") can not be used with Foreman. Please use Factorio 1.1.4 or newer.");
				ErrorLogging.LogLine(string.Format("Factorio version was too old. {0} instead of 1.1.4+", factorioVersionInfo.ProductVersion));
				CleanupFailedImport();
				return;
			}

			string modsPath = ModsLocationComboBox.Text;
			if (string.IsNullOrEmpty(modsPath) || !File.Exists(Path.Combine(modsPath, "mod-list.json")))
			{
				string userDataPath = FactorioPathsProcessor.GetFactorioUserPath(installPath, true);
				if (string.IsNullOrEmpty(userDataPath))
				{
					MessageBox.Show("Couldnt auto-locate the mods folder - please manually locate the folder");
					CleanupFailedImport();
					return;
				}
				modsPath = Path.Combine(userDataPath, "mods");
			}

			//we now have the two paths to use - installPath and modsPath. can begin processing Factorio
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
			NewPresetName = await ProcessPreset(installPath, modsPath, progress, token);
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

		private async Task<string> ProcessPreset(string installPath, string modsPath, IProgress<KeyValuePair<int, string>> progress, CancellationToken token)
		{
			return await Task.Run(() =>
			{
				//prepare for running factorio
				string exePath = Path.Combine(new string[] { installPath, "bin", "x64", "factorio.exe" });
				string presetPath = Path.Combine(new string[] { Application.StartupPath, "Presets", NewPresetName });
				if (!File.Exists(exePath))
				{
					MessageBox.Show("factorio.exe not found..."); //considering that we got here with factorio.exe checks, this is a bit redundant. but whatevs.
					CleanupFailedImport();
					return "";
				}
				//ensure mod path exists and doesnt have the foreman export mod in it
				try
				{
					if (!Directory.Exists(modsPath))
						Directory.CreateDirectory(modsPath);
					if (Directory.Exists(Path.Combine(modsPath, "foremanexport_1.0.0")))
						Directory.Delete(Path.Combine(modsPath, "foremanexport_1.0.0"));
				}
				catch (Exception e)
				{
					if (e is UnauthorizedAccessException)
					{
						MessageBox.Show("Insufficient access to the factorio mods folder. Please ensure factorio mods are in an accessible folder, or launch Foreman with Administrator privileges.");
						ErrorLogging.LogLine("insufficient access to factorio mods folder E: " + e.ToString());
					}
					else
					{
						MessageBox.Show("Unknown error trying to access factorio mods folder. Sorry");
						ErrorLogging.LogLine("Error while accessing factorio mods folder E:" + e.ToString());
					}
					CleanupFailedImport(modsPath);
					return "";
				}

				//launch factorio to create the temporary save we will use for export (LAUNCH #1)
				Process process = new Process();
				process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				process.StartInfo.FileName = exePath;

				progress.Report(new KeyValuePair<int, string>(10, "Running Factorio - creating test save."));
				process.StartInfo.Arguments = string.Format("--mod-directory \"{0}\" --create temp-save.zip", modsPath);
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardInput = true;
				process.Start();
				string resultString = "";
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

				if (resultString.IndexOf("Is another instance already running?") != -1)
				{
					MessageBox.Show("Foreman export could not be completed because this instance of Factorio is currently running. Please stop expanding the factory for just a brief moment and let the export commence in peace!");
					CleanupFailedImport(modsPath);
					return "";
				}

				//ensure that the foreman export mod is correctly added to the mod-list and is enabled
				string modListPath = Path.Combine(modsPath, "mod-list.json");
				JObject modlist = null;
				if (!File.Exists(modListPath))
					modlist = new JObject();
				else
					modlist = JObject.Parse(File.ReadAllText(modListPath));
				if (modlist["mods"] == null)
					modlist.Add("mods", new JArray());

				JToken foremanModToken = modlist["mods"].ToList().FirstOrDefault(t => t["name"] != null && (string)t["name"] == "foremanexport");
				if (foremanModToken == null)
					((JArray)modlist["mods"]).Add(new JObject() { { "name", "foremanexport" }, { "enabled", true } });
				else
					foremanModToken["enabled"] = true;

				//copy the files as necessary
				try
				{
					Directory.CreateDirectory(Path.Combine(modsPath, "foremanexport_1.0.0"));
					File.WriteAllText(modListPath, modlist.ToString(Formatting.Indented)); //updated mod list with foreman export enabled

					File.Copy(Path.Combine(new string[] { "Mods", "foremanexport_1.0.0", "info.json" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "info.json" }));
					File.Copy(Path.Combine(new string[] { "Mods", "foremanexport_1.0.0", "instrument-after-data.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-after-data.lua" }), true);

					//recipe&technology difficulties each have their own lua script
					if (NormalRecipeRButton.Checked)
					{
						if (NormalTechnologyRButton.Checked)
							File.Copy(Path.Combine(new string[] { "Mods", "foremanexport_1.0.0", "instrument-control - nn.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-control.lua" }), true);
						else
							File.Copy(Path.Combine(new string[] { "Mods", "foremanexport_1.0.0", "instrument-control - ne.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-control.lua" }), true);
					}
					else
					{
						if (NormalTechnologyRButton.Checked)
							File.Copy(Path.Combine(new string[] { "Mods", "foremanexport_1.0.0", "instrument-control - en.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-control.lua" }), true);
						else
							File.Copy(Path.Combine(new string[] { "Mods", "foremanexport_1.0.0", "instrument-control - ee.lua" }), Path.Combine(new string[] { modsPath, "foremanexport_1.0.0", "instrument-control.lua" }), true);
					}
				}
				catch (Exception e)
				{
					if (e is UnauthorizedAccessException)
					{
						MessageBox.Show("Insufficient access to copy foreman export mod files (Mods/foremanexport_1.0.0/) to the factorio mods folder. Please ensure factorio mods are in an accessible folder, or launch Foreman with Administrator privileges.");
						ErrorLogging.LogLine("copying of foreman export mod files failed - insufficient access E:" + e.ToString());
					}
					else
					{
						MessageBox.Show("could not copy foreman export mod files (Mods/foremanexport_1.0.0/) to the factorio mods folder. Reinstall foreman?");
						ErrorLogging.LogLine("copying of foreman export mod files failed. E:" + e.ToString());
					}
					CleanupFailedImport(modsPath);
					return "";
				}

				//launch factorio again to export the data (LAUNCH #2)
				progress.Report(new KeyValuePair<int, string>(20, "Running Factorio - foreman export scripts."));
				process.StartInfo.Arguments = string.Format("--mod-directory \"{0}\" --instrument-mod foremanexport --benchmark temp-save.zip --benchmark-ticks 1 --benchmark-runs 1", modsPath);
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

				string lnamesString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-LN>>>") + 23);
				lnamesString = lnamesString.Substring(0, lnamesString.IndexOf("<<<END-EXPORT-LN>>>") - 2);
				lnamesString = lnamesString.Replace("\n", "").Replace("\r", "").Replace("<#~#>", "\n");

				string iconString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-P1>>>") + 23);
				iconString = iconString.Substring(0, iconString.IndexOf("<<<END-EXPORT-P1>>>") - 2);

				string dataString = resultString.Substring(resultString.IndexOf("<<<START-EXPORT-P2>>>") + 23);
				dataString = dataString.Substring(0, dataString.IndexOf("<<<END-EXPORT-P2>>>") - 2);

				string[] lnames = lnamesString.Split('\n'); //keep empties - we know where they are!
				Dictionary<string, string> localisedNames = new Dictionary<string, string>(); //this is the link between the 'lid' property and the localised names in dataString
				for (int i = 0; i < lnames.Length / 2; i++)
					localisedNames.Add('$' + i.ToString(), lnames[(i * 2) + 1].Replace("Unknown key: \"", "").Replace("\"", ""));

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

				//now to trawl over the dataJObject entities and replace any 'lid' with 'localised_name'
				foreach (JToken set in dataJObject.Values().ToList())
				{
					foreach (JToken obj in set.ToList())
					{
						if (obj is JObject jobject && (string)jobject["lid"] != null)
						{
							JProperty lname = new JProperty("localised_name", localisedNames[(string)jobject["lid"]]);
							jobject.Add(lname);
							jobject.Remove("lid");
						}
					}
				}

				//save new preset (data)
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
			else if (existingPresets.Any(p => p.Name.ToLower() == filteredText.ToLower()))
				PresetNameTextBox.BackColor = Color.Pink;
			else
				PresetNameTextBox.BackColor = Color.LightGreen;
		}
	}
}
