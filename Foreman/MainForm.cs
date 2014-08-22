using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{	
	public partial class MainForm : Form
	{

		public MainForm()
		{
			InitializeComponent();
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			DataCache.LoadRecipes();

			rateOptionsDropDown.SelectedIndex = 0;

			ItemListBox.Items.Clear();
			ItemListBox.Items.AddRange(DataCache.Items.Values.ToArray());
			ItemListBox.DisplayMember = "FriendlyName";
			ItemListBox.Sorted = true;
		}

		private void ItemListForm_KeyDown(object sender, KeyEventArgs e)
		{
#if DEBUG
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
#endif
		}

		private void AddItemButton_Click(object sender, EventArgs e)
		{
			GraphViewer.AddDemands(ItemListBox.SelectedItems.Cast<Item>());
		}

		private void RemoveNodeButton_Click(object sender, EventArgs e)
		{
			GraphViewer.DeleteNode(GraphViewer.SelectedNode);
		}

		private void rateButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.graph.SelectedAmountType = AmountType.Rate;
				rateOptionsDropDown.Enabled = true;
			}
			else
			{
				rateOptionsDropDown.Enabled = false;
			}
			GraphViewer.Invalidate();
		}

		private void fixedAmountButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.graph.SelectedAmountType = AmountType.FixedAmount;
			}
			GraphViewer.Invalidate();
		}

		private void rateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((sender as ComboBox).SelectedIndex)
			{
				case 0:
					GraphViewer.graph.SelectedUnit = RateUnit.PerSecond;
					GraphViewer.Invalidate();
					break;
				case 1:
					GraphViewer.graph.SelectedUnit = RateUnit.PerMinute;
					GraphViewer.Invalidate();
					break;
			}
		}

		private void AutomaticCompleteButton_Click(object sender, EventArgs e)
		{
			GraphViewer.graph.SatisfyAllItemDemands();
			GraphViewer.CreateMissingControls();
		}

		private void ClearButton_Click(object sender, EventArgs e)
		{
			GraphViewer.graph.Nodes.Clear();
			GraphViewer.nodeControls.Clear();
			GraphViewer.Invalidate();
		}

		private void ItemListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ItemListBox.SelectedItems.Count == 0)
			{
				AddItemButton.Enabled = false;
			}
			else if (ItemListBox.SelectedItems.Count == 1)
			{
				AddItemButton.Enabled = true;
				AddItemButton.Text = "Add Output";
			}
			else if (ItemListBox.SelectedItems.Count > 1)
			{
				AddItemButton.Enabled = true;
				AddItemButton.Text = "Add Outputs";
			}
		}

		private void RemoveUnusedButton_Click(object sender, EventArgs e)
		{
			foreach (ProductionNode node in GraphViewer.graph.GetTopologicalSort().Reverse<ProductionNode>())
			{
				if (node.Outputs.All(i => node.GetTotalOutput(i) == 0))
				{
					node.Destroy();
					GraphViewer.nodeControls.Remove(node);
					GraphViewer.Invalidate();
				}
			}
		}
	}
}
