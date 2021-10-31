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
		private static Brush SelectionOverlayBrush = new SolidBrush(Color.FromArgb(100, 100, 100, 200));

		private static Font size10Font = new Font(FontFamily.GenericSansSerif, 10);
		private static Font size7Font = new Font(FontFamily.GenericSansSerif, 7);

		private static StringFormat centreFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

		//most values are attempted to fit the grid (6 * 2^n) - ex: 72 = 6 * (4+8)
		public const int baseHeight = 96; //public because we actually use this as a nice offset value when making a new node (so when you drag a link and drop it it offsets the newly created node by half this)
		public const int baseWIconHeight = 128; //same as above
		private const int tabPadding = 7; //makes each tab be evenly spaced for grid
		private const int modD = 24; //(6*4) -> height and width will be divisible by this (-2 on each to leave a space between adjacent nodes)
		private const int passthroughNodeWidth = 72;
		private const int minWidth = 144;

		private Brush backgroundBrush;

		private List<ItemTabElement> inputTabs;
		private List<ItemTabElement> outputTabs;

		private ContextMenu rightClickMenu;

		private Point MouseDownLocation; //location where the mouse click down first happened - in graph coordinates (used to ensure that any drag operation begins at the start, and not at the point (+- a few pixels) where the drag was officially registed as a drag and not just a mouse click.
		private Point MouseDownNodeLocation; //location of this node the moment the mouse click down first happened - in graph coordinates
		private bool DragStarted;

		public NodeElement(ProductionGraphViewer graphViewer, BaseNode node) : base(graphViewer)
		{
			DisplayedNode = node;
			DragStarted = false;

			rightClickMenu = new ContextMenu();
			inputTabs = new List<ItemTabElement>();
			outputTabs = new List<ItemTabElement>();

			Width = (DisplayedNode is PassthroughNode)? passthroughNodeWidth : minWidth;
			Height = baseHeight;

			if (DisplayedNode is ConsumerNode cNode)
				backgroundBrush = cNode.IsValid ? demandBGBrush : missingBGBrush;
			else if (DisplayedNode is SupplierNode sNode)
				backgroundBrush = sNode.IsValid ? supplyBGBrush : missingBGBrush;
			else if (DisplayedNode is PassthroughNode pNode)
				backgroundBrush = pNode.IsValid ? passthroughBGBrush : missingBGBrush;
			else if (DisplayedNode is RecipeNode rNode)
				backgroundBrush = rNode.IsValid ? recipeBGBrush : missingBGBrush;
			else
				Trace.Fail("No branch for node: " + DisplayedNode.ToString());

			//first stage item tab creation - absolutely necessary in the constructor due to the creation and simultaneous linking of nodes being possible (drag to new node for example).
			foreach (Item item in DisplayedNode.Inputs)
				inputTabs.Add(new ItemTabElement(item, LinkType.Input, myGraphViewer, this));
			foreach (Item item in DisplayedNode.Outputs)
				outputTabs.Add(new ItemTabElement(item, LinkType.Output, myGraphViewer, this));
		}

		public void Update()
		{
			//size update
			int width = Math.Max((DisplayedNode is PassthroughNode) ? passthroughNodeWidth : minWidth, Math.Max(GetIconWidths(inputTabs), GetIconWidths(outputTabs)) + 10);
			int height = myGraphViewer.SimpleView ? baseHeight : baseWIconHeight;

			//final size confirms
			if (width % modD != 0)
			{
				width += modD;
				width -= width % modD;
			}
			if (height % modD != 0)
			{
				height += modD;
				height -= height % modD;
			}
			Width = width - 2;
			Height = height - 2;

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
				if (inputTabs.FirstOrDefault(tab => tab.Item == item) == null)
					inputTabs.Add(new ItemTabElement(item, LinkType.Input, myGraphViewer, this));
			foreach (Item item in DisplayedNode.Outputs)
				if (outputTabs.FirstOrDefault(tab => tab.Item == item) == null)
					outputTabs.Add(new ItemTabElement(item, LinkType.Output, myGraphViewer, this));

			//update tab values
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

		public ItemTabElement GetOutputLineItemTab(Item item) { return outputTabs.First(it => it.Item == item); }
		public ItemTabElement GetInputLineItemTab(Item item) { return inputTabs.First(it => it.Item == item); }

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

				//background
				Brush bgBrush = backgroundBrush;
				foreach (ItemTabElement tab in inputTabs.Union(outputTabs))
					if (tab.LinkType == LinkType.Input && DisplayedNode.IsOversupplied(tab.Item))
						bgBrush = oversuppliedBGBrush;

				RecipeNode rNode = DisplayedNode as RecipeNode;
				if (DisplayedNode.ManualRateNotMet() || (rNode != null && !(rNode.BaseRecipe.Enabled && rNode.BaseRecipe.HasEnabledAssemblers)))
					GraphicsStuff.FillRoundRect(trans.X - (Width / 2) - 5, trans.Y - (Height / 2) - 5, Width + 10, Height + 10, 13, graphics, Brushes.DarkRed);

				GraphicsStuff.FillRoundRect(trans.X - (Width / 2), trans.Y - (Height / 2), Width, Height, 8, graphics, bgBrush);
				graphics.DrawString(DisplayedNode.GetNameString(), size10Font, textBrush, trans.X, trans.Y, centreFormat);

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
#if DEBUG
			if (tab == null && DisplayedNode is RecipeNode rNode)
			{
				TooltipInfo tti = new TooltipInfo();
				tti.Direction = Direction.Left;
				tti.ScreenLocation = myGraphViewer.GraphToScreen(Point.Add(Location, new Size(Width / 2, 0)));
				tti.Text = String.Format("WH: {0},{1}\n", Width, Height);
				tti.Text += String.Format("XY: {0},{1}\n", X, Y);
			}
#endif
			if (tab == null)
			{
				TooltipInfo helpToolTipInfo = new TooltipInfo();
				helpToolTipInfo.Text = "Left click on this node to edit how fast it runs.\nRight click for options.";
				helpToolTipInfo.Direction = Direction.None;
				helpToolTipInfo.ScreenLocation = new Point(10, 10);
				toolTips.Add(helpToolTipInfo);
			}

			return toolTips;
		}

		public override void MouseDown(Point graph_point, MouseButtons button)
		{
			MouseDownLocation = graph_point;
			MouseDownNodeLocation = new Point(X, Y);

			if (button == MouseButtons.Left)
				myGraphViewer.MouseDownElement = this;
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
				Size offset = (Size)myGraphViewer.Grid.AlignToGrid(Point.Subtract(graph_point, (Size)MouseDownLocation));
				Point newLocation = Point.Add(MouseDownNodeLocation, offset);
				if(myGraphViewer.Grid.LockDragToAxis)
				{
					Point lockedDragOffset = Point.Subtract(graph_point, (Size)myGraphViewer.Grid.DragOrigin);

					if (Math.Abs(lockedDragOffset.X) > Math.Abs(lockedDragOffset.Y))
						newLocation.Y = myGraphViewer.Grid.DragOrigin.Y;
					else
						newLocation.X = myGraphViewer.Grid.DragOrigin.X;
				}

				if (Location != newLocation)
				{
					Location = newLocation;

					this.UpdateTabOrder();
					foreach (BaseNode node in DisplayedNode.InputLinks.Select(l => l.Supplier))
						myGraphViewer.NodeElementDictionary[node].UpdateTabOrder();
					foreach (BaseNode node in DisplayedNode.OutputLinks.Select(l => l.Consumer))
						myGraphViewer.NodeElementDictionary[node].UpdateTabOrder();
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