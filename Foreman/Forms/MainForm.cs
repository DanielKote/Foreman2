using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

				if (!Enum.IsDefined(typeof(ProductionGraphViewer.RateUnit), Properties.Settings.Default.DefaultRateUnit))
					Properties.Settings.Default.DefaultRateUnit = (int)ProductionGraphViewer.RateUnit.Per1Sec;
				GraphViewer.SelectedRateUnit = (ProductionGraphViewer.RateUnit)Properties.Settings.Default.DefaultRateUnit;
				RateOptionsDropDown.Items.AddRange(ProductionGraphViewer.RateUnitNames);
				RateOptionsDropDown.SelectedIndex = (int)GraphViewer.SelectedRateUnit;

				if (!Enum.IsDefined(typeof(ModuleSelector.Style), Properties.Settings.Default.DefaultModuleOption))
					Properties.Settings.Default.DefaultModuleOption = (int)ModuleSelector.Style.None;
				GraphViewer.Graph.ModuleSelector.SelectionStyle = (ModuleSelector.Style)Properties.Settings.Default.DefaultModuleOption;
				ModuleDropDown.Items.AddRange(ModuleSelector.StyleNames);
				ModuleDropDown.SelectedIndex = (int)GraphViewer.Graph.ModuleSelector.SelectionStyle;

				if (!Enum.IsDefined(typeof(AssemblerSelector.Style), Properties.Settings.Default.DefaultAssemblerOption))
					Properties.Settings.Default.DefaultAssemblerOption = (int)AssemblerSelector.Style.WorstNonBurner;
				GraphViewer.Graph.AssemblerSelector.SelectionStyle = (AssemblerSelector.Style)Properties.Settings.Default.DefaultAssemblerOption;
				AssemblerDropDown.Items.AddRange(AssemblerSelector.StyleNames);
				AssemblerDropDown.SelectedIndex = (int)GraphViewer.Graph.AssemblerSelector.SelectionStyle;

				MinorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MinorGridlines;
				MajorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MajorGridlines;
				GridlinesCheckbox.Checked = Properties.Settings.Default.AltGridlines;

				DynamicLWCheckBox.Checked = Properties.Settings.Default.DynamicLineWidth;
				SimpleViewCheckBox.Checked = Properties.Settings.Default.SimpleView;

				Properties.Settings.Default.Save();

				UpdateControlValues();
				GraphViewer.Focus();
			}
		}

		private void UpdateControlValues()
		{
			RateOptionsDropDown.SelectedIndex = (int)GraphViewer.SelectedRateUnit;
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
				GraphViewer.Graph.SerializeNodeList = null; //we want to save everything.
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
				if (Path.GetExtension(dialog.FileName).ToLower() == ".fjson")
					GraphViewer.LoadFromJson(JObject.Parse(File.ReadAllText(dialog.FileName)), false);
				else if (Path.GetExtension(dialog.FileName).ToLower() == ".json")
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
			GraphViewer.ClearGraph();
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
				SettingsForm.SettingsFormOptions oldOptions = new SettingsForm.SettingsFormOptions(GraphViewer.DCache);

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
			Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));
			GraphViewer.AddRecipe(new Point(15, 15), null, location, ProductionGraphViewer.NewNodeType.Disconnected);
		}

		private void AddItemButton_Click(object sender, EventArgs e)
		{
			Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));
			GraphViewer.AddItem(new Point(15, 15), location);
		}

		//---------------------------------------------------------Production properties

		private void RateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.DefaultRateUnit = RateOptionsDropDown.SelectedIndex;
			GraphViewer.SelectedRateUnit = (ProductionGraphViewer.RateUnit)RateOptionsDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
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

			if (GraphViewer.Grid.CurrentGridUnit != updatedGridUnit)
			{
				GraphViewer.Grid.CurrentGridUnit = updatedGridUnit;
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

			if (GraphViewer.Grid.CurrentMajorGridUnit != updatedGridUnit)
			{
				GraphViewer.Grid.CurrentMajorGridUnit = updatedGridUnit;
				GraphViewer.Invalidate();
			}

			Properties.Settings.Default.MajorGridlines = MajorGridlinesDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

		private void GridlinesCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (GraphViewer.Grid.ShowGrid != GridlinesCheckbox.Checked)
			{
				GraphViewer.Grid.ShowGrid = GridlinesCheckbox.Checked;
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
				GraphViewer.Grid.ShowGrid = !GraphViewer.Grid.ShowGrid;
				GridlinesCheckbox.Checked = GraphViewer.Grid.ShowGrid;
			}
		}

		//---------------------------------------------------------Assemblers

		private void SimpleViewCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.SimpleView = SimpleViewCheckBox.Checked;
			Properties.Settings.Default.SimpleView = SimpleViewCheckBox.Checked;
			Properties.Settings.Default.Save();
			GraphViewer.Invalidate();
		}

		private void ModuleDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.DefaultModuleOption = ModuleDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
			GraphViewer.Graph.ModuleSelector.SelectionStyle = (ModuleSelector.Style)ModuleDropDown.SelectedIndex;
		}

		private void AssemblerDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.DefaultAssemblerOption = AssemblerDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
			GraphViewer.Graph.AssemblerSelector.SelectionStyle = (AssemblerSelector.Style)AssemblerDropDown.SelectedIndex;
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
