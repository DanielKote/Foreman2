using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{
	public enum LinkType { Input, Output };

	public partial class NodeElement : GraphElement
	{
		public int DragOffsetX;
		public int DragOffsetY;

		public bool Moused { get { return Parent.MousedNode == this; }}
		public Point MousePosition = Point.Empty;

		private Color recipeColour = Color.FromArgb(190, 217, 212);
		private Color supplyColour = Color.FromArgb(249, 237, 195);
		private Color consumerColour = Color.FromArgb(231, 214, 224);
		private Color backgroundColour;
		
		private const int assemblerSize = 32;
		private const int assemblerBorderX = 10;
		private const int assemblerBorderY = 30;

		private const int tabPadding = 8;
		
		TextBox editorBox;
		Item editedItem;
		float originalEditorValue;

		private AssemblerBox assemblerBox;
		private List<ItemTab> inputTabs = new List<ItemTab>();
		private List<ItemTab> outputTabs = new List<ItemTab>();
		
		public ProductionNode DisplayedNode { get; private set; }

		public NodeElement(ProductionNode node, ProductionGraphViewer parent) : base(parent)
		{
			Width = 100;
			Height = 80;

			DisplayedNode = node;

			if (DisplayedNode.GetType() == typeof(ConsumerNode))
			{
				backgroundColour = supplyColour;
			}
			else if (DisplayedNode.GetType() == typeof(SupplyNode))
			{
				backgroundColour = consumerColour;
			}
			else
			{
				backgroundColour = recipeColour;
			}

			foreach (Item item in node.Inputs)
			{
				ItemTab newTab = new ItemTab(item, LinkType.Input, Parent);
				SubElements.Add(newTab);
				inputTabs.Add(newTab);
			}
			foreach (Item item in node.Outputs)
			{
				ItemTab newTab = new ItemTab(item, LinkType.Output, Parent);
				SubElements.Add(newTab);
				outputTabs.Add(newTab);
			}

			if (DisplayedNode is RecipeNode)
			{
				assemblerBox = new AssemblerBox(Parent);
				SubElements.Add(assemblerBox);
				assemblerBox.Height = 50;
				assemblerBox.Width = Width;
				Height = 120;
			}
		}

		private int getIconWidths()
		{
			return Math.Max(GetInputIconWidths(), GetOutputIconWidths());
		}

		private int GetInputIconWidths()
		{
			int result = tabPadding;
			foreach (ItemTab tab in inputTabs)
			{
				result += tab.Width + tabPadding;
			}
			return result;
		}
		private int GetOutputIconWidths()
		{
			int result = tabPadding;
			foreach (ItemTab tab in outputTabs)
			{
				result += tab.Width + tabPadding;
			}
			return result;
		}
		
		public void Update()
		{
			UpdateTabOrder();

			Width = Math.Max(75, getIconWidths());

			int x = (Width - GetOutputIconWidths()) / 2;
			foreach (ItemTab tab in outputTabs)
			{
				x += tabPadding;
				tab.X = x;
				tab.Y = -tab.Height / 2;
				x += tab.Width;

				tab.FillColour = chooseIconColour(tab.Item, tab.Type);
				tab.Text = getIconString(tab.Item, tab.Type);
			}
			x = (Width - GetInputIconWidths()) / 2;
			foreach (ItemTab tab in inputTabs)
			{
				x += tabPadding;
				tab.X = x;
				tab.Y = Height - tab.Height / 2;
				x += tab.Width;

				tab.FillColour = chooseIconColour(tab.Item, tab.Type);
				tab.Text = getIconString(tab.Item, tab.Type);
			}

			if (DisplayedNode is RecipeNode)
			{
				assemblerBox.AssemblerList = (DisplayedNode as RecipeNode).GetMinimumAssemblers();
				assemblerBox.Width = Width;
				assemblerBox.X = (Width - assemblerBox.Width) / 2;
				assemblerBox.Y = (Height - assemblerBox.Height) / 2;
				assemblerBox.Update();
			}
		}

		public void UpdateTabOrder()
		{
			inputTabs = inputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();
			outputTabs = outputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();
		}

		public int GetItemTabXHeuristic(ItemTab tab)
		{
			int total = 0;
			if (tab.Type == LinkType.Input)
			{
				foreach (NodeLink link in DisplayedNode.InputLinks.Where(l => l.Item == tab.Item))
				{
					total += Parent.GetElementForNode(link.Supplier).X;
				}
			}
			else
			{
				foreach (NodeLink link in DisplayedNode.OutputLinks.Where(l => l.Item == tab.Item))
				{
					total += Parent.GetElementForNode(link.Consumer).X;
				}
			}
			return total;
		}

		public Point GetOutputLineConnectionPoint(Item item)
		{
			ItemTab tab = outputTabs.First(it => it.Item == item);
			return new Point(X + tab.X + tab.Width / 2, Y + tab.Y);
		}

		public Point GetInputLineConnectionPoint(Item item)
		{
			ItemTab tab = inputTabs.First(it => it.Item == item);
			return new Point(X + tab.X + tab.Width / 2, Y + tab.Y + tab.Height);
		}
		
		public override void Paint(Graphics graphics)
		{
			using (SolidBrush brush = new SolidBrush(backgroundColour))
			{
				GraphicsStuff.FillRoundRect(0, 0, Width, Height, 8, graphics, brush);
			}

			if (Parent.ClickedNode == this)
			{
				using (Pen pen = new Pen(Color.WhiteSmoke, 3f))
				{
					GraphicsStuff.DrawRoundRect(0, 0, Width, Height, 8, graphics, pen);
				}
			}
			else if (Parent.MousedNode == this)
			{
				using (Pen pen = new Pen(Color.LightGray, 3f))
				{
					GraphicsStuff.DrawRoundRect(0, 0, Width, Height, 8, graphics, pen);
				}
			}
			else if (Parent.SelectedNode == this)
			{
				using (Pen pen = new Pen(Color.DarkGray, 3f))
				{
					GraphicsStuff.DrawRoundRect(0, 0, Width, Height, 8, graphics, pen);
				}
			}

			if (editorBox != null)
			{
				TooltipInfo ttinfo = new TooltipInfo();
				ttinfo.ScreenLocation = Parent.GraphToScreen(GetInputLineConnectionPoint(editedItem));
				ttinfo.Direction = Direction.Up;
				ttinfo.ScreenSize = new Point(editorBox.Size);
				Parent.AddTooltip(ttinfo);
			}

			base.Paint(graphics);
		}

		private String getIconString(Item item, LinkType linkType)
		{
			String line1Format = "{0:0.##}{1}";
			String line2Format = "\n({0:0.##}{1})";
			String finalString = "";

			String unit = "";

			float actualAmount = 0; 
			float desiredAmount = 0;
			if (linkType == LinkType.Input)
			{
				actualAmount = DisplayedNode.GetTotalInput(item);
				desiredAmount = DisplayedNode.GetRequiredInput(item);
			}
			else
			{
				actualAmount = DisplayedNode.GetTotalOutput(item);
			}
			if (Parent.Graph.SelectedAmountType == AmountType.Rate && Parent.Graph.SelectedUnit == RateUnit.PerSecond)
			{
				unit = "/s";
			}
			else if (Parent.Graph.SelectedAmountType == AmountType.Rate && Parent.Graph.SelectedUnit == RateUnit.PerMinute)
			{
				unit = "/m";
				actualAmount *= 60;
				desiredAmount *= 60;
			}

			if (linkType == LinkType.Input)
			{
				finalString = String.Format(line1Format, actualAmount, unit);
				if (DisplayedNode.GetRequiredInput(item) > DisplayedNode.GetTotalInput(item))
				{
					finalString += String.Format(line2Format, desiredAmount, unit);
				}
			}
			else
			{
				finalString = String.Format(line1Format, actualAmount, unit);
			}

			return finalString;
		}

		private Color chooseIconColour(Item item, LinkType linkType)
		{
			Color notEnough = Color.FromArgb(255, 255, 193, 193);
			Color enough = Color.White;
			Color tooMuch = Color.FromArgb(255, 174, 198, 206);

			if (linkType == LinkType.Input)
			{
				if (DisplayedNode.GetRequiredInput(item) <= DisplayedNode.GetTotalInput(item))
				{
					return enough;
				}
				else
				{
					return notEnough;
				}
			}
			else
			{
				if (DisplayedNode.GetTotalOutput(item) > DisplayedNode.GetRequiredOutput(item))
				{
					return tooMuch;
				}
				else
				{
					return enough;
				}
			}
		}

		public override void MouseUp(Point location, MouseButtons button)
		{
			ItemTab clickedTab = null;
			foreach (ItemTab tab in SubElements.OfType<ItemTab>())
			{
				if (tab.bounds.Contains(location))
				{
					clickedTab = tab;
				}
			}

			if (button == MouseButtons.Left)
			{
				if (clickedTab != null && clickedTab.Type == LinkType.Input && DisplayedNode is ConsumerNode)
				{
					beginEditingInputAmount(clickedTab.Item);
				}
			}
			else if (button == MouseButtons.Right)
			{
				ContextMenu rightClickMenu = new ContextMenu();
				if (clickedTab != null)
				{
					if (clickedTab.Type == LinkType.Input)
					{
						if (DisplayedNode.GetUnsatisfiedDemand(clickedTab.Item) > 0)
						{
							rightClickMenu.MenuItems.Add(new MenuItem("Automatically choose/create a node to produce this item",
								new EventHandler((o, e) =>
								{
									DisplayedNode.Graph.AutoSatisfyNodeDemand(DisplayedNode, clickedTab.Item);
									Parent.UpdateNodes();
									Parent.Invalidate();
								})));

							rightClickMenu.MenuItems.Add(new MenuItem("Manually create a node to produce this item",
								new EventHandler((o, e) =>
									{
										RecipeChooserForm form = new RecipeChooserForm(clickedTab.Item.Recipes, new List<Item>{clickedTab.Item});
										var result = form.ShowDialog();
										if (result == DialogResult.OK)
										{
											if (form.selectedRecipe != null)
											{
												DisplayedNode.Graph.CreateRecipeNodeToSatisfyItemDemand(DisplayedNode, clickedTab.Item, form.selectedRecipe);
											}
											else
											{
												DisplayedNode.Graph.CreateSupplyNodeToSatisfyItemDemand(DisplayedNode, clickedTab.Item);
											}
											Parent.UpdateNodes();
											Parent.Invalidate();
										}
									})));
							}
					}
				}

				rightClickMenu.MenuItems.Add(new MenuItem("Delete node",
					new EventHandler((o, e) =>
						{
							Parent.DeleteNode(this);
						})));

				rightClickMenu.Show(Parent, Parent.GraphToScreen(Point.Add(location, new Size(X, Y))));
			}
		}

		public void beginEditingInputAmount(Item item)
		{
			if (editorBox != null)
			{
				stopEditingInputAmount();
			}

			editorBox = new TextBox();
			editedItem = item;
			float amountToShow = (DisplayedNode as ConsumerNode).ConsumptionAmount;
			if (Parent.Graph.SelectedAmountType == AmountType.Rate && Parent.Graph.SelectedUnit == RateUnit.PerMinute)
			{
				amountToShow *= 60;
			}
			editorBox.Text = amountToShow.ToString();
			originalEditorValue = amountToShow;
			editorBox.SelectAll();
			editorBox.Size = new Size(100, 30);
			Rectangle tooltipRect = Parent.getTooltipScreenBounds(Parent.GraphToScreen(GetInputLineConnectionPoint(item)), new Point(editorBox.Size), Direction.Up);
			editorBox.Location = new Point(tooltipRect.X, tooltipRect.Y);
			Parent.Controls.Add(editorBox);
			editorBox.Focus();
			editorBox.TextChanged += editorBoxTextChanged;
			editorBox.KeyDown += new KeyEventHandler(editorBoxKeyDown);
			editorBox.LostFocus += new EventHandler((sender, e) => stopEditingInputAmount());
		}

		private void stopEditingInputAmount()
		{
			if (editorBox != null && !editorBox.IsDisposed)
			{
				Parent.Controls.Remove(editorBox);
				editorBox = null;
				editedItem = null;
				Parent.Focus();
			}
		}

		private void editorBoxTextChanged(object sender, EventArgs e)
		{
			float amount;
			bool amountIsValid = float.TryParse((sender as TextBox).Text, out amount);

			if (amountIsValid)
			{
				if (Parent.Graph.SelectedAmountType == AmountType.Rate && Parent.Graph.SelectedUnit == RateUnit.PerMinute)
				{
					amount /= 60;
				}
				(DisplayedNode as ConsumerNode).ConsumptionAmount = amount;
				Parent.UpdateNodes();
				Parent.Invalidate();
			}
		}

		private void editorBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				stopEditingInputAmount();
			}
			else if (e.KeyCode == Keys.Escape)
			{
				if (editorBox != null) //This line only happens if the window is closed with the esc button with the editor box open.
				{
					editorBox.Text = originalEditorValue.ToString();
				}
				stopEditingInputAmount();
			}			
		}

		public override void MouseMoved(Point location)
		{
			if (editorBox == null)
			{
				ItemTab mousedTab = null;
				foreach (ItemTab tab in SubElements.OfType<ItemTab>())
				{
					if (tab.bounds.Contains(location))
					{
						mousedTab = tab;
					}
				}

				TooltipInfo tti = new TooltipInfo();
				if (mousedTab != null)
				{
					tti.Text = mousedTab.Item.FriendlyName;
					if (mousedTab.Type == LinkType.Input)
					{
						if (DisplayedNode is ConsumerNode)
						{
							tti.Text += "\nClick to edit desired amount";
						}
						tti.Text += "\nDrag to create a new connection";
						tti.Direction = Direction.Up;
						tti.ScreenLocation = Parent.GraphToScreen(GetInputLineConnectionPoint(mousedTab.Item));
					}
					else
					{
						tti.Text = mousedTab.Item.FriendlyName;
						tti.Text += "\nDrag to create a new connection";
						tti.Direction = Direction.Down;
						tti.ScreenLocation = Parent.GraphToScreen(GetOutputLineConnectionPoint(mousedTab.Item));
					}
				}

				Parent.AddTooltip(tti);
			}
		}

		public override bool ContainsPoint(Point point)
		{
			if (new Rectangle(0, 0, Width, Height).Contains(point.X, point.Y))
			{
				return true;
			}
			foreach (ItemTab tab in SubElements.OfType<ItemTab>())
			{
				if (tab.bounds.Contains(point))
				{
					return true;
				}
			}
			return false;
		}

		public override void MouseDown(Point location, MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				Parent.DraggedElement = this;
				DragOffsetX = location.X;
				DragOffsetY = location.Y;
			}
		}

		public override void Dragged(Point location)
		{
			ItemTab draggedTab = null;

			foreach (ItemTab tab in SubElements.OfType<ItemTab>())
			{
				if (tab.bounds.Contains(new Point(DragOffsetX, DragOffsetY)))
				{
					draggedTab = tab;
				}
			}

			if (draggedTab != null)
			{
				DraggedLinkElement newLink = new DraggedLinkElement(Parent, this, draggedTab.Type, draggedTab.Item);
				if (draggedTab.Type == LinkType.Input)
				{
					newLink.ConsumerElement = this;
				}
				else
				{
					newLink.SupplierElement = this;
				}
				Parent.DraggedElement = newLink;
			}
			else
			{
				X += location.X - DragOffsetX;
				Y += location.Y - DragOffsetY;

				foreach (ProductionNode node in DisplayedNode.InputLinks.Select<NodeLink, ProductionNode>(l => l.Supplier))
				{
					Parent.GetElementForNode(node).UpdateTabOrder();
				}
				foreach (ProductionNode node in DisplayedNode.OutputLinks.Select<NodeLink, ProductionNode>(l => l.Consumer))
				{
					Parent.GetElementForNode(node).UpdateTabOrder();
				}
			}
		}
	}
}