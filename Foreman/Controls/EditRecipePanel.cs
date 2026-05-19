using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public partial class EditRecipePanel : UserControl {
        private static readonly Color ErrorColor = Color.DarkRed;
        private static readonly Color SelectedColor = Color.DarkOrange;

        private readonly List<Button> AssemblerOptions;
        private readonly List<Button> FuelOptions;
        private readonly List<Button> AssemblerModules;
        private readonly List<Button> AModuleOptions;
        private readonly List<Button> BeaconOptions;
        private readonly List<Button> BeaconModules;
        private readonly List<Button> BModuleOptions;

        private readonly Dictionary<object, int> LastScrollY;

        private readonly ProductionGraphViewer myGraphViewer;
        private readonly DataCache panelCache;
        private readonly RecipeNodeController nodeController;
        private readonly IRecipeNodeViewModel nodeData;

        private double RateMultiplier { get { return myGraphViewer.Graph.GetRateMultipler(); } }
        private string RateName { get { return myGraphViewer.Graph.GetRateName(); } }

        private readonly List<IQuality> qualitySelectorIndexSet;

        public EditRecipePanel(IRecipeNodeViewModel node, ProductionGraphViewer graphViewer) {
            nodeData = node;
            if (graphViewer.Session.Editor.RequestNodeController(node.Id) is not RecipeNodeController controller)
                throw new InvalidOperationException("Recipe node has no controller.");
            nodeController = controller;
            myGraphViewer = graphViewer;
            panelCache = graphViewer.DCache ?? throw new InvalidOperationException("Data cache is not loaded.");
            qualitySelectorIndexSet = [];

            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            RateOptionsTable.AutoSize = false; //simplest way of ensuring the width of the panel remains constant (it needs to be autosized during initialization due to DPI & font scaling)

            KeyNodeCheckBox.Checked = nodeData.KeyNode;
            KeyNodeTitleLabel.Visible = nodeData.KeyNode;
            KeyNodeTitleInput.Visible = nodeData.KeyNode;
            KeyNodeTitleInput.Text = nodeData.KeyNodeTitle;

            LowPriorityCheckBox.Checked = nodeData.LowPriority;

            FixedAssemblerInput.Maximum = (decimal)(node.MaxDesiredSetValue);

            foreach (IQuality quality in panelCache.AvailableQualities.Where(q => q.Enabled)) {
                QualitySelector.Items.Add(quality.FriendlyName);
                qualitySelectorIndexSet.Add(quality);
            }

            if (QualitySelector.Items.Count == 1)
                QualitySelector.Enabled = false;
            var defQuality = graphViewer.Graph.DefaultAssemblerQuality;
            QualitySelector.SelectedIndex = (defQuality is null || !qualitySelectorIndexSet.Contains(defQuality)) ? 0 : qualitySelectorIndexSet.IndexOf(defQuality);

            if (nodeData.BeaconCount % 1 != 0)
                BeaconCountInput.DecimalPlaces = 1;
            BeaconCountInput.Value = Math.Min(BeaconCountInput.Maximum, (decimal)nodeData.BeaconCount);
            BeaconsPerAssemblerInput.Value = Math.Min(BeaconsPerAssemblerInput.Maximum, (decimal)nodeData.BeaconsPerAssembler);
            ConstantBeaconInput.Value = Math.Min(ConstantBeaconInput.Maximum, (decimal)nodeData.BeaconsConst);
            NeighbourInput.Value = Math.Min(NeighbourInput.Maximum, (decimal)nodeData.NeighbourCount);
            ExtraProductivityInput.Value = Math.Min(ExtraProductivityInput.Maximum, (decimal)(nodeData.ExtraProductivity * 100));

            AssemblerOptions = [];
            FuelOptions = [];
            AssemblerModules = [];
            AModuleOptions = [];
            BeaconOptions = [];
            BeaconModules = [];
            BModuleOptions = [];

            //setup scrolling
            LastScrollY = new Dictionary<object, int> {
                { AssemblerChoicePanel, 0 },
                { FuelOptionsPanel, 0 },
                { SelectedAModulesPanel, 0 },
                { AModulesChoicePanel, 0 },
                { BeaconChoicePanel, 0 },
                { SelectedBModulesPanel, 0 },
                { BModulesChoicePanel, 0 }
            };
            AssemblerChoicePanel.MouseWheel += new MouseEventHandler(OptionsPanel_MouseWheel);
            FuelOptionsPanel.MouseWheel += new MouseEventHandler(OptionsPanel_MouseWheel);
            SelectedAModulesPanel.MouseWheel += new MouseEventHandler(OptionsPanel_MouseWheel);
            AModulesChoicePanel.MouseWheel += new MouseEventHandler(OptionsPanel_MouseWheel);
            BeaconChoicePanel.MouseWheel += new MouseEventHandler(OptionsPanel_MouseWheel);
            SelectedBModulesPanel.MouseWheel += new MouseEventHandler(OptionsPanel_MouseWheel);
            BModulesChoicePanel.MouseWheel += new MouseEventHandler(OptionsPanel_MouseWheel);

            UpdateRowHeights(AssemblerChoiceTable);
            UpdateRowHeights(FuelOptionsTable);
            UpdateRowHeights(SelectedAModulesTable);
            UpdateRowHeights(AModulesChoiceTable);
            UpdateRowHeights(BeaconChoiceTable);
            UpdateRowHeights(SelectedBModulesTable);
            UpdateRowHeights(BModulesChoiceTable);

            InitializeRates();
            SetupAssemblerOptions();

            //set these event handlers last - after we have set up all the values / settings
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

            ApplyViewportBounds();
        }

        private void OptionsPanel_MouseWheel(object? sender, MouseEventArgs e) {
            //had to set up this slightly convoluted scrolling option to account for mouse wheel events being WAY too fast -> it would skip from start to end in a single tick, potentially missing out several lines worth of items.
            if (sender is not Panel sPanel)
                return;

            if (e.Delta < 0 && LastScrollY[sender] < sPanel.Controls[0].Height - sPanel.Height + 5)
                LastScrollY[sender] += sPanel.Height / 4;
            else if (e.Delta > 0 && LastScrollY[sender] > 0)
                LastScrollY[sender] -= sPanel.Height / 4;
            sPanel.AutoScrollPosition = new Point(0, LastScrollY[sender]);
        }

        private void InitializeRates() {
            if (nodeData.RateType == RateType.Auto) {
                AutoAssemblersOption.Checked = true;
                FixedAssemblerInput.Enabled = false;
                FixedAssemblerInput.Value = Math.Min(FixedAssemblerInput.Maximum, (decimal)nodeData.ActualSetValue);
            } else {
                FixedAssemblersOption.Checked = true;
                FixedAssemblerInput.Enabled = true;
                FixedAssemblerInput.Value = Math.Min(FixedAssemblerInput.Maximum, (decimal)nodeData.DesiredSetValue);
            }
            UpdateFixedFlowInputDecimals(FixedAssemblerInput);
        }

        private void SetupAssemblerOptions() {
            if (nodeData.BaseRecipe is not { Recipe: IRecipe baseRecipe })
                return;

            CleanTable(AssemblerChoiceTable, baseRecipe.Assemblers.Count(a => a.Enabled));

            AssemblerOptions.Clear();
            foreach (IAssembler assembler in baseRecipe.Assemblers.Where(a => a.Enabled)) {
                Button button = InitializeBaseButton(assembler, qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                button.Click += new EventHandler(AssemblerButton_Click);

                AssemblerChoiceTable.Controls.Add(button, AssemblerOptions.Count % (AssemblerChoiceTable.ColumnCount - 1), AssemblerOptions.Count / (AssemblerChoiceTable.ColumnCount - 1));
                AssemblerOptions.Add(button);
            }

            UpdateAssembler();
        }

        private void UpdateAssembler() {
            //assembler button colors
            foreach (Button abutton in AssemblerOptions)
                if (abutton.Tag is IAssembler asm)
                    abutton.BackColor = (asm == nodeData.SelectedAssembler.Assembler && qualitySelectorIndexSet[QualitySelector.SelectedIndex] == nodeData.SelectedAssembler.Quality) ? SelectedColor : (asm.IsMissing || !asm.Available) ? ErrorColor : AssemblerChoiceTable.BackColor;

            //neighbour count panel
            if (nodeData.SelectedAssembler.Assembler.EntityType != EntityType.Reactor) {
                NeighbourInput.Visible = false;
                NeighboursLabel.Visible = false;
            }

            //extra productivity bonus panel
            if (nodeData.BaseRecipe is { Recipe: IRecipe productivityRecipe } && !productivityRecipe.HasProductivityResearch && (nodeData.SelectedAssembler.Assembler.EntityType != EntityType.Miner && !myGraphViewer.Graph.EnableExtraProductivityForNonMiners)) {
                ExtraProductivityInput.Visible = false;
                ExtraProductivityLabel.Visible = false;
            }

            //fuel panel
            FuelTitle.Visible = nodeData.SelectedAssembler.Assembler.IsBurner;
            SelectedFuelIcon.Visible = nodeData.SelectedAssembler.Assembler.IsBurner;
            FuelOptionsPanel.Visible = nodeData.SelectedAssembler.Assembler.IsBurner;
            SetupFuelOptions();

            //modules panel
            List<IModule> moduleOptions = GetAssemblerModuleOptions();
            bool showModules = nodeData.SelectedAssembler.Assembler.ModuleSlots > 0 && moduleOptions.Count > 0;
            AModulesLabel.Visible = showModules;
            AModuleOptionsLabel.Visible = showModules;
            SelectedAModulesPanel.Visible = showModules;
            AModulesChoicePanel.Visible = showModules;
            SetupAssemblerModuleOptions();

            //beacon panel
            SetupBeaconOptions();
            BeaconTable.Visible = (BeaconOptions.Count != 0);
            RefreshViewportLayout();
        }

        private void SetupFuelOptions() {

            var fuels = nodeData.SelectedAssembler.Assembler.Fuels.Where(f => f.ProductionRecipes.Any(r => r.Enabled && r.Assemblers.Any(a => a.Enabled))).ToList();

            CleanTable(FuelOptionsTable, fuels.Count);
            FuelOptionsPanel.Height = (int)(FuelOptionsTable.RowStyles[0].Height * (fuels.Count <= 13 ? 1.2 : 2.2));

            FuelOptions.Clear();
            foreach (IItem fuel in fuels) {
                Button button = InitializeBaseButton(fuel, panelCache.DefaultQuality);
                button.Click += new EventHandler(FuelButton_Click);

                FuelOptionsTable.Controls.Add(button, FuelOptions.Count % (FuelOptionsTable.ColumnCount - 1), FuelOptions.Count / (FuelOptionsTable.ColumnCount - 1));
                FuelOptions.Add(button);
            }

            UpdateFuel();
        }

        private void UpdateFuel() {
            foreach (Button fbutton in FuelOptions)
                if (fbutton.Tag is IItem item)
                    fbutton.BackColor = (item == nodeData.Fuel) ? SelectedColor : (item.IsMissing || !item.Available || !item.ProductionRecipes.Any(r => r.Available && r.Assemblers.Any(a => a.Available))) ? ErrorColor : FuelOptionsTable.BackColor;

            FuelTitle.Text = string.Format(DisplayCulture.Format, "Fuel: {0}", nodeData.Fuel == null ? "-none-" : nodeData.Fuel.FriendlyName);
            SelectedFuelIcon.Image = nodeData.Fuel?.Icon;

            UpdateAssemblerInfo();
        }

        private void SetupAssemblerModuleOptions() {
            List<IModule> moduleOptions = GetAssemblerModuleOptions();

            CleanTable(AModulesChoiceTable, moduleOptions.Count);
            AModuleOptions.Clear();
            for (int i = 0; i < moduleOptions.Count; i++) {
                Button button = InitializeBaseButton(moduleOptions[i], qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                if (!moduleOptions[i].Available)
                    button.BackColor = ErrorColor;

                button.MouseUp += new MouseEventHandler(AModuleOptionButton_Click);

                AModulesChoiceTable.Controls.Add(button, AModuleOptions.Count % (AModulesChoiceTable.ColumnCount - 1), AModuleOptions.Count / (AModulesChoiceTable.ColumnCount - 1));
                AModuleOptions.Add(button);
            }

            UpdateAssemblerModules();
        }

        private void UpdateAssemblerModules() {
            foreach (Button mbutton in AModuleOptions)
                mbutton.Enabled = nodeData.AssemblerModules.Count < nodeData.SelectedAssembler.Assembler.ModuleSlots;

            List<IModule> moduleOptions = nodeData.BaseRecipe is { Recipe: IRecipe assemblerRecipe }
                ? [.. assemblerRecipe.AssemblerModules.Intersect(nodeData.SelectedAssembler.Assembler.Modules).OrderBy(m => m.LFriendlyName)]
                : [];

            CleanTable(SelectedAModulesTable, nodeData.AssemblerModules.Count);

            AssemblerModules.Clear();
            for (int i = 0; i < nodeData.AssemblerModules.Count; i++) {
                Button button = InitializeBaseButton(nodeData.AssemblerModules[i].Module, nodeData.AssemblerModules[i].Quality);
                if (nodeData.AssemblerModules[i].Module.IsMissing || !nodeData.AssemblerModules[i].Module.Available || !nodeData.AssemblerModules[i].Module.Enabled || !moduleOptions.Contains(nodeData.AssemblerModules[i].Module) || i >= nodeData.SelectedAssembler.Assembler.ModuleSlots)
                    button.BackColor = ErrorColor;
                button.MouseUp += new MouseEventHandler(AModuleButton_Click);

                SelectedAModulesTable.Controls.Add(button, AssemblerModules.Count % (SelectedAModulesTable.ColumnCount - 1), AssemblerModules.Count / (SelectedAModulesTable.ColumnCount - 1));
                AssemblerModules.Add(button);
            }

            AModulesLabel.Text = string.Format(DisplayCulture.Format, "Modules ({0}/{1}):", nodeData.AssemblerModules.Count, nodeData.SelectedAssembler.Assembler.ModuleSlots);
            UpdateAssemblerInfo();
        }

        private void SetupBeaconOptions() {
            if (nodeData.BaseRecipe is not { Recipe: IRecipe beaconHostRecipe })
                return;

            var moduleOptions = beaconHostRecipe.BeaconModules.ToList();

            CleanTable(BeaconChoiceTable, panelCache.Beacons.Values.Count(b => b.Enabled));

            BeaconOptions.Clear();
            if (nodeData.SelectedAssembler.Assembler.AllowBeacons) {
                foreach (IBeacon beacon in panelCache.Beacons.Values.Where(b => b.Enabled)) {
                    if (!moduleOptions.Any(m => beacon.Modules.Contains(m)))
                        continue;

                    Button button = InitializeBaseButton(beacon, qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                    button.Click += new EventHandler(BeaconButton_Click);

                    BeaconChoiceTable.Controls.Add(button, BeaconOptions.Count % (BeaconChoiceTable.ColumnCount - 1), BeaconOptions.Count / (BeaconChoiceTable.ColumnCount - 1));
                    BeaconOptions.Add(button);
                }
            }

            UpdateBeacon();
        }

        private void UpdateBeacon() {
            foreach (Button bbutton in BeaconOptions)
                if (bbutton.Tag is IBeacon bcn && nodeData.SelectedBeacon is { Beacon: IBeacon selectedBeacon, Quality: IQuality selectedBeaconQuality })
                    bbutton.BackColor = (bcn == selectedBeacon && qualitySelectorIndexSet[QualitySelector.SelectedIndex] == selectedBeaconQuality) ? SelectedColor : (bcn.IsMissing || !bcn.Available) ? ErrorColor : BeaconChoiceTable.BackColor;
                else if (bbutton.Tag is IBeacon unselectedBcn)
                    bbutton.BackColor = (unselectedBcn.IsMissing || !unselectedBcn.Available) ? ErrorColor : BeaconChoiceTable.BackColor;

            //modules panel
            List<IModule> moduleOptions = GetBeaconModuleOptions();
            bool showModules = nodeData.SelectedBeacon is { Beacon: IBeacon beaconForModules } && beaconForModules.ModuleSlots > 0 && moduleOptions.Count > 0;

            BeaconValuesTable.Visible = nodeData.SelectedBeacon;
            BeaconInfoTable.Visible = nodeData.SelectedBeacon;

            BModulesLabel.Visible = showModules;
            BModuleOptionsLabel.Visible = showModules;
            SelectedBModulesPanel.Visible = showModules;
            BModulesChoicePanel.Visible = showModules;
            SetupBeaconModuleOptions();

            //beacon values
            if (nodeData.SelectedBeacon)
                SetBeaconValues(true);
        }

        private void SetupBeaconModuleOptions() {
            List<IModule> moduleOptions = GetBeaconModuleOptions();
            int moduleSlots = nodeData.SelectedBeacon is { Beacon: IBeacon beaconForSlots } ? beaconForSlots.ModuleSlots : 0;

            CleanTable(BModulesChoiceTable, moduleOptions.Count);
            BModuleOptions.Clear();
            for (int i = 0; i < moduleOptions.Count; i++) {
                Button button = InitializeBaseButton(moduleOptions[i], qualitySelectorIndexSet[QualitySelector.SelectedIndex]);
                if (!moduleOptions[i].Available)
                    button.BackColor = ErrorColor;

                button.MouseUp += new MouseEventHandler(BModuleOptionButton_Click);

                BModulesChoiceTable.Controls.Add(button, BModuleOptions.Count % (BModulesChoiceTable.ColumnCount - 1), BModuleOptions.Count / (BModulesChoiceTable.ColumnCount - 1));
                BModuleOptions.Add(button);
            }

            UpdateBeaconModules();
        }

        private void UpdateBeaconModules() {
            int moduleSlots = nodeData.SelectedBeacon is { Beacon: IBeacon beaconForSlots } ? beaconForSlots.ModuleSlots : 0;
            foreach (Button mbutton in BModuleOptions)
                mbutton.Enabled = nodeData.BeaconModules.Count < moduleSlots;

            List<IModule> moduleOptions = GetBeaconModuleOptions();

            CleanTable(SelectedBModulesTable, nodeData.BeaconModules.Count);

            BeaconModules.Clear();
            for (int i = 0; i < nodeData.BeaconModules.Count; i++) {
                Button button = InitializeBaseButton(nodeData.BeaconModules[i].Module, nodeData.BeaconModules[i].Quality);
                if (nodeData.BeaconModules[i].Module.IsMissing || !nodeData.BeaconModules[i].Module.Available || !nodeData.BeaconModules[i].Module.Enabled || !moduleOptions.Contains(nodeData.BeaconModules[i].Module) || i >= moduleSlots)
                    button.BackColor = ErrorColor;
                button.MouseUp += new MouseEventHandler(BModuleButton_Click);

                SelectedBModulesTable.Controls.Add(button, BeaconModules.Count % (SelectedBModulesTable.ColumnCount - 1), BeaconModules.Count / (SelectedBModulesTable.ColumnCount - 1));
                BeaconModules.Add(button);
            }

            BModulesLabel.Text = string.Format(DisplayCulture.Format, "Modules ({0}/{1}):", nodeData.BeaconModules.Count, moduleSlots);

            UpdateBeaconInfo();
            UpdateAssemblerInfo(); //for the impact of the beacon
        }

        private void UpdateAssemblerInfo() {
            AssemblerRateLabel.Text = string.Format(DisplayCulture.Format, "# of {0}:", nodeData.SelectedAssembler.Assembler.GetEntityTypeName(true));
            AssemblerTitle.Text = string.Format(DisplayCulture.Format, "{0}: {1}", nodeData.SelectedAssembler.Assembler.GetEntityTypeName(false), nodeData.SelectedAssembler.Assembler.FriendlyName);
            SelectedAssemblerIcon.Image = nodeData.SelectedAssembler.Icon;

            AssemblerEnergyPercentLabel.Text = nodeData.GetConsumptionMultiplier().ToString("P0", DisplayCulture.Format);
            AssemblerSpeedPercentLabel.Text = nodeData.GetSpeedMultiplier().ToString("P0", DisplayCulture.Format);
            AssemblerProductivityPercentLabel.Text = nodeData.GetProductivityMultiplier().ToString("P0", DisplayCulture.Format);
            AssemblerPollutionPercentLabel.Text = nodeData.GetPollutionMultiplier().ToString("P0", DisplayCulture.Format);
            AssemblerQualityPercentLabel.Text = nodeData.GetQualityMultiplier().ToString("P0", DisplayCulture.Format);

            bool isAssembler = (nodeData.SelectedAssembler.Assembler.EntityType == EntityType.Assembler || nodeData.SelectedAssembler.Assembler.EntityType == EntityType.Miner || nodeData.SelectedAssembler.Assembler.EntityType == EntityType.OffshorePump);
            AssemblerSpeedTitleLabel.Visible = isAssembler;
            AssemblerSpeedLabel.Visible = isAssembler;
            AssemblerSpeedPercentLabel.Visible = isAssembler;
            AssemblerProductivityTitleLabel.Visible = isAssembler;
            AssemblerProductivityPercentLabel.Visible = isAssembler;
            AssemblerPollutionTitleLabel.Visible = isAssembler;
            AssemblerPollutionPercentLabel.Visible = isAssembler;
            AssemblerQualityTitleLabel.Visible = isAssembler;
            AssemblerQualityPercentLabel.Visible = isAssembler;

            bool isGenerator = nodeData.SelectedAssembler.Assembler.EntityType == EntityType.Generator;
            GeneratorTemperatureLabel.Visible = isGenerator;
            GeneratorTemperatureRangeLabel.Visible = isGenerator;

            AssemblerSpeedLabel.Text = string.Format(DisplayCulture.Format, "{0} ({1} crafts / {2})", nodeData.GetAssemblerSpeed().ToString("0.##", DisplayCulture.Format), nodeData.GetTotalCrafts() < 1 ? nodeData.GetTotalCrafts().ToString("0.####", DisplayCulture.Format) : nodeData.GetTotalCrafts().ToString("0.#", DisplayCulture.Format), RateName);

            AssemblerEnergyLabel.Text = nodeData.SelectedAssembler.Assembler.IsBurner && nodeData.Fuel != null
                ? string.Format(DisplayCulture.Format, "{0} ({1} fuel / {2})", GraphicsStuff.DoubleToEnergy(nodeData.GetAssemblerEnergyConsumption(), "W"), GraphicsStuff.DoubleToString(nodeData.GetTotalAssemblerFuelConsumption()), RateName)
                : GraphicsStuff.DoubleToEnergy(nodeData.GetAssemblerEnergyConsumption(), "W");

            AssemblerPollutionLabel.Text = string.Format(DisplayCulture.Format, "{0} / min", (nodeData.GetAssemblerPollutionProduction() * 60).ToString("0.##", DisplayCulture.Format));

            if (isGenerator) {
                double minTemp = nodeData.GetGeneratorMinimumTemperature();
                double maxTemp = nodeData.GetGeneratorMaximumTemperature();
                double operationalTemp = nodeData.SelectedAssembler.Assembler.OperationTemperature;
                double effectivity = nodeData.GetGeneratorEffectivity();

                GeneratorTemperatureRangeLabel.Text = double.IsInfinity(maxTemp)
                    ? string.Format(DisplayCulture.Format, "min {0}°c  (optimal: {1}°c)", Math.Round(minTemp, 1).ToString("0.#", DisplayCulture.Format), Math.Round(operationalTemp, 1).ToString("0.#", DisplayCulture.Format))
                    : string.Format(DisplayCulture.Format, "{0}-{1}°c  (optimal: {2}°c)", Math.Round(minTemp, 1).ToString("0.#", DisplayCulture.Format), Math.Round(maxTemp, 1).ToString("0.#", DisplayCulture.Format), Math.Round(operationalTemp, 1).ToString("0.#", DisplayCulture.Format));

                AssemblerEnergyLabel.Text = GraphicsStuff.DoubleToEnergy(nodeData.GetGeneratorElectricalProduction(), "W");
                AssemblerEnergyPercentLabel.Text = effectivity.ToString("P0", DisplayCulture.Format);
            }
        }

        private void UpdateBeaconInfo() {
            if (nodeData.SelectedBeacon is { Beacon: IBeacon beacon, Quality: IQuality beaconQuality }) {
                BeaconTitle.Text = string.Format(DisplayCulture.Format, "Beacon: {0}", beacon.FriendlyName);
                SelectedBeaconIcon.Image = nodeData.SelectedBeacon.Icon;
                BeaconEnergyLabel.Text = GraphicsStuff.DoubleToEnergy(nodeData.GetBeaconEnergyConsumption(), "W");
                BeaconModuleCountLabel.Text = beacon.ModuleSlots.ToString(DisplayCulture.Format);
                BeaconEfficiencyLabel.Text = beacon.GetBeaconEffectivity(beaconQuality, nodeData.BeaconCount).ToString("P0", DisplayCulture.Format);
                TotalBeaconEnergyLabel.Text = GraphicsStuff.DoubleToEnergy(nodeData.GetTotalBeaconElectricalConsumption(), "W");
            } else {
                BeaconTitle.Text = string.Format(DisplayCulture.Format, "Beacon: {0}", "-none-");
                SelectedBeaconIcon.Image = null;
                BeaconEnergyLabel.Text = "0J";
                BeaconModuleCountLabel.Text = "0";
                BeaconEfficiencyLabel.Text = "0%";
                TotalBeaconEnergyLabel.Text = "0J";
            }
            TotalBeaconsLabel.Text = nodeData.GetTotalBeacons().ToString(DisplayCulture.Format);
        }

        //------------------------------------------------------------------------------------------------------Helper functions

        private List<IModule> GetAssemblerModuleOptions() {
            return nodeData.SelectedAssembler.Assembler.AllowModules && nodeData.BaseRecipe is { Recipe: IRecipe recipe }
                ? [.. recipe.AssemblerModules.Intersect(nodeData.SelectedAssembler.Assembler.Modules).Where(m => m.Enabled).OrderBy(m => m.LFriendlyName)]
                : [];
        }

        private List<IModule> GetBeaconModuleOptions() {
            return nodeData.SelectedAssembler.Assembler.AllowBeacons && nodeData.SelectedBeacon is { Beacon: IBeacon beacon } && nodeData.BaseRecipe is { Recipe: IRecipe recipe }
                ? [.. recipe.BeaconModules.Intersect(beacon.Modules).Where(m => m.Enabled).OrderBy(m => m.LFriendlyName)]
                : [];
        }

        private NFButton InitializeBaseButton(IDataObjectBase obj, IQuality? quality) {
            var button = new NFButton {
                //button.BackColor = RecipeNode.SelectedAssembler == assembler? Color.DarkOrange : assembler.Available? Color.Gray : Color.DarkRed;
                ForeColor = Color.Gray,
                BackgroundImageLayout = ImageLayout.Zoom,
                BackgroundImage = quality == panelCache.DefaultQuality || quality is null ? obj.Icon : IconCacheProcessor.CombinedQualityIcon(obj.Icon, quality.Icon),
                UseVisualStyleBackColor = false,
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.BorderColor = Color.Black;
            button.TabStop = false;
            button.Margin = new Padding(0);
            button.Size = new Size(1, 1);
            button.Dock = DockStyle.Fill;
            button.Tag = obj;
            button.Enabled = true;

            button.MouseHover += new EventHandler(Button_MouseHover);
            button.MouseLeave += new EventHandler(Button_MouseLeave);
            return button;
        }

        private static void CleanTable(TableLayoutPanel table, int newCellCount) {
            while (table.Controls.Count > 0)
                table.Controls[0].Dispose();
            while (table.RowStyles.Count > 1)
                table.RowStyles.RemoveAt(0);
            for (int i = 0; i < (newCellCount - 1) / (table.ColumnCount - 1); i++)
                table.RowStyles.Add(new RowStyle(table.RowStyles[0].SizeType, table.RowStyles[0].Height));
            table.RowCount = table.RowStyles.Count;
        }

        private static void UpdateRowHeights(TableLayoutPanel table) {
            int height = (table.Width - (table.RowStyles.Count > 2 ? 20 : 0)) / (table.ColumnCount - 1);
            for (int i = 0; i < table.RowStyles.Count; i++)
                table.RowStyles[i].Height = height;
        }

        private static void UpdateFixedFlowInputDecimals(NumericUpDown nud, int max = 4) {
            int decimals = MathDecimals.GetDecimals(nud.Value);
            decimals = Math.Min(decimals, max);
            nud.DecimalPlaces = decimals;
        }

        //------------------------------------------------------------------------------------------------------Button clicks

        private void AssemblerButton_Click(object? sender, EventArgs e) {
            if (sender is not Button b || b.Tag is not IAssembler newAssembler)
                return;
            IQuality quality = qualitySelectorIndexSet[QualitySelector.SelectedIndex];
            nodeController.SetAssembler(new AssemblerQualityPair(newAssembler, quality));
            myGraphViewer.Graph.UpdateNodeValues();
            UpdateAssembler();

        }
        private void FuelButton_Click(object? sender, EventArgs e) {
            if (sender is not Button b || b.Tag is not IItem newFuel)
                return;
            nodeController.SetFuel(newFuel);
            myGraphViewer.Graph.UpdateNodeValues();
            UpdateFuel();
        }
        private void AModuleButton_Click(object? sender, MouseEventArgs e) {
            if (sender is not Button btn || !new Rectangle(new Point(0, 0), btn.Size).Contains(e.Location))
                return;

            ToolTip.Hide(btn);
            int index = AssemblerModules.IndexOf(btn);

            if (e.Button == MouseButtons.Left)
                nodeController.RemoveAssemblerModule(index);
            else if (e.Button == MouseButtons.Right)
                nodeController.RemoveAssemblerModules(nodeData.AssemblerModules[index]);
            else
                return;

            myGraphViewer.Graph.UpdateNodeValues();
            UpdateAssemblerModules();
        }
        private void AModuleOptionButton_Click(object? sender, MouseEventArgs e) {
            if (sender is not Button btn || btn.Tag is not IModule newModule || !new Rectangle(new Point(0, 0), btn.Size).Contains(e.Location))
                return;

            IQuality quality = qualitySelectorIndexSet[QualitySelector.SelectedIndex];

            if (e.Button == MouseButtons.Left)
                nodeController.AddAssemblerModule(new ModuleQualityPair(newModule, quality));
            else if (e.Button == MouseButtons.Right)
                nodeController.AddAssemblerModules(new ModuleQualityPair(newModule, quality));
            else
                return;

            myGraphViewer.Graph.UpdateNodeValues();
            UpdateAssemblerModules();
        }
        private void BeaconButton_Click(object? sender, EventArgs e) {
            if (sender is not Button b || b.Tag is not IBeacon newBeacon)
                return;
            IQuality quality = qualitySelectorIndexSet[QualitySelector.SelectedIndex];
            var newBeaconQP = new BeaconQualityPair(newBeacon, quality);

            if (nodeData.SelectedBeacon == newBeaconQP)
                nodeController.ClearBeacon();
            else
                nodeController.SetBeacon(newBeaconQP);
            myGraphViewer.Graph.UpdateNodeValues();
            UpdateBeacon();
        }
        private void BModuleButton_Click(object? sender, MouseEventArgs e) {
            if (sender is not Button btn || !new Rectangle(new Point(0, 0), btn.Size).Contains(e.Location))
                return;

            ToolTip.Hide(btn);
            int index = BeaconModules.IndexOf(btn);

            if (e.Button == MouseButtons.Left)
                nodeController.RemoveBeaconModule(index);
            else if (e.Button == MouseButtons.Right)
                nodeController.RemoveBeaconModules(nodeData.BeaconModules[index]);
            else
                return;

            myGraphViewer.Graph.UpdateNodeValues();
            UpdateBeaconModules();
        }
        private void BModuleOptionButton_Click(object? sender, MouseEventArgs e) {
            if (sender is not Button btn || btn.Tag is not IModule newModule || !new Rectangle(new Point(0, 0), btn.Size).Contains(e.Location))
                return;

            IQuality quality = qualitySelectorIndexSet[QualitySelector.SelectedIndex];

            if (e.Button == MouseButtons.Left)
                nodeController.AddBeaconModule(new ModuleQualityPair(newModule, quality));
            else if (e.Button == MouseButtons.Right)
                nodeController.AddBeaconModules(new ModuleQualityPair(newModule, quality));
            else
                return;

            myGraphViewer.Graph.UpdateNodeValues();
            UpdateBeaconModules();
        }

        //------------------------------------------------------------------------------------------------------Button hovers

        private void Button_MouseHover(object? sender, EventArgs e) {
            if (sender is not Control control)
                return;
            if (control.Tag is IItem fuel) {
                //the only items in this panel are fuels
                ToolTip.SetText(fuel.FriendlyName + "\nFuel value: " + GraphicsStuff.DoubleToEnergy(fuel.FuelValue, "J"));
                ToolTip.Show(this, Point.Add(PointToClient(Control.MousePosition), new Size(15, 5)));
            } else if (control.Tag is IDataObjectBase dob) {
                ToolTip.SetText(dob.FriendlyName);
                ToolTip.Show(this, Point.Add(PointToClient(Control.MousePosition), new Size(15, 5)));
            }
        }

        private void Button_MouseLeave(object? sender, EventArgs e) {
            if (sender is Control control)
                ToolTip.Hide(control);
        }

        //------------------------------------------------------------------------------------------------------Priority Checkbox
        private void LowPriorityCheckBox_CheckedChanged(object? sender, EventArgs e) {
            nodeController.SetPriority(LowPriorityCheckBox.Checked);
            myGraphViewer.Graph.UpdateNodeValues();
        }

        //------------------------------------------------------------------------------------------------------Rate input & keynode events

        private void SetFixedRate() {
            if (nodeData.DesiredSetValue != (double)FixedAssemblerInput.Value) {
                nodeController.SetDesiredSetValue((double)FixedAssemblerInput.Value);
                myGraphViewer.Graph.UpdateNodeValues();

                UpdateAssemblerInfo();
                UpdateBeaconInfo();
            }
        }

        private void FixedAssemblerOption_CheckedChanged(object? sender, EventArgs e) {
            FixedAssemblerInput.Enabled = FixedAssemblersOption.Checked;
            RateType updatedRateType = (FixedAssemblersOption.Checked) ? RateType.Manual : RateType.Auto;

            if (nodeData.RateType != updatedRateType) {
                nodeController.SetRateType(updatedRateType);
                nodeController.SetDesiredSetValue((double)FixedAssemblerInput.Value);
                myGraphViewer.Graph.UpdateNodeValues();

                UpdateAssemblerInfo();
                UpdateBeaconInfo();
            }
        }

        private void FixedAssemblerInput_ValueChanged(object? sender, EventArgs e) {
            if (sender is not NumericUpDown nud)
                return;
            SetFixedRate();
            UpdateFixedFlowInputDecimals(nud, 2);
        }

        private void KeyNodeCheckBox_CheckedChanged(object? sender, EventArgs e) {
            nodeController.SetKeyNode(KeyNodeCheckBox.Checked);
            KeyNodeTitleLabel.Visible = nodeData.KeyNode;
            KeyNodeTitleInput.Visible = nodeData.KeyNode;
            KeyNodeTitleInput.Text = nodeData.KeyNodeTitle;
            myGraphViewer.Invalidate();
            RefreshViewportLayout();
        }

        private void KeyNodeTitleInput_TextChanged(object? sender, EventArgs e) {
            nodeController.SetKeyNodeTitle(KeyNodeTitleInput.Text);
        }

        //------------------------------------------------------------------------------------------------------assembler neighbour bonus input events

        private void SetNeighbourBonus() {
            if (nodeData.NeighbourCount != (double)NeighbourInput.Value) {
                nodeController.SetNeighbourCount((double)NeighbourInput.Value);
                myGraphViewer.Graph.UpdateNodeValues();

                UpdateAssemblerInfo();
            }
        }

        private void NeighbourInput_ValueChanged(object? sender, EventArgs e) {
            if (sender is not NumericUpDown nud)
                return;
            SetNeighbourBonus();
            UpdateFixedFlowInputDecimals(nud, 2);
        }

        //------------------------------------------------------------------------------------------------------assembler extra productivity input events

        private void SetExtraProductivityBonus() {
            if (nodeData.ExtraProductivity != (double)ExtraProductivityInput.Value / 100) {
                nodeController.SetExtraProductivityBonus((double)ExtraProductivityInput.Value / 100);
                myGraphViewer.Graph.UpdateNodeValues();

                UpdateAssemblerInfo();
            }
        }

        private void ExtraProductivityInput_ValueChanged(object? sender, EventArgs e) {
            SetExtraProductivityBonus();
        }

        //------------------------------------------------------------------------------------------------------beacon input events

        private void SetBeaconValues(bool graphUpdateRequired) {
            if (nodeData.BeaconCount != (double)BeaconCountInput.Value || nodeData.BeaconsPerAssembler != (double)BeaconsPerAssemblerInput.Value || nodeData.BeaconsConst != (double)ConstantBeaconInput.Value) {
                nodeController.SetBeaconCount((double)BeaconCountInput.Value);
                nodeController.SetBeaconsPerAssembler((double)BeaconsPerAssemblerInput.Value);
                nodeController.SetBeaconsCont((double)ConstantBeaconInput.Value);

                if (graphUpdateRequired)
                    myGraphViewer.Graph.UpdateNodeValues(); //only graph update worthy change is the # of beacons. the others arent as important

                UpdateAssemblerInfo();
                UpdateBeaconInfo();
            }
        }

        private void BeaconInput_ValueChanged(object? sender, EventArgs e) {
            if (sender is not NumericUpDown nud)
                return;
            SetBeaconValues(sender == BeaconCountInput);
            UpdateFixedFlowInputDecimals(nud, 2);
        }

        private void QualitySelector_SelectedIndexChanged(object? sender, EventArgs e) {
            SetupAssemblerOptions();
            SetupAssemblerModuleOptions();
            SetupBeaconOptions();
            SetupBeaconModuleOptions();

            myGraphViewer.Graph.DefaultAssemblerQuality = qualitySelectorIndexSet[QualitySelector.SelectedIndex];
            RefreshViewportLayout();
        }
    }
}
