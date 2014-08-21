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
			ItemListBox.Items.AddRange(DataCache.Items.Keys.ToArray());
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
			List<Item> selectedItems = new List<Item>();

			foreach (String itemName in ItemListBox.SelectedItems)
			{
				selectedItems.Add(DataCache.Items[itemName]);
			}
			ProductionGraph.AddDemands(selectedItems);
		}

		private void RemoveNodeButton_Click(object sender, EventArgs e)
		{
			if (ProductionGraph.SelectedNode != null)
			{
				ProductionGraph.SelectedNode.DisplayedNode.Destroy();
				ProductionGraph.nodeControls.Remove(ProductionGraph.SelectedNode.DisplayedNode);
				ProductionGraph.graph.UpdateNodeAmounts();
				ProductionGraph.Invalidate();
			}
		}

		private void rateButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.ProductionGraph.graph.SelectedAmountType = AmountType.Rate;
				rateOptionsDropDown.Enabled = true;
			}
			else
			{
				rateOptionsDropDown.Enabled = false;
			}
			ProductionGraph.Invalidate();
		}

		private void fixedAmountButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.ProductionGraph.graph.SelectedAmountType = AmountType.FixedAmount;
			}
			ProductionGraph.Invalidate();
		}

		private void rateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((sender as ComboBox).SelectedIndex)
			{
				case 0:
					ProductionGraph.graph.SelectedUnit = RateUnit.PerSecond;
					ProductionGraph.Invalidate();
					break;
				case 1:
					ProductionGraph.graph.SelectedUnit = RateUnit.PerMinute;
					ProductionGraph.Invalidate();
					break;
			}
		}
	}
}
