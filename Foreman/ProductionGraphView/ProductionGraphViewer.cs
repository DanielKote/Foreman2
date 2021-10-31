using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace Foreman
{
	public enum AmountType { FixedAmount, Rate }
	public enum RateUnit { PerMinute, PerSecond }

	[Serializable]
	public partial class ProductionGraphViewer : UserControl, ISerializable
	{

		private enum DragOperation { None, Item, Selection, Processed }
		public enum NewNodeType { Disconnected, Supplier, Consumer }

		public AmountType SelectedAmountType { get; set; }
		public RateUnit SelectedRateUnit { get; set; }

		public HashSet<GraphElement> Elements = new HashSet<GraphElement>();
		public ProductionGraph Graph;
		private Point lastMouseDragPoint;
		public Point ViewOffset;
		public float ViewScale = 1f;
		private GraphElement mouseDownElement;
		public GraphElement MouseDownElement
		{
			get { return mouseDownElement; }
			set { mouseDownStartScreenPoint = Control.MousePosition; mouseDownElement = value; }
		}
		private Point mouseDownStartScreenPoint;
		private DragOperation currentDragOperation = DragOperation.None;
		private bool viewBeingDragged = false; //separate from dragOperation due to being able to drag view at all stages of dragOperation
		private const int minDragDiff = 30;

		public int CurrentGridUnit = 0;
		public int CurrentMajorGridUnit = 0;
		public bool ShowGrid = false;
		public bool LockDragToAxis = false;
		public Rectangle SelectionZone;
		public Point SelectionZoneOriginPoint;

        public IReadOnlyCollection<NodeElement> SelectedNodes { get { return selectedNodes; } }
		private HashSet<NodeElement> selectedNodes; //main list of selected nodes
		private HashSet<NodeElement> CurrentSelectionNodes; //list of nodes currently under the selection zone (which can be added/removed/replace the full list)

		private Pen gridPen = new Pen(Color.FromArgb(220, 220, 220), 1);
		private Pen gridMPen = new Pen(Color.FromArgb(180, 180, 180), 1);
		private Brush gridBrush = new SolidBrush(Color.FromArgb(230, 230, 230));
		private Pen zeroAxisPen = new Pen(Color.FromArgb(140, 140, 140), 2);
		private Pen lockedAxisPen = new Pen(Color.FromArgb(180, 80, 80), 4);
		private Pen pausedBorders = new Pen(Color.FromArgb(255, 80, 80), 5);
		private Brush backgroundBrush = new SolidBrush(Color.White);
		private Pen selectionPen = new Pen(Color.FromArgb(100, 100, 200), 2);

		private readonly Font size10Font = new Font(FontFamily.GenericSansSerif, 10);

		public bool SimpleView { get; set; } //simple: show only the item/recipe names.

		public bool DynamicLinkWidth = false;
		private const int minLinkWidth = 3;
		private const int maxLinkWidth = 40;

		public HashSet<FloatingTooltipControl> floatingTooltipControls = new HashSet<FloatingTooltipControl>();
		StringFormat stringFormat = new StringFormat(); //used for tooltip drawing so as not to create a new one each time

		public DataCache DCache;

		private Rectangle visibleGraphBounds;

		private ContextMenu rightClickMenu = new ContextMenu();

		public ProductionGraphViewer()
		{
			InitializeComponent();
			MouseWheel += new MouseEventHandler(ProductionGraphViewer_MouseWheel);
			Resize += new EventHandler(ProductionGraphViewer_Resized);
			
			ViewOffset = new Point(Width / -2, Height / -2);
			ViewScale = 1f;

			Graph = new ProductionGraph();
            //Graph.ClearGraph()
            Graph.NodeAdded += Graph_NodeAdded;
            Graph.NodeDeleted += Graph_NodeDeleted;
            Graph.LinkAdded += Graph_LinkAdded;
            Graph.LinkDeleted += Graph_LinkDeleted;
            Graph.NodeValuesUpdated += Graph_NodeValuesUpdated;

			selectedNodes = new HashSet<NodeElement>();
			CurrentSelectionNodes = new HashSet<NodeElement>();

			UpdateGraphBounds();
			Invalidate();
		}

        private void Graph_NodeValuesUpdated(object sender, EventArgs e)
        {
			try
			{
				foreach (NodeElement node in Elements.OfType<NodeElement>().ToList())
					node.Update();
			}
			catch (OverflowException) { }//Same as when working out node values, there's not really much to do here... Maybe I could show a tooltip saying the numbers are too big or something...
			Invalidate();
		}

        private void Graph_LinkDeleted(object sender, NodeLinkEventArgs e)
        {
			LinkElement deletedElement = Elements.OfType<LinkElement>().FirstOrDefault(l => l.DisplayedLink == e.nodeLink);
			Elements.Remove(deletedElement);
			Invalidate();
        }

        private void Graph_LinkAdded(object sender, NodeLinkEventArgs e)
        {
			NodeElement supplier = Elements.OfType<NodeElement>().FirstOrDefault(n => n.DisplayedNode == e.nodeLink.Supplier);
			NodeElement consumer = Elements.OfType<NodeElement>().FirstOrDefault(n => n.DisplayedNode == e.nodeLink.Consumer);
			Elements.Add(new LinkElement(this, e.nodeLink, supplier, consumer));
			Invalidate();
        }

        private void Graph_NodeDeleted(object sender, NodeEventArgs e)
        {
			NodeElement deletedElement = Elements.OfType<NodeElement>().FirstOrDefault(l => l.DisplayedNode == e.node);
			Elements.Remove(deletedElement);
			Invalidate();
		}

        private void Graph_NodeAdded(object sender, NodeEventArgs e)
        {
			Elements.Add(new NodeElement(this, e.node));
			Invalidate();
		}

        //----------------------------------------------Adding new node functions

        public void AddItem(Point drawOrigin, Point newLocation)
		{
			ItemChooserPanel itemChooser = new ItemChooserPanel(this, drawOrigin);
			itemChooser.Show(selectedItem => {
				if (selectedItem != null)
					AddRecipe(drawOrigin, selectedItem, newLocation, NewNodeType.Disconnected);
			});
		}

		public void AddRecipe(Point drawOrigin, Item baseItem, Point newLocation, NewNodeType nNodeType , NodeElement originElement = null)
		{
			if (nNodeType != NewNodeType.Disconnected && (originElement == null || baseItem == null)) //just in case check (should
				Trace.Fail("Origin element or base item not provided for a new (linked) node");

			if (ShowGrid)
				newLocation = new Point(AlignToGrid(newLocation.X), AlignToGrid(newLocation.Y));

			fRange tempRange = new fRange(0,0, true);
			if (baseItem != null && baseItem.IsTemperatureDependent)
			{
				if (nNodeType == NewNodeType.Consumer) //need to check all nodes down to recipes for range of temperatures being produced
					tempRange = LinkElement.GetTemperatureRange(baseItem, originElement, LinkType.Output);
				else if (nNodeType == NewNodeType.Supplier) //need to check all nodes up to recipes for range of temperatures being consumed (guaranteed to be in a SINGLE [] range)
					tempRange = LinkElement.GetTemperatureRange(baseItem, originElement, LinkType.Input);
			}

			RecipeChooserPanel recipeChooser = new RecipeChooserPanel(this, drawOrigin, baseItem, tempRange, nNodeType != NewNodeType.Consumer, nNodeType != NewNodeType.Supplier);
			BaseNode newNode = null;
			recipeChooser.Show((nodeType, recipe) =>
			{
				switch(nodeType)
                {
					case NodeType.Consumer:
						newNode = Graph.CreateConsumerNode(baseItem, newLocation);
						break;
					case NodeType.Supplier:
						newNode = Graph.CreateSupplierNode(baseItem, newLocation);
						break;
					case NodeType.Passthrough:
						newNode = Graph.CreatePassthroughNode(baseItem, newLocation);
						break;
					case NodeType.Recipe:
						newNode = Graph.CreateRecipeNode(recipe, newLocation);
						break;
                }

				if (nNodeType == NewNodeType.Consumer)
					Graph.CreateLink(originElement.DisplayedNode, newNode, baseItem);
				else if (nNodeType == NewNodeType.Supplier)
					Graph.CreateLink(newNode, originElement.DisplayedNode, baseItem);

				Graph.UpdateNodeValues();
			});
        }

		public NodeElement GetElementForNode(BaseNode node)
		{
			return Elements.OfType<NodeElement>().FirstOrDefault(e => e.DisplayedNode == node);
		}

		public IEnumerable<GraphElement> GetElementsAtPoint(Point point)
		{
			foreach (GraphElement element in GetPaintingOrder().Reverse())
				if (element.ContainsPoint(point))
					yield return element;
		}

		public void TryDeleteSelectedNodes()
		{
			bool proceed = true;
			if (selectedNodes.Count > 10)
				proceed = (MessageBox.Show("You are deleting " + selectedNodes.Count + " nodes. \nAre you sure?", "Confirm delete.", MessageBoxButtons.YesNo) == DialogResult.Yes);
			if (proceed)
			{
				foreach (NodeElement node in selectedNodes)
					Graph.DeleteNode(node.DisplayedNode);
				selectedNodes.Clear();
				Graph.UpdateNodeValues();
			}
		}

		private void UpdateSelection()
		{
			if ((Control.ModifierKeys & Keys.Alt) != 0) //remove zone
			{
				foreach (NodeElement selectedNode in selectedNodes)
					selectedNode.Selected = true;
				foreach (NodeElement newlySelectedNode in CurrentSelectionNodes)
					newlySelectedNode.Selected = false;
			}
			else if ((Control.ModifierKeys & Keys.Control) != 0)  //add zone
			{
				foreach (NodeElement selectedNode in selectedNodes)
					selectedNode.Selected = true;
				foreach (NodeElement newlySelectedNode in CurrentSelectionNodes)
					newlySelectedNode.Selected = true;
			}
			else //add zone (additive with ctrl or simple selection)
			{
				foreach (NodeElement newlySelectedNode in CurrentSelectionNodes)
					newlySelectedNode.Selected = true;
			}
		}

		public void AlignSelected()
		{
			foreach (NodeElement ne in selectedNodes)
				ne.Location = new Point(AlignToGrid(ne.X), AlignToGrid(ne.Y));
			Invalidate();
		}

		//----------------------------------------------Paint functions

		public IEnumerable<GraphElement> GetPaintingOrder()
		{
			foreach (LinkElement element in Elements.OfType<LinkElement>())
				yield return element;
			foreach (NodeElement element in Elements.OfType<NodeElement>())
				yield return element;
			foreach (DraggedLinkElement element in Elements.OfType<DraggedLinkElement>())
				yield return element;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//update visibility of all elements
			foreach (GraphElement element in GetPaintingOrder())
			{
				element.UpdateVisibility(visibleGraphBounds, 0, 30); //give 30 border for elements to account for item boxes
			}

			//proceed with the paint operations
			base.OnPaint(e);
			e.Graphics.ResetTransform();
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			e.Graphics.Clear(this.BackColor);
			e.Graphics.TranslateTransform(Width / 2, Height / 2);
			e.Graphics.ScaleTransform(ViewScale, ViewScale);
			e.Graphics.TranslateTransform(ViewOffset.X, ViewOffset.Y);

			Paint(e.Graphics);
		}

		public new void Paint(Graphics graphics)
		{
			gridPen.Width = 1 / ViewScale;
			gridMPen.Width = 1 / ViewScale;
			zeroAxisPen.Width = 2 / ViewScale;
			lockedAxisPen.Width = 3 / ViewScale;
			selectionPen.Width = 2 / ViewScale;

			//background
			graphics.FillRectangle(backgroundBrush, visibleGraphBounds);

			if (ShowGrid)
			{
				//minor grid
				if (CurrentGridUnit > 0)
				{
					if ((visibleGraphBounds.Width > CurrentGridUnit) && (Bounds.Height / (visibleGraphBounds.Width / CurrentGridUnit)) > 1)
					{
						for (int ix = visibleGraphBounds.X - (visibleGraphBounds.X % CurrentGridUnit); ix < visibleGraphBounds.X + visibleGraphBounds.Width; ix += CurrentGridUnit)
							graphics.DrawLine(gridPen, ix, visibleGraphBounds.Y, ix, visibleGraphBounds.Y + visibleGraphBounds.Height);

						for (int iy = visibleGraphBounds.Y - (visibleGraphBounds.Y % CurrentGridUnit); iy < visibleGraphBounds.Y + visibleGraphBounds.Height; iy += CurrentGridUnit)
							graphics.DrawLine(gridPen, visibleGraphBounds.X, iy, visibleGraphBounds.X + visibleGraphBounds.Width, iy);
					}
					else
						graphics.FillRectangle(gridBrush, visibleGraphBounds);
				}

				//major grid
				if (CurrentMajorGridUnit > CurrentGridUnit)
				{
					if ((visibleGraphBounds.Width > CurrentMajorGridUnit) && (Bounds.Height / (visibleGraphBounds.Width / CurrentMajorGridUnit)) > 2)
					{
						for (int ix = visibleGraphBounds.X - (visibleGraphBounds.X % CurrentMajorGridUnit); ix < visibleGraphBounds.X + visibleGraphBounds.Width; ix += CurrentMajorGridUnit)
							graphics.DrawLine(gridMPen, ix, visibleGraphBounds.Y, ix, visibleGraphBounds.Y + visibleGraphBounds.Height);

						for (int iy = visibleGraphBounds.Y - (visibleGraphBounds.Y % CurrentMajorGridUnit); iy < visibleGraphBounds.Y + visibleGraphBounds.Height; iy += CurrentMajorGridUnit)
							graphics.DrawLine(gridMPen, visibleGraphBounds.X, iy, visibleGraphBounds.X + visibleGraphBounds.Width, iy);
					}
				}

				//zero axis
				graphics.DrawLine(zeroAxisPen, 0, visibleGraphBounds.Y, 0, visibleGraphBounds.Y + visibleGraphBounds.Height);
				graphics.DrawLine(zeroAxisPen, visibleGraphBounds.X, 0, visibleGraphBounds.X + visibleGraphBounds.Width, 0);
			}

			//drag axis
			if(LockDragToAxis && currentDragOperation == DragOperation.Item)
            {
                NodeElement draggedNode = MouseDownElement as NodeElement;
                int xaxis = draggedNode.DragOrigin.X;
                int yaxis = draggedNode.DragOrigin.Y;
                xaxis = AlignToGrid(xaxis);
                yaxis = AlignToGrid(yaxis);

				graphics.DrawLine(lockedAxisPen, xaxis, visibleGraphBounds.Y, xaxis, visibleGraphBounds.Y + visibleGraphBounds.Height);
				graphics.DrawLine(lockedAxisPen, visibleGraphBounds.X, yaxis, visibleGraphBounds.X + visibleGraphBounds.Width, yaxis);
			}

			//process link element widths
			if (DynamicLinkWidth)
			{
				float max = 0;
				foreach (LinkElement element in Elements.OfType<LinkElement>())
					max = Math.Max(max, element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item));
				if (max > 0)
					foreach (LinkElement element in Elements.OfType<LinkElement>())
						element.LinkWidth = minLinkWidth + (maxLinkWidth - minLinkWidth) * (element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item) / max) / (element.Item.IsFluid ? 10 : 1);
				else
					foreach (LinkElement element in Elements.OfType<LinkElement>())
						element.LinkWidth = minLinkWidth;
			}
			else
			{
				foreach (LinkElement element in Elements.OfType<LinkElement>())
					element.LinkWidth = minLinkWidth;
			}

			//all elements (nodes & lines)
			foreach (GraphElement element in GetPaintingOrder())
			element.Paint(graphics, new Point(0,0));

			//selection zone
			if(currentDragOperation == DragOperation.Selection)
				graphics.DrawRectangle(selectionPen, SelectionZone);

			//everything below will be drawn directly on the screen instead of scaled/shifted based on graph
			graphics.ResetTransform();

			//floating tooltips
			if (currentDragOperation == DragOperation.None && !viewBeingDragged)
			{
				foreach (var fttp in floatingTooltipControls)
					DrawTooltip(GraphToScreen(fttp.GraphLocation), fttp.Control.Size, fttp.Direction, graphics, null);

				var element = GetElementsAtPoint(ScreenToGraph(PointToClient(Control.MousePosition))).FirstOrDefault();
				if (element != null)
				{
					foreach (TooltipInfo tti in element.GetToolTips(Point.Subtract(ScreenToGraph(PointToClient(Control.MousePosition)), (Size)element.Location)))
						DrawTooltip(tti.ScreenLocation, tti.ScreenSize, tti.Direction, graphics, tti.Text);
				}
			}

			//paused border
			if (Graph != null && Graph.PauseUpdates) //graph null check is purely for design view
				graphics.DrawRectangle(pausedBorders, 0, 0, Width - 3, Height - 3);
		}

		private void DrawTooltip(Point screenArrowPoint, Size size, Direction direction, Graphics graphics, String text = null)
		{
			if(text != null)
            {
				SizeF stringSize = graphics.MeasureString(text, size10Font);
				size = new Size((int)stringSize.Width, (int)stringSize.Height);
			}

			int border = 2;
			int arrowSize = 10;
			Point arrowPoint1 = new Point();
			Point arrowPoint2 = new Point();

			stringFormat.LineAlignment = StringAlignment.Center;

			switch (direction)
			{
				case Direction.Down:
					arrowPoint1 = new Point(screenArrowPoint.X - arrowSize / 2, screenArrowPoint.Y - arrowSize);
					arrowPoint2 = new Point(screenArrowPoint.X + arrowSize / 2, screenArrowPoint.Y - arrowSize);
					stringFormat.Alignment = StringAlignment.Center;
					break;
				case Direction.Left:
					arrowPoint1 = new Point(screenArrowPoint.X + arrowSize, screenArrowPoint.Y - arrowSize / 2);
					arrowPoint2 = new Point(screenArrowPoint.X + arrowSize, screenArrowPoint.Y + arrowSize / 2);
					stringFormat.Alignment = StringAlignment.Near;
					break;
				case Direction.Up:
					arrowPoint1 = new Point(screenArrowPoint.X - arrowSize / 2, screenArrowPoint.Y + arrowSize);
					arrowPoint2 = new Point(screenArrowPoint.X + arrowSize / 2, screenArrowPoint.Y + arrowSize);
					stringFormat.Alignment = StringAlignment.Center;
					break;
				case Direction.Right:
					arrowPoint1 = new Point(screenArrowPoint.X - arrowSize, screenArrowPoint.Y - arrowSize / 2);
					arrowPoint2 = new Point(screenArrowPoint.X - arrowSize, screenArrowPoint.Y + arrowSize / 2);
					stringFormat.Alignment = StringAlignment.Near;
					break;
			}

			Rectangle rect = getTooltipScreenBounds(screenArrowPoint, size, direction);
			Point[] points = new Point[] { screenArrowPoint, arrowPoint1, arrowPoint2 };

			if (direction == Direction.None)
			{
				rect = new Rectangle(screenArrowPoint, size);
				stringFormat.Alignment = StringAlignment.Center;
			}

			graphics.FillPolygon(Brushes.DarkGray, points);
			GraphicsStuff.FillRoundRect(rect.X - border, rect.Y - border, rect.Width + border * 2, rect.Height + border * 2, 3, graphics, Brushes.DarkGray);

			Point point;
			if (stringFormat.Alignment == StringAlignment.Center)
			{
				point = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
			}
			else
			{
				point = new Point(rect.X, rect.Y + rect.Height / 2);
			}
			graphics.DrawString(text, size10Font, Brushes.White, point, stringFormat);

		}

		public Rectangle getTooltipScreenBounds(Point screenArrowPoint, Size screenSize, Direction direction)
		{
			Point centreOffset = new Point();
			int arrowSize = 10;

			switch (direction)
			{
				case Direction.Down:
					centreOffset = new Point(0, -arrowSize - screenSize.Height / 2);
					break;
				case Direction.Left:
					centreOffset = new Point(arrowSize + screenSize.Width / 2, 0);
					break;
				case Direction.Up:
					centreOffset = new Point(0, arrowSize + screenSize.Height / 2);
					break;
				case Direction.Right:
					centreOffset = new Point(-arrowSize - screenSize.Width / 2, 0);
					break;
			}
			int X = (screenArrowPoint.X + centreOffset.X - screenSize.Width / 2);
			int Y = (screenArrowPoint.Y + centreOffset.Y - screenSize.Height / 2);
			int Width = screenSize.Width;
			int Height = screenSize.Height;

			return new Rectangle(X, Y, Width, Height);
		}

		//----------------------------------------------Mouse & keyboard event functions

		private void ProductionGraphViewer_MouseDown(object sender, MouseEventArgs e)
		{
			ClearFloatingControls();

			Focus();
			ActiveControl = null;

			var clickedElement = GetElementsAtPoint(ScreenToGraph(e.Location)).FirstOrDefault();
			if (clickedElement != null)
			{
				clickedElement.MouseDown(Point.Subtract(ScreenToGraph(e.Location), (Size)clickedElement.Location), e.Button);
			}

			if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Right && clickedElement == null))
			{
				viewBeingDragged = true;
				lastMouseDragPoint = new Point(e.X, e.Y);
			}

			if (e.Button == MouseButtons.Left)
            {
				if (clickedElement == null)
				{
					SelectionZoneOriginPoint = ScreenToGraph(e.Location);
					SelectionZone = new Rectangle();
					if ((Control.ModifierKeys & Keys.Control) == 0 && (Control.ModifierKeys & Keys.Alt) == 0) //clear all selected nodes if we arent using modifier keys
					{
						foreach (NodeElement ne in selectedNodes)
							ne.Selected = false;
						selectedNodes.Clear();
					}
				}
            }
		}

		private void ProductionGraphViewer_MouseUp(object sender, MouseEventArgs e)
		{
			ClearFloatingControls();

			Focus();

			if (!viewBeingDragged && currentDragOperation != DragOperation.Selection) //dont care about mouse up operations on elements if we were dragging view or selection
			{
				GraphElement element = GetElementsAtPoint(ScreenToGraph(e.Location)).FirstOrDefault();
				if (element != null)
				{
					element.MouseUp(Point.Subtract(ScreenToGraph(e.Location), (Size)element.Location), e.Button, (currentDragOperation == DragOperation.Item));
				}
			}

			switch (e.Button)
			{
				case MouseButtons.Right:
				case MouseButtons.Middle:
                    viewBeingDragged = false;
                    break;
				case MouseButtons.Left:
					//finished selecting the given zone (process selected nodes)
					if (currentDragOperation == DragOperation.Selection)
					{
						if ((Control.ModifierKeys & Keys.Alt) != 0) //removal zone processing
						{
							foreach (NodeElement newlySelectedNode in CurrentSelectionNodes)
								selectedNodes.Remove(newlySelectedNode);
						}
						else
						{
							if ((Control.ModifierKeys & Keys.Control) == 0) //if we arent using control, then we are just selecting
								selectedNodes.Clear();

							foreach (NodeElement newlySelectedNode in CurrentSelectionNodes)
								selectedNodes.Add(newlySelectedNode);
						}
						CurrentSelectionNodes.Clear();
					}
					//this is a release of a left click (non-drag operation) -> modify selection if clicking on node & using modifier keys
					else if (currentDragOperation == DragOperation.None && MouseDownElement is NodeElement clickedNode)
					{
						if ((Control.ModifierKeys & Keys.Alt) != 0) //remove
						{
							selectedNodes.Remove(clickedNode);
							clickedNode.Selected = false;
							MouseDownElement = null;
							Invalidate();
						}
						else if ((Control.ModifierKeys & Keys.Control) != 0) //add if unselected, remove if selected
						{
							if (clickedNode.Selected)
								selectedNodes.Remove(clickedNode);
							else
								selectedNodes.Add(clickedNode);

							clickedNode.Selected = !clickedNode.Selected;
							MouseDownElement = null;
							Invalidate();
						}
					}
					
					currentDragOperation = DragOperation.None;
					MouseDownElement = null;
					break;
			}
		}

        private void ProductionGraphViewer_MouseMove(object sender, MouseEventArgs e)
		{
			LockDragToAxis = (Control.ModifierKeys & Keys.Shift) != 0;

			if (currentDragOperation != DragOperation.Selection) //dont care about element mouse move operations during selection operation
			{

				var element = GetElementsAtPoint(ScreenToGraph(e.Location)).FirstOrDefault();
				if (element != null)
				{
					element.MouseMoved(Point.Subtract(ScreenToGraph(e.Location), (Size)element.Location));
				}
			}

			switch(currentDragOperation)
			{
				case DragOperation.None: //check for minimal distance to be considered a drag operation
					Point dragDiff = Point.Subtract(Control.MousePosition, (Size)mouseDownStartScreenPoint);
					if (dragDiff.X * dragDiff.X + dragDiff.Y * dragDiff.Y > minDragDiff)
					{
						if (MouseDownElement != null) //there is an item under the mouse during drag
								currentDragOperation = DragOperation.Item;
						else if ((Control.MouseButtons & MouseButtons.Left) != 0)
								currentDragOperation = DragOperation.Selection;
					}
					break;

				case DragOperation.Item:
					if (selectedNodes.Contains(MouseDownElement)) //dragging a group
					{
						Point startPoint = MouseDownElement.Location;
						MouseDownElement.Dragged(Point.Subtract(ScreenToGraph(e.Location), (Size)MouseDownElement.Location));
						Point endPoint = MouseDownElement.Location;
						if (startPoint != endPoint)
							foreach(NodeElement node in selectedNodes)
								if (node != MouseDownElement)
									node.Location = new Point(node.X + endPoint.X - startPoint.X, node.Y + endPoint.X - startPoint.Y);
					}
					else //dragging single item
					{
						MouseDownElement.Dragged(Point.Subtract(ScreenToGraph(e.Location), (Size)MouseDownElement.Location));
					}
					break;

				case DragOperation.Selection:
					Point graphPoint = ScreenToGraph(e.Location);
					SelectionZone = new Rectangle(Math.Min(SelectionZoneOriginPoint.X, graphPoint.X), Math.Min(SelectionZoneOriginPoint.Y, graphPoint.Y), Math.Abs(SelectionZoneOriginPoint.X - graphPoint.X), Math.Abs(SelectionZoneOriginPoint.Y - graphPoint.Y));
					CurrentSelectionNodes.Clear();
					foreach (GraphElement ge in Elements)
					{
						if (ge is NodeElement ne)
						{
							ne.Selected = false;

							if (ne.IntersectsWithZone(SelectionZone, -20, -20))
								CurrentSelectionNodes.Add(ne);
						}
					}
					UpdateSelection();
					break;
            }

			//dragging view (can happen during any drag operation)
			if (viewBeingDragged)
			{
				ViewOffset = new Point(ViewOffset.X + (int)((e.X - lastMouseDragPoint.X) / ViewScale), ViewOffset.Y + (int)((e.Y - lastMouseDragPoint.Y) / ViewScale));
				UpdateGraphBounds(MouseDownElement == null); //only hard limit the graph bounds if we arent dragging an object
				lastMouseDragPoint = e.Location;
			}

			Invalidate();
		}

		void ProductionGraphViewer_MouseWheel(object sender, MouseEventArgs e)
		{
			if (ContainsFocus && !this.Focused) //currently have a control created within this viewer active (ex: recipe chooser) -> dont want to scroll then
				return;

			ClearFloatingControls();
			Point oldZoomCenter = ScreenToGraph(e.Location);

			if (e.Delta > 0)
			{
				ViewScale *= 1.1f;
			}
			else
			{
				ViewScale /= 1.1f;
			}
			ViewScale = Math.Max(ViewScale, 0.01f);
			ViewScale = Math.Min(ViewScale, 5f);

			Point newZoomCenter = ScreenToGraph(e.Location);
			ViewOffset.Offset(newZoomCenter.X - oldZoomCenter.X, newZoomCenter.Y - oldZoomCenter.Y);

			UpdateGraphBounds();
			Invalidate();
		}

		void ProductionGraphViewer_MouseEnter(object sender, EventArgs e)
		{
			Invalidate();
		}

		void ProductionGraphViewer_KeyDown(object sender, KeyEventArgs e)
        {
			if(currentDragOperation == DragOperation.Selection) //possible changes to selection type
				UpdateSelection();
			Invalidate();
        }

		void ProductionGraphViewer_KeyUp(object sender, KeyEventArgs e)
		{
			if (currentDragOperation == DragOperation.Selection) //possible changes to selection type
				UpdateSelection();
			if (currentDragOperation == DragOperation.None)
			{
				switch (e.KeyCode)
				{
					case Keys.Delete:
						TryDeleteSelectedNodes();
						e.Handled = true;
						break;
				}
				Invalidate();
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			bool processed = false;
			int moveUnit = (CurrentGridUnit > 0) ? CurrentGridUnit : 6;
			if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) //large move
				moveUnit = (CurrentMajorGridUnit > CurrentGridUnit) ? CurrentMajorGridUnit : moveUnit * 4;

			if ((keyData & Keys.KeyCode) == Keys.Left)
			{
				Console.WriteLine((int)(keyData));
				Console.WriteLine((int)(keyData & Keys.Left));
				Console.WriteLine((int)(Keys.Left));

				foreach (NodeElement node in selectedNodes)
					node.Location = new Point(node.X - moveUnit, node.Y);
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Right)
			{
				foreach (NodeElement node in selectedNodes)
					node.Location = new Point(node.X + moveUnit, node.Y);
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Up)
			{
				foreach (NodeElement node in selectedNodes)
					node.Location = new Point(node.X, node.Y - moveUnit);
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Down)
			{
				foreach (NodeElement node in selectedNodes)
					node.Location = new Point(node.X, node.Y + moveUnit);
				processed = true;
			}

			if (processed)
				return true;
			return base.ProcessCmdKey(ref msg, keyData);
		}

		public void ClearFloatingControls()
		{
			foreach (var control in floatingTooltipControls.ToArray())
				control.Dispose();
		}

		//----------------------------------------------Viewpoint event functions

		void ProductionGraphViewer_Resized(object sender, EventArgs e)
        {
			UpdateGraphBounds();
			Invalidate();
        }

		private void ProductionGraphViewer_LostFocus(object sender, EventArgs e)
		{
			Invalidate();
		}

		public void UpdateGraphBounds(bool limitView = true)
		{
			if (limitView)
			{
				Rectangle bounds = Graph.Bounds;
				Point screenCentre = ScreenToGraph(Width / 2, Height / 2);
				if (bounds.Width == 0 || bounds.Height == 0)
				{
					ViewOffset.X = 0;
					ViewOffset.Y = 0;
				}
				else
				{
					if (screenCentre.X < bounds.X) { ViewOffset.X -= bounds.X - screenCentre.X; }
					if (screenCentre.Y < bounds.Y) { ViewOffset.Y -= bounds.Y - screenCentre.Y; }
					if (screenCentre.X > bounds.X + bounds.Width) { ViewOffset.X -= bounds.X + bounds.Width - screenCentre.X; }
					if (screenCentre.Y > bounds.Y + bounds.Height) { ViewOffset.Y -= bounds.Y + bounds.Height - screenCentre.Y; }
				}
			}

			visibleGraphBounds = new Rectangle(
				(int)(-Width / (2 * ViewScale) - ViewOffset.X),
				(int)(-Height / (2 * ViewScale) - ViewOffset.Y),
				(int)(Width / ViewScale),
				(int)(Height / ViewScale));
		}

		//----------------------------------------------Helper functions (point conversions, alignment, etc)

		public Point DesktopToGraph(Point point)
		{
			return ScreenToGraph(PointToClient(point));
		}

		public Point DesktopToGraph(int X, int Y)
		{
			return DesktopToGraph(new Point(X, Y));
		}

		public Point ScreenToGraph(Point point)
		{
			return ScreenToGraph(point.X, point.Y);
		}

		public Point ScreenToGraph(int X, int Y)
		{
			return new Point(Convert.ToInt32(((X - Width / 2) / ViewScale) - ViewOffset.X), Convert.ToInt32(((Y - Height / 2) / ViewScale) - ViewOffset.Y));
		}

		public Point GraphToScreen(Point point)
		{
			return GraphToScreen(point.X, point.Y);
		}

		public Point GraphToScreen(int X, int Y)
		{
			return new Point(Convert.ToInt32(((X + ViewOffset.X) * ViewScale) + Width / 2), Convert.ToInt32(((Y + ViewOffset.Y) * ViewScale) + Height / 2));
		}

		public int AlignToGrid(int original)
		{
			if (CurrentGridUnit < 1)
				return original;

			original += Math.Sign(original) * CurrentGridUnit / 2;
			original -= original % CurrentGridUnit;
			return original;
		}

		//----------------------------------------------Save/Load JSON functions

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//write
			info.AddValue("SavedPresetName", DCache.PresetName);
			info.AddValue("AmountType", SelectedAmountType);
			info.AddValue("Unit", SelectedRateUnit);
			info.AddValue("ViewOffset", ViewOffset);
			info.AddValue("ViewScale", ViewScale);

			info.AddValue("IncludedMods", DCache.IncludedMods.Select(m => m.Key + "|"+m.Value));

			info.AddValue("HiddenRecipes", DCache.Recipes.Values.Where(r => r.Hidden).Select(r => r.Name));
			info.AddValue("EnabledAssemblers", DCache.Assemblers.Values.Where(a => a.Enabled).Select(a => a.Name));
			info.AddValue("EnabledMiners", DCache.Miners.Values.Where(m => m.Enabled).Select(m => m.Name));
			info.AddValue("EnabledModules", DCache.Modules.Values.Where(m => m.Enabled).Select(m => m.Name));

			info.AddValue("ProductionGraph", Graph);
		}

		public void LoadFromOldJson(JObject json)
        {
			//need to convert it to the new format
			//then:
			//LoadFromJson(json);

			//... i will do this last - after I make sure I wont change the format of the 'new' json save file structure any more.
			// i mean - i already changed it 3 times to accomodate various things, and I havent even updated the assembler/modules for a given node yet!
        }

		public void LoadFromJson(JObject json, bool useFirstPreset, bool enableEverything = false)
		{
			//grab mod list
			Dictionary<string, string> modSet = new Dictionary<string, string>();
			foreach (string str in json["IncludedMods"].Select(t => (string)t).ToList())
            {
				string[] mod = str.Split('|');
				modSet.Add(mod[0], mod[1]);
            }

			//grab recipe list
			List<string> itemNames = json["ProductionGraph"]["IncludedItems"].Select(t => (string)t).ToList();
			List<RecipeShort> recipeShorts = RecipeShort.GetSetFromJson(json["ProductionGraph"]["IncludedRecipes"]);

			//now - two options:
			// a) we are told to use the first preset (basically, the selected preset) - so that is the only one added to the possible Presets
			// b) we can choose preset - so go through each one and compare mod lists - ask to continue if
			// the preset list will then be checked for compatibility based on recipes, and the one with least errors will be used.
			// any errors will prompt a message box saying that 'incompatibility was found, but proceeding anyways'.
			List<Preset> allPresets = MainForm.GetValidPresetsList();
			List<PresetErrorPackage> presetErrors = new List<PresetErrorPackage>();
			Preset chosenPreset = null;
			if (useFirstPreset)
				chosenPreset = allPresets[0];
			else
			{
				//test for the preset specified in the json save
				Preset savedWPreset = allPresets.FirstOrDefault(p => p.Name == (string)json["SavedPresetName"]);
				if (savedWPreset != null)
				{
					var errors = DataCache.TestPreset(savedWPreset, modSet, itemNames, recipeShorts);
					if (errors != null && errors.ErrorCount == 0) //no errors found here. We will then use this exact preset and not search for a different one
						chosenPreset = savedWPreset;
					else
					{
						//errors found. even though the name fits, but the preset seems to be the wrong one. Proceed with searching for best-fit
						presetErrors.Add(errors);
						allPresets.Remove(savedWPreset);
					}
				}

				//havent found the preset, or it returned some errors (not good) -> have to search for best fit (and leave the decision to user if we have multiple)
				if (chosenPreset == null)
				{
					foreach (Preset preset in allPresets)
					{
						PresetErrorPackage errors = DataCache.TestPreset(preset, modSet, itemNames, recipeShorts);
						if (errors != null)
							presetErrors.Add(errors);
					}

					//show the menu to select the preferred preset
					using (PresetSelectionForm form = new PresetSelectionForm(presetErrors))
					{
						form.StartPosition = FormStartPosition.Manual;
						form.Left = ParentForm.Left + 50;
						form.Top = ParentForm.Top + 50;

						if (form.ShowDialog() != DialogResult.OK || form.ChosenPreset == null) //null check is not necessary - if we get an ok dialogresult, we know it will be set
							return;
						chosenPreset = form.ChosenPreset;
						Properties.Settings.Default.CurrentPresetName = chosenPreset.Name;
						Properties.Settings.Default.Save();
					}
				}
				else if(chosenPreset.Name != Properties.Settings.Default.CurrentPresetName) //we had to switch the preset to a new one (without the user having to select a preset from a list)
					MessageBox.Show(string.Format("Loaded graph uses a different Preset.\nPreset switched from \"{0}\" to \"{1}\"", Properties.Settings.Default.CurrentPresetName, chosenPreset.Name));
			}

			//clear graph
			Elements.Clear();

			//load new preset
			using (DataLoadForm form = new DataLoadForm(chosenPreset))
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = this.Left + 150;
				form.Top = this.Top + 100;
				form.ShowDialog(); //LOAD FACTORIO DATA
				DCache = form.GetDataCache();
				Graph = new ProductionGraph();
				GC.Collect(); //loaded a new data cache - the old one should be collected (data caches can be over 1gb in size due to icons, plus whatever was in the old graph)
			}

			//set up graph options
			SelectedAmountType = (AmountType)(int)json["AmountType"];
			SelectedRateUnit = (RateUnit)(int)json["Unit"];

			string[] viewOffsetString = ((string)json["ViewOffset"]).Split(',');
			ViewOffset = new Point(int.Parse(viewOffsetString[0]), int.Parse(viewOffsetString[1]));
			ViewScale = (float)json["ViewScale"];

			if (!enableEverything)
			{
				//update enabled statuses
				foreach (Assembler assembler in DCache.Assemblers.Values)
					assembler.Enabled = false;
				foreach (string name in json["EnabledAssemblers"].Select(t => (string)t).ToList())
					if (DCache.Assemblers.ContainsKey(name))
						DCache.Assemblers[name].Enabled = true;

				foreach (Miner miner in DCache.Miners.Values)
					miner.Enabled = false;
				foreach (string name in json["EnabledMiners"].Select(t => (string)t).ToList())
					if (DCache.Miners.ContainsKey(name))
						DCache.Miners[name].Enabled = true;

				foreach (Module module in DCache.Modules.Values)
					module.Enabled = false;
				foreach (string name in json["EnabledModules"].Select(t => (string)t).ToList())
					if (DCache.Modules.ContainsKey(name))
						DCache.Modules[name].Enabled = true;

				foreach (string recipe in json["HiddenRecipes"].Select(t => (string)t).ToList())
					if (DCache.Recipes.ContainsKey(recipe))
						DCache.Recipes[recipe].Hidden = true;
			}

			//add all nodes
			ProductionGraph.NewNodeCollection newNodes = Graph.InsertNodesFromJson(DCache, json["ProductionGraph"], 1f);

			UpdateGraphBounds();
			Graph.UpdateNodeValues();
		}

		//Stolen from the designer file
		protected override void Dispose(bool disposing)
		{
			stringFormat.Dispose();
			size10Font.Dispose();

			foreach (var element in Elements.ToList())
			{
				element.Dispose();
			}


			if (disposing && (components != null))
			{
				components.Dispose();
			}
			rightClickMenu.Dispose();

			base.Dispose(disposing);
		}
	}
}