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
        public bool RecipeOriginallyEnabled;
        private Bitmap ColorIcon;
        private Bitmap GrayIcon;

        public RecipeChooserControl(Recipe recipe, String text, String filterText) : base(text, filterText)
		{
			InitializeComponent();
			DisplayedRecipe = recipe;
            RecipeOriginallyEnabled = recipe.Enabled;

            ColorIcon = recipe.Icon;
            GrayIcon = GraphicsStuff.MakeMonochrome(recipe.Icon);

            setClickHandler(new MouseEventHandler(RecipeChooserControl_MouseUp), this);
		}

        internal override void UpdateIconSize(int iconSize)
        {
            RecipeLayoutPanel.ColumnStyles[0].Width = iconSize;
            iconPictureBox.Size = new Size(iconSize, iconSize);
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
				inputListBox.Items.Add(String.Format("{0} ({1})", ingredient.FriendlyName, DisplayedRecipe.Ingredients[ingredient]));
			}
			foreach (Item result in DisplayedRecipe.Results.Keys)
			{
				outputListBox.Items.Add(String.Format("{0} ({1})", result.FriendlyName, DisplayedRecipe.Results[result]));
			}
            iconPictureBox.Image = DisplayedRecipe.Icon;
			iconPictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            fakeDisable(DisplayedRecipe.Enabled && DisplayedRecipe.HasEnabledAssemblers);

			RegisterMouseEvents(this);
		}


        private void RecipeChooserControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DisplayedRecipe.Enabled = !DisplayedRecipe.Enabled;
                fakeDisable(DisplayedRecipe.Enabled && DisplayedRecipe.HasEnabledAssemblers);
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

            if(ParentForm is MainForm mForm)
                mForm.UpdateRecipeListItemCheckmark(DisplayedRecipe);

            this.Invalidate();
        }
    }
}
