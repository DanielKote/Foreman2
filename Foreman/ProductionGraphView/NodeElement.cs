using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Foreman
{
	public enum LinkType { Input, Output };

	public partial class NodeElement : GraphElement
	{
        public bool StickyDragOrigin = false; //true: drag axies are established at start of drag; false: drag axies are established upon activation of axis limits (shift)
		public int MouseDownOffsetX;
		public int MouseDownOffsetY;
		public Point DragOrigin;
		public bool Selected = false;

		private Color recipeColour = Color.FromArgb(190, 217, 212);
		private Color passthroughColour = Color.FromArgb(190, 217, 212);
		private Color supplyColour = Color.FromArgb(231, 214, 224);
		private Color outputColour = Color.FromArgb(249, 237, 195);
		private Color missingColour = Color.FromArgb(0xff, 0x7f, 0x6b);
		private Color backgroundOversuppliedColor = Color.FromArgb(0xff, 0x7f, 0x6b);
        private Color darkTextColour = Color.FromArgb(69, 69, 69);
		private Color productivityTickColor = Color.FromArgb(166, 0, 0);
		private Brush SelectionOverlayBrush = new SolidBrush(Color.FromArgb(100,100, 100, 200));
		private Brush backgroundBrush;
		private Brush backgroundOversuppliedBrush;
		private Brush textBrush;
		private Pen productivityPen;
		private Font size10Font = new Font(FontFamily.GenericSansSerif, 10);
		private Font size7Font = new Font(FontFamily.GenericSansSerif, 7);
		private Font myFont;
		private StringFormat centreFormat = new StringFormat();

		//most values are attempted to fit the grid (6 * 2^n) - ex: 72 = 6 * (4+8)
		private const int tabPadding = 7; //makes each tab be evenly spaced for grid
		private const int baseHeight = 96;
		private const int baseWIconHeight = 128;
		private const int minWidth = 72;
		private const int dWidth = 24;
		private const int dHeight = 12;
		private const int maxT10Width = 144;
		//private const int lineWidth = 1;

		public String text = "";
		
		private AssemblerBox assemblerBox;
		private List<ItemTab> inputTabs = new List<ItemTab>();
		private List<ItemTab> outputTabs = new List<ItemTab>();

		private ContextMenu rightClickMenu = new ContextMenu();
		
		public ProductionNode DisplayedNode { get; private set; }

		public bool tooltipsEnabled = true;

		public NodeElement(ProductionNode node, ProductionGraphViewer parent) : base(parent)
		{
			Width = minWidth;
			Height = baseHeight;
			myFont = size10Font;

			DisplayedNode = node;

            Color backgroundColour = missingColour;
            Color textColour = darkTextColour;
            if (DisplayedNode is ConsumerNode)
            {
                backgroundColour = outputColour;
                if (((ConsumerNode)DisplayedNode).ConsumedItem.IsMissingItem)
                    backgroundColour = missingColour;
            }
            else if (DisplayedNode is SupplyNode)
            {
                backgroundColour = supplyColour;
                if (((SupplyNode)DisplayedNode).SuppliedItem.IsMissingItem)
                    backgroundColour = missingColour;
            }
            else if (DisplayedNode is RecipeNode)
            {
                backgroundColour = recipeColour;
                if (((RecipeNode)DisplayedNode).BaseRecipe.IsMissingRecipe)
                    backgroundColour = missingColour;
            }
            else if (DisplayedNode is PassthroughNode)
            {
                backgroundColour = passthroughColour;
                if (((PassthroughNode)DisplayedNode).PassedItem.IsMissingItem)
                    backgroundColour = missingColour;
            }
			else if(DisplayedNode is ErrorNode)
            {
				backgroundColour = missingColour;
            }
            else
                Trace.Fail("No branch for node: " + DisplayedNode.ToString());

			backgroundBrush = new SolidBrush(backgroundColour);
			backgroundOversuppliedBrush = new SolidBrush(backgroundOversuppliedColor);
			textBrush = new SolidBrush(textColour);
			productivityPen = new Pen(productivityTickColor, 5);

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

			if (DisplayedNode is RecipeNode || DisplayedNode is SupplyNode)
			{
				assemblerBox = new AssemblerBox(Parent);
				SubElements.Add(assemblerBox); 
				assemblerBox.Height = assemblerBox.Width = 128;
			}

			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;
		}

		private int getIconWidths()
		{
			return Math.Max(GetInputIconWidths(), GetOutputIconWidths());
		}

		private int GetInputIconWidths()
		{
			int result = tabPadding;
			foreach (ItemTab tab in inputTabs)
				result += tab.Width + tabPadding;
			return result;
		}
		private int GetOutputIconWidths()
		{
			int result = tabPadding;
			foreach (ItemTab tab in outputTabs)
				result += tab.Width + tabPadding;
			return result;
		}
		
		public void Update()
		{
			UpdateTabOrder();

			if (DisplayedNode is SupplyNode)
			{
				SupplyNode node = (SupplyNode)DisplayedNode;
				if (!Parent.ShowMiners)
				{
					if (node.SuppliedItem.IsMissingItem)
						text = String.Format("Item not loaded! ({0})", node.DisplayName);
					else
						text = "Input: " + node.SuppliedItem.FriendlyName;
				}
				else
					text = "";
			}
			else if (DisplayedNode is ConsumerNode)
			{
				ConsumerNode node = (ConsumerNode)DisplayedNode;
				if (node.ConsumedItem.IsMissingItem)
					text = String.Format("Item not loaded! ({0})", node.DisplayName);
				else
					text = "Output: " + node.ConsumedItem.FriendlyName;
			}
			else if (DisplayedNode is RecipeNode)
			{
				RecipeNode node = (RecipeNode)DisplayedNode;
				if (!Parent.ShowAssemblers)
				{
					if (node.BaseRecipe.IsMissingRecipe)
						text = String.Format("Recipe not loaded! ({0})", node.DisplayName);
					else
						//text = "Recipe: " + node.BaseRecipe.FriendlyName;
						text = node.BaseRecipe.FriendlyName;
				}
				else
					text = "";
			}

			int iconWidth = Math.Max(minWidth, getIconWidths() + 10);

			Graphics graphics = Parent.CreateGraphics();
			int minTextWidth = (int)graphics.MeasureString(text, size10Font).Width + 10;
			myFont = size10Font;
			if(minTextWidth > maxT10Width && minTextWidth > iconWidth)
            {
				myFont = size7Font;
				minTextWidth = (int)graphics.MeasureString(text, size7Font).Width + 10;
			}
			Width = Math.Max(iconWidth, minTextWidth);
			Height = baseHeight;

			if (assemblerBox != null) //recipe box (update width&height if displaying assembler/miner, or just increase min width if not displaying assembler/miner) + update assembler list
			{
				if ((DisplayedNode is RecipeNode && Parent.ShowAssemblers)
					|| (DisplayedNode is SupplyNode && Parent.ShowMiners))
				{
					if (DisplayedNode is RecipeNode)
					{
                        var assemblers = (DisplayedNode as RecipeNode).GetAssemblers();
                        if (Parent.Graph.SelectedAmountType == AmountType.FixedAmount)
                            assemblers = assemblers.ToDictionary(p => p.Key, p => 0);
						assemblerBox.AssemblerList = assemblers;
					}
					else if (DisplayedNode is SupplyNode)
						assemblerBox.AssemblerList = (DisplayedNode as SupplyNode).GetMinimumMiners();

					assemblerBox.Update();
					int assemblerBoxWidth = assemblerBox.Width + 20;
					Width = Math.Max(Width, assemblerBoxWidth) + dWidth * 2;
					Width -= Width % dWidth;

					Height = baseWIconHeight;

					assemblerBox.X = -assemblerBox.Width / 2 + 2;
					assemblerBox.Y = -assemblerBox.Height / 2 + 2;
				}
				else
				{
					assemblerBox.AssemblerList.Clear();
					assemblerBox.Update();
				}
			}

			//final size confirms
			if (Width % dWidth != 0)
			{
				Width += dWidth;
				Width -= Width % dWidth;
			}
			Width -= 2;
			if (Height % dHeight != 0)
			{
				Height += dHeight;
				Height -= Height % dHeight;
			}
			Height -= 2;

			//update tabs
			foreach (ItemTab tab in inputTabs.Union(outputTabs))
			{
				tab.BorderColor = chooseIconBorderColor(tab.Item, tab.Type);
				tab.Text = getIconString(tab.Item, tab.Type);
			}
			UpdateTabOrder();
		}

		public void UpdateTabOrder()
		{
			inputTabs = inputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();
			outputTabs = outputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();

			int x = - GetOutputIconWidths() / 2;
			foreach (ItemTab tab in outputTabs)
			{
				x += tabPadding;
				tab.X = x;
				tab.Y = (-tab.Height / 2) - (Height / 2);
				x += tab.Width;
			}
			x = - GetInputIconWidths() / 2;
			foreach (ItemTab tab in inputTabs)
			{
				x += tabPadding;
				tab.X = x;
				tab.Y = (-tab.Height / 2) + (Height / 2);
				x += tab.Width;
			}
		}

		public int GetItemTabXHeuristic(ItemTab tab)
		{
			int total = 0;
			if (tab.Type == LinkType.Input)
			{
				foreach (NodeLink link in DisplayedNode.InputLinks.Where(l => l.Item == tab.Item))
				{
					NodeElement node = Parent.GetElementForNode(link.Supplier);
					Point diff = Point.Subtract(new Point(node.Location.X + node.Width / 2, node.Location.Y + node.Height / 2), new Size(Location.X + Width / 2, Location.Y + Height / 2));
					diff.Y = Math.Max(0, diff.Y);
					total += Convert.ToInt32(Math.Atan2(diff.X, diff.Y) * 1000);
				}
			}
			else
			{
				foreach (NodeLink link in DisplayedNode.OutputLinks.Where(l => l.Item == tab.Item))
				{
					NodeElement node = Parent.GetElementForNode(link.Consumer);
					if (node == null)
					{
						continue;
					}
					Point diff = Point.Subtract(new Point(node.Location.X + node.Width / 2, -node.Location.Y + -node.Height / 2), new Size(Location.X + Width / 2, -Location.Y + -Height / 2));
					diff.Y = Math.Max(0, diff.Y);
					total += Convert.ToInt32(Math.Atan2(diff.X, diff.Y) * 1000);
				}
			}
			return total;
		}

		public Point GetOutputLineConnectionPoint(Item item)
		{
			if (!outputTabs.Any())
			{
				return new Point(X + Width / 2, Y);
			}
			ItemTab tab = outputTabs.First(it => it.Item == item);
			return new Point(X + tab.X + tab.Width / 2, Y + tab.Y);
		}

		public ItemTab GetOutputLineItemTab(Item item)
        {
			if (!outputTabs.Any())
			{
				return null;
			}
			return outputTabs.First(it => it.Item == item);
		}

		public Point GetInputLineConnectionPoint(Item item)
		{
			if (!inputTabs.Any())
			{
				return new Point(X + Width / 2, Y + Height);
			}
			ItemTab tab = inputTabs.First(it => it.Item == item);
			return new Point(X + tab.X + tab.Width / 2, Y + tab.Y + tab.Height);
		}

		public ItemTab GetInputLineItemTab(Item item)
		{
			if (!inputTabs.Any())
			{
				return null;
			}
			return inputTabs.First(it => it.Item == item);
		}

		public override void Paint(Graphics graphics, Point trans)
		{
			if (Visible)
			{
				Brush bgBrush = backgroundBrush;
				foreach (ItemTab tab in inputTabs.Union(outputTabs))
					if (tab.Type == LinkType.Input && DisplayedNode.OverSupplied(tab.Item))
						bgBrush = backgroundOversuppliedBrush;

				if (DisplayedNode.ManualRateNotMet() || (DisplayedNode is RecipeNode && !(((RecipeNode)DisplayedNode).BaseRecipe.Enabled && ((RecipeNode)DisplayedNode).BaseRecipe.HasEnabledAssemblers)))
					GraphicsStuff.FillRoundRect(trans.X - (Width / 2) - 5, trans.Y - (Height / 2) - 5, Width + 10, Height + 10, 13, graphics, Brushes.DarkRed);

				GraphicsStuff.FillRoundRect(trans.X - (Width/2), trans.Y - (Height / 2), Width, Height, 8, graphics, bgBrush);
				graphics.DrawString(text, myFont, textBrush, trans.X, trans.Y, centreFormat);

				if (DisplayedNode is RecipeNode)
				{
					int prodCount = 0;
					var assemblers = (DisplayedNode as RecipeNode).GetAssemblers();
					foreach (MachinePermutation assembler in assemblers.Keys)
						foreach (Module module in assembler.modules)
							if (module.ProductivityBonus > 0)
								graphics.DrawEllipse(productivityPen, trans.X - (Width / 2) - 2, trans.Y - (Height / 2) + 20 + prodCount++ * 10, 5, 5);
				}

				if (Selected)
					GraphicsStuff.FillRoundRect(trans.X - (Width / 2), trans.Y - (Height / 2), Width, Height, 8, graphics, SelectionOverlayBrush);

				base.Paint(graphics, trans);
			}
		}

		private String getIconString(Item item, LinkType linkType)
		{
			String line1FormatA = "{0:0.##}{1}";
			String line1FormatB = "{0:0.#}{1}";
			String line1FormatC = "{0:0}{1}";
			String line2FormatA = "\n({0:0.##}{1})";
			String line2FormatB = "\n({0:0.#}{1})";
			String line2FormatC = "\n({0:0}{1})";
			String finalString = "";

			String unit = "";

			var actualAmount = 0.0; 
            var suppliedAmount = 0.0;

			if (linkType == LinkType.Input)
			{
				actualAmount = DisplayedNode.GetConsumeRate(item);
				suppliedAmount = DisplayedNode.GetSuppliedRate(item);
			}
			else
			{
				actualAmount = DisplayedNode.GetSupplyRate(item);
			}
			if (Parent.Graph.SelectedAmountType == AmountType.Rate && Parent.Graph.SelectedUnit == RateUnit.PerSecond)
			{
				unit = "/s";
			}
			else if (Parent.Graph.SelectedAmountType == AmountType.Rate && Parent.Graph.SelectedUnit == RateUnit.PerMinute)
			{
				unit = "/m";
				actualAmount *= 60;
				suppliedAmount *= 60;
			}

			if(actualAmount >= 1000)
				finalString = String.Format(line1FormatC, actualAmount, unit);
			else if(actualAmount >= 100)
				finalString = String.Format(line1FormatB, actualAmount, unit);
			else
				finalString = String.Format(line1FormatA, actualAmount, unit);

			if (linkType == LinkType.Input && DisplayedNode.OverSupplied(item))
			{
				if(suppliedAmount >= 1000)
					finalString += String.Format(line2FormatC, suppliedAmount, unit);
				else if (suppliedAmount >= 100)
					finalString += String.Format(line2FormatB, suppliedAmount, unit);
				else
					finalString += String.Format(line2FormatA, suppliedAmount, unit);
			}

			return finalString;
		}

		private Color chooseIconBorderColor(Item item, LinkType linkType)
		{
			Color enough = Color.Gray;
			Color tooMuch = Color.DarkRed;

			if (linkType == LinkType.Input && DisplayedNode.OverSupplied(item))
				return tooMuch;

            return enough;
		}

		public override void MouseUp(Point location, MouseButtons button, bool wasDragged)
		{
			if (Parent.MouseDownElement == this && !wasDragged)
			{
				if (button == MouseButtons.Left)
				{
					if ((Control.ModifierKeys & Keys.Control) == 0 && (Control.ModifierKeys & Keys.Alt) == 0)
						beginEditingNodeRate();
				}
			}

			if (button == MouseButtons.Right)
			{
				//check if we are in an item tab (for deleting connections)
				bool inItemTab = false;
				foreach (ItemTab tab in SubElements.OfType<ItemTab>())
				{
					if (tab.bounds.Contains(new Point(MouseDownOffsetX, MouseDownOffsetY)))
					{
						inItemTab = true;
						rightClickMenu.MenuItems.Clear();
						rightClickMenu.MenuItems.Add(new MenuItem("Delete connections",
							new EventHandler((o, e) =>
							{
								Parent.RemoveAssociatedLinks(tab);
							})));
						rightClickMenu.Show(Parent, Parent.GraphToScreen(Point.Add(location, new Size(X, Y))));
					}
				}
				if (!inItemTab)
				{
					Parent.OpenNodeMenu(this);
				}
			}
		}

		public void beginEditingNodeRate()
		{
			RateOptionsPanel newPanel = new RateOptionsPanel(DisplayedNode, Parent);
			new FloatingTooltipControl(newPanel, Direction.Right, new Point(Location.X - (Width / 2), Location.Y), Parent);
		}

		public override List<TooltipInfo> GetToolTips(Point location)
        {
			List<TooltipInfo> toolTips = new List<TooltipInfo>();
			if (tooltipsEnabled)
			{
				ItemTab mousedTab = null;
				foreach (ItemTab tab in SubElements.OfType<ItemTab>())
				{
					if (tab.bounds.Contains(location))
						mousedTab = tab;
				}

				if (mousedTab != null)
				{
					TooltipInfo tti = new TooltipInfo();
					tti.Text = mousedTab.Item.FriendlyName;
					if (mousedTab.Type == LinkType.Input)
					{
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
					toolTips.Add(tti);
				}
				else if (DisplayedNode is RecipeNode)
				{
					TooltipInfo tti = new TooltipInfo();
					tti.Direction = Direction.Left;
					tti.ScreenLocation = Parent.GraphToScreen(Point.Add(Location, new Size(Width/2, 0)));
					tti.Text = String.Format("WH: {0},{1}\n", Width, Height);
					tti.Text += String.Format("XY: {0},{1}\n", X, Y);

					tti.Text += String.Format("Recipe: {0}", (DisplayedNode as RecipeNode).BaseRecipe.FriendlyName);
					tti.Text += String.Format("\n--Base Time: {0}s", (DisplayedNode as RecipeNode).BaseRecipe.Time);
					tti.Text += String.Format("\n--Base Ingredients:");
					foreach (var kvp in (DisplayedNode as RecipeNode).BaseRecipe.Ingredients)
					{
						tti.Text += String.Format("\n----{0} ({1})", kvp.Key.FriendlyName, kvp.Value.ToString());
					}
					tti.Text += String.Format("\n--Base Results:");
					foreach (var kvp in (DisplayedNode as RecipeNode).BaseRecipe.Results)
					{
						tti.Text += String.Format("\n----{0} ({1})", kvp.Key.FriendlyName, kvp.Value.ToString());
					}
					if (Parent.ShowAssemblers)
					{
						tti.Text += String.Format("\n\nAssemblers:");
						foreach (var kvp in assemblerBox.AssemblerList)
						{
							tti.Text += String.Format("\n----{0} ({1})", kvp.Key.assembler.FriendlyName, kvp.Value.ToString());
							foreach (var Module in kvp.Key.modules.Where(m => m != null))
							{
								tti.Text += String.Format("\n------{0}", Module.FriendlyName);
							}
						}
					}

					if (Parent.Graph.SelectedAmountType == AmountType.FixedAmount)
					{
						tti.Text += String.Format("\n\nCurrent iterations: {0}", DisplayedNode.actualRate);
					}
					else
					{
						tti.Text += String.Format("\n\nCurrent Rate: {0}/{1}",
							Parent.Graph.SelectedUnit == RateUnit.PerMinute ? DisplayedNode.actualRate / 60 : DisplayedNode.actualRate,
							Parent.Graph.SelectedUnit == RateUnit.PerMinute ? "m" : "s");
					}
					toolTips.Add(tti);
				}

				TooltipInfo helpToolTipInfo = new TooltipInfo();
				helpToolTipInfo.Text = "Left click on this node to edit how fast it runs\nRight click to delete it";
				helpToolTipInfo.Direction = Direction.None;
				helpToolTipInfo.ScreenLocation = new Point(10, 10);
				toolTips.Add(helpToolTipInfo);
			}
			return toolTips;
		}

		public override bool ContainsPoint(Point point)
		{
			if (new Rectangle(-Width / 2, -Height / 2, Width, Height).Contains(point.X, point.Y))
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
			MouseDownOffsetX = location.X;
			MouseDownOffsetY = location.Y;

			if (button == MouseButtons.Left)
			{
				Parent.MouseDownElement = this;
				DragOrigin = new Point(X, Y);
			}
		}

		public override void Dragged(Point location)
		{
			ItemTab draggedTab = null;

			foreach (ItemTab tab in SubElements.OfType<ItemTab>())
			{
				if (tab.bounds.Contains(new Point(MouseDownOffsetX, MouseDownOffsetY)))
					draggedTab = tab;
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
				Parent.MouseDownElement = newLink;
			}
			else
			{
				int xtemp = X;
				int ytemp = Y;

				//lock to grid if it is set & is visible
				if(Parent.ShowGrid)
                {
					xtemp += location.X;
					xtemp = Parent.AlignToGrid(xtemp);

					ytemp += location.Y;
					ytemp = Parent.AlignToGrid(ytemp);
                }
				else
                {
					xtemp += location.X - MouseDownOffsetX;
					ytemp += location.Y - MouseDownOffsetY;
                }

                if (Parent.LockDragToAxis)
                {
                    if (Math.Abs(DragOrigin.X - xtemp) > Math.Abs(DragOrigin.Y - ytemp))
                        ytemp = Parent.AlignToGrid(DragOrigin.Y + Height / 2) - Height / 2;
                    else
                        xtemp = Parent.AlignToGrid(DragOrigin.X + Width / 2) - Width / 2;
                }
                else if (!StickyDragOrigin)
                    DragOrigin = new Point(X, Y);

				X = xtemp;
				Y = ytemp;

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

		public override void Dispose()
		{
			size10Font.Dispose();
			centreFormat.Dispose();
			backgroundBrush.Dispose();
			if (assemblerBox != null)
			{
				assemblerBox.Dispose();
			}
			rightClickMenu.Dispose();
			base.Dispose();
		}
	}
}