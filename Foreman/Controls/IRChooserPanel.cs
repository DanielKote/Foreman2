using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public abstract partial class IRChooserPanel : UserControl
	{
		public event EventHandler<EventArgs> PanelClosed;

		private static readonly Color SelectedGroupButtonBGColor = Color.SandyBrown;
		protected static readonly Color IRButtonDefaultColor = Color.FromArgb(255, 70, 70, 70);
		protected static readonly Color IRButtonHiddenColor = Color.FromArgb(255, 120, 0, 0);
		protected static readonly Color IRButtonNoAssemblerColor = Color.FromArgb(255, 100, 100, 0);
		protected static readonly Color IRButtonUnavailableColor = Color.FromArgb(255, 170, 10, 160);


		private NFButton[,] IRButtons;
		private List<NFButton> GroupButtons = new List<NFButton>();
		private Dictionary<Group, NFButton> GroupButtonLinks = new Dictionary<Group, NFButton>();
		private List<KeyValuePair<DataObjectBase, Color>[]> filteredIRRowsList = new List<KeyValuePair<DataObjectBase, Color>[]>(); //updated on every filter command & group selection. Represents the full set of items/recipes in the IRFlowPanel (the visible ones will come from this set based on scrolling), with each array being size 10 (#buttons/line). bool (value) is the 'use BW icon'
		protected int CurrentRow { get; private set; } //used to ensure we dont update twice when filtering or group change (once due to update request, second due to setting scroll bar value to 0)

		protected List<Group> SortedGroups;
		protected Group SelectedGroup; //provides some continuity between selections - if you last selected from the intermediates group for example, adding another recipe will select that group as the starting group
		private static Group StartingGroup;
		protected ProductionGraphViewer PGViewer;

		protected abstract ToolTip IRButtonToolTip { get; }
		private CustomToolTip GroupButtonToolTip;

		protected abstract List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList();
		protected abstract void IRButton_MouseUp(object sender, MouseEventArgs e);
		//protected abstract void IRButton_Hover(object sender, EventArgs e);

		protected bool ShowUnavailable { get; private set; }

		public IRChooserPanel(ProductionGraphViewer parent, Point originPoint)
		{
			PGViewer = parent;
			this.DoubleBuffered = true;
			this.ShowUnavailable = Properties.Settings.Default.ShowUnavailable;

			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			this.Disposed += IRChooserPanel_Disposed;
			this.Anchor = AnchorStyles.Top | AnchorStyles.Left;

			IRButtons = new NFButton[IRTable.ColumnCount - 1, IRTable.RowCount];

			GroupButtonToolTip = new CustomToolTip();

			IRScrollBar.Minimum = 0;
			IRScrollBar.Maximum = 0;
			IRScrollBar.Enabled = false;
			IRScrollBar.SmallChange = 1;
			IRScrollBar.LargeChange = IRTable.RowCount;
			CurrentRow = 0;

			IRTable.MouseWheel += new MouseEventHandler(IRFlowPanel_MouseWheel);

			ShowHiddenCheckBox.Checked = Properties.Settings.Default.ShowHidden;
			IgnoreAssemblerCheckBox.Checked = Properties.Settings.Default.IgnoreAssemblerStatus;
			RecipeNameOnlyFilterCheckBox.Checked = Properties.Settings.Default.RecipeNameOnlyFilter;

			this.Location = originPoint;
		}

		public new void Show()
		{
			InitializeButtons();
			SetSelectedGroup(null);

			//set up the event handlers last so as not to cause unexpected calls when setting checked status ob checkboxes
			ShowHiddenCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);
			IgnoreAssemblerCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);

			PGViewer.Controls.Add(this);
			this.BringToFront();
			PGViewer.PerformLayout();
			this.Focus();
			FilterTextBox.Focus();
		}

		//-----------------------------------------------------------------------------------------------------Button initialization & update

		private void InitializeButtons()
		{
			//initialize the group buttons
			SortedGroups = GetSortedGroups();

			GroupTable.SuspendLayout();

			//add any extra rows to handle the amount of sorted groups
			for (int i = 0; i < (SortedGroups.Count - 1) / GroupTable.ColumnCount; i++)
				GroupTable.RowStyles.Add(new RowStyle(GroupTable.RowStyles[0].SizeType, GroupTable.RowStyles[0].Height));

			//add in the group buttons
			for (int i = 0; i < SortedGroups.Count; i++)
			{
				NFButton button = new NFButton();
				button.BackColor = Color.DimGray;
				button.UseVisualStyleBackColor = false;
				button.FlatStyle = FlatStyle.Flat;
				button.FlatAppearance.BorderSize = 0;
				button.TabStop = false;
				button.Margin = new Padding(0);
				button.Size = new Size(1, 1);
				button.Dock = DockStyle.Fill;
				button.Image = new Bitmap(SortedGroups[i].Icon, (int)(GroupTable.Width / (GroupTable.ColumnCount * 1.1f)), (int)(GroupTable.Width / (GroupTable.ColumnCount * 1.1f)));
				button.Tag = SortedGroups[i];

				GroupButtonToolTip.SetToolTip(button, string.IsNullOrEmpty(SortedGroups[i].FriendlyName) ? "-" : SortedGroups[i].FriendlyName);

				button.Click += new EventHandler(GroupButton_Click);
				button.MouseHover += new EventHandler(GroupButton_MouseHover);
				button.MouseLeave += new EventHandler(GroupButton_MouseLeave);

				GroupButtons.Add(button);
				GroupButtonLinks.Add(SortedGroups[i], button);

				GroupTable.Controls.Add(button, i % GroupTable.ColumnCount, i / GroupTable.ColumnCount);
			}
			GroupTable.ResumeLayout();

			//initialize the item/recipe buttons
			IRTable.SuspendLayout();
			for (int column = 0; column < IRButtons.GetLength(0); column++)
			{
				for (int row = 0; row < IRButtons.GetLength(1); row++)
				{
					NFButton button = new NFButton();
					button.BackgroundImageLayout = ImageLayout.Zoom;
					button.UseVisualStyleBackColor = false;
					button.FlatStyle = FlatStyle.Flat;
					button.FlatAppearance.BorderSize = Math.Max(1, IRTable.Width / (IRButtons.GetLength(0) * 24));
					button.TabStop = false;
					button.ForeColor = Color.Gray;
					button.BackColor = Color.DimGray;
					button.Margin = new Padding(1);
					button.Size = new Size(1, 1);
					button.Dock = DockStyle.Fill;
					button.BackgroundImage = null;
					button.Tag = null;
					button.Enabled = false;

					button.MouseUp += new MouseEventHandler(IRButton_MouseUp);
					button.MouseHover += new EventHandler(IRButton_MouseHover);
					button.MouseLeave += new EventHandler(IRButton_MouseLeave);
					IRButtons[column, row] = button;

					IRTable.Controls.Add(button, column, row);
				}
			}
			IRTable.ResumeLayout();
		}

		protected abstract List<Group> GetSortedGroups();

		private long updateID = 0;
		protected async void UpdateIRButtons(int startRow = 0, bool scrollOnly = false) //if scroll only, then we dont need to update the filtered set, just use what is there
		{
			long currentID = ++updateID;

			await Task.Run(() =>
			{
				//if we are actually changing the filtered list, then update it (through the GetSubgroupList)
				if (!scrollOnly)
				{
					filteredIRRowsList.Clear();
					int currentRow = 0;
					foreach (List<KeyValuePair<DataObjectBase, Color>> sgList in GetSubgroupList().Where(n => n.Count > 0))
					{
						filteredIRRowsList.Add(new KeyValuePair<DataObjectBase, Color>[10]);
						int currentColumn = 0;
						foreach (KeyValuePair<DataObjectBase, Color> kvp in sgList)
						{
							if (currentColumn == IRButtons.GetLength(0))
							{
								filteredIRRowsList.Add(new KeyValuePair<DataObjectBase, Color>[10]);
								currentColumn = 0;
								currentRow++;
							}
							filteredIRRowsList[currentRow][currentColumn] = kvp;
							currentColumn++;
						}
						currentRow++;
					}
				}

				bool so = scrollOnly;
				this.UIThread(delegate
				{
					if (currentID != updateID)
						return;

					if (!so)
					{
						IRScrollBar.Maximum = Math.Max(0, filteredIRRowsList.Count - 1);
						IRScrollBar.Enabled = IRScrollBar.Maximum >= IRScrollBar.LargeChange;
					}
					CurrentRow = startRow;
					IRScrollBar.Value = startRow;

					IRTable.ResumeLayout();
					IRTable.SuspendLayout();
				});

				//update all the buttons to be based off of the filteredIRSet
				for (int column = 0; column < IRButtons.GetLength(0); column++)
				{
					for (int row = 0; row < IRButtons.GetLength(1); row++)
					{
						if (currentID != updateID)
							return;

						int c = column;
						int r = row;
						this.UIThread(delegate
						{
							if (currentID != updateID)
								return;

							DataObjectBase irObject = (r + startRow < filteredIRRowsList.Count) ? filteredIRRowsList[r + startRow][c].Key : null;
							NFButton b = IRButtons[c, r];
							if (irObject != null) //full
							{

								b.ForeColor = Color.Black;
								b.BackColor = (r + startRow < filteredIRRowsList.Count) ? filteredIRRowsList[r + startRow][c].Value : Color.DimGray;
								b.BackgroundImage = irObject.Icon;
								b.Tag = irObject;
								b.Enabled = true;
								IRButtonToolTip.SetToolTip(b, string.IsNullOrEmpty(irObject.FriendlyName) ? "-" : irObject.FriendlyName);
							}
							else
							{
								b.ForeColor = Color.Gray;
								b.BackColor = Color.DimGray;
								b.BackgroundImage = null;
								b.Tag = null;
								b.Enabled = false;
							}
						});
					}
				}
			});
			this.UIThread(delegate
			{
				if (currentID != updateID)
					return;
				IRTable.ResumeLayout();
			});
		}

		protected void SetSelectedGroup(Group sGroup, bool causeUpdate = true)
		{
			if (sGroup == null || !SortedGroups.Contains(sGroup)) //want to select the starting group, then update all buttons (including a possibility of group change)
			{
				sGroup = SortedGroups.Contains(StartingGroup) ? StartingGroup : SortedGroups[0];
				StartingGroup = sGroup;
				SelectedGroup = sGroup;
				UpdateIRButtons();
			}
			else
			{
				foreach (NFButton groupButton in GroupButtons)
					groupButton.BackColor = ((Group)(groupButton.Tag) == sGroup) ? SelectedGroupButtonBGColor : Color.DimGray;
				if (SelectedGroup != sGroup)
				{
					StartingGroup = sGroup;
					SelectedGroup = sGroup;
					if (causeUpdate)
						UpdateIRButtons();
				}
			}
		}

		protected void UpdateGroupButton(Group group, bool enabled)
		{
			this.UIThread(delegate
			{
				GroupButtonLinks[group].Enabled = enabled;
			});
		}

		//-----------------------------------------------------------------------------------------------------Group Button events

		private void GroupButton_Click(object sender, EventArgs e)
		{
			SetSelectedGroup((Group)((NFButton)sender).Tag);
		}

		private void GroupButton_MouseHover(object sender, EventArgs e)
		{
			Control control = (Control)sender;
			GroupButtonToolTip.SetText(GroupButtonToolTip.GetToolTip(control));
			GroupButtonToolTip.Show(control, new Point(control.Width, 10));
		}

		private void GroupButton_MouseLeave(object sender, EventArgs e)
		{
			GroupButtonToolTip.Hide((Control)sender);
		}

		//-----------------------------------------------------------------------------------------------------IR button events (including scrolling)

		private void IRPanelScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			if (e.NewValue != CurrentRow)
				UpdateIRButtons(e.NewValue, true);
		}

		private void IRFlowPanel_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta < 0 && IRScrollBar.Value <= (IRScrollBar.Maximum - IRScrollBar.LargeChange))
			{
				IRScrollBar.Value++;
				UpdateIRButtons(IRScrollBar.Value, true);
			}
			else if (e.Delta > 0 && IRScrollBar.Value > 0)
			{
				IRScrollBar.Value--;
				UpdateIRButtons(IRScrollBar.Value, true);
			}
		}

		internal virtual void IRButton_MouseHover(object sender, EventArgs e)
		{
			Control control = (Control)sender;
			(IRButtonToolTip as CustomToolTip).SetText(IRButtonToolTip.GetToolTip(control));
			(IRButtonToolTip as CustomToolTip).Show(control, new Point(control.Width, 10));
		}
		private void IRButton_MouseLeave(object sender, EventArgs e)
		{
			IRButtonToolTip.Hide((Control)sender);
		}

		//-----------------------------------------------------------------------------------------------------Filter

		protected void FilterCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateIRButtons();
		}

		private void FilterTextBox_TextChanged(object sender, EventArgs e)
		{
			UpdateIRButtons();
		}

		//-----------------------------------------------------------------------------------------------------Closing functions

		private void IRChooserPanel_Leave(object sender, EventArgs e)
		{
			Dispose();
		}

		protected virtual void IRChooserPanel_Disposed(object sender, EventArgs e)
		{
			Properties.Settings.Default.ShowHidden = ShowHiddenCheckBox.Checked;
			Properties.Settings.Default.IgnoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
			Properties.Settings.Default.RecipeNameOnlyFilter = RecipeNameOnlyFilterCheckBox.Checked;
			Properties.Settings.Default.Save();
			PanelClosed?.Invoke(this, EventArgs.Empty);
		}
	}

	public class ItemChooserPanel : IRChooserPanel
	{
		public event EventHandler<ItemRequestArgs> ItemRequested;

		private ToolTip iToolTip = new CustomToolTip();
		protected override ToolTip IRButtonToolTip { get { return iToolTip; } }
		private Item selectedItem;

		public ItemChooserPanel(ProductionGraphViewer parent, Point originPoint) : base(parent, originPoint) { }

		protected override void IRChooserPanel_Disposed(object sender, EventArgs e)
		{
			base.IRChooserPanel_Disposed(sender, e);
			if (selectedItem != null)
				ItemRequested?.Invoke(this, new ItemRequestArgs(selectedItem));
		}

		protected override List<Group> GetSortedGroups()
		{
			List<Group> groups = new List<Group>();
			foreach (Group group in ShowUnavailable ? PGViewer.DCache.Groups.Values : PGViewer.DCache.AvailableGroups)
			{
				int itemCount = 0;
				foreach (Subgroup sgroup in group.Subgroups)
					itemCount += ShowUnavailable ? sgroup.Items.Count : sgroup.Items.Count(i => i.Available);
				if (itemCount > 0)
					groups.Add(group);
			}
			groups.Sort();
			return groups;
		}

		protected override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList()
		{
			//step 1: calculate the visible items within each group (used to disable any group button with 0 items, plus shift the selected group if it contains 0 items)
			string filterString = FilterTextBox.Text.ToLower();
			bool ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
			bool showHidden = ShowHiddenCheckBox.Checked;

			Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>> filteredItems = new Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>>();
			Dictionary<Group, int> filteredItemCount = new Dictionary<Group, int>();
			foreach (Group group in SortedGroups)
			{
				int itemCounter = 0;
				List<List<KeyValuePair<DataObjectBase, Color>>> sgList = new List<List<KeyValuePair<DataObjectBase, Color>>>();
				foreach (Subgroup sgroup in group.Subgroups)
				{
					List<KeyValuePair<DataObjectBase, Color>> itemList = new List<KeyValuePair<DataObjectBase, Color>>();
					foreach (Item item in sgroup.Items.Where(n => ((ShowUnavailable || n.Available) && (n.LFriendlyName.Contains(filterString) || n.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1))))
					{
						bool visible = (ShowUnavailable || item.Available) &&
							((item.ConsumptionRecipes.Any(r => r.Enabled && (ShowUnavailable || r.Available))) ||
							(item.ProductionRecipes.Any(r => r.Enabled && (ShowUnavailable || r.Available))));

						bool validAssembler =
							(item.ConsumptionRecipes.Any(r => (ShowUnavailable || r.Available) && r.Assemblers.Any(a => a.Enabled && (ShowUnavailable || a.Available)))) ||
							(item.ProductionRecipes.Any(r => (ShowUnavailable || r.Available) && r.Assemblers.Any(a => a.Enabled && (ShowUnavailable || a.Available))));


						Color bgColor = (visible && item.Available) ? validAssembler ? IRButtonDefaultColor : IRButtonNoAssemblerColor : IRButtonHiddenColor;

						if ((visible || showHidden) && (validAssembler || ignoreAssemblerStatus))
						{
							itemCounter++;
							itemList.Add(new KeyValuePair<DataObjectBase, Color>(item, bgColor));
						}
					}
					sgList.Add(itemList);
				}
				filteredItems.Add(group, sgList);
				filteredItemCount.Add(group, itemCounter);
				UpdateGroupButton(group, (itemCounter != 0));
			}

			//step 2: select working group (currently selected group, or if it has 0 items then the first group with >0 items to the left, then the first group with >0 items to the right, then itself)
			Group alternateGroup = null;
			if (filteredItemCount[SelectedGroup] == 0)
			{
				int selectedGroupIndex = 0;
				for (int i = 0; i < SortedGroups.Count; i++)
					if (SortedGroups[i] == SelectedGroup)
						selectedGroupIndex = i;
				for (int i = selectedGroupIndex; i >= 0; i--)
					if (filteredItemCount[SortedGroups[i]] > 0)
						alternateGroup = SortedGroups[i];
				if (alternateGroup == null)
					for (int i = selectedGroupIndex; i < SortedGroups.Count; i++)
						if (filteredItemCount[SortedGroups[i]] > 0)
							alternateGroup = SortedGroups[i];
				if (alternateGroup == null)
					alternateGroup = SelectedGroup;
			}
			SetSelectedGroup(alternateGroup == null ? SelectedGroup : alternateGroup, false);

			//now the base class will take care of setting up the buttons based on the filtered items
			return filteredItems[SelectedGroup];
		}

		protected override void IRButton_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				selectedItem = (Item)((Button)sender).Tag;
				Dispose();
			}
		}
	}

	public class RecipeChooserPanel : IRChooserPanel
	{
		public event EventHandler<RecipeRequestArgs> RecipeRequested;

		protected Item KeyItem;
		protected fRange KeyItemTempRange;

		private ToolTip rToolTip = new RecipeToolTip();
		protected override ToolTip IRButtonToolTip { get { return rToolTip; } }

		public RecipeChooserPanel(ProductionGraphViewer parent, Point originPoint, Item item, fRange tempRange, NewNodeType nodeType) : base(parent, originPoint)
		{
			bool asIngredient = (nodeType == NewNodeType.Consumer || nodeType == NewNodeType.Disconnected);
			bool asProduct = (nodeType == NewNodeType.Supplier || nodeType == NewNodeType.Disconnected);

			AsIngredientCheckBox.Checked = asIngredient;
			AsProductCheckBox.Checked = asProduct;
			ShowHiddenCheckBox.Text = "Show Disabled";

			AddConsumerButton.Click += AddConsumerButton_Click;
			AddPassthroughButton.Click += AddPassthroughButton_Click;
			AddSupplyButton.Click += AddSupplyButton_Click;
			AsIngredientCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
			AsProductCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
			AsFuelCheckBox.CheckedChanged += FilterCheckBox_CheckedChanged;
			RecipeNameOnlyFilterCheckBox.CheckedChanged += new EventHandler(FilterCheckBox_CheckedChanged);

			KeyItem = item;
			KeyItemTempRange = (nodeType == NewNodeType.Disconnected) ? new fRange(0, 0, true) : tempRange; //cant use temp range if its a disconnected node

			RecipeNameOnlyFilterCheckBox.Visible = true;
			if (KeyItem == null)
			{
				OtherNodeOptionsTable.Visible = false;
			}
			else
			{
				ItemIconPanel.Visible = true;
				ItemIconPanel.BackgroundImage = KeyItem.Icon;
				OtherNodeOptionsTable.Visible = true;
				AddConsumerButton.Visible = asIngredient;
				AddSupplyButton.Visible = asProduct;

				bool hasConsumptionRecipes = KeyItem.ConsumptionRecipes.Count > 0;
				bool hasFuelConsumptionRecipes = KeyItem.FuelsEntities.FirstOrDefault(a => (a is Assembler assembler) && assembler.Enabled && assembler.Recipes.FirstOrDefault(r => r.Enabled) != null) != null;
				bool hasProductionRecipes = KeyItem.ProductionRecipes.Count > 0;
				bool hasFuelProductionRecipes = (KeyItem.FuelOrigin != null && KeyItem.FuelOrigin.FuelsEntities.Any(a => (a is Assembler assembler) && assembler.Enabled && assembler.Recipes.Any(r => r.Enabled)));

				if (!(asIngredient && (hasConsumptionRecipes || hasFuelConsumptionRecipes)) && !(asProduct && (hasProductionRecipes || hasFuelProductionRecipes))) //no valid recipes
				{
					GroupTable.Visible = false;
					IRTable.Visible = false;
					IRScrollBar.Visible = false;
					FilterTextBox.Visible = false;
					FilterLabel.Visible = false;
					RecipeNameOnlyFilterCheckBox.Visible = false;
					ShowHiddenCheckBox.Visible = false;
					IgnoreAssemblerCheckBox.Visible = false;
					ItemIconPanel.Location = new Point(4, 4);
				}
				else if (asIngredient && asProduct)
				{
					AsFuelCheckBox.Visible = (asIngredient && hasFuelConsumptionRecipes) || (asProduct && hasFuelProductionRecipes);
					AsIngredientCheckBox.Visible = true;
					AsProductCheckBox.Visible = true;
				}
				else if (asIngredient)
				{
					AsFuelCheckBox.Visible = (asIngredient && KeyItem.FuelsEntities.Count > 0);
				}
			}
		}

		protected override List<Group> GetSortedGroups()
		{
			List<Group> groups = new List<Group>();
			foreach (Group group in ShowUnavailable ? PGViewer.DCache.Groups.Values : PGViewer.DCache.AvailableGroups)
			{
				int recipeCount = 0;
				foreach (Subgroup sgroup in group.Subgroups)
					recipeCount += ShowUnavailable ? sgroup.Recipes.Count : sgroup.Recipes.Count(r => r.Available);
				if (recipeCount > 0)
					groups.Add(group);
			}
			groups.Sort();
			return groups;
		}

		protected override List<List<KeyValuePair<DataObjectBase, Color>>> GetSubgroupList()
		{
			//step 1: calculate the visible recipes for each group (those that pass filter & hidden status)
			string filterString = FilterTextBox.Text.ToLower();
			bool ignoreAssemblerStatus = IgnoreAssemblerCheckBox.Checked;
			bool checkRecipeIPs = !RecipeNameOnlyFilterCheckBox.Checked;
			bool showHidden = ShowHiddenCheckBox.Checked;
			bool includeSuppliers = AsProductCheckBox.Checked;
			bool includeConsumers = AsIngredientCheckBox.Checked;
			bool includeFuel = AsFuelCheckBox.Checked;
			bool ignoreItem = (KeyItem == null);

			Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>> filteredRecipes = new Dictionary<Group, List<List<KeyValuePair<DataObjectBase, Color>>>>();
			Dictionary<Group, int> filteredRecipeCount = new Dictionary<Group, int>();
			foreach (Group group in SortedGroups)
			{
				int recipeCounter = 0;
				List<List<KeyValuePair<DataObjectBase, Color>>> sgList = new List<List<KeyValuePair<DataObjectBase, Color>>>();
				foreach (Subgroup sgroup in group.Subgroups)
				{
					List<KeyValuePair<DataObjectBase, Color>> recipeList = new List<KeyValuePair<DataObjectBase, Color>>();
					//filter for the items first (simpler and clears out most recipes if there is a key item provided). NOTE: need to filter for use of item in ingredients, products, and any recipe that has an assembler that has this item as fuel or burn result
					foreach (Recipe recipe in sgroup.Recipes.Where(r => ignoreItem ||
						(includeConsumers && r.IngredientSet.ContainsKey(KeyItem)) ||
						(includeSuppliers && r.ProductSet.ContainsKey(KeyItem)) ||
						(includeConsumers && includeFuel && KeyItem.FuelsEntities.Count > 0 && r.Assemblers.Any(a => a.Fuels.Contains(KeyItem))) ||
						(includeSuppliers && includeFuel && KeyItem.FuelOrigin != null && r.Assemblers.Any(a => a.Fuels.Contains(KeyItem.FuelOrigin)))))
					{
						//quick hidden / enabled / available assembler check (done prior to name check for speed)
						if ((recipe.Enabled || showHidden) && (recipe.Assemblers.Any(a => a.Enabled) || ignoreAssemblerStatus) && (recipe.Available || ShowUnavailable))
						{
							//name check - have to check recipe name along with all ingredients and products (both friendly name and base name) - if selected
							if (recipe.LFriendlyName.Contains(filterString) ||
								recipe.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1 || (checkRecipeIPs && (
								recipe.IngredientList.Any(i => i.LFriendlyName.Contains(filterString) || i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1) ||
								recipe.ProductList.Any(i => i.LFriendlyName.Contains(filterString) || i.Name.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1))))
							{
								//further check for temperature
								if (KeyItemTempRange.Ignore ||
									(includeConsumers && recipe.IngredientTemperatureMap[KeyItem].Contains(KeyItemTempRange)) ||
									(includeSuppliers && KeyItemTempRange.Contains(recipe.ProductTemperatureMap[KeyItem])))
								{
									//holy... so - we finally finished all the checks, eh? Well, throw it on the pile of recipes to show then.
									Color bgColor = !recipe.Enabled ? IRButtonHiddenColor :
										(!recipe.Available || !recipe.Assemblers.Any(a => a.Available)) ? IRButtonUnavailableColor :
										!recipe.Assemblers.Any(a => a.Enabled) ? IRButtonNoAssemblerColor : IRButtonDefaultColor;
									recipeCounter++;
									recipeList.Add(new KeyValuePair<DataObjectBase, Color>(recipe, bgColor));
								}
							}
						}
					}
					sgList.Add(recipeList);
				}
				filteredRecipes.Add(group, sgList);
				filteredRecipeCount.Add(group, recipeCounter);
				UpdateGroupButton(group, (recipeCounter != 0));
			}

			//step 2: select working group (currently selected group, or if it has 0 recipes then the first group with >0 recipes to the left, then the first group with >0 recipes to the right, then itself)
			Group alternateGroup = null;
			if (filteredRecipeCount[SelectedGroup] == 0)
			{
				int selectedGroupIndex = 0;
				for (int i = 0; i < SortedGroups.Count; i++)
					if (SortedGroups[i] == SelectedGroup)
						selectedGroupIndex = i;
				for (int i = selectedGroupIndex; i >= 0; i--)
					if (filteredRecipeCount[SortedGroups[i]] > 0)
						alternateGroup = SortedGroups[i];
				if (alternateGroup == null)
					for (int i = selectedGroupIndex; i < SortedGroups.Count; i++)
						if (filteredRecipeCount[SortedGroups[i]] > 0)
							alternateGroup = SortedGroups[i];
				if (alternateGroup == null)
					alternateGroup = SelectedGroup;
			}
			SetSelectedGroup(alternateGroup == null ? SelectedGroup : alternateGroup, false);

			//now the base class will take care of setting up the buttons based on the filtered recipes
			return filteredRecipes[SelectedGroup];
		}

		protected override void IRButton_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) //select recipe
			{
				Recipe sRecipe = (Recipe)((Button)sender).Tag;
				RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Recipe, (Recipe)((Button)sender).Tag));

				if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
					Dispose();
			}
			else if (e.Button == MouseButtons.Right) //flip hidden status of recipe
			{
				Recipe selectedRecipe = (sender as NFButton).Tag as Recipe;
				selectedRecipe.Enabled = !selectedRecipe.Enabled;
				UpdateIRButtons();
			}
		}

		private void AddSupplyButton_Click(object sender, EventArgs e)
		{
			RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Supplier, null));

			if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
				Dispose();
		}

		private void AddConsumerButton_Click(object sender, EventArgs e)
		{
			RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Consumer, null));

			if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
				Dispose();
		}

		private void AddPassthroughButton_Click(object sender, EventArgs e)
		{
			RecipeRequested?.Invoke(this, new RecipeRequestArgs(NodeType.Passthrough, null));

			if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
				Dispose();
		}

		internal override void IRButton_MouseHover(object sender, EventArgs e)
		{
			Control control = (Control)sender;

			int yoffset = -control.Location.Y + 16 + Math.Max(-100, Math.Min(0, 348 - RecipeToolTip.GetRecipeToolTipHeight((Recipe)((Button)sender).Tag)));

			(IRButtonToolTip as RecipeToolTip).SetRecipe((Recipe)((Button)sender).Tag);
			(IRButtonToolTip as RecipeToolTip).Show(control, new Point(control.Width, yoffset));
		}
	}

	public class NFButton : Button
	{
		public NFButton() : base() { this.SetStyle(ControlStyles.Selectable, false); }
		protected override bool ShowFocusCues { get { return false; } }
	}

	public class RecipeRequestArgs : EventArgs
	{
		public Recipe Recipe;
		public NodeType NodeType;
		public RecipeRequestArgs(NodeType nodeType, Recipe recipe) { Recipe = recipe; NodeType = nodeType; }
	}

	public class ItemRequestArgs : EventArgs
	{
		public Item Item;
		public ItemRequestArgs(Item item) { Item = item; }
	}
}
