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
                Mod mod = (Mod)ModSelectionBox.Items[i];
                if (mod.Enabled)
                {
                    ModSelectionBox.SetItemChecked(i, true);
                }

                foreach (ModDependency dep in mod.parsedDependencies)
                {
                    if (dep.Optional)
                        continue;

                    Mod otherMod = this.getModFromName(dep.ModName);
                    if (otherMod == null)
                    {
                        ModSelectionBox.errors[i] = mod.Name + " requires " + dep.ModName + " but is missing";
                        break;
                    }
                    else if (!mod.DependsOn(otherMod, false))
                    {
                        string versionCompStr = "";
                        switch (dep.VersionType)
                        {
                            case DependencyType.EqualTo:
                                versionCompStr = "=";
                                break;
                            case DependencyType.GreaterThan:
                                versionCompStr = ">";
                                break;
                            case DependencyType.GreaterThanOrEqual:
                                versionCompStr = ">=";
                                break;
                        }
                        ModSelectionBox.errors[i] = $"{mod.Name} requires {dep.ModName} {versionCompStr} {dep.Version} but is {otherMod.version}";
                        break;
                    }
                }
			}

			ModsChanged = false;
		}

        private Mod getModFromName(string name)
        {
            for (int i = 0; i < ModSelectionBox.Items.Count; i++)
            {
                Mod mod = (Mod)ModSelectionBox.Items[i];
                if (mod.Name == name)
                    return mod;
            }

            return null;
        }

		private void AssemblerSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			((Assembler)AssemblerSelectionBox.Items[e.Index]).Enabled = e.NewValue == CheckState.Checked;
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
			if (mod.Enabled)
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
			else
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
			ModsChanged = true;
		}
    }
}
