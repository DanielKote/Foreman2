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
		
		private void rateButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.Graph.SelectedAmountType = AmountType.Rate;
				rateOptionsDropDown.Enabled = true;
			}
			else
			{
				rateOptionsDropDown.Enabled = false;
			}
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
		}

		private void fixedAmountButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.Graph.SelectedAmountType = AmountType.FixedAmount;
				AssemblerDisplayCheckBox.Checked = false;
				AssemblerDisplayCheckBox.Enabled = false;
			}
			else
			{
				AssemblerDisplayCheckBox.Checked = true;
				AssemblerDisplayCheckBox.Enabled = true;
			}
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
		}

		private void rateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((sender as ComboBox).SelectedIndex)
			{
				case 0:
					GraphViewer.Graph.SelectedUnit = RateUnit.PerSecond;
					GraphViewer.Invalidate();
					GraphViewer.UpdateNodes();
					break;
				case 1:
					GraphViewer.Graph.SelectedUnit = RateUnit.PerMinute;
					GraphViewer.Invalidate();
					GraphViewer.UpdateNodes();
					break;
			}
		}

		private void AutomaticCompleteButton_Click(object sender, EventArgs e)
		{
			GraphViewer.Graph.SatisfyAllItemDemands();
			GraphViewer.AddRemoveElements();
			GraphViewer.PositionNodes();
		}

		private void ClearButton_Click(object sender, EventArgs e)
		{
			GraphViewer.Graph.Nodes.Clear();
			GraphViewer.Elements.Clear();
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
			foreach (ProductionNode node in GraphViewer.Graph.GetTopologicalSort().Reverse<ProductionNode>())
			{
				if (node.Outputs.All(i => node.GetUsedOutput(i) == 0))
				{
					GraphViewer.DeleteNode(GraphViewer.GetElementForNode(node));
				}
			}
		}

		private void AssemblerDisplayCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.ShowAssemblers = (sender as CheckBox).Checked;
			GraphViewer.UpdateNodes();
		}

		private void SingleAssemblerPerRecipeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.Graph.OneAssemblerPerRecipe = (sender as CheckBox).Checked;
			GraphViewer.UpdateNodes();
		}
	}
}
