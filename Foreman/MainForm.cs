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

			ItemListBox.Items.Clear();
			ItemListBox.Items.AddRange(DataCache.Items.Keys.ToArray());
			ItemListBox.Sorted = true;
		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void ItemListForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
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
	}
}
