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
		private List<Item> items;
		private List<Recipe> recipes;
		private String itemText = "";
		private String recipeText = "";

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
		public Item selectedItem
		{
			get
			{
				if (selectedControl is ItemChooserControl)
				{
					return (selectedControl as ItemChooserControl).DisplayedItem;
				}
				else
				{
					return null;
				}
			}
		}

		public RecipeChooserForm(IEnumerable<Recipe> recipes, IEnumerable<Item> items, String itemText, String recipeText)
		{
			InitializeComponent();

			this.recipes = recipes.ToList();
			this.items = items.ToList();
			this.itemText = itemText;
			this.recipeText = recipeText;
		}

		private void RecipeChooserForm_Load(object sender, EventArgs e)
		{
			foreach (Item item in items)
			{
				recipeListPanel.Controls.Add(new ItemChooserControl(item, itemText));
			}
			foreach (Recipe recipe in recipes)
			{
				recipeListPanel.Controls.Add(new RecipeChooserControl(recipe, recipeText));
			}

			MaximumSize = new Size(Int32.MaxValue, 500);
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
