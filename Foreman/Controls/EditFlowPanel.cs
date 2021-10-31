using System;
using System.Windows.Forms;

namespace Foreman
{
	public partial class EditFlowPanel : UserControl
	{
		private ProductionGraphViewer myGraphViewer;
		private BaseNode baseNode;

		public EditFlowPanel(BaseNode baseNode, ProductionGraphViewer graphViewer)
		{
			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			FixedFlowInput.Maximum = (decimal)ProductionGraph.MaxSetFlow;

			this.baseNode = baseNode;
			myGraphViewer = graphViewer;
			RateLabel.Text = string.Format("Item Flowrate (per {0})", myGraphViewer.GetRateName());
			FixedFlowInput.Value = Math.Round((decimal)baseNode.DesiredRate, 4);
			UpdateFixedFlowInputDecimals(FixedFlowInput);

			AutoOption.Checked = (this.baseNode.RateType == RateType.Auto);
			FixedFlowInput.Enabled = (this.baseNode.RateType != RateType.Auto);
		}

		private void SetFixedRate()
		{
			if (baseNode.DesiredRate != (double)FixedFlowInput.Value)
			{
				baseNode.DesiredRate = (double)FixedFlowInput.Value;
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

			if (baseNode.RateType != updatedRateType)
			{
				baseNode.RateType = updatedRateType;
				myGraphViewer.Graph.UpdateNodeValues();
			}
		}

		private void FixedFlowInput_ValueChanged(object sender, EventArgs e)
		{
			SetFixedRate();
		}
	}
}
