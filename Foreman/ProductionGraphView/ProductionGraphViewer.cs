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
	public enum NewNodeType { Disconnected, Supplier, Consumer, Label}
	public enum NodeDrawingStyle { Regular, PrintStyle, Simple, IconsOnly } //printstyle is meant for any additional chages (from regular) for exporting to image format, simple will only draw the node boxes (no icons or text) and link lines, iconsonly will draw node icons instead of nodes (for zoomed view)

	[Serializable]
	public partial class ProductionGraphViewer : UserControl, ISerializable
	{
		private enum DragOperation { None, Item, Selection }
		public enum LOD { Low, Medium, High } //low: only names. medium: assemblers, beacons, etc. high: include assembler percentages

		public LOD LevelOfDetail { get; set; }
		public bool ArrowsOnLinks { get; set; }
		public bool IconsOnly { get; set; }
		public int IconsSize { get; set; }
		public int IconsDrawSize { get { return ViewScale > ((double)IconsSize / 96)? 96 : (int)(IconsSize / ViewScale); } }

		public int NodeCountForSimpleView { get; set; } //if the number of elements to draw is over this amount then the drawing functions will switch to simple view draws (mostly for FPS during zoomed out views)
		public bool ShowRecipeToolTip { get; set; }
		public bool TooltipsEnabled { get; set; }
		private bool SubwindowOpen; //used together with tooltip enabled -> if we open up an item/recipe/assembler window, this will halt tooltip show.
		public bool DynamicLinkWidth = false;
		public bool LockedRecipeEditPanelPosition = true;
		public bool FlagOUSuppliedNodes = false; //if true, will add a flag for over or under supplied nodes

		public bool SmartNodeDirection { get; set; }

		public DataCache DCache { get; set; }
		public ProductionGraph Graph { get; private set; }
		public GridManager Grid { get; private set; }
		public FloatingTooltipRenderer ToolTipRenderer { get; private set; }
		public PointingArrowRenderer ArrowRenderer { get; private set; }

		public GraphElement MouseDownElement { get; set; }

		public IReadOnlyDictionary<ReadOnlyBaseNode, BaseNodeElement> NodeElementDictionary { get { return nodeElementDictionary; } }
		public IReadOnlyDictionary<ReadOnlyNodeLink, LinkElement> LinkElementDictionary { get { return linkElementDictionary; } }

		public IReadOnlyCollection<BaseNodeElement> SelectedNodes { get { return selectedNodes; } }

		public Point ViewOffset { get; private set; }
		public float ViewScale { get; private set; }
		public Rectangle VisibleGraphBounds { get; private set; }

		//scroll keys
		public Keys KeyUpCode;
		public Keys KeyDownCode;
		public Keys KeyLeftCode;
		public Keys KeyRightCode;
		public decimal KeyScrollRatio = 10;

		private const int minDragDiff = 30;
		private const int minLinkWidth = 3;
		private const int maxLinkWidth = 35;

		private static readonly Pen pausedBorders = new Pen(Color.FromArgb(255, 80, 80), 5);
		private static readonly Pen selectionPen = new Pen(Color.FromArgb(100, 100, 200), 2);

		private Dictionary<ReadOnlyBaseNode, BaseNodeElement> nodeElementDictionary;
		private List<BaseNodeElement> nodeElements;
		private Dictionary<ReadOnlyNodeLink, LinkElement> linkElementDictionary;
		private List<LinkElement> linkElements;
		private DraggedLinkElement draggedLinkElement;

		private Point mouseDownStartScreenPoint;
		private MouseButtons downButtons; //we use this to ensure that any mouse operations only count if they started on this panel

		private Point ViewDragOriginPoint;
		private bool viewBeingDragged = false; //separate from dragOperation due to being able to drag view at all stages of dragOperation

		private DragOperation currentDragOperation = DragOperation.None;

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

			IconsOnly = false;
			IconsSize = 32;

			TooltipsEnabled = true;
			SubwindowOpen = false;

			Graph = new ProductionGraph();
			//Graph.ClearGraph()
			Graph.NodeAdded += Graph_NodeAdded;
			Graph.NodeDeleted += Graph_NodeDeleted;
			Graph.LinkAdded += Graph_LinkAdded;
			Graph.LinkDeleted += Graph_LinkDeleted;
			Graph.NodeValuesUpdated += Graph_NodeValuesUpdated;

			Grid = new GridManager();
			ToolTipRenderer = new FloatingTooltipRenderer(this);
			ArrowRenderer = new PointingArrowRenderer(this);

			nodeElementDictionary = new Dictionary<ReadOnlyBaseNode, BaseNodeElement>();
			nodeElements = new List<BaseNodeElement>();
			linkElementDictionary = new Dictionary<ReadOnlyNodeLink, LinkElement>();
			linkElements = new List<LinkElement>();

			selectedNodes = new HashSet<BaseNodeElement>();
			currentSelectionNodes = new HashSet<BaseNodeElement>();

			UpdateGraphBounds();
			Invalidate();
		}

		public void ClearGraph()
		{
			DisposeLinkDrag();
			Graph.ClearGraph();
			//at this point every node element and link element has been removed.

			selectedNodes.Clear();
			currentSelectionNodes.Clear();
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

		public void AddLabel(Point drawOrigin, Point newLocation)
		{
			AddRecipe(drawOrigin, null, newLocation, NewNodeType.Label);
		}

		public void AddItem(Point drawOrigin, Point newLocation)
		{
			if (string.IsNullOrEmpty(DCache.PresetName))
			{
				MessageBox.Show("The current preset (" + Properties.Settings.Default.CurrentPresetName + ") is corrupt.");
				return;
			}

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
			if(string.IsNullOrEmpty(DCache.PresetName))
			{
				DisposeLinkDrag();
				MessageBox.Show("The current preset (" + Properties.Settings.Default.CurrentPresetName + ") is corrupt.");
				return;
			}

			//MR: if ((nNodeType != NewNodeType.Disconnected) && (originElement == null || baseItem == null))
			//MR:	Trace.Fail("Origin element or base item not provided for a new (linked) node");
			
			if (Grid.ShowGrid)
				newLocation = Grid.AlignToGrid(newLocation);

			int lastNodeWidth = 0;
			NodeDirection newNodeDirection = (originElement == null || !SmartNodeDirection) ? Graph.DefaultNodeDirection :
				draggedLinkElement.Type != BaseLinkElement.LineType.UShape ? originElement.DisplayedNode.NodeDirection :
				originElement.DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up;

			void ProcessNodeRequest(object o, RecipeRequestArgs recipeRequestArgs)
			{
				ReadOnlyBaseNode newNode = null;
				switch (recipeRequestArgs.NodeType)
				{
					case NodeType.Label:
						newNode = Graph.CreateLabelNode("Empty label", newLocation, 50);
						break;
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
						ReadOnlyRecipeNode rNode = Graph.CreateRecipeNode(recipeRequestArgs.Recipe, newLocation);
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

							RecipeNodeController controller = (RecipeNodeController)Graph.RequestNodeController(rNode);
							if (nNodeType == NewNodeType.Consumer)
							{
								controller.SetAssembler(assemblerOptions.First(a => a.Fuels.Contains(baseItem)));
								controller.SetFuel(baseItem);
							}
							else // if(nNodeType == NewNodeType.Supplier)
							{
								controller.SetAssembler(assemblerOptions.First(a => a.Fuels.Contains(baseItem.FuelOrigin)));
								controller.SetFuel(baseItem.FuelOrigin);
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
				{
					offsetDistance += (lastNodeWidth / 2);
					int newOffsetDistance = Grid.AlignToGrid(offsetDistance);
					if (newOffsetDistance < offsetDistance)
						newOffsetDistance += Grid.CurrentGridUnit;
					offsetDistance = newOffsetDistance;
				}
				newLocation = new Point(newLocation.X + offsetDistance, newLocation.Y);

				int yoffset = offsetLocationToItemTabLevel ? (nNodeType == NewNodeType.Consumer ? -newNodeElement.Height / 2 : nNodeType == NewNodeType.Supplier ? newNodeElement.Height / 2 : 0) : 0;
				yoffset *= newNodeDirection == NodeDirection.Up ? 1 : -1;
				Graph.RequestNodeController(newNode).SetLocation(new Point(newLocation.X, newLocation.Y + yoffset));

				if (originElement != null)
					Graph.RequestNodeController(newNode).SetDirection(newNodeDirection);

				if (nNodeType == NewNodeType.Consumer)
					Graph.CreateLink(originElement.DisplayedNode, newNode, baseItem);
				else if (nNodeType == NewNodeType.Supplier)
					Graph.CreateLink(newNode, originElement.DisplayedNode, baseItem);

				DisposeLinkDrag();
				Graph.UpdateNodeValues();
			}

			if ((Control.ModifierKeys & Keys.Control) == Keys.Control) //control key pressed -> we are making a passthrough node.
			{
				ProcessNodeRequest(null, new RecipeRequestArgs(NodeType.Passthrough, null));
				DisposeLinkDrag();
				Graph.UpdateNodeStates();
				Invalidate();
			}
			else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
			{
				if (nNodeType == NewNodeType.Consumer)
				{
					ProcessNodeRequest(null, new RecipeRequestArgs(NodeType.Consumer, null));
				}
				else
				{
					ProcessNodeRequest(null, new RecipeRequestArgs(NodeType.Supplier, null));
				}
			}
			else if (nNodeType == NewNodeType.Label)
			{ 
				ProcessNodeRequest(null, new RecipeRequestArgs(NodeType.Label, null));
			}
			else
			{
				fRange tempRange = new fRange(0, 0, true);
				if (baseItem != null && baseItem is Fluid fluid && fluid.IsTemperatureDependent)
				{
					if (nNodeType == NewNodeType.Consumer) //need to check all nodes down to recipes for range of temperatures being produced
						tempRange = LinkChecker.GetTemperatureRange(fluid, originElement.DisplayedNode, LinkType.Output, true);
					else if (nNodeType == NewNodeType.Supplier) //need to check all nodes up to recipes for range of temperatures being consumed (guaranteed to be in a SINGLE [] range)
						tempRange = LinkChecker.GetTemperatureRange(fluid, originElement.DisplayedNode, LinkType.Input, true);
				}

				RecipeChooserPanel recipeChooser = new RecipeChooserPanel(this, drawOrigin, baseItem, tempRange, nNodeType);
				recipeChooser.RecipeRequested += ProcessNodeRequest;
				recipeChooser.PanelClosed += (o, e) =>
				{
					SubwindowOpen = false;
					DisposeLinkDrag();
					Graph.UpdateNodeStates();
					Invalidate();
				};

				SubwindowOpen = true;
				recipeChooser.Show();
			}
		}

		public void AddPassthroughNodesFromSelection(LinkType linkType, Size offset)
		{
			List<BaseNodeElement> newPassthroughNodes = new List<BaseNodeElement>();
			foreach(PassthroughNodeElement passthroughNode in selectedNodes)
			{
				NodeDirection newNodeDirection = !SmartNodeDirection ? Graph.DefaultNodeDirection :
					draggedLinkElement.Type != BaseLinkElement.LineType.UShape ? passthroughNode.DisplayedNode.NodeDirection :
					passthroughNode.DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up;

				Item passthroughItem = ((ReadOnlyPassthroughNode)passthroughNode.DisplayedNode).PassthroughItem;

				int yoffset = linkType == LinkType.Input ? passthroughNode.Height / 2 : -passthroughNode.Height / 2;
				yoffset *= newNodeDirection == NodeDirection.Up ? 1 : -1;
				yoffset += offset.Height;

				ReadOnlyPassthroughNode newNode = Graph.CreatePassthroughNode(passthroughItem, new Point(passthroughNode.Location.X + offset.Width, passthroughNode.Location.Y + yoffset));
				PassthroughNodeController controller = (PassthroughNodeController)Graph.RequestNodeController(newNode);
				controller.SetDirection(newNodeDirection);

				if (linkType == LinkType.Input)
					Graph.CreateLink(newNode, passthroughNode.DisplayedNode, passthroughItem );
				else
					Graph.CreateLink(passthroughNode.DisplayedNode, newNode, passthroughItem );

				newPassthroughNodes.Add(nodeElementDictionary[newNode]);
			}
			SetSelection(newPassthroughNodes);

			DisposeLinkDrag();
			Graph.UpdateNodeStates();
			Invalidate();
		}

		public void TryDeleteSelectedNodes()
		{
			bool proceed = true;
			if (selectedNodes.Count > 10)
				proceed = (MessageBox.Show("You are deleting " + selectedNodes.Count + " nodes. \nAre you sure?", "Confirm delete.", MessageBoxButtons.YesNo) == DialogResult.Yes);
			if (proceed)
			{
				foreach (BaseNodeElement node in selectedNodes.ToList())
					Graph.DeleteNode(node.DisplayedNode);
				selectedNodes.Clear();
				Graph.UpdateNodeValues();
			}
		}

		public void FlipSelectedNodes()
		{
			foreach (BaseNodeElement node in selectedNodes.ToList())
				Graph.RequestNodeController(node.DisplayedNode).SetDirection(node.DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
			Invalidate();
		}

		public void SetSelectedPassthroughNodesSimpleDraw(bool simpleDraw)
		{
			foreach (PassthroughNodeElement node in selectedNodes.Where(n => n is PassthroughNodeElement).ToList())
				((PassthroughNodeController)Graph.RequestNodeController(node.DisplayedNode)).SetSimpleDraw(simpleDraw);
			Invalidate();
		}

		public void EditNode(BaseNodeElement bNodeElement)
		{
			if (bNodeElement is RecipeNodeElement rNodeElement)
			{
				EditRecipeNode(rNodeElement);
				return;
			}

			if (bNodeElement is LabelNodeElement labelNodeElement)
            {
				EditLabelNode(labelNodeElement);
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
			fttc.Closing += (s, e) =>
			{
				SubwindowOpen = false;
				//bNodeElement.Update();
				Graph.UpdateNodeValues();
			};
		}

		public void EditLabelNode(LabelNodeElement rNodeElement)
		{
			LabelNodeController nc = (LabelNodeController)Graph.RequestNodeController(rNodeElement.DisplayedNode);

			SubwindowOpen = true;
			ReadOnlyLabelNode rNode = (ReadOnlyLabelNode)rNodeElement.DisplayedNode;
			EditLabel editLabel = new EditLabel(rNode, this);
			editLabel.Location = new Point(15, 15);

			new FloatingTooltipControl(editLabel, Direction.Left, new Point(rNodeElement.X + (rNodeElement.Width / 2), rNodeElement.Y), this, true, true);
			FloatingTooltipControl fttc = new FloatingTooltipControl(editLabel, Direction.Right, new Point(rNodeElement.X - (rNodeElement.Width / 2), rNodeElement.Y), this, true, true);
			fttc.Closing += (s, e) => { SubwindowOpen = false; rNodeElement.RequestStateUpdate(); Graph.UpdateNodeValues(); };

		}

		public void EditRecipeNode(RecipeNodeElement rNodeElement)
		{
			SubwindowOpen = true;
			ReadOnlyRecipeNode rNode = (ReadOnlyRecipeNode)rNodeElement.DisplayedNode;
			Control editPanel = new EditRecipePanel(rNode, this);
			RecipePanel recipePanel = new RecipePanel(new Recipe[] { rNode.BaseRecipe });

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
			fttc.Closing += (s, e) => { SubwindowOpen = false; rNodeElement.RequestStateUpdate(); Graph.UpdateNodeValues(); };
		}

		//----------------------------------------------Selection functions

		private void SetSelection(IEnumerable<BaseNodeElement> newSelection)
		{
			foreach (BaseNodeElement element in selectedNodes)
				element.Highlighted = false;

			selectedNodes.Clear();
			selectedNodes.UnionWith(newSelection);

			foreach (BaseNodeElement element in selectedNodes)
				element.Highlighted = true;
		}

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

		public void ClearSelection()
		{
			foreach (BaseNodeElement element in nodeElements)
				element.Highlighted = false;
			selectedNodes.Clear();
			currentSelectionNodes.Clear();
			Invalidate();
		}

		public void AlignSelected()
		{
			foreach (BaseNodeElement ne in selectedNodes)
				ne.SetLocation(Grid.AlignToGrid(ne.Location));
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
					node.RequestStateUpdate();
			}
			catch (OverflowException) { }//Same as when working out node values, there's not really much to do here... Maybe I could show a tooltip saying the numbers are too big or something...
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.ResetTransform();
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			e.Graphics.Clear(this.BackColor);
			e.Graphics.TranslateTransform(Width / 2, Height / 2);
			e.Graphics.ScaleTransform(ViewScale, ViewScale);
			e.Graphics.TranslateTransform(ViewOffset.X, ViewOffset.Y);

			Paint(e.Graphics, false);
		}

		public new void Paint(Graphics graphics, bool FullGraph = false)
		{
			//update visibility of all elements
			if (FullGraph)
				foreach (GraphElement element in GetPaintingOrder())
					element.UpdateVisibility(Graph.Bounds);
			else
				foreach (GraphElement element in GetPaintingOrder())
					element.UpdateVisibility(VisibleGraphBounds);

			//ensure width of selection is correct
			selectionPen.Width = 2 / ViewScale;

			//grid
			if(!FullGraph)
				Grid.Paint(graphics, ViewScale, VisibleGraphBounds, (currentDragOperation == DragOperation.Item) ? MouseDownElement as BaseNodeElement : null);

			//process link element widths
			if (DynamicLinkWidth)
			{
				double itemMax = 0;
				double fluidMax = 0;
				foreach (LinkElement element in linkElements)
				{
					if (element.Item is Fluid && !element.Item.Name.StartsWith("§§")) //§§ is the foreman added special items (currently just §§heat). ignore them
						fluidMax = Math.Max(fluidMax, element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item));
					else
						itemMax = Math.Max(itemMax, element.ConsumerElement.DisplayedNode.GetConsumeRate(element.Item));
				}
				itemMax += itemMax == 0 ? 1 : 0;
				fluidMax += fluidMax == 0 ? 1 : 0;

				foreach (LinkElement element in linkElements)
				{
					if (element.Item is Fluid)
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

			//run any pre-paint functions
			foreach (GraphElement elemnent in GetPaintingOrder())
				elemnent.PrePaint();

			//paint all elements (nodes & lines)
			int visibleElements = GetPaintingOrder().Count(e => e.Visible && e is BaseNodeElement);
			foreach (GraphElement element in GetPaintingOrder())
				element.Paint(graphics, FullGraph? NodeDrawingStyle.PrintStyle : IconsOnly? NodeDrawingStyle.IconsOnly : (visibleElements > NodeCountForSimpleView || ViewScale < 0.2)? NodeDrawingStyle.Simple : NodeDrawingStyle.Regular); //if viewscale is 0.2, then the text, images, etc being drawn are ~1/5th the size: aka: ~6x6 pixel images, etc. Use simple draw. Also simple draw if too many objects

			//selection zone
			if (currentDragOperation == DragOperation.Selection && !FullGraph)
			{
				graphics.DrawRectangle(selectionPen, SelectionZone);
				double pConsumption = currentSelectionNodes.Where(n => n.DisplayedNode is ReadOnlyRecipeNode).Sum(n => ((ReadOnlyRecipeNode)n.DisplayedNode).GetTotalAssemblerElectricalConsumption() + ((ReadOnlyRecipeNode)n.DisplayedNode).GetTotalBeaconElectricalConsumption());
				double pProduction = currentSelectionNodes.Where(n => n.DisplayedNode is ReadOnlyRecipeNode).Sum(n => ((ReadOnlyRecipeNode)n.DisplayedNode).GetTotalGeneratorElectricalProduction());
				int recipeNodeCount = currentSelectionNodes.Count(n => n.DisplayedNode is ReadOnlyRecipeNode);
				int buildingCount = (int)Math.Ceiling(currentSelectionNodes.Where(n => n.DisplayedNode is ReadOnlyRecipeNode).Sum(n => ((ReadOnlyRecipeNode)n.DisplayedNode).ActualAssemblerCount));
				int beaconCount = currentSelectionNodes.Where(n => n.DisplayedNode is ReadOnlyRecipeNode).Sum(n => ((ReadOnlyRecipeNode)n.DisplayedNode).GetTotalBeacons());

				ToolTipRenderer.AddExtraToolTip(new TooltipInfo() { Text = string.Format("Power consumption: {0}\nPower production: {1}\nRecipe count: {2}\nBuilding count: {3}\nBeacon count: {4}", GraphicsStuff.DoubleToEnergy(pConsumption, "W"), GraphicsStuff.DoubleToEnergy(pProduction, "W"), recipeNodeCount, buildingCount, beaconCount), Direction = Direction.None, ScreenLocation = new Point(10, 10) });
			}

			//everything below will be drawn directly on the screen instead of scaled/shifted based on graph
			graphics.ResetTransform();

			if (!FullGraph)
			{
				//warning/error arrows
				ArrowRenderer.Paint(graphics, Graph);

				//floating tooltips
				ToolTipRenderer.Paint(graphics, TooltipsEnabled && !SubwindowOpen && currentDragOperation == DragOperation.None && !viewBeingDragged);
				ToolTipRenderer.ClearExtraToolTips();

				//paused border
				if (Graph != null && Graph.PauseUpdates) //graph null check is purely for design view
					graphics.DrawRectangle(pausedBorders, 0, 0, Width - 3, Height - 3);
			}
		}

		//----------------------------------------------Production Graph events

		private void Graph_NodeValuesUpdated(object sender, EventArgs e)
		{
			UpdateNodeVisuals();
		}

		private void Graph_LinkDeleted(object sender, NodeLinkEventArgs e)
		{
			BaseNodeElement supplier = nodeElementDictionary[e.nodeLink.Supplier];
			BaseNodeElement consumer = nodeElementDictionary[e.nodeLink.Consumer];

			LinkElement element = linkElementDictionary[e.nodeLink];
			linkElementDictionary.Remove(e.nodeLink);
			linkElements.Remove(element);
			element.Dispose();

			supplier.RequestStateUpdate();
			consumer.RequestStateUpdate();
			Invalidate();
		}

		private void Graph_LinkAdded(object sender, NodeLinkEventArgs e)
		{
			BaseNodeElement supplier = nodeElementDictionary[e.nodeLink.Supplier];
			BaseNodeElement consumer = nodeElementDictionary[e.nodeLink.Consumer];

			LinkElement element = new LinkElement(this, e.nodeLink, supplier, consumer);
			linkElementDictionary.Add(e.nodeLink, element);
			linkElements.Add(element);

			supplier.RequestStateUpdate();
			consumer.RequestStateUpdate();
			Invalidate();
		}

		private void Graph_NodeDeleted(object sender, NodeEventArgs e)
		{
			BaseNodeElement element = nodeElementDictionary[e.node];
			nodeElementDictionary.Remove(e.node);
			nodeElements.Remove(element);
			selectedNodes.Remove(element);
			element.Dispose();
			Invalidate();
		}

		private void Graph_NodeAdded(object sender, NodeEventArgs e)
		{
			BaseNodeElement element = null;
			if (e.node is ReadOnlySupplierNode snode)
				element = new SupplierNodeElement(this, snode);
			else if (e.node is ReadOnlyConsumerNode cnode)
				element = new ConsumerNodeElement(this, cnode);
			else if (e.node is ReadOnlyPassthroughNode pnode)
				element = new PassthroughNodeElement(this, pnode);
			else if (e.node is ReadOnlyRecipeNode rnode)
				element = new RecipeNodeElement(this, rnode);
			else if (e.node is ReadOnlyLabelNode lnode)
				element = new LabelNodeElement(this, lnode);
			else
				Trace.Fail("Unexpected node type created in graph.");

			nodeElementDictionary.Add(e.node, element);
			nodeElements.Add(element);
			Invalidate();
		}

		//----------------------------------------------Mouse events

		private void ProductionGraphViewer_MouseDown(object sender, MouseEventArgs e)
		{
			downButtons |= e.Button;

			ToolTipRenderer.ClearFloatingControls();
			ActiveControl = null; //helps panels like IRChooserPanel (for item/recipe choosing) close when we click on the graph

			mouseDownStartScreenPoint = Control.MousePosition;
			Point graph_location = ScreenToGraph(e.Location);

			GraphElement clickedElement = (GraphElement)draggedLinkElement ?? GetNodeAtPoint(ScreenToGraph(e.Location));
			clickedElement?.MouseDown(graph_location, e.Button);

			if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Right))
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
			downButtons &= ~e.Button;

			ToolTipRenderer.ClearFloatingControls();
			Point graph_location = ScreenToGraph(e.Location);
			GraphElement element = (GraphElement)draggedLinkElement ?? GetNodeAtPoint(graph_location);

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
					else if(currentDragOperation != DragOperation.Selection)
						element?.MouseUp(graph_location, e.Button, (currentDragOperation == DragOperation.Item));
					break;
				case MouseButtons.Middle:
					viewBeingDragged = false;
					break;
				case MouseButtons.Left:
					//finished selecting the given zone (process selected nodes)
					if (currentDragOperation == DragOperation.Selection)
					{
						if ((Control.ModifierKeys & Keys.Alt) != 0) //removal zone processing
							selectedNodes.ExceptWith(currentSelectionNodes);
						else
						{
							if ((Control.ModifierKeys & Keys.Control) == 0) //if we arent using control, then we are just selecting
								selectedNodes.Clear();
							selectedNodes.UnionWith(currentSelectionNodes);
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
						else if (!viewBeingDragged) //left click without modifier keys -> pass click to node
						{
							clickedNode.MouseUp(graph_location, e.Button, false);
						}
					}
					else if (!viewBeingDragged)
						element?.MouseUp(graph_location, e.Button, (currentDragOperation == DragOperation.Item));


					currentDragOperation = DragOperation.None;
					MouseDownElement = null;
					break;
			}
		}

		private void ProductionGraphViewer_MouseMove(object sender, MouseEventArgs e)
		{
			downButtons &= Control.MouseButtons; //only care about those buttons that were pressed down on this control. This is also the best place to update mouse changes done outside the control (ex: clicking down, dragging outside the window, letting go, moving mouse back into window)

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
						if ((downButtons & MouseButtons.Middle) == MouseButtons.Middle || (downButtons & MouseButtons.Right) == MouseButtons.Right)
							viewBeingDragged = true;

						if (MouseDownElement != null) //there is an item under the mouse during drag
							currentDragOperation = DragOperation.Item;
						else if ((downButtons & MouseButtons.Left) != 0)
							currentDragOperation = DragOperation.Selection;
					}
					break;

				case DragOperation.Item:
					if (selectedNodes.Contains(MouseDownElement)) //dragging a group
					{
						Point startPoint = MouseDownElement.Location;
						GraphElement element = MouseDownElement;
						MouseDownElement.Dragged(graph_location);
						if (element == MouseDownElement) //check to ensure that the dragged operation hasnt changed the mousedown element -> as is the case with item tab to dragged link
						{
							Point endPoint = MouseDownElement.Location;
							if (startPoint != endPoint)
								foreach (BaseNodeElement node in selectedNodes.Where(node => node != MouseDownElement))
									node.SetLocation(new Point(node.X + endPoint.X - startPoint.X, node.Y + endPoint.Y - startPoint.Y));
							Invalidate();
						}
					}
					else //dragging single item
					{
						MouseDownElement.Dragged(graph_location);
						Invalidate();
					}

					//accept middle mouse button for view dragging purposes (while dragging item or selection)
					if ((downButtons & MouseButtons.Middle) == MouseButtons.Middle)
						viewBeingDragged = true;
					break;

				case DragOperation.Selection:
					SelectionZone = new Rectangle(Math.Min(SelectionZoneOriginPoint.X, graph_location.X), Math.Min(SelectionZoneOriginPoint.Y, graph_location.Y), Math.Abs(SelectionZoneOriginPoint.X - graph_location.X), Math.Abs(SelectionZoneOriginPoint.Y - graph_location.Y));
					currentSelectionNodes.Clear();
					currentSelectionNodes.UnionWith(nodeElements.Where(element => element.IntersectsWithZone(SelectionZone, -20, -20)));

					UpdateSelection();

					//accept middle mouse button for view dragging purposes (while dragging item or selection)
					if ((downButtons & MouseButtons.Middle) == MouseButtons.Middle)
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
			ViewOffset = new Point(ViewOffset.X + newZoomCenter.X - oldZoomCenter.X, ViewOffset.Y + newZoomCenter.Y - oldZoomCenter.Y);

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
					var writer = new JsonTextWriter(new StringWriter(stringBuilder));

					Graph.SerializeNodeIdSet = new HashSet<int>();
					Graph.SerializeNodeIdSet.UnionWith(selectedNodes.Select(n => n.DisplayedNode.NodeID));

					JsonSerializer serialiser = JsonSerializer.Create();
					serialiser.Formatting = Formatting.None;
					serialiser.Serialize(writer, Graph);

					Graph.SerializeNodeIdSet.Clear();
					Graph.SerializeNodeIdSet = null;

					Clipboard.SetText(stringBuilder.ToString());

					if (e.KeyCode == Keys.X) //cut
						foreach (BaseNodeElement node in selectedNodes.ToList())
							Graph.DeleteNode(node.DisplayedNode);
				}
				else if (e.KeyCode == Keys.V && (e.Modifiers & Keys.Control) == Keys.Control) //paste
				{
					try
					{
						JObject json = JObject.Parse(Clipboard.GetText());
						ImportNodesFromJson(json, ScreenToGraph(PointToClient(Cursor.Position)));
					}
					catch { Console.WriteLine("Non-Foreman paste detected."); } //clipboard string wasnt a proper json object, or didnt process properly. Likely answer: was a clip NOT from foreman.
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
			int hor = 0;
			int ver = 0;

			if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) //large move
				moveUnit = (Grid.CurrentMajorGridUnit > Grid.CurrentGridUnit) ? Grid.CurrentMajorGridUnit : moveUnit * 4;

			if (selectedNodes.Count>0)
			{
				if ((keyData & Keys.KeyCode) == Keys.Left)
				{
					foreach (BaseNodeElement node in selectedNodes)
						node.SetLocation(new Point(node.X - moveUnit, node.Y));
					processed = true;
				}
				else if ((keyData & Keys.KeyCode) == Keys.Right)
				{
					foreach (BaseNodeElement node in selectedNodes)
						node.SetLocation(new Point(node.X + moveUnit, node.Y));
					processed = true;
				}
				else if ((keyData & Keys.KeyCode) == Keys.Up)
				{
					foreach (BaseNodeElement node in selectedNodes)
						node.SetLocation(new Point(node.X, node.Y - moveUnit));
					processed = true;
				}
				else if ((keyData & Keys.KeyCode) == Keys.Down)
				{
					foreach (BaseNodeElement node in selectedNodes)
						node.SetLocation(new Point(node.X, node.Y + moveUnit));
					processed = true;
				}

				if (processed)
				{
					Invalidate();
					return true;
				}
			} else
            {
				if (((keyData & Keys.KeyCode) == Keys.Left) ^ ((keyData & Keys.KeyCode) == KeyLeftCode)) 
				{
					hor = 1;
				}

				if (((keyData & Keys.KeyCode) == Keys.Right) ^ ((keyData & Keys.KeyCode) == KeyRightCode))
				{
					hor = -1;
				}

				if (((keyData & Keys.KeyCode) == Keys.Up) ^ ((keyData & Keys.KeyCode) == KeyUpCode))
				{
					ver = 1;
				}

				if (((keyData & Keys.KeyCode) == Keys.Down) ^ ((keyData & Keys.KeyCode) == KeyDownCode))
				{
					ver = -1;
				}

				ViewOffset = Point.Add(ViewOffset, new Size(hor * (int)KeyScrollRatio, ver * (int)KeyScrollRatio));
				UpdateGraphBounds(false);
				Invalidate();
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
					ViewOffset = new Point(0, 0);
				}
				else
				{
					int newX = ViewOffset.X;
					int newY = ViewOffset.Y;
					if (screenCentre.X < bounds.X) { newX -= bounds.X - screenCentre.X; }
					if (screenCentre.Y < bounds.Y) { newY -= bounds.Y - screenCentre.Y; }
					if (screenCentre.X > bounds.X + bounds.Width) { newX -= bounds.X + bounds.Width - screenCentre.X; }
					if (screenCentre.Y > bounds.Y + bounds.Height) { newY -= bounds.Y + bounds.Height - screenCentre.Y; }
					ViewOffset = new Point(newX, newY);
				}
			}

			VisibleGraphBounds = new Rectangle(
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
			info.AddValue("Version", Properties.Settings.Default.ForemanVersion);
			info.AddValue("Object", "ProductionGraphViewer");
			info.AddValue("SavedPresetName", DCache.PresetName);
			info.AddValue("IncludedMods", DCache.IncludedMods.Select(m => m.Key + "|" + m.Value));

			//graph viewer options
			info.AddValue("Unit", Graph.SelectedRateUnit);
			info.AddValue("ViewOffset", ViewOffset);
			info.AddValue("ViewScale", ViewScale);

			//graph defaults (saved here instead of within the graph since they are used here, plus they arent used during copy/paste)
			info.AddValue("ExtraProdForNonMiners", Graph.EnableExtraProductivityForNonMiners);
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

		public void ImportNodesFromJson(JObject json, Point origin)
		{
			ProductionGraph.NewNodeCollection newNodeCollection = newNodeCollection = Graph.InsertNodesFromJson(DCache, json); //NOTE: missing items & recipes may be added here!
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
			Size offset = (Size)Grid.AlignToGrid(Point.Subtract(origin, (Size)importCenter));
			foreach (ReadOnlyBaseNode newNode in newNodeCollection.newNodes)
				Graph.RequestNodeController(newNode).SetLocation(Point.Add(newNode.Location, offset));

			//update the selection to be just the newly imported nodes
			ClearSelection();
			foreach (BaseNodeElement newNodeElement in newNodeCollection.newNodes.Select(node => nodeElementDictionary[node]))
			{
				selectedNodes.Add(newNodeElement);
				newNodeElement.Highlighted = true;
			}
			Console.WriteLine(selectedNodes.Count);

			UpdateGraphBounds();
			Graph.UpdateNodeValues();
		}

		public void LoadPreset(Preset preset)
		{
			using (DataLoadForm form = new DataLoadForm(preset))
			{
				form.StartPosition = FormStartPosition.Manual;
				form.Left = ParentForm.Left + 150;
				form.Top = ParentForm.Top + 200;
				DialogResult result = form.ShowDialog(); //LOAD FACTORIO DATA
				DCache = form.GetDataCache();
				if (result == DialogResult.Abort)
				{
					MessageBox.Show("The current preset (" + Properties.Settings.Default.CurrentPresetName + ") is corrupt. Switching to the default preset (Factorio 1.1 Vanilla)");
					Properties.Settings.Default.CurrentPresetName = MainForm.DefaultPreset;
					using (DataLoadForm form2 = new DataLoadForm(new Preset(MainForm.DefaultPreset, false, true)))
					{
						form2.StartPosition = FormStartPosition.Manual;
						form2.Left = ParentForm.Left + 150;
						form2.Top = ParentForm.Top + 200;
						DialogResult result2 = form2.ShowDialog(); //LOAD default preset
						DCache = form2.GetDataCache();
						if (result2 == DialogResult.Abort)
							MessageBox.Show("The default preset (" + Properties.Settings.Default.CurrentPresetName + ") is corrupt. No Preset is loaded!");
					}
				}
				GC.Collect(); //loaded a new data cache - the old one should be collected (data caches can be over 1gb in size due to icons, plus whatever was in the old graph)
			}
			Invalidate();
		}

		public async Task LoadFromJson(JObject json, bool useFirstPreset, bool setEnablesFromJson)
		{
			if (json["Version"] == null || (int)json["Version"] != Properties.Settings.Default.ForemanVersion || json["Object"] == null || (string)json["Object"] != "ProductionGraphViewer")
			{
				json = VersionUpdater.UpdateSave(json, DCache);
				if (json == null) //update failed
					return;
			}

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
						if(errors != null)
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
			LoadPreset(chosenPreset);

			//set up graph options
			Graph.SelectedRateUnit = (ProductionGraph.RateUnit)(int)json["Unit"];
			Graph.AssemblerSelector.DefaultSelectionStyle = (AssemblerSelector.Style)(int)json["AssemblerSelectorStyle"];
			Graph.ModuleSelector.DefaultSelectionStyle = (ModuleSelector.Style)(int)json["ModuleSelectorStyle"];
			foreach (string fuelType in json["FuelPriorityList"].Select(t => (string)t))
				if (DCache.Items.ContainsKey(fuelType))
					Graph.FuelSelector.UseFuel(DCache.Items[fuelType]);
			Graph.EnableExtraProductivityForNonMiners = (bool)json["ExtraProdForNonMiners"];

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
				DCache.RocketAssembler.Enabled = DCache.Assemblers["rocket-silo"]?.Enabled ?? false;

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
			ProductionGraph.NewNodeCollection collection = Graph.InsertNodesFromJson(DCache, json["ProductionGraph"]);

			//check for old import
			if (json["OldImport"] != null)
				foreach (ReadOnlyRecipeNode rNode in collection.newNodes.Where(node => node is ReadOnlyRecipeNode))
					((RecipeNodeController)Graph.RequestNodeController(rNode)).AutoSetAssembler(AssemblerSelector.Style.BestNonBurner);

			//upgrade graph & values
			UpdateGraphBounds();
			Graph.UpdateNodeValues();
			this.Focus();
			Invalidate();
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