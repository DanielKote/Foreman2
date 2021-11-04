using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman
{
	public partial class EditRecipePanel : UserControl
	{
		private static readonly Color ErrorColor = Color.DarkRed;
		private static readonly Color SelectedColor = Color.DarkOrange;

		private List<Button> AssemblerOptions;
		private List<Button> FuelOptions;
		private List<Button> AssemblerModules;
		private List<Button> AModuleOptions;
		private List<Button> BeaconOptions;
		private List<Button> BeaconModules;
		private List<Button> BModuleOptions;

		private Dictionary<object, int> LastScrollY;

		private readonly ProductionGraphViewer myGraphViewer;
		private readonly RecipeNodeController nodeController;
		private readonly ReadOnlyRecipeNode nodeData;
		private double RateMultiplier { get { return myGraphViewer.Graph.GetRateMultipler(); } }
		private string RateName { get { return myGraphViewer.Graph.GetRateName(); } }

		public EditRecipePanel(ReadOnlyRecipeNode node, ProductionGraphViewer graphViewer)
		{
			nodeData = node;
			nodeController = (RecipeNodeController)graphViewer.Graph.RequestNodeController(node);
			myGraphViewer = graphViewer;

			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			RateOptionsTable.AutoSize = false; //simplest way of ensuring the width of the panel remains constant (it needs to be autosized during initialization due to DPI & font scaling)

			FixedAssemblerInput.Maximum = (decimal)(ProductionGraph.MaxSetFlow / (1000 * RateMultiplier));
			BeaconCountInput.Value = Math.Min(BeaconCountInput.Maximum, (decimal)nodeData.BeaconCount);
			BeaconsPerAssemblerInput.Value = Math.Min(BeaconsPerAssemblerInput.Maximum, (decimal)nodeData.BeaconsPerAssembler);
			ConstantBeaconInput.Value = Math.Min(ConstantBeaconInput.Maximum, (decimal)nodeData.BeaconsConst);
			NeighbourInput.Value = Math.Min(NeighbourInput.Maximum, (decimal)nodeData.NeighbourCount);
			ExtraProductivityInput.Value = Math.Min(ExtraProductivityInput.Maximum, (decimal)(nodeData.ExtraProductivity * 100));

			AssemblerOptions = new List<Button>();
			FuelOptions = new List<Button>();
			AssemblerModules = new List<Button>();
			AModuleOptions = new List<Button>();
			BeaconOptions = new List<Button>();
			BeaconModules = new List<Button>();
			BModuleOptions = new List<Button>();

			//setup scrolling
			LastScrollY = new Dictionary<object, int>();
			LastScrollY.Add(AssemblerChoicePanel, 0);
			LastScrollY.Add(FuelOptionsPanel, 0);
			LastScrollY.Add(SelectedAModulesPanel, 0);
			LastScrollY.Add(AModulesChoicePanel, 0);
			LastScrollY.Add(BeaconChoicePanel, 0);
			LastScrollY.Add(SelectedBModulesPanel, 0);
			LastScrollY.Add(BModulesChoicePanel, 0);
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
			FixedAssemblersOption.CheckedChanged += FixedAssemblerOption_CheckedChanged;
			FixedAssemblerInput.ValueChanged += FixedAssemblerInput_ValueChanged;
			NeighbourInput.ValueChanged += NeighbourInput_ValueChanged;
			ExtraProductivityInput.ValueChanged += ExtraProductivityInput_ValueChanged;
			BeaconCountInput.ValueChanged += BeaconInput_ValueChanged;
			BeaconsPerAssemblerInput.ValueChanged += BeaconInput_ValueChanged;
			ConstantBeaconInput.ValueChanged += BeaconInput_ValueChanged;
		}

		private void OptionsPanel_MouseWheel(object sender, MouseEventArgs e)
		{
			//had to set up this slightly convoluted scrolling option to account for mouse wheel events being WAY too fast -> it would skip from start to end in a single tick, potentially missing out several lines worth of items.
			Panel sPanel = sender as Panel;

			if (e.Delta < 0 && LastScrollY[sender] < sPanel.Controls[0].Height - sPanel.Height + 5)
				LastScrollY[sender] += sPanel.Height / 4;
			else if (e.Delta > 0 && LastScrollY[sender] > 0)
				LastScrollY[sender] -= sPanel.Height / 4;
			sPanel.AutoScrollPosition = new Point(0, LastScrollY[sender]);
		}

		private void InitializeRates()
		{
			if (nodeData.RateType == RateType.Auto)
			{
				AutoAssemblersOption.Checked = true;
				FixedAssemblerInput.Enabled = false;
				FixedAssemblerInput.Value = Math.Min(FixedAssemblerInput.Maximum, (decimal)nodeData.ActualAssemblerCount);
			}
			else
			{
				FixedAssemblersOption.Checked = true;
				FixedAssemblerInput.Enabled = true;
				FixedAssemblerInput.Value = Math.Min(FixedAssemblerInput.Maximum, (decimal)nodeData.DesiredAssemblerCount);
			}
			UpdateFixedFlowInputDecimals(FixedAssemblerInput);
		}

		private void SetupAssemblerOptions()
		{
			CleanTable(AssemblerChoiceTable, nodeData.BaseRecipe.Assemblers.Count(a => a.Enabled));

			AssemblerOptions.Clear();
			foreach (Assembler assembler in nodeData.BaseRecipe.Assemblers.Where(a => a.Enabled))
			{
				Button button = InitializeBaseButton(assembler);
				button.Click += new EventHandler(AssemblerButton_Click);

				AssemblerChoiceTable.Controls.Add(button, AssemblerOptions.Count % (AssemblerChoiceTable.ColumnCount - 1), AssemblerOptions.Count / (AssemblerChoiceTable.ColumnCount - 1));
				AssemblerOptions.Add(button);
			}

			UpdateAssembler();
		}

		private void UpdateAssembler()
		{
			//assembler button colors
			foreach (Button abutton in AssemblerOptions)
				abutton.BackColor = ((Assembler)abutton.Tag == nodeData.SelectedAssembler) ? SelectedColor : (((Assembler)abutton.Tag).IsMissing || !((Assembler)abutton.Tag).Available) ? ErrorColor : AssemblerChoiceTable.BackColor;

			//neighbour count panel
			if (nodeData.SelectedAssembler.EntityType != EntityType.Reactor)
			{
				NeighbourInput.Visible = false;
				NeighboursLabel.Visible = false;
			}

			//extra productivity bonus panel
			if(nodeData.SelectedAssembler.EntityType != EntityType.Miner && !myGraphViewer.Graph.EnableExtraProductivityForNonMiners)
			{
				ExtraProductivityInput.Visible = false;
				ExtraProductivityLabel.Visible = false;
			}

			//fuel panel
			FuelTitle.Visible = nodeData.SelectedAssembler.IsBurner;
			SelectedFuelIcon.Visible = nodeData.SelectedAssembler.IsBurner;
			FuelOptionsPanel.Visible = nodeData.SelectedAssembler.IsBurner;
			SetupFuelOptions();

			//modules panel
			List<Module> moduleOptions = nodeData.BaseRecipe.Modules.Intersect(nodeData.SelectedAssembler.Modules).OrderBy(m => m.LFriendlyName).ToList();
			bool showModules = nodeData.SelectedAssembler.ModuleSlots > 0 && moduleOptions.Count > 0;
			AModulesLabel.Visible = showModules;
			AModuleOptionsLabel.Visible = showModules;
			SelectedAModulesPanel.Visible = showModules;
			AModulesChoicePanel.Visible = showModules;
			SetupAssemblerModuleOptions();

			//beacon panel
			BeaconTable.Visible = moduleOptions.Count > 0 && myGraphViewer.DCache.Beacons.Count > 0;
			SetupBeaconOptions();
		}

		private void SetupFuelOptions()
		{

			List<Item> fuels = nodeData.SelectedAssembler.Fuels.Where(f => f.ProductionRecipes.Any(r => r.Enabled && r.Assemblers.Any(a => a.Enabled))).ToList();

			CleanTable(FuelOptionsTable, fuels.Count);
			FuelOptionsPanel.Height = (int)(FuelOptionsTable.RowStyles[0].Height * (fuels.Count <= 13 ? 1.2 : 2.2));

			FuelOptions.Clear();
			foreach (Item fuel in fuels)
			{
				Button button = InitializeBaseButton(fuel);
				button.Click += new EventHandler(FuelButton_Click);

				FuelOptionsTable.Controls.Add(button, FuelOptions.Count % (FuelOptionsTable.ColumnCount - 1), FuelOptions.Count / (FuelOptionsTable.ColumnCount - 1));
				FuelOptions.Add(button);
			}

			UpdateFuel();
		}

		private void UpdateFuel()
		{
			foreach (Button fbutton in FuelOptions)
				fbutton.BackColor = ((Item)fbutton.Tag == nodeData.Fuel) ? SelectedColor : (((Item)fbutton.Tag).IsMissing || !((Item)fbutton.Tag).Available || !((Item)fbutton.Tag).ProductionRecipes.Any(r => r.Available && r.Assemblers.Any(a => a.Available))) ? ErrorColor : FuelOptionsTable.BackColor;

			FuelTitle.Text = string.Format("Fuel: {0}", nodeData.Fuel == null ? "-none-" : nodeData.Fuel.FriendlyName);
			SelectedFuelIcon.Image = nodeData.Fuel?.Icon;

			UpdateAssemblerInfo();
		}

		private void SetupAssemblerModuleOptions()
		{
			List<Module> moduleOptions = nodeData.BaseRecipe.Modules.Intersect(nodeData.SelectedAssembler.Modules).Where(m => m.Enabled).OrderBy(m => m.LFriendlyName).ToList();

			CleanTable(AModulesChoiceTable, moduleOptions.Count);
			AModuleOptions.Clear();
			for (int i = 0; i < moduleOptions.Count; i++)
			{
				Button button = InitializeBaseButton(moduleOptions[i]);
				if (!moduleOptions[i].Available)
					button.BackColor = ErrorColor;

				button.Click += new EventHandler(AModuleOptionButton_Click);

				AModulesChoiceTable.Controls.Add(button, AModuleOptions.Count % (AModulesChoiceTable.ColumnCount - 1), AModuleOptions.Count / (AModulesChoiceTable.ColumnCount - 1));
				AModuleOptions.Add(button);
			}

			UpdateAssemblerModules();
		}

		private void UpdateAssemblerModules()
		{
			foreach (Button mbutton in AModuleOptions)
				mbutton.Enabled = nodeData.AssemblerModules.Count < nodeData.SelectedAssembler.ModuleSlots;

			List<Module> moduleOptions = nodeData.BaseRecipe.Modules.Intersect(nodeData.SelectedAssembler.Modules).OrderBy(m => m.LFriendlyName).ToList();

			CleanTable(SelectedAModulesTable, nodeData.AssemblerModules.Count);

			AssemblerModules.Clear();
			for (int i = 0; i < nodeData.AssemblerModules.Count; i++)
			{
				Button button = InitializeBaseButton(nodeData.AssemblerModules[i]);
				if (nodeData.AssemblerModules[i].IsMissing || !nodeData.AssemblerModules[i].Available || !nodeData.AssemblerModules[i].Enabled || !moduleOptions.Contains(nodeData.AssemblerModules[i]) || i >= nodeData.SelectedAssembler.ModuleSlots)
					button.BackColor = ErrorColor;
				button.Click += new EventHandler(AModuleButton_Click);

				SelectedAModulesTable.Controls.Add(button, AssemblerModules.Count % (SelectedAModulesTable.ColumnCount - 1), AssemblerModules.Count / (SelectedAModulesTable.ColumnCount - 1));
				AssemblerModules.Add(button);
			}

			AModulesLabel.Text = string.Format("Modules ({0}/{1}):", nodeData.AssemblerModules.Count, nodeData.SelectedAssembler.ModuleSlots);
			UpdateAssemblerInfo();
		}

		private void SetupBeaconOptions()
		{
			CleanTable(BeaconChoiceTable, myGraphViewer.DCache.Beacons.Values.Count(b => b.Enabled));

			BeaconOptions.Clear();
			foreach (Beacon beacon in myGraphViewer.DCache.Beacons.Values.Where(b => b.Enabled))
			{
				Button button = InitializeBaseButton(beacon);
				button.Click += new EventHandler(BeaconButton_Click);

				BeaconChoiceTable.Controls.Add(button, BeaconOptions.Count % (BeaconChoiceTable.ColumnCount - 1), BeaconOptions.Count / (BeaconChoiceTable.ColumnCount - 1));
				BeaconOptions.Add(button);
			}

			UpdateBeacon();
		}

		private void UpdateBeacon()
		{
			foreach (Button bbutton in BeaconOptions)
				bbutton.BackColor = (((Beacon)bbutton.Tag) == nodeData.SelectedBeacon) ? SelectedColor : (((Beacon)bbutton.Tag).IsMissing || !((Beacon)bbutton.Tag).Available) ? ErrorColor : BeaconChoiceTable.BackColor;

			//modules panel
			List<Module> moduleOptions = nodeData.SelectedBeacon == null ? new List<Module>() : nodeData.BaseRecipe.Modules.Intersect(nodeData.SelectedAssembler.Modules).Intersect(nodeData.SelectedBeacon.Modules).OrderBy(m => m.LFriendlyName).ToList();
			bool showModules = nodeData.SelectedBeacon != null && nodeData.SelectedBeacon.ModuleSlots > 0 && moduleOptions.Count > 0;

			BeaconValuesTable.Visible = nodeData.SelectedBeacon != null;
			BeaconInfoTable.Visible = nodeData.SelectedBeacon != null;

			BModulesLabel.Visible = showModules;
			BModuleOptionsLabel.Visible = showModules;
			SelectedBModulesPanel.Visible = showModules;
			BModulesChoicePanel.Visible = showModules;
			SetupBeaconModuleOptions();

			//beacon values
			if (nodeData.SelectedBeacon != null)
				SetBeaconValues(true);
		}

		private void SetupBeaconModuleOptions()
		{
			List<Module> moduleOptions = nodeData.SelectedBeacon == null ? new List<Module>() : nodeData.BaseRecipe.Modules.Intersect(nodeData.SelectedAssembler.Modules).Intersect(nodeData.SelectedBeacon.Modules).Where(m => m.Enabled).OrderBy(m => m.LFriendlyName).ToList();
			int moduleSlots = nodeData.SelectedBeacon == null ? 0 : nodeData.SelectedBeacon.ModuleSlots;

			CleanTable(BModulesChoiceTable, moduleOptions.Count);
			BModuleOptions.Clear();
			for (int i = 0; i < moduleOptions.Count; i++)
			{
				Button button = InitializeBaseButton(moduleOptions[i]);
				if (!moduleOptions[i].Available)
					button.BackColor = ErrorColor;

				button.Click += new EventHandler(BModuleOptionButton_Click);

				BModulesChoiceTable.Controls.Add(button, BModuleOptions.Count % (BModulesChoiceTable.ColumnCount - 1), BModuleOptions.Count / (BModulesChoiceTable.ColumnCount - 1));
				BModuleOptions.Add(button);
			}

			UpdateBeaconModules();
		}

		private void UpdateBeaconModules()
		{
			foreach (Button mbutton in BModuleOptions)
				mbutton.Enabled = nodeData.BeaconModules.Count < nodeData.SelectedBeacon.ModuleSlots;

			List<Module> moduleOptions = nodeData.SelectedBeacon == null ? new List<Module>() : nodeData.BaseRecipe.Modules.Intersect(nodeData.SelectedAssembler.Modules).Intersect(nodeData.SelectedBeacon.Modules).OrderBy(m => m.LFriendlyName).ToList();
			int moduleSlots = nodeData.SelectedBeacon == null ? 0 : nodeData.SelectedBeacon.ModuleSlots;

			CleanTable(SelectedBModulesTable, nodeData.BeaconModules.Count);

			BeaconModules.Clear();
			for (int i = 0; i < nodeData.BeaconModules.Count; i++)
			{
				Button button = InitializeBaseButton(nodeData.BeaconModules[i]);
				if (nodeData.BeaconModules[i].IsMissing || !nodeData.BeaconModules[i].Available || !nodeData.BeaconModules[i].Enabled || !moduleOptions.Contains(nodeData.BeaconModules[i]) || i >= moduleSlots)
					button.BackColor = ErrorColor;
				button.Click += new EventHandler(BModuleButton_Click);

				SelectedBModulesTable.Controls.Add(button, BeaconModules.Count % (SelectedBModulesTable.ColumnCount - 1), BeaconModules.Count / (SelectedBModulesTable.ColumnCount - 1));
				BeaconModules.Add(button);
			}

			BModulesLabel.Text = string.Format("Modules ({0}/{1}):", nodeData.BeaconModules.Count, moduleSlots);

			UpdateBeaconInfo();
			UpdateAssemblerInfo(); //for the impact of the beacon
		}

		private void UpdateAssemblerInfo()
		{
			AssemblerRateLabel.Text = string.Format("# of {0}:", nodeData.SelectedAssembler.GetEntityTypeName(true));
			AssemblerTitle.Text = string.Format("{0}: {1}", nodeData.SelectedAssembler.GetEntityTypeName(false), nodeData.SelectedAssembler.FriendlyName);
			SelectedAssemblerIcon.Image = nodeData.SelectedAssembler.Icon;

			AssemblerEnergyPercentLabel.Text = nodeData.GetConsumptionMultiplier().ToString("P0");
			AssemblerSpeedPercentLabel.Text = nodeData.GetSpeedMultiplier().ToString("P0");
			AssemblerProductivityPercentLabel.Text = nodeData.GetProductivityMultiplier().ToString("P0");
			AssemblerPollutionPercentLabel.Text = nodeData.GetPollutionMultiplier().ToString("P0");

			bool isAssembler = (nodeData.SelectedAssembler.EntityType == EntityType.Assembler || nodeData.SelectedAssembler.EntityType == EntityType.Miner || nodeData.SelectedAssembler.EntityType == EntityType.OffshorePump);
			AssemblerSpeedTitleLabel.Visible = isAssembler;
			AssemblerSpeedLabel.Visible = isAssembler;
			AssemblerSpeedPercentLabel.Visible = isAssembler;
			AssemblerProductivityTitleLabel.Visible = isAssembler;
			AssemblerProductivityPercentLabel.Visible = isAssembler;
			AssemblerPollutionPercentLabel.Visible = isAssembler;
			bool isGenerator = nodeData.SelectedAssembler.EntityType == EntityType.Generator;
			GeneratorTemperatureLabel.Visible = isGenerator;
			GeneratorTemperatureRangeLabel.Visible = isGenerator;

			AssemblerSpeedLabel.Text = string.Format("{0} ({1} crafts / {2})", nodeData.GetAssemblerSpeed().ToString("0.##"), nodeData.GetTotalCrafts().ToString("0.#"), RateName);

			if (nodeData.SelectedAssembler.IsBurner && nodeData.Fuel != null)
				AssemblerEnergyLabel.Text = string.Format("{0} ({1} fuel / {2})", GraphicsStuff.DoubleToEnergy(nodeData.GetAssemblerEnergyConsumption(), "W"), GraphicsStuff.DoubleToString(nodeData.GetTotalAssemblerFuelConsumption()), RateName);
			else
				AssemblerEnergyLabel.Text = GraphicsStuff.DoubleToEnergy(nodeData.GetAssemblerEnergyConsumption(), "W");

			AssemblerPollutionLabel.Text = string.Format("{0} / min", (nodeData.GetAssemblerPollutionProduction() * 60).ToString("0.##"));

			if(isGenerator)
			{
				double minTemp = nodeData.GetGeneratorMinimumTemperature();
				double maxTemp = nodeData.GetGeneratorMaximumTemperature();
				double operationalTemp = nodeData.SelectedAssembler.OperationTemperature;
				double effectivity = nodeData.GetGeneratorEffectivity();

				if(double.IsInfinity(maxTemp))
					GeneratorTemperatureRangeLabel.Text = string.Format("min {0}°c  (optimal: {1}°c)", Math.Round(minTemp, 1).ToString("0.#"), Math.Round(operationalTemp, 1).ToString("0.#"));
				else
					GeneratorTemperatureRangeLabel.Text = string.Format("{0}-{1}°c  (optimal: {2}°c)", Math.Round(minTemp, 1).ToString("0.#"), Math.Round(maxTemp, 1).ToString("0.#"), Math.Round(operationalTemp, 1).ToString("0.#"));

				AssemblerEnergyLabel.Text = GraphicsStuff.DoubleToEnergy(nodeData.GetGeneratorElectricalProduction(), "W");
				AssemblerEnergyPercentLabel.Text = effectivity.ToString("P0");
			}
		}

		private void UpdateBeaconInfo()
		{
			BeaconTitle.Text = string.Format("Beacon: {0}", nodeData.SelectedBeacon == null ? "-none-" : nodeData.SelectedBeacon.FriendlyName);
			SelectedBeaconIcon.Image = nodeData.SelectedBeacon?.Icon;

			BeaconEnergyLabel.Text = nodeData.SelectedBeacon == null ? "0J" : GraphicsStuff.DoubleToEnergy(nodeData.GetBeaconEnergyConsumption(), "W");
			BeaconModuleCountLabel.Text = nodeData.SelectedBeacon == null ? "0" : nodeData.SelectedBeacon.ModuleSlots.ToString();
			BeaconEfficiencyLabel.Text = nodeData.SelectedBeacon == null ? "0%" : nodeData.SelectedBeacon.BeaconEffectivity.ToString("P0");
			TotalBeaconsLabel.Text = nodeData.GetTotalBeacons().ToString();
			TotalBeaconEnergyLabel.Text = nodeData.SelectedBeacon == null ? "0J" : GraphicsStuff.DoubleToEnergy(nodeData.GetTotalBeaconElectricalConsumption(), "W");
		}

		//------------------------------------------------------------------------------------------------------Helper functions

		private Button InitializeBaseButton(DataObjectBase obj)
		{
			NFButton button = new NFButton();
			//button.BackColor = RecipeNode.SelectedAssembler == assembler? Color.DarkOrange : assembler.Available? Color.Gray : Color.DarkRed;
			button.ForeColor = Color.Gray;
			button.BackgroundImageLayout = ImageLayout.Zoom;
			button.BackgroundImage = obj.Icon;
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

			button.MouseHover += new EventHandler(Button_MouseHover);
			button.MouseLeave += new EventHandler(Button_MouseLeave);
			return button;
		}

		private void CleanTable(TableLayoutPanel table, int newCellCount)
		{
			while (table.Controls.Count > 0)
				table.Controls[0].Dispose();
			while (table.RowStyles.Count > 1)
				table.RowStyles.RemoveAt(0);
			for (int i = 0; i < (newCellCount - 1) / (table.ColumnCount - 1); i++)
				table.RowStyles.Add(new RowStyle(table.RowStyles[0].SizeType, table.RowStyles[0].Height));
			table.RowCount = table.RowStyles.Count;
		}

		private void UpdateRowHeights(TableLayoutPanel table)
		{
			int height = (table.Width - (table.RowStyles.Count > 2 ? 20 : 0)) / (table.ColumnCount - 1);
			for (int i = 0; i < table.RowStyles.Count; i++)
				table.RowStyles[i].Height = height;
		}

		private void UpdateFixedFlowInputDecimals(NumericUpDown nud, int max = 4)
		{
			int decimals = MathDecimals.GetDecimals(nud.Value);
			decimals = Math.Min(decimals, max);
			nud.DecimalPlaces = decimals;
		}

		//------------------------------------------------------------------------------------------------------Button clicks

		private void AssemblerButton_Click(object sender, EventArgs e)
		{
			Assembler newAssembler = ((Button)sender).Tag as Assembler;
			nodeController.SetAssembler(newAssembler);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateAssembler();

		}
		private void FuelButton_Click(object sender, EventArgs e)
		{
			Item newFuel = ((Button)sender).Tag as Item;
			nodeController.SetFuel(newFuel);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateFuel();
		}
		private void AModuleButton_Click(object sender, EventArgs e)
		{
			ToolTip.Hide((Control)sender);
			int index = AssemblerModules.IndexOf((Button)sender);
			nodeController.RemoveAssemblerModule(index);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateAssemblerModules();
		}
		private void AModuleOptionButton_Click(object sender, EventArgs e)
		{
			Module newModule = ((Button)sender).Tag as Module;
			nodeController.AddAssemblerModule(newModule);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateAssemblerModules();
		}
		private void BeaconButton_Click(object sender, EventArgs e)
		{
			Beacon newBeacon = ((Button)sender).Tag as Beacon;
			if (nodeData.SelectedBeacon == newBeacon)
				nodeController.SetBeacon(null);
			else
				nodeController.SetBeacon(newBeacon);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateBeacon();
		}
		private void BModuleButton_Click(object sender, EventArgs e)
		{
			ToolTip.Hide((Control)sender);
			int index = BeaconModules.IndexOf((Button)sender);
			myGraphViewer.Graph.UpdateNodeValues();
			nodeController.RemoveBeaconModule(index);
			UpdateBeaconModules();
		}
		private void BModuleOptionButton_Click(object sender, EventArgs e)
		{
			Module newModule = ((Button)sender).Tag as Module;
			nodeController.AddBeaconModule(newModule);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateBeaconModules();
		}

		//------------------------------------------------------------------------------------------------------Button hovers

		private void Button_MouseHover(object sender, EventArgs e)
		{
			Control control = (Control)sender;
			if(control.Tag is Item fuel)
			{
				//the only items in this panel are fuels
				ToolTip.SetText(fuel.FriendlyName + "\nFuel value: " + GraphicsStuff.DoubleToEnergy(fuel.FuelValue, "J"));
				ToolTip.Show(this, Point.Add(PointToClient(Control.MousePosition), new Size(15, 5)));
			}
			else if (control.Tag is DataObjectBase dob)
			{
				ToolTip.SetText(dob.FriendlyName);
				ToolTip.Show(this, Point.Add(PointToClient(Control.MousePosition), new Size(15, 5)));
			}
		}

		private void Button_MouseLeave(object sender, EventArgs e)
		{
			ToolTip.Hide((Control)sender);
		}

		//------------------------------------------------------------------------------------------------------Rate input events

		private void SetFixedRate()
		{
			if (nodeData.DesiredAssemblerCount != (double)FixedAssemblerInput.Value)
			{
				nodeController.SetDesiredAssemblerCount((double)FixedAssemblerInput.Value);
				myGraphViewer.Graph.UpdateNodeValues();

				UpdateAssemblerInfo();
				UpdateBeaconInfo();
			}
		}

		private void FixedAssemblerOption_CheckedChanged(object sender, EventArgs e)
		{
			FixedAssemblerInput.Enabled = FixedAssemblersOption.Checked;
			RateType updatedRateType = (FixedAssemblersOption.Checked) ? RateType.Manual : RateType.Auto;

			if (nodeData.RateType != updatedRateType)
			{
				nodeController.SetRateType(updatedRateType);
				myGraphViewer.Graph.UpdateNodeValues();

				UpdateAssemblerInfo();
				UpdateBeaconInfo();
			}
		}

		private void FixedAssemblerInput_ValueChanged(object sender, EventArgs e)
		{
			SetFixedRate();
			UpdateFixedFlowInputDecimals(sender as NumericUpDown, 2);
		}

		//------------------------------------------------------------------------------------------------------assembler neighbour bonus input events

		private void SetNeighbourBonus()
		{
			if (nodeData.NeighbourCount != (double)NeighbourInput.Value)
			{
				nodeController.SetNeighbourCount((double)NeighbourInput.Value);
				myGraphViewer.Graph.UpdateNodeValues();

				UpdateAssemblerInfo();
			}
		}

		private void NeighbourInput_ValueChanged(object sender, EventArgs e)
		{
			SetNeighbourBonus();
			UpdateFixedFlowInputDecimals(sender as NumericUpDown, 2);
		}

		//------------------------------------------------------------------------------------------------------assembler extra productivity input events

		private void SetExtraProductivityBonus()
		{
			if (nodeData.ExtraProductivity != (double)ExtraProductivityInput.Value / 100)
			{
				nodeController.SetExtraProductivityBonus((double)ExtraProductivityInput.Value / 100);
				myGraphViewer.Graph.UpdateNodeValues();

				UpdateAssemblerInfo();
			}
		}

		private void ExtraProductivityInput_ValueChanged(object sender, EventArgs e)
		{
			SetExtraProductivityBonus();
		}

		//------------------------------------------------------------------------------------------------------beacon input events

		private void SetBeaconValues(bool graphUpdateRequired)
		{
			if (nodeData.BeaconCount != (double)BeaconCountInput.Value || nodeData.BeaconsPerAssembler != (double)BeaconsPerAssemblerInput.Value || nodeData.BeaconsConst != (double)ConstantBeaconInput.Value)
			{
				nodeController.SetBeaconCount((double)BeaconCountInput.Value);
				nodeController.SetBeaconsPerAssembler((double)BeaconsPerAssemblerInput.Value);
				nodeController.SetBeaconsCont((double)ConstantBeaconInput.Value);

				if (graphUpdateRequired)
					myGraphViewer.Graph.UpdateNodeValues(); //only graph update worthy change is the # of beacons. the others arent as important

				UpdateAssemblerInfo();
				UpdateBeaconInfo();
			}
		}

		private void BeaconInput_ValueChanged(object sender, EventArgs e)
		{
			SetBeaconValues(sender == BeaconCountInput);
			UpdateFixedFlowInputDecimals(sender as NumericUpDown, 2);
		}
	}
}
