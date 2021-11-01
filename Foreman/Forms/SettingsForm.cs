using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Foreman
{
	public partial class SettingsForm : Form
	{
		public class SettingsFormOptions
		{
			public DataCache DCache { get; private set; }

			public List<Preset> Presets;
			public Preset SelectedPreset;
			public bool RequireReload;

			public ProductionGraphViewer.LOD LevelOfDetail;
			public int NodeCountForSimpleView;
			public bool DynamicLinkWidth;
			public bool AbbreviateSciPacks;
			public bool ShowRecipeToolTip;
			public bool RoundAssemblerCount;
			public bool LockedRecipeEditPanelPosition;

			public bool ShowErrorArrows;
			public bool ShowWarningArrows;

			public AssemblerSelector.Style DefaultAssemblerStyle;
			public ModuleSelector.Style DefaultModuleStyle;
			
			public bool DEV_ShowUnavailableItems;
			public bool DEV_UseRecipeBWFilters;

			public HashSet<DataObjectBase> EnabledObjects;

			public SettingsFormOptions(DataCache cache)
			{
				DCache = cache;
				Presets = new List<Preset>();
				EnabledObjects = new HashSet<DataObjectBase>();
				RequireReload = false;
			}
		}

		private static readonly Color AvailableObjectColor = Color.White;
		private static readonly Color UnavailableObjectColor = Color.Pink;

		public SettingsFormOptions Options;

		private List<ListViewItem> unfilteredAssemblerList;
		private List<ListViewItem> unfilteredMinerList;
		private List<ListViewItem> unfilteredPowerList;
		private List<ListViewItem> unfilteredBeaconList;
		private List<ListViewItem> unfilteredModuleList;
		private List<ListViewItem> unfilteredRecipeList;

		private List<ListViewItem> filteredAssemblerList;
		private List<ListViewItem> filteredMinerList;
		private List<ListViewItem> filteredPowerList;
		private List<ListViewItem> filteredBeaconList;
		private List<ListViewItem> filteredModuleList;
		private List<ListViewItem> filteredRecipeList;

		public SettingsForm(SettingsFormOptions options)
		{
			Options = options;

			InitializeComponent();
			MainForm.SetDoubleBuffered(AssemblerListView);
			MainForm.SetDoubleBuffered(MinerListView);
			MainForm.SetDoubleBuffered(ModuleListView);
			MainForm.SetDoubleBuffered(RecipeListView);

			AssemblerListView.Columns[0].Width = AssemblerListView.Width - 32;
			MinerListView.Columns[0].Width = MinerListView.Width - 32;
			ModuleListView.Columns[0].Width = ModuleListView.Width - 32;
			RecipeListView.Columns[0].Width = RecipeListView.Width - 32;

			unfilteredAssemblerList = new List<ListViewItem>();
			unfilteredMinerList = new List<ListViewItem>();
			unfilteredPowerList = new List<ListViewItem>();
			unfilteredBeaconList = new List<ListViewItem>();
			unfilteredModuleList = new List<ListViewItem>();
			unfilteredRecipeList = new List<ListViewItem>();

			filteredAssemblerList = new List<ListViewItem>();
			filteredMinerList = new List<ListViewItem>();
			filteredPowerList = new List<ListViewItem>();
			filteredBeaconList = new List<ListViewItem>();
			filteredModuleList = new List<ListViewItem>();
			filteredRecipeList = new List<ListViewItem>();

			SelectPresetMenuItem.Click += SelectPresetMenuItem_Click;
			DeletePresetMenuItem.Click += DeletePresetMenuItem_Click;

			MouseHoverDetector mhDetector = new MouseHoverDetector(100, 200);
			mhDetector.Add(RecipeListView, RecipeListView_StartHover, RecipeListView_EndHover);

			CurrentPresetLabel.Text = Options.SelectedPreset.Name;
			PresetListBox.Items.AddRange(Options.Presets.ToArray());
			PresetListBox.Items.RemoveAt(0); //0 is the currently active preset.

			//settings
			DynamicLWCheckBox.Checked = Options.DynamicLinkWidth;
			NodeCountForSimpleViewInput.Maximum = Math.Max(NodeCountForSimpleViewInput.Maximum, options.NodeCountForSimpleView);
			NodeCountForSimpleViewInput.Value = options.NodeCountForSimpleView;
			ShowNodeRecipeCheckBox.Checked = Options.ShowRecipeToolTip;
			RoundAssemblerCountCheckBox.Checked = options.RoundAssemblerCount;
			AbbreviateSciPackCheckBox.Checked = Options.AbbreviateSciPacks;
			RecipeEditPanelPositionLockCheckBox.Checked = Options.LockedRecipeEditPanelPosition;
			WarningArrowsCheckBox.Checked = Options.ShowWarningArrows;
			ErrorArrowsCheckBox.Checked = Options.ShowErrorArrows;

			switch(Options.LevelOfDetail)
			{
				case ProductionGraphViewer.LOD.Low:
					LowLodRadioButton.Checked = true;
					break;
				case ProductionGraphViewer.LOD.Medium:
					MediumLodRadioButton.Checked = true;
					break;
				case ProductionGraphViewer.LOD.High:
					HighLodRadioButton.Checked = true;
					break;
			}

			AssemblerSelectorStyleDropDown.Items.AddRange(AssemblerSelector.StyleNames);
			AssemblerSelectorStyleDropDown.SelectedIndex = (int)Options.DefaultAssemblerStyle;
			ModuleSelectorStyleDropDown.Items.AddRange(ModuleSelector.StyleNames);
			ModuleSelectorStyleDropDown.SelectedIndex = (int)Options.DefaultModuleStyle;

			ShowUnavailablesCheckBox.Checked = Options.DEV_ShowUnavailableItems;
			LoadBarrelingCheckBox.Checked = !Options.DEV_UseRecipeBWFilters;

			//lists
			LoadUnfilteredLists();
			UpdateModList();
		}

		private void UpdateModList()
		{
			Preset selectedPreset = (Preset)PresetListBox.SelectedItem;
			if (selectedPreset == null)
				selectedPreset = Options.SelectedPreset;

			PresetInfo presetInfo = PresetProcessor.ReadPresetInfo(selectedPreset);
			ModSelectionBox.Items.Clear();
			if (presetInfo.ModList != null)
			{
				List<string> modList = presetInfo.ModList.Select(kvp => kvp.Key + "_" + kvp.Value).ToList();
				modList.Sort();
				ModSelectionBox.Items.AddRange(modList.ToArray());
			}
			RecipeDifficultyLabel.Text = presetInfo.ExpensiveRecipes ? "Expensive" : "Normal";
			TechnologyDifficultyLabel.Text = presetInfo.ExpensiveTechnology ? "Expensive" : "Normal";
		}

		private void LoadUnfilteredLists()
		{
			IconList.Images.Clear();
			IconList.Images.Add(DataCache.UnknownIcon);

			LoadUnfilteredList(Options.DCache.Assemblers.Values.Where(a => a.EntityType == EntityType.Assembler), unfilteredAssemblerList);
			LoadUnfilteredList(Options.DCache.Assemblers.Values.Where(a => a.EntityType == EntityType.Miner), unfilteredMinerList);
			LoadUnfilteredList(Options.DCache.Assemblers.Values.Where(a => a.EntityType == EntityType.Boiler || a.EntityType == EntityType.BurnerGenerator || a.EntityType == EntityType.Generator || a.EntityType == EntityType.Reactor), unfilteredPowerList);
			LoadUnfilteredList(Options.DCache.Beacons.Values, unfilteredBeaconList);
			LoadUnfilteredList(Options.DCache.Modules.Values, unfilteredModuleList);
			LoadUnfilteredList(Options.DCache.Recipes.Values, unfilteredRecipeList);

			UpdateFilteredLists();
		}

		private void LoadUnfilteredList(IEnumerable<DataObjectBase> origin, List<ListViewItem> lviList)
		{
			foreach (DataObjectBase dObject in origin)
			{
				ListViewItem lvItem = new ListViewItem();
				if (dObject.Icon != null)
				{
					IconList.Images.Add(dObject.Icon);
					lvItem.ImageIndex = IconList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}

				lvItem.Text = dObject.FriendlyName;
				lvItem.Tag = dObject;
				lvItem.Name = dObject.Name; //key
				lvItem.Checked = true; //have to set this to true before (potentially) changing to false in order for the check boxes to appear
				lvItem.Checked = Options.EnabledObjects.Contains(dObject);
				lvItem.BackColor = dObject.Available ? AvailableObjectColor : UnavailableObjectColor;
				lviList.Add(lvItem);
			}
			unfilteredRecipeList.Sort(delegate (ListViewItem a, ListViewItem b)
			{
				DataObjectBase dobA = (DataObjectBase)a.Tag;
				DataObjectBase dobB = (DataObjectBase)b.Tag;

				int availableDiff = dobA.Available.CompareTo(dobB.Available);
				if (availableDiff != 0) return -availableDiff;
				return dobA.FriendlyName.CompareTo(dobB.FriendlyName);
			});

		}

		private void UpdateFilteredLists()
		{
			UpdateFilteredList(unfilteredAssemblerList, filteredAssemblerList, AssemblerListView);
			UpdateFilteredList(unfilteredMinerList, filteredMinerList, MinerListView);
			UpdateFilteredList(unfilteredPowerList, filteredPowerList, PowerListView);
			UpdateFilteredList(unfilteredBeaconList, filteredBeaconList, BeaconListView);
			UpdateFilteredList(unfilteredModuleList, filteredModuleList, ModuleListView);
			UpdateFilteredList(unfilteredRecipeList, filteredRecipeList, RecipeListView);
		}

		private void UpdateFilteredList(List<ListViewItem> unfilteredList, List<ListViewItem> filteredList, ListView owner)
		{
			string filterString = FilterTextBox.Text.ToLower();
			bool showUnavailables = ShowUnavailablesFilterCheckBox.Checked;

			filteredList.Clear();

			foreach (ListViewItem lvItem in unfilteredList)
				if ((showUnavailables || ((DataObjectBase)lvItem.Tag).Available) && (string.IsNullOrEmpty(filterString) || lvItem.Text.ToLower().Contains(filterString)))
					filteredList.Add(lvItem);


			owner.VirtualListSize = filteredList.Count;
			owner.Invalidate();
		}

		//PRESETS LIST------------------------------------------------------------------------------------------
		private void EnableSelectionBox_Enter(object sender, EventArgs e) { PresetListBox.SelectedItem = null; }
		private void CurrentPresetLabel_Click(object sender, EventArgs e) { PresetListBox.SelectedItem = null; }

		private void PresetListBox_SelectedValueChanged(object sender, EventArgs e)
		{
			UpdateModList();
			if (PresetListBox.SelectedItem == null)
				CurrentPresetLabel.Font = new Font(CurrentPresetLabel.Font, FontStyle.Bold);
			else
				CurrentPresetLabel.Font = new Font(CurrentPresetLabel.Font, FontStyle.Regular);
		}

		private void PresetListBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right) return;

			var index = PresetListBox.IndexFromPoint(e.Location);
			if (index != ListBox.NoMatches)
			{
				Preset rclickedPreset = ((Preset)PresetListBox.Items[index]);
				PresetListBox.SelectedIndex = index;

				if (rclickedPreset.IsCurrentlySelected)
				{
					SelectPresetMenuItem.Text = "Current Preset";
					SelectPresetMenuItem.Enabled = false;
				}
				else
				{
					SelectPresetMenuItem.Text = "Use This Preset";
					SelectPresetMenuItem.Enabled = true;
				}
				SelectPresetMenuItem.Enabled = !rclickedPreset.IsCurrentlySelected;
				if (rclickedPreset.IsDefaultPreset)
				{
					DeletePresetMenuItem.Text = "Default Preset";
					DeletePresetMenuItem.Enabled = false;
				}
				else
				{
					DeletePresetMenuItem.Text = "Delete This Preset";
					DeletePresetMenuItem.Enabled = !rclickedPreset.IsCurrentlySelected;
				}

				PresetMenuStrip.Show(Cursor.Position);
				PresetMenuStrip.Visible = true;
			}
			else
				PresetMenuStrip.Visible = false;
		}

		private void PresetListBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var index = PresetListBox.IndexFromPoint(e.Location);
			if (index != ListBox.NoMatches)
			{
				Options.SelectedPreset = ((Preset)PresetListBox.Items[index]);
				UpdateSettings();
				DialogResult = DialogResult.OK;
				this.Close();

			}
		}

		private void DeletePresetMenuItem_Click(object sender, EventArgs e)
		{
			Preset selectedPreset = (Preset)PresetListBox.SelectedItem;
			if (!selectedPreset.IsCurrentlySelected && !selectedPreset.IsDefaultPreset) //safety check - should always pass
			{
				if (MessageBox.Show("Are you sure you wish to delete the \"" + selectedPreset.Name + "\" preset? This is irreversible.", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					string jsonPath = Path.Combine(new string[] { Application.StartupPath, "Presets", selectedPreset.Name + ".json" });
					string iconPath = Path.Combine(new string[] { Application.StartupPath, "Presets", selectedPreset.Name + ".dat" });

					if (File.Exists(jsonPath))
						File.Delete(jsonPath);
					if (File.Exists(iconPath))
						File.Delete(iconPath);

					PresetListBox.Items.Remove(selectedPreset);
					Options.Presets.Remove(selectedPreset);
				}
			}
		}

		private void SelectPresetMenuItem_Click(object sender, EventArgs e)
		{
			Options.SelectedPreset = (Preset)PresetListBox.SelectedItem;
			UpdateSettings();
			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void Filters_Changed(object sender, EventArgs e)
		{
			UpdateFilteredLists();
		}

		//LIST VIEWS------------------------------------------------------------------------------------------

		private void ListView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.A && (e.Modifiers & Keys.Control) != 0)
				NativeMethods.SelectAllItems(sender as ListView);
		}

		private void ListView_MouseClick(object sender, MouseEventArgs e)
		{
			ListViewItem lvi = (sender as ListView).GetItemAt(e.X, e.Y);
			if (lvi != null && e.X < (lvi.Bounds.Left + 16))
			{
				if (lvi.Selected) //check all selected
				{
					bool setCheck = !lvi.Checked;
					foreach (int index in (sender as ListView).SelectedIndices)
					{
						lvi = (sender as ListView).Items[index];
						lvi.Checked = setCheck;
						if (lvi.Checked)
							Options.EnabledObjects.Add((DataObjectBase)lvi.Tag);
						else
							Options.EnabledObjects.Remove((DataObjectBase)lvi.Tag);
					}
				}
				else
				{
					lvi.Checked = !lvi.Checked;
					if (lvi.Checked)
						Options.EnabledObjects.Add((DataObjectBase)lvi.Tag);
					else
						Options.EnabledObjects.Remove((DataObjectBase)lvi.Tag);
				}
				(sender as ListView).Invalidate();
			}
		}

		private void ListView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListViewItem lvi = (sender as ListView).GetItemAt(e.X, e.Y);
			if (lvi != null && e.X < (lvi.Bounds.Left + 16))
			{
				if (lvi.Selected) //check all selected
				{
					bool setCheck = lvi.Checked;
					foreach (int index in (sender as ListView).SelectedIndices)
					{
						lvi = (sender as ListView).Items[index];
						lvi.Checked = setCheck;
						if (lvi.Checked)
							Options.EnabledObjects.Add((DataObjectBase)lvi.Tag);
						else
							Options.EnabledObjects.Remove((DataObjectBase)lvi.Tag);
					}
				}
				else
				{
					//lvi.Checked = lvi.Checked;
					if (lvi.Checked)
						Options.EnabledObjects.Add((DataObjectBase)lvi.Tag);
					else
						Options.EnabledObjects.Remove((DataObjectBase)lvi.Tag);
				}
				(sender as ListView).Invalidate();
			}
		}

		private void AssemblerListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredAssemblerList[e.ItemIndex]; }
		private void MinerListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredMinerList[e.ItemIndex]; }
		private void PowerListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredPowerList[e.ItemIndex]; }
		private void BeaconListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredBeaconList[e.ItemIndex]; }
		private void ModuleListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredModuleList[e.ItemIndex]; }
		private void RecipeListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e) { e.Item = filteredRecipeList[e.ItemIndex]; }

		private void RecipeListView_StartHover(object sender, MouseEventArgs e)
		{
			ListViewItem lvi = ((ListView)sender).GetItemAt(e.Location.X, e.Location.Y);
			Point location = new Point(e.X + 15, e.Y);
			if (lvi != null)
			{
				RecipeToolTip.SetRecipe(lvi.Tag as Recipe);
				RecipeToolTip.Show((Control)sender, location);
			}
		}
		private void RecipeListView_EndHover(object sender, EventArgs e)
		{
			RecipeToolTip.Hide((Control)sender);
		}

		//CONFIRM / RELOAD / CANCEL------------------------------------------------------------------------------------------
		private void ConfirmButton_Click(object sender, EventArgs e)
		{
			UpdateSettings();
			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void UpdateSettings()
		{
			Options.LevelOfDetail = LowLodRadioButton.Checked ? ProductionGraphViewer.LOD.Low : MediumLodRadioButton.Checked ? ProductionGraphViewer.LOD.Medium : ProductionGraphViewer.LOD.High;
			Options.NodeCountForSimpleView = (int)NodeCountForSimpleViewInput.Value;
			Options.DynamicLinkWidth = DynamicLWCheckBox.Checked;
			Options.AbbreviateSciPacks = AbbreviateSciPackCheckBox.Checked;
			Options.ShowRecipeToolTip = ShowNodeRecipeCheckBox.Checked;
			Options.RoundAssemblerCount = RoundAssemblerCountCheckBox.Checked;
			Options.LockedRecipeEditPanelPosition = RecipeEditPanelPositionLockCheckBox.Checked;
			Options.ShowErrorArrows = ErrorArrowsCheckBox.Checked;
			Options.ShowWarningArrows = WarningArrowsCheckBox.Checked;

			Options.DefaultAssemblerStyle = (AssemblerSelector.Style)AssemblerSelectorStyleDropDown.SelectedIndex;
			Options.DefaultModuleStyle = (ModuleSelector.Style)ModuleSelectorStyleDropDown.SelectedIndex;

			Options.DEV_ShowUnavailableItems = ShowUnavailablesCheckBox.Checked;
			Options.DEV_UseRecipeBWFilters = !LoadBarrelingCheckBox.Checked;

			//Options.EnabledObjects auto set by check/uncheck of lists
		}

		//PRESET FORMS (Import / compare)------------------------------------------------------------------------------------------

		private void ImportPresetButton_Click(object sender, EventArgs e)
		{
			using (PresetImportForm form = new PresetImportForm())
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = this.Left + 250;
				form.Top = this.Top + 50;
				DialogResult result = form.ShowDialog();

				if (form.ImportStarted)
					GC.Collect(); //we just processed a new preset (either fully or cancelled) - this required the opening of (potentially) alot of zip files and processing of a ton of bitmaps that are now stuck in garbate. In large mod packs like A&B this could clear out 2GB+ of memory.

				if (result == DialogResult.OK && !string.IsNullOrEmpty(form.NewPresetName)) //we have added a new preset
				{
					Preset newPreset = Options.Presets.FirstOrDefault(p => p.Name.ToLower() == form.NewPresetName.ToLower()); //extra check just in case we were overwriting
					if (newPreset == null)
					{
						newPreset = new Preset(form.NewPresetName, false, false);
						Options.Presets.Add(newPreset);
						PresetListBox.Items.Add(newPreset);
					}


					if(newPreset == Options.Presets[0]) //we have overwritten the currently active preset. Must force a reload
					{
						Options.RequireReload = true;
						UpdateSettings();
						DialogResult = DialogResult.OK;
						this.Close();
					}
					else if (MessageBox.Show("Preset import complete! Do you wish to switch to the new preset?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						Options.SelectedPreset = newPreset;
						UpdateSettings();
						DialogResult = DialogResult.OK;
						this.Close();
					}
				}
			}
		}

		private void ComparePresetsButton_Click(object sender, EventArgs e)
		{
			if (Options.Presets.Count < 2)
			{
				MessageBox.Show("Can not compare presets!\n...you only have 1 preset :/");
				return;
			}

			using (PresetComparatorForm form = new PresetComparatorForm())
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = this.Left + 50;
				form.Top = this.Top + 50;
				form.ShowDialog();
			}
		}

		private void LoadEnabledFromSaveButton_Click(object sender, EventArgs e)
		{
			using (SaveFileLoadForm form = new SaveFileLoadForm(Options.DCache, Options.EnabledObjects))
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = this.Left + 50;
				form.Top = this.Top + 50;
				DialogResult result = form.ShowDialog();

				if(result == DialogResult.OK)
					UpdateEnabledStatus();
				else if (result == DialogResult.Abort)
					MessageBox.Show("Error while reading save file. Try running factorio, opening the save game, saving again, and retrying?");
			}
		}

		private void SetEnabledFromSciencePacksButton_Click(object sender, EventArgs e)
		{
			using (SciencePacksLoadForm form = new SciencePacksLoadForm(Options.DCache, Options.EnabledObjects))
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = this.Left + 50;
				form.Top = this.Top + 50;
				DialogResult result = form.ShowDialog();

				if (result == DialogResult.OK)
					UpdateEnabledStatus();
			}
		}

		private void UpdateEnabledStatus()
		{
			//this requires a bit of juggling in order to prevent listview (virtual) from throwing a fit. we will ensure filtered lists contain all from unfiltered, then conduct the check updates, then update filtered.

			filteredAssemblerList.Clear();
			filteredAssemblerList.AddRange(unfilteredAssemblerList);
			AssemblerListView.VirtualListSize = filteredAssemblerList.Count;

			filteredBeaconList.Clear();
			filteredBeaconList.AddRange(unfilteredBeaconList);
			BeaconListView.VirtualListSize = filteredBeaconList.Count;

			filteredMinerList.Clear();
			filteredMinerList.AddRange(unfilteredMinerList);
			MinerListView.VirtualListSize = filteredMinerList.Count;

			filteredModuleList.Clear();
			filteredModuleList.AddRange(unfilteredModuleList);
			ModuleListView.VirtualListSize = filteredModuleList.Count;

			filteredPowerList.Clear();
			filteredPowerList.AddRange(unfilteredPowerList);
			PowerListView.VirtualListSize = filteredPowerList.Count;

			filteredRecipeList.Clear();
			filteredRecipeList.AddRange(unfilteredRecipeList);
			RecipeListView.VirtualListSize = filteredRecipeList.Count;


			foreach (ListViewItem item in unfilteredAssemblerList)
				item.Checked = Options.EnabledObjects.Contains((DataObjectBase)item.Tag);
			foreach (ListViewItem item in unfilteredBeaconList)
				item.Checked = Options.EnabledObjects.Contains((DataObjectBase)item.Tag);
			foreach (ListViewItem item in unfilteredMinerList)
				item.Checked = Options.EnabledObjects.Contains((DataObjectBase)item.Tag);
			foreach (ListViewItem item in unfilteredModuleList)
				item.Checked = Options.EnabledObjects.Contains((DataObjectBase)item.Tag);
			foreach (ListViewItem item in unfilteredPowerList)
				item.Checked = Options.EnabledObjects.Contains((DataObjectBase)item.Tag);
			foreach (ListViewItem item in unfilteredRecipeList)
				item.Checked = Options.EnabledObjects.Contains((DataObjectBase)item.Tag);

			UpdateFilteredLists();
		}
	}
}
