using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Foreman
{	
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			if (!Directory.Exists(Properties.Settings.Default.FactorioDataPath))
			{
				foreach (String defaultPath in new String[]{
												  Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"), "Factorio", "Data"),
												  Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), "Factorio", "Data"),
												  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Applications", "factorio.app", "Contents", "data")}) //Not actually tested on a Mac
				{
					if (Directory.Exists(defaultPath))
					{
						Properties.Settings.Default["FactorioDataPath"] = defaultPath;
						Properties.Settings.Default.Save();
						break;
					}
				}
			}

			if (!Directory.Exists(Properties.Settings.Default.FactorioDataPath))
			{
				using (DirectoryChooserForm form = new DirectoryChooserForm())
				{
					if (form.ShowDialog() == DialogResult.OK)
					{
						Properties.Settings.Default["FactorioDataPath"] = Path.Combine(form.SelectedPath, "data"); ;
						Properties.Settings.Default.Save();
					}
					else
					{
						Close();
						Dispose();
						return;
					}
				}
			}

			DataCache.FactorioDataPath = Properties.Settings.Default.FactorioDataPath;
			DataCache.LoadRecipes();

			rateOptionsDropDown.SelectedIndex = 0;

			AssemblerSelectionBox.Items.Clear();
			AssemblerSelectionBox.Items.AddRange(DataCache.Assemblers.Values.ToArray());
			AssemblerSelectionBox.Sorted = true;
			AssemblerSelectionBox.DisplayMember = "FriendlyName";
			for ( int i = 0; i < AssemblerSelectionBox.Items.Count; i++)
			{
				AssemblerSelectionBox.SetItemChecked(i, true);
			}

			MinerSelectionBox.Items.AddRange(DataCache.Miners.Values.ToArray());
			MinerSelectionBox.Sorted = true;
			MinerSelectionBox.DisplayMember = "FriendlyName";
			for (int i = 0; i < MinerSelectionBox.Items.Count; i++)
			{
				MinerSelectionBox.SetItemChecked(i, true);
			}

			ModuleSelectionBox.Items.AddRange(DataCache.Modules.Values.ToArray());
			ModuleSelectionBox.Sorted = true;
			ModuleSelectionBox.DisplayMember = "FriendlyName";
			for (int i = 0; i < ModuleSelectionBox.Items.Count; i++)
			{
				ModuleSelectionBox.SetItemChecked(i, true);
			}


			//Listview
			ItemListView.Items.Clear();
			ItemImageList.Images.Add(DataCache.UnknownIcon);
			foreach (var aitem in DataCache.Items)
			{
				ListViewItem lvItem = new ListViewItem();
				if (DataCache.Items[aitem.Key].Icon != null)
				{
					ItemImageList.Images.Add(DataCache.Items[aitem.Key].Icon);
					lvItem.ImageIndex = ItemImageList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}
				lvItem.Text = aitem.Value.FriendlyName;
				lvItem.Tag = aitem.Value;
				ItemListView.Items.Add(lvItem);
			}

			ItemListView.Sorting = SortOrder.Ascending;
			ItemListView.Sort();

		}

		private void ItemListForm_KeyDown(object sender, KeyEventArgs e)
		{
#if DEBUG
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
#endif
		}

		private void AddItemButton_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem lvItem in ItemListView.SelectedItems)
			{
				GraphViewer.AddDemand((Item)lvItem.Tag);
			}
		}
		
		private void rateButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.Graph.SelectedAmountType = AmountType.Rate;
				rateOptionsDropDown.Enabled = true;
			}
			else
			{
				rateOptionsDropDown.Enabled = false;
			}
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
		}

		private void fixedAmountButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.Graph.SelectedAmountType = AmountType.FixedAmount;
			}

				MinerDisplayCheckBox.Checked 
					= MinerDisplayCheckBox.Enabled
					= AssemblerDisplayCheckBox.Enabled
					= AssemblerDisplayCheckBox.Checked
					= !(sender as RadioButton).Checked;
			
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
		}

		private void rateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((sender as ComboBox).SelectedIndex)
			{
				case 0:
					GraphViewer.Graph.SelectedUnit = RateUnit.PerSecond;
					GraphViewer.Invalidate();
					GraphViewer.UpdateNodes();
					break;
				case 1:
					GraphViewer.Graph.SelectedUnit = RateUnit.PerMinute;
					GraphViewer.Invalidate();
					GraphViewer.UpdateNodes();
					break;
			}
		}

		private void AutomaticCompleteButton_Click(object sender, EventArgs e)
		{
			GraphViewer.Graph.SatisfyAllItemDemands();
			GraphViewer.AddRemoveElements();
			GraphViewer.PositionNodes();
		}

		private void ClearButton_Click(object sender, EventArgs e)
		{
			GraphViewer.Graph.Nodes.Clear();
			GraphViewer.Elements.Clear();
			GraphViewer.Invalidate();
		}

		private void AssemblerDisplayCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.ShowAssemblers = (sender as CheckBox).Checked;
			GraphViewer.UpdateNodes();
		}

		private void SingleAssemblerPerRecipeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.Graph.OneAssemblerPerRecipe = (sender as CheckBox).Checked;
			GraphViewer.UpdateNodes();
		}

		private void ExportImageButton_Click(object sender, EventArgs e)
		{
			ImageExportForm form = new ImageExportForm(GraphViewer);			
			form.Show();
		}

		private void AssemblerSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			((Assembler)AssemblerSelectionBox.Items[e.Index]).Enabled = e.NewValue == CheckState.Checked;
			GraphViewer.UpdateNodes();
		}

		private void MinerDisplayCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.ShowMiners = (sender as CheckBox).Checked;
			GraphViewer.UpdateNodes();
		}

		private void MinerSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			((Miner)MinerSelectionBox.Items[e.Index]).Enabled = e.NewValue == CheckState.Checked;
			GraphViewer.UpdateNodes();
		}

		private void ModuleSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			((Module)ModuleSelectionBox.Items[e.Index]).Enabled = e.NewValue == CheckState.Checked;
			GraphViewer.UpdateNodes();
		}

		private void ItemListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ItemListView.SelectedItems.Count == 0)
			{
				AddItemButton.Enabled = false;
			}
			else if (ItemListView.SelectedItems.Count == 1)
			{
				AddItemButton.Enabled = true;
				AddItemButton.Text = "Add Output";
			}
			else if (ItemListView.SelectedItems.Count > 1)
			{
				AddItemButton.Enabled = true;
				AddItemButton.Text = "Add Outputs";
			}
		}

		private void ItemListView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			GraphViewer.AddDemand((Item)ItemListView.SelectedItems[0].Tag);
		}
		
		private void ItemListView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			HashSet<Item> draggedItems = new HashSet<Item>();
			foreach (ListViewItem item in ItemListView.SelectedItems)
			{
				draggedItems.Add((Item)item.Tag);
			}
			DoDragDrop(draggedItems, DragDropEffects.All);
		}
	}
}
