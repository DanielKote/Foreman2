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

			float amountToShow = baseNode.manualRate;
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
				fixedTextBox.Width = 85;
			}
			fixedTextBox.Text = Convert.ToString(amountToShow);
		}

		private void fixedOption_CheckedChanged(object sender, EventArgs e)
		{
			fixedTextBox.Enabled = fixedOption.Checked;
			if (fixedOption.Checked)
			{
				BaseNode.rateType = RateType.Manual;
			}
			else
			{
				BaseNode.rateType = RateType.Auto;
			}
			GraphViewer.UpdateNodes();
		}

		private void fixedTextBox_TextChanged(object sender, EventArgs e)
		{
			float newAmount;
			if (float.TryParse(fixedTextBox.Text, out newAmount))
			{
				if (GraphViewer.Graph.SelectedAmountType == AmountType.Rate && GraphViewer.Graph.SelectedUnit == RateUnit.PerMinute)
				{
					newAmount /= 60;
				}
				BaseNode.manualRate = newAmount;
				GraphViewer.UpdateNodes();
			}
		}

		private void KeyPressed(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				GraphViewer.ClearFloatingControls();
				GraphViewer.UpdateNodes();
			}
		}
	}
}
