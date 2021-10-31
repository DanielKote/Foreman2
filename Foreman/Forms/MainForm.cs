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
		internal const string DefaultPreset = "_default (Factorio 1.1 Vanilla)";

		public MainForm()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		private void MainForm_Load(object sender, EventArgs e)
        {
			//WindowState = FormWindowState.Maximized;

			bool gotPreset = SetSelectedPreset(Properties.Settings.Default.CurrentPresetName);
			if (gotPreset)
			{
				using (DataLoadForm form = new DataLoadForm())
				{
					form.ShowDialog(); //LOAD FACTORIO DATA
					GraphViewer.Graph = new ProductionGraph(form.GetDataCache());
				}

				ModuleDropDown.SelectedIndex = Properties.Settings.Default.DefaultModules;
				MinorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MinorGridlines;
				MajorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MajorGridlines;
				GridlinesCheckbox.Checked = Properties.Settings.Default.AltGridlines;
				DynamicLWCheckBox.Checked = Properties.Settings.Default.DynamicLineWidth;
				UpdateControlValues();
			}
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
			dialog.DefaultExt = ".fjson";
			dialog.Filter = "Foreman files (*.fjson)|*.fjson|All files|*.*";
			dialog.AddExtension = true;
			dialog.OverwritePrompt = true;
			dialog.FileName = "Flowchart.fjson";
			if (dialog.ShowDialog() != DialogResult.OK)
				return;

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
			dialog.Filter = "Foreman files (*.fjson)|*.fjson|Old Foreman files (*.json)|*.json";
			dialog.CheckFileExists = true;
			if (dialog.ShowDialog() != DialogResult.OK)
				return;

			try
			{
				if(Path.GetExtension(dialog.FileName).ToLower() == ".fjson")
					GraphViewer.LoadFromJson(JObject.Parse(File.ReadAllText(dialog.FileName)));
				else if(Path.GetExtension(dialog.FileName).ToLower() == ".json")
					GraphViewer.LoadFromOldJson(JObject.Parse(File.ReadAllText(dialog.FileName)));
				//NOTE: MainCache will update
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
			//yea, need to add a help window at some point...
        }

		//---------------------------------------------------------Settings/export/additem/addrecipe

		private void SettingsButton_Click(object sender, EventArgs e)
		{
			bool reload = false;
			do
			{
				SettingsForm.SettingsFormOptions oldOptions = new SettingsForm.SettingsFormOptions();
				foreach (Assembler assembler in GraphViewer.Graph.DCache.Assemblers.Values)
					oldOptions.Assemblers.Add(assembler, assembler.Enabled);
				foreach (Miner miner in GraphViewer.Graph.DCache.Miners.Values)
					oldOptions.Miners.Add(miner, miner.Enabled);
				foreach (Module module in GraphViewer.Graph.DCache.Modules.Values)
					oldOptions.Modules.Add(module, module.Enabled);
				foreach (KeyValuePair<string,string> kvp in GraphViewer.Graph.DCache.IncludedMods)
					oldOptions.Mods.Add(kvp.Key + " - " + kvp.Value);
				oldOptions.Mods.Sort();

				foreach (string presetFile in Directory.GetFiles(Path.Combine(Application.StartupPath, "Presets"), "*.json"))
					if (File.Exists(Path.ChangeExtension(presetFile, "dat")))
						oldOptions.Presets.Add(Path.GetFileNameWithoutExtension(presetFile));

				oldOptions.SelectedPreset = Properties.Settings.Default.CurrentPresetName;
				if(!oldOptions.Presets.Contains(oldOptions.SelectedPreset))
                {
					if (!oldOptions.Presets.Contains(DefaultPreset))
					{
						MessageBox.Show("The default preset (Factorio 1.1 Vanilla) has been removed. Please re-install / re-download Foreman");
						Close();
						Dispose();
						return;
					}
					oldOptions.SelectedPreset = DefaultPreset;
                }					

				using (SettingsForm form = new SettingsForm(oldOptions))
				{
					form.StartPosition = FormStartPosition.Manual;
					form.Left = this.Left + 250;
					form.Top = this.Top + 25;
					form.ShowDialog();
					reload = form.ReloadRequired;

					if (oldOptions.SelectedPreset != form.CurrentOptions.SelectedPreset) //different preset -> need to reload datacache
					{
						SetSelectedPreset(form.CurrentOptions.SelectedPreset);
						GraphViewer.LoadFromJson(JObject.Parse(JsonConvert.SerializeObject(GraphViewer)));
						Properties.Settings.Default.Save();
					}
					else //update the assemblers, miners, modules if we havent switched preset (if we have, then all are enabled)
					{
						foreach (KeyValuePair<Assembler, bool> kvp in form.CurrentOptions.Assemblers)
							kvp.Key.Enabled = kvp.Value;
						foreach (KeyValuePair<Miner, bool> kvp in form.CurrentOptions.Miners)
							kvp.Key.Enabled = kvp.Value;
						foreach (KeyValuePair<Module, bool> kvp in form.CurrentOptions.Modules)
							kvp.Key.Enabled = kvp.Value;
					}

					GraphViewer.UpdateNodes();
					UpdateControlValues();
				}
			} while (reload);
		}

		private bool SetSelectedPreset(string preset) //NOTE: this does NOT! update the cache, it only sets the string in properties.
        {
			if (string.IsNullOrEmpty(preset) ||
				!File.Exists(Path.Combine(new string[] { Application.StartupPath, "Presets", preset + ".json" })) ||
				!File.Exists(Path.Combine(new string[] { Application.StartupPath, "Presets", preset + ".dat" })))
			{
				if (preset != DefaultPreset)
				{
					string message;
					if (string.IsNullOrEmpty(preset))
						message = "No preset has been specified.\nResetting to default preset (Factorio 1.1 Vanilla)";
					else
						message = "Selected preset (" + preset + ") does not exist.\nResetting to default preset (Factorio 1.1 Vanilla)";
					MessageBox.Show(message);
				}

				Properties.Settings.Default.CurrentPresetName = DefaultPreset;
				if (!File.Exists(Path.Combine(new string[] { Application.StartupPath, "Presets", DefaultPreset + ".json" })) ||
					!File.Exists(Path.Combine(new string[] { Application.StartupPath, "Presets", DefaultPreset + ".dat" })))
				{
					MessageBox.Show("The default preset (Factorio 1.1 Vanilla) has been removed. Please re-install / re-download Foreman");
					Close();
					Dispose();
					return false;
				}
			}
			else
				Properties.Settings.Default.CurrentPresetName = preset;
			Properties.Settings.Default.Save();
			return true;
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
