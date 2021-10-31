using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Foreman
{
	public partial class RateOptionsPanel : UserControl
	{
		public ProductionNode BaseNode { get; private set; }
		public ProductionGraphViewer GraphViewer { get; private set; }

		private static Assembler BestAssembler = new Assembler("best\n") { FriendlyName = "Best" };
		private static Module NoneModule = new Module("none\n", 0, 0, new HashSet<string>()) { FriendlyName = "None" };
		private static Module BestModule = new Module("best\n", 0, 0, new HashSet<string>()) { FriendlyName = "Max Speed" };
		private static Module ProdModule = new Module("productive\n", 0, 0, new HashSet<string>()) { FriendlyName = "Max Productivity" };

		public RateOptionsPanel(ProductionNode baseNode, ProductionGraphViewer graphViewer)
		{
			InitializeComponent();

			BaseNode = baseNode;
			GraphViewer = graphViewer;

			if (baseNode.rateType == RateType.Auto)
			{
				autoOption.Checked = true;
				fixedTextBox.Enabled = false;
			}
			else
			{
				fixedOption.Checked = true;
				fixedTextBox.Enabled = true;
			}

			float amountToShow = baseNode.desiredRate;
			if (GraphViewer.Graph.SelectedAmountType == AmountType.Rate)
			{
				fixedTextBox.Width = 65;
				unitLabel.Visible = true;

				if (GraphViewer.Graph.SelectedUnit == RateUnit.PerMinute)
				{
					amountToShow *= 60;
					unitLabel.Text = "/m";
				}
				else
				{
					unitLabel.Text = "/s";
				}
			}
			else
			{
				unitLabel.Visible = false;
				unitLabel.Text = "";
				unitLabel.Width = unitLabel.PreferredWidth;
				ratePanel.Width = ratePanel.PreferredSize.Width;
				fixedTextBox.Width = 85;
			}
			fixedTextBox.Text = Convert.ToString(amountToShow);

            this.productivityBonusTextBox.Text = Convert.ToString(baseNode.ProductivityBonus);
            this.speedBonusTextBox.Text = Convert.ToString(baseNode.SpeedBonus);

			if (baseNode is RecipeNode rNode)
			{
				this.assemblerPanel.Visible = true;
				Recipe recipe = rNode.BaseRecipe;

				//Set up the assembler selection list
				var allowedAssemblers = DataCache.Assemblers.Values
					.Where(a => a.Enabled)
					.Where(a => a.Categories.Contains(recipe.Category));

				AssemblerSelectionBox.Items.Add(BestAssembler);
				foreach (Assembler assembler in allowedAssemblers.OrderBy(a => a.FriendlyName))
					AssemblerSelectionBox.Items.Add(assembler);

				if (rNode.Assembler == null)
					AssemblerSelectionBox.SelectedIndex = 0; //the 'best' option
				else
					AssemblerSelectionBox.SelectedItem = rNode.Assembler;

				//Set up the module selection list
				var allowedModules = DataCache.Modules.Values
					.Where(a => a.Enabled)
					.Where(a => a.AllowedIn(recipe));

				ModuleSelectionBox.Items.Add(NoneModule);
				ModuleSelectionBox.Items.Add(BestModule);
				ModuleSelectionBox.Items.Add(ProdModule);
				foreach (Module module in allowedModules.OrderBy(a => a.FriendlyName))
					ModuleSelectionBox.Items.Add(module);
				if (DataCache.Modules.ContainsKey(rNode.NodeModules.Name))
					ModuleSelectionBox.SelectedItem = DataCache.Modules[rNode.NodeModules.Name];
				else if (rNode.NodeModules == ModuleSelector.Fastest)
					ModuleSelectionBox.SelectedIndex = 1;
				else if (rNode.NodeModules == ModuleSelector.Productive)
					ModuleSelectionBox.SelectedIndex = 2;
				else //if (rNode.NodeModules == ModuleSelector.None)
					ModuleSelectionBox.SelectedIndex = 0;
			}
			else
				this.assemblerPanel.Visible = false;
		}

		private void updateValues()
        {
			bool valuesChanged = false;

			float newAmount;
			if (float.TryParse(fixedTextBox.Text, out newAmount))
			{
				if (GraphViewer.Graph.SelectedAmountType == AmountType.Rate && GraphViewer.Graph.SelectedUnit == RateUnit.PerMinute)
				{
					newAmount /= 60;
				}

				if (BaseNode.desiredRate != newAmount)
				{
					BaseNode.desiredRate = newAmount;
					valuesChanged = true;
				}
			}

			double newProductivity;
			if (double.TryParse(productivityBonusTextBox.Text, out newProductivity))
			{
				if (BaseNode.ProductivityBonus != newProductivity)
				{
					BaseNode.ProductivityBonus = newProductivity;
					valuesChanged = true;
				}
			}

			double newSpeed;
			if (double.TryParse(speedBonusTextBox.Text, out newSpeed))
			{
				if (BaseNode.SpeedBonus != newSpeed)
				{
					BaseNode.SpeedBonus = newSpeed;
					valuesChanged = true;
				}
			}

			if (valuesChanged)
			{
				GraphViewer.Graph.UpdateNodeValues();
				GraphViewer.UpdateNodes();
			}
		}

		private void fixedOption_CheckedChanged(object sender, EventArgs e)
		{
			fixedTextBox.Enabled = fixedOption.Checked;
			RateType updatedRateType = (fixedOption.Checked) ? RateType.Manual : RateType.Auto;

			if (BaseNode.rateType != updatedRateType)
			{
				BaseNode.rateType = updatedRateType;
				GraphViewer.Graph.UpdateNodeValues();
				GraphViewer.UpdateNodes();
			}
		}

		private void fixedTextBox_LostFocus(object sender, EventArgs e)
		{
			updateValues();
		}

		private void productivityBonusTextBox_LostFocus(object sender, EventArgs e)
		{
			updateValues();
		}

		private void speedBonusTextBox_LostFocus(object sender, EventArgs e)
		{
			updateValues();
		}

		private void KeyPressed(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				updateValues();
		}

        private void AssemblerSelectionBox_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (BaseNode is RecipeNode rNode) //should be obvious
			{
				Assembler updatedAssembler = AssemblerSelectionBox.SelectedItem as Assembler;
				if (updatedAssembler == BestAssembler)
					updatedAssembler = null;
				if(rNode.Assembler != updatedAssembler)
                {
					rNode.Assembler = updatedAssembler;
					GraphViewer.Graph.UpdateNodeValues();
					GraphViewer.UpdateNodes();
				}
			}
        }

		private void ModulesSelectionBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (BaseNode is RecipeNode rNode) //should be obvious
			{
				ModuleSelector updatedMSelector;
				if (ModuleSelectionBox.SelectedItem == NoneModule)
					updatedMSelector = ModuleSelector.None;
				else if (ModuleSelectionBox.SelectedItem == BestModule)
					updatedMSelector = ModuleSelector.Fastest;
				else if (ModuleSelectionBox.SelectedItem == ProdModule)
					updatedMSelector = ModuleSelector.Productive;
				else
					updatedMSelector = ModuleSelector.Specific(ModuleSelectionBox.SelectedItem as Module);

				if(rNode.NodeModules != updatedMSelector)
                {
					rNode.NodeModules = updatedMSelector;
					GraphViewer.Graph.UpdateNodeValues();
					GraphViewer.UpdateNodes();
				}
			}
		}
    }
}
