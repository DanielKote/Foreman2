using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Collections.Specialized;

namespace Foreman
{
	public partial class MainForm : Form
	{
		//Updating bools are used to prevent a call to change properties.settings for each item on the 'select all' or 'select none' buttons
		private bool AssemblerSelectionBoxItemUpdating = false;
		private bool MinerSelectionBoxItemUpdating = false;
		private bool ModuleSelectionBoxItemUpdating = false;

		//unfiltered sets are filled directly from data cache and contain the full list - those displayed are just filtered (saves on not having to make ListViewItems every time)
		//to simplify things they have been sorted: alphabetically a->z by LFriendlyName (which is the lowercase FriendlyName).
		private List<ListViewItem> unfilteredItemList;
		private List<ListViewItem> unfilteredRecipeList;
		//filtered lists are used to actually display the items to the listboxes, and as such have been filtered & sorted (and will include the hidden versions, IF they are to be shown)
		//filtered sets are used when drawing the items to decide if they should be b&w / darker (disabled/hidden, but we need to show them)
		//NOTE: items are hidden when they are not part of any visible recipe. recipes are hidden when they have no valid assembler
		private List<ListViewItem> filteredItemList;
		private List<ListViewItem> filteredRecipeList;
		private HashSet<ListViewItem> filteredHiddenItemSet;
		private HashSet<ListViewItem> filteredHiddenRecipeSet;

		private static readonly Color NormalBackground = Color.White;
		private static readonly Color HiddenBackground = Color.FromArgb(255, 255, 170, 170);

		public MainForm()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
			AssemblerSelectionBox.DisplayMember = "FriendlyName";
			MinerSelectionBox.DisplayMember = "FriendlyName";
			ModuleSelectionBox.DisplayMember = "FriendlyName";
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);

			DataCache.DataLoaded += DataCache_DataLoaded;
			unfilteredItemList = new List<ListViewItem>();
			unfilteredRecipeList = new List<ListViewItem>();
			filteredItemList = new List<ListViewItem>();
			filteredRecipeList = new List<ListViewItem>();
			filteredHiddenItemSet = new HashSet<ListViewItem>();
			filteredHiddenRecipeSet = new HashSet<ListViewItem>();
		}

		private void MainForm_Load(object sender, EventArgs e)
        {
			WindowState = FormWindowState.Maximized;

            //I changed the name of the variable (again), so this copies the value over for people who are upgrading their Foreman version
            if (Properties.Settings.Default.FactorioPath == "" && Properties.Settings.Default.FactorioDataPath != "")
            {
                Properties.Settings.Default.FactorioPath = Path.GetDirectoryName(Properties.Settings.Default.FactorioDataPath);
                Properties.Settings.Default.FactorioDataPath = "";
            }

            if (!Directory.Exists(Properties.Settings.Default.FactorioPath))
            {
                List<FoundInstallation> installations = new List<FoundInstallation>();
                String steamFolder = Path.Combine("Steam", "steamapps", "common");
                foreach (String defaultPath in new String[]{
                  Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"), steamFolder, "Factorio"),
                  Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), steamFolder, "Factorio"),
                  Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"), "Factorio"),
                  Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), "Factorio"),
                  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Applications", "factorio.app", "Contents")}) //Not actually tested on a Mac
                {
                    if (Directory.Exists(defaultPath))
                    {
                        FoundInstallation inst = FoundInstallation.GetInstallationFromPath(defaultPath);
                        if (inst != null)
                            installations.Add(inst);
                    }
                }

                if (installations.Count > 0)
                {
                    if (installations.Count > 1)
                    {
                        using (InstallationChooserForm form = new InstallationChooserForm(installations))
                        {
                            if (form.ShowDialog() == DialogResult.OK && form.SelectedPath != null)
                            {
                                Properties.Settings.Default.FactorioPath = form.SelectedPath;
                            }
                            else
                            {
                                Close();
                                Dispose();
                                return;
                            }
                        }
                    }
                    else
                    {
                        Properties.Settings.Default.FactorioPath = installations[0].path;
                    }

                    Properties.Settings.Default.Save();
                }
            }

            if (!Directory.Exists(Properties.Settings.Default.FactorioPath))
            {
                using (DirectoryChooserForm form = new DirectoryChooserForm(""))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        Properties.Settings.Default.FactorioPath = form.SelectedPath;
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

            if (!Directory.Exists(Properties.Settings.Default.FactorioUserDataPath))
            {
                if (Directory.Exists(Properties.Settings.Default.FactorioPath))
                    Properties.Settings.Default.FactorioUserDataPath = Properties.Settings.Default.FactorioPath;
                else if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio")))
                    Properties.Settings.Default.FactorioUserDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio");
            }

            if (Properties.Settings.Default.EnabledMods == null) Properties.Settings.Default.EnabledMods = new StringCollection();
            if (Properties.Settings.Default.EnabledAssemblers == null) Properties.Settings.Default.EnabledAssemblers = new StringCollection();
            if (Properties.Settings.Default.EnabledMiners == null) Properties.Settings.Default.EnabledMiners = new StringCollection();
            if (Properties.Settings.Default.EnabledModules == null) Properties.Settings.Default.EnabledModules = new StringCollection();

			ModuleDropDown.SelectedIndex = Properties.Settings.Default.DefaultModules;
			MinorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MinorGridlines;
			MajorGridlinesDropDown.SelectedIndex = Properties.Settings.Default.MajorGridlines;
			GridlinesCheckbox.Checked = Properties.Settings.Default.AltGridlines;
			ShowDisabledRecipesCheckBox.Checked = Properties.Settings.Default.ShowHidden;
			ShowHiddenItemsCheckBox.Checked = Properties.Settings.Default.ShowHidden;

			using (DataReloadForm form = new DataReloadForm())
				form.ShowDialog(); //LOAD FACTORIO DATA

			Properties.Settings.Default.EnabledAssemblers.Clear();
			foreach (string assembler in DataCache.Assemblers.Keys)
				Properties.Settings.Default.EnabledAssemblers.Add(assembler);

			Properties.Settings.Default.EnabledMiners.Clear();
			foreach (string miner in DataCache.Miners.Keys)
				Properties.Settings.Default.EnabledMiners.Add(miner);

			Properties.Settings.Default.EnabledModules.Clear();
			foreach (string module in DataCache.Modules.Keys)
				Properties.Settings.Default.EnabledModules.Add(module);

			Properties.Settings.Default.Save();

			UpdateControlValues();
        }

		private void DataCache_DataLoaded(object _, EventArgs e)
		{
			//fill in the selection boxes (under 'other' tab)
			AssemblerSelectionBox.Items.Clear();
			AssemblerSelectionBox.Items.AddRange(DataCache.Assemblers.Values.ToArray());
			MinerSelectionBox.Items.Clear();
			MinerSelectionBox.Items.AddRange(DataCache.Miners.Values.ToArray());
			ModuleSelectionBox.Items.Clear();
			ModuleSelectionBox.Items.AddRange(DataCache.Modules.Values.ToArray());
			UpdateEDCheckLists();

			//fill in the unfiltered lists (and sort them alpha)
			unfilteredRecipeList.Clear();
			unfilteredItemList.Clear();
			IconList.Images.Clear();
			if (DataCache.UnknownIcon != null)
				IconList.Images.Add(DataCache.UnknownIcon);

			foreach (Recipe recipe in DataCache.Recipes.Values)
			{
				ListViewItem lvItem = new ListViewItem();
				if (recipe.Icon != null)
				{
					IconList.Images.Add(recipe.Icon);
					lvItem.ImageIndex = IconList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}
				lvItem.Text = recipe.FriendlyName;
				lvItem.Tag = recipe;
				lvItem.Name = recipe.Name; //key
				lvItem.Checked = recipe.Enabled;
				unfilteredRecipeList.Add(lvItem);
			}

			foreach (Item item in DataCache.Items.Values)
			{
				ListViewItem lvItem = new ListViewItem();
				if (item.Icon != null)
				{
					IconList.Images.Add(item.Icon);
					lvItem.ImageIndex = IconList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}
				lvItem.Text = item.FriendlyName;
				lvItem.Tag = item;
				lvItem.Name = item.Name; //key
				unfilteredItemList.Add(lvItem);
			}

			unfilteredRecipeList.Sort((a, b) => ((Recipe)a.Tag).CompareTo((Recipe)b.Tag));
			unfilteredItemList.Sort((a, b) => ((Item)a.Tag).CompareTo((Item)b.Tag));
		}

		public void UpdateVisibleItemList()
		{
			//very quick filter for only items used by the currently active recipes:
			DataCache.UpdateRecipesAssemblerStatus();
			HashSet<Item> availableItems = new HashSet<Item>();
			foreach (Recipe recipe in DataCache.Recipes.Values.Where(n => n.Enabled && n.HasEnabledAssemblers))
			{
				foreach (Item item in recipe.Ingredients.Keys)
					availableItems.Add(item);
				foreach (Item item in recipe.Results.Keys)
					availableItems.Add(item);
			}
			string filterString = (ItemFilterTextBox.Text).ToLower();

			filteredItemList.Clear();
			filteredHiddenItemSet.Clear();
			foreach (ListViewItem lvItem in unfilteredItemList)
			{
				if (availableItems.Contains(lvItem.Tag as Item) && lvItem.Text.ToLower().Contains(filterString))
				{
					filteredItemList.Add(lvItem);
					lvItem.BackColor = NormalBackground;
				}
			}
			if (Properties.Settings.Default.ShowHidden) //if we are to show hidden items, we will place them under the visible items, seperately sorted by alpha (due to the way we add them here)
			{
				foreach (ListViewItem lvItem in unfilteredItemList)
				{
					if (!availableItems.Contains(lvItem.Tag as Item) && lvItem.Text.ToLower().Contains(filterString))
					{
						filteredItemList.Add(lvItem);
						filteredHiddenItemSet.Add(lvItem);
						lvItem.BackColor = HiddenBackground;
					}
				}
			}
			ItemListView.VirtualListSize = filteredItemList.Count;
			ItemListView.Invalidate();
		}

		public void UpdateVisibleRecipeList()
		{
			DataCache.UpdateRecipesAssemblerStatus();
			string filterString = (RecipeFilterTextBox.Text).ToLower();

			filteredRecipeList.Clear();
			filteredHiddenRecipeSet.Clear();
			foreach (ListViewItem lvItem in unfilteredRecipeList)
			{
				if ((lvItem.Tag as Recipe).HasEnabledAssemblers && lvItem.Text.ToLower().Contains(filterString))
				{
					filteredRecipeList.Add(lvItem);
					lvItem.BackColor = NormalBackground;
				}
			}
			if (Properties.Settings.Default.ShowHidden) //if we are to show disabled recipes, we will place them under the visible recipes, seperately sorted by alpha (due to the way we add them here)
			{
				foreach (ListViewItem lvItem in unfilteredRecipeList)
				{
					if (!(lvItem.Tag as Recipe).HasEnabledAssemblers && lvItem.Text.ToLower().Contains(filterString))
					{
						filteredRecipeList.Add(lvItem);
						filteredHiddenRecipeSet.Add(lvItem);
						lvItem.BackColor = HiddenBackground;
					}
				}
			}
			RecipeListView.VirtualListSize = filteredRecipeList.Count;
			RecipeListView.Invalidate();
		}

		public void UpdateEDCheckLists()
		{
			for (int i = 0; i < AssemblerSelectionBox.Items.Count; i++)
				if (((Assembler)AssemblerSelectionBox.Items[i]).Enabled)
					AssemblerSelectionBox.SetItemChecked(i, true);

			for (int i = 0; i < MinerSelectionBox.Items.Count; i++)
				if (((Miner)MinerSelectionBox.Items[i]).Enabled)
					MinerSelectionBox.SetItemChecked(i, true);

			for (int i = 0; i < ModuleSelectionBox.Items.Count; i++)
				if (((Module)ModuleSelectionBox.Items[i]).Enabled)
					ModuleSelectionBox.SetItemChecked(i, true);
		}

		private void ItemListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			e.Item = filteredItemList[e.ItemIndex];
		}

		private void RecipeListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			e.Item = filteredRecipeList[e.ItemIndex];
		}

		private void RecipeListView_MouseClick(object sender, MouseEventArgs e)
		{
			ListViewItem lvi = RecipeListView.GetItemAt(e.X, e.Y);
			if (lvi != null && e.X < (lvi.Bounds.Left + 16))
			{
				if (lvi.Selected) //check all selected
				{
					bool setCheck = !lvi.Checked;
					foreach (int index in RecipeListView.SelectedIndices)
					{
						lvi = filteredRecipeList[index];
						lvi.Checked = setCheck;
						(lvi.Tag as Recipe).Enabled = lvi.Checked;
					}
				}
				else
				{
					lvi.Checked = !lvi.Checked;
					(lvi.Tag as Recipe).Enabled = lvi.Checked;
				}
				RecipeListView.Invalidate();
				GraphViewer.Invalidate();
				//filtered item list does need to update, but since we are updating it any time we switch to it (and it isnt visible), we might as well ignore it
			}
		}

		private void RecipeListView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ListViewItem lvi = RecipeListView.GetItemAt(e.X, e.Y);
			if (lvi != null && e.X < (lvi.Bounds.Left + 16))
			{
				if (lvi.Selected) //check all selected
				{
					bool setCheck = lvi.Checked;
					foreach (int index in RecipeListView.SelectedIndices)
					{
						lvi = filteredRecipeList[index];
						lvi.Checked = setCheck;
						(lvi.Tag as Recipe).Enabled = lvi.Checked;
					}
				}
				else
				{
					//lvi.Checked = lvi.Checked;
					(lvi.Tag as Recipe).Enabled = lvi.Checked;
				}
				RecipeListView.Invalidate();
				GraphViewer.Invalidate();
				//filtered item list does need to update, but since we are updating it any time we switch to it (and it isnt visible), we might as well ignore it
			}
		}

		private void RecipeListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (RecipeListView.SelectedIndices.Count == 0)
				AddRecipeButton.Enabled = false;
			else if (RecipeListView.SelectedIndices.Count == 1)
			{
				AddRecipeButton.Enabled = true;
				AddRecipeButton.Text = "Add Recipe";
			}
			else
			{
				AddRecipeButton.Enabled = true;
				AddRecipeButton.Text = "Add Recipes";
			}
		}

		private void RecipeListView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.A && (e.Modifiers & Keys.Control) != 0)
				NativeMethods.SelectAllItems(RecipeListView);
		}

		public void UpdateRecipeListItemCheckmark(Recipe recipe)
        {
			string rliKey = recipe.Name;
			if (RecipeListView.Items.ContainsKey(rliKey))
				RecipeListView.Items[rliKey].Checked = recipe.Enabled;
			RecipeListView.Invalidate();
        }

		private void ItemFilterTextBox_TextChanged(object sender, EventArgs e)
		{
			UpdateVisibleItemList();
		}

		private void RecipeFilterTextBox_TextChanged(object sender, EventArgs e)
		{
			UpdateVisibleRecipeList();
		}

		private void RecipeFilterTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (RecipeListView.Items.Count == 0)
			{
				return;
			}
			int currentSelection;
			if (RecipeListView.SelectedIndices.Count == 0)
			{
				currentSelection = -1;
			}
			else
			{
				currentSelection = RecipeListView.SelectedIndices[0];
			}
			if (e.KeyCode == Keys.Down)
			{
				int newSelection = currentSelection + 1;
				if (newSelection >= RecipeListView.Items.Count) newSelection = RecipeListView.Items.Count - 1;
				if (newSelection <= 0) newSelection = 0;
				RecipeListView.SelectedIndices.Clear();
				RecipeListView.SelectedIndices.Add(newSelection);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up)
			{
				int newSelection = currentSelection - 1;
				if (newSelection == -1) newSelection = 0;
				RecipeListView.SelectedIndices.Clear();
				RecipeListView.SelectedIndices.Add(newSelection);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Enter)
			{
				AddRecipeButton.PerformClick();
			}
		}

		private void AddItemButton_Click(object sender, EventArgs e)
		{
			Item item = filteredItemList[ItemListView.SelectedIndices[0]].Tag as Item;
			Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));
			if (GraphViewer.ShowGrid)
			{
				location.X = GraphViewer.AlignToGrid(location.X);
				location.Y = GraphViewer.AlignToGrid(location.Y);
			}
			var chooserPanel = new ChooserPanel(item, true, true, GraphViewer);

			chooserPanel.Show(c =>
			{
				if (c != null)
				{
					NodeElement newElement = new NodeElement(c, GraphViewer);
					newElement.Update();
					newElement.Location = location;
					GraphViewer.Graph.UpdateNodeValues();
				}
			});
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
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
		}

		private void fixedAmountButton_CheckedChanged(object sender, EventArgs e)
		{
			if ((sender as RadioButton).Checked)
			{
				this.GraphViewer.Graph.SelectedAmountType = AmountType.FixedAmount;
			}

			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
		}

		private void rateOptionsDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((sender as ComboBox).SelectedIndex)
			{
				case 0:
					GraphViewer.Graph.SelectedUnit = RateUnit.PerSecond;
					break;
				case 1:
					GraphViewer.Graph.SelectedUnit = RateUnit.PerMinute;
					break;
			}
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.Invalidate();
			GraphViewer.UpdateNodes();
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
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
		}

		private void ExportImageButton_Click(object sender, EventArgs e)
		{
			ImageExportForm form = new ImageExportForm(GraphViewer);
			form.Show();
		}

		private void MinerDisplayCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.ShowMiners = (sender as CheckBox).Checked;
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
		}

        private void ItemListView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			AddItemButton.PerformClick();
		}

		private void ItemListView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			DoDragDrop(filteredItemList[ItemListView.SelectedIndices[0]].Tag as Item, DragDropEffects.All);
		}

		private void saveGraphButton_Click(object sender, EventArgs e)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.DefaultExt = ".json";
			dialog.Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
			dialog.AddExtension = true;
			dialog.OverwritePrompt = true;
			dialog.FileName = "Flowchart.json";
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			var serialiser = JsonSerializer.Create();
			serialiser.Formatting = Formatting.Indented;
			var writer = new JsonTextWriter(new StreamWriter(dialog.FileName));
			try
			{
				serialiser.Serialize(writer, GraphViewer);
			}
			catch (Exception exception)
			{
				MessageBox.Show("Could not save this file. See log for more details");
				ErrorLogging.LogLine(String.Format("Error saving file '{0}'. Error: '{1}'", dialog.FileName, exception.Message));
			}
			finally
			{
				writer.Close();
			}
		}

		private void loadGraphButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
			dialog.CheckFileExists = true;
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			//try
			//{
				GraphViewer.LoadFromJson(JObject.Parse(File.ReadAllText(dialog.FileName)));
			//}
			//catch (Exception exception)
			//{
			//	MessageBox.Show("Could not load this file. See log for more details");
			//	ErrorLogging.LogLine(String.Format("Error loading file '{0}'. Error: '{1}'", dialog.FileName, exception.Message));
			//}

			UpdateControlValues();
			GraphViewer.Invalidate();
		}

		private void SettingsButton_Click(object sender, EventArgs e)
		{
			bool reload = false;
			do
			{
				SettingsForm.SettingsFormOptions oldOptions = new SettingsForm.SettingsFormOptions();
				foreach (Mod mod in DataCache.Mods)
				{
					if(mod.Name == "core" || mod.Name == "base")
						mod.Enabled = true;
					oldOptions.Mods.Add(mod, mod.Enabled);
				}
				foreach (Language language in DataCache.Languages)
					oldOptions.LanguageOptions.Add(language);
				oldOptions.selectedLanguage = DataCache.Languages.FirstOrDefault(l => l.Name == Properties.Settings.Default.Language);
				oldOptions.InstallLocation = Properties.Settings.Default.FactorioPath;
				oldOptions.UserDataLocation = Properties.Settings.Default.FactorioUserDataPath;
				oldOptions.GenerationType = (DataCache.GenerationType)(Properties.Settings.Default.GenerationType);
				oldOptions.NormalDifficulty = (Properties.Settings.Default.FactorioNormalDifficulty);
				oldOptions.UseSaveFileData = (Properties.Settings.Default.UseSaveFileData);

				using (SettingsForm form = new SettingsForm(oldOptions))
				{
					form.StartPosition = FormStartPosition.Manual;
					form.Left = this.Left + 250;
					form.Top = this.Top + 25;
					form.ShowDialog();
					reload = form.ReloadRequested;

					if (!oldOptions.Equals(form.CurrentOptions) || reload) //some changes have been made OR reload was requested
					{
						DataCache.LocaleFiles.Clear();
						DataCache.LoadLocaleFiles((form.CurrentOptions.selectedLanguage == null)? "en" : form.CurrentOptions.selectedLanguage.Name);
						if(form.CurrentOptions.selectedLanguage != null)
							Properties.Settings.Default.Language = form.CurrentOptions.selectedLanguage.Name;

						Properties.Settings.Default.FactorioPath = form.CurrentOptions.InstallLocation;
						Properties.Settings.Default.FactorioUserDataPath = form.CurrentOptions.UserDataLocation;
						Properties.Settings.Default.GenerationType = (int)(form.CurrentOptions.GenerationType);
						Properties.Settings.Default.FactorioNormalDifficulty = form.CurrentOptions.NormalDifficulty;
						Properties.Settings.Default.UseSaveFileData = form.CurrentOptions.UseSaveFileData;

						Properties.Settings.Default.EnabledMods.Clear();
						foreach (KeyValuePair<Mod, bool> kvp in form.CurrentOptions.Mods)
						{
							kvp.Key.Enabled = kvp.Value;
							if (kvp.Value)
								Properties.Settings.Default.EnabledMods.Add(kvp.Key.Name);
						}

						Properties.Settings.Default.Save();

						GraphViewer.LoadFromJson(JObject.Parse(JsonConvert.SerializeObject(GraphViewer)));
						GraphViewer.UpdateNodes();
						UpdateControlValues();
					}
				}
			} while (reload);
		}

		private void ItemListView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				AddItemButton.PerformClick();
				e.Handled = true;
			}
		}

		private void FilterTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (ItemListView.Items.Count == 0)
			{
				return;
			}
			int currentSelection;
			if (ItemListView.SelectedIndices.Count == 0)
			{
				currentSelection = -1;
			}
			else
			{
				currentSelection = ItemListView.SelectedIndices[0];
			}
			if (e.KeyCode == Keys.Down)
			{
				int newSelection = currentSelection + 1;
				if (newSelection >= ItemListView.Items.Count) newSelection = ItemListView.Items.Count - 1;
				if (newSelection <= 0) newSelection = 0;
				ItemListView.SelectedIndices.Clear();
				ItemListView.SelectedIndices.Add(newSelection);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up)
			{
				int newSelection = currentSelection - 1;
				if (newSelection == -1) newSelection = 0;
				ItemListView.SelectedIndices.Clear();
				ItemListView.SelectedIndices.Add(newSelection);
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Enter)
			{
				AddItemButton.PerformClick();
			}
		}

		private void UpdateControlValues()
		{
			fixedAmountButton.Checked = GraphViewer.Graph.SelectedAmountType == AmountType.FixedAmount;
			rateButton.Checked = GraphViewer.Graph.SelectedAmountType == AmountType.Rate;
			if (GraphViewer.Graph.SelectedUnit == RateUnit.PerSecond)
				rateOptionsDropDown.SelectedIndex = 0;
			else
				rateOptionsDropDown.SelectedIndex = 1;

            AssemblerDisplayCheckBox.Checked = GraphViewer.ShowAssemblers;
			MinerDisplayCheckBox.Checked = GraphViewer.ShowMiners;

			UpdateEDCheckLists();
			UpdateVisibleRecipeList();
			UpdateVisibleItemList();

			GraphViewer.Invalidate();
		}

		private void AddRecipeButton_Click(object sender, EventArgs e)
		{
			foreach (int index in RecipeListView.SelectedIndices)
			{				
				Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));

				NodeElement newElement = new NodeElement(RecipeNode.Create(filteredRecipeList[index].Tag as Recipe, GraphViewer.Graph), GraphViewer);
				newElement.Update();
				newElement.Location = location;
			}

			GraphViewer.Graph.UpdateNodeValues();
		}
		
		private void RecipeListView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			HashSet<Recipe> draggedRecipes = new HashSet<Recipe>();
			foreach (int index in RecipeListView.SelectedIndices)
				draggedRecipes.Add(filteredRecipeList[index].Tag as Recipe);
			DoDragDrop(draggedRecipes, DragDropEffects.All);
		}

        private void MinorGridlinesDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
			int updatedGridUnit = 0;
			if (MinorGridlinesDropDown.SelectedIndex > 0)
				updatedGridUnit = 6 * (int)(Math.Pow(2, MinorGridlinesDropDown.SelectedIndex - 1));

			if(GraphViewer.CurrentGridUnit != updatedGridUnit)
            {
				GraphViewer.CurrentGridUnit = updatedGridUnit;
				GraphViewer.Invalidate();
            }

			Properties.Settings.Default.MinorGridlines = MinorGridlinesDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

		private void MajorGridlinesDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
			int updatedGridUnit = 0;
			if (MajorGridlinesDropDown.SelectedIndex > 0)
				updatedGridUnit = 6 * (int)(Math.Pow(2, MajorGridlinesDropDown.SelectedIndex - 1));

			if (GraphViewer.CurrentMajorGridUnit != updatedGridUnit)
			{
				GraphViewer.CurrentMajorGridUnit = updatedGridUnit;
				GraphViewer.Invalidate();
			}

			Properties.Settings.Default.MajorGridlines = MajorGridlinesDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

        private void ModuleDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
			switch(ModuleDropDown.SelectedIndex)
            {
				case 1:
					GraphViewer.Graph.defaultModuleSelector = ModuleSelector.Fastest;
					break;

				case 2:
					GraphViewer.Graph.defaultModuleSelector = ModuleSelector.Productive;
					break;

				case 0:
				default:
					GraphViewer.Graph.defaultModuleSelector = ModuleSelector.None;
					break;
            }

			Properties.Settings.Default.DefaultModules = ModuleDropDown.SelectedIndex;
			Properties.Settings.Default.Save();
		}

        private void gridlinesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
			if (GraphViewer.ShowGrid != GridlinesCheckbox.Checked)
			{
				GraphViewer.ShowGrid = GridlinesCheckbox.Checked;
				GraphViewer.Invalidate();
			}

			Properties.Settings.Default.AltGridlines = (GridlinesCheckbox.Checked);
			Properties.Settings.Default.Save();
		}

		private void GraphViewer_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				GraphViewer.ShowGrid = !GraphViewer.ShowGrid;
				GridlinesCheckbox.Checked = GraphViewer.ShowGrid;
			}
		}

		private void PauseUpdatesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
			GraphViewer.Graph.PauseUpdates = PauseUpdatesCheckbox.Checked;
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
			GraphViewer.Invalidate();
        }

        private void AlignSelectionButton_Click(object sender, EventArgs e)
        {
			GraphViewer.AlignSelected();
        }

        private void ListTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (ListTabControl.SelectedIndex == 0) //the item tab
				UpdateVisibleItemList();
			else if(ListTabControl.SelectedIndex == 1) //the recipes tab
				UpdateVisibleRecipeList();
			GraphViewer.Invalidate();
		}

        private void AssemblerSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
			if (!AssemblerSelectionBoxItemUpdating)
			{
				Assembler assembler = (Assembler)AssemblerSelectionBox.Items[e.Index];
				assembler.Enabled = (e.NewValue == CheckState.Checked);
				if (assembler.Enabled)
					Properties.Settings.Default.EnabledAssemblers.Add(assembler.Name);
				else
					Properties.Settings.Default.EnabledAssemblers.Remove(assembler.Name);
				Properties.Settings.Default.Save();
				GraphViewer.Invalidate();
			}
		}

		private void AssemblerSelectionBoxAllButton_Click(object sender, EventArgs e)
        {
			AssemblerSelectionBoxItemUpdating = true;

			for (int i = 0; i < AssemblerSelectionBox.Items.Count; i++)
				AssemblerSelectionBox.SetItemChecked(i, true);
			Properties.Settings.Default.EnabledAssemblers.Clear();
			foreach(Assembler assembler in DataCache.Assemblers.Values)
            {
				assembler.Enabled = true;
				Properties.Settings.Default.EnabledAssemblers.Add(assembler.Name);
            }
			Properties.Settings.Default.Save();
			DataCache.UpdateRecipesAssemblerStatus();
			GraphViewer.Invalidate();

			AssemblerSelectionBoxItemUpdating = false;
		}

		private void AssemblerSelectionBoxNoneButton_Click(object sender, EventArgs e)
        {
			AssemblerSelectionBoxItemUpdating = true;
	
			for (int i = 0; i < AssemblerSelectionBox.Items.Count; i++)
				AssemblerSelectionBox.SetItemChecked(i, false);

			Properties.Settings.Default.EnabledAssemblers.Clear();
			foreach (Assembler assembler in DataCache.Assemblers.Values)
			{
				assembler.Enabled = false;
			}
			Properties.Settings.Default.Save();
			DataCache.UpdateRecipesAssemblerStatus();
			GraphViewer.Invalidate();

			AssemblerSelectionBoxItemUpdating = false;
		}

		private void MinerSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
			if (!MinerSelectionBoxItemUpdating)
			{
				Miner miner = (Miner)MinerSelectionBox.Items[e.Index];
				miner.Enabled = (e.NewValue == CheckState.Checked);
				if (miner.Enabled)
					Properties.Settings.Default.EnabledMiners.Add(miner.Name);
				else
					Properties.Settings.Default.EnabledMiners.Remove(miner.Name);
				Properties.Settings.Default.Save();
				DataCache.UpdateRecipesAssemblerStatus();
				GraphViewer.Invalidate();
			}
		}

        private void MinerSelectionBoxAllButton_Click(object sender, EventArgs e)
        {
			MinerSelectionBoxItemUpdating = true;

			for (int i = 0; i < MinerSelectionBox.Items.Count; i++)
				MinerSelectionBox.SetItemChecked(i, true);
			Properties.Settings.Default.EnabledMiners.Clear();
			foreach (Miner miner in DataCache.Miners.Values)
			{
				miner.Enabled = true;
				Properties.Settings.Default.EnabledMiners.Add(miner.Name);
			}
			Properties.Settings.Default.Save();
			DataCache.UpdateRecipesAssemblerStatus();
			GraphViewer.Invalidate();

			MinerSelectionBoxItemUpdating = false;
		}

		private void MinerSelectionBoxNoneButton_Click(object sender, EventArgs e)
        {
			MinerSelectionBoxItemUpdating = true;

			for (int i = 0; i < MinerSelectionBox.Items.Count; i++)
				MinerSelectionBox.SetItemChecked(i, false);
			Properties.Settings.Default.EnabledMiners.Clear();
			foreach (Miner miner in DataCache.Miners.Values)
			{
				miner.Enabled = false;
			}
			Properties.Settings.Default.Save();
			DataCache.UpdateRecipesAssemblerStatus();
			GraphViewer.Invalidate();

			MinerSelectionBoxItemUpdating = false;
		}

		private void ModuleSelectionBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
			if (!ModuleSelectionBoxItemUpdating)
			{
				Module module = (Module)ModuleSelectionBox.Items[e.Index];
				module.Enabled = (e.NewValue == CheckState.Checked);
				if (module.Enabled)
					Properties.Settings.Default.EnabledModules.Add(module.Name);
				else
					Properties.Settings.Default.EnabledModules.Remove(module.Name);
				Properties.Settings.Default.Save();
			}
		}

        private void ModuleSelectionBoxAllButton_Click(object sender, EventArgs e)
        {
			ModuleSelectionBoxItemUpdating = true;
			for (int i = 0; i < ModuleSelectionBox.Items.Count; i++)
				ModuleSelectionBox.SetItemChecked(i, true);
			Properties.Settings.Default.EnabledModules.Clear();
			foreach (Module module in DataCache.Modules.Values)
			{
				module.Enabled = true;
				Properties.Settings.Default.EnabledModules.Add(module.Name);
			}
			Properties.Settings.Default.Save();
			ModuleSelectionBoxItemUpdating = false;
		}

        private void ModuleSelectionBoxNoneButton_Click(object sender, EventArgs e)
        {
			ModuleSelectionBoxItemUpdating = true;
			for (int i = 0; i < ModuleSelectionBox.Items.Count; i++)
				ModuleSelectionBox.SetItemChecked(i, false);
			Properties.Settings.Default.EnabledModules.Clear();
			foreach (Module module in DataCache.Modules.Values)
			{
				module.Enabled = false;
			}
			Properties.Settings.Default.Save();
			ModuleSelectionBoxItemUpdating = false;
		}

		private void ShowHiddenItemsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.ShowHidden = ShowHiddenItemsCheckBox.Checked;
			if (ShowHiddenItemsCheckBox.Checked != ShowDisabledRecipesCheckBox.Checked)
				ShowDisabledRecipesCheckBox.Checked = ShowHiddenItemsCheckBox.Checked;
			Properties.Settings.Default.Save();
			UpdateVisibleItemList();
		}

		private void ShowDisabledRecipesCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			Properties.Settings.Default.ShowHidden = ShowDisabledRecipesCheckBox.Checked;
			if (ShowHiddenItemsCheckBox.Checked != ShowDisabledRecipesCheckBox.Checked)
				ShowHiddenItemsCheckBox.Checked = ShowDisabledRecipesCheckBox.Checked;
			Properties.Settings.Default.Save();
			UpdateVisibleRecipeList();
		}

		public static void SetDoubleBuffered(System.Windows.Forms.Control c)
		{
			if (System.Windows.Forms.SystemInformation.TerminalServerSession)
				return;
			System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered",
			System.Reflection.BindingFlags.NonPublic |
			System.Reflection.BindingFlags.Instance);
			aProp.SetValue(c, true, null);
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;
				return cp;
			}
		}
    }
}
