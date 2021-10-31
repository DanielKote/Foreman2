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
		private ProductionGraphViewer myGraphViewer;
		private BaseNode baseNode;

		public EditRecipePanel(BaseNode baseNode, ProductionGraphViewer graphViewer)
		{
			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);


			this.baseNode = baseNode;
			myGraphViewer = graphViewer;
			fixedTextBox.Text = Convert.ToString(baseNode.DesiredRate);

			if (this.baseNode.RateType == RateType.Auto)
			{
				autoOption.Checked = true;
				fixedTextBox.Enabled = false;
			}
			else
			{
				fixedOption.Checked = true;
				fixedTextBox.Enabled = true;
			}
		}

		private void SetFixedRate()
		{
			if (float.TryParse(fixedTextBox.Text, out float newAmount))
			{
				if (baseNode is RecipeNode rNode)
					rNode.SetBaseNumberOfAssemblers(newAmount / myGraphViewer.GetRateMultipler());
				else
					baseNode.DesiredRate = newAmount;

				myGraphViewer.Graph.UpdateNodeValues();
			}
		}

		private void fixedOption_CheckedChanged(object sender, EventArgs e)
		{
			fixedTextBox.Enabled = fixedOption.Checked;
			RateType updatedRateType = (fixedOption.Checked) ? RateType.Manual : RateType.Auto;

			if (baseNode.RateType != updatedRateType)
			{
				baseNode.RateType = updatedRateType;
				myGraphViewer.Graph.UpdateNodeValues();
			}
		}

		private void fixedTextBox_LostFocus(object sender, EventArgs e)
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
