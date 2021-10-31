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


		private ProductionGraphViewer myGraphViewer;
		private RecipeNode RecipeNode;

		private List<Button> AssemblerOptions;
		private List<Button> FuelOptions;
		private List<Button> AssemblerModules;
		private List<Button> AModuleOptions;
		private List<Button> BeaconOptions;
		private List<Button> BeaconModules;
		private List<Button> BModuleOptions;

		public EditRecipePanel(RecipeNode rNode, ProductionGraphViewer graphViewer)
		{
			RecipeNode = rNode;
			myGraphViewer = graphViewer;
			if (RecipeNode.SelectedAssembler == null)
				Dispose();

			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

			FixedAssemblerInput.Maximum = (decimal)(ProductionGraph.MaxSetFlow / (1000 * graphViewer.GetRateMultipler()));
			BeaconCountInput.Value = Math.Min(BeaconCountInput.Maximum, (decimal)rNode.BeaconCount);
			BeaconsPerAssemblerInput.Value = Math.Min(BeaconsPerAssemblerInput.Maximum, (decimal)rNode.BeaconsPerAssembler);
			ConstantBeaconInput.Value = Math.Min(ConstantBeaconInput.Maximum, (decimal)rNode.BeaconsConst);
			NeighbourInput.Value = Math.Min(NeighbourInput.Maximum, (decimal)rNode.NeighbourCount);

			AssemblerOptions = new List<Button>();
			FuelOptions = new List<Button>();
			AssemblerModules = new List<Button>();
			AModuleOptions = new List<Button>();
			BeaconOptions = new List<Button>();
			BeaconModules = new List<Button>();
			BModuleOptions = new List<Button>();

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
			BeaconCountInput.ValueChanged += BeaconInput_ValueChanged;
			BeaconsPerAssemblerInput.ValueChanged += BeaconInput_ValueChanged;
			ConstantBeaconInput.ValueChanged += BeaconInput_ValueChanged;
		}

		private void InitializeRates()
		{
			if (RecipeNode.RateType == RateType.Auto)
			{
				AutoAssemblersOption.Checked = true;
				FixedAssemblerInput.Enabled = false;
				FixedAssemblerInput.Value = Math.Min(FixedAssemblerInput.Maximum, (decimal)(RecipeNode.ActualAssemblerCount / myGraphViewer.GetRateMultipler()));
			}
			else
			{
				FixedAssemblersOption.Checked = true;
				FixedAssemblerInput.Enabled = true;
				FixedAssemblerInput.Value = Math.Min(FixedAssemblerInput.Maximum, (decimal)(RecipeNode.DesiredAssemblerCount / myGraphViewer.GetRateMultipler()));
			}
			UpdateFixedFlowInputDecimals(FixedAssemblerInput);
		}

		private void SetupAssemblerOptions()
		{
			CleanTable(AssemblerChoiceTable, RecipeNode.BaseRecipe.Assemblers.Count(a => a.Enabled));

			AssemblerOptions.Clear();
			foreach (Assembler assembler in RecipeNode.BaseRecipe.Assemblers.Where(a => a.Enabled))
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
			foreach(Button abutton in AssemblerOptions)
				abutton.BackColor = ((Assembler)abutton.Tag == RecipeNode.SelectedAssembler) ? SelectedColor : (((Assembler)abutton.Tag).IsMissing || !((Assembler)abutton.Tag).Available) ? ErrorColor : AssemblerChoiceTable.BackColor;

			//neighbour count panel
			if (RecipeNode.SelectedAssembler.EntityType != EntityType.Reactor)
			{
				NeighbourInput.Visible = false;
				NeighboursLabel.Visible = false;
			}

			//fuel panel
			FuelTitle.Visible = RecipeNode.SelectedAssembler.IsBurner;
			SelectedFuelIcon.Visible = RecipeNode.SelectedAssembler.IsBurner;
			FuelOptionsPanel.Visible = RecipeNode.SelectedAssembler.IsBurner;
			SetupFuelOptions();

			//modules panel
			List<Module> moduleOptions = RecipeNode.BaseRecipe.Modules.Intersect(RecipeNode.SelectedAssembler.Modules).OrderBy(m => m.LFriendlyName).ToList();
			bool showModules = RecipeNode.SelectedAssembler.ModuleSlots > 0 && moduleOptions.Count > 0;
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
			CleanTable(FuelOptionsTable, RecipeNode.SelectedAssembler.Fuels.Count(f => f.Enabled));

			FuelOptions.Clear();
			foreach (Item fuel in RecipeNode.SelectedAssembler.Fuels.Where(a => a.Enabled))
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
				fbutton.BackColor = ((Item)fbutton.Tag == RecipeNode.Fuel)? SelectedColor : (((Item)fbutton.Tag).IsMissing || !((Item)fbutton.Tag).Available) ? ErrorColor : FuelOptionsTable.BackColor;

			FuelTitle.Text = string.Format("Fuel: {0}", RecipeNode.Fuel == null? "-none-" : RecipeNode.Fuel.FriendlyName);
			SelectedFuelIcon.Image = RecipeNode.Fuel?.Icon;

			UpdateAssemblerInfo();
		}

		private void SetupAssemblerModuleOptions()
		{
			List<Module> moduleOptions = RecipeNode.BaseRecipe.Modules.Intersect(RecipeNode.SelectedAssembler.Modules).OrderBy(m => m.LFriendlyName).ToList();

			CleanTable(AModulesChoiceTable, moduleOptions.Count);
			AModuleOptions.Clear();
			for(int i = 0; i < moduleOptions.Count; i++)
			{
				Button button = InitializeBaseButton(moduleOptions[i]);
				button.Click += new EventHandler(AModuleOptionButton_Click);

				AModulesChoiceTable.Controls.Add(button, AModuleOptions.Count % (AModulesChoiceTable.ColumnCount - 1), AModuleOptions.Count / (AModulesChoiceTable.ColumnCount - 1));
				AModuleOptions.Add(button);
			}

			UpdateAssemblerModules();
		}

		private void UpdateAssemblerModules()
		{
			foreach (Button mbutton in AModuleOptions)
				mbutton.Enabled = RecipeNode.AssemblerModules.Count < RecipeNode.SelectedAssembler.ModuleSlots;

			List<Module> moduleOptions = RecipeNode.BaseRecipe.Modules.Intersect(RecipeNode.SelectedAssembler.Modules).OrderBy(m => m.LFriendlyName).ToList();

			CleanTable(SelectedAModulesTable, RecipeNode.SelectedAssembler.ModuleSlots);

			AssemblerModules.Clear();
			for (int i = 0; i < RecipeNode.AssemblerModules.Count; i++)
			{
				Button button = InitializeBaseButton(RecipeNode.AssemblerModules[i]);
				if (RecipeNode.AssemblerModules[i].IsMissing || !RecipeNode.AssemblerModules[i].Available || !RecipeNode.AssemblerModules[i].Enabled || !moduleOptions.Contains(RecipeNode.AssemblerModules[i]) || i >= RecipeNode.SelectedAssembler.ModuleSlots)
					button.BackColor = ErrorColor;
				button.Click += new EventHandler(AModuleButton_Click);

				SelectedAModulesTable.Controls.Add(button, AssemblerModules.Count % (SelectedAModulesTable.ColumnCount - 1), AssemblerModules.Count / (SelectedAModulesTable.ColumnCount - 1));
				AssemblerModules.Add(button);
			}

			AModulesLabel.Text = string.Format("Modules ({0}/{1}):", RecipeNode.AssemblerModules.Count, RecipeNode.SelectedAssembler.ModuleSlots);
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
			foreach(Button bbutton in BeaconOptions)
				bbutton.BackColor = (((Beacon)bbutton.Tag) == RecipeNode.SelectedBeacon) ? SelectedColor : (((Beacon)bbutton.Tag).IsMissing || !((Beacon)bbutton.Tag).Available) ? ErrorColor : BeaconChoiceTable.BackColor;

			//modules panel
			List<Module> moduleOptions = RecipeNode.SelectedBeacon == null ? new List<Module>() : RecipeNode.BaseRecipe.Modules.Intersect(RecipeNode.SelectedAssembler.Modules).Intersect(RecipeNode.SelectedBeacon.Modules).OrderBy(m => m.LFriendlyName).ToList();
			bool showModules = RecipeNode.SelectedBeacon != null && RecipeNode.SelectedBeacon.ModuleSlots > 0 && moduleOptions.Count > 0;
			
			BeaconValuesTable.Visible = RecipeNode.SelectedBeacon != null;
			BeaconInfoTable.Visible = RecipeNode.SelectedBeacon != null;

			BModulesLabel.Visible = showModules;
			BModuleOptionsLabel.Visible = showModules;
			SelectedBModulesPanel.Visible = showModules;
			BModulesChoicePanel.Visible = showModules;
			SetupBeaconModuleOptions();

			//beacon values
			if (RecipeNode.SelectedBeacon != null)
				SetBeaconValues(true);
		}

		private void SetupBeaconModuleOptions()
		{
			List<Module> moduleOptions = RecipeNode.SelectedBeacon == null ? new List<Module>() : RecipeNode.BaseRecipe.Modules.Intersect(RecipeNode.SelectedAssembler.Modules).Intersect(RecipeNode.SelectedBeacon.Modules).OrderBy(m => m.LFriendlyName).ToList();
			int moduleSlots = RecipeNode.SelectedBeacon == null ? 0 : RecipeNode.SelectedBeacon.ModuleSlots;

			CleanTable(BModulesChoiceTable, moduleOptions.Count);
			BModuleOptions.Clear();
			for (int i = 0; i < moduleOptions.Count; i++)
			{
				Button button = InitializeBaseButton(moduleOptions[i]);
				button.Click += new EventHandler(BModuleOptionButton_Click);

				BModulesChoiceTable.Controls.Add(button, BModuleOptions.Count % (BModulesChoiceTable.ColumnCount - 1), BModuleOptions.Count / (BModulesChoiceTable.ColumnCount - 1));
				BModuleOptions.Add(button);
			}

			UpdateBeaconModules();
		}

		private void UpdateBeaconModules()
		{
			foreach (Button mbutton in BModuleOptions)
				mbutton.Enabled = RecipeNode.BeaconModules.Count < RecipeNode.SelectedBeacon.ModuleSlots;

			List<Module> moduleOptions = RecipeNode.SelectedBeacon == null? new List<Module>() : RecipeNode.BaseRecipe.Modules.Intersect(RecipeNode.SelectedAssembler.Modules).Intersect(RecipeNode.SelectedBeacon.Modules).OrderBy(m => m.LFriendlyName).ToList();
			int moduleSlots = RecipeNode.SelectedBeacon == null ? 0 : RecipeNode.SelectedBeacon.ModuleSlots;

			CleanTable(SelectedBModulesTable,  moduleSlots);

			BeaconModules.Clear();
			for (int i = 0; i < RecipeNode.BeaconModules.Count; i++)
			{
				Button button = InitializeBaseButton(RecipeNode.BeaconModules[i]);
				if (RecipeNode.BeaconModules[i].IsMissing || !RecipeNode.BeaconModules[i].Available || !RecipeNode.BeaconModules[i].Enabled || !moduleOptions.Contains(RecipeNode.BeaconModules[i]) || i >= moduleSlots)
					button.BackColor = ErrorColor;
				button.Click += new EventHandler(BModuleButton_Click);

				SelectedBModulesTable.Controls.Add(button, BeaconModules.Count % (SelectedBModulesTable.ColumnCount - 1), BeaconModules.Count / (SelectedBModulesTable.ColumnCount - 1));
				BeaconModules.Add(button);
			}

			BModulesLabel.Text = string.Format("Modules ({0}/{1}):", RecipeNode.BeaconModules.Count, moduleSlots);

			UpdateBeaconInfo();
		}

		private void UpdateAssemblerInfo()
		{
			AssemblerTitle.Text = string.Format("Assembler: {0}", RecipeNode.SelectedAssembler.FriendlyName);
			SelectedAssemblerIcon.Image = RecipeNode.SelectedAssembler.Icon;

			AssemblerEnergyPercentLabel.Text = RecipeNode.GetConsumptionMultiplier().ToString("P0");
			AssemblerSpeedPercentLabel.Text = RecipeNode.GetSpeedMultiplier().ToString("P0");
			AssemblerProductivityPercentLabel.Text = RecipeNode.GetProductivityMultiplier().ToString("P0");
			AssemblerPollutionPercentLabel.Text = RecipeNode.GetPollutionMultiplier().ToString("P0");

			double speed = RecipeNode.SelectedAssembler.Speed * RecipeNode.GetSpeedMultiplier();
			AssemblerSpeedLabel.Text = string.Format("{0} ({1} crafts / {2})", (speed).ToString("0.##"), (speed * myGraphViewer.GetRateMultipler() / RecipeNode.BaseRecipe.Time).ToString("0.#"), myGraphViewer.GetRateName());

			double energy = RecipeNode.GetAssemblerElectricalProduction() > 0? -RecipeNode.GetAssemblerElectricalProduction() : RecipeNode.GetAssemblerEnergyConsumption();
			if (RecipeNode.SelectedAssembler.IsBurner && RecipeNode.Fuel != null)
				AssemblerEnergyLabel.Text = string.Format("{0} ({1} fuel / {2})", GraphicsStuff.DoubleToEnergy(energy, "W"), GraphicsStuff.DoubleToString(RecipeNode.GetTotalAssemblerFuelConsumption()), myGraphViewer.GetRateName());
			else
				AssemblerEnergyLabel.Text = GraphicsStuff.DoubleToEnergy(energy, "W");

			AssemblerPollutionLabel.Text = string.Format("{0} / {1}", (RecipeNode.GetAssemblerPollutionProduction() * RecipeNode.ActualAssemblerCount).ToString("0.##"), myGraphViewer.GetRateName());
		}

		private void UpdateBeaconInfo()
		{
			BeaconTitle.Text = string.Format("Beacon: {0}", RecipeNode.SelectedBeacon == null ? "-none-" : RecipeNode.SelectedBeacon.FriendlyName);
			SelectedBeaconIcon.Image = RecipeNode.SelectedBeacon?.Icon;

			BeaconEnergyLabel.Text = RecipeNode.SelectedBeacon == null ? "0J" : GraphicsStuff.DoubleToEnergy(RecipeNode.SelectedBeacon.EnergyConsumption + RecipeNode.SelectedBeacon.EnergyDrain, "W");
			BeaconModuleCountLabel.Text = RecipeNode.SelectedBeacon == null ? "0" : RecipeNode.SelectedBeacon.ModuleSlots.ToString();
			BeaconEfficiencyLabel.Text = RecipeNode.SelectedBeacon == null ? "0%" : RecipeNode.SelectedBeacon.BeaconEffectivity.ToString("P0");
			TotalBeaconsLabel.Text = RecipeNode.GetTotalBeacons(myGraphViewer.GetRateMultipler()).ToString();
			TotalBeaconEnergyLabel.Text = RecipeNode.SelectedBeacon == null? "0J" : GraphicsStuff.DoubleToEnergy((RecipeNode.SelectedBeacon.EnergyConsumption + RecipeNode.SelectedBeacon.EnergyDrain) * RecipeNode.GetTotalBeacons(myGraphViewer.GetRateMultipler()), "W");
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
			int height = (table.Width - (table.RowStyles.Count > 2? 20 : 0)) / (table.ColumnCount - 1);
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
			RecipeNode.SetAssembler(newAssembler);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateAssembler();

		}
		private void FuelButton_Click(object sender, EventArgs e)
		{
			Item newFuel = ((Button)sender).Tag as Item;
			RecipeNode.SetFuel(newFuel);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateFuel();
		}
		private void AModuleButton_Click(object sender, EventArgs e)
		{
			int index = AssemblerModules.IndexOf((Button)sender);
			RecipeNode.RemoveAssemblerModule(index);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateAssemblerModules();
		}
		private void AModuleOptionButton_Click(object sender, EventArgs e)
		{
			Module newModule = ((Button)sender).Tag as Module;
			RecipeNode.AddAssemblerModule(newModule);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateAssemblerModules();
		}
		private void BeaconButton_Click(object sender, EventArgs e)
		{
			Beacon newBeacon = ((Button)sender).Tag as Beacon;
			if (RecipeNode.SelectedBeacon == newBeacon)
				RecipeNode.SetBeacon(null);
			else
				RecipeNode.SetBeacon(newBeacon);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateBeacon();
		}
		private void BModuleButton_Click(object sender, EventArgs e)
		{
			int index = BeaconModules.IndexOf((Button)sender);
			myGraphViewer.Graph.UpdateNodeValues();
			RecipeNode.RemoveBeaconModule(index);
			UpdateBeaconModules();
		}
		private void BModuleOptionButton_Click(object sender, EventArgs e)
		{
			Module newModule = ((Button)sender).Tag as Module;
			RecipeNode.AddBeaconModule(newModule);
			myGraphViewer.Graph.UpdateNodeValues();
			UpdateBeaconModules();
		}

		//------------------------------------------------------------------------------------------------------Button hovers

		private void Button_MouseHover(object sender, EventArgs e)
		{
			Control control = (Control)sender;
			if (control.Tag is DataObjectBase dob)
			{
				ToolTip.SetText(dob.FriendlyName);
				ToolTip.Show(this, Point.Add(PointToClient(Control.MousePosition), new Size(15,5)));
			}
		}

		private void Button_MouseLeave(object sender, EventArgs e)
		{
			ToolTip.Hide((Control)sender);
		}

		//------------------------------------------------------------------------------------------------------Rate input events

		private void SetFixedRate()
		{
			if (RecipeNode.DesiredAssemblerCount != (double)FixedAssemblerInput.Value * myGraphViewer.GetRateMultipler())
			{
				RecipeNode.DesiredAssemblerCount = (double)FixedAssemblerInput.Value * myGraphViewer.GetRateMultipler();
				myGraphViewer.Graph.UpdateNodeValues();

				UpdateAssemblerInfo();
				UpdateBeaconInfo();
			}
		}

		private void FixedAssemblerOption_CheckedChanged(object sender, EventArgs e)
		{
			FixedAssemblerInput.Enabled = FixedAssemblersOption.Checked;
			RateType updatedRateType = (FixedAssemblersOption.Checked) ? RateType.Manual : RateType.Auto;

			if (RecipeNode.RateType != updatedRateType)
			{
				RecipeNode.RateType = updatedRateType;
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
			if (RecipeNode.NeighbourCount != (double)NeighbourInput.Value)
			{
				RecipeNode.NeighbourCount = (double)NeighbourInput.Value;
				myGraphViewer.Graph.UpdateNodeStates();

				UpdateAssemblerInfo();
			}
		}

		private void NeighbourInput_ValueChanged(object sender, EventArgs e)
		{
			SetNeighbourBonus();
			UpdateFixedFlowInputDecimals(sender as NumericUpDown, 2);
		}

		//------------------------------------------------------------------------------------------------------beacon input events

		private void SetBeaconValues(bool graphUpdateRequired)
		{
			if(RecipeNode.BeaconCount != (double)BeaconCountInput.Value || RecipeNode.BeaconsPerAssembler != (double)BeaconsPerAssemblerInput.Value || RecipeNode.BeaconsConst != (double)ConstantBeaconInput.Value)
			{
				RecipeNode.BeaconCount = (double)BeaconCountInput.Value;
				RecipeNode.BeaconsPerAssembler = (double)BeaconsPerAssemblerInput.Value;
				RecipeNode.BeaconsConst = (double)ConstantBeaconInput.Value;

				if(graphUpdateRequired)
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
