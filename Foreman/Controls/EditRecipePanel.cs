using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class EditRecipePanel : UserControl {
        private static readonly Color ErrorColor = Color.DarkRed;
        private static readonly Color SelectedColor = Color.DarkOrange;

        private List<Button> _assemblerOptions;
        private List<Button> _fuelOptions;
        private List<Button> _assemblerModules;
        private List<Button> _aModuleOptions;
        private List<Button> _beaconOptions;
        private List<Button> _beaconModules;
        private List<Button> _bModuleOptions;

        private Dictionary<object, int> _lastScrollY;

        private readonly ProductionGraphViewer _myGraphViewer;
        private readonly RecipeNodeController _nodeController;
        private readonly ReadOnlyRecipeNode _nodeData;

        private double RateMultiplier => _myGraphViewer.Graph.GetRateMultiplier();

        private string RateName => _myGraphViewer.Graph.GetRateName();

        private List<Quality> _qualitySelectorIndexSet;

        public EditRecipePanel(ReadOnlyRecipeNode node, ProductionGraphViewer graphViewer) {
            _nodeData = node;
            _nodeController = (RecipeNodeController) graphViewer.Graph.RequestNodeController(node);
            _myGraphViewer = graphViewer;
            _qualitySelectorIndexSet = [];

            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            // simplest way of ensuring the width of the panel remains constant
            // (it needs to be automized during initialization due to DPI & font scaling)
            RateOptionsTable.AutoSize = false;

            KeyNodeCheckBox.Checked = _nodeData.KeyNode;
            KeyNodeTitleLabel.Visible = _nodeData.KeyNode;
            KeyNodeTitleInput.Visible = _nodeData.KeyNode;
            KeyNodeTitleInput.Text = _nodeData.KeyNodeTitle;

            LowPriorityCheckBox.Checked = _nodeData.LowPriority;

            FixedAssemblerInput.Maximum = (decimal) node.MaxDesiredSetValue;

            foreach (var quality in graphViewer.DCache.AvailableQualities.Where(q => q.Enabled)) {
                QualitySelector.Items.Add(quality.FriendlyName);
                _qualitySelectorIndexSet.Add(quality);
            }

            if (QualitySelector.Items.Count == 1)
                QualitySelector.Enabled = false;
            var defQuality = graphViewer.Graph.DefaultAssemblerQuality;
            QualitySelector.SelectedIndex = _qualitySelectorIndexSet.IndexOf(defQuality) != -1 ? _qualitySelectorIndexSet.IndexOf(defQuality) : 0;

            if (_nodeData.BeaconCount % 1 != 0) BeaconCountInput.DecimalPlaces = 1;
            BeaconCountInput.Value = Math.Min(BeaconCountInput.Maximum, (decimal) _nodeData.BeaconCount);
            BeaconsPerAssemblerInput.Value = Math.Min(BeaconsPerAssemblerInput.Maximum, (decimal) _nodeData.BeaconsPerAssembler);
            ConstantBeaconInput.Value = Math.Min(ConstantBeaconInput.Maximum, (decimal) _nodeData.BeaconsConst);
            NeighbourInput.Value = Math.Min(NeighbourInput.Maximum, (decimal) _nodeData.NeighbourCount);
            ExtraProductivityInput.Value = Math.Min(ExtraProductivityInput.Maximum, (decimal) (_nodeData.ExtraProductivity * 100));

            _assemblerOptions = [];
            _fuelOptions = [];
            _assemblerModules = [];
            _aModuleOptions = [];
            _beaconOptions = [];
            _beaconModules = [];
            _bModuleOptions = [];

            // setup scrolling

            _lastScrollY = new Dictionary<object, int> {
                { AssemblerChoicePanel, 0 },
                { FuelOptionsPanel, 0 },
                { SelectedAModulesPanel, 0 },
                { AModulesChoicePanel, 0 },
                { BeaconChoicePanel, 0 },
                { SelectedBModulesPanel, 0 },
                { BModulesChoicePanel, 0 }
            };

            AssemblerChoicePanel.MouseWheel += OptionsPanel_MouseWheel;
            FuelOptionsPanel.MouseWheel += OptionsPanel_MouseWheel;
            SelectedAModulesPanel.MouseWheel += OptionsPanel_MouseWheel;
            AModulesChoicePanel.MouseWheel += OptionsPanel_MouseWheel;
            BeaconChoicePanel.MouseWheel += OptionsPanel_MouseWheel;
            SelectedBModulesPanel.MouseWheel += OptionsPanel_MouseWheel;
            BModulesChoicePanel.MouseWheel += OptionsPanel_MouseWheel;

            UpdateRowHeights(AssemblerChoiceTable);
            UpdateRowHeights(FuelOptionsTable);
            UpdateRowHeights(SelectedAModulesTable);
            UpdateRowHeights(AModulesChoiceTable);
            UpdateRowHeights(BeaconChoiceTable);
            UpdateRowHeights(SelectedBModulesTable);
            UpdateRowHeights(BModulesChoiceTable);

            InitializeRates();
            SetupAssemblerOptions();

            // set these event handlers last - after we have set up all the values / settings

            LowPriorityCheckBox.CheckedChanged += LowPriorityCheckBox_CheckedChanged;
            KeyNodeCheckBox.CheckedChanged += KeyNodeCheckBox_CheckedChanged;
            KeyNodeTitleInput.TextChanged += KeyNodeTitleInput_TextChanged;

            FixedAssemblersOption.CheckedChanged += FixedAssemblerOption_CheckedChanged;
            FixedAssemblerInput.ValueChanged += FixedAssemblerInput_ValueChanged;
            NeighbourInput.ValueChanged += NeighbourInput_ValueChanged;
            ExtraProductivityInput.ValueChanged += ExtraProductivityInput_ValueChanged;
            BeaconCountInput.ValueChanged += BeaconInput_ValueChanged;
            BeaconsPerAssemblerInput.ValueChanged += BeaconInput_ValueChanged;
            ConstantBeaconInput.ValueChanged += BeaconInput_ValueChanged;

            QualitySelector.SelectedIndexChanged += QualitySelector_SelectedIndexChanged;
        }

        // had to set up this slightly convoluted scrolling option to account for mouse wheel events being WAY too fast
        // -> it would skip from start to end in a single tick, potentially missing out several lines worth of items.
        private void OptionsPanel_MouseWheel(object sender, MouseEventArgs e) {
            var panel = sender as Panel;

            if (e.Delta < 0 && _lastScrollY[sender] < panel.Controls[0].Height - panel.Height + 5)
                _lastScrollY[sender] += panel.Height / 4;
            else if (e.Delta > 0 && _lastScrollY[sender] > 0)
                _lastScrollY[sender] -= panel.Height / 4;
            panel.AutoScrollPosition = new Point(0, _lastScrollY[sender]);
        }

        private void InitializeRates() {
            if (_nodeData.RateType == RateType.Auto) {
                AutoAssemblersOption.Checked = true;
                FixedAssemblerInput.Enabled = false;
                FixedAssemblerInput.Value = Math.Min(FixedAssemblerInput.Maximum, (decimal) _nodeData.ActualSetValue);
            } else {
                FixedAssemblersOption.Checked = true;
                FixedAssemblerInput.Enabled = true;
                FixedAssemblerInput.Value = Math.Min(FixedAssemblerInput.Maximum, (decimal) _nodeData.DesiredSetValue);
            }

            UpdateFixedFlowInputDecimals(FixedAssemblerInput);
        }

        private void SetupAssemblerOptions() {
            CleanTable(AssemblerChoiceTable, _nodeData.BaseRecipe.Recipe.Assemblers.Count(a => a.Enabled));

            _assemblerOptions.Clear();
            foreach (var assembler in _nodeData.BaseRecipe.Recipe.Assemblers.Where(a => a.Enabled)) {
                var button = InitializeBaseButton(assembler, _qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                button.Click += AssemblerButton_Click;

                AssemblerChoiceTable.Controls.Add(button, _assemblerOptions.Count % (AssemblerChoiceTable.ColumnCount - 1),
                    _assemblerOptions.Count / (AssemblerChoiceTable.ColumnCount - 1));
                _assemblerOptions.Add(button);
            }

            UpdateAssembler();
        }

        private void UpdateAssembler() {
            // assembler button colors

            foreach (var button in _assemblerOptions) {
                button.BackColor =
                    (Assembler) button.Tag == _nodeData.SelectedAssembler.Assembler &&
                    _qualitySelectorIndexSet[QualitySelector.SelectedIndex] == _nodeData.SelectedAssembler.Quality ? SelectedColor :
                    ((Assembler) button.Tag).IsMissing || !((Assembler) button.Tag).Available ? ErrorColor : AssemblerChoiceTable.BackColor;
            }

            // neighbour count panel

            if (_nodeData.SelectedAssembler.Assembler.EntityType != EntityType.Reactor) {
                NeighbourInput.Visible = false;
                NeighboursLabel.Visible = false;
            }

            // extra productivity bonus panel

            if (!_nodeData.BaseRecipe.Recipe.HasProductivityResearch && _nodeData.SelectedAssembler.Assembler.EntityType != EntityType.Miner &&
                !_myGraphViewer.Graph.EnableExtraProductivityForNonMiners) {
                ExtraProductivityInput.Visible = false;
                ExtraProductivityLabel.Visible = false;
            }

            // fuel panel

            FuelTitle.Visible = _nodeData.SelectedAssembler.Assembler.IsBurner;
            SelectedFuelIcon.Visible = _nodeData.SelectedAssembler.Assembler.IsBurner;
            FuelOptionsPanel.Visible = _nodeData.SelectedAssembler.Assembler.IsBurner;
            SetupFuelOptions();

            // modules panel

            var moduleOptions = GetAssemblerModuleOptions();
            var showModules = _nodeData.SelectedAssembler.Assembler.ModuleSlots > 0 && moduleOptions.Count > 0;
            AModulesLabel.Visible = showModules;
            AModuleOptionsLabel.Visible = showModules;
            SelectedAModulesPanel.Visible = showModules;
            AModulesChoicePanel.Visible = showModules;
            SetupAssemblerModuleOptions();

            // beacon panel

            SetupBeaconOptions();
            BeaconTable.Visible = _beaconOptions.Count != 0;
        }

        private void SetupFuelOptions() {
            var fuels = _nodeData.SelectedAssembler.Assembler.Fuels
                .Where(f => f.ProductionRecipes.Any(r => r.Enabled && r.Assemblers.Any(a => a.Enabled))).ToList();

            CleanTable(FuelOptionsTable, fuels.Count);
            FuelOptionsPanel.Height = (int) (FuelOptionsTable.RowStyles[0].Height * (fuels.Count <= 13 ? 1.2 : 2.2));

            _fuelOptions.Clear();
            foreach (var fuel in fuels) {
                var button = InitializeBaseButton(fuel, _myGraphViewer.DCache.DefaultQuality);
                button.Click += FuelButton_Click;

                FuelOptionsTable.Controls.Add(button, _fuelOptions.Count % (FuelOptionsTable.ColumnCount - 1),
                    _fuelOptions.Count / (FuelOptionsTable.ColumnCount - 1));
                _fuelOptions.Add(button);
            }

            UpdateFuel();
        }

        private void UpdateFuel() {
            foreach (var button in _fuelOptions) {
                button.BackColor = (Item) button.Tag == _nodeData.Fuel ? SelectedColor :
                    ((Item) button.Tag).IsMissing || !((Item) button.Tag).Available ||
                    !((Item) button.Tag).ProductionRecipes.Any(r => r.Available && r.Assemblers.Any(a => a.Available)) ? ErrorColor :
                    FuelOptionsTable.BackColor;
            }

            FuelTitle.Text = $"Fuel: {(_nodeData.Fuel == null ? "-none-" : _nodeData.Fuel.FriendlyName)}";
            SelectedFuelIcon.Image = _nodeData.Fuel?.Icon;

            UpdateAssemblerInfo();
        }

        private void SetupAssemblerModuleOptions() {
            var moduleOptions = GetAssemblerModuleOptions();

            CleanTable(AModulesChoiceTable, moduleOptions.Count);
            _aModuleOptions.Clear();
            foreach (var module in moduleOptions) {
                var button = InitializeBaseButton(module, _qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                if (!module.Available)
                    button.BackColor = ErrorColor;

                button.MouseUp += AModuleOptionButton_Click;

                AModulesChoiceTable.Controls.Add(button, _aModuleOptions.Count % (AModulesChoiceTable.ColumnCount - 1),
                    _aModuleOptions.Count / (AModulesChoiceTable.ColumnCount - 1));
                _aModuleOptions.Add(button);
            }

            UpdateAssemblerModules();
        }

        private void UpdateAssemblerModules() {
            foreach (var button in _aModuleOptions)
                button.Enabled = _nodeData.AssemblerModules.Count < _nodeData.SelectedAssembler.Assembler.ModuleSlots;

            var moduleOptions = _nodeData.BaseRecipe.Recipe.AssemblerModules
                .Intersect(_nodeData.SelectedAssembler.Assembler.Modules)
                .OrderBy(m => m.LFriendlyName)
                .ToList();

            CleanTable(SelectedAModulesTable, _nodeData.AssemblerModules.Count);

            _assemblerModules.Clear();
            for (var i = 0; i < _nodeData.AssemblerModules.Count; i++) {
                var button = InitializeBaseButton(_nodeData.AssemblerModules[i].Module, _nodeData.AssemblerModules[i].Quality);
                if (_nodeData.AssemblerModules[i].Module.IsMissing || !_nodeData.AssemblerModules[i].Module.Available ||
                    !_nodeData.AssemblerModules[i].Module.Enabled || !moduleOptions.Contains(_nodeData.AssemblerModules[i].Module) ||
                    i >= _nodeData.SelectedAssembler.Assembler.ModuleSlots)
                    button.BackColor = ErrorColor;
                button.MouseUp += AModuleButton_Click;

                SelectedAModulesTable.Controls.Add(button, _assemblerModules.Count % (SelectedAModulesTable.ColumnCount - 1),
                    _assemblerModules.Count / (SelectedAModulesTable.ColumnCount - 1));
                _assemblerModules.Add(button);
            }

            AModulesLabel.Text = $"Modules ({_nodeData.AssemblerModules.Count}/{_nodeData.SelectedAssembler.Assembler.ModuleSlots}):";
            UpdateAssemblerInfo();
        }

        private void SetupBeaconOptions() {
            var moduleOptions = _nodeData.BaseRecipe.Recipe.BeaconModules.ToList();

            CleanTable(BeaconChoiceTable, _myGraphViewer.DCache.Beacons.Values.Count(b => b.Enabled));

            _beaconOptions.Clear();
            if (_nodeData.SelectedAssembler.Assembler.AllowBeacons) {
                foreach (var beacon in _myGraphViewer.DCache.Beacons.Values.Where(b => b.Enabled)) {
                    if (!moduleOptions.Any(m => beacon.Modules.Contains(m)))
                        continue;

                    var button = InitializeBaseButton(beacon, _qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                    button.Click += BeaconButton_Click;

                    BeaconChoiceTable.Controls.Add(button, _beaconOptions.Count % (BeaconChoiceTable.ColumnCount - 1),
                        _beaconOptions.Count / (BeaconChoiceTable.ColumnCount - 1));
                    _beaconOptions.Add(button);
                }
            }

            UpdateBeacon();
        }

        private void UpdateBeacon() {
            foreach (var button in _beaconOptions) {
                button.BackColor =
                    (Beacon) button.Tag == _nodeData.SelectedBeacon.Beacon &&
                    _qualitySelectorIndexSet[QualitySelector.SelectedIndex] == _nodeData.SelectedBeacon.Quality ? SelectedColor :
                    ((Beacon) button.Tag).IsMissing || !((Beacon) button.Tag).Available ? ErrorColor : BeaconChoiceTable.BackColor;
            }

            // modules panel

            var moduleOptions = GetBeaconModuleOptions();
            var showModules = _nodeData.SelectedBeacon && _nodeData.SelectedBeacon.Beacon.ModuleSlots > 0 && moduleOptions.Count > 0;

            BeaconValuesTable.Visible = _nodeData.SelectedBeacon;
            BeaconInfoTable.Visible = _nodeData.SelectedBeacon;

            BModulesLabel.Visible = showModules;
            BModuleOptionsLabel.Visible = showModules;
            SelectedBModulesPanel.Visible = showModules;
            BModulesChoicePanel.Visible = showModules;
            SetupBeaconModuleOptions();

            // beacon values

            if (_nodeData.SelectedBeacon)
                SetBeaconValues(true);
        }

        private void SetupBeaconModuleOptions() {
            var moduleOptions = GetBeaconModuleOptions();
            var moduleSlots = _nodeData.SelectedBeacon ? _nodeData.SelectedBeacon.Beacon.ModuleSlots : 0;

            CleanTable(BModulesChoiceTable, moduleOptions.Count);
            _bModuleOptions.Clear();
            foreach (var module in moduleOptions) {
                var button = InitializeBaseButton(module, _qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                if (!module.Available)
                    button.BackColor = ErrorColor;

                button.MouseUp += BModuleOptionButton_Click;

                BModulesChoiceTable.Controls.Add(button, _bModuleOptions.Count % (BModulesChoiceTable.ColumnCount - 1),
                    _bModuleOptions.Count / (BModulesChoiceTable.ColumnCount - 1));
                _bModuleOptions.Add(button);
            }

            UpdateBeaconModules();
        }

        private void UpdateBeaconModules() {
            foreach (var button in _bModuleOptions)
                button.Enabled = _nodeData.BeaconModules.Count < _nodeData.SelectedBeacon.Beacon.ModuleSlots;

            var moduleOptions = GetBeaconModuleOptions();
            var moduleSlots = _nodeData.SelectedBeacon ? _nodeData.SelectedBeacon.Beacon.ModuleSlots : 0;

            CleanTable(SelectedBModulesTable, _nodeData.BeaconModules.Count);

            _beaconModules.Clear();
            for (var i = 0; i < _nodeData.BeaconModules.Count; i++) {
                var button = InitializeBaseButton(_nodeData.BeaconModules[i].Module, _nodeData.BeaconModules[i].Quality);
                if (_nodeData.BeaconModules[i].Module.IsMissing || !_nodeData.BeaconModules[i].Module.Available || !_nodeData.BeaconModules[i].Module.Enabled ||
                    !moduleOptions.Contains(_nodeData.BeaconModules[i].Module) || i >= moduleSlots)
                    button.BackColor = ErrorColor;
                button.MouseUp += BModuleButton_Click;

                SelectedBModulesTable.Controls.Add(button, _beaconModules.Count % (SelectedBModulesTable.ColumnCount - 1),
                    _beaconModules.Count / (SelectedBModulesTable.ColumnCount - 1));
                _beaconModules.Add(button);
            }

            BModulesLabel.Text = $"Modules ({_nodeData.BeaconModules.Count}/{moduleSlots}):";

            UpdateBeaconInfo();
            UpdateAssemblerInfo(); //for the impact of the beacon
        }

        private void UpdateAssemblerInfo() {
            AssemblerRateLabel.Text = $"# of {_nodeData.SelectedAssembler.Assembler.GetEntityTypeName(true)}:";
            AssemblerTitle.Text = $"{_nodeData.SelectedAssembler.Assembler.GetEntityTypeName(false)}: {_nodeData.SelectedAssembler.Assembler.FriendlyName}";
            SelectedAssemblerIcon.Image = _nodeData.SelectedAssembler.Icon;

            AssemblerEnergyPercentLabel.Text = _nodeData.GetConsumptionMultiplier().ToString("P0");
            AssemblerSpeedPercentLabel.Text = _nodeData.GetSpeedMultiplier().ToString("P0");
            AssemblerProductivityPercentLabel.Text = _nodeData.GetProductivityMultiplier().ToString("P0");
            AssemblerPollutionPercentLabel.Text = _nodeData.GetPollutionMultiplier().ToString("P0");
            AssemblerQualityPercentLabel.Text = _nodeData.GetQualityMultiplier().ToString("P0");

            var isAssembler = _nodeData.SelectedAssembler.Assembler.EntityType is EntityType.Assembler or EntityType.Miner or EntityType.OffshorePump;
            AssemblerSpeedTitleLabel.Visible = isAssembler;
            AssemblerSpeedLabel.Visible = isAssembler;
            AssemblerSpeedPercentLabel.Visible = isAssembler;
            AssemblerProductivityTitleLabel.Visible = isAssembler;
            AssemblerProductivityPercentLabel.Visible = isAssembler;
            AssemblerPollutionTitleLabel.Visible = isAssembler;
            AssemblerPollutionPercentLabel.Visible = isAssembler;
            AssemblerQualityTitleLabel.Visible = isAssembler;
            AssemblerQualityPercentLabel.Visible = isAssembler;

            var isGenerator = _nodeData.SelectedAssembler.Assembler.EntityType == EntityType.Generator;
            GeneratorTemperatureLabel.Visible = isGenerator;
            GeneratorTemperatureRangeLabel.Visible = isGenerator;

            AssemblerSpeedLabel.Text =
                $"{_nodeData.GetAssemblerSpeed().ToString("0.##")} ({(_nodeData.GetTotalCrafts() < 1 ? _nodeData.GetTotalCrafts().ToString("0.####") : _nodeData.GetTotalCrafts().ToString("0.#"))} crafts / {RateName})";

            if (_nodeData.SelectedAssembler.Assembler.IsBurner && _nodeData.Fuel != null)
                AssemblerEnergyLabel.Text =
                    $"{GraphicsStuff.DoubleToEnergy(_nodeData.GetAssemblerEnergyConsumption(), "W")} ({GraphicsStuff.DoubleToString(_nodeData.GetTotalAssemblerFuelConsumption())} fuel / {RateName})";
            else
                AssemblerEnergyLabel.Text = GraphicsStuff.DoubleToEnergy(_nodeData.GetAssemblerEnergyConsumption(), "W");

            AssemblerPollutionLabel.Text = $"{(_nodeData.GetAssemblerPollutionProduction() * 60).ToString("0.##")} / min";

            if (!isGenerator)
                return;

            var minTemp = _nodeData.GetGeneratorMinimumTemperature();
            var maxTemp = _nodeData.GetGeneratorMaximumTemperature();
            var operationalTemp = _nodeData.SelectedAssembler.Assembler.OperationTemperature;
            var effectivity = _nodeData.GetGeneratorEffectivity();

            GeneratorTemperatureRangeLabel.Text = double.IsInfinity(maxTemp)
                ? $"min {Math.Round(minTemp, 1):0.#}°c  (optimal: {Math.Round(operationalTemp, 1):0.#}°c)"
                : $"{Math.Round(minTemp, 1):0.#}-{Math.Round(maxTemp, 1):0.#}°c  (optimal: {Math.Round(operationalTemp, 1):0.#}°c)";

            AssemblerEnergyLabel.Text = GraphicsStuff.DoubleToEnergy(_nodeData.GetGeneratorElectricalProduction(), "W");
            AssemblerEnergyPercentLabel.Text = effectivity.ToString("P0");
        }

        private void UpdateBeaconInfo() {
            BeaconTitle.Text = $"Beacon: {(_nodeData.SelectedBeacon ? _nodeData.SelectedBeacon.Beacon.FriendlyName : "-none-")}";
            SelectedBeaconIcon.Image = _nodeData.SelectedBeacon.Icon;

            BeaconEnergyLabel.Text = _nodeData.SelectedBeacon ? GraphicsStuff.DoubleToEnergy(_nodeData.GetBeaconEnergyConsumption(), "W") : "0J";
            BeaconModuleCountLabel.Text = _nodeData.SelectedBeacon ? _nodeData.SelectedBeacon.Beacon.ModuleSlots.ToString() : "0";
            BeaconEfficiencyLabel.Text = _nodeData.SelectedBeacon
                ? _nodeData.SelectedBeacon.Beacon.GetBeaconEffectivity(_nodeData.SelectedBeacon.Quality, _nodeData.BeaconCount).ToString("P0")
                : "0%";
            TotalBeaconsLabel.Text = _nodeData.GetTotalBeacons().ToString();
            TotalBeaconEnergyLabel.Text = _nodeData.SelectedBeacon ? GraphicsStuff.DoubleToEnergy(_nodeData.GetTotalBeaconElectricalConsumption(), "W") : "0J";
        }

        //------------------------------------------------------------------------------------------------------Helper functions

        private List<Module> GetAssemblerModuleOptions() {
            if (_nodeData.SelectedAssembler.Assembler.AllowModules)
                return _nodeData.BaseRecipe.Recipe.AssemblerModules.Intersect(_nodeData.SelectedAssembler.Assembler.Modules).Where(m => m.Enabled)
                    .OrderBy(m => m.LFriendlyName).ToList();
            return [];
        }

        private List<Module> GetBeaconModuleOptions() {
            if (_nodeData.SelectedAssembler.Assembler.AllowBeacons && _nodeData.SelectedBeacon)
                return _nodeData.BaseRecipe.Recipe.BeaconModules.Intersect(_nodeData.SelectedBeacon.Beacon.Modules).Where(m => m.Enabled)
                    .OrderBy(m => m.LFriendlyName).ToList();
            return [];
        }

        private Button InitializeBaseButton(DataObjectBase obj, Quality quality) {
            var button = new NfButton();
            //button.BackColor = RecipeNode.SelectedAssembler == assembler? Color.DarkOrange : assembler.Available? Color.Gray : Color.DarkRed;
            button.ForeColor = Color.Gray;
            button.BackgroundImageLayout = ImageLayout.Zoom;
            button.BackgroundImage = quality == _myGraphViewer.DCache.DefaultQuality
                ? obj.Icon
                : IconCacheProcessor.CombinedQualityIcon(obj.Icon, quality.Icon);
            button.UseVisualStyleBackColor = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.BorderColor = Color.Black;
            button.TabStop = false;
            button.Margin = new Padding(0);
            button.Size = new Size(1, 1);
            button.Dock = DockStyle.Fill;
            button.Tag = obj;
            button.Enabled = true;

            button.MouseHover += Button_MouseHover;
            button.MouseLeave += Button_MouseLeave;
            return button;
        }

        private void CleanTable(TableLayoutPanel table, int newCellCount) {
            while (table.Controls.Count > 0)
                table.Controls[0].Dispose();
            while (table.RowStyles.Count > 1)
                table.RowStyles.RemoveAt(0);
            for (var i = 0; i < (newCellCount - 1) / (table.ColumnCount - 1); i++)
                table.RowStyles.Add(new RowStyle(table.RowStyles[0].SizeType, table.RowStyles[0].Height));
            table.RowCount = table.RowStyles.Count;
        }

        private void UpdateRowHeights(TableLayoutPanel table) {
            var height = (table.Width - (table.RowStyles.Count > 2 ? 20 : 0)) / (table.ColumnCount - 1);
            for (var i = 0; i < table.RowStyles.Count; i++)
                table.RowStyles[i].Height = height;
        }

        private void UpdateFixedFlowInputDecimals(NumericUpDown nud, int max = 4) {
            var decimals = MathDecimals.GetDecimals(nud.Value);
            decimals = Math.Min(decimals, max);
            nud.DecimalPlaces = decimals;
        }

        //------------------------------------------------------------------------------------------------------Button clicks

        private void AssemblerButton_Click(object sender, EventArgs e) {
            var newAssembler = ((Button) sender).Tag as Assembler;
            var quality = _qualitySelectorIndexSet[QualitySelector.SelectedIndex];
            _nodeController.SetAssembler(new AssemblerQualityPair(newAssembler, quality));
            _myGraphViewer.Graph.UpdateNodeValues();
            UpdateAssembler();
        }

        private void FuelButton_Click(object sender, EventArgs e) {
            var newFuel = ((Button) sender).Tag as Item;
            _nodeController.SetFuel(newFuel);
            _myGraphViewer.Graph.UpdateNodeValues();
            UpdateFuel();
        }

        private void AModuleButton_Click(object sender, MouseEventArgs e) {
            if (!new Rectangle(new Point(0, 0), ((Button) sender).Size).Contains(e.Location))
                return;

            ToolTip.Hide((Control) sender);
            var index = _assemblerModules.IndexOf((Button) sender);

            switch (e.Button) {
                case MouseButtons.Left:
                    _nodeController.RemoveAssemblerModule(index);
                    break;
                case MouseButtons.Right:
                    _nodeController.RemoveAssemblerModules(_nodeData.AssemblerModules[index]);
                    break;
                default:
                    return;
            }

            _myGraphViewer.Graph.UpdateNodeValues();
            UpdateAssemblerModules();
        }

        private void AModuleOptionButton_Click(object sender, MouseEventArgs e) {
            if (!new Rectangle(new Point(0, 0), ((Button) sender).Size).Contains(e.Location))
                return;

            var newModule = ((Button) sender).Tag as Module;
            var quality = _qualitySelectorIndexSet[QualitySelector.SelectedIndex];

            switch (e.Button) {
                case MouseButtons.Left:
                    _nodeController.AddAssemblerModule(new ModuleQualityPair(newModule, quality));
                    break;
                case MouseButtons.Right:
                    _nodeController.AddAssemblerModules(new ModuleQualityPair(newModule, quality));
                    break;
                default:
                    return;
            }

            _myGraphViewer.Graph.UpdateNodeValues();
            UpdateAssemblerModules();
        }

        private void BeaconButton_Click(object sender, EventArgs e) {
            var newBeacon = ((Button) sender).Tag as Beacon;
            var quality = _qualitySelectorIndexSet[QualitySelector.SelectedIndex];
            var newBeaconQp = new BeaconQualityPair(newBeacon, quality);

            if (_nodeData.SelectedBeacon == newBeaconQp)
                _nodeController.ClearBeacon();
            else
                _nodeController.SetBeacon(newBeaconQp);
            _myGraphViewer.Graph.UpdateNodeValues();
            UpdateBeacon();
        }

        private void BModuleButton_Click(object sender, MouseEventArgs e) {
            if (!new Rectangle(new Point(0, 0), ((Button) sender).Size).Contains(e.Location))
                return;

            ToolTip.Hide((Control) sender);
            var index = _beaconModules.IndexOf((Button) sender);

            switch (e.Button) {
                case MouseButtons.Left:
                    _nodeController.RemoveBeaconModule(index);
                    break;
                case MouseButtons.Right:
                    _nodeController.RemoveBeaconModules(_nodeData.BeaconModules[index]);
                    break;
                default:
                    return;
            }

            _myGraphViewer.Graph.UpdateNodeValues();
            UpdateBeaconModules();
        }

        private void BModuleOptionButton_Click(object sender, MouseEventArgs e) {
            if (!new Rectangle(new Point(0, 0), ((Button) sender).Size).Contains(e.Location))
                return;

            var newModule = ((Button) sender).Tag as Module;
            var quality = _qualitySelectorIndexSet[QualitySelector.SelectedIndex];

            switch (e.Button) {
                case MouseButtons.Left:
                    _nodeController.AddBeaconModule(new ModuleQualityPair(newModule, quality));
                    break;
                case MouseButtons.Right:
                    _nodeController.AddBeaconModules(new ModuleQualityPair(newModule, quality));
                    break;
                default:
                    return;
            }

            _myGraphViewer.Graph.UpdateNodeValues();
            UpdateBeaconModules();
        }

        //------------------------------------------------------------------------------------------------------Button hovers

        private void Button_MouseHover(object sender, EventArgs e) {
            var control = (Control) sender;
            if (control.Tag is Item fuel) { // the only items in this panel are fuels
                ToolTip.SetText(fuel.FriendlyName + "\nFuel value: " + GraphicsStuff.DoubleToEnergy(fuel.FuelValue, "J"));
                ToolTip.Show(this, Point.Add(PointToClient(MousePosition), new Size(15, 5)));
            } else if (control.Tag is DataObjectBase dob) {
                ToolTip.SetText(dob.FriendlyName);
                ToolTip.Show(this, Point.Add(PointToClient(MousePosition), new Size(15, 5)));
            }
        }

        private void Button_MouseLeave(object sender, EventArgs e) {
            ToolTip.Hide((Control) sender);
        }

        //------------------------------------------------------------------------------------------------------Priority Checkbox
        private void LowPriorityCheckBox_CheckedChanged(object sender, EventArgs e) {
            _nodeController.SetPriority(LowPriorityCheckBox.Checked);
            _myGraphViewer.Graph.UpdateNodeValues();
        }

        //------------------------------------------------------------------------------------------------------Rate input & key node events

        private void SetFixedRate() {
            if (Math.Abs(_nodeData.DesiredSetValue - (double) FixedAssemblerInput.Value) > double.Epsilon) {
                _nodeController.SetDesiredSetValue((double) FixedAssemblerInput.Value);
                _myGraphViewer.Graph.UpdateNodeValues();

                UpdateAssemblerInfo();
                UpdateBeaconInfo();
            }
        }

        private void FixedAssemblerOption_CheckedChanged(object sender, EventArgs e) {
            FixedAssemblerInput.Enabled = FixedAssemblersOption.Checked;
            var updatedRateType = FixedAssemblersOption.Checked ? RateType.Manual : RateType.Auto;

            if (_nodeData.RateType != updatedRateType) {
                _nodeController.SetRateType(updatedRateType);
                _nodeController.SetDesiredSetValue((double) FixedAssemblerInput.Value);
                _myGraphViewer.Graph.UpdateNodeValues();

                UpdateAssemblerInfo();
                UpdateBeaconInfo();
            }
        }

        private void FixedAssemblerInput_ValueChanged(object sender, EventArgs e) {
            SetFixedRate();
            UpdateFixedFlowInputDecimals(sender as NumericUpDown, 2);
        }

        private void KeyNodeCheckBox_CheckedChanged(object sender, EventArgs e) {
            _nodeController.SetKeyNode(KeyNodeCheckBox.Checked);
            KeyNodeTitleLabel.Visible = _nodeData.KeyNode;
            KeyNodeTitleInput.Visible = _nodeData.KeyNode;
            KeyNodeTitleInput.Text = _nodeData.KeyNodeTitle;
            _myGraphViewer.Invalidate();
        }

        private void KeyNodeTitleInput_TextChanged(object sender, EventArgs e) {
            _nodeController.SetKeyNodeTitle(KeyNodeTitleInput.Text);
        }

        //------------------------------------------------------------------------------------------------------assembler neighbour bonus input events

        private void SetNeighbourBonus() {
            if (Math.Abs(_nodeData.NeighbourCount - (double) NeighbourInput.Value) > double.Epsilon) {
                _nodeController.SetNeighbourCount((double) NeighbourInput.Value);
                _myGraphViewer.Graph.UpdateNodeValues();

                UpdateAssemblerInfo();
            }
        }

        private void NeighbourInput_ValueChanged(object sender, EventArgs e) {
            SetNeighbourBonus();
            UpdateFixedFlowInputDecimals(sender as NumericUpDown, 2);
        }

        //------------------------------------------------------------------------------------------------------assembler extra productivity input events

        private void SetExtraProductivityBonus() {
            if (Math.Abs(_nodeData.ExtraProductivity - (double) ExtraProductivityInput.Value / 100) > double.Epsilon) {
                _nodeController.SetExtraProductivityBonus((double) ExtraProductivityInput.Value / 100);
                _myGraphViewer.Graph.UpdateNodeValues();

                UpdateAssemblerInfo();
            }
        }

        private void ExtraProductivityInput_ValueChanged(object sender, EventArgs e) {
            SetExtraProductivityBonus();
        }

        //------------------------------------------------------------------------------------------------------beacon input events

        private void SetBeaconValues(bool graphUpdateRequired) {
            if (Math.Abs(_nodeData.BeaconCount - (double) BeaconCountInput.Value) > double.Epsilon
                || Math.Abs(_nodeData.BeaconsPerAssembler - (double) BeaconsPerAssemblerInput.Value) > double.Epsilon
                || Math.Abs(_nodeData.BeaconsConst - (double) ConstantBeaconInput.Value) > double.Epsilon) {
                _nodeController.SetBeaconCount((double) BeaconCountInput.Value);
                _nodeController.SetBeaconsPerAssembler((double) BeaconsPerAssemblerInput.Value);
                _nodeController.SetBeaconsCont((double) ConstantBeaconInput.Value);

                // only graph update worthy change is the # of beacons. the others aren't as important

                if (graphUpdateRequired)
                    _myGraphViewer.Graph.UpdateNodeValues();

                UpdateAssemblerInfo();
                UpdateBeaconInfo();
            }
        }

        private void BeaconInput_ValueChanged(object sender, EventArgs e) {
            SetBeaconValues(sender == BeaconCountInput);
            UpdateFixedFlowInputDecimals(sender as NumericUpDown, 2);
        }

        private void QualitySelector_SelectedIndexChanged(object sender, EventArgs e) {
            SetupAssemblerOptions();
            SetupAssemblerModuleOptions();
            SetupBeaconOptions();
            SetupBeaconModuleOptions();

            _myGraphViewer.Graph.DefaultAssemblerQuality = _qualitySelectorIndexSet[QualitySelector.SelectedIndex];
        }
    }
}