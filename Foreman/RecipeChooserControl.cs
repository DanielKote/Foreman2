using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{
	public partial class RecipeChooserControl : ChooserControl
	{
		public Recipe DisplayedRecipe;

		public RecipeChooserControl(Recipe recipe, String text, String filterText) : base(text, filterText)
		{
			InitializeComponent();
			DisplayedRecipe = recipe;
		}

		private void RecipeChooserOption_Load(object sender, EventArgs e)
		{
			nameLabel.Text = String.Format(DisplayText, DisplayedRecipe.FriendlyName);
			foreach (Item ingredient in DisplayedRecipe.Ingredients.Keys)
			{
				inputListBox.Items.Add(String.Format("{0} ({1})", ingredient.FriendlyName, DisplayedRecipe.Ingredients[ingredient]));
			}
			foreach (Item result in DisplayedRecipe.Results.Keys)
			{
				outputListBox.Items.Add(String.Format("{0} ({1})", result.FriendlyName, DisplayedRecipe.Results[result]));
			}
			iconPictureBox.Image = DisplayedRecipe.Icon;
			iconPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

			RegisterMouseEvents(this);
		}
	}
}
