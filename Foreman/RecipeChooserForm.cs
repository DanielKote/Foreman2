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
	public partial class RecipeChooserForm : Form
	{
		private Item itemToChooseFor;
		public UserControl selectedControl = null;
		public UserControl SelectedControl
		{
			get
			{
				return selectedControl;
			}
			set
			{
				if (selectedControl != null)
				{
					selectedControl.BackColor = Color.White;
				}
				selectedControl = value;
				if (value != null)
				{
					selectedControl.BackColor = Color.FromArgb(0xFF, 0xAE, 0xC6, 0xCF);
				}
			}
		}
		public Recipe selectedRecipe
		{
			get
			{
				if (selectedControl is RecipeChooserControl)
				{
					return (selectedControl as RecipeChooserControl).DisplayedRecipe;
				}
				else
				{
					return null;
				}
			}
		}

		public RecipeChooserForm(Item item)
		{
			InitializeComponent();

			itemToChooseFor = item;
		}

		private void RecipeChooserForm_Load(object sender, EventArgs e)
		{
			recipeListPanel.Controls.Add(new SupplyNodeChooserControl(itemToChooseFor));
			foreach (Recipe recipe in itemToChooseFor.Recipes)
			{
				recipeListPanel.Controls.Add(new RecipeChooserControl(recipe));
			}
		}

		private void RegisterKeyEvents(Control control)
		{
			control.KeyDown += RecipeChooserForm_KeyDown;

			foreach (Control subControl in control.Controls)
			{
				RegisterKeyEvents(subControl);
			}
		}

		private void RecipeChooserForm_MouseMove(object sender, MouseEventArgs e)
		{
			SelectedControl = null;
		}

		private void RecipeChooserForm_MouseLeave(object sender, EventArgs e)
		{
			SelectedControl = null;
		}

		private void RecipeChooserForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}
	}
}
