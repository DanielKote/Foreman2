using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Foreman
{
	public abstract class BaseNodeElement : GraphElement
	{
		public bool Highlighted = false; //selection - note that this doesnt mean it is or isnt in selection (at least not during drag operation - ex: dragging a not-selection over a group of selected nodes will change their highlight status, but wont add them to the 'selected' set until you let go of the drag)
		public BaseNode DisplayedNode { get; private set; }

		public override int X { get { return DisplayedNode.Location.X; } set { DisplayedNode.Location = new Point(value, DisplayedNode.Location.Y); } }
		public override int Y { get { return DisplayedNode.Location.Y; } set { DisplayedNode.Location = new Point(DisplayedNode.Location.X, value); } }
		public override Point Location { get { return DisplayedNode.Location; } set { DisplayedNode.Location = value; } }

		protected abstract Brush CleanBgBrush { get; }
		private static readonly Brush errorBgBrush = Brushes.Coral;
		private static readonly Brush ManualRateBGFilterBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0));

		private static readonly Brush equalFlowBorderBrush = Brushes.DarkGreen;
		private static readonly Brush oversuppliedFlowBorderBrush = Brushes.DarkGoldenrod;
		private static readonly Brush undersuppliedFlowBorderBrush = Brushes.DarkRed;

		private static readonly Brush selectionOverlayBrush = new SolidBrush(Color.FromArgb(100, 100, 100, 200));

		protected static readonly Brush TextBrush = Brushes.Black;
		protected static readonly Font BaseFont = new Font(FontFamily.GenericSansSerif, 10f);
		protected static readonly Font TitleFont = new Font(FontFamily.GenericSansSerif, 9.2f, FontStyle.Bold);

		protected static StringFormat TitleFormat = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };
		protected static StringFormat TextFormat = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };

		//most values are attempted to fit the grid (6 * 2^n) - ex: 72 = 6 * (4+8)
		protected const int BaseSimpleHeight = 94; // 98 fits grid, -2 for border
		protected const int BaseRecipeHeight = 130; //144 fits grid, -2 for border
		protected const int TabPadding = 7; //makes each tab be evenly spaced for grid
		protected const int WidthD = 24; //(6*4) -> width will be divisible by this (-2 on each to leave a space between adjacent nodes)
		protected const int PassthroughNodeWidth = 72;
		protected const int MinWidth = 144;

		protected List<ItemTabElement> InputTabs;
		protected List<ItemTabElement> OutputTabs;

		private Point MouseDownLocation; //location where the mouse click down first happened - in graph coordinates (used to ensure that any drag operation begins at the start, and not at the point (+- a few pixels) where the drag was officially registed as a drag and not just a mouse click.
		private Point MouseDownNodeLocation; //location of this node the moment the mouse click down first happened - in graph coordinates
		private bool DragStarted;

		protected ErrorNoticeElement errorNotice;

		public BaseNodeElement(ProductionGraphViewer graphViewer, BaseNode node) : base(graphViewer)
		{
			DisplayedNode = node;
			DragStarted = false;

			InputTabs = new List<ItemTabElement>();
			OutputTabs = new List<ItemTabElement>();

			errorNotice = new ErrorNoticeElement(graphViewer, this);
			errorNotice.Location = new Point(-Width / 2, -Height / 2);
			errorNotice.SetVisibility(false);

			//first stage item tab creation - absolutely necessary in the constructor due to the creation and simultaneous linking of nodes being possible (drag to new node for example).
			foreach (Item item in DisplayedNode.Inputs)
				InputTabs.Add(new ItemTabElement(item, LinkType.Input, base.graphViewer, this));
			foreach (Item item in DisplayedNode.Outputs)
				OutputTabs.Add(new ItemTabElement(item, LinkType.Output, base.graphViewer, this));
		}

		public virtual void Update()
		{
			//update error notice
			errorNotice.SetVisibility(DisplayedNode.State != NodeState.Clean);
			errorNotice.X = -Width / 2;
			errorNotice.Y = -Height / 2;


			//update tab values
			foreach (ItemTabElement tab in InputTabs)
				tab.UpdateValues(DisplayedNode.GetConsumeRate(tab.Item), DisplayedNode.GetSuppliedRate(tab.Item), DisplayedNode.IsOversupplied(tab.Item)); //for inputs we want the consumption/supply/oversupply values
			foreach (ItemTabElement tab in OutputTabs)
				tab.UpdateValues(DisplayedNode.GetSupplyRate(tab.Item), 0, false); //for outputs we only care to display the supply rate
			UpdateTabOrder();
		}

		private void UpdateTabOrder()
		{
			InputTabs = InputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();
			OutputTabs = OutputTabs.OrderBy(it => GetItemTabXHeuristic(it)).ToList();

			int x = -GetIconWidths(OutputTabs) / 2;
			foreach (ItemTabElement tab in OutputTabs)
			{
				x += TabPadding;
				tab.Location = new Point(x + (tab.Width / 2), -Height / 2);
				x += tab.Width;
			}
			x = -GetIconWidths(InputTabs) / 2;
			foreach (ItemTabElement tab in InputTabs)
			{
				x += TabPadding;
				tab.Location = new Point(x + (tab.Width / 2), Height / 2);
				x += tab.Width;
			}
		}

		protected int GetIconWidths(List<ItemTabElement> tabs)
		{
			int result = TabPadding;
			foreach (ItemTabElement tab in tabs)
				result += tab.Bounds.Width + TabPadding;
			return result;
		}

		private int GetItemTabXHeuristic(ItemTabElement tab)
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

		public ItemTabElement GetOutputLineItemTab(Item item) { return OutputTabs.First(it => it.Item == item); }
		public ItemTabElement GetInputLineItemTab(Item item) { return InputTabs.First(it => it.Item == item); }

		public override void UpdateVisibility(Rectangle graph_zone, int xborder = 0, int yborder = 0)
		{
			base.UpdateVisibility(graph_zone, xborder, yborder + 30); //account for the vertical item boxes
		}

		public override bool ContainsPoint(Point graph_point)
		{
			if (!Visible)
				return false;
			if (base.ContainsPoint(graph_point))
				return true;

			foreach (ItemTabElement tab in SubElements.OfType<ItemTabElement>())
				if (tab.ContainsPoint(graph_point))
					return true;
			if (errorNotice.ContainsPoint(graph_point))
				return true;

			return false;
		}

		protected override void Draw(Graphics graphics, bool simple)
		{
			Point trans = LocalToGraph(new Point(0, 0)); //all draw operations happen in graph 0,0 origin coordinates. So we need to transform all our draw operations to the local 0,0 (center of object)

			//background
			Brush bgBrush = DisplayedNode.State == NodeState.Error ? errorBgBrush : CleanBgBrush;
			Brush borderBrush = DisplayedNode.ManualRateNotMet() ? undersuppliedFlowBorderBrush : DisplayedNode.IsOversupplied() ? oversuppliedFlowBorderBrush : equalFlowBorderBrush;

			GraphicsStuff.FillRoundRect(trans.X - (Width / 2), trans.Y - (Height / 2), Width, Height, 10, graphics, borderBrush); //flow status border
			GraphicsStuff.FillRoundRect(trans.X - (Width / 2) + 3, trans.Y - (Height / 2) + 3, Width - 6, Height - 6, 7, graphics, bgBrush); //basic background (with given background brush)
			if (DisplayedNode.RateType == RateType.Manual)
				GraphicsStuff.FillRoundRect(trans.X - (Width / 2) + 3, trans.Y - (Height / 2) + 3, Width - 6, Height - 6, 7, graphics, ManualRateBGFilterBrush); //darken background if its a manual rate set

			if (DisplayedNode.State == NodeState.Warning)
				GraphicsStuff.FillRoundRectTLFlag(trans.X - (Width / 2) + 3, trans.Y - (Height / 2) + 3, Width / 2 - 6, Height / 2 - 6, 7, graphics, errorBgBrush); //warning flag


			//draw in all the inside details for this node
			DetailsDraw(graphics, trans, simple);

			//highlight
			if (Highlighted)
				GraphicsStuff.FillRoundRect(trans.X - (Width / 2), trans.Y - (Height / 2), Width, Height, 8, graphics, selectionOverlayBrush);
		}

		protected abstract void DetailsDraw(Graphics graphics, Point trans, bool simple); //draw the inside of the node.

		public override List<TooltipInfo> GetToolTips(Point graph_point)
		{
			GraphElement element = SubElements.FirstOrDefault(it => it.ContainsPoint(graph_point));
			List<TooltipInfo> subTooltips = element?.GetToolTips(graph_point) ?? null;
			List<TooltipInfo> myTooltips = GetMyToolTips(graph_point, subTooltips == null || subTooltips.Count == 0);

			if (myTooltips == null)
				myTooltips = new List<TooltipInfo>();
			if (subTooltips != null)
				myTooltips.AddRange(subTooltips);

			return myTooltips;
		}

		protected abstract List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive); //exclusive = true means no other tooltips are shown

		public override void MouseDown(Point graph_point, MouseButtons button)
		{
			MouseDownLocation = graph_point;
			MouseDownNodeLocation = new Point(X, Y);

			if (button == MouseButtons.Left)
				graphViewer.MouseDownElement = this;
		}

		public override void MouseUp(Point graph_point, MouseButtons button, bool wasDragged)
		{
			DragStarted = false;
			GraphElement subelement = SubElements.OfType<ItemTabElement>().FirstOrDefault(it => it.ContainsPoint(graph_point));
			if (!wasDragged)
			{
				if (subelement != null)
					subelement.MouseUp(graph_point, button, false);
				if (errorNotice.ContainsPoint(graph_point))
					errorNotice.MouseUp(graph_point, button, false);
				else
					MouseUpAction(graph_point, button);
			}
		}

		protected virtual void MouseUpAction(Point graph_point, MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				graphViewer.EditNode(this);
			}
			else if (button == MouseButtons.Right)
			{
				RightClickMenu.MenuItems.Clear();
				RightClickMenu.MenuItems.Add(new MenuItem("Delete node",
						new EventHandler((o, e) =>
						{
							graphViewer.Graph.DeleteNode(DisplayedNode);
							graphViewer.Graph.UpdateNodeValues();
						})));
				if (graphViewer.SelectedNodes.Count > 2 && graphViewer.SelectedNodes.Contains(this))
				{
					RightClickMenu.MenuItems.Add(new MenuItem("Delete selected nodes",
						new EventHandler((o, e) =>
						{
							graphViewer.TryDeleteSelectedNodes();
						}))
					{ });
				}
				RightClickMenu.Show(graphViewer, graphViewer.GraphToScreen(graph_point));
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
					graphViewer.StartLinkDrag(this, draggedTab.LinkType, draggedTab.Item);
				else
				{
					DragStarted = true;
				}
			}
			else //drag started -> proceed with dragging the node around
			{
				Size offset = (Size)Point.Subtract(graph_point, (Size)MouseDownLocation);
				Point newLocation = graphViewer.Grid.AlignToGrid(Point.Add(MouseDownNodeLocation, offset));
				if (graphViewer.Grid.LockDragToAxis)
				{
					Point lockedDragOffset = Point.Subtract(graph_point, (Size)graphViewer.Grid.DragOrigin);

					if (Math.Abs(lockedDragOffset.X) > Math.Abs(lockedDragOffset.Y))
						newLocation.Y = graphViewer.Grid.DragOrigin.Y;
					else
						newLocation.X = graphViewer.Grid.DragOrigin.X;
				}

				if (Location != newLocation)
				{
					Location = newLocation;

					this.UpdateTabOrder();
					foreach (BaseNode node in DisplayedNode.InputLinks.Select(l => l.Supplier))
						graphViewer.NodeElementDictionary[node].UpdateTabOrder();
					foreach (BaseNode node in DisplayedNode.OutputLinks.Select(l => l.Consumer))
						graphViewer.NodeElementDictionary[node].UpdateTabOrder();
				}
			}
		}
	}
}