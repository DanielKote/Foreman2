using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Drawing;

namespace Foreman
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		private void MainForm_Load(object sender, EventArgs e)
        {
			//WindowState = FormWindowState.Maximized;

            //I changed the name of the variable (again), so this copies the value over for people who are upgrading their Foreman version
            if (Properties.Settings.Default.FactorioPath == "" && Properties.Settings.Default.FactorioDataPath != "")
            {
                Properties.Settings.Default.FactorioPath = Path.GetDirectoryName(Properties.Settings.Default.FactorioDataPath);
                Properties.Settings.Default.FactorioDataPath = "";
            }

            if (!Directory.Exists(Properties.Settings.Default.FactorioPath))
            {
                List<FoundInstallation> installations = new List<FoundInstallation>();
                String steamFolder = Path.Combine("Steam", "steamapps", "common");
                foreach (String defaultPath in new String[]{
                  Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"), steamFolder, "Factorio"),
                  Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), steamFolder, "Factorio"),
                  Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"), "Factorio"),
                  Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), "Factorio"),
                  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Applications", "factorio.app", "Contents")}) //Not actually tested on a Mac
                {
                    if (Directory.Exists(defaultPath))
                    {
                        FoundInstallation inst = FoundInstallation.GetInstallationFromPath(defaultPath);
                        if (inst != null)
                            installations.Add(inst);
                    }
                }

                if (installations.Count > 0)
                {
                    if (installations.Count > 1)
                    {
                        using (InstallationChooserForm form = new InstallationChooserForm(installations))
                        {
                            if (form.ShowDialog() == DialogResult.OK && form.SelectedPath != null)
                            {
                                Properties.Settings.Default.FactorioPath = form.SelectedPath;
                            }
                            else
                            {
                                Close();
                                Dispose();
                                return;
                            }
                        }
                    }
                    else
                    {
                        Properties.Settings.Default.FactorioPath = installations[0].path;
                    }

                    Properties.Settings.Default.Save();
                }
            }

            if (!Directory.Exists(Properties.Settings.Default.FactorioPath))
            {
                using (DirectoryChooserForm form = new DirectoryChooserForm(""))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        Properties.Settings.Default.FactorioPath = form.SelectedPath;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        Close();
                        Dispose();
                        return;
                    }
                }
            }

            if (!Directory.Exists(Properties.Settings.Default.FactorioUserDataPath))
            {
                if (Directory.Exists(Properties.Settings.Default.FactorioPath))
                    Properties.Settings.Default.FactorioUserDataPath = Properties.Settings.Default.FactorioPath;
                else if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio")))
                    Properties.Settings.Default.FactorioUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio");
            }

            if (Properties.Settings.Default.EnabledMods == null) Properties.Settings.Default.EnabledMods = new StringCollection();
            if (Properties.Settings.Default.EnabledAssemblers == null) Properties.Settings.Default.EnabledAssemblers = new StringCollection();
            if (Properties.Settings.Default.EnabledMiners == null) Properties.Settings.Default.EnabledMiners = new StringCollection();
            if (Properties.Settings.Default.EnabledModules == null) Properties.Settings.Default.EnabledModules = new StringCollection();

			ModuleDropDown.SelectedIndex = Properties.Settings.Default.DefaultModules;
			MinorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MinorGridlines;
			MajorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MajorGridlines;
			GridlinesCheckbox.Checked = Properties.Settings.Default.AltGridlines;
			DynamicLWCheckBox.Checked = Properties.Settings.Default.DynamicLineWidth;

			using (DataReloadForm form = new DataReloadForm(false))
				form.ShowDialog(); //LOAD FACTORIO DATA

			Properties.Settings.Default.EnabledAssemblers.Clear();
			foreach (string assembler in DataCache.Assemblers.Keys)
				Properties.Settings.Default.EnabledAssemblers.Add(assembler);

			Properties.Settings.Default.EnabledMiners.Clear();
			foreach (string miner in DataCache.Miners.Keys)
				Properties.Settings.Default.EnabledMiners.Add(miner);

			Properties.Settings.Default.EnabledModules.Clear();
			foreach (string module in DataCache.Modules.Keys)
				Properties.Settings.Default.EnabledModules.Add(module);

			Properties.Settings.Default.Save();

			UpdateControlValues();
        }

		private void UpdateControlValues()
		{
			FixedAmountButton.Checked = GraphViewer.Graph.SelectedAmountType == AmountType.FixedAmount;
			RateButton.Checked = GraphViewer.Graph.SelectedAmountType == AmountType.Rate;
			if (GraphViewer.Graph.SelectedUnit == RateUnit.PerSecond)
				RateOptionsDropDown.SelectedIndex = 0;
			else
				RateOptionsDropDown.SelectedIndex = 1;

			AssemblerDisplayCheckBox.Checked = GraphViewer.ShowAssemblers;
			MinerDisplayCheckBox.Checked = GraphViewer.ShowMiners;

			GraphViewer.Invalidate();
		}

		//---------------------------------------------------------Save/Load/Clear/Help

		private void SaveGraphButton_Click(object sender, EventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.DefaultExt = ".json";
			dialog.Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
			dialog.AddExtension = true;
			dialog.OverwritePrompt = true;
			dialog.FileName = "Flowchart.json";
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			var serialiser = JsonSerializer.Create();
			serialiser.Formatting = Formatting.Indented;
			var writer = new JsonTextWriter(new StreamWriter(dialog.FileName));
			try
			{
				serialiser.Serialize(writer, GraphViewer);
			}
			catch (Exception exception)
			{
				MessageBox.Show("Could not save this file. See log for more details");
				ErrorLogging.LogLine(String.Format("Error saving file '{0}'. Error: '{1}'", dialog.FileName, exception.Message));
			}
			finally
			{
				writer.Close();
			}
		}

		private void LoadGraphButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
			dialog.CheckFileExists = true;
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			try
			{
				GraphViewer.LoadFromJson(JObject.Parse(File.ReadAllText(dialog.FileName)), false);
			}
			catch (Exception exception)
			{
				MessageBox.Show("Could not load this file. See log for more details");
				ErrorLogging.LogLine(String.Format("Error loading file '{0}'. Error: '{1}'", dialog.FileName, exception.Message));
			}

			UpdateControlValues();
			GraphViewer.Invalidate();
		}

		private void ClearButton_Click(object sender, EventArgs e)
		{
			GraphViewer.Graph.Nodes.Clear();
			GraphViewer.Elements.Clear();
			GraphViewer.Invalidate();
		}

        private void MainHelpButton_Click(object sender, EventArgs e)
        {

        }

		//---------------------------------------------------------Settings/export/additem/addrecipe

		private void SettingsButton_Click(object sender, EventArgs e)
		{
			bool reload = false;
			do
			{
				SettingsForm.SettingsFormOptions oldOptions = new SettingsForm.SettingsFormOptions();
				foreach (Assembler assembler in DataCache.Assemblers.Values)
					oldOptions.Assemblers.Add(assembler, assembler.Enabled);
				foreach (Miner miner in DataCache.Miners.Values)
					oldOptions.Miners.Add(miner, miner.Enabled);
				foreach (Module module in DataCache.Modules.Values)
					oldOptions.Modules.Add(module, module.Enabled);
				foreach (KeyValuePair<string, bool> modKVP in DataCache.Mods.ToArray())
				{
					if(modKVP.Key == "core" || modKVP.Key == "base")
						DataCache.Mods[modKVP.Key] = true;
					oldOptions.Mods.Add(modKVP.Key, modKVP.Value);
				}
				foreach (Language language in FactorioModsProcessor.Languages)
					oldOptions.LanguageOptions.Add(language);

				oldOptions.selectedLanguage = FactorioModsProcessor.Languages.FirstOrDefault(l => l.Name == Properties.Settings.Default.Language);
				oldOptions.InstallLocation = Properties.Settings.Default.FactorioPath;
				oldOptions.UserDataLocation = Properties.Settings.Default.FactorioUserDataPath;
				oldOptions.GenerationType = (DataCache.GenerationType)(Properties.Settings.Default.GenerationType);
				oldOptions.NormalDifficulty = (Properties.Settings.Default.FactorioNormalDifficulty);

				using (SettingsForm form = new SettingsForm(oldOptions))
				{
					form.StartPosition = FormStartPosition.Manual;
					form.Left = this.Left + 250;
					form.Top = this.Top + 25;
					form.ShowDialog();
					reload = form.ReloadRequested;

					//update the assemblers, miners, modules no matter what
					Properties.Settings.Default.EnabledAssemblers.Clear();
					foreach (KeyValuePair<Assembler, bool> kvp in form.CurrentOptions.Assemblers)
					{
						kvp.Key.Enabled = kvp.Value;
						if (kvp.Value)
							Properties.Settings.Default.EnabledAssemblers.Add(kvp.Key.Name);
					}

					Properties.Settings.Default.EnabledMiners.Clear();
					foreach (KeyValuePair<Miner, bool> kvp in form.CurrentOptions.Miners)
					{
						kvp.Key.Enabled = kvp.Value;
						if (kvp.Value)
							Properties.Settings.Default.EnabledMiners.Add(kvp.Key.Name);
					}

					Properties.Settings.Default.EnabledModules.Clear();
					foreach (KeyValuePair<Module, bool> kvp in form.CurrentOptions.Modules)
					{
						kvp.Key.Enabled = kvp.Value;
						if (kvp.Value)
							Properties.Settings.Default.EnabledModules.Add(kvp.Key.Name);
					}

					DataCache.UpdateRecipesAssemblerStatus();

					//reload the full data cache if we had some major changes to the options
					if (!oldOptions.Equals(form.CurrentOptions, true) || reload) //some changes have been made OR reload was requested
					{
						FactorioModsProcessor.LocaleFiles.Clear();
						FactorioModsProcessor.LoadLocaleFiles((form.CurrentOptions.selectedLanguage == null)? "en" : form.CurrentOptions.selectedLanguage.Name);
						if(form.CurrentOptions.selectedLanguage != null)
							Properties.Settings.Default.Language = form.CurrentOptions.selectedLanguage.Name;

						Properties.Settings.Default.FactorioPath = form.CurrentOptions.InstallLocation;
						Properties.Settings.Default.FactorioUserDataPath = form.CurrentOptions.UserDataLocation;
						Properties.Settings.Default.GenerationType = (int)(form.CurrentOptions.GenerationType);
						Properties.Settings.Default.FactorioNormalDifficulty = form.CurrentOptions.NormalDifficulty;

						Properties.Settings.Default.EnabledMods.Clear();
						foreach (KeyValuePair<string, bool> kvp in form.CurrentOptions.Mods)
						{
							DataCache.Mods[kvp.Key] = kvp.Value;
							Mod mod = FactorioModsProcessor.Mods.FirstOrDefault(n => n.Name == kvp.Key);
							if (mod != null)
								mod.Enabled = kvp.Value;

							Properties.Settings.Default.EnabledMods.Add(kvp.Key);
						}

						GraphViewer.LoadFromJson(JObject.Parse(JsonConvert.SerializeObject(GraphViewer)), reload);
					}

					Properties.Settings.Default.Save();
					GraphViewer.UpdateNodes();
					UpdateControlValues();

				}
			} while (reload);
		}

		private void ExportImageButton_Click(object sender, EventArgs e)
		{
			ImageExportForm form = new ImageExportForm(GraphViewer);
			form.Show();
		}

		private void AddRecipeButton_Click(object sender, EventArgs e)
		{
			Point location = GraphViewer.ScreenToGraph(new Point(this.Width / 2, this.Height / 2));
			GraphViewer.AddRecipe(new Point(15, 15), null, location, ProductionGraphViewer.NewNodeType.Disconnected);
		}

		private void AddItemButton_Click(object sender, EventArgs e)
		{
			Point location = GraphViewer.ScreenToGraph(new Point(this.Width / 2, this.Height / 2));
			GraphViewer.AddItem(new Point(15,15), location);
		}

		//---------------------------------------------------------Production properties

		private void RateButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.Graph.SelectedAmountType = AmountType.Rate;
				RateOptionsDropDown.Enabled = true;
			}
			else
			{
				RateOptionsDropDown.Enabled = false;
			}
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
		}

		private void FixedAmountButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.Graph.SelectedAmountType = AmountType.FixedAmount;
			}

			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
		}

		private void RateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((sender as ComboBox).SelectedIndex)
			{
				case 0:
					GraphViewer.Graph.SelectedUnit = RateUnit.PerSecond;
					break;
				case 1:
					GraphViewer.Graph.SelectedUnit = RateUnit.PerMinute;
					break;
			}
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.Invalidate();
			GraphViewer.UpdateNodes();
		}

		private void DynamicLWCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (GraphViewer.DynamicLinkWidth != DynamicLWCheckBox.Checked)
			{
				GraphViewer.DynamicLinkWidth = DynamicLWCheckBox.Checked;
				GraphViewer.Invalidate();
			}

			Properties.Settings.Default.DynamicLineWidth = (DynamicLWCheckBox.Checked);
			Properties.Settings.Default.Save();
		}

		private void PauseUpdatesCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.Graph.PauseUpdates = PauseUpdatesCheckbox.Checked;
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
		}

		//---------------------------------------------------------Gridlines

		private void MinorGridlinesDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
			int updatedGridUnit = 0;
			if (MinorGridlinesDropDown.SelectedIndex > 0)
				updatedGridUnit = 6 * (int)(Math.Pow(2, MinorGridlinesDropDown.SelectedIndex - 1));

			if(GraphViewer.CurrentGridUnit != updatedGridUnit)
            {
				GraphViewer.CurrentGridUnit = updatedGridUnit;
				GraphViewer.Invalidate();
            }

			Properties.Settings.Default.MinorGridlines = MinorGridlinesDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

		private void MajorGridlinesDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
			int updatedGridUnit = 0;
			if (MajorGridlinesDropDown.SelectedIndex > 0)
				updatedGridUnit = 6 * (int)(Math.Pow(2, MajorGridlinesDropDown.SelectedIndex - 1));

			if (GraphViewer.CurrentMajorGridUnit != updatedGridUnit)
			{
				GraphViewer.CurrentMajorGridUnit = updatedGridUnit;
				GraphViewer.Invalidate();
			}

			Properties.Settings.Default.MajorGridlines = MajorGridlinesDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

		private void GridlinesCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (GraphViewer.ShowGrid != GridlinesCheckbox.Checked)
			{
				GraphViewer.ShowGrid = GridlinesCheckbox.Checked;
				GraphViewer.Invalidate();
			}

			Properties.Settings.Default.AltGridlines = (GridlinesCheckbox.Checked);
			Properties.Settings.Default.Save();
		}

		private void AlignSelectionButton_Click(object sender, EventArgs e)
		{
			GraphViewer.AlignSelected();
		}

		private void GraphViewer_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				GraphViewer.ShowGrid = !GraphViewer.ShowGrid;
				GridlinesCheckbox.Checked = GraphViewer.ShowGrid;
			}
		}

		//---------------------------------------------------------Assemblers

		private void AssemblerDisplayCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.ShowAssemblers = (sender as CheckBox).Checked;
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
		}

		private void MinerDisplayCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.ShowMiners = (sender as CheckBox).Checked;
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
		}

		private void ModuleDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
			switch(ModuleDropDown.SelectedIndex)
            {
				case 1:
					GraphViewer.Graph.defaultModuleSelector = ModuleSelector.Fastest;
					break;

				case 2:
					GraphViewer.Graph.defaultModuleSelector = ModuleSelector.Productive;
					break;

				case 0:
				default:
					GraphViewer.Graph.defaultModuleSelector = ModuleSelector.None;
					break;
            }

			Properties.Settings.Default.DefaultModules = ModuleDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

		//---------------------------------------------------------double buffering commands

		public static void SetDoubleBuffered(System.Windows.Forms.Control c)
		{
			if (System.Windows.Forms.SystemInformation.TerminalServerSession)
				return;
			System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered",
			System.Reflection.BindingFlags.NonPublic |
			System.Reflection.BindingFlags.Instance);
			aProp.SetValue(c, true, null);
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;
				return cp;
			}
		}
    }
}
