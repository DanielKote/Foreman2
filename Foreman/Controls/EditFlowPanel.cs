using System;
using System.Windows.Forms;

namespace Foreman {
    public partial class EditFlowPanel : UserControl {
        private readonly ProductionGraphViewer _myGraphViewer;
        private readonly BaseNodeController _nodeController;
        private readonly ReadOnlyBaseNode _nodeData;

        public EditFlowPanel(ReadOnlyBaseNode node, ProductionGraphViewer graphViewer) {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            _nodeData = node;
            _nodeController = graphViewer.Graph.RequestNodeController(node);
            _myGraphViewer = graphViewer;

            RateLabel.Text = node.SetValueDescription;
            FixedFlowInput.Maximum = (decimal) (node.MaxDesiredSetValue * graphViewer.Graph.GetRateMultiplier());

            if (node is ReadOnlyPassthroughNode pNode) {
                SimplePassthroughNodesCheckBox.Checked = pNode.SimpleDraw;
                SimplePassthroughNodesCheckBox.Visible = true;
            }

            KeyNodeCheckBox.Checked = _nodeData.KeyNode;
            KeyNodeTitleLabel.Visible = _nodeData.KeyNode;
            KeyNodeTitleInput.Visible = _nodeData.KeyNode;
            KeyNodeTitleInput.Text = _nodeData.KeyNodeTitle;

            InitializeRates();

            SimplePassthroughNodesCheckBox.CheckedChanged += SimplePassthroughNodesCheckBox_CheckedChanged;
            KeyNodeCheckBox.CheckedChanged += KeyNodeCheckBox_CheckedChanged;
            KeyNodeTitleInput.TextChanged += KeyNodeTitleInput_TextChanged;
        }

        private void InitializeRates() {
            if (_nodeData.RateType == RateType.Auto) {
                AutoOption.Checked = true;
                FixedFlowInput.Enabled = false;
                FixedFlowInput.Value = Math.Min(FixedFlowInput.Maximum, (decimal) _nodeData.ActualSetValue);
            } else {
                FixedOption.Checked = true;
                FixedFlowInput.Enabled = true;
                FixedFlowInput.Value = Math.Min(FixedFlowInput.Maximum, (decimal) _nodeData.DesiredSetValue);
            }

            UpdateFixedFlowInputDecimals(FixedFlowInput);
        }

        private void SetFixedRate() {
            if (Math.Abs(_nodeData.DesiredSetValue - (double) FixedFlowInput.Value) > double.Epsilon) {
                _nodeController.SetDesiredSetValue((double) FixedFlowInput.Value);
                _myGraphViewer.Graph.UpdateNodeValues();
            }

            UpdateFixedFlowInputDecimals(FixedFlowInput);
        }

        private void UpdateFixedFlowInputDecimals(NumericUpDown nud) {
            var decimals = MathDecimals.GetDecimals(nud.Value);
            decimals = Math.Min(decimals, 4);
            nud.DecimalPlaces = decimals;
        }

        private void FixedOption_CheckChanged(object sender, EventArgs e) {
            FixedFlowInput.Enabled = FixedOption.Checked;
            var updatedRateType = FixedOption.Checked ? RateType.Manual : RateType.Auto;

            if (_nodeData.RateType == updatedRateType)
                return;

            _nodeController.SetRateType(updatedRateType);
            _myGraphViewer.Graph.UpdateNodeValues();
        }

        private void FixedFlowInput_ValueChanged(object sender, EventArgs e) {
            SetFixedRate();
        }

        private void SimplePassthroughNodesCheckBox_CheckedChanged(object sender, EventArgs e) {
            (_nodeController as PassthroughNodeController).SetSimpleDraw(SimplePassthroughNodesCheckBox.Checked);
            _myGraphViewer.Invalidate();
        }

        private void KeyNodeCheckBox_CheckedChanged(object sender, EventArgs e) {
            _nodeController.SetKeyNode(KeyNodeCheckBox.Checked);
            KeyNodeTitleLabel.Visible = _nodeData.KeyNode;
            KeyNodeTitleInput.Visible = _nodeData.KeyNode;
            KeyNodeTitleInput.Text = _nodeData.KeyNodeTitle;
            _myGraphViewer.Invalidate();
        }

        private void KeyNodeTitleInput_TextChanged(object sender, EventArgs e) {
            _nodeController.SetKeyNodeTitle(KeyNodeTitleInput.Text);
        }
    }
}