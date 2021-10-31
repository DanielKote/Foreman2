using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public partial class SciencePacksLoadForm : Form
	{
		private Dictionary<Button, bool> SciencePackButtons;
		private static Color EnabledPackBGColor = Color.DarkGreen;
		private static Color DisabledPackBGColor = Color.DarkRed;
		private int IconSize = 48; //actually a bit smaller due to button padding, but whatever.

		private readonly DataCache DCache;

		public SciencePacksLoadForm(DataCache cache)
		{
			DCache = cache;

			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SciencePackTable.RowStyles[0].SizeType = SizeType.Absolute;
			SciencePackTable.RowStyles[0].Height = IconSize;

			SciencePackButtons = new Dictionary<Button, bool>();

			PopulateSciencePackOptions();
		}

		private void PopulateSciencePackOptions()
		{
			SciencePackTable.Height = IconSize;
			foreach(Item sciencePack in DCache.SciencePacks)
			{
				NFButton button = new NFButton();
				button.BackColor = DisabledPackBGColor;
				button.ForeColor = Color.Gray;
				button.BackgroundImageLayout = ImageLayout.Zoom;
				button.BackgroundImage = sciencePack.Icon;
				button.UseVisualStyleBackColor = false;
				button.FlatStyle = FlatStyle.Flat;
				button.FlatAppearance.BorderSize = 0;
				button.FlatAppearance.BorderColor = Color.Black;
				button.TabStop = false;
				button.Margin = new Padding(0);
				button.Size = new Size(1, 1);
				button.Dock = DockStyle.Fill;
				button.Tag = sciencePack;
				button.Enabled = true;

				button.MouseHover += new EventHandler(Button_MouseHover);
				button.MouseLeave += new EventHandler(Button_MouseLeave);
				button.Click += new EventHandler(Button_Click);

				SciencePackTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, IconSize));
				SciencePackTable.ColumnCount = SciencePackTable.ColumnStyles.Count;
				SciencePackTable.Controls.Add(button, SciencePackTable.ColumnStyles.Count - 1, 0);
				SciencePackButtons.Add(button, false);
			}

		}

		private void Button_Click(object sender, EventArgs e)
		{
			Button sciPackButton = (Button)sender;
			SciencePackButtons[sciPackButton] = !SciencePackButtons[sciPackButton];
			sciPackButton.BackColor = SciencePackButtons[sciPackButton] ? EnabledPackBGColor : DisabledPackBGColor;
		}

		//------------------------------------------------------------------------------------------------------Button hovers

		private void Button_MouseHover(object sender, EventArgs e)
		{
			Control control = (Control)sender;
			if (control.Tag is DataObjectBase dob)
			{
				ToolTip.SetText(dob.FriendlyName);
				ToolTip.Show(this, Point.Add(PointToClient(Control.MousePosition), new Size(15, 5)));
			}
		}

		private void Button_MouseLeave(object sender, EventArgs e)
		{
			ToolTip.Hide((Control)sender);
		}

		private void ConfirmationButton_Click(object sender, EventArgs e)
		{
			HashSet<Item> acceptedSciencePacks = new HashSet<Item>(SciencePackButtons.Where(kvp => kvp.Value).Select(kvp => kvp.Key.Tag as Item));

			//go through all technologies, check for fit compared to accepted science packs, and enable if it conforms
			foreach (Technology tech in DCache.Technologies.Values)
			{
				tech.Enabled = tech.Available && !tech.SciPackList.Except(acceptedSciencePacks).Any();
				if (tech.Enabled) Console.WriteLine(tech);
			}

			//disable all recipes, then enable only those recipes that are enabled by the enabled technologies
			foreach (Recipe recipe in DCache.Recipes.Values)
				recipe.Enabled = false;
			foreach (Technology tech in DCache.Technologies.Values.Where(t => t.Enabled))
				foreach (Recipe recipe in tech.UnlockedRecipes)
					recipe.Enabled = recipe.Available;

			//update enabled status of assemblers, beacons, and modules based on the enabled status of their items' production recipes
			foreach (Assembler assembler in DCache.Assemblers.Values)
			{
				bool enabled = false;
				foreach (IReadOnlyCollection<Recipe> recipes in assembler.AssociatedItems.Select(item => item.ProductionRecipes))
					foreach (Recipe recipe in recipes)
						enabled |= recipe.Enabled;
				assembler.Enabled = enabled;
			}
			DCache.PlayerAssembler.Enabled = true;

			foreach (Beacon beacon in DCache.Beacons.Values)
			{
				bool enabled = false;
				foreach (IReadOnlyCollection<Recipe> recipes in beacon.AssociatedItems.Select(item => item.ProductionRecipes))
					foreach (Recipe recipe in recipes)
						enabled |= recipe.Enabled;
				beacon.Enabled = enabled;
			}

			foreach (Module module in DCache.Modules.Values)
			{
				bool enabled = false;
				foreach (Recipe recipe in module.AssociatedItem.ProductionRecipes)
					enabled |= recipe.Enabled;
				module.Enabled = enabled;
			}

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
