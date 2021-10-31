using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{
	public partial class RateOptionsPanel : UserControl
	{
		public ProductionNode BaseNode { get; private set; }
		public ProductionGraphViewer GraphViewer { get; private set; }
		private Font BaseAssemblerButtonFont;
		private int BaseAssemblerButtonWidth;
		private Font BaseModuleButtonFont;
		private int BaseModuleButtonWidth;

		public RateOptionsPanel(ProductionNode baseNode, ProductionGraphViewer graphViewer)
		{
			InitializeComponent();
			BaseAssemblerButtonFont = assemblerButton.Font;
			BaseAssemblerButtonWidth = assemblerButton.Width;
			BaseModuleButtonFont = modulesButton.Font;
			BaseModuleButtonWidth = modulesButton.Width;

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

			if (GraphViewer.ShowAssemblers && baseNode is RecipeNode)
			{
				this.assemblerPanel.Visible = true;
				updateButtons();
			}
			else
			{
				this.assemblerPanel.Visible = false;
			}
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

		private void updateButtons()
		{
			var assembler = (BaseNode as RecipeNode).Assembler;
			string newName = (assembler == null ? "Best" : assembler.FriendlyName);

			assemblerButton.Font = BaseAssemblerButtonFont;
			assemblerButton.Text = newName;
			assemblerButton.Width = BaseAssemblerButtonWidth;
			while(assemblerButton.Width > BaseAssemblerButtonWidth)
            {
				assemblerButton.Text = "";
				assemblerButton.Width = BaseAssemblerButtonWidth;
				assemblerButton.Font = new Font(assemblerButton.Font.FontFamily, assemblerButton.Font.Size - 1);
				assemblerButton.Text = newName;
			}

			newName = (BaseNode as RecipeNode).NodeModules.Name;
			modulesButton.Font = BaseModuleButtonFont;
			modulesButton.Text = newName;
			modulesButton.Width = BaseModuleButtonWidth;
			while (modulesButton.Width > BaseModuleButtonWidth)
			{
				modulesButton.Text = "";
				modulesButton.Width = BaseModuleButtonWidth;
				modulesButton.Font = new Font(modulesButton.Font.FontFamily, modulesButton.Font.Size - 1);
				modulesButton.Text = newName;
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

		private void assemblerButton_Click(object sender, EventArgs e)
		{
			var optionList = new List<ChooserControl>();
			var bestOption = new ItemChooserControl(null, "Best", "Best");
			optionList.Add(bestOption);

			var recipeNode = (BaseNode as RecipeNode);
			var recipe = recipeNode.BaseRecipe;

			var allowedAssemblers = DataCache.Assemblers.Values
				.Where(a => a.Enabled)
				.Where(a => a.Categories.Contains(recipe.Category))
				.Where(a => a.MaxIngredients >= recipe.Ingredients.Count);
			foreach (var assembler in allowedAssemblers.OrderBy(a => a.FriendlyName))
			{
				var item = DataCache.Items.Values.SingleOrDefault(i => i.Name == assembler.Name);
				optionList.Add(new ItemChooserControl(item, assembler.FriendlyName, assembler.FriendlyName));
			}

			var chooserPanel = new ChooserPanel(optionList, GraphViewer);

			Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));

			chooserPanel.Show(c =>
			{
				if (c != null)
				{
					Assembler updatedAssembler = null;
					if (c == bestOption)
					{
						updatedAssembler = null;
					}
					if (c != bestOption)
					{
						updatedAssembler = DataCache.Assemblers.Single(a => a.Key == c.DisplayText).Value;

					}
					if ((BaseNode as RecipeNode).Assembler != updatedAssembler)
					{
						(BaseNode as RecipeNode).Assembler = updatedAssembler;
						updateButtons();
						GraphViewer.Graph.UpdateNodeValues();
						GraphViewer.UpdateNodes();
					}
				}
			});
		}

		private void modulesButton_Click(object sender, EventArgs e)
		{
			var optionList = new List<ChooserControl>();
			var fastestOption = new ItemChooserControl(null, "Best", "Best");
			optionList.Add(fastestOption);

			var noneOption = new ItemChooserControl(null, "None", "None");
			optionList.Add(noneOption);

			var productivityOption = new ItemChooserControl(null, "Most Productive", "Most Productive");
			optionList.Add(productivityOption);

			var recipeNode = (BaseNode as RecipeNode);
			var recipe = recipeNode.BaseRecipe;

			var allowedModules = DataCache.Modules.Values
				.Where(a => a.Enabled)
                .Where(a => a.AllowedIn(recipe));

			foreach (var module in allowedModules.OrderBy(a => a.FriendlyName))
			{
				var item = DataCache.Items.Values.SingleOrDefault(i => i.Name == module.Name);
				optionList.Add(new ItemChooserControl(item, module.FriendlyName, module.FriendlyName));
			}

			var chooserPanel = new ChooserPanel(optionList, GraphViewer);

			Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));

			chooserPanel.Show(c =>
			{
				if (c != null)
				{
					ModuleSelector updatedModules;

					if (c == fastestOption)
						updatedModules = ModuleSelector.Fastest;
					else if (c == noneOption)
						updatedModules = ModuleSelector.None;
					else if (c == productivityOption)
						updatedModules = ModuleSelector.Productive;
					else
						updatedModules = ModuleSelector.Specific(DataCache.Modules.Single(a => a.Key == c.DisplayText).Value);

					if ((BaseNode as RecipeNode).NodeModules != updatedModules)
					{
						(BaseNode as RecipeNode).NodeModules = updatedModules;
						updateButtons();
						GraphViewer.Graph.UpdateNodeValues();
						GraphViewer.UpdateNodes();
					}
				}
			});
		}
    }
}
