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
	public partial class EnableDisableItemsForm : Form
	{
		public bool ShowMiners;
		public bool ModsChanged;

		public EnableDisableItemsForm()
		{
			InitializeComponent();

			AssemblerSelectionBox.Items.AddRange(DataCache.Assemblers.Values.ToArray());
			AssemblerSelectionBox.Sorted = true;
			AssemblerSelectionBox.DisplayMember = "FriendlyName";
			for (int i = 0; i < AssemblerSelectionBox.Items.Count; i++)
			{
				if (((Assembler)AssemblerSelectionBox.Items[i]).Enabled)
				{
					AssemblerSelectionBox.SetItemChecked(i, true);
				}
			}

			MinerSelectionBox.Items.AddRange(DataCache.Miners.Values.ToArray());
			MinerSelectionBox.Sorted = true;
			MinerSelectionBox.DisplayMember = "FriendlyName";
			for (int i = 0; i < MinerSelectionBox.Items.Count; i++)
			{
				if (((Miner)MinerSelectionBox.Items[i]).Enabled)
				{
					MinerSelectionBox.SetItemChecked(i, true);
				}
			}

			ModuleSelectionBox.Items.AddRange(DataCache.Modules.Values.ToArray());
			ModuleSelectionBox.Sorted = true;
			ModuleSelectionBox.DisplayMember = "FriendlyName";
			for (int i = 0; i < ModuleSelectionBox.Items.Count; i++)
			{
				if (((Module)ModuleSelectionBox.Items[i]).Enabled)
				{
					ModuleSelectionBox.SetItemChecked(i, true);
				}
			}

			ModSelectionBox.Items.AddRange(DataCache.Mods.ToArray());
			ModSelectionBox.DisplayMember = "name";
			for (int i = 0; i < ModSelectionBox.Items.Count; i++)
			{
				if (((Mod)ModSelectionBox.Items[i]).Enabled)
				{
					ModSelectionBox.SetItemChecked(i, true);
				}
			}

			ModsChanged = false;
		}

		private void AssemblerSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
            Assembler assembler = (Assembler) AssemblerSelectionBox.Items[e.Index];
			assembler.Enabled = e.NewValue == CheckState.Checked;

            if (assembler.Enabled)
            {
                // This assembler just turned on.
                foreach (var recipe in DataCache.Recipes.Values.Where(r => assembler.Categories.Contains(r.Category)))
                {
                    // Enable every recipe this assembler can process.
                    recipe.Enabled = true; 
                }
            }
            else
            {
                // This assembler just turned off, disable any recipes in catagories that are now entirely disabled.
                foreach (var category in assembler.Categories)
                {
                    //If any assembler that supports this category is enabled, we don't disable the associated recipes.
                    if (!DataCache.Assemblers.Values.Any(a => a.Categories.Contains(category) && a.Enabled))
                    {
                        foreach (var recipe in DataCache.Recipes.Values.Where(r => r.Category == category))
                        {
                            recipe.Enabled = false;
                        }
                    }
                }
            }

        }

		private void MinerSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			((Miner)MinerSelectionBox.Items[e.Index]).Enabled = e.NewValue == CheckState.Checked;
		}

		private void ModuleSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			((Module)ModuleSelectionBox.Items[e.Index]).Enabled = e.NewValue == CheckState.Checked;
		}

		private void ModSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			Mod mod = (Mod)ModSelectionBox.Items[e.Index];
			mod.Enabled = e.NewValue == CheckState.Checked;
			if (!mod.Enabled)
			{
				for (int i = 0; i < ModSelectionBox.Items.Count; i++)
				{
					if (((Mod)ModSelectionBox.Items[i]).DependsOn(mod, true))
					{
						((Mod)ModSelectionBox.Items[i]).Enabled = false;
						ModSelectionBox.SetItemChecked(i, false);
					}
				}
			}
			else
			{
				for (int i = 0; i < ModSelectionBox.Items.Count; i++)
				{
					if (mod.DependsOn((Mod)ModSelectionBox.Items[i], true))
					{
						((Mod)ModSelectionBox.Items[i]).Enabled = true;
						ModSelectionBox.SetItemChecked(i, true);
					}
				}
			}
			ModsChanged = true;
		}
	}
}
