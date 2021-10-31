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
using System.ComponentModel;
using System.Threading;

namespace Foreman
{
	public partial class MainForm : Form
	{
		private List<ListViewItem> unfilteredItemList;
		private List<ListViewItem> unfilteredRecipeList;

		public MainForm()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		private void Form1_Load(object sender, EventArgs e)
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

            ReloadFactorioData();
            UpdateControlValues();
        }

        private static void ReloadFactorioData()
        {
			using (DataReloadForm form = new DataReloadForm())
			{
				form.ShowDialog();
			}
        }

        public void LoadItemList()
		{
			//quick filter for only items used by the currently active recipes:
			HashSet<Item> availableItems = new HashSet<Item>();
			foreach(Recipe recipe in DataCache.Recipes.Values.Where(n => n.Enabled))
            {
				foreach (Item item in recipe.Ingredients.Keys)
					availableItems.Add(item);
				foreach (Item item in recipe.Results.Keys)
					availableItems.Add(item);
            }

			//Listview
			ItemListView.Items.Clear();
			unfilteredItemList = new List<ListViewItem>();
			if (DataCache.UnknownIcon != null)
			{
				ItemImageList.Images.Add(DataCache.UnknownIcon);
			}
			foreach (var item in availableItems)
			{
				ListViewItem lvItem = new ListViewItem();
				if (item.Icon != null)
				{
					ItemImageList.Images.Add(item.Icon);
					lvItem.ImageIndex = ItemImageList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}
				lvItem.Text = item.FriendlyName;
				lvItem.Tag = item;
				unfilteredItemList.Add(lvItem);
				ItemListView.Items.Add(lvItem);
			}

			ItemListView.Sorting = SortOrder.Ascending;
			ItemListView.Sort();
		}

		public void LoadRecipeList()
		{
			RecipeListView.Items.Clear();
			unfilteredRecipeList = new List<ListViewItem>();
			if (DataCache.UnknownIcon != null)
			{
				RecipeImageList.Images.Add(DataCache.UnknownIcon);
			}
			foreach (var recipe in DataCache.Recipes)
			{
				ListViewItem lvItem = new ListViewItem();
				if (recipe.Value.Icon != null)
				{
					RecipeImageList.Images.Add(recipe.Value.Icon);
					lvItem.ImageIndex = RecipeImageList.Images.Count - 1;
				} else
				{
					lvItem.ImageIndex = 0;
				}
				lvItem.Text = recipe.Value.FriendlyName;
				lvItem.Tag = recipe.Value;
				lvItem.Checked = recipe.Value.Enabled;
				unfilteredRecipeList.Add(lvItem);
				RecipeListView.Items.Add(lvItem);
			}

			RecipeListView.Sorting = SortOrder.Ascending;
			RecipeListView.Sort();
		}

		private void AddItemButton_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem lvItem in ItemListView.SelectedItems)
			{
				Item item = (Item)lvItem.Tag;
				NodeElement newElement = null;

				var itemSupplyOption = new ItemChooserControl(item, "Create infinite supply node", item.FriendlyName);
				var itemOutputOption = new ItemChooserControl(item, "Create output node", item.FriendlyName);

				var optionList = new List<ChooserControl>();

				optionList.Add(itemOutputOption);

				foreach (Recipe recipe in item.ProductionRecipes)
					if (recipe.Enabled)
						optionList.Add(new RecipeChooserControl(recipe, String.Format("Create '{0}' recipe node", recipe.FriendlyName), recipe.FriendlyName));

				optionList.Add(itemSupplyOption);

				foreach (Recipe recipe in item.ConsumptionRecipes)
					if (recipe.Enabled)
						optionList.Add(new RecipeChooserControl(recipe, String.Format("Create '{0}' recipe node", recipe.FriendlyName), recipe.FriendlyName));


				var chooserPanel = new ChooserPanel(optionList, GraphViewer, ChooserPanel.RecipeIconSize);

				Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));
				if (GraphViewer.ShowGrid)
				{
					location.X = GraphViewer.AlignToGrid(location.X);
					location.Y = GraphViewer.AlignToGrid(location.Y);
				}

				chooserPanel.Show(c =>
				{
					if (c != null)
					{
						if (c == itemSupplyOption)
						{
							newElement = new NodeElement(SupplyNode.Create(item, GraphViewer.Graph), GraphViewer);
						}
						else if (c is RecipeChooserControl)
						{
							newElement = new NodeElement(RecipeNode.Create((c as RecipeChooserControl).DisplayedRecipe, GraphViewer.Graph), GraphViewer);
						}
						else if (c == itemOutputOption)
						{
							newElement = new NodeElement(ConsumerNode.Create(item, GraphViewer.Graph), GraphViewer);
						}

						newElement.Update();
						newElement.Location = location;
					}
				});
			}

			GraphViewer.Graph.UpdateNodeValues();
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

		private void ItemListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ItemListView.SelectedItems.Count == 0)
			{
				AddItemButton.Enabled = false;
			}
			else if (ItemListView.SelectedItems.Count == 1)
			{
				AddItemButton.Enabled = true;
				AddItemButton.Text = "Add Item";
			}
			else if (ItemListView.SelectedItems.Count > 1) //disabled now
			{
				AddItemButton.Enabled = true;
				AddItemButton.Text = "Add Items";
			}
		}

		private void ItemListView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			AddItemButton.PerformClick();
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

		private void FilterTextBox_TextChanged(object sender, EventArgs e)
		{
			ItemListView.Items.Clear();
			ItemListView.Items.AddRange(unfilteredItemList.Where(i => i.Text.ToLower().Contains(ItemFilterTextBox.Text.ToLower())).ToArray());
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

			try
			{
				GraphViewer.LoadFromJson(JObject.Parse(File.ReadAllText(dialog.FileName)));
			}
			catch (Exception exception)
			{
				MessageBox.Show("Could not load this file. See log for more details");
				ErrorLogging.LogLine(String.Format("Error loading file '{0}'. Error: '{1}'", dialog.FileName, exception.Message));
			}

			UpdateControlValues();
			GraphViewer.Invalidate();
		}

		private void SettingsButton_Click(object sender, EventArgs e)
		{
			bool reload = false;
			do
			{
				SettingsForm.SettingsFormOptions oldOptions = new SettingsForm.SettingsFormOptions();
				foreach (Assembler assembler in DataCache.Assemblers.Values)
					oldOptions.Assemblers.Add(assembler, assembler.Enabled);
				foreach (Miner miner in DataCache.Miners.Values)
					oldOptions.Miners.Add(miner, miner.Enabled);
				foreach (Module module in DataCache.Modules.Values)
					oldOptions.Modules.Add(module, module.Enabled);
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

						Properties.Settings.Default.EnabledAssemblers.Clear();
						foreach (KeyValuePair<Assembler, bool> kvp in form.CurrentOptions.Assemblers)
						{
							kvp.Key.Enabled = kvp.Value;
							if (kvp.Value)
								Properties.Settings.Default.EnabledAssemblers.Add(kvp.Key.Name + "|" + kvp.Value.ToString());
						}

						Properties.Settings.Default.EnabledMiners.Clear();
						foreach (KeyValuePair<Miner, bool> kvp in form.CurrentOptions.Miners)
						{
							kvp.Key.Enabled = kvp.Value;
							if (kvp.Value)
								Properties.Settings.Default.EnabledMiners.Add(kvp.Key.Name + "|" + kvp.Value.ToString());
						}

						Properties.Settings.Default.EnabledModules.Clear();
						foreach (KeyValuePair<Module, bool> kvp in form.CurrentOptions.Modules)
						{
							kvp.Key.Enabled = kvp.Value;
							if (kvp.Value)
								Properties.Settings.Default.EnabledModules.Add(kvp.Key.Name + "|" + kvp.Value.ToString());
						}

						Properties.Settings.Default.EnabledMods.Clear();
						foreach (KeyValuePair<Mod, bool> kvp in form.CurrentOptions.Mods)
						{
							kvp.Key.Enabled = kvp.Value;
							if (kvp.Value)
								Properties.Settings.Default.EnabledMods.Add(kvp.Key.Name + "|" + kvp.Value.ToString());
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
			{
				rateOptionsDropDown.SelectedIndex = 0;
			}
			else
			{
				rateOptionsDropDown.SelectedIndex = 1;
			}

            AssemblerDisplayCheckBox.Checked = GraphViewer.ShowAssemblers;
			MinerDisplayCheckBox.Checked = GraphViewer.ShowMiners;

			LoadItemList();
			LoadRecipeList();

			GraphViewer.Invalidate();
		}

		private void ArrangeNodesButton_Click(object sender, EventArgs e)
		{
			GraphViewer.PositionNodes();
		}

		private void RecipeFilterTextBox_TextChanged(object sender, EventArgs e)
		{
			RecipeListView.Items.Clear();
			RecipeListView.Items.AddRange(unfilteredRecipeList.Where(r => r.Text.ToLower().Contains(RecipeFilterTextBox.Text.ToLower())).ToArray());
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

		private void RecipeListView_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			((Recipe)e.Item.Tag).Enabled = e.Item.Checked;
			GraphViewer.Invalidate();
		}

		private void AddRecipeButton_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem lvItem in RecipeListView.SelectedItems)
			{				
				Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));

				NodeElement newElement = new NodeElement(RecipeNode.Create((Recipe)lvItem.Tag, GraphViewer.Graph), GraphViewer);
				newElement.Update();
				newElement.Location = location;
			}

			GraphViewer.Graph.UpdateNodeValues();
		}
		
		private void RecipeListView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			HashSet<Recipe> draggedRecipes = new HashSet<Recipe>();
			foreach (ListViewItem recipe in RecipeListView.SelectedItems)
			{
				draggedRecipes.Add((Recipe)recipe.Tag);
			}
			DoDragDrop(draggedRecipes, DragDropEffects.All);
		}

		private void RecipeListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (RecipeListView.SelectedItems.Count == 0)
			{
				AddRecipeButton.Enabled = false;
			}
			else if (RecipeListView.SelectedItems.Count == 1)
			{
				AddRecipeButton.Enabled = true;
				AddRecipeButton.Text = "Add Recipe";
			}
			else if (RecipeListView.SelectedItems.Count > 1)
			{
				AddRecipeButton.Enabled = true;
				AddRecipeButton.Text = "Add Recipes";
			}
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

        private void ListTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (ListTabControl.SelectedIndex == 0) //the item tab
				LoadItemList();
		}

        private void ListTabControl_KeyDown(object sender, KeyEventArgs e)
        {
			if (e.KeyCode == Keys.A && (e.Modifiers & Keys.Control) != 0)
				foreach (ListViewItem item in RecipeListView.Items)
					item.Selected = true;
        }
    }
}
