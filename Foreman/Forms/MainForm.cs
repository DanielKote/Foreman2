using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static Foreman.NativeMethods;

namespace Foreman {
    public partial class MainForm : Form {
        internal const string DefaultPreset = "Factorio 2.0 Vanilla";
        internal string DefaultAppName;
        private string _savefilePath;

        public MainForm() {
            InitializeComponent();
            DoubleBuffered = true;
            DefaultAppName = Text;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        public void SetLightMode() {
            var falseVal = 0;
            DwmSetWindowAttribute(Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref falseVal, Marshal.SizeOf(typeof(int)));
            ChangeTheme(DefaultBackColor, DefaultForeColor, this);
        }

        private static void ChangeTheme(Color bg, Color fg, Control root) {
            root.BackColor = bg;
            root.ForeColor = fg;
            ChangeTheme(bg, fg, root.Controls);
        }

        private static void ChangeTheme(Color bg, Color fg, Control.ControlCollection container) {
            foreach (Control component in container) {
                ChangeTheme(bg, fg, component.Controls);
                component.BackColor = bg;
                component.ForeColor = fg;

                switch (component) {
                    case Button b:
                        b.UseVisualStyleBackColor = true;
                        b.FlatStyle = FlatStyle.Flat;
                        break;
                    case ProductionGraphViewer pgv:
                        GridManager.SetGridColors(bg, fg);
                        break;
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            WindowState = FormWindowState.Maximized;

            Properties.Settings.Default.ForemanVersion = VersionUpdater.CurrentVersion;

            if (!Enum.IsDefined(typeof(ProductionGraph.RateUnit), Properties.Settings.Default.DefaultRateUnit))
                Properties.Settings.Default.DefaultRateUnit = (int) ProductionGraph.RateUnit.Per1Sec;
            GraphViewer.Graph.SelectedRateUnit = (ProductionGraph.RateUnit) Properties.Settings.Default.DefaultRateUnit;

            if (!Enum.IsDefined(typeof(ModuleSelector.Style), Properties.Settings.Default.DefaultModuleOption))
                Properties.Settings.Default.DefaultModuleOption = (int) ModuleSelector.Style.None;
            GraphViewer.Graph.ModuleSelector.DefaultSelectionStyle = (ModuleSelector.Style) Properties.Settings.Default.DefaultModuleOption;

            if (!Enum.IsDefined(typeof(AssemblerSelector.Style), Properties.Settings.Default.DefaultAssemblerOption))
                Properties.Settings.Default.DefaultAssemblerOption = (int) AssemblerSelector.Style.WorstNonBurner;
            GraphViewer.Graph.AssemblerSelector.DefaultSelectionStyle = (AssemblerSelector.Style) Properties.Settings.Default.DefaultAssemblerOption;

            GraphViewer.ArrowsOnLinks = Properties.Settings.Default.ArrowsOnLinks;
            GraphViewer.DynamicLinkWidth = Properties.Settings.Default.DynamicLineWidth;
            GraphViewer.ShowRecipeToolTip = Properties.Settings.Default.ShowRecipeToolTip;
            GraphViewer.LockedRecipeEditPanelPosition = Properties.Settings.Default.LockedRecipeEditorPosition;

            if (!Enum.IsDefined(typeof(ProductionGraphViewer.Lod), Properties.Settings.Default.LevelOfDetail))
                Properties.Settings.Default.LevelOfDetail = (int) ProductionGraphViewer.Lod.Medium;
            GraphViewer.LevelOfDetail = (ProductionGraphViewer.Lod) Properties.Settings.Default.LevelOfDetail;

            if (!Enum.IsDefined(typeof(NodeDirection), Properties.Settings.Default.DefaultNodeDirection))
                Properties.Settings.Default.DefaultNodeDirection = (int) NodeDirection.Up;
            GraphViewer.Graph.DefaultNodeDirection = (NodeDirection) Properties.Settings.Default.DefaultNodeDirection;

            GraphViewer.SmartNodeDirection = Properties.Settings.Default.SmartNodeDirection;

            GraphViewer.Graph.EnableExtraProductivityForNonMiners = Properties.Settings.Default.EnableExtraProductivityForNonMiners;
            GraphViewer.NodeCountForSimpleView = Properties.Settings.Default.NodeCountForSimpleView;
            GraphViewer.FlagOuSuppliedNodes = Properties.Settings.Default.FlagOUSuppliedNodes;

            GraphViewer.ArrowRenderer.ShowErrorArrows = Properties.Settings.Default.ShowErrorArrows;
            GraphViewer.ArrowRenderer.ShowWarningArrows = Properties.Settings.Default.ShowWarningArrows;
            GraphViewer.ArrowRenderer.ShowDisconnectedArrows = Properties.Settings.Default.ShowDisconnectedArrows;
            GraphViewer.ArrowRenderer.ShowOuNodeArrows = Properties.Settings.Default.ShowOUSuppliedArrows;

            RateOptionsDropDown.Items.AddRange(ProductionGraph.RateUnitNames);
            RateOptionsDropDown.SelectedIndex = (int) GraphViewer.Graph.SelectedRateUnit;
            MinorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MinorGridlines;
            MajorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MajorGridlines;
            GridlinesCheckbox.Checked = Properties.Settings.Default.AltGridlines;

            GraphViewer.Graph.DefaultToSimplePassthroughNodes = Properties.Settings.Default.SimplePassthroughNodes;

            GraphViewer.IconsOnly = Properties.Settings.Default.IconsOnlyView;
            IconViewCheckBox.Checked = GraphViewer.IconsOnly;
            if (Properties.Settings.Default.IconsSize < 8) Properties.Settings.Default.IconsSize = 8;
            if (Properties.Settings.Default.IconsSize > 256) Properties.Settings.Default.IconsSize = 256;
            GraphViewer.IconsSize = Properties.Settings.Default.IconsSize;

            Properties.Settings.Default.Save();

            NewGraph();
            GraphViewer.Invalidate();
            GraphViewer.Focus();
#if DEBUG
            //LoadGraph(Path.Combine(new string[] { Application.StartupPath, "Saved Graphs", "NodeLayoutTestpage.fjson" }));
#endif
        }

        //---------------------------------------------------------Save/Load/New/Exit


        private void SaveButton_Click(object sender, EventArgs e) {
            if (_savefilePath == null || !SaveGraph(_savefilePath))
                SaveGraphAs();
        }

        private void SaveAsGraphButton_Click(object sender, EventArgs e) {
            SaveGraphAs();
        }

        private void LoadGraphButton_Click(object sender, EventArgs e) {
            LoadGraph();
        }

        private void ImportGraphButton_Click(object sender, EventArgs e) {
            ImportGraph();
        }

        private void NewGraphButton_Click(object sender, EventArgs e) {
            NewGraph();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = !TestGraphSavedStatus();
        }

        private void SaveGraphAs() {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = ".fjson";
            dialog.Filter = "Foreman files (*.fjson)|*.fjson|All files|*.*";
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "Saved Graphs")))
                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Saved Graphs"));
            dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Saved Graphs");
            dialog.AddExtension = true;
            dialog.OverwritePrompt = true;
            dialog.FileName = "Flowchart.fjson";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            SaveGraph(dialog.FileName);
        }

        private bool SaveGraph(string path) {
            var serializer = JsonSerializer.Create();
            serializer.Formatting = Formatting.Indented;
            var writer = new JsonTextWriter(new StreamWriter(path));
            try {
                GraphViewer.Graph.SerializeNodeIdSet = null; //we want to save everything.
                serializer.Serialize(writer, GraphViewer);
                _savefilePath = path;
                Text = $"{DefaultAppName} ({Properties.Settings.Default.CurrentPresetName}) - {_savefilePath}";
                return true;
            } catch (Exception exception) {
                MessageBox.Show("Could not save this file. See log for more details");
                ErrorLogging.LogLine($"Error saving file '{path}'. Error: '{exception.Message}'");
                ErrorLogging.LogLine($"Full error output: {exception}");
                return false;
            } finally {
                writer.Close();
            }
        }

        private void LoadGraph() {
            if (!TestGraphSavedStatus())
                return;

            var dialog = new OpenFileDialog();
            dialog.Filter = "Foreman files (*.fjson)|*.fjson|Old Foreman files (*.json)|*.json";
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "Saved Graphs")))
                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Saved Graphs"));
            dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Saved Graphs");
            dialog.CheckFileExists = true;
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            LoadGraph(dialog.FileName);
        }

        private async void LoadGraph(string path) {
            try {
                await GraphViewer.LoadFromJson(JObject.Parse(File.ReadAllText(path)), false, true);
                _savefilePath = path;
            } catch (Exception exception) {
                MessageBox.Show("Could not load this file. See log for more details");
                ErrorLogging.LogLine($"Error loading file '{path}'. Error: '{exception.Message}'");
                ErrorLogging.LogLine($"Full error output: {exception}");
            }

            RateOptionsDropDown.SelectedIndex = (int) GraphViewer.Graph.SelectedRateUnit;
            Properties.Settings.Default.EnableExtraProductivityForNonMiners = GraphViewer.Graph.EnableExtraProductivityForNonMiners;
            Properties.Settings.Default.DefaultRateUnit = (int) GraphViewer.Graph.SelectedRateUnit;
            Properties.Settings.Default.DefaultAssemblerOption = (int) GraphViewer.Graph.AssemblerSelector.DefaultSelectionStyle;
            Properties.Settings.Default.DefaultModuleOption = (int) GraphViewer.Graph.ModuleSelector.DefaultSelectionStyle;
            Properties.Settings.Default.DefaultNodeDirection = (int) GraphViewer.Graph.DefaultNodeDirection;

            Properties.Settings.Default.EnableExtraProductivityForNonMiners = GraphViewer.Graph.EnableExtraProductivityForNonMiners;

            Properties.Settings.Default.Save();
            GraphViewer.Invalidate();
            Text = $"{DefaultAppName} ({Properties.Settings.Default.CurrentPresetName}) - {_savefilePath ?? "Untitled"}";
        }

        private void NewGraph() {
            if (!TestGraphSavedStatus())
                return;

            GraphViewer.ClearGraph();
            GraphViewer.Graph.LowPriorityPower = 2f;
            GraphViewer.Graph.PullOutputNodes = false;
            GraphViewer.Graph.PullOutputNodesPower = 1f;

            var validPresets = GetValidPresetsList();
            if (validPresets is { Count: > 0 }) {
                Properties.Settings.Default.CurrentPresetName = validPresets[0].Name;
                GraphViewer.LoadPreset(validPresets[0]);
                _savefilePath = null;
            } else {
                Properties.Settings.Default.CurrentPresetName = "No Preset!";
            }

            Properties.Settings.Default.Save();
            Text = $"{DefaultAppName} ({Properties.Settings.Default.CurrentPresetName}) - {_savefilePath ?? "Untitled"}";
        }

        private void ImportGraph() {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Foreman files (*.fjson)|*.fjson|Old Foreman files (*.json)|*.json";
            if (!Directory.Exists(Path.Combine(Application.StartupPath, "Saved Graphs")))
                Directory.CreateDirectory(Path.Combine(Application.StartupPath, "Saved Graphs"));
            dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Saved Graphs");
            dialog.CheckFileExists = true;
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            ImportGraph(dialog.FileName);
        }

        private void ImportGraph(string path) {
            try {
                GraphViewer.ImportNodesFromJson((JObject) JObject.Parse(File.ReadAllText(path))["ProductionGraph"],
                    GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2)), true);
            } catch (Exception exception) {
                MessageBox.Show("Could not import from this file. See log for more details");
                ErrorLogging.LogLine($"Error importing from file '{path}'. Error: '{exception.Message}'");
                ErrorLogging.LogLine($"Full error output: {exception}");
            }
        }

        private bool TestGraphSavedStatus() {
            if (_savefilePath == null) {
                if (GraphViewer.Graph.Nodes.Any())
                    return MessageBox.Show("The current graph hasnt been saved!\nIf you continue, you will loose it forever!", "Are you sure?",
                        MessageBoxButtons.OKCancel) == DialogResult.OK;
                else
                    return true;
            }

            if (!File.Exists(_savefilePath))
                return MessageBox.Show("The current graph's save file has been deleted!\nIf you continue, you will loose it forever!", "Are you sure?",
                    MessageBoxButtons.OKCancel) == DialogResult.OK;

            var stringBuilder = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(stringBuilder));

            var serializer = JsonSerializer.Create();
            serializer.Formatting = Formatting.Indented;
            GraphViewer.Graph.SerializeNodeIdSet = null; //we want to save everything.
            serializer.Serialize(writer, GraphViewer);

            if (File.ReadAllText(_savefilePath) == stringBuilder.ToString())
                return true;

            var result = MessageBox.Show("The current graph has been modified!\nDo you wish to save before continuing?", "Are you sure?",
                MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Cancel)
                return false;

            if (result == DialogResult.OK)
                SaveGraph(_savefilePath);
            return true;
        }

        //---------------------------------------------------------Settings/export/additem/addrecipe

        public static List<Preset> GetValidPresetsList() {
            var presets = new List<Preset>();
            var existingPresetFiles = new List<string>();
            foreach (var presetFile in Directory.GetFiles(Path.Combine(Application.StartupPath, "Presets"), "*.pjson")) {
                if (File.Exists(Path.ChangeExtension(presetFile, "dat")))
                    existingPresetFiles.Add(Path.GetFileNameWithoutExtension(presetFile));
            }

            existingPresetFiles.Sort();

            if (!existingPresetFiles.Contains(Properties.Settings.Default.CurrentPresetName)) {
                MessageBox.Show("The current preset (" + Properties.Settings.Default.CurrentPresetName +
                    ") has been removed. Switching to the default preset (Factorio 2.0 Vanilla)");
                Properties.Settings.Default.CurrentPresetName = DefaultPreset;
            }

            if (!existingPresetFiles.Contains(DefaultPreset)) {
                MessageBox.Show("The default preset (Factorio 2.0 Vanilla) has been removed. Please re-install / re-download Foreman");
                Application.Exit();
                return null;
            }

            existingPresetFiles.Remove(Properties.Settings.Default.CurrentPresetName);
            existingPresetFiles.Remove(DefaultPreset);

            presets.Add(new Preset(Properties.Settings.Default.CurrentPresetName, true, Properties.Settings.Default.CurrentPresetName == DefaultPreset));
            if (Properties.Settings.Default.CurrentPresetName != DefaultPreset)
                presets.Add(new Preset(DefaultPreset, false, true));
            foreach (var presetName in existingPresetFiles)
                presets.Add(new Preset(presetName, false, false));

            Properties.Settings.Default.Save();
            return presets;
        }

        private async void SettingsButton_Click(object sender, EventArgs e) {
            var options = new SettingsForm.SettingsFormOptions(GraphViewer.DCache) {
                Presets = GetValidPresetsList()
            };

            options.SelectedPreset = options.Presets[0];

            options.QualitySteps = GraphViewer.Graph.MaxQualitySteps;

            options.LevelOfDetail = GraphViewer.LevelOfDetail;
            options.NodeCountForSimpleView = GraphViewer.NodeCountForSimpleView;
            options.IconsOnlyIconSize = GraphViewer.IconsSize;

            options.ArrowsOnLinks = GraphViewer.ArrowsOnLinks;
            options.SimplePassthroughNodes = GraphViewer.Graph.DefaultToSimplePassthroughNodes;
            options.DynamicLinkWidth = GraphViewer.DynamicLinkWidth;
            options.ShowRecipeToolTip = GraphViewer.ShowRecipeToolTip;
            options.LockedRecipeEditPanelPosition = GraphViewer.LockedRecipeEditPanelPosition;
            options.FlagOuSuppliedNodes = GraphViewer.FlagOuSuppliedNodes;

            //options.FlagDarkMode = Properties.Settings.Default.FlagDarkMode;

            options.DefaultAssemblerStyle = GraphViewer.Graph.AssemblerSelector.DefaultSelectionStyle;
            options.DefaultModuleStyle = GraphViewer.Graph.ModuleSelector.DefaultSelectionStyle;
            options.DefaultNodeDirection = GraphViewer.Graph.DefaultNodeDirection;
            options.SmartNodeDirection = GraphViewer.SmartNodeDirection;

            options.ShowErrorArrows = GraphViewer.ArrowRenderer.ShowErrorArrows;
            options.ShowWarningArrows = GraphViewer.ArrowRenderer.ShowWarningArrows;
            options.ShowDisconnectedArrows = GraphViewer.ArrowRenderer.ShowDisconnectedArrows;
            options.ShowOuSuppliedArrows = GraphViewer.ArrowRenderer.ShowOuNodeArrows;

            options.RoundAssemblerCount = Properties.Settings.Default.RoundAssemblerCount;
            options.AbbreviateSciPacks = Properties.Settings.Default.AbbreviateSciPacks;

            options.EnableExtraProductivityForNonMiners = GraphViewer.Graph.EnableExtraProductivityForNonMiners;
            options.DevShowUnavailableItems = Properties.Settings.Default.ShowUnavailable;
            options.DevUseRecipeBwFilters = Properties.Settings.Default.UseRecipeBWfilters;

            options.SolverLowPriorityPower = GraphViewer.Graph.LowPriorityPower;
            options.SolverPullConsumerNodes = GraphViewer.Graph.PullOutputNodes;
            options.SolverPullConsumerNodesPower = GraphViewer.Graph.PullOutputNodesPower;

            options.EnabledObjects.UnionWith(GraphViewer.DCache.Recipes.Values.Where(r => r.Enabled));
            options.EnabledObjects.UnionWith(GraphViewer.DCache.Assemblers.Values.Where(r => r.Enabled));
            options.EnabledObjects.UnionWith(GraphViewer.DCache.Beacons.Values.Where(r => r.Enabled));
            options.EnabledObjects.UnionWith(GraphViewer.DCache.Modules.Values.Where(r => r.Enabled));
            options.EnabledObjects.UnionWith(GraphViewer.DCache.Qualities.Values.Where(r => r.Enabled));

            using var form = new SettingsForm(options, this);

            form.StartPosition = FormStartPosition.Manual;
            form.Left = Left + 50;
            form.Top = Top + 50;
            if (form.ShowDialog() != DialogResult.OK)
                return;

            // different preset or recipeBWFilter change -> need to reload datacache
            if (options.SelectedPreset != options.Presets[0]
                || options.DevUseRecipeBwFilters != Properties.Settings.Default.UseRecipeBWfilters
                || options.RequireReload
            ) {
                Properties.Settings.Default.CurrentPresetName = form.Options.SelectedPreset.Name;
                Properties.Settings.Default.UseRecipeBWfilters = options.DevUseRecipeBwFilters;

                var validPresets = GetValidPresetsList();
                await GraphViewer.LoadFromJson(JObject.Parse(JsonConvert.SerializeObject(GraphViewer)), true, false);
                Text = $"{DefaultAppName} ({Properties.Settings.Default.CurrentPresetName}) - {_savefilePath ?? "Untitled"}";
            } else { // not loading a new preset -> update the enabled statuses
                foreach (var recipe in GraphViewer.DCache.Recipes.Values)
                    recipe.Enabled = options.EnabledObjects.Contains(recipe);
                foreach (var assembler in GraphViewer.DCache.Assemblers.Values)
                    assembler.Enabled = options.EnabledObjects.Contains(assembler);
                foreach (var beacon in GraphViewer.DCache.Beacons.Values)
                    beacon.Enabled = options.EnabledObjects.Contains(beacon);
                foreach (var module in GraphViewer.DCache.Modules.Values)
                    module.Enabled = options.EnabledObjects.Contains(module);
                foreach (var quality in GraphViewer.DCache.Qualities.Values)
                    quality.Enabled = options.EnabledObjects.Contains(quality);
                GraphViewer.DCache.DefaultQuality.Enabled = true;
                GraphViewer.DCache.RocketAssembler.Enabled = GraphViewer.DCache.Assemblers["rocket-silo"]?.Enabled ?? false;
            }

            GraphViewer.Graph.MaxQualitySteps = options.QualitySteps;

            GraphViewer.LevelOfDetail = options.LevelOfDetail;
            Properties.Settings.Default.LevelOfDetail = (int) options.LevelOfDetail;
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
            GraphViewer.FlagOuSuppliedNodes = options.FlagOuSuppliedNodes;
            Properties.Settings.Default.FlagOUSuppliedNodes = options.FlagOuSuppliedNodes;

            GraphViewer.Graph.AssemblerSelector.DefaultSelectionStyle = options.DefaultAssemblerStyle;
            Properties.Settings.Default.DefaultAssemblerOption = (int) options.DefaultAssemblerStyle;
            GraphViewer.Graph.ModuleSelector.DefaultSelectionStyle = options.DefaultModuleStyle;
            Properties.Settings.Default.DefaultModuleOption = (int) options.DefaultModuleStyle;
            GraphViewer.Graph.DefaultNodeDirection = options.DefaultNodeDirection;
            Properties.Settings.Default.DefaultNodeDirection = (int) options.DefaultNodeDirection;
            GraphViewer.SmartNodeDirection = options.SmartNodeDirection;
            Properties.Settings.Default.SmartNodeDirection = options.SmartNodeDirection;

            GraphViewer.ArrowRenderer.ShowErrorArrows = options.ShowErrorArrows;
            Properties.Settings.Default.ShowErrorArrows = options.ShowErrorArrows;
            GraphViewer.ArrowRenderer.ShowWarningArrows = options.ShowWarningArrows;
            Properties.Settings.Default.ShowWarningArrows = options.ShowWarningArrows;
            GraphViewer.ArrowRenderer.ShowDisconnectedArrows = options.ShowDisconnectedArrows;
            Properties.Settings.Default.ShowDisconnectedArrows = options.ShowDisconnectedArrows;
            GraphViewer.ArrowRenderer.ShowOuNodeArrows = options.ShowOuSuppliedArrows;
            Properties.Settings.Default.ShowOUSuppliedArrows = options.ShowOuSuppliedArrows;

            Properties.Settings.Default.RoundAssemblerCount = options.RoundAssemblerCount;
            Properties.Settings.Default.AbbreviateSciPacks = options.AbbreviateSciPacks;

            GraphViewer.Graph.EnableExtraProductivityForNonMiners = options.EnableExtraProductivityForNonMiners;
            Properties.Settings.Default.EnableExtraProductivityForNonMiners = options.EnableExtraProductivityForNonMiners;

            GraphViewer.Graph.LowPriorityPower = options.SolverLowPriorityPower;
            GraphViewer.Graph.PullOutputNodesPower = options.SolverPullConsumerNodesPower;
            GraphViewer.Graph.PullOutputNodes = options.SolverPullConsumerNodes;

            Properties.Settings.Default.ShowUnavailable = options.DevShowUnavailableItems;
            Properties.Settings.Default.Save();

            GraphViewer.Graph.UpdateNodeMaxQualities();
            GraphViewer.Graph.UpdateNodeStates(true);
            GraphViewer.Graph.UpdateNodeValues();

            if (options.RequireReload)
                SettingsButton_Click(this, EventArgs.Empty);
        }

        private void ExportImageButton_Click(object sender, EventArgs e) {
            var form = new ImageExportForm(GraphViewer);
            form.StartPosition = FormStartPosition.Manual;
            form.Left = Left + 50;
            form.Top = Top + 50;
            form.ShowDialog();
        }

        private void AddRecipeButton_Click(object sender, EventArgs e) {
            var location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));
            GraphViewer.AddNewNode(new Point(15, 15), new ItemQualityPair("adding disconnected recipe node"), location, NewNodeType.Disconnected);
        }

        private void AddItemButton_Click(object sender, EventArgs e) {
            var location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));
            GraphViewer.AddItem(new Point(15, 15), location);
        }

        private void HelpButton_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/DanielKote/Foreman2");
        }

        //---------------------------------------------------------Key & Mouse events

        private void MainForm_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.S && (ModifierKeys & Keys.Control) == Keys.Control)
                if (_savefilePath == null || !SaveGraph(_savefilePath))
                    SaveGraphAs();
        }

        //---------------------------------------------------------Production Graph properties

        private void RateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e) {
            Properties.Settings.Default.DefaultRateUnit = RateOptionsDropDown.SelectedIndex;
            GraphViewer.Graph.SelectedRateUnit = (ProductionGraph.RateUnit) RateOptionsDropDown.SelectedIndex;
            Properties.Settings.Default.Save();
            GraphViewer.Graph.UpdateNodeValues();
        }

        private void PauseUpdatesCheckbox_CheckedChanged(object sender, EventArgs e) {
            GraphViewer.Graph.PauseUpdates = PauseUpdatesCheckbox.Checked;
            if (!GraphViewer.Graph.PauseUpdates)
                GraphViewer.Graph.UpdateNodeValues();
            else
                GraphViewer.Invalidate();
        }

        private void IconViewCheckBox_CheckedChanged(object sender, EventArgs e) {
            GraphViewer.IconsOnly = IconViewCheckBox.Checked;
            Properties.Settings.Default.IconsOnlyView = IconViewCheckBox.Checked;
            Properties.Settings.Default.Save();
            GraphViewer.Invalidate();
        }

        private void GraphSummaryButton_Click(object sender, EventArgs e) {
            using var form = new GraphSummaryForm(GraphViewer.Graph.Nodes, GraphViewer.Graph.NodeLinks, GraphViewer.Graph.GetRateName());

            form.StartPosition = FormStartPosition.Manual;
            form.Left = Left + 50;
            form.Top = Top + 50;
            form.ShowDialog();
        }

        //---------------------------------------------------------Gridlines

        private void MinorGridlinesDropDown_SelectedIndexChanged(object sender, EventArgs e) {
            var updatedGridUnit = 0;
            if (MinorGridlinesDropDown.SelectedIndex > 0)
                updatedGridUnit = 6 * (int) Math.Pow(2, MinorGridlinesDropDown.SelectedIndex - 1);

            if (GraphViewer.Grid.CurrentGridUnit != updatedGridUnit) {
                GraphViewer.Grid.CurrentGridUnit = updatedGridUnit;
                GraphViewer.Invalidate();
            }

            Properties.Settings.Default.MinorGridlines = MinorGridlinesDropDown.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void MajorGridlinesDropDown_SelectedIndexChanged(object sender, EventArgs e) {
            var updatedGridUnit = 0;
            if (MajorGridlinesDropDown.SelectedIndex > 0)
                updatedGridUnit = 6 * (int) Math.Pow(2, MajorGridlinesDropDown.SelectedIndex - 1);

            if (GraphViewer.Grid.CurrentMajorGridUnit != updatedGridUnit) {
                GraphViewer.Grid.CurrentMajorGridUnit = updatedGridUnit;
                GraphViewer.Invalidate();
            }

            Properties.Settings.Default.MajorGridlines = MajorGridlinesDropDown.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void GridlinesCheckbox_CheckedChanged(object sender, EventArgs e) {
            if (GraphViewer.Grid.ShowGrid != GridlinesCheckbox.Checked) {
                GraphViewer.Grid.ShowGrid = GridlinesCheckbox.Checked;
                GraphViewer.Invalidate();
            }

            Properties.Settings.Default.AltGridlines = GridlinesCheckbox.Checked;
            Properties.Settings.Default.Save();
        }

        private void AlignSelectionButton_Click(object sender, EventArgs e) {
            GraphViewer.AlignSelected();
        }

        private void GraphViewer_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode != Keys.Space)
                return;

            GraphViewer.Grid.ShowGrid = !GraphViewer.Grid.ShowGrid;
            GridlinesCheckbox.Checked = GraphViewer.Grid.ShowGrid;
        }

        //---------------------------------------------------------double buffering commands

        public static void SetDoubleBuffered(Control c) {
            if (SystemInformation.TerminalServerSession)
                return;
            var aProp = typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
    }

    public class Preset(string name, bool isCurrentlySelected, bool isDefaultPreset) : IEquatable<Preset> {
        public string Name { get; set; } = name;
        public bool IsCurrentlySelected { get; set; } = isCurrentlySelected;
        public bool IsDefaultPreset { get; set; } = isDefaultPreset;

        public bool Equals(Preset other) {
            return this == other;
        }
    }
}