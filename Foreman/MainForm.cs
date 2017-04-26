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
		private List<ListViewItem> unfilteredItemList;
		private List<ListViewItem> unfilteredRecipeList;

		public MainForm()
		{
			InitializeComponent();
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//I changed the name of the variable, so this copies the value over for people who are upgrading their Foreman version
			if (Properties.Settings.Default.FactorioPath == "" && Properties.Settings.Default.FactorioDataPath != "")
			{
				Properties.Settings.Default["FactorioPath"] = Path.GetDirectoryName(Properties.Settings.Default.FactorioDataPath);
				Properties.Settings.Default["FactorioDataPath"] = "";
			}

			if (!Directory.Exists(Properties.Settings.Default.FactorioPath))
			{
				foreach (String defaultPath in new String[]{
												  Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"), "Factorio"),
												  Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), "Factorio"),
												  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Applications", "factorio.app", "Contents")}) //Not actually tested on a Mac
				{
					if (Directory.Exists(defaultPath))
					{
						Properties.Settings.Default["FactorioPath"] = defaultPath;
						Properties.Settings.Default.Save();
						break;
					}
				}
			}

			if (!Directory.Exists(Properties.Settings.Default.FactorioPath))
			{
				using (DirectoryChooserForm form = new DirectoryChooserForm(""))
				{
					if (form.ShowDialog() == DialogResult.OK)
					{
						Properties.Settings.Default["FactorioPath"] = form.SelectedPath; ;
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

			if (!Directory.Exists(Properties.Settings.Default.FactorioModPath))
			{
				if (Directory.Exists(Path.Combine(Properties.Settings.Default.FactorioPath, "mods")))
				{
					Properties.Settings.Default["FactorioModPath"] = Path.Combine(Properties.Settings.Default.FactorioPath, "mods");
				}
				else if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio", "mods")))
				{
					Properties.Settings.Default["FactorioModPath"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio", "mods");
				}
			}

			if (Properties.Settings.Default.EnabledMods == null) Properties.Settings.Default.EnabledMods = new StringCollection();
			if (Properties.Settings.Default.EnabledAssemblers == null) Properties.Settings.Default.EnabledAssemblers = new StringCollection();
			if (Properties.Settings.Default.EnabledMiners == null) Properties.Settings.Default.EnabledMiners = new StringCollection();
			if (Properties.Settings.Default.EnabledModules == null) Properties.Settings.Default.EnabledModules = new StringCollection();

		    if (Properties.Settings.Default.MinersUseModules) MinersUseModulesCheckBox.Checked = true;

            DataCache.LoadAllData(null);
			
			LanguageDropDown.Items.AddRange(DataCache.Languages.ToArray());
			LanguageDropDown.SelectedItem = DataCache.Languages.FirstOrDefault(l => l.Name == Properties.Settings.Default.Language);

			UpdateControlValues();
		}

		public void LoadItemList()
		{
			//Listview
			ItemListView.Items.Clear();
			unfilteredItemList = new List<ListViewItem>();
			if (DataCache.UnknownIcon != null)
			{
				ItemImageList.Images.Add(DataCache.UnknownIcon);
			}
			foreach (var item in DataCache.Items)
			{
				ListViewItem lvItem = new ListViewItem();
				if (item.Value.Icon != null)
				{
					ItemImageList.Images.Add(item.Value.Icon);
					lvItem.ImageIndex = ItemImageList.Images.Count - 1;
				}
				else
				{
					lvItem.ImageIndex = 0;
				}
				lvItem.Text = item.Value.FriendlyName;
				lvItem.Tag = item.Value;
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
				foreach (Recipe recipe in DataCache.Recipes.Values.Where(r => r.Enabled))
				{
					if (recipe.Results.ContainsKey(item))
					{
						optionList.Add(new RecipeChooserControl(recipe, $"Create '{recipe.FriendlyName}' recipe node", recipe.FriendlyName));
					}
				}
				optionList.Add(itemSupplyOption);
                
				foreach (Recipe recipe in DataCache.Recipes.Values.Where(r => r.Enabled))
				{
					if (recipe.Ingredients.ContainsKey(item))
					{
						optionList.Add(new RecipeChooserControl(recipe, $"Create '{recipe.FriendlyName}' recipe node", recipe.FriendlyName));
					}
				}

				var chooserPanel = new ChooserPanel(optionList, GraphViewer);

				Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));

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
						newElement.Location = Point.Add(location, new Size(-newElement.Width / 2, -newElement.Height / 2));
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

			MinerDisplayCheckBox.Checked
				= MinerDisplayCheckBox.Enabled
				= AssemblerDisplayCheckBox.Enabled
				= AssemblerDisplayCheckBox.Checked
				= !(sender as RadioButton).Checked;

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

		private void AutomaticCompleteButton_Click(object sender, EventArgs e)
		{
			GraphViewer.Graph.LinkUpAllInputs();
			GraphViewer.Graph.UpdateNodeValues();
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
			GraphViewer.ShowMiners = (sender as CheckBox).Checked;
			GraphViewer.Graph.UpdateNodeValues();
			GraphViewer.UpdateNodes();
		}

		private void SingleAssemblerPerRecipeCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			GraphViewer.Graph.OneAssemblerPerRecipe = (sender as CheckBox).Checked;
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
			else if (ItemListView.SelectedItems.Count > 1)
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

		private void FactorioDirectoryButton_Click(object sender, EventArgs e)
		{
			using (DirectoryChooserForm form = new DirectoryChooserForm(Properties.Settings.Default.FactorioPath))
			{
				form.Text = "Locate the factorio directory";
				if (form.ShowDialog() == DialogResult.OK)
				{
					Properties.Settings.Default["FactorioPath"] = form.SelectedPath;
					Properties.Settings.Default.Save();

					JObject savedGraph = JObject.Parse(JsonConvert.SerializeObject(GraphViewer));
					DataCache.LoadAllData(null);
					GraphViewer.LoadFromJson(savedGraph);
					UpdateControlValues();
				}
			}
		}

		private void ModDirectoryButton_Click(object sender, EventArgs e)
		{
			using (DirectoryChooserForm form = new DirectoryChooserForm(Properties.Settings.Default.FactorioModPath))
			{
				form.Text = "Locate the mods directory";
				if (form.ShowDialog() == DialogResult.OK)
				{
					Properties.Settings.Default["FactorioModPath"] = form.SelectedPath;
					Properties.Settings.Default.Save();

					JObject savedGraph = JObject.Parse(JsonConvert.SerializeObject(GraphViewer));
					DataCache.LoadAllData(null);
					GraphViewer.LoadFromJson(savedGraph);
					UpdateControlValues();
				}
			}
		}

		private void ReloadButton_Click(object sender, EventArgs e)
		{
			GraphViewer.LoadFromJson(JObject.Parse(JsonConvert.SerializeObject(GraphViewer)));
			UpdateControlValues();
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
				ErrorLogging.LogLine($"Error saving file '{dialog.FileName}'. Error: '{exception.Message}'");
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
				ErrorLogging.LogLine($"Error loading file '{dialog.FileName}'. Error: '{exception.Message}'");
			}

			UpdateControlValues();
			GraphViewer.Invalidate();
		}

		private void EnableDisableButton_Click(object sender, EventArgs e)
		{
			EnableDisableItemsForm form = new EnableDisableItemsForm();
			form.ShowDialog();
			SaveEnabledObjects();

			if (form.ModsChanged)
			{
				GraphViewer.LoadFromJson(JObject.Parse(JsonConvert.SerializeObject(GraphViewer)));
				UpdateControlValues();
			}
		}

		private void SaveEnabledObjects()
		{
			Properties.Settings.Default.EnabledMods.Clear();
			Properties.Settings.Default.EnabledAssemblers.Clear();
			Properties.Settings.Default.EnabledMiners.Clear();
			Properties.Settings.Default.EnabledModules.Clear();

			Properties.Settings.Default.EnabledMods.AddRange(DataCache.Mods.Select<Mod, string>(m => m.Name + "|" + m.Enabled.ToString()).ToArray());
			Properties.Settings.Default.EnabledAssemblers.AddRange(DataCache.Assemblers.Values.Select<Assembler, string>(a => a.Name + "|" + a.Enabled.ToString()).ToArray());
			Properties.Settings.Default.EnabledMiners.AddRange(DataCache.Miners.Values.Select<Miner, string>(m => m.Name + "|" + m.Enabled.ToString()).ToArray());
			Properties.Settings.Default.EnabledModules.AddRange(DataCache.Modules.Values.Select<Module, string>(m => m.Name + "|" + m.Enabled.ToString()).ToArray());

			Properties.Settings.Default.Save();
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

		private void LanguageDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			String newLocale = (LanguageDropDown.SelectedItem as Language).Name;

			DataCache.LocaleFiles.Clear();
			DataCache.LoadLocaleFiles(newLocale);

			GraphViewer.UpdateNodes();
			UpdateControlValues();

			Properties.Settings.Default["Language"] = newLocale;
			Properties.Settings.Default.Save();
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
			SingleAssemblerPerRecipeCheckBox.Checked = GraphViewer.Graph.OneAssemblerPerRecipe;
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
		}

		private void AddRecipeButton_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem lvItem in RecipeListView.SelectedItems)
			{				
				Point location = GraphViewer.ScreenToGraph(new Point(GraphViewer.Width / 2, GraphViewer.Height / 2));

				NodeElement newElement = new NodeElement(RecipeNode.Create((Recipe)lvItem.Tag, GraphViewer.Graph), GraphViewer);
				newElement.Update();
				newElement.Location = Point.Add(location, new Size(-newElement.Width / 2, -newElement.Height / 2));
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

        private void MinersUseModulesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            GraphViewer.MinersUseModules = ((CheckBox)sender).Checked;
            Properties.Settings.Default.MinersUseModules = ((CheckBox)sender).Checked;
            Properties.Settings.Default.Save();

            JObject savedGraph = JObject.Parse(JsonConvert.SerializeObject(GraphViewer));
            DataCache.LoadAllData(null);
            GraphViewer.LoadFromJson(savedGraph);
            UpdateControlValues();
        }
    }
}