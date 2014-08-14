using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Foreman
{
	public partial class RecipeNodeViewer : ProductionNodeViewer
	{
		public RecipeNodeViewer()
		{
			InitializeComponent();
		}

		public Recipe DisplayedRecipe
		{
			get
			{
				return (displayedNode as RecipeNode).BaseRecipe;
			}
		}

		private void NameBox_TextChanged(object sender, EventArgs e)
		{

		}

		private void label1_Click(object sender, EventArgs e)
		{

		}

		private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
		{

		}

		private void RateLabel_Click(object sender, EventArgs e)
		{

		}

		private void RateTextBox_TextChanged_1(object sender, EventArgs e)
		{

		}
	}
}
