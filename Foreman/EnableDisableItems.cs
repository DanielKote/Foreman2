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
			ModSelectionBox.Sorted = true;
			ModSelectionBox.DisplayMember = "name";
			for (int i = 0; i < ModSelectionBox.Items.Count; i++)
			{
				if (((Mod)ModSelectionBox.Items[i]).Enabled)
				{
					ModSelectionBox.SetItemChecked(i, true);
				}
			}
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
			((Mod)ModSelectionBox.Items[e.Index]).Enabled = e.NewValue == CheckState.Checked;
			ModsChanged = true;
		}
	}
}
