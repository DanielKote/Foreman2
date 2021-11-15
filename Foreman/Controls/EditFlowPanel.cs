using System;
using System.Windows.Forms;

namespace Foreman
{
	public partial class EditFlowPanel : UserControl
	{
		private readonly ProductionGraphViewer myGraphViewer;
		private readonly BaseNodeController nodeController;
		private readonly ReadOnlyBaseNode nodeData;

		public EditFlowPanel(ReadOnlyBaseNode node, ProductionGraphViewer graphViewer)
		{
			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

			nodeData = node;
			nodeController = graphViewer.Graph.RequestNodeController(node);
			myGraphViewer = graphViewer;

			RateLabel.Text = string.Format("Item Flowrate (per {0})", myGraphViewer.Graph.GetRateName());
			FixedFlowInput.Maximum = (decimal)(ProductionGraph.MaxSetFlow * myGraphViewer.Graph.GetRateMultipler());

			if(node is ReadOnlyPassthroughNode pNode)
			{
				SimplePassthroughNodesCheckBox.Checked = pNode.SimpleDraw;
				SimplePassthroughNodesCheckBox.Visible = true;
			}

			KeyNodeCheckBox.Checked = nodeData.KeyNode;
			KeyNodeTitleLabel.Visible = nodeData.KeyNode;
			KeyNodeTitleInput.Visible = nodeData.KeyNode;
			KeyNodeTitleInput.Text = nodeData.KeyNodeTitle;

			InitializeRates();

			SimplePassthroughNodesCheckBox.CheckedChanged += SimplePassthroughNodesCheckBox_CheckedChanged;
			KeyNodeCheckBox.CheckedChanged += KeyNodeCheckBox_CheckedChanged;
			KeyNodeTitleInput.TextChanged += KeyNodeTitleInput_TextChanged;
		}

		private void InitializeRates()
		{
			if (nodeData.RateType == RateType.Auto)
			{
				AutoOption.Checked = true;
				FixedFlowInput.Enabled = false;
				FixedFlowInput.Value = Math.Min(FixedFlowInput.Maximum, (decimal)nodeData.ActualRate);
			}
			else
			{
				FixedOption.Checked = true;
				FixedFlowInput.Enabled = true;
				FixedFlowInput.Value = Math.Min(FixedFlowInput.Maximum, (decimal)nodeData.DesiredRate);
			}
			UpdateFixedFlowInputDecimals(FixedFlowInput);
		}

		private void SetFixedRate()
		{
			if (nodeData.DesiredRate != (double)FixedFlowInput.Value)
			{
				nodeController.SetDesiredRate((double)FixedFlowInput.Value);
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

			if (nodeData.RateType != updatedRateType)
			{
				nodeController.SetRateType(updatedRateType);
				myGraphViewer.Graph.UpdateNodeValues();
			}
		}

		private void FixedFlowInput_ValueChanged(object sender, EventArgs e)
		{
			SetFixedRate();
		}

		private void SimplePassthroughNodesCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			(nodeController as PassthroughNodeController).SetSimpleDraw(SimplePassthroughNodesCheckBox.Checked);
			myGraphViewer.Invalidate();
		}

		private void KeyNodeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			nodeController.SetKeyNode(KeyNodeCheckBox.Checked);
			KeyNodeTitleLabel.Visible = nodeData.KeyNode;
			KeyNodeTitleInput.Visible = nodeData.KeyNode;
			KeyNodeTitleInput.Text = nodeData.KeyNodeTitle;
			myGraphViewer.Invalidate();
		}

		private void KeyNodeTitleInput_TextChanged(object sender, EventArgs e)
		{
			nodeController.SetKeyNodeTitle(KeyNodeTitleInput.Text);
		}
	}
}
