using System;
using System.Linq;
using System.Windows.Forms;

namespace Foreman
{
	public partial class EditFlowPanel : UserControl
	{
		private static readonly char[] ExtraChars = new char[] { '.', ',' };

		private ProductionGraphViewer myGraphViewer;
		private BaseNode baseNode;

		public EditFlowPanel(BaseNode baseNode, ProductionGraphViewer graphViewer)
		{
			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

			this.baseNode = baseNode;
			myGraphViewer = graphViewer;
			RateGroup.Text = string.Format("Item Flowrate (per {0})", myGraphViewer.GetRateName());
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
				if (newAmount > ProductionGraph.MaxSetFlow)
					newAmount = ProductionGraph.MaxSetFlow;
				if (baseNode.DesiredRate != newAmount)
				{
					baseNode.DesiredRate = newAmount;
					myGraphViewer.Graph.UpdateNodeValues();
				}
			}
			fixedTextBox.Text = baseNode.DesiredRate.ToString();
			fixedTextBox.SelectionStart = fixedTextBox.Text.Length;
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

		private void fixedTextBox_TextChanged(object sender, EventArgs e)
		{
			int i = fixedTextBox.SelectionStart;
			string filteredText = string.Concat(fixedTextBox.Text.Where(c => char.IsDigit(c) || ExtraChars.Contains(c)));
			if (filteredText != fixedTextBox.Text)
			{
				i = Math.Max(i + filteredText.Length - fixedTextBox.Text.Length, 0);
				fixedTextBox.Text = filteredText;
				fixedTextBox.SelectionStart = i;
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
