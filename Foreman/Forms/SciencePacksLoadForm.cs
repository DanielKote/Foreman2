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
		private readonly HashSet<DataObjectBase> EnabledObjects;

		public SciencePacksLoadForm(DataCache cache, HashSet<DataObjectBase> enabledObjects)
		{
			DCache = cache;
			EnabledObjects = enabledObjects;

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
			bool enabled = !SciencePackButtons[sciPackButton];
			SciencePackButtons[sciPackButton] = enabled;
			sciPackButton.BackColor = enabled ? EnabledPackBGColor : DisabledPackBGColor;

			//NOTE: this is a bit wrong and can fail if there are multiple ways of getting to a given science pack (that are on different tech tree groups); ex: T3 science that can be researched either with T1A science or with T1B science (as this will consider it requiring both T1A AND T1B instead of T1A OR T1B)
			//but this situation should ideally never happen -> I dont know any mod that allows this sort of tech tree (wouldnt it be extremely confusing to have 2+ widely different techs that can grant you a science pack???)

			if (enabled) //enable all the science packs that are required to make the clicked science pack
			{
				HashSet<Item> requiredSciPacks = new HashSet<Item>();
				foreach (Recipe r in (sciPackButton.Tag as Item).ProductionRecipes)
					foreach (Technology t in r.MyUnlockTechnologies)
						requiredSciPacks.UnionWith(t.SciPackList);
				foreach (Button sciButton in SciencePackButtons.Keys.ToArray())
				{
					if (requiredSciPacks.Contains(sciButton.Tag as Item))
					{
						sciButton.BackColor = EnabledPackBGColor;
						SciencePackButtons[sciButton] = true;
					}
				}
			}
			else //disable all the science packs that have the current science pack in their 'required' to make list
			{
				foreach (Button sciButton in SciencePackButtons.Keys.ToArray())
				{
					foreach (Recipe r in (sciButton.Tag as Item).ProductionRecipes)
					{
						foreach (Technology t in r.MyUnlockTechnologies)
						{
							if (t.SciPackSet.ContainsKey(sciPackButton.Tag as Item))
							{
								sciButton.BackColor = DisabledPackBGColor;
								SciencePackButtons[sciButton] = false;
							}
						}
					}
				}
			}
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
			EnabledObjects.Clear();
			EnabledObjects.Add(DCache.PlayerAssembler);

			//go through all technologies, check for fit compared to accepted science packs, and add its recipes to the set of enabled recipes
			foreach (Technology tech in DCache.Technologies.Values)
				if (tech.Available && !tech.SciPackList.Except(acceptedSciencePacks).Any())
					EnabledObjects.UnionWith(tech.UnlockedRecipes);

			//go through all the assemblers, beacons, and modules and add them to the enabled set if at least one of their associated items has at least one production recipe that is in the enabled set.
			foreach (Assembler assembler in DCache.Assemblers.Values)
			{
				bool enabled = false;
				foreach (IReadOnlyCollection<Recipe> recipes in assembler.AssociatedItems.Select(item => item.ProductionRecipes))
					foreach (Recipe recipe in recipes)
						enabled |= EnabledObjects.Contains(recipe);
				if (enabled)
					EnabledObjects.Add(assembler);
			}

			foreach (Beacon beacon in DCache.Beacons.Values)
			{
				bool enabled = false;
				foreach (IReadOnlyCollection<Recipe> recipes in beacon.AssociatedItems.Select(item => item.ProductionRecipes))
					foreach (Recipe recipe in recipes)
						enabled |= EnabledObjects.Contains(recipe);
				if (enabled)
					EnabledObjects.Add(beacon);
			}

			foreach (Module module in DCache.Modules.Values)
			{
				bool enabled = false;
				foreach (Recipe recipe in module.AssociatedItem.ProductionRecipes)
					enabled |= EnabledObjects.Contains(recipe);
				if (enabled)
					EnabledObjects.Add(module);
			}

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
