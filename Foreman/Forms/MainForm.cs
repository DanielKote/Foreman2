using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Foreman
{
	public partial class MainForm : Form
	{
		internal const string DefaultPreset = "Factorio 1.1 Vanilla";

		public ProductionGraphViewer GraphViewer { get; set; }

		public MainForm()
		{
			InitializeComponent();
			
			GraphViewerTabContainer.ParentForm = this;
			
			schemaList.mainForm = this;
			schemaList.LoadSchemas();

			this.DoubleBuffered = true;
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

        private void MainForm_Load(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Maximized;

			Properties.Settings.Default.ForemanVersion = 5;

			RateOptionsDropDown.Items.AddRange(ProductionGraph.RateUnitNames);
			RateOptionsDropDown.SelectedIndex = Properties.Settings.Default.DefaultRateUnit; ;
			MinorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MinorGridlines;
			MajorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MajorGridlines;
			GridlinesCheckbox.Checked = Properties.Settings.Default.AltGridlines;

			Properties.Settings.Default.Save();

			GraphViewerTabContainer.NewGraph();

			if (GraphViewer != null)
			{
				GraphViewer.Invalidate();
				GraphViewer.Focus();
			}
#if DEBUG
			string str = Application.StartupPath + "\\Saved Graphs" + "\\SB_Alien.fjson";
			GraphViewerTabContainer.LoadGraph(str, str);
			//LoadGraph(Path.Combine(new string[] { Application.StartupPath, "Saved Graphs", "NodeLayoutTestpage.fjson" }));
#endif
		}

		//---------------------------------------------------------Save/Load/New/Exit


		private void SaveButton_Click(object sender, EventArgs e)
		{
			Controls.TabPageGV pg = (Controls.TabPageGV)GraphViewerTabContainer.SelectedTab;
			if (pg != null) 
			{ 
				if (pg.savefilePath == null || !pg.SaveGraph(pg.savefilePath))
					pg.SaveGraphAs();
			}
		}

		private void SaveAsGraphButton_Click(object sender, EventArgs e)
		{
			Controls.TabPageGV pg = (Controls.TabPageGV)GraphViewerTabContainer.SelectedTab;
			if (pg != null)
            {
				pg.SaveGraphAs();
            }				
		}

		private void LoadGraphButton_Click(object sender, EventArgs e)
		{
			GraphViewerTabContainer.LoadGraph();
		}

		private void ImportGraphButton_Click(object sender, EventArgs e)
		{
			ImportGraph();
		}

		private void NewGraphButton_Click(object sender, EventArgs e)
		{
			GraphViewerTabContainer.NewGraph();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			//MR_TODO: e.Cancel = !TestGraphSavedStatus();
		}





		private void ImportGraph()
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Foreman files (*.fjson)|*.fjson|Old Foreman files (*.json)|*.json";
			if (!Directory.Exists(Path.Combine(Application.StartupPath, "Saved Graphs")))
				Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Saved Graphs"));
			dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Saved Graphs");
			dialog.CheckFileExists = true;
			if (dialog.ShowDialog() != DialogResult.OK)
				return;

			ImportGraph(dialog.FileName);
		}

		private void ImportGraph(string path)
		{
			try
			{
				GraphViewer.ImportNodesFromJson((JObject)JObject.Parse(File.ReadAllText(path))["ProductionGraph"], GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2)));
			}
			catch (Exception exception)
			{
				MessageBox.Show("Could not import from this file. See log for more details");
				ErrorLogging.LogLine(string.Format("Error importing from file '{0}'. Error: '{1}'", path, exception.Message));
				ErrorLogging.LogLine(string.Format("Full error output: {0}", exception.ToString()));
			}
		}



		//---------------------------------------------------------Settings/export/additem/addrecipe

		public static List<Preset> GetValidPresetsList()
		{
			List<Preset> presets = new List<Preset>();
			List<string> existingPresetFiles = new List<string>();
			foreach (string presetFile in Directory.GetFiles(Path.Combine(Application.StartupPath, "Presets"), "*.pjson"))
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

		private async void SettingsButton_Click(object sender, EventArgs e)
		{
			SettingsForm.SettingsFormOptions options = new SettingsForm.SettingsFormOptions(GraphViewer.DCache);

			options.Presets = GetValidPresetsList();
			options.SelectedPreset = options.Presets[0];

			options.LevelOfDetail = GraphViewer.LevelOfDetail;
			options.NodeCountForSimpleView = GraphViewer.NodeCountForSimpleView;
			options.IconsOnlyIconSize = GraphViewer.IconsSize;

			options.ArrowsOnLinks = GraphViewer.ArrowsOnLinks;
			options.SimplePassthroughNodes = GraphViewer.Graph.DefaultToSimplePassthroughNodes;
			options.DynamicLinkWidth = GraphViewer.DynamicLinkWidth;
			options.ShowRecipeToolTip = GraphViewer.ShowRecipeToolTip;
			options.LockedRecipeEditPanelPosition = GraphViewer.LockedRecipeEditPanelPosition;
			options.FlagOUSuppliedNodes = GraphViewer.FlagOUSuppliedNodes;

			options.DefaultAssemblerStyle = GraphViewer.Graph.AssemblerSelector.DefaultSelectionStyle;
			options.DefaultModuleStyle = GraphViewer.Graph.ModuleSelector.DefaultSelectionStyle;
			options.DefaultNodeDirection = GraphViewer.Graph.DefaultNodeDirection;
			options.SmartNodeDirection = GraphViewer.SmartNodeDirection;

			options.ShowErrorArrows = GraphViewer.ArrowRenderer.ShowErrorArrows;
			options.ShowWarningArrows = GraphViewer.ArrowRenderer.ShowWarningArrows;
			options.ShowDisconnectedArrows = GraphViewer.ArrowRenderer.ShowDisconnectedArrows;
			options.ShowOUSuppliedArrows = GraphViewer.ArrowRenderer.ShowOUNodeArrows;

			options.RoundAssemblerCount = Properties.Settings.Default.RoundAssemblerCount;
			options.AbbreviateSciPacks = Properties.Settings.Default.AbbreviateSciPacks;

			options.EnableExtraProductivityForNonMiners = GraphViewer.Graph.EnableExtraProductivityForNonMiners;
			options.DEV_ShowUnavailableItems = Properties.Settings.Default.ShowUnavailable;
			options.DEV_UseRecipeBWFilters = Properties.Settings.Default.UseRecipeBWfilters;

			options.Solver_LowPriorityPower = GraphViewer.Graph.LowPriorityPower;
			options.Solver_PullConsumerNodes = GraphViewer.Graph.PullOutputNodes;
			options.Solver_PullConsumerNodesPower = GraphViewer.Graph.PullOutputNodesPower;

			options.EnabledObjects.UnionWith(GraphViewer.DCache.Recipes.Values.Where(r => r.Enabled));
			options.EnabledObjects.UnionWith(GraphViewer.DCache.Assemblers.Values.Where(r => r.Enabled));
			options.EnabledObjects.UnionWith(GraphViewer.DCache.Beacons.Values.Where(r => r.Enabled));
			options.EnabledObjects.UnionWith(GraphViewer.DCache.Modules.Values.Where(r => r.Enabled));

			//scroll keys
			options.KeyDownCode = GraphViewer.KeyDownCode;
			options.KeyUpCode = GraphViewer.KeyUpCode;
			options.KeyLeftCode = GraphViewer.KeyLeftCode;
			options.KeyRightCode = GraphViewer.KeyRightCode;
			options.KeyScrollRatio = GraphViewer.KeyScrollRatio;

			using (SettingsForm form = new SettingsForm(options))
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = this.Left + 50;
				form.Top = this.Top + 50;
				if (form.ShowDialog() == DialogResult.OK)
				{
					if (options.SelectedPreset != options.Presets[0] || options.DEV_UseRecipeBWFilters != Properties.Settings.Default.UseRecipeBWfilters || options.RequireReload) //different preset or recipeBWFilter change -> need to reload datacache
					{
						Properties.Settings.Default.CurrentPresetName = form.Options.SelectedPreset.Name;
						Properties.Settings.Default.UseRecipeBWfilters = options.DEV_UseRecipeBWFilters;

						List<Preset> validPresets = GetValidPresetsList();
						await GraphViewer.LoadFromJson(JObject.Parse(JsonConvert.SerializeObject(GraphViewer)), true, false);
						//MR_TODO: this.Text = string.Format("Foreman 2.0 ({0}) - {1}", Properties.Settings.Default.CurrentPresetName, savefilePath ?? "Untitled");
					}
					else //not loading a new preset -> update the enabled statuses
					{
						foreach (Recipe recipe in GraphViewer.DCache.Recipes.Values)
							recipe.Enabled = options.EnabledObjects.Contains(recipe);
						foreach (Assembler assembler in GraphViewer.DCache.Assemblers.Values)
							assembler.Enabled = options.EnabledObjects.Contains(assembler);
						foreach (Beacon beacon in GraphViewer.DCache.Beacons.Values)
							beacon.Enabled = options.EnabledObjects.Contains(beacon);
						foreach (Module module in GraphViewer.DCache.Modules.Values)
							module.Enabled = options.EnabledObjects.Contains(module);
						GraphViewer.DCache.RocketAssembler.Enabled = GraphViewer.DCache.Assemblers["rocket-silo"]?.Enabled?? false;
					}

					GraphViewer.LevelOfDetail = options.LevelOfDetail;
					Properties.Settings.Default.LevelOfDetail = (int)options.LevelOfDetail;
					GraphViewer.NodeCountForSimpleView = options.NodeCountForSimpleView;
					Properties.Settings.Default.NodeCountForSimpleView = options.NodeCountForSimpleView;
					GraphViewer.IconsSize = options.IconsOnlyIconSize;
					Properties.Settings.Default.IconsSize = options.IconsOnlyIconSize;

					GraphViewer.ArrowsOnLinks = options.ArrowsOnLinks;
					Properties.Settings.Default.ArrowsOnLinks = options.ArrowsOnLinks;
					GraphViewer.Graph.DefaultToSimplePassthroughNodes = options.SimplePassthroughNodes;
					Properties.Settings.Default.SimplePassthroughNodes = options.SimplePassthroughNodes;
					GraphViewer.DynamicLinkWidth = options.DynamicLinkWidth;
					Properties.Settings.Default.DynamicLineWidth = options.DynamicLinkWidth;
					GraphViewer.ShowRecipeToolTip = options.ShowRecipeToolTip;
					Properties.Settings.Default.ShowRecipeToolTip = options.ShowRecipeToolTip;
					GraphViewer.LockedRecipeEditPanelPosition = options.LockedRecipeEditPanelPosition;
					Properties.Settings.Default.LockedRecipeEditorPosition = options.LockedRecipeEditPanelPosition;
					GraphViewer.FlagOUSuppliedNodes = options.FlagOUSuppliedNodes;
					Properties.Settings.Default.FlagOUSuppliedNodes = options.FlagOUSuppliedNodes;

					GraphViewer.Graph.AssemblerSelector.DefaultSelectionStyle = options.DefaultAssemblerStyle;
					Properties.Settings.Default.DefaultAssemblerOption = (int)options.DefaultAssemblerStyle;
					GraphViewer.Graph.ModuleSelector.DefaultSelectionStyle = options.DefaultModuleStyle;
					Properties.Settings.Default.DefaultModuleOption = (int)options.DefaultModuleStyle;
					GraphViewer.Graph.DefaultNodeDirection = options.DefaultNodeDirection;
					Properties.Settings.Default.DefaultNodeDirection = (int)options.DefaultNodeDirection;
					GraphViewer.SmartNodeDirection = options.SmartNodeDirection;
					Properties.Settings.Default.SmartNodeDirection = options.SmartNodeDirection;

					GraphViewer.ArrowRenderer.ShowErrorArrows = options.ShowErrorArrows;
					Properties.Settings.Default.ShowErrorArrows = options.ShowErrorArrows;
					GraphViewer.ArrowRenderer.ShowWarningArrows = options.ShowWarningArrows;
					Properties.Settings.Default.ShowWarningArrows = options.ShowWarningArrows;
					GraphViewer.ArrowRenderer.ShowDisconnectedArrows = options.ShowDisconnectedArrows;
					Properties.Settings.Default.ShowDisconnectedArrows = options.ShowDisconnectedArrows;
					GraphViewer.ArrowRenderer.ShowOUNodeArrows = options.ShowOUSuppliedArrows;
					Properties.Settings.Default.ShowOUSuppliedArrows = options.ShowOUSuppliedArrows;

					Properties.Settings.Default.RoundAssemblerCount = options.RoundAssemblerCount;
					Properties.Settings.Default.AbbreviateSciPacks = options.AbbreviateSciPacks;

					GraphViewer.Graph.EnableExtraProductivityForNonMiners = options.EnableExtraProductivityForNonMiners;
					Properties.Settings.Default.EnableExtraProductivityForNonMiners = options.EnableExtraProductivityForNonMiners;

					GraphViewer.Graph.LowPriorityPower = options.Solver_LowPriorityPower;
					GraphViewer.Graph.PullOutputNodesPower = options.Solver_PullConsumerNodesPower;
					GraphViewer.Graph.PullOutputNodes = options.Solver_PullConsumerNodes;

					//scroll keys
					GraphViewer.KeyDownCode = options.KeyDownCode;
					GraphViewer.KeyUpCode = options.KeyUpCode;
					GraphViewer.KeyLeftCode = options.KeyLeftCode;
					GraphViewer.KeyRightCode = options.KeyRightCode;
					GraphViewer.KeyScrollRatio = options.KeyScrollRatio;

					Properties.Settings.Default.ShowUnavailable = options.DEV_ShowUnavailableItems;
					Properties.Settings.Default.Save();

					GraphViewer.Graph.UpdateNodeStates();
					GraphViewer.Graph.UpdateNodeValues();

					if (options.RequireReload)
						SettingsButton_Click(this, EventArgs.Empty);
				}
			}
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
			if (GraphViewer != null)
			{
				Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));
				GraphViewer.AddRecipe(new Point(15, 15), null, location, NewNodeType.Disconnected);
			}
		}

		private void AddItemButton_Click(object sender, EventArgs e)
		{
			if (GraphViewer != null)
			{
				Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));
				GraphViewer.AddItem(new Point(15, 15), location);
			}
		}

		//---------------------------------------------------------Key & Mouse events

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.S && (Control.ModifierKeys & Keys.Control) == Keys.Control)
			{ 
				Controls.TabPageGV pg = (Controls.TabPageGV)GraphViewerTabContainer.SelectedTab;
				if (pg.savefilePath == null || !pg.SaveGraph(pg.savefilePath))
					pg.SaveGraphAs(); 
			}
		}

		//---------------------------------------------------------Production Graph properties

		private void RateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (GraphViewer != null)
			{
				Properties.Settings.Default.DefaultRateUnit = RateOptionsDropDown.SelectedIndex;
				GraphViewer.Graph.SelectedRateUnit = (ProductionGraph.RateUnit)RateOptionsDropDown.SelectedIndex;
				Properties.Settings.Default.Save();
				GraphViewer.Graph.UpdateNodeValues();
			}
		}

		private void PauseUpdatesCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.Graph.PauseUpdates = PauseUpdatesCheckbox.Checked;
			if (!GraphViewer.Graph.PauseUpdates)
				GraphViewer.Graph.UpdateNodeValues();
			else
				GraphViewer.Invalidate();
		}

		private void IconViewCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.IconsOnly = IconViewCheckBox.Checked;
			Properties.Settings.Default.IconsOnlyView = IconViewCheckBox.Checked;
			Properties.Settings.Default.Save();
			GraphViewer.Invalidate();

		}

		private void GraphSummaryButton_Click(object sender, EventArgs e)
		{
			using (GraphSummaryForm form = new GraphSummaryForm(GraphViewer.Graph.Nodes, GraphViewer.Graph.NodeLinks, GraphViewer.Graph.GetRateName()))
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = this.Left + 50;
				form.Top = this.Top + 50;
				form.ShowDialog();

			}
		}

		//---------------------------------------------------------Gridlines

		private void MinorGridlinesDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			int updatedGridUnit = 0;
			if (MinorGridlinesDropDown.SelectedIndex > 0)
				updatedGridUnit = 6 * (int)(Math.Pow(2, MinorGridlinesDropDown.SelectedIndex - 1));

			if (GraphViewer != null)
			{
				if (GraphViewer.Grid.CurrentGridUnit != updatedGridUnit)
				{
					GraphViewer.Grid.CurrentGridUnit = updatedGridUnit;
					GraphViewer.Invalidate();
				}
			}
			Properties.Settings.Default.MinorGridlines = MinorGridlinesDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

		private void MajorGridlinesDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			int updatedGridUnit = 0;
			if (MajorGridlinesDropDown.SelectedIndex > 0)
				updatedGridUnit = 6 * (int)(Math.Pow(2, MajorGridlinesDropDown.SelectedIndex - 1));

			if (GraphViewer != null)
			{
				if (GraphViewer.Grid.CurrentMajorGridUnit != updatedGridUnit)
				{
					GraphViewer.Grid.CurrentMajorGridUnit = updatedGridUnit;
					GraphViewer.Invalidate();
				}
			}
			Properties.Settings.Default.MajorGridlines = MajorGridlinesDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

		private void GridlinesCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (GraphViewer != null)
			{
				if (GraphViewer.Grid.ShowGrid != GridlinesCheckbox.Checked)
				{
					GraphViewer.Grid.ShowGrid = GridlinesCheckbox.Checked;
					GraphViewer.Invalidate();
				}

				Properties.Settings.Default.AltGridlines = (GridlinesCheckbox.Checked);
				Properties.Settings.Default.Save();
			}
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
