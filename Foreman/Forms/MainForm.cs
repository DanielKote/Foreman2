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
		internal const string DefaultPreset = "Factorio 1.1 Vanilla";

		public MainForm()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		private void MainForm_Load(object sender, EventArgs e)
        {
			//WindowState = FormWindowState.Maximized;

			List<Preset> validPresets = GetValidPresetsList();
			if (validPresets != null && validPresets.Count > 0)
			{
				using (DataLoadForm form = new DataLoadForm(validPresets[0]))
				{
					form.StartPosition = FormStartPosition.Manual;
					form.Left = this.Left + 150;
					form.Top = this.Top + 100;
					form.ShowDialog(); //LOAD FACTORIO DATA
					GraphViewer.DCache = form.GetDataCache();
					//gc collection is unnecessary - first data cache to be created.
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
			FixedAmountButton.Checked = GraphViewer.SelectedAmountType == AmountType.FixedAmount;
			RateButton.Checked = GraphViewer.SelectedAmountType == AmountType.Rate;
			if (GraphViewer.SelectedRateUnit == RateUnit.PerSecond)
				RateOptionsDropDown.SelectedIndex = 0;
			else
				RateOptionsDropDown.SelectedIndex = 1;

			SimpleViewCheckBox.Checked = GraphViewer.SimpleView;

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

			//try
			//{
				if(Path.GetExtension(dialog.FileName).ToLower() == ".fjson")
					GraphViewer.LoadFromJson(JObject.Parse(File.ReadAllText(dialog.FileName)), false);
				else if(Path.GetExtension(dialog.FileName).ToLower() == ".json")
					GraphViewer.LoadFromOldJson(JObject.Parse(File.ReadAllText(dialog.FileName)));
				//NOTE: MainCache will update
			//}
			//catch (Exception exception)
			//{
			//	MessageBox.Show("Could not load this file. See log for more details");
			//	ErrorLogging.LogLine(String.Format("Error loading file '{0}'. Error: '{1}'", dialog.FileName, exception.Message));
			//}

			UpdateControlValues();
			GraphViewer.Invalidate();
		}

		private void ClearButton_Click(object sender, EventArgs e)
		{
			GraphViewer.Graph.ClearGraph();
			GraphViewer.Elements.Clear();
			GraphViewer.Invalidate();
		}

        private void MainHelpButton_Click(object sender, EventArgs e)
        {
			//yea, need to add a help window at some point...
        }

		//---------------------------------------------------------Settings/export/additem/addrecipe

		public static List<Preset> GetValidPresetsList()
        {
			List<Preset> presets = new List<Preset>();
			List<string> existingPresetFiles = new List<string>();
			foreach (string presetFile in Directory.GetFiles(Path.Combine(Application.StartupPath, "Presets"), "*.json"))
				if (File.Exists(Path.ChangeExtension(presetFile, "dat")))
					existingPresetFiles.Add(Path.GetFileNameWithoutExtension(presetFile));
			existingPresetFiles.Sort();

			if (!existingPresetFiles.Contains(Properties.Settings.Default.CurrentPresetName))
			{
				MessageBox.Show("The current preset (" + Properties.Settings.Default.CurrentPresetName + ") has been removed. Switching to the default preset (Factorio 1.1 Vanilla)");
				Properties.Settings.Default.CurrentPresetName = DefaultPreset;
			}
			if (!existingPresetFiles.Contains(DefaultPreset))
			{
				MessageBox.Show("The default preset (Factorio 1.1 Vanilla) has been removed. Please re-install / re-download Foreman");
				Application.Exit();
				return null;
			}
			existingPresetFiles.Remove(Properties.Settings.Default.CurrentPresetName);
			existingPresetFiles.Remove(DefaultPreset);

			presets.Add(new Preset(Properties.Settings.Default.CurrentPresetName, true, Properties.Settings.Default.CurrentPresetName == DefaultPreset));
			if (Properties.Settings.Default.CurrentPresetName != DefaultPreset)
				presets.Add(new Preset(DefaultPreset, false, true));
			foreach (string presetName in existingPresetFiles)
				presets.Add(new Preset(presetName, false, false));

			Properties.Settings.Default.Save();
			return presets;
		}

		private void SettingsButton_Click(object sender, EventArgs e)
		{
			bool reload = false;
			do
			{
				SettingsForm.SettingsFormOptions oldOptions = new SettingsForm.SettingsFormOptions();
				foreach (Assembler assembler in GraphViewer.DCache.Assemblers.Values)
					oldOptions.Assemblers.Add(assembler, assembler.Enabled);
				foreach (Miner miner in GraphViewer.DCache.Miners.Values)
					oldOptions.Miners.Add(miner, miner.Enabled);
				foreach (Module module in GraphViewer.DCache.Modules.Values)
					oldOptions.Modules.Add(module, module.Enabled);

				oldOptions.Presets = GetValidPresetsList();
				oldOptions.SelectedPreset = oldOptions.Presets[0];

				using (SettingsForm form = new SettingsForm(oldOptions))
				{
					form.StartPosition = FormStartPosition.Manual;
					form.Left = this.Left + 50;
					form.Top = this.Top + 50;
					form.ShowDialog();
					reload = (oldOptions.SelectedPreset != form.CurrentOptions.SelectedPreset); //if we changed the preset, then load up another settings form after processing

					if (oldOptions.SelectedPreset != form.CurrentOptions.SelectedPreset) //different preset -> need to reload datacache
					{
						Properties.Settings.Default.CurrentPresetName = form.CurrentOptions.SelectedPreset.Name;
						List<Preset> validPresets = GetValidPresetsList();
						GraphViewer.LoadFromJson(JObject.Parse(JsonConvert.SerializeObject(GraphViewer)), true, true);
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

					GraphViewer.Invalidate();
				}
			} while (reload);
		}

		private void ExportImageButton_Click(object sender, EventArgs e)
		{
			ImageExportForm form = new ImageExportForm(GraphViewer);
			form.StartPosition = FormStartPosition.Manual;
			form.Left = this.Left + 50;
			form.Top = this.Top + 50;
			form.ShowDialog();
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
				this.GraphViewer.SelectedAmountType = AmountType.Rate;
				RateOptionsDropDown.Enabled = true;
			}
			else
			{
				RateOptionsDropDown.Enabled = false;
			}
			GraphViewer.Graph.UpdateNodeValues();
		}

		private void FixedAmountButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.SelectedAmountType = AmountType.FixedAmount;
			}

			GraphViewer.Graph.UpdateNodeValues();
		}

		private void RateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((sender as ComboBox).SelectedIndex)
			{
				case 0:
					GraphViewer.SelectedRateUnit = RateUnit.PerSecond;
					break;
				case 1:
					GraphViewer.SelectedRateUnit = RateUnit.PerMinute;
					break;
			}
			GraphViewer.Graph.UpdateNodeValues();
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

		private void SimpleViewCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.SimpleView = SimpleViewCheckBox.Checked;
			GraphViewer.Invalidate();
        }

		private void ModuleDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
			Properties.Settings.Default.DefaultModules = ModuleDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

		//---------------------------------------------------------double buffering commands

		public static void SetDoubleBuffered(Control c)
		{
			if (SystemInformation.TerminalServerSession)
				return;
			System.Reflection.PropertyInfo aProp = typeof(Control).GetProperty("DoubleBuffered",
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

    public class Preset : IEquatable<Preset>
	{
		public string Name { get; set; }
		public bool IsCurrentlySelected { get; set; }
		public bool IsDefaultPreset { get; set; }

		public Preset(string name, bool isCurrentlySelected, bool isDefaultPreset)
		{
			Name = name;
			IsCurrentlySelected = isCurrentlySelected;
			IsDefaultPreset = isDefaultPreset;
		}

		public bool Equals(Preset other)
		{
			return this == other;
		}
	}
}
