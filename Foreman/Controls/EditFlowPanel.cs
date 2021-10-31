using System;
using System.Windows.Forms;

namespace Foreman
{
	public partial class EditFlowPanel : UserControl
	{
		private ProductionGraphViewer myGraphViewer;
		private BaseNode BaseNode;

		public EditFlowPanel(BaseNode baseNode, ProductionGraphViewer graphViewer)
		{
			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			FixedFlowInput.Maximum = (decimal)ProductionGraph.MaxSetFlow;

			this.BaseNode = baseNode;
			myGraphViewer = graphViewer;
			RateLabel.Text = string.Format("Item Flowrate (per {0})", myGraphViewer.GetRateName());

			InitializeRates();
		}

		private void InitializeRates()
		{
			if (BaseNode.RateType == RateType.Auto)
			{
				AutoOption.Checked = true;
				FixedFlowInput.Enabled = false;
				FixedFlowInput.Value = Math.Min(FixedFlowInput.Maximum, (decimal)BaseNode.ActualRate);
			}
			else
			{
				FixedOption.Checked = true;
				FixedFlowInput.Enabled = true;
				FixedFlowInput.Value = Math.Min(FixedFlowInput.Maximum, (decimal)BaseNode.DesiredRate);
			}
			UpdateFixedFlowInputDecimals(FixedFlowInput);
		}

		private void SetFixedRate()
		{
			if (BaseNode.DesiredRate != (double)FixedFlowInput.Value)
			{
				BaseNode.DesiredRate = (double)FixedFlowInput.Value;
				myGraphViewer.Graph.UpdateNodeValues();
			}
			UpdateFixedFlowInputDecimals(FixedFlowInput);
		}

		private void UpdateFixedFlowInputDecimals(NumericUpDown nud)
		{
			int decimals = MathDecimals.GetDecimals(nud.Value);
			decimals = Math.Min(decimals, 4);
			nud.DecimalPlaces = decimals;
		}

		private void FixedOption_CheckChanged(object sender, EventArgs e)
		{
			FixedFlowInput.Enabled = FixedOption.Checked;
			RateType updatedRateType = (FixedOption.Checked) ? RateType.Manual : RateType.Auto;

			if (BaseNode.RateType != updatedRateType)
			{
				BaseNode.RateType = updatedRateType;
				myGraphViewer.Graph.UpdateNodeValues();
			}
		}

		private void FixedFlowInput_ValueChanged(object sender, EventArgs e)
		{
			SetFixedRate();
		}
	}
}
