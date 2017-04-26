using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Foreman
{
	public partial class RecipeChooserControl : ChooserControl
	{
		public Recipe DisplayedRecipe;
        private Bitmap ColorIcon;
        private Bitmap GrayIcon;

		public RecipeChooserControl(Recipe recipe, String text, String filterText) : base(text, filterText)
		{
			InitializeComponent();
			DisplayedRecipe = recipe;

            ColorIcon = recipe.Icon;
            GrayIcon = GraphicsStuff.MakeMonochrome(recipe.Icon);

            setClickHandler(new MouseEventHandler(RecipeChooserControl_MouseUp), this);
            
		}

        private static void setClickHandler(MouseEventHandler h, Control c)
        {
            c.MouseUp += h;
            foreach (Control child in c.Controls)
            {
                setClickHandler(h, child);
            }
        }

        

        private void RecipeChooserOption_Load(object sender, EventArgs e)
		{
			nameLabel.Text = String.Format(DisplayText, DisplayedRecipe.FriendlyName);
			foreach (Item ingredient in DisplayedRecipe.Ingredients.Keys)
			{
				inputListBox.Items.Add($"{ingredient.FriendlyName} ({DisplayedRecipe.Ingredients[ingredient]})");
			}
			foreach (Item result in DisplayedRecipe.Results.Keys)
			{
				outputListBox.Items.Add($"{result.FriendlyName} ({DisplayedRecipe.Results[result]})");
			}
            iconPictureBox.Image = DisplayedRecipe.Icon;
			iconPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;

            fakeDisable(DisplayedRecipe.Enabled);

			RegisterMouseEvents(this);
		}


        private void RecipeChooserControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DisplayedRecipe.Enabled = !DisplayedRecipe.Enabled;
                fakeDisable(DisplayedRecipe.Enabled);
            }
        }

        // when Enabled is false, change appearance to that of a disabled control, without actually disabling.
        private void fakeDisable(bool enabled)
        {
            Color newListboxColor = (enabled ? SystemColors.WindowText : SystemColors.GrayText);
            Color newTextboxColor = (enabled ? SystemColors.WindowText : SystemColors.GrayText);

            nameLabel.ForeColor = newTextboxColor;
            inputListBox.ForeColor = newListboxColor;
            outputListBox.ForeColor = newListboxColor;

            iconPictureBox.Image = (enabled ? ColorIcon : GrayIcon);

            this.Invalidate();

        }



    }
}
