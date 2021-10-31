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

			if (RecipeNode.SelectedAssembler.Modules.Count == 0)
				BeaconTable.Visible = false;
			if(RecipeNode.SelectedAssembler.ModuleSlots == 0)
			{
				AModulesLabel.Visible = false;
				AModuleOptionsLabel.Visible = false;
				AModulesChoicePanel.Visible = false;
				SelectedAModulesPanel.Visible = false;
			}

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
			UpdateAssemblerOptions();
			UpdateFuelOptions();
			UpdateCurrentAssemblerModules();
			UpdateAssemblerModuleOptions();
			UpdateBeaconOptions();
			UpdateCurrentBeaconModules();
			UpdateBeaconModuleOptions();
		}

		private void InitializeRates()
		{
			if (RecipeNode.RateType == RateType.Auto)
			{
				AutoAssemblersOption.Checked = true;
				FixedAssemblerCountInput.Enabled = false;
			}
			else
			{
				FixedAssemblersOption.Checked = true;
				FixedAssemblerCountInput.Enabled = true;
			}

			//FixedAssemblerCountInput.Text = Convert.ToString(RecipeNode.DesiredRate);
		}

		private void UpdateAssemblerOptions()
		{
			CleanTable(AssemblerChoiceTable, RecipeNode.BaseRecipe.Assemblers.Count(a => a.Enabled));

			AssemblerOptions.Clear();
			foreach (Assembler assembler in RecipeNode.BaseRecipe.Assemblers.Where(a => a.Enabled))
			{
				Button button = InitializeBaseButton(assembler);
				button.BackColor = (assembler == RecipeNode.SelectedAssembler) ? SelectedColor : (assembler.IsMissing || !assembler.Available) ? ErrorColor : AssemblerChoiceTable.BackColor;
				button.Click += new EventHandler(AssemblerButton_Click);

				AssemblerChoiceTable.Controls.Add(button, AssemblerOptions.Count % (AssemblerChoiceTable.ColumnCount - 1), AssemblerOptions.Count / (AssemblerChoiceTable.ColumnCount - 1));
				AssemblerOptions.Add(button);
			}

			//fuel panel
			FuelTitle.Visible = RecipeNode.SelectedAssembler.IsBurner;
			SelectedFuelIcon.Visible = RecipeNode.SelectedAssembler.IsBurner;
			FuelOptionsPanel.Visible = RecipeNode.SelectedAssembler.IsBurner;

			//modules panel
			List<Module> moduleOptions = RecipeNode.BaseRecipe.Modules.Intersect(RecipeNode.SelectedAssembler.Modules).OrderBy(m => m.LFriendlyName).ToList();
			bool showModules = RecipeNode.SelectedAssembler.ModuleSlots > 0 && moduleOptions.Count > 0;
			AModulesLabel.Visible = showModules;
			AModuleOptionsLabel.Visible = showModules;
			SelectedAModulesPanel.Visible = showModules;
			AModulesChoicePanel.Visible = showModules;

			//beacon panel
			BeaconTable.Visible = moduleOptions.Count > 0;

			//set the current assembler info
			AssemblerTitle.Text = string.Format("Assembler: {0}", RecipeNode.SelectedAssembler.FriendlyName);
			SelectedAssemblerIcon.Image = RecipeNode.SelectedAssembler.Icon;

			AssemblerEnergyPercentLabel.Text = RecipeNode.GetConsumptionMultiplier().ToString("P0");
			AssemblerSpeedPercentLabel.Text = RecipeNode.GetSpeedMultiplier().ToString("P0");
			AssemblerProductivityPercentLabel.Text = RecipeNode.GetProductivityMultiplier().ToString("P0");
			AssemblerPollutionPercentLabel.Text = RecipeNode.GetPollutionMultiplier().ToString("P0");

			float speed = RecipeNode.SelectedAssembler.Speed * RecipeNode.GetSpeedMultiplier();
			AssemblerSpeedLabel.Text = string.Format("{0} ({1} crafts / {2})", (speed).ToString("0.#"), GraphicsStuff.FloatToString(speed / (RecipeNode.BaseRecipe.Time * myGraphViewer.GetRateMultipler())), myGraphViewer.GetRateName());

			float energy = RecipeNode.SelectedAssembler.EnergyConsumption * RecipeNode.GetConsumptionMultiplier();
			if (RecipeNode.SelectedAssembler.IsBurner && RecipeNode.Fuel != null)
				AssemblerEnergyLabel.Text = string.Format("");
			else
				AssemblerEnergyLabel.Text = GraphicsStuff.FloatToEnergy(energy);

		}

		private void UpdateFuelOptions()
		{
			CleanTable(FuelOptionsTable, RecipeNode.SelectedAssembler.Fuels.Count(f => f.Enabled));

			FuelOptions.Clear();
			foreach (Item fuel in RecipeNode.SelectedAssembler.Fuels.Where(a => a.Enabled))
			{
				Button button = InitializeBaseButton(fuel);
				button.Click += new EventHandler(FuelButton_Click);
				if (fuel.IsMissing || !fuel.Available)
					button.BackColor = ErrorColor;

				FuelOptionsTable.Controls.Add(button, FuelOptions.Count % (FuelOptionsTable.ColumnCount - 1), FuelOptions.Count / (FuelOptionsTable.ColumnCount - 1));
				FuelOptions.Add(button);
			}

			FuelTitle.Text = string.Format("Fuel: {0}", RecipeNode.Fuel == null? "-none-" : RecipeNode.Fuel.FriendlyName);
			SelectedFuelIcon.Image = RecipeNode.Fuel?.Icon;
		}

		private void UpdateCurrentAssemblerModules()
		{
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
		}

		private void UpdateAssemblerModuleOptions()
		{
			List<Module> moduleOptions = RecipeNode.BaseRecipe.Modules.Intersect(RecipeNode.SelectedAssembler.Modules).OrderBy(m => m.LFriendlyName).ToList();

			CleanTable(AModulesChoiceTable, moduleOptions.Count);
			AModuleOptions.Clear();
			for(int i = 0; i < moduleOptions.Count; i++)
			{
				Button button = InitializeBaseButton(moduleOptions[i]);
				button.Enabled = RecipeNode.AssemblerModules.Count < RecipeNode.SelectedAssembler.ModuleSlots;
				button.Click += new EventHandler(AModuleOptionButton_Click);

				AModulesChoiceTable.Controls.Add(button, AModuleOptions.Count % (AModulesChoiceTable.ColumnCount - 1), AModuleOptions.Count / (AModulesChoiceTable.ColumnCount - 1));
				AModuleOptions.Add(button);
			}
		}

		private void UpdateBeaconOptions()
		{
			CleanTable(BeaconChoiceTable, myGraphViewer.DCache.Beacons.Values.Count(b => b.Enabled));

			BeaconOptions.Clear();
			foreach (Beacon beacon in myGraphViewer.DCache.Beacons.Values.Where(b => b.Enabled))
			{
				Button button = InitializeBaseButton(beacon);
				button.BackColor = (beacon == RecipeNode.SelectedBeacon) ? SelectedColor : (beacon.IsMissing || !beacon.Available) ? ErrorColor : BeaconChoiceTable.BackColor;
				button.Click += new EventHandler(BeaconButton_Click);

				BeaconChoiceTable.Controls.Add(button, BeaconOptions.Count % (BeaconChoiceTable.ColumnCount - 1), BeaconOptions.Count / (BeaconChoiceTable.ColumnCount - 1));
				BeaconOptions.Add(button);
			}

			BeaconTitle.Text = string.Format("Beacon: {0}", RecipeNode.SelectedBeacon == null? "-none-" : RecipeNode.SelectedBeacon.FriendlyName);
			SelectedBeaconIcon.Image = RecipeNode.SelectedBeacon?.Icon;
		}

		private void UpdateCurrentBeaconModules()
		{
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
		}

		private void UpdateBeaconModuleOptions()
		{
			List<Module> moduleOptions = RecipeNode.SelectedBeacon == null ? new List<Module>() : RecipeNode.BaseRecipe.Modules.Intersect(RecipeNode.SelectedAssembler.Modules).Intersect(RecipeNode.SelectedBeacon.Modules).OrderBy(m => m.LFriendlyName).ToList();
			int moduleSlots = RecipeNode.SelectedBeacon == null ? 0 : RecipeNode.SelectedBeacon.ModuleSlots;

			CleanTable(BModulesChoiceTable, moduleOptions.Count);
			BModuleOptions.Clear();
			for (int i = 0; i < moduleOptions.Count; i++)
			{
				Button button = InitializeBaseButton(moduleOptions[i]);
				button.Enabled = RecipeNode.BeaconModules.Count < moduleSlots;
				button.Click += new EventHandler(BModuleOptionButton_Click);

				BModulesChoiceTable.Controls.Add(button, BModuleOptions.Count % (BModulesChoiceTable.ColumnCount - 1), BModuleOptions.Count / (BModulesChoiceTable.ColumnCount - 1));
				BModuleOptions.Add(button);
			}
		}

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

		//------------------------------------------------------------------------------------------------------Selection update functions


		//------------------------------------------------------------------------------------------------------Button clicks


		private void AssemblerButton_Click(object sender, EventArgs e)
		{
			Assembler newAssembler = ((Button)sender).Tag as Assembler;
			RecipeNode.SetAssembler(newAssembler);

			UpdateAssemblerOptions();
			UpdateFuelOptions();
			UpdateCurrentAssemblerModules();
			UpdateAssemblerModuleOptions();
			UpdateBeaconOptions();
			UpdateCurrentBeaconModules();
			UpdateBeaconModuleOptions();
		}
		private void FuelButton_Click(object sender, EventArgs e)
		{

		}
		private void AModuleButton_Click(object sender, EventArgs e)
		{

		}
		private void AModuleOptionButton_Click(object sender, EventArgs e)
		{

		}
		private void BeaconButton_Click(object sender, EventArgs e)
		{

		}
		private void BModuleButton_Click(object sender, EventArgs e)
		{

		}
		private void BModuleOptionButton_Click(object sender, EventArgs e)
		{

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
		//------------------------------------------------------------------------------------------------------Rate Options

		private void SetFixedRate()
		{
			if (float.TryParse(FixedAssemblerCountInput.Text, out float newAmount))
			{
				RecipeNode.SetBaseNumberOfAssemblers(newAmount / myGraphViewer.GetRateMultipler());

				myGraphViewer.Graph.UpdateNodeValues();
			}
		}

		private void FixedAssemblerOption_CheckedChanged(object sender, EventArgs e)
		{
			FixedAssemblerCountInput.Enabled = FixedAssemblersOption.Checked;
			RateType updatedRateType = (FixedAssemblersOption.Checked) ? RateType.Manual : RateType.Auto;

			if (RecipeNode.RateType != updatedRateType)
			{
				RecipeNode.RateType = updatedRateType;
				myGraphViewer.Graph.UpdateNodeValues();
			}
		}

		private void FixedAssemblerCountInput_LostFocus(object sender, EventArgs e)
		{
			SetFixedRate();
		}

		private void KeyPressed(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				SetFixedRate();
		}
	}
}
