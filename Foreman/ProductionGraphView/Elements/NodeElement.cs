using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Foreman
{
	public partial class NodeElement : GraphElement
	{
        public static bool StickyDragOrigin = false; //true: drag axies are established at start of drag; false: drag axies are established upon activation of axis limits (shift)
		public Point DragOrigin { get; private set; } //used to limit drag to horizontal or vertical (if such is selected)

		public bool Highlighted = false; //selection - note that this doesnt mean it is or isnt in selection (at least not during drag operation - ex: dragging a not-selection over a group of selected nodes will change their highlight status, but wont add them to the 'selected' set until you let go of the drag)
		public BaseNode DisplayedNode { get; private set; }

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
		public const int baseHeight = 96; //public because we actually use this as a nice offset value when making a new node (so when you drag a link and drop it it offsets the newly created node by half this)
		public const int baseWIconHeight = 128; //same as above
		private const int minWidth = 72;
		private const int dWidth = 24;
		private const int dHeight = 12;
		private const int tabPadding = 7; //makes each tab be evenly spaced for grid
		private const int maxT10Width = 144;

		private string Text;

		private Brush backgroundBrush;
		private Font myFont;

		private List<ItemTabElement> inputTabs;
		private List<ItemTabElement> outputTabs;

		private ContextMenu rightClickMenu;

		private Point MouseDownLocation; //location where the mouse click down first happened - in graph coordinates (used to ensure that any drag operation begins at the start, and not at the point (+- a few pixels) where the drag was officially registed as a drag and not just a mouse click.
		private bool DragStarted;

		public NodeElement(ProductionGraphViewer graphViewer, BaseNode node) : base(graphViewer)
		{
			Text = "";
			myFont = size10Font;
			DisplayedNode = node;
			DragStarted = false;

			rightClickMenu = new ContextMenu();
			inputTabs = new List<ItemTabElement>();
			outputTabs = new List<ItemTabElement>();

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

			if (DisplayedNode is RecipeNode || DisplayedNode is SupplierNode)
			{
				//assembler stuff
			}

			//first stage item tab creation - absolutely necessary in the constructor due to the creation and simultaneous linking of nodes being possible (drag to new node for example).
			foreach (Item item in DisplayedNode.Inputs)
			{
				ItemTabElement newTab = new ItemTabElement(item, LinkType.Input, myGraphViewer, this);
				inputTabs.Add(newTab);
			}
			foreach (Item item in DisplayedNode.Outputs)
			{
				ItemTabElement newTab = new ItemTabElement(item, LinkType.Output, myGraphViewer, this);
				outputTabs.Add(newTab);
			}
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

			//update tabs (necessary now that it is possible that an item was added or removed)... I am looking at you furnaces!!!
			//done by first checking all old tabs and removing any that are no longer part of the displayed node, then looking at the displayed node io and adding any new tabs that are necessary.
			//could potentially be done by just deleting all the old ones and remaking them from scratch, but come on - thats much more intensive than just doing some checks!
			foreach (ItemTabElement oldTab in inputTabs.Where(tab => !DisplayedNode.Inputs.Contains(tab.Item)).ToList())
			{
				foreach (NodeLink link in DisplayedNode.InputLinks.Where(link => link.Item == oldTab.Item).ToList())
					link.Delete();
				inputTabs.Remove(oldTab);
				oldTab.Dispose();
			}
			foreach (ItemTabElement oldTab in outputTabs.Where(tab => !DisplayedNode.Outputs.Contains(tab.Item)).ToList())
			{
				foreach (NodeLink link in DisplayedNode.OutputLinks.Where(link => link.Item == oldTab.Item).ToList())
					link.Delete();
				outputTabs.Remove(oldTab);
				oldTab.Dispose();
			}
			foreach (Item item in DisplayedNode.Inputs)
			{
				if (inputTabs.FirstOrDefault(tab => tab.Item == item) == null)
				{
					ItemTabElement newTab = new ItemTabElement(item, LinkType.Input, myGraphViewer, this);
					inputTabs.Add(newTab);
				}
			}
			foreach (Item item in DisplayedNode.Outputs)
			{
				if (outputTabs.FirstOrDefault(tab => tab.Item == item) == null)
				{
					ItemTabElement newTab = new ItemTabElement(item, LinkType.Output, myGraphViewer, this);
					outputTabs.Add(newTab);
				}
			}

			//size update
			int iconWidth = Math.Max(minWidth, getMaxIconWidths() + 10);
			int width, height;

			Graphics graphics = myGraphViewer.CreateGraphics();
			int minTextWidth = (int)graphics.MeasureString(Text, size10Font).Width + 10;
			myFont = size10Font;
			if (minTextWidth > maxT10Width && minTextWidth > iconWidth)
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
			foreach (ItemTabElement tab in inputTabs)
				tab.UpdateValues(DisplayedNode.GetConsumeRate(tab.Item), DisplayedNode.GetSuppliedRate(tab.Item), DisplayedNode.IsOversupplied(tab.Item)); //for inputs we want the consumption/supply/oversupply values
			foreach (ItemTabElement tab in outputTabs)
				tab.UpdateValues(DisplayedNode.GetSupplyRate(tab.Item), 0, false); //for outputs we only care to display the supply rate

			UpdateTabOrder();
		}

		protected void UpdateTabOrder()
		{
			inputTabs = inputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();
			outputTabs = outputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();

			int x = -GetIconWidths(outputTabs) / 2;
			foreach (ItemTabElement tab in outputTabs)
			{
				x += tabPadding;
				tab.Location = new Point(x + (tab.Width / 2), -Height / 2);
				x += tab.Width;
			}
			x = -GetIconWidths(inputTabs) / 2;
			foreach (ItemTabElement tab in inputTabs)
			{
				x += tabPadding;
				tab.Location = new Point(x + (tab.Width / 2), Height / 2);
				x += tab.Width;
			}
		}

		private int getMaxIconWidths()
		{
			return Math.Max(GetIconWidths(inputTabs), GetIconWidths(outputTabs));
		}

		private int GetIconWidths(List<ItemTabElement> tabs)
		{
			int result = tabPadding;
			foreach (ItemTabElement tab in tabs)
				result += tab.Bounds.Width + tabPadding;
			return result;
		}
		
		protected int GetItemTabXHeuristic(ItemTabElement tab)
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

		public ItemTabElement GetOutputLineItemTab(Item item)
		{
			if (!outputTabs.Any())
				return null;
			return outputTabs.First(it => it.Item == item);
		}

		public ItemTabElement GetInputLineItemTab(Item item)
		{
			if (!inputTabs.Any())
				return null;
			return inputTabs.First(it => it.Item == item);
		}

		public void beginEditingNodeRate()
		{
			DevNodeOptionsPanel newPanel = new DevNodeOptionsPanel(DisplayedNode, myGraphViewer);
			new FloatingTooltipControl(newPanel, Direction.Right, new Point(Location.X - (Width / 2), Location.Y), myGraphViewer);
		}

        public override void UpdateVisibility(Rectangle graph_zone, int xborder = 0, int yborder = 0)
        {
			base.UpdateVisibility(graph_zone, xborder, yborder + 30); //account for the vertical item boxes
        }

		public override bool ContainsPoint(Point graph_point)
		{
			if (base.ContainsPoint(graph_point))
				return true;

			foreach (ItemTabElement tab in SubElements.OfType<ItemTabElement>())
				if (tab.ContainsPoint(graph_point))
					return true;

			return false;
		}

		protected override void Draw(Graphics graphics)
		{
			if (Visible)
			{
				Point trans = ConvertToGraph(new Point(0, 0)); //all draw operations happen in graph 0,0 origin coordinates. So we need to transform all our draw operations to the local 0,0 (center of object)

				Brush bgBrush = backgroundBrush;
				foreach (ItemTabElement tab in inputTabs.Union(outputTabs))
					if (tab.LinkType == LinkType.Input && DisplayedNode.IsOversupplied(tab.Item))
						bgBrush = oversuppliedBGBrush;

				RecipeNode rNode = DisplayedNode as RecipeNode;
				if (DisplayedNode.ManualRateNotMet() || (rNode != null && !(rNode.BaseRecipe.Enabled && rNode.BaseRecipe.HasEnabledAssemblers)))
					GraphicsStuff.FillRoundRect(trans.X - (Width / 2) - 5, trans.Y - (Height / 2) - 5, Width + 10, Height + 10, 13, graphics, Brushes.DarkRed);

				GraphicsStuff.FillRoundRect(trans.X - (Width / 2), trans.Y - (Height / 2), Width, Height, 8, graphics, bgBrush);
				graphics.DrawString(Text, myFont, textBrush, trans.X, trans.Y, centreFormat);

				if (rNode != null)
				{
					int prodCount = rNode.AssemblerModules.Count(module => module.ProductivityBonus != 0) + rNode.BeaconModules.Count(module => module.ProductivityBonus != 0);
					for (int i = 0; i < prodCount; i++)
						graphics.DrawEllipse(productivityTickBGPen, trans.X - (Width / 2) - 2, trans.Y - (Height / 2) + 20 + i * 10, 5, 5);
				}

				if (Highlighted)
					GraphicsStuff.FillRoundRect(trans.X - (Width / 2), trans.Y - (Height / 2), Width, Height, 8, graphics, SelectionOverlayBrush);
			}
		}

		public override List<TooltipInfo> GetToolTips(Point graph_point)
		{
			ItemTabElement tab = SubElements.OfType<ItemTabElement>().FirstOrDefault(it => it.ContainsPoint(graph_point));
			List<TooltipInfo> toolTips = tab?.GetToolTips(graph_point) ?? new List<TooltipInfo>();

			if (tab == null && DisplayedNode is RecipeNode rNode)
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

				tti.Text += String.Format("\n\nCurrent Rate: {0}", DisplayedNode.ActualRate);
				toolTips.Add(tti);
			}

			TooltipInfo helpToolTipInfo = new TooltipInfo();
			helpToolTipInfo.Text = "Left click on this node to edit how fast it runs\nRight click for options";
			helpToolTipInfo.Direction = Direction.None;
			helpToolTipInfo.ScreenLocation = new Point(10, 10);
			toolTips.Add(helpToolTipInfo);

			return toolTips;
		}

        public override void MouseDown(Point graph_point, MouseButtons button)
		{
			MouseDownLocation = graph_point;

			if (button == MouseButtons.Left)
			{
				myGraphViewer.MouseDownElement = this;
				DragOrigin = new Point(X, Y);
			}
		}

		public override void MouseUp(Point graph_point, MouseButtons button, bool wasDragged)
		{
			DragStarted = false;

			if (button == MouseButtons.Left && myGraphViewer.MouseDownElement == this && !wasDragged)
			{
				if ((Control.ModifierKeys & Keys.Control) == 0 && (Control.ModifierKeys & Keys.Alt) == 0)
					beginEditingNodeRate();
			}
			else if (button == MouseButtons.Right)
			{
				//check if we clicked an item tab (and process it), if not -> process node
				ItemTabElement tab = SubElements.OfType<ItemTabElement>().FirstOrDefault(it => it.ContainsPoint(graph_point));
				tab?.MouseUp(graph_point, button, wasDragged);
				if (tab == null)
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
                    rightClickMenu.Show(myGraphViewer, myGraphViewer.GraphToScreen(graph_point));
                }
            }
		}

		public override void Dragged(Point graph_point)
		{
			if (!DragStarted)
			{
				ItemTabElement draggedTab = null;
				foreach (ItemTabElement tab in SubElements.OfType<ItemTabElement>())
					if (tab.ContainsPoint(MouseDownLocation))
						draggedTab = tab;
				if (draggedTab != null)
					myGraphViewer.StartLinkDrag(this, draggedTab.LinkType, draggedTab.Item);
				else
					DragStarted = true;
			}
			else //drag started -> proceed with dragging the node around
			{
				int xtemp = graph_point.X;
				int ytemp = graph_point.Y;

				//lock to grid if it is set & is visible
				if (myGraphViewer.Grid.ShowGrid)
				{
					xtemp = myGraphViewer.Grid.AlignToGrid(xtemp);
					ytemp = myGraphViewer.Grid.AlignToGrid(ytemp);
				}

				if (myGraphViewer.Grid.LockDragToAxis)
				{
					if (Math.Abs(DragOrigin.X - xtemp) > Math.Abs(DragOrigin.Y - ytemp))
						ytemp = myGraphViewer.Grid.AlignToGrid(DragOrigin.Y);
					else
						xtemp = myGraphViewer.Grid.AlignToGrid(DragOrigin.X);
				}
				else if (!StickyDragOrigin)
					DragOrigin = new Point(X, Y);

				this.Location = new Point(xtemp, ytemp);

				foreach (BaseNode node in DisplayedNode.InputLinks.Select<NodeLink, BaseNode>(l => l.Supplier))
					myGraphViewer.NodeElementDictionary[node].UpdateTabOrder();
				foreach (BaseNode node in DisplayedNode.OutputLinks.Select<NodeLink, BaseNode>(l => l.Consumer))
					myGraphViewer.NodeElementDictionary[node].UpdateTabOrder();
			}
		}

		public override void Dispose()
		{
			rightClickMenu.Dispose();
			base.Dispose();
		}
	}
}