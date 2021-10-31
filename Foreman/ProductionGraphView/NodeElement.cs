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
	public partial class NodeElement : GraphElement
	{
        public static bool StickyDragOrigin = false; //true: drag axies are established at start of drag; false: drag axies are established upon activation of axis limits (shift)

		public bool Selected = false;
		public string Text { get; set; }
		public BaseNode DisplayedNode { get; private set; }
		public bool TooltipsEnabled = true;

		public Point DragOrigin { get; private set; }

		public override int X { get { return DisplayedNode.Location.X; } set { DisplayedNode.Location = new Point(value, DisplayedNode.Location.Y); } }
		public override int Y { get { return DisplayedNode.Location.Y; } set { DisplayedNode.Location = new Point(DisplayedNode.Location.X, value); } }
		public override Point Location { get { return DisplayedNode.Location; } set { DisplayedNode.Location = value; } }

		private static Brush recipeBGBrush = new SolidBrush(Color.FromArgb(190, 217, 212));
		private static Brush passthroughBGBrush = new SolidBrush(Color.FromArgb(190, 217, 212));
		private static Brush supplyBGBrush = new SolidBrush(Color.FromArgb(231, 214, 224));
		private static Brush demandBGBrush = new SolidBrush(Color.FromArgb(249, 237, 195));
		private static Brush missingBGBrush = new SolidBrush(Color.FromArgb(0xff, 0x7f, 0x6b));
		private static Brush oversuppliedBGBrush = new SolidBrush(Color.FromArgb(0xff, 0x7f, 0x6b));
        private static Brush textBrush = new SolidBrush(Color.FromArgb(69, 69, 69));
		private static Pen productivityTickBGPen = new Pen(Color.FromArgb(166, 0, 0), 5);
		private static Brush SelectionOverlayBrush = new SolidBrush(Color.FromArgb(100,100, 100, 200));

		private static Font size10Font = new Font(FontFamily.GenericSansSerif, 10);
		private static Font size7Font = new Font(FontFamily.GenericSansSerif, 7);

		private static StringFormat centreFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

		//most values are attempted to fit the grid (6 * 2^n) - ex: 72 = 6 * (4+8)
		private const int tabPadding = 7; //makes each tab be evenly spaced for grid
		private const int baseHeight = 96;
		private const int baseWIconHeight = 128;
		private const int minWidth = 72;
		private const int dWidth = 24;
		private const int dHeight = 12;
		private const int maxT10Width = 144;

		private Brush backgroundBrush;
		private Font myFont;

		private List<ItemTab> inputTabs = new List<ItemTab>();
		private List<ItemTab> outputTabs = new List<ItemTab>();

		private ContextMenu rightClickMenu = new ContextMenu();

		private Point MouseDownLocation; //local coordinate

		public NodeElement(ProductionGraphViewer parent, BaseNode node) : base(parent)
		{
			Text = "";
			myFont = size10Font;
			DisplayedNode = node;

			Width = minWidth;
			Height = baseHeight;

            if (DisplayedNode is ConsumerNode)
            {
				backgroundBrush = demandBGBrush;
                if (((ConsumerNode)DisplayedNode).ConsumedItem.IsMissingItem)
					backgroundBrush = missingBGBrush;
            }
            else if (DisplayedNode is SupplierNode)
            {
				backgroundBrush = supplyBGBrush;
                if (((SupplierNode)DisplayedNode).SuppliedItem.IsMissingItem)
					backgroundBrush = missingBGBrush;
            }
            else if (DisplayedNode is RecipeNode)
            {
				backgroundBrush = recipeBGBrush;
                if (((RecipeNode)DisplayedNode).BaseRecipe.IsMissingRecipe)
					backgroundBrush = missingBGBrush;
            }
            else if (DisplayedNode is PassthroughNode)
            {
				backgroundBrush = passthroughBGBrush;
                if (((PassthroughNode)DisplayedNode).PassthroughItem.IsMissingItem)
					backgroundBrush = missingBGBrush;
            }
            else
                Trace.Fail("No branch for node: " + DisplayedNode.ToString());

			foreach (Item item in node.Inputs)
			{
				ItemTab newTab = new ItemTab(item, LinkType.Input, myGraphViewer);
				SubElements.Add(newTab);
				inputTabs.Add(newTab);
			}
			foreach (Item item in node.Outputs)
			{
				ItemTab newTab = new ItemTab(item, LinkType.Output, myGraphViewer);
				SubElements.Add(newTab);
				outputTabs.Add(newTab);
			}

			if (DisplayedNode is RecipeNode || DisplayedNode is SupplierNode)
			{
				//assembler stuff
			}
		}

		private int getIconWidths()
		{
			return Math.Max(GetIconWidths(inputTabs), GetIconWidths(outputTabs));
		}

		private int GetIconWidths(List<ItemTab> tabs)
		{
			int result = tabPadding;
			foreach (ItemTab tab in tabs)
				result += tab.Bounds.Width + tabPadding;
			return result;
		}
		
		public void Update()
		{
			if (DisplayedNode is SupplierNode)
			{
				SupplierNode node = (SupplierNode)DisplayedNode;
				if (node.SuppliedItem.IsMissingItem)
					Text = String.Format("Item not loaded! ({0})", node.DisplayName);
				else
					Text = "Input: " + node.SuppliedItem.FriendlyName;
			}
			else if (DisplayedNode is ConsumerNode)
			{
				ConsumerNode node = (ConsumerNode)DisplayedNode;
				if (node.ConsumedItem.IsMissingItem)
					Text = String.Format("Item not loaded! ({0})", node.DisplayName);
				else
					Text = "Output: " + node.ConsumedItem.FriendlyName;
			}
			else if (DisplayedNode is RecipeNode)
			{
				RecipeNode node = (RecipeNode)DisplayedNode;
				if (node.BaseRecipe.IsMissingRecipe)
					Text = String.Format("Recipe not loaded! ({0})", node.DisplayName);
				else
					Text = node.BaseRecipe.FriendlyName;
			}

			int iconWidth = Math.Max(minWidth, getIconWidths() + 10);
			int width, height;

			Graphics graphics = myGraphViewer.CreateGraphics();
			int minTextWidth = (int)graphics.MeasureString(Text, size10Font).Width + 10;
			myFont = size10Font;
			if(minTextWidth > maxT10Width && minTextWidth > iconWidth)
            {
				myFont = size7Font;
				minTextWidth = (int)graphics.MeasureString(Text, size7Font).Width + 10;
			}
			width = Math.Max(iconWidth, minTextWidth);
			height = baseHeight;

			//final size confirms
			if (width % dWidth != 0)
			{
				width += dWidth;
				width -= width % dWidth;
			}
			if (height % dHeight != 0)
			{
				height += dHeight;
				height -= height % dHeight;
			}
			width -= 2; //shrink by 1 on each side to make nodes being placed next to each other have a 2px gap between them (at least)
			height -= 2;

			Width = width;
			Height = height;

			//update tabs
			foreach (ItemTab tab in inputTabs)
				tab.UpdateValues(DisplayedNode.GetConsumeRate(tab.Item), DisplayedNode.GetSuppliedRate(tab.Item), DisplayedNode.IsOversupplied(tab.Item)); //for inputs we want the consumption/supply/oversupply values
			foreach(ItemTab tab in outputTabs)
				tab.UpdateValues(DisplayedNode.GetSupplyRate(tab.Item), 0, false); //for outputs we only care to display the supply rate

			UpdateTabOrder();
		}

		public void UpdateTabOrder()
		{
			inputTabs = inputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();
			outputTabs = outputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();

			int x = - GetIconWidths(outputTabs) / 2;
			foreach (ItemTab tab in outputTabs)
			{
				x += tabPadding;
				tab.Location = new Point(x + (tab.Width / 2), -Height / 2);
				x += tab.Width;
			}
			x = - GetIconWidths(inputTabs) / 2;
			foreach (ItemTab tab in inputTabs)
			{
				x += tabPadding;
				tab.Location = new Point(x + (tab.Width / 2), Height / 2);
				x += tab.Width;
			}
		}

		public int GetItemTabXHeuristic(ItemTab tab)
		{
			int total = 0;
			IEnumerable<NodeLink> links;
			if (tab.LinkType == LinkType.Input)
				links = DisplayedNode.InputLinks.Where(l => l.Item == tab.Item);
			else //if(tab.Type == LinkType.Output)
				links = DisplayedNode.OutputLinks.Where(l => l.Item == tab.Item);

			foreach (NodeLink link in links)
			{
				Point diff = Point.Subtract(link.Supplier.Location, (Size)DisplayedNode.Location);
				diff.Y = Math.Max(0, diff.Y);
				total += Convert.ToInt32(Math.Atan2(diff.X, diff.Y) * 1000);
			}
			return total;
		}

		public ItemTab GetOutputLineItemTab(Item item)
		{
			if (!outputTabs.Any())
				return null;
			return outputTabs.First(it => it.Item == item);
		}

		public Point GetOutputLineConnectionPoint(Item item)
		{
			ItemTab tab = GetOutputLineItemTab(item);
			if (tab == null)
				return new Point(X, Y - (Height / 2));

			return new Point(X + tab.X, Y + tab.Y - (tab.Height / 2));
		}

		public ItemTab GetInputLineItemTab(Item item)
		{
			if (!inputTabs.Any())
				return null;
			return inputTabs.First(it => it.Item == item);
		}

		public Point GetInputLineConnectionPoint(Item item)
		{
			ItemTab tab = GetInputLineItemTab(item);
			if (tab == null)
				return new Point(X, Y + (Height / 2));

			return new Point(X + tab.X, Y + tab.Y + (tab.Height / 2));
		}

		public void beginEditingNodeRate()
		{
			//RateOptionsPanel newPanel = new RateOptionsPanel(DisplayedNode, Parent);
			//new FloatingTooltipControl(newPanel, Direction.Right, new Point(Location.X - (Width / 2), Location.Y), Parent);
		}

		public override bool ContainsPoint(Point parent_point)
		{
			if (base.ContainsPoint(parent_point))
				return true;

			Point local_point = Point.Subtract(parent_point, (Size)Location);
			foreach (ItemTab tab in SubElements.OfType<ItemTab>())
				if (tab.ContainsPoint(local_point))
					return true;

			return false;
		}

		protected override void Draw(Graphics graphics, Point trans) //trans is transform to get from graph viewer 0,0 to this node's center (0,0)
		{
			if (Visible)
			{
				//once again: NOTE: all drawing is done based on the 0,0 origin (for the node) being in the CENTER of the Bounds rectangle (this being its 'location')
				Brush bgBrush = backgroundBrush;
				foreach (ItemTab tab in inputTabs.Union(outputTabs))
					if (tab.LinkType == LinkType.Input && DisplayedNode.IsOversupplied(tab.Item))
						bgBrush = oversuppliedBGBrush;

				RecipeNode rNode = DisplayedNode as RecipeNode;
				if (DisplayedNode.ManualRateNotMet() || (rNode != null && !(!rNode.BaseRecipe.Hidden && rNode.BaseRecipe.HasEnabledAssemblers)))
					GraphicsStuff.FillRoundRect(trans.X - (Width / 2) - 5, trans.Y - (Height / 2) - 5, Width + 10, Height + 10, 13, graphics, Brushes.DarkRed);

				GraphicsStuff.FillRoundRect(trans.X - (Width / 2), trans.Y - (Height / 2), Width, Height, 8, graphics, bgBrush);
				graphics.DrawString(Text, myFont, textBrush, trans.X, trans.Y, centreFormat);

				if (rNode != null)
				{
					//int prodCount = 0;
					//var assemblers = rNode.GetAssemblers();
					//foreach (MachinePermutation assembler in assemblers.Keys)
					//	foreach (Module module in assembler.modules)
					//		if (module.ProductivityBonus > 0)
					//			graphics.DrawEllipse(productivityPen, trans.X - (Width / 2) - 2, trans.Y - (Height / 2) + 20 + prodCount++ * 10, 5, 5);
				}

				if (Selected)
					GraphicsStuff.FillRoundRect(trans.X - (Width / 2), trans.Y - (Height / 2), Width, Height, 8, graphics, SelectionOverlayBrush);
			}
		}

		public override List<TooltipInfo> GetToolTips(Point location)
		{
			List<TooltipInfo> toolTips = new List<TooltipInfo>();
			if (TooltipsEnabled)
			{
				ItemTab mousedTab = null;
				foreach (ItemTab tab in SubElements.OfType<ItemTab>())
				{
					if (tab.ContainsPoint(location))
						mousedTab = tab;
				}

				if (mousedTab != null)
				{
					TooltipInfo tti = new TooltipInfo();

					if (mousedTab.LinkType == LinkType.Input)
					{
						if (DisplayedNode is RecipeNode rNode)
							tti.Text = rNode.BaseRecipe.GetIngredientFriendlyName(mousedTab.Item);
						else if (!mousedTab.Item.IsTemperatureDependent)
							tti.Text = mousedTab.Item.FriendlyName;
						else
						{
							fRange tempRange = LinkElement.GetTemperatureRange(mousedTab.Item, this, LinkType.Output); //input type tab means output of connection link
							if (tempRange.Ignore && DisplayedNode is PassthroughNode)
								tempRange = LinkElement.GetTemperatureRange(mousedTab.Item, this, LinkType.Input); //if there was no temp range on this side of this throughput node, try to just copy the other side
							tti.Text = mousedTab.Item.GetTemperatureRangeFriendlyName(tempRange);
						}

						tti.Text += "\nDrag to create a new connection";
						tti.Direction = Direction.Up;
						tti.ScreenLocation = myGraphViewer.GraphToScreen(GetInputLineConnectionPoint(mousedTab.Item));
					}
					else //if(mousedTab.Type == LinkType.Output)
					{
						if (DisplayedNode is RecipeNode rNode)
							tti.Text = rNode.BaseRecipe.GetProductFriendlyName(mousedTab.Item);
						else if (!mousedTab.Item.IsTemperatureDependent)
							tti.Text = mousedTab.Item.FriendlyName;
						else
						{
							fRange tempRange = LinkElement.GetTemperatureRange(mousedTab.Item, this, LinkType.Input); //output type tab means input of connection link
							if (tempRange.Ignore && DisplayedNode is PassthroughNode)
								tempRange = LinkElement.GetTemperatureRange(mousedTab.Item, this, LinkType.Output); //if there was no temp range on this side of this throughput node, try to just copy the other side
							tti.Text = mousedTab.Item.GetTemperatureRangeFriendlyName(tempRange);
						}

						tti.Text += "\nDrag to create a new connection";
						tti.Direction = Direction.Down;
						tti.ScreenLocation = myGraphViewer.GraphToScreen(GetOutputLineConnectionPoint(mousedTab.Item));
					}
					toolTips.Add(tti);
				}
				else if (DisplayedNode is RecipeNode rNode)
				{
					TooltipInfo tti = new TooltipInfo();
					tti.Direction = Direction.Left;
					tti.ScreenLocation = myGraphViewer.GraphToScreen(Point.Add(Location, new Size(Width / 2, 0)));
					tti.Text = String.Format("WH: {0},{1}\n", Width, Height);
					tti.Text += String.Format("XY: {0},{1}\n", X, Y);

					tti.Text += String.Format("Recipe: {0}", rNode.BaseRecipe.FriendlyName);
					tti.Text += String.Format("\n--Base Time: {0}s", rNode.BaseRecipe.Time);
					tti.Text += String.Format("\n--Base Ingredients:");
					foreach (var kvp in rNode.BaseRecipe.IngredientSet)
					{
						tti.Text += String.Format("\n----{0} ({1})", kvp.Key.FriendlyName, kvp.Value.ToString());
					}
					tti.Text += String.Format("\n--Base Products:");
					foreach (var kvp in rNode.BaseRecipe.ProductSet)
					{
						tti.Text += String.Format("\n----{0} ({1})", kvp.Key.FriendlyName, kvp.Value.ToString());
					}
					if (!myGraphViewer.SimpleView)
					{
						/*
						tti.Text += String.Format("\n\nAssemblers:");
						foreach (var kvp in assemblerBox.AssemblerList)
						{
							tti.Text += String.Format("\n----{0} ({1})", kvp.Key.assembler.FriendlyName, kvp.Value.ToString());
							foreach (var Module in kvp.Key.modules.Where(m => m != null))
							{
								tti.Text += String.Format("\n------{0}", Module.FriendlyName);
							}
						}*/
					}

					if (myGraphViewer.SelectedAmountType == AmountType.FixedAmount)
					{
						tti.Text += String.Format("\n\nCurrent iterations: {0}", DisplayedNode.ActualRate);
					}
					else
					{
						tti.Text += String.Format("\n\nCurrent Rate: {0}/{1}",
							myGraphViewer.SelectedRateUnit == RateUnit.PerMinute ? DisplayedNode.ActualRate / 60 : DisplayedNode.ActualRate,
							myGraphViewer.SelectedRateUnit == RateUnit.PerMinute ? "m" : "s");
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

        public override void MouseDown(Point location, MouseButtons button)
		{
			MouseDownLocation = location;

			if (button == MouseButtons.Left)
			{
				myGraphViewer.MouseDownElement = this;
				DragOrigin = new Point(X, Y);
			}
		}

		public override void MouseUp(Point location, MouseButtons button, bool wasDragged)
		{
			if (myGraphViewer.MouseDownElement == this && !wasDragged)
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
				Console.WriteLine(location);
				foreach (ItemTab tab in SubElements.OfType<ItemTab>())
				{
					if (tab.ContainsPoint(location))
					{
						inItemTab = true;
						rightClickMenu.MenuItems.Clear();
						rightClickMenu.MenuItems.Add(new MenuItem("Delete connections",
							new EventHandler((o, e) =>
							{
								if (tab.LinkType == LinkType.Input)
								{
									var removedLinks = DisplayedNode.InputLinks.Where(l => l.Item == tab.Item);
									foreach (NodeLink link in removedLinks)
										myGraphViewer.Graph.DeleteLink(link);
								}
								else //if(tab.Type == LinkType.Output)
								{
									var removedLinks = DisplayedNode.OutputLinks.Where(l => l.Item == tab.Item);
									foreach (NodeLink link in removedLinks)
										myGraphViewer.Graph.DeleteLink(link);
								}

								myGraphViewer.Graph.UpdateNodeValues();
							})));
						rightClickMenu.Show(myGraphViewer, myGraphViewer.GraphToScreen(Point.Add(location, new Size(X, Y))));
					}
				}
				if (!inItemTab)
				{
                    rightClickMenu.MenuItems.Clear();
                    rightClickMenu.MenuItems.Add(new MenuItem("Delete node",
                        new EventHandler((o, e) =>
                        {
							myGraphViewer.Graph.DeleteNode(DisplayedNode);
							myGraphViewer.Graph.UpdateNodeValues();
                        })));
                    if (myGraphViewer.SelectedNodes.Count > 2 && myGraphViewer.SelectedNodes.Contains(this))
                    {
                        rightClickMenu.MenuItems.Add(new MenuItem("Delete selected nodes",
                        new EventHandler((o, e) =>
                        {
                            myGraphViewer.TryDeleteSelectedNodes();
                        })));
                    }
                    rightClickMenu.Show(myGraphViewer, myGraphViewer.GraphToScreen(Point.Add(location, new Size(X, Y))));
                }
            }
		}

		public override void Dragged(Point location)
		{
			ItemTab draggedTab = null;

			foreach (ItemTab tab in SubElements.OfType<ItemTab>())
			{
				if (tab.ContainsPoint(MouseDownLocation))
					draggedTab = tab;
			}

			if (draggedTab != null)
			{
				DraggedLinkElement newLink = new DraggedLinkElement(myGraphViewer, this, draggedTab.LinkType, draggedTab.Item);
				if (draggedTab.LinkType == LinkType.Input)
				{
					newLink.ConsumerElement = this;
				}
				else
				{
					newLink.SupplierElement = this;
				}
				myGraphViewer.MouseDownElement = newLink;
			}
			else
			{
				int xtemp = X;
				int ytemp = Y;

				xtemp += location.X - MouseDownLocation.X;
				ytemp += location.Y - MouseDownLocation.Y;
				//lock to grid if it is set & is visible
				if (myGraphViewer.ShowGrid)
				{
					xtemp = myGraphViewer.AlignToGrid(xtemp);
					ytemp = myGraphViewer.AlignToGrid(ytemp);
				}

				if (myGraphViewer.LockDragToAxis)
				{
					if (Math.Abs(DragOrigin.X - xtemp) > Math.Abs(DragOrigin.Y - ytemp))
						ytemp = myGraphViewer.AlignToGrid(DragOrigin.Y);
					else
						xtemp = myGraphViewer.AlignToGrid(DragOrigin.X);
				}
				else if (!StickyDragOrigin)
					DragOrigin = new Point(X, Y);

				this.Location = new Point(xtemp, ytemp);

				foreach (BaseNode node in DisplayedNode.InputLinks.Select<NodeLink, BaseNode>(l => l.Supplier))
				{
					myGraphViewer.GetElementForNode(node).UpdateTabOrder();
				}
				foreach (BaseNode node in DisplayedNode.OutputLinks.Select<NodeLink, BaseNode>(l => l.Consumer))
				{
					myGraphViewer.GetElementForNode(node).UpdateTabOrder();
				}
			}
		}

		public override void Dispose()
		{
			rightClickMenu.Dispose();
			base.Dispose();
		}
	}
}