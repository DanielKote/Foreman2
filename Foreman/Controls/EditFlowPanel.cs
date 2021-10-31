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
			RateLabel.Text = string.Format("Item Flowrate (per {0})", myGraphViewer.GetRateName());
			FixedItemFlowInput.Text = Convert.ToString(baseNode.DesiredRate);

			if (this.baseNode.RateType == RateType.Auto)
			{
				AutoOption.Checked = true;
				FixedItemFlowInput.Enabled = false;
			}
			else
			{
				FixedOption.Checked = true;
				FixedItemFlowInput.Enabled = true;
			}
		}

		private void SetFixedRate()
		{
			if (float.TryParse(FixedItemFlowInput.Text, out float newAmount))
			{
				if (newAmount > ProductionGraph.MaxSetFlow)
					newAmount = ProductionGraph.MaxSetFlow;
				if (baseNode.DesiredRate != newAmount)
				{
					baseNode.DesiredRate = newAmount;
					myGraphViewer.Graph.UpdateNodeValues();
				}
			}
			FixedItemFlowInput.Text = baseNode.DesiredRate.ToString();
			FixedItemFlowInput.SelectionStart = FixedItemFlowInput.Text.Length;
		}

		private void FixedOption_CheckChanged(object sender, EventArgs e)
		{
			FixedItemFlowInput.Enabled = FixedOption.Checked;
			RateType updatedRateType = (FixedOption.Checked) ? RateType.Manual : RateType.Auto;

			if (baseNode.RateType != updatedRateType)
			{
				baseNode.RateType = updatedRateType;
				myGraphViewer.Graph.UpdateNodeValues();
			}
		}

		private void FixedItemFlowInput_TextChanged(object sender, EventArgs e)
		{
			int i = FixedItemFlowInput.SelectionStart;
			string filteredText = string.Concat(FixedItemFlowInput.Text.Where(c => char.IsDigit(c) || ExtraChars.Contains(c)));
			if (filteredText != FixedItemFlowInput.Text)
			{
				i = Math.Max(i + filteredText.Length - FixedItemFlowInput.Text.Length, 0);
				FixedItemFlowInput.Text = filteredText;
				FixedItemFlowInput.SelectionStart = i;
			}
		}

		private void FixedItemFlowInput_LostFocus(object sender, EventArgs e)
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
