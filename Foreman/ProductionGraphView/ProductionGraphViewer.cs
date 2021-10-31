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
using System.Threading.Tasks;

namespace Foreman
{
	public enum NewNodeType { Disconnected, Supplier, Consumer }

	[Serializable]
	public partial class ProductionGraphViewer : UserControl, ISerializable
	{

		private enum DragOperation { None, Item, Selection }
		public enum LOD { Low, Medium, High } //low: only names. medium: assemblers, beacons, etc. high: include assembler percentages

		public LOD LevelOfDetail { get; set; }
		public int NodeCountForSimpleView { get; set; } //if the number of elements to draw is over this amount then the drawing functions will switch to simple view draws (mostly for FPS during zoomed out views)
		public bool ShowRecipeToolTip { get; set; }
		public bool TooltipsEnabled { get; set; }
		private bool SubwindowOpen; //used together with tooltip enabled -> if we open up an item/recipe/assembler window, this will halt tooltip show.
		public bool DynamicLinkWidth = false;
		public bool LockedRecipeEditPanelPosition = true;

		public DataCache DCache { get; set; }
		public ProductionGraph Graph { get; private set; }
		public Grid Grid { get; private set; }
		public FloatingTooltipRenderer ToolTipRenderer { get; private set; }

		public GraphElement MouseDownElement { get; set; }

		public IReadOnlyDictionary<BaseNode, BaseNodeElement> NodeElementDictionary { get { return nodeElementDictionary; } }
		public IReadOnlyDictionary<NodeLink, LinkElement> LinkElementDictionary { get { return linkElementDictionary; } }

		public IReadOnlyCollection<BaseNodeElement> SelectedNodes { get { return selectedNodes; } }

		private const int minDragDiff = 30;
		private const int minLinkWidth = 3;
		private const int maxLinkWidth = 35;

		private static readonly Pen pausedBorders = new Pen(Color.FromArgb(255, 80, 80), 5);
		private static readonly Pen selectionPen = new Pen(Color.FromArgb(100, 100, 200), 2);

		private Dictionary<BaseNode, BaseNodeElement> nodeElementDictionary;
		private List<BaseNodeElement> nodeElements;
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

		private HashSet<BaseNodeElement> selectedNodes; //main list of selected nodes
		private HashSet<BaseNodeElement> currentSelectionNodes; //list of nodes currently under the selection zone (which can be added/removed/replace the full list)

		private ContextMenu rightClickMenu = new ContextMenu();

		public ProductionGraphViewer()
		{
			InitializeComponent();
			MouseWheel += new MouseEventHandler(ProductionGraphViewer_MouseWheel);
			Resize += new EventHandler(ProductionGraphViewer_Resized);

			ViewOffset = new Point(Width / -2, Height / -2);
			ViewScale = 1f;
			NodeCountForSimpleView = 200;

			TooltipsEnabled = true;
			SubwindowOpen = false;

			Graph = new ProductionGraph();
			//Graph.ClearGraph()
			Graph.NodeAdded += Graph_NodeAdded;
			Graph.NodeDeleted += Graph_NodeDeleted;
			Graph.LinkAdded += Graph_LinkAdded;
			Graph.LinkDeleted += Graph_LinkDeleted;
			Graph.NodeValuesUpdated += Graph_NodeValuesUpdated;

			Grid = new Grid();

			ToolTipRenderer = new FloatingTooltipRenderer(this);

			nodeElementDictionary = new Dictionary<BaseNode, BaseNodeElement>();
			nodeElements = new List<BaseNodeElement>();
			linkElementDictionary = new Dictionary<NodeLink, LinkElement>();
			linkElements = new List<LinkElement>();

			selectedNodes = new HashSet<BaseNodeElement>();
			currentSelectionNodes = new HashSet<BaseNodeElement>();

			UpdateGraphBounds();
			Invalidate();
		}

		public void ClearGraph()
		{
			foreach (BaseNodeElement element in nodeElements)
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

		public BaseNodeElement GetNodeAtPoint(Point point) //returns first such node (in case of stacking)
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

		//----------------------------------------------Adding new node functions (including link dragging) + Node edit

		public void StartLinkDrag(BaseNodeElement startNode, LinkType linkType, Item item)
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
			SubwindowOpen = true;
			ItemChooserPanel itemChooser = new ItemChooserPanel(this, drawOrigin);
			itemChooser.ItemRequested += (o, itemRequestArgs) =>
			{
				AddRecipe(drawOrigin, itemRequestArgs.Item, newLocation, NewNodeType.Disconnected);
			};
			itemChooser.PanelClosed += (o, e) => { SubwindowOpen = false; };

			itemChooser.Show();
		}

		public void AddRecipe(Point drawOrigin, Item baseItem, Point newLocation, NewNodeType nNodeType, BaseNodeElement originElement = null, bool offsetLocationToItemTabLevel = false)
		{
			SubwindowOpen = true;
			if ((nNodeType != NewNodeType.Disconnected) && (originElement == null || baseItem == null))
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

			RecipeChooserPanel recipeChooser = new RecipeChooserPanel(this, drawOrigin, baseItem, tempRange, nNodeType);
			BaseNode newNode = null;
			int lastNodeWidth = 0;
			recipeChooser.RecipeRequested += (o, recipeRequestArgs) =>
			 {
				 switch (recipeRequestArgs.NodeType)
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
						 RecipeNode rNode = Graph.CreateRecipeNode(recipeRequestArgs.Recipe, newLocation);
						 newNode = rNode;
						 if ((nNodeType == NewNodeType.Consumer && !recipeRequestArgs.Recipe.IngredientSet.ContainsKey(baseItem)) || (nNodeType == NewNodeType.Supplier && !recipeRequestArgs.Recipe.ProductSet.ContainsKey(baseItem))) 
						 {
							 AssemblerSelector.Style style;
							 switch (Graph.AssemblerSelector.DefaultSelectionStyle)
							 {
								 case AssemblerSelector.Style.Best:
								 case AssemblerSelector.Style.BestBurner:
								 case AssemblerSelector.Style.BestNonBurner:
									 style = AssemblerSelector.Style.BestBurner;
									 break;
								 case AssemblerSelector.Style.Worst:
								 case AssemblerSelector.Style.WorstBurner:
								 case AssemblerSelector.Style.WorstNonBurner:
								 default:
									 style = AssemblerSelector.Style.WorstBurner;
									 break;
							 }
							 List<Assembler> assemblerOptions = Graph.AssemblerSelector.GetOrderedAssemblerList(recipeRequestArgs.Recipe, style);

							 if(nNodeType == NewNodeType.Consumer)
							 {
								 rNode.SetAssembler(assemblerOptions.First(a => a.Fuels.Contains(baseItem)));
								 rNode.SetFuel(baseItem);
							 }
							 else // if(nNodeType == NewNodeType.Supplier)
							 {
								 rNode.SetAssembler(assemblerOptions.First(a => a.Fuels.Contains(baseItem.FuelOrigin)));
								 rNode.SetFuel(baseItem.FuelOrigin);
							 }
						 }
						 break;
				 }

				 //this is the offset to take into account multiple recipe additions (holding shift while selecting recipe). First node isnt shifted, all subsequent ones are 'attempted' to be spaced.
				 //should be updated once the node graphics are updated (so that the node size doesnt depend as much on the text)
				 BaseNodeElement newNodeElement = NodeElementDictionary[newNode];
				 int offsetDistance = lastNodeWidth / 2;
				 lastNodeWidth = newNodeElement.Width; //effectively: this recipe width
				 if (offsetDistance > 0)
					 offsetDistance += (lastNodeWidth / 2) + 12;
				 if (offsetLocationToItemTabLevel)
					 newLocation = new Point(Grid.AlignToGrid(newLocation.X + offsetDistance), Grid.AlignToGrid(newLocation.Y + (nNodeType == NewNodeType.Consumer ? -newNodeElement.Height / 2 : nNodeType == NewNodeType.Supplier ? newNodeElement.Height / 2 : 0)));
				 else
					 newLocation = new Point(Grid.AlignToGrid(newLocation.X + offsetDistance), Grid.AlignToGrid(newLocation.Y));

				 newNode.Location = newLocation;
				 Invalidate();

				 if (nNodeType == NewNodeType.Consumer)
					 Graph.CreateLink(originElement.DisplayedNode, newNode, baseItem);
				 else if (nNodeType == NewNodeType.Supplier)
					 Graph.CreateLink(newNode, originElement.DisplayedNode, baseItem);

				 Graph.UpdateNodeStates();
				 Graph.UpdateNodeValues();
			 };

			recipeChooser.PanelClosed += (o, e) =>
			{
				DisposeLinkDrag();
				Invalidate();
				SubwindowOpen = false;
			};

			recipeChooser.Show();
		}

		public void TryDeleteSelectedNodes()
		{
			bool proceed = true;
			if (selectedNodes.Count > 10)
				proceed = (MessageBox.Show("You are deleting " + selectedNodes.Count + " nodes. \nAre you sure?", "Confirm delete.", MessageBoxButtons.YesNo) == DialogResult.Yes);
			if (proceed)
			{
				foreach (BaseNodeElement node in selectedNodes)
					Graph.DeleteNode(node.DisplayedNode);
				selectedNodes.Clear();
				Graph.UpdateNodeStates();
				Graph.UpdateNodeValues();
			}
		}

		public void EditNode(BaseNodeElement bNodeElement)
		{
			if (bNodeElement is RecipeNodeElement rNodeElement)
			{
				EditRecipeNode(rNodeElement);
				return;
			}

			SubwindowOpen = true;
			Control editPanel = new EditFlowPanel(bNodeElement.DisplayedNode, this);

			//offset view if necessary to ensure entire window will be seen (with 25 pixels boundary)
			Point screenOriginPoint = GraphToScreen(new Point(bNodeElement.X - (bNodeElement.Width / 2), bNodeElement.Y));
			screenOriginPoint = new Point(screenOriginPoint.X - editPanel.Width, screenOriginPoint.Y - (editPanel.Height / 2));
			Point offset = new Point(
				(int)(Math.Min(Math.Max(0, 25 - screenOriginPoint.X), this.Width - screenOriginPoint.X - editPanel.Width - bNodeElement.Width - 25)),
				(int)(Math.Min(Math.Max(0, 25 - screenOriginPoint.Y), this.Height - screenOriginPoint.Y - editPanel.Height - 25)));

			ViewOffset = Point.Add(ViewOffset, new Size((int)(offset.X / ViewScale), (int)(offset.Y / ViewScale)));
			UpdateGraphBounds();
			Invalidate();

			//open up the edit panel
			FloatingTooltipControl fttc = new FloatingTooltipControl(editPanel, Direction.Right, new Point(bNodeElement.X - (bNodeElement.Width / 2), bNodeElement.Y), this, true, false);
			fttc.Closing += (s, e) => { SubwindowOpen = false; bNodeElement.Update(); Graph.UpdateNodeValues(); };
		}

		public void EditRecipeNode(RecipeNodeElement rNodeElement)
		{
			SubwindowOpen = true;
			Control editPanel = new EditRecipePanel((RecipeNode)rNodeElement.DisplayedNode, this);
			RecipePanel recipePanel = new RecipePanel(new Recipe[] { ((RecipeNode)rNodeElement.DisplayedNode).BaseRecipe });

			if (LockedRecipeEditPanelPosition)
			{
				editPanel.Location = new Point(15, 15);
				recipePanel.Location = new Point(editPanel.Location.X + editPanel.Width + 5, editPanel.Location.Y);
			}
			else
			{
				//offset view if necessary to ensure entire window will be seen (with 25 pixels boundary). Additionally we want the tooltips to start 100 pixels above the arrow point instead of based on the center of the control (due to the dynamically changing height of the recipe option panel)
				Point recipeEditPanelOriginPoint = ToolTipRenderer.getTooltipScreenBounds(GraphToScreen(new Point(rNodeElement.X - (rNodeElement.Width / 2), rNodeElement.Y)), editPanel.Size, Direction.Right).Location;
				recipeEditPanelOriginPoint.Y += editPanel.Height / 2 - 125;
				recipeEditPanelOriginPoint.X -= recipePanel.Width + 5;
				Point offset = new Point(
					(int)(Math.Min(Math.Max(0, 25 - recipeEditPanelOriginPoint.X), this.Width - recipeEditPanelOriginPoint.X - editPanel.Width)),
					(int)(Math.Min(Math.Max(0, 25 - recipeEditPanelOriginPoint.Y), this.Height - recipeEditPanelOriginPoint.Y - editPanel.Height - 25)));

				editPanel.Location = Point.Add(recipeEditPanelOriginPoint, (Size)offset);
				recipePanel.Location = new Point(editPanel.Location.X + editPanel.Width + 5, editPanel.Location.Y);

				ViewOffset = Point.Add(ViewOffset, new Size((int)(offset.X / ViewScale), (int)(offset.Y / ViewScale)));
				UpdateGraphBounds(false);
				Invalidate();

			}

			//add the visible recipe to the right of the node
			new FloatingTooltipControl(recipePanel, Direction.Left, new Point(rNodeElement.X + (rNodeElement.Width / 2), rNodeElement.Y), this, true, true);
			FloatingTooltipControl fttc = new FloatingTooltipControl(editPanel, Direction.Right, new Point(rNodeElement.X - (rNodeElement.Width / 2), rNodeElement.Y), this, true, true);
			fttc.Closing += (s, e) => { SubwindowOpen = false; rNodeElement.Update(); Graph.UpdateNodeValues(); };
		}

		//----------------------------------------------Selection functions

		private void UpdateSelection()
		{
			foreach (BaseNodeElement element in nodeElements)
				element.Highlighted = false;

			if ((Control.ModifierKeys & Keys.Alt) != 0) //remove zone
			{
				foreach (BaseNodeElement selectedNode in selectedNodes)
					selectedNode.Highlighted = true;
				foreach (BaseNodeElement newlySelectedNode in currentSelectionNodes)
					newlySelectedNode.Highlighted = false;
			}
			else if ((Control.ModifierKeys & Keys.Control) != 0)  //add zone
			{
				foreach (BaseNodeElement selectedNode in selectedNodes)
					selectedNode.Highlighted = true;
				foreach (BaseNodeElement newlySelectedNode in currentSelectionNodes)
					newlySelectedNode.Highlighted = true;
			}
			else //add zone (additive with ctrl or simple selection)
			{
				foreach (BaseNodeElement newlySelectedNode in currentSelectionNodes)
					newlySelectedNode.Highlighted = true;
			}
		}

		private void ClearSelection()
		{
			foreach (BaseNodeElement element in nodeElements)
				element.Highlighted = false;
			selectedNodes.Clear();
			currentSelectionNodes.Clear();
		}

		public void AlignSelected()
		{
			foreach (BaseNodeElement ne in selectedNodes)
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
			foreach (BaseNodeElement element in nodeElements)
				yield return element;
		}

		public void UpdateNodeVisuals()
		{
			try
			{
				foreach (BaseNodeElement node in nodeElements)
					node.Update();
			}
			catch (OverflowException) { }//Same as when working out node values, there's not really much to do here... Maybe I could show a tooltip saying the numbers are too big or something...
			Invalidate();
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
			Grid.Paint(graphics, ViewScale, visibleGraphBounds, (currentDragOperation == DragOperation.Item) ? MouseDownElement as BaseNodeElement : null);

			//process link element widths
			if (DynamicLinkWidth)
			{
				double itemMax = 0;
				double fluidMax = 0;
				foreach (LinkElement element in linkElements)
				{
					if (element.Item.IsFluid)
						fluidMax = Math.Max(fluidMax, element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item));
					else if (element.Item.Name != "§§heat") //ignore heat as an item	
						itemMax = Math.Max(itemMax, element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item));
				}
				itemMax += itemMax == 0 ? 1 : 0;
				fluidMax += fluidMax == 0 ? 1 : 0;

				foreach (LinkElement element in linkElements)
				{
					if (element.Item.IsFluid)
						element.LinkWidth = (float)Math.Min((minLinkWidth + (maxLinkWidth - minLinkWidth) * (element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item) / fluidMax)), maxLinkWidth);
					else
						element.LinkWidth = (float)Math.Min((minLinkWidth + (maxLinkWidth - minLinkWidth) * (element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item) / itemMax)), maxLinkWidth);
				}
			}
			else
			{
				foreach (LinkElement element in linkElements)
					element.LinkWidth = minLinkWidth;
			}

			//all elements (nodes & lines)
			int visibleElements = GetPaintingOrder().Count(e => e.Visible && e is BaseNodeElement);
			foreach (GraphElement element in GetPaintingOrder())
				element.Paint(graphics, visibleElements > NodeCountForSimpleView || ViewScale < 0.2); //if viewscale is 0.2, then the text, images, etc being drawn are ~1/5th the size: aka: ~6x6 pixel images, etc. Use simple draw. Also simple draw if too many objects

			//selection zone
			if (currentDragOperation == DragOperation.Selection)
				graphics.DrawRectangle(selectionPen, SelectionZone);

			//everything below will be drawn directly on the screen instead of scaled/shifted based on graph
			graphics.ResetTransform();

			//floating tooltips
			ToolTipRenderer.Paint(graphics, TooltipsEnabled && !SubwindowOpen && currentDragOperation == DragOperation.None && !viewBeingDragged);

			//paused border
			if (Graph != null && Graph.PauseUpdates) //graph null check is purely for design view
				graphics.DrawRectangle(pausedBorders, 0, 0, Width - 3, Height - 3);
		}

		//----------------------------------------------Production Graph events

		private void Graph_NodeValuesUpdated(object sender, EventArgs e)
		{
			UpdateNodeVisuals();
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
			BaseNodeElement supplier = nodeElementDictionary[e.nodeLink.Supplier];
			BaseNodeElement consumer = nodeElementDictionary[e.nodeLink.Consumer];
			supplier.Update();
			consumer.Update();

			LinkElement element = new LinkElement(this, e.nodeLink, supplier, consumer);
			linkElementDictionary.Add(e.nodeLink, element);
			linkElements.Add(element);
			Invalidate();
		}

		private void Graph_NodeDeleted(object sender, NodeEventArgs e)
		{
			BaseNodeElement element = nodeElementDictionary[e.node];
			nodeElementDictionary.Remove(e.node);
			nodeElements.Remove(element);
			element.Dispose();
			Invalidate();
		}

		private void Graph_NodeAdded(object sender, NodeEventArgs e)
		{
			BaseNodeElement element = null;
			if (e.node is SupplierNode)
				element = new SupplierNodeElement(this, e.node);
			else if (e.node is ConsumerNode)
				element = new ConsumerNodeElement(this, e.node);
			else if (e.node is PassthroughNode)
				element = new PassthroughNodeElement(this, e.node);
			else if (e.node is RecipeNode)
				element = new RecipeNodeElement(this, e.node);
			else
				Trace.Fail("Unexpected node type created in graph.");

			nodeElementDictionary.Add(e.node, element);
			nodeElements.Add(element);
			Invalidate();
		}

		//----------------------------------------------Mouse events

		private void ProductionGraphViewer_MouseDown(object sender, MouseEventArgs e)
		{
			ToolTipRenderer.ClearFloatingControls();
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
					foreach (BaseNodeElement ne in selectedNodes)
						ne.Highlighted = false;
					selectedNodes.Clear();
				}
			}
		}

		private void ProductionGraphViewer_MouseUp(object sender, MouseEventArgs e)
		{
			ToolTipRenderer.ClearFloatingControls();
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
							foreach (BaseNodeElement newlySelectedNode in currentSelectionNodes)
								selectedNodes.Remove(newlySelectedNode);
						}
						else
						{
							if ((Control.ModifierKeys & Keys.Control) == 0) //if we arent using control, then we are just selecting
								selectedNodes.Clear();

							foreach (BaseNodeElement newlySelectedNode in currentSelectionNodes)
								selectedNodes.Add(newlySelectedNode);
						}
						currentSelectionNodes.Clear();
					}
					//this is a release of a left click (non-drag operation) -> modify selection if clicking on node & using modifier keys
					else if (currentDragOperation == DragOperation.None && MouseDownElement is BaseNodeElement clickedNode)
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
							foreach (BaseNodeElement node in selectedNodes.Where(node => node != MouseDownElement))
								node.Location = new Point(node.X + endPoint.X - startPoint.X, node.Y + endPoint.Y - startPoint.Y);

					}
					else //dragging single item
					{
						MouseDownElement.Dragged(graph_location);
					}

					//accept middle mouse button for view dragging purposes (while dragging item or selection)
					if ((Control.MouseButtons & MouseButtons.Middle) == MouseButtons.Middle)
						viewBeingDragged = true;
					break;

				case DragOperation.Selection:
					SelectionZone = new Rectangle(Math.Min(SelectionZoneOriginPoint.X, graph_location.X), Math.Min(SelectionZoneOriginPoint.Y, graph_location.Y), Math.Abs(SelectionZoneOriginPoint.X - graph_location.X), Math.Abs(SelectionZoneOriginPoint.Y - graph_location.Y));
					currentSelectionNodes.Clear();
					foreach (BaseNodeElement element in nodeElements.Where(element => element.IntersectsWithZone(SelectionZone, -20, -20)))
						currentSelectionNodes.Add(element);

					UpdateSelection();

					//accept middle mouse button for view dragging purposes (while dragging item or selection)
					if ((Control.MouseButtons & MouseButtons.Middle) == MouseButtons.Middle)
						viewBeingDragged = true;
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

			ToolTipRenderer.ClearFloatingControls();

			Point oldZoomCenter = ScreenToGraph(e.Location);

			if (e.Delta > 0)
				ViewScale *= 1.1f;
			else
				ViewScale /= 1.1f;

			ViewScale = Math.Max(ViewScale, 0.01f);
			ViewScale = Math.Min(ViewScale, 2f);

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

					Graph.SerializeNodeIdSet = new HashSet<int>();
					foreach (BaseNodeElement selectedNode in selectedNodes)
						Graph.SerializeNodeIdSet.Add(selectedNode.ID);
					serialiser.Serialize(writer, Graph);
					Graph.SerializeNodeIdSet.Clear();
					Graph.SerializeNodeIdSet = null;

					Clipboard.SetText(stringBuilder.ToString());

					if (e.KeyCode == Keys.X) //cut
						foreach (BaseNodeElement node in selectedNodes)
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
					foreach (ReadOnlyBaseNode newNode in newNodeCollection.newNodes)
					{
						xAve += newNode.Location.X;
						yAve += newNode.Location.Y;
					}
					xAve /= newNodeCollection.newNodes.Count;
					yAve /= newNodeCollection.newNodes.Count;


					Point importCenter = new Point((int)xAve, (int)yAve);
					Point mousePos = ScreenToGraph(PointToClient(Cursor.Position));
					Size offset = (Size)Grid.AlignToGrid(Point.Subtract(mousePos, (Size)importCenter));
					foreach (ReadOnlyBaseNode newNode in newNodeCollection.newNodes)
						Graph.RequestNodeController(newNode).Location = Point.Add(newNode.Location, offset);

					//update the selection to be just the newly imported nodes
					ClearSelection();
					foreach (BaseNodeElement newNodeElement in newNodeCollection.newNodes.Select(node => nodeElementDictionary[node]))
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
				Grid.DragOrigin = Grid.AlignToGrid(MouseDownElement?.Location ?? new Point());
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
				Grid.DragOrigin = Grid.AlignToGrid(MouseDownElement?.Location ?? new Point());
				if (currentDragOperation == DragOperation.Item)
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
				foreach (BaseNodeElement node in selectedNodes)
					node.Location = new Point(node.X - moveUnit, node.Y);
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Right)
			{
				foreach (BaseNodeElement node in selectedNodes)
					node.Location = new Point(node.X + moveUnit, node.Y);
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Up)
			{
				foreach (BaseNodeElement node in selectedNodes)
					node.Location = new Point(node.X, node.Y - moveUnit);
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Down)
			{
				foreach (BaseNodeElement node in selectedNodes)
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

		private void ProductionGraphViewer_Resize(object sender, EventArgs e)
		{
			ToolTipRenderer?.ClearFloatingControls(); //resize can happen before tooltip is created (due to scaling)
		}

		private void ProductionGraphViewer_Leave(object sender, EventArgs e)
		{
			ToolTipRenderer.ClearFloatingControls();
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
			info.AddValue("Unit", Graph.SelectedRateUnit);
			info.AddValue("ViewOffset", ViewOffset);
			info.AddValue("ViewScale", ViewScale);

			//graph defaults (saved here instead of within the graph since they are used here, plus they arent used during copy/paste)
			info.AddValue("AssemblerSelectorStyle", Graph.AssemblerSelector.DefaultSelectionStyle);
			info.AddValue("ModuleSelectorStyle", Graph.ModuleSelector.DefaultSelectionStyle);
			info.AddValue("FuelPriorityList", Graph.FuelSelector.FuelPriority.Select(i => i.Name));

			//enabled lists
			info.AddValue("EnabledRecipes", DCache.Recipes.Values.Where(r => r.Enabled).Select(r => r.Name));
			info.AddValue("EnabledAssemblers", DCache.Assemblers.Values.Where(a => a.Enabled).Select(a => a.Name));
			info.AddValue("EnabledModules", DCache.Modules.Values.Where(m => m.Enabled).Select(m => m.Name));
			info.AddValue("EnabledBeacons", DCache.Beacons.Values.Where(b => b.Enabled).Select(b => b.Name));

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

		public async Task LoadFromJson(JObject json, bool useFirstPreset, bool setEnablesFromJson)
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
			List<string> assemblerNames = json["ProductionGraph"]["IncludedAssemblers"].Select(t => (string)t).ToList();
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
					var errors = await PresetProcessor.TestPreset(savedWPreset, modSet, itemNames, assemblerNames, recipeShorts);
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
						PresetErrorPackage errors = await PresetProcessor.TestPreset(preset, modSet, itemNames, assemblerNames, recipeShorts);
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
				form.Left = ParentForm.Left + 150;
				form.Top = ParentForm.Top + 200;
				form.ShowDialog(); //LOAD FACTORIO DATA
				DCache = form.GetDataCache();
				GC.Collect(); //loaded a new data cache - the old one should be collected (data caches can be over 1gb in size due to icons, plus whatever was in the old graph)
			}

			//set up graph options
			Graph.SelectedRateUnit = (ProductionGraph.RateUnit)(int)json["Unit"];
			Graph.AssemblerSelector.DefaultSelectionStyle = (AssemblerSelector.Style)(int)json["AssemblerSelectorStyle"];
			Graph.ModuleSelector.DefaultSelectionStyle = (ModuleSelector.Style)(int)json["ModuleSelectorStyle"];
			foreach (string fuelType in json["FuelPriorityList"].Select(t => (string)t))
				if (DCache.Items.ContainsKey(fuelType))
					Graph.FuelSelector.UseFuel(DCache.Items[fuelType]);

			//set up graph view options
			string[] viewOffsetString = ((string)json["ViewOffset"]).Split(',');
			ViewOffset = new Point(int.Parse(viewOffsetString[0]), int.Parse(viewOffsetString[1]));
			ViewScale = (float)json["ViewScale"];

			//update enabled statuses
			if (setEnablesFromJson)
			{
				foreach (Beacon beacon in DCache.Beacons.Values)
					beacon.Enabled = false;
				foreach (string beacon in json["EnabledBeacons"].Select(t => (string)t).ToList())
					if (DCache.Beacons.ContainsKey(beacon))
						DCache.Beacons[beacon].Enabled = true;

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


			if (disposing && (components != null))
			{
				components.Dispose();
			}

			rightClickMenu.Dispose();

			base.Dispose(disposing);
		}
	}
}