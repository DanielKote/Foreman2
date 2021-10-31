using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace Foreman
{

	[Serializable]
	public partial class ProductionGraphViewer : UserControl, ISerializable
	{
		public enum RateUnit { Per1Sec, Per1Min, Per5Min, Per10Min, Per30Min, Per1Hour };//, Per6Hour, Per12Hour, Per24Hour }
		public static readonly string[] RateUnitNames = new string[] { "1 sec", "1 min", "5 min", "10 min", "30 min", "1 hour" }; //, "6 hours", "12 hours", "24 hours" };
		private static readonly float[] RateMultiplier = new float[] { 1 / 1, 1 / 60, 1 / 300, 1 / 600, 1 / 1800, 1 / 3600 }; //, 1/21600, 1/43200, 1/86400 };
		public static float GetRateMultipler(RateUnit ru) { return RateMultiplier[(int)ru]; } //the amount of assemblers required will be multipled by the rate multipler when displaying.

		private enum DragOperation { None, Item, Selection }
		public enum NewNodeType { Disconnected, Supplier, Consumer }

		public RateUnit SelectedRateUnit { get; set; }

		public bool SimpleView { get; set; } //simple: show only the item/recipe names.
		public bool TooltipsEnabled { get; set; }
		public bool DynamicLinkWidth = false;

		public DataCache DCache { get; set; }
		public ProductionGraph Graph { get; private set; }
		public Grid Grid { get; private set; }

		public GraphElement MouseDownElement { get; set; }

		public IReadOnlyDictionary<BaseNode, NodeElement> NodeElementDictionary { get { return nodeElementDictionary; } }
		public IReadOnlyDictionary<NodeLink, LinkElement> LinkElementDictionary { get { return linkElementDictionary; } }

		public IReadOnlyCollection<NodeElement> SelectedNodes { get { return selectedNodes; } }

		private const int minDragDiff = 30;
		private const int minLinkWidth = 3;
		private const int maxLinkWidth = 35;

		private static readonly Pen pausedBorders = new Pen(Color.FromArgb(255, 80, 80), 5);
		private static readonly Pen selectionPen = new Pen(Color.FromArgb(100, 100, 200), 2);
		private readonly Font size10Font = new Font(FontFamily.GenericSansSerif, 10);

		private Dictionary<BaseNode, NodeElement> nodeElementDictionary;
		private List<NodeElement> nodeElements;
		private Dictionary<NodeLink, LinkElement> linkElementDictionary;
		private List<LinkElement> linkElements;
		private DraggedLinkElement draggedLinkElement;

		private Point mouseDownStartScreenPoint;

		private Point ViewDragOriginPoint;
		private bool viewBeingDragged = false; //separate from dragOperation due to being able to drag view at all stages of dragOperation

		private DragOperation currentDragOperation = DragOperation.None;

		private Point ViewOffset;
		private float ViewScale = 1f;
		private Rectangle visibleGraphBounds;

		private Rectangle SelectionZone;
		private Point SelectionZoneOriginPoint;

		private HashSet<NodeElement> selectedNodes; //main list of selected nodes
		private HashSet<NodeElement> currentSelectionNodes; //list of nodes currently under the selection zone (which can be added/removed/replace the full list)

		private ContextMenu rightClickMenu = new ContextMenu();
		public HashSet<FloatingTooltipControl> floatingTooltipControls = new HashSet<FloatingTooltipControl>();

		private StringFormat stringFormat = new StringFormat(); //used for tooltip drawing so as not to create a new one each time

		public ProductionGraphViewer()
		{
			InitializeComponent();
			MouseWheel += new MouseEventHandler(ProductionGraphViewer_MouseWheel);
			Resize += new EventHandler(ProductionGraphViewer_Resized);

			ViewOffset = new Point(Width / -2, Height / -2);
			ViewScale = 1f;

			TooltipsEnabled = true;

			Graph = new ProductionGraph();
			//Graph.ClearGraph()
			Graph.NodeAdded += Graph_NodeAdded;
			Graph.NodeDeleted += Graph_NodeDeleted;
			Graph.LinkAdded += Graph_LinkAdded;
			Graph.LinkDeleted += Graph_LinkDeleted;
			Graph.NodeValuesUpdated += Graph_NodeValuesUpdated;

			Grid = new Grid();

			nodeElementDictionary = new Dictionary<BaseNode, NodeElement>();
			nodeElements = new List<NodeElement>();
			linkElementDictionary = new Dictionary<NodeLink, LinkElement>();
			linkElements = new List<LinkElement>();

			selectedNodes = new HashSet<NodeElement>();
			currentSelectionNodes = new HashSet<NodeElement>();

			UpdateGraphBounds();
			Invalidate();
		}

		public void ClearGraph()
		{
			foreach (NodeElement element in nodeElements)
				element.Dispose();
			foreach (LinkElement element in linkElements)
				element.Dispose();
			DisposeLinkDrag();
			nodeElements.Clear();
			nodeElementDictionary.Clear();
			linkElements.Clear();
			linkElementDictionary.Clear();
			selectedNodes.Clear();
			currentSelectionNodes.Clear();
			Graph.ClearGraph();
		}

		public NodeElement GetNodeAtPoint(Point point) //returns first such node (in case of stacking)
		{
			//done in a 2 stage process -> first we do a rough check on the node's location (if it is within the 500x300 zone centered on the given point, it goes to part 2)
			//							-> then we do a full element.containsPoint check

			Rectangle initialCheckZone = new Rectangle(point.X - 250, point.Y - 150, 500, 300);
			for (int i = nodeElements.Count - 1; i >= 0; i--)
			{
				if (initialCheckZone.Contains(nodeElements[i].Location))
					if (nodeElements[i].ContainsPoint(point))
						return nodeElements[i];
			}
			return null;
		}

		//----------------------------------------------Adding new node functions (including link dragging)

		public void StartLinkDrag(NodeElement startNode, LinkType linkType, Item item)
		{
			draggedLinkElement?.Dispose();
			draggedLinkElement = new DraggedLinkElement(this, startNode, linkType, item);
			MouseDownElement = draggedLinkElement;
		}

		public void DisposeLinkDrag()
		{
			draggedLinkElement?.Dispose();
			draggedLinkElement = null;
		}

		public void AddItem(Point drawOrigin, Point newLocation)
		{
			ItemChooserPanel itemChooser = new ItemChooserPanel(this, drawOrigin);
			itemChooser.Show(selectedItem =>
			{
				if (selectedItem != null)
					AddRecipe(drawOrigin, selectedItem, newLocation, NewNodeType.Disconnected);
			});
		}

		public void AddRecipe(Point drawOrigin, Item baseItem, Point newLocation, NewNodeType nNodeType, NodeElement originElement = null)
		{
			if (nNodeType != NewNodeType.Disconnected && (originElement == null || baseItem == null)) //just in case check (should
				Trace.Fail("Origin element or base item not provided for a new (linked) node");

			if (Grid.ShowGrid)
				newLocation = Grid.AlignToGrid(newLocation);

			fRange tempRange = new fRange(0, 0, true);
			if (baseItem != null && baseItem.IsTemperatureDependent)
			{
				if (nNodeType == NewNodeType.Consumer) //need to check all nodes down to recipes for range of temperatures being produced
					tempRange = LinkChecker.GetTemperatureRange(baseItem, originElement.DisplayedNode, LinkType.Output);
				else if (nNodeType == NewNodeType.Supplier) //need to check all nodes up to recipes for range of temperatures being consumed (guaranteed to be in a SINGLE [] range)
					tempRange = LinkChecker.GetTemperatureRange(baseItem, originElement.DisplayedNode, LinkType.Input);
			}

			RecipeChooserPanel recipeChooser = new RecipeChooserPanel(this, drawOrigin, baseItem, tempRange, nNodeType != NewNodeType.Consumer, nNodeType != NewNodeType.Supplier);
			BaseNode newNode = null;
			int lastRecipeWidth = 0;
			recipeChooser.Show((nodeType, recipe) =>
			{
				switch (nodeType)
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
						newNode = Graph.CreateRecipeNode(recipe, newLocation, true);
						break;
				}

				//this is the offset to take into account multiple recipe additions (holding shift while selecting recipe). First node isnt shifted, all subsequent ones are 'attempted' to be spaced.
				//should be updated once the node graphics are updated (so that the node size doesnt depend as much on the text)
				int offsetDistance = lastRecipeWidth / 2;
				lastRecipeWidth = 50 + Math.Max(newNode.Inputs.Count(), newNode.Outputs.Count()) * (ItemTabElement.TabWidth + ItemTabElement.TabBorder);
				if (offsetDistance > 0)
					offsetDistance += lastRecipeWidth / 2;
				newLocation = new Point(Grid.AlignToGrid(newLocation.X + offsetDistance), Grid.AlignToGrid(newLocation.Y));
				newNode.Location = newLocation;
				Invalidate();

				if (nNodeType == NewNodeType.Consumer)
					Graph.CreateLink(originElement.DisplayedNode, newNode, baseItem);
				else if (nNodeType == NewNodeType.Supplier)
					Graph.CreateLink(newNode, originElement.DisplayedNode, baseItem);

				Graph.UpdateNodeValues();
			}, () =>
			{
				DisposeLinkDrag();
				Invalidate();
			});
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

		//----------------------------------------------Selection functions

		private void UpdateSelection()
		{
			foreach (NodeElement element in nodeElements)
				element.Highlighted = false;

			if ((Control.ModifierKeys & Keys.Alt) != 0) //remove zone
			{
				foreach (NodeElement selectedNode in selectedNodes)
					selectedNode.Highlighted = true;
				foreach (NodeElement newlySelectedNode in currentSelectionNodes)
					newlySelectedNode.Highlighted = false;
			}
			else if ((Control.ModifierKeys & Keys.Control) != 0)  //add zone
			{
				foreach (NodeElement selectedNode in selectedNodes)
					selectedNode.Highlighted = true;
				foreach (NodeElement newlySelectedNode in currentSelectionNodes)
					newlySelectedNode.Highlighted = true;
			}
			else //add zone (additive with ctrl or simple selection)
			{
				foreach (NodeElement newlySelectedNode in currentSelectionNodes)
					newlySelectedNode.Highlighted = true;
			}
		}

		private void ClearSelection()
		{
			foreach (NodeElement element in nodeElements)
				element.Highlighted = false;
			selectedNodes.Clear();
			currentSelectionNodes.Clear();
		}

		public void AlignSelected()
		{
			foreach (NodeElement ne in selectedNodes)
				ne.Location = Grid.AlignToGrid(ne.Location);
			Invalidate();
		}

		//----------------------------------------------Paint functions

		protected IEnumerable<GraphElement> GetPaintingOrder()
		{
			if (draggedLinkElement != null)
				yield return draggedLinkElement;
			foreach (LinkElement element in linkElements)
				yield return element;
			foreach (NodeElement element in nodeElements)
				yield return element;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//update visibility of all elements
			foreach (GraphElement element in GetPaintingOrder())
				element.UpdateVisibility(visibleGraphBounds);

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
			selectionPen.Width = 2 / ViewScale;

			//grid
			Grid.Paint(graphics, ViewScale, visibleGraphBounds, (currentDragOperation == DragOperation.Item) ? MouseDownElement as NodeElement : null);

			//process link element widths
			if (DynamicLinkWidth)
			{
				float itemMax = 0;
				float fluidMax = 0;
				foreach (LinkElement element in linkElements)
				{
					if (element.Item.IsFluid)
						fluidMax = Math.Max(fluidMax, element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item));
					else
						itemMax = Math.Max(itemMax, element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item));
				}
				itemMax += itemMax == 0 ? 1 : 0;
				fluidMax += fluidMax == 0 ? 1 : 0;

				foreach (LinkElement element in linkElements)
				{
					if (element.Item.IsFluid)
						element.LinkWidth = minLinkWidth + (maxLinkWidth - minLinkWidth) * (element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item) / fluidMax);
					else
						element.LinkWidth = minLinkWidth + (maxLinkWidth - minLinkWidth) * (element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item) / itemMax);
				}
			}
			else
			{
				foreach (LinkElement element in linkElements)
					element.LinkWidth = minLinkWidth;
			}

			//all elements (nodes & lines)
			foreach (GraphElement element in GetPaintingOrder())
				element.Paint(graphics);

			//selection zone
			if (currentDragOperation == DragOperation.Selection)
				graphics.DrawRectangle(selectionPen, SelectionZone);

			//everything below will be drawn directly on the screen instead of scaled/shifted based on graph
			graphics.ResetTransform();

			//floating tooltips
			if (TooltipsEnabled && currentDragOperation == DragOperation.None && !viewBeingDragged)
			{
				foreach (var fttp in floatingTooltipControls)
					DrawTooltip(GraphToScreen(fttp.GraphLocation), fttp.Control.Size, fttp.Direction, graphics, null);

				NodeElement element = GetNodeAtPoint(ScreenToGraph(PointToClient(Control.MousePosition)));
				if (element != null)
				{
					foreach (TooltipInfo tti in element.GetToolTips(ScreenToGraph(PointToClient(Control.MousePosition))))
						DrawTooltip(tti.ScreenLocation, tti.ScreenSize, tti.Direction, graphics, tti.Text);
				}
			}

			//paused border
			if (Graph != null && Graph.PauseUpdates) //graph null check is purely for design view
				graphics.DrawRectangle(pausedBorders, 0, 0, Width - 3, Height - 3);
		}

		private void DrawTooltip(Point screenArrowPoint, Size size, Direction direction, Graphics graphics, String text = null)
		{
			if (text != null)
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

		public void ClearFloatingControls()
		{
			foreach (var control in floatingTooltipControls.ToArray())
				control.Dispose();
		}

		//----------------------------------------------Production Graph events

		private void Graph_NodeValuesUpdated(object sender, EventArgs e)
		{
			try
			{
				foreach (NodeElement node in nodeElements)
					node.Update();
			}
			catch (OverflowException) { }//Same as when working out node values, there's not really much to do here... Maybe I could show a tooltip saying the numbers are too big or something...
			Invalidate();
		}

		private void Graph_LinkDeleted(object sender, NodeLinkEventArgs e)
		{
			LinkElement element = linkElementDictionary[e.nodeLink];
			linkElementDictionary.Remove(e.nodeLink);
			linkElements.Remove(element);
			element.Dispose();
			Invalidate();
		}

		private void Graph_LinkAdded(object sender, NodeLinkEventArgs e)
		{
			NodeElement supplier = nodeElementDictionary[e.nodeLink.Supplier];
			NodeElement consumer = nodeElementDictionary[e.nodeLink.Consumer];
			LinkElement element = new LinkElement(this, e.nodeLink, supplier, consumer);
			linkElementDictionary.Add(e.nodeLink, element);
			linkElements.Add(element);
			Invalidate();
		}

		private void Graph_NodeDeleted(object sender, NodeEventArgs e)
		{
			NodeElement element = nodeElementDictionary[e.node];
			nodeElementDictionary.Remove(e.node);
			nodeElements.Remove(element);
			element.Dispose();
			Invalidate();
		}

		private void Graph_NodeAdded(object sender, NodeEventArgs e)
		{
			NodeElement element = new NodeElement(this, e.node);
			nodeElementDictionary.Add(e.node, element);
			nodeElements.Add(element);
			Invalidate();
		}

		//----------------------------------------------Mouse events

		private void ProductionGraphViewer_MouseDown(object sender, MouseEventArgs e)
		{
			ClearFloatingControls();
			ActiveControl = null; //helps panels like IRChooserPanel (for item/recipe choosing) close when we click on the graph

			mouseDownStartScreenPoint = Control.MousePosition;
			Point graph_location = ScreenToGraph(e.Location);

			GraphElement clickedElement = (GraphElement)draggedLinkElement ?? GetNodeAtPoint(ScreenToGraph(e.Location));
			clickedElement?.MouseDown(graph_location, e.Button);

			if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Right && clickedElement == null)) //scrolling - middle button always, right if not clicking on element
			{
				ViewDragOriginPoint = graph_location;
			}
			else if (e.Button == MouseButtons.Left && clickedElement == null) //selection
			{
				SelectionZoneOriginPoint = graph_location;
				SelectionZone = new Rectangle();
				if ((Control.ModifierKeys & Keys.Control) == 0 && (Control.ModifierKeys & Keys.Alt) == 0) //clear all selected nodes if we arent using modifier keys
				{
					foreach (NodeElement ne in selectedNodes)
						ne.Highlighted = false;
					selectedNodes.Clear();
				}
			}
		}

		private void ProductionGraphViewer_MouseUp(object sender, MouseEventArgs e)
		{
			ClearFloatingControls();
			Point graph_location = ScreenToGraph(e.Location);
			GraphElement element = null;

			if (!viewBeingDragged && currentDragOperation != DragOperation.Selection) //dont care about mouse up operations on elements if we were dragging view or selection
			{
				element = (GraphElement)draggedLinkElement ?? GetNodeAtPoint(graph_location);
				element?.MouseUp(graph_location, e.Button, (currentDragOperation == DragOperation.Item));
			}

			switch (e.Button)
			{
				case MouseButtons.Right:
					if (viewBeingDragged)
						viewBeingDragged = false;
					else if (currentDragOperation == DragOperation.None && element == null) //right click on an empty space -> show add item/recipe menu
					{
						Point screenPoint = new Point(e.Location.X - 150, 15);
						screenPoint.X = Math.Max(15, Math.Min(Width - 650, screenPoint.X)); //want to position the recipe selector such that it is well visible.

						rightClickMenu.MenuItems.Clear();
						rightClickMenu.MenuItems.Add(new MenuItem("Add Item",
							new EventHandler((o, ee) =>
							{
								AddItem(screenPoint, ScreenToGraph(e.Location));
							})));
						rightClickMenu.MenuItems.Add(new MenuItem("Add Recipe",
							new EventHandler((o, ee) =>
							{
								AddRecipe(screenPoint, null, ScreenToGraph(e.Location), NewNodeType.Disconnected);
							})));
						rightClickMenu.Show(this, e.Location);
					}
					break;
				case MouseButtons.Middle:
					viewBeingDragged = false;
					break;
				case MouseButtons.Left:
					//finished selecting the given zone (process selected nodes)
					if (currentDragOperation == DragOperation.Selection)
					{
						if ((Control.ModifierKeys & Keys.Alt) != 0) //removal zone processing
						{
							foreach (NodeElement newlySelectedNode in currentSelectionNodes)
								selectedNodes.Remove(newlySelectedNode);
						}
						else
						{
							if ((Control.ModifierKeys & Keys.Control) == 0) //if we arent using control, then we are just selecting
								selectedNodes.Clear();

							foreach (NodeElement newlySelectedNode in currentSelectionNodes)
								selectedNodes.Add(newlySelectedNode);
						}
						currentSelectionNodes.Clear();
					}
					//this is a release of a left click (non-drag operation) -> modify selection if clicking on node & using modifier keys
					else if (currentDragOperation == DragOperation.None && MouseDownElement is NodeElement clickedNode)
					{
						if ((Control.ModifierKeys & Keys.Alt) != 0) //remove
						{
							selectedNodes.Remove(clickedNode);
							clickedNode.Highlighted = false;
							MouseDownElement = null;
							Invalidate();
						}
						else if ((Control.ModifierKeys & Keys.Control) != 0) //add if unselected, remove if selected
						{
							if (clickedNode.Highlighted)
								selectedNodes.Remove(clickedNode);
							else
								selectedNodes.Add(clickedNode);

							clickedNode.Highlighted = !clickedNode.Highlighted;
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
			Point graph_location = ScreenToGraph(e.Location);

			if (currentDragOperation != DragOperation.Selection) //dont care about element mouse move operations during selection operation
			{
				GraphElement element = draggedLinkElement ?? MouseDownElement;
				element?.MouseMoved(graph_location);
			}

			switch (currentDragOperation)
			{
				case DragOperation.None: //check for minimal distance to be considered a drag operation
					Point dragDiff = Point.Subtract(Control.MousePosition, (Size)mouseDownStartScreenPoint);
					if (dragDiff.X * dragDiff.X + dragDiff.Y * dragDiff.Y > minDragDiff)
					{
						if ((Control.MouseButtons & MouseButtons.Middle) == MouseButtons.Middle || (Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right)
							viewBeingDragged = true;

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
						MouseDownElement.Dragged(graph_location);
						Point endPoint = MouseDownElement.Location;
						if (startPoint != endPoint)
							foreach (NodeElement node in selectedNodes.Where(node => node != MouseDownElement))
								node.Location = new Point(node.X + endPoint.X - startPoint.X, node.Y + endPoint.Y - startPoint.Y);
					}
					else //dragging single item
					{
						MouseDownElement.Dragged(graph_location);
					}
					break;

				case DragOperation.Selection:
					SelectionZone = new Rectangle(Math.Min(SelectionZoneOriginPoint.X, graph_location.X), Math.Min(SelectionZoneOriginPoint.Y, graph_location.Y), Math.Abs(SelectionZoneOriginPoint.X - graph_location.X), Math.Abs(SelectionZoneOriginPoint.Y - graph_location.Y));
					currentSelectionNodes.Clear();
					foreach (NodeElement element in nodeElements.Where(element => element.IntersectsWithZone(SelectionZone, -20, -20)))
						currentSelectionNodes.Add(element);

					UpdateSelection();
					break;
			}

			//dragging view (can happen during any drag operation)
			if (viewBeingDragged)
			{
				ViewOffset = Point.Add(ViewOffset, (Size)Point.Subtract(graph_location, (Size)ViewDragOriginPoint));// new Point(ViewOffset.X + (int)((graph_location.X - lastMouseDragPoint.X) / ViewScale), ViewOffset.Y + (int)((graph_location.Y - lastMouseDragPoint.Y) / ViewScale));
				UpdateGraphBounds(MouseDownElement == null); //only hard limit the graph bounds if we arent dragging an object
			}

			Invalidate();
		}

		private void ProductionGraphViewer_MouseWheel(object sender, MouseEventArgs e)
		{
			if (ContainsFocus && !this.Focused) //currently have a control created within this viewer active (ex: recipe chooser) -> dont want to scroll then
				return;

			ClearFloatingControls();

			Point oldZoomCenter = ScreenToGraph(e.Location);

			if (e.Delta > 0)
				ViewScale *= 1.1f;
			else
				ViewScale /= 1.1f;

			ViewScale = Math.Max(ViewScale, 0.01f);
			ViewScale = Math.Min(ViewScale, 5f);

			Point newZoomCenter = ScreenToGraph(e.Location);
			ViewOffset.Offset(newZoomCenter.X - oldZoomCenter.X, newZoomCenter.Y - oldZoomCenter.Y);

			UpdateGraphBounds();
			Invalidate();
		}

		private void ProductionGraphViewer_KeyDown(object sender, KeyEventArgs e)
		{
			if (currentDragOperation == DragOperation.None)
			{
				if ((e.KeyCode == Keys.C || e.KeyCode == Keys.X) && (e.Modifiers & Keys.Control) == Keys.Control) //copy or cut
				{
					StringBuilder stringBuilder = new StringBuilder();
					JsonSerializer serialiser = JsonSerializer.Create();
					serialiser.Formatting = Formatting.None;
					var writer = new JsonTextWriter(new StringWriter(stringBuilder));

					Graph.SerializeNodeList = new List<BaseNode>();
					Graph.SerializeNodeList.AddRange(selectedNodes.Select(node => node.DisplayedNode)); //set the list of nodes we will serialize
					serialiser.Serialize(writer, Graph);
					Graph.SerializeNodeList.Clear();
					Graph.SerializeNodeList = null;

					Clipboard.SetText(stringBuilder.ToString());

					if (e.KeyCode == Keys.X) //cut
						foreach (NodeElement node in selectedNodes)
							Graph.DeleteNode(node.DisplayedNode);
				}
				else if (e.KeyCode == Keys.V && (e.Modifiers & Keys.Control) == Keys.Control) //paste
				{
					ProductionGraph.NewNodeCollection newNodeCollection = null;
					try
					{
						JObject json = JObject.Parse(Clipboard.GetText());
						newNodeCollection = Graph.InsertNodesFromJson(DCache, json); //NOTE: missing items & recipes may be added here!
					}
					catch { Console.WriteLine("Non-Foreman paste detected."); } //clipboard string wasnt a proper json object, or didnt process properly. Likely answer: was a clip NOT from foreman.
					if (newNodeCollection == null || newNodeCollection.newNodes.Count == 0)
						return;

					//update the locations of the new nodes to be centered around the mouse position (as opposed to wherever they were before)
					long xAve = 0;
					long yAve = 0;
					foreach (BaseNode newNode in newNodeCollection.newNodes)
					{
						xAve += newNode.Location.X;
						yAve += newNode.Location.Y;
					}
					xAve /= newNodeCollection.newNodes.Count;
					yAve /= newNodeCollection.newNodes.Count;


					Point importCenter = new Point((int)xAve, (int)yAve);
					Point mousePos = ScreenToGraph(PointToClient(Cursor.Position));
					Size offset = (Size)Grid.AlignToGrid(Point.Subtract(mousePos, (Size)importCenter));
					foreach (BaseNode newNode in newNodeCollection.newNodes)
						newNode.Location = Point.Add(newNode.Location, offset);

					//update the selection to be just the newly imported nodes
					ClearSelection();
					foreach (NodeElement newNodeElement in newNodeCollection.newNodes.Select(node => nodeElementDictionary[node]))
					{
						selectedNodes.Add(newNodeElement);
						newNodeElement.Highlighted = true;
					}

					UpdateGraphBounds();
					Graph.UpdateNodeValues();
				}
			}
			else if (currentDragOperation == DragOperation.Selection) //possible changes to selection type
				UpdateSelection();

			bool lockDragAxis = (Control.ModifierKeys & Keys.Shift) != 0;
			if (Grid.LockDragToAxis != lockDragAxis)
			{
				Grid.LockDragToAxis = lockDragAxis;
				Grid.DragOrigin = MouseDownElement?.Location ?? new Point();
				if (currentDragOperation == DragOperation.Item)
					MouseDownElement?.Dragged(ScreenToGraph(PointToClient(Control.MousePosition)));
			}
			Invalidate();
		}

		private void ProductionGraphViewer_KeyUp(object sender, KeyEventArgs e)
		{
			if (currentDragOperation == DragOperation.None)
			{
				switch (e.KeyCode)
				{
					case Keys.Delete:
						TryDeleteSelectedNodes();
						e.Handled = true;
						break;
				}
			}
			else if (currentDragOperation == DragOperation.Selection) //possible changes to selection type
				UpdateSelection();

			bool lockDragAxis = (Control.ModifierKeys & Keys.Shift) != 0;
			if (Grid.LockDragToAxis != lockDragAxis)
			{
				Grid.LockDragToAxis = lockDragAxis;
				Grid.DragOrigin = MouseDownElement?.Location ?? new Point();
				if(currentDragOperation == DragOperation.Item)
					MouseDownElement?.Dragged(ScreenToGraph(PointToClient(Control.MousePosition)));
			}
			Invalidate();
		}

		//----------------------------------------------Keyboard events

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) //arrow keys to move the current selection
		{
			bool processed = false;
			int moveUnit = (Grid.CurrentGridUnit > 0) ? Grid.CurrentGridUnit : 6;
			if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) //large move
				moveUnit = (Grid.CurrentMajorGridUnit > Grid.CurrentGridUnit) ? Grid.CurrentMajorGridUnit : moveUnit * 4;

			if ((keyData & Keys.KeyCode) == Keys.Left)
			{
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
			{
				Invalidate();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}



		//----------------------------------------------Viewpoint events

		private void ProductionGraphViewer_Resized(object sender, EventArgs e)
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
				Point screenCentre = ScreenToGraph(new Point(Width / 2, Height / 2));
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

		public Point ScreenToGraph(Point point)
		{
			return new Point(Convert.ToInt32(((point.X - Width / 2) / ViewScale) - ViewOffset.X), Convert.ToInt32(((point.Y - Height / 2) / ViewScale) - ViewOffset.Y));
		}

		public Point GraphToScreen(Point point)
		{
			return new Point(Convert.ToInt32(((point.X + ViewOffset.X) * ViewScale) + Width / 2), Convert.ToInt32(((point.Y + ViewOffset.Y) * ViewScale) + Height / 2));
		}

		//----------------------------------------------Save/Load JSON functions

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//preset options
			info.AddValue("SavedPresetName", DCache.PresetName);
			info.AddValue("IncludedMods", DCache.IncludedMods.Select(m => m.Key + "|" + m.Value));

			//graph viewer options
			info.AddValue("Unit", SelectedRateUnit);
			info.AddValue("ViewOffset", ViewOffset);
			info.AddValue("ViewScale", ViewScale);

			//graph defaults (saved here instead of within the graph since they are used here, plus they arent used during copy/paste)
			info.AddValue("AssemblerSelectorStyle", Graph.AssemblerSelector.SelectionStyle);
			info.AddValue("ModuleSelectorStyle", Graph.ModuleSelector.SelectionStyle);
			info.AddValue("FuelPriorityList", Graph.FuelSelector.FuelPriority.Select(i => i.Name));

			//enabled lists
			info.AddValue("EnabledRecipes", DCache.Recipes.Values.Where(r => r.Enabled).Select(r => r.Name));
			info.AddValue("EnabledAssemblers", DCache.Assemblers.Values.Where(a => a.Enabled).Select(a => a.Name));
			info.AddValue("EnabledModules", DCache.Modules.Values.Where(m => m.Enabled).Select(m => m.Name));

			//graph :)
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
				else if (chosenPreset.Name != Properties.Settings.Default.CurrentPresetName) //we had to switch the preset to a new one (without the user having to select a preset from a list)
				{
					MessageBox.Show(string.Format("Loaded graph uses a different Preset.\nPreset switched from \"{0}\" to \"{1}\"", Properties.Settings.Default.CurrentPresetName, chosenPreset.Name));
					Properties.Settings.Default.CurrentPresetName = chosenPreset.Name;
					Properties.Settings.Default.Save();
				}
			}

			//clear graph
			ClearGraph();

			//load new preset
			using (DataLoadForm form = new DataLoadForm(chosenPreset))
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = this.Left + 150;
				form.Top = this.Top + 100;
				form.ShowDialog(); //LOAD FACTORIO DATA
				DCache = form.GetDataCache();
				GC.Collect(); //loaded a new data cache - the old one should be collected (data caches can be over 1gb in size due to icons, plus whatever was in the old graph)
			}

			//set up graph options
			SelectedRateUnit = (RateUnit)(int)json["Unit"];
			Graph.AssemblerSelector.SelectionStyle = (AssemblerSelector.Style)(int)json["AssemblerSelectorStyle"];
			Graph.ModuleSelector.SelectionStyle = (ModuleSelector.Style)(int)json["ModuleSelectorStyle"];
			foreach (string fuelType in json["FuelPriorityList"].Select(t => (string)t))
				if (DCache.Items.ContainsKey(fuelType))
					Graph.FuelSelector.UseFuel(DCache.Items[fuelType]);

			//set up graph view options
			string[] viewOffsetString = ((string)json["ViewOffset"]).Split(',');
			ViewOffset = new Point(int.Parse(viewOffsetString[0]), int.Parse(viewOffsetString[1]));
			ViewScale = (float)json["ViewScale"];

			
			//update enabled statuses
			if (!enableEverything)
			{
				foreach (Assembler assembler in DCache.Assemblers.Values)
					assembler.Enabled = false;
				foreach (string name in json["EnabledAssemblers"].Select(t => (string)t).ToList())
					if (DCache.Assemblers.ContainsKey(name))
						DCache.Assemblers[name].Enabled = true;

				foreach (Module module in DCache.Modules.Values)
					module.Enabled = false;
				foreach (string name in json["EnabledModules"].Select(t => (string)t).ToList())
					if (DCache.Modules.ContainsKey(name))
						DCache.Modules[name].Enabled = true;

				foreach (Recipe recipe in DCache.Recipes.Values)
					recipe.Enabled = false;
				foreach (string recipe in json["EnabledRecipes"].Select(t => (string)t).ToList())
					if (DCache.Recipes.ContainsKey(recipe))
						DCache.Recipes[recipe].Enabled = true;
			}

			//add all nodes
			Graph.InsertNodesFromJson(DCache, json["ProductionGraph"]);

			//upgrade graph & values
			UpdateGraphBounds();
			Graph.UpdateNodeValues();
		}

		//Stolen from the designer file
		protected override void Dispose(bool disposing)
		{
			ClearGraph();

			stringFormat.Dispose();
			size10Font.Dispose();

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			rightClickMenu.Dispose();

			base.Dispose(disposing);
		}
	}
}