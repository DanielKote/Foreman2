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

			listBox1.Items.Clear();
			listBox1.Items.AddRange(DataCache.Items.Keys.ToArray());
			listBox1.Sorted = true;
			

		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			ProductionGraph.AddDemand(DataCache.Items[listBox1.SelectedItem.ToString()]);
		}

		private void ItemListForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		}
	}
}
