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
	[Serializable]
	public partial class ProductionGraphViewer : UserControl, ISerializable
	{
		private enum DragOperation { None, Item, Selection, Processed }
		public enum NewNodeType { Disconnected, Supplier, Consumer }

		public HashSet<GraphElement> Elements = new HashSet<GraphElement>();
		public ProductionGraph Graph; //set it up when we have cache
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

		public bool ShowAssemblers = false;
		public bool ShowMiners = false;

		public bool DynamicLinkWidth = false;
		private const int minLinkWidth = 3;
		private const int maxLinkWidth = 40;

		public HashSet<FloatingTooltipControl> floatingTooltipControls = new HashSet<FloatingTooltipControl>();
		StringFormat stringFormat = new StringFormat(); //used for tooltip drawing so as not to create a new one each time

		public Rectangle GraphBounds
		{
			get
			{
				int counter = 0;
				int x = int.MaxValue;
				int y = int.MaxValue;
				foreach (NodeElement element in Elements.OfType<NodeElement>())
				{
					counter++;
					x = Math.Min(element.X, x);
					y = Math.Min(element.Y, y);
				}
				int width = 0;
				int height = 0;
				foreach (NodeElement element in Elements.OfType<NodeElement>())
				{
					height = Math.Max(element.Y + element.Height - y, height);
					width = Math.Max(element.X + element.Width - x, width);
				}

				if (counter > 0)
					return new Rectangle(x - 80, y - 80, width + 160, height + 160);
				else
					return new Rectangle(0, 0, 0, 0);
			}
		}
		private Rectangle visibleGraphBounds;

		private ContextMenu rightClickMenu = new ContextMenu();

		public ProductionGraphViewer()
		{
			InitializeComponent();
			MouseWheel += new MouseEventHandler(ProductionGraphViewer_MouseWheel);
			Resize += new EventHandler(ProductionGraphViewer_Resized);
			ViewOffset = new Point(Width / -2, Height / -2);

			selectedNodes = new HashSet<NodeElement>();
			CurrentSelectionNodes = new HashSet<NodeElement>();

			UpdateGraphBounds();
			Invalidate();
		}

		//----------------------------------------------Node functions

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
			recipeChooser.Show(newProductionNode =>
			{
				if(newProductionNode != null)
                {
					NodeElement newElement = new NodeElement(newProductionNode, this);
					newElement.Update();
					newElement.Location = newLocation;

					if(nNodeType == NewNodeType.Consumer)
                    {
						NodeLink newLink = NodeLink.Create(originElement.DisplayedNode, newElement.DisplayedNode, baseItem);
						new LinkElement(this, newLink, originElement, newElement);
					}
					else if(nNodeType == NewNodeType.Supplier)
                    {
						NodeLink newLink = NodeLink.Create(newElement.DisplayedNode, originElement.DisplayedNode, baseItem);
						new LinkElement(this, newLink, newElement, originElement);
					}
				}

				Graph.UpdateNodeValues();
				AddRemoveElements();
				UpdateNodes();
				UpdateGraphBounds();
				Invalidate();
			});
        }

		public void UpdateNodes()
		{
			try
			{

				foreach (NodeElement node in Elements.OfType<NodeElement>().ToList())
				{
					node.Update();
				}
			} catch (OverflowException)
			{
				//Same as when working out node values, there's not really much to do here... Maybe I could show a tooltip saying the numbers are too big or something...
			}
			Invalidate();
		}

		public void AddRemoveElements()
		{
			Elements.RemoveWhere(e => e is LinkElement le && !Graph.GetAllNodeLinks().Contains(le.DisplayedLink));
			Elements.RemoveWhere(e => e is NodeElement le && !Graph.Nodes.Contains(le.DisplayedNode));
			selectedNodes.RemoveWhere(e => !Graph.Nodes.Contains(e.DisplayedNode));

			foreach (ProductionNode node in Graph.Nodes)
			{
				if (!Elements.OfType<NodeElement>().Any(e => e.DisplayedNode == node))
				{
					Elements.Add(new NodeElement(node, this));
				}
			}

			foreach (NodeLink link in Graph.GetAllNodeLinks())
			{
				if (!Elements.OfType<LinkElement>().Any(e => e.DisplayedLink == link))
				{
					Elements.Add(new LinkElement(this, link, GetElementForNode(link.Supplier), GetElementForNode(link.Consumer)));
				}
			}

			UpdateNodes();
			Invalidate();
		}

		public void RemoveAssociatedLinks(ItemTab tab)
        {
			if (tab.Type == LinkType.Input)
			{
				var removedLinks = Elements.Where(e => e is LinkElement le && le.ConsumerTab == tab).ToList();
				foreach (LinkElement removedLink in removedLinks)
				{
					removedLink.DisplayedLink.Destroy();
					Elements.Remove(removedLink);
				}
			}
			else
			{
				var removedLinks = Elements.Where(e => e is LinkElement le && le.SupplierTab == tab).ToList();
				foreach (LinkElement removedLink in removedLinks)
				{
					removedLink.DisplayedLink.Destroy();
					Elements.Remove(removedLink);
				}
			}

			Graph.UpdateNodeValues();
			UpdateNodes();
			Invalidate();
		}

		public NodeElement GetElementForNode(ProductionNode node)
		{
			return Elements.OfType<NodeElement>().FirstOrDefault(e => e.DisplayedNode == node);
		}

		public IEnumerable<GraphElement> GetElementsAtPoint(Point point)
		{
			foreach (GraphElement element in GetPaintingOrder().Reverse())
				if (element.ContainsPoint(Point.Add(point, new Size(-element.X, -element.Y))))
					yield return element;
		}

		public void DeleteNode(NodeElement node)
		{
			if (node != null)
			{
				foreach (NodeLink link in node.DisplayedNode.InputLinks.ToList().Union(node.DisplayedNode.OutputLinks.ToList()))
				{
					Elements.RemoveWhere(e => e is LinkElement le && le.DisplayedLink == link);
				}
				Elements.Remove(node);
				node.DisplayedNode.Destroy();
				Graph.UpdateNodeValues();
				UpdateNodes();
				Invalidate();
			}
		}

		public void TryDeleteSelectedNodes()
		{
			bool proceed = true;
			if (selectedNodes.Count > 10)
				proceed = (MessageBox.Show("You are deleting " + selectedNodes.Count + " nodes. \nAre you sure?", "Confirm delete.", MessageBoxButtons.YesNo) == DialogResult.Yes);
			if (proceed)
			{
				foreach (NodeElement node in selectedNodes)
					DeleteNode(node);
				selectedNodes.Clear();
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
			{
				ne.X = AlignToGrid(ne.X);
				ne.Y = AlignToGrid(ne.Y);
			}
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
			element.Paint(graphics, new Point(element.X, element.Y));

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
					foreach (TooltipInfo tti in element.GetToolTips(Point.Add(ScreenToGraph(PointToClient(Control.MousePosition)), new Size(-element.X, -element.Y))))
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
				clickedElement.MouseDown(Point.Add(ScreenToGraph(e.Location), new Size(-clickedElement.X, -clickedElement.Y)), e.Button);
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
					element.MouseUp(Point.Add(ScreenToGraph(e.Location), new Size(-element.X, -element.Y)), e.Button, (currentDragOperation == DragOperation.Item));
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
					element.MouseMoved(Point.Add(ScreenToGraph(e.Location), new Size(-element.X, -element.Y)));
				}
			}

			switch(currentDragOperation)
			{
				case DragOperation.None: //check for minimal distance to be considered a drag operation
					Point dragDiff = Point.Add(Control.MousePosition, new Size(-mouseDownStartScreenPoint.X, -mouseDownStartScreenPoint.Y));
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
						MouseDownElement.Dragged(Point.Add(ScreenToGraph(e.Location), new Size(-MouseDownElement.X, -MouseDownElement.Y)));
						Point endPoint = MouseDownElement.Location;
						if (startPoint != endPoint)
						{
							foreach(NodeElement node in selectedNodes)
                            {
								if (node != MouseDownElement)
								{
									node.X += endPoint.X - startPoint.X;
									node.Y += endPoint.Y - startPoint.Y;
								}
                            }
						}
					}
					else //dragging single item
					{
						MouseDownElement.Dragged(Point.Add(ScreenToGraph(e.Location), new Size(-MouseDownElement.X, -MouseDownElement.Y)));
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
					node.X -= moveUnit;
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Right)
			{
				foreach (NodeElement node in selectedNodes)
					node.X += moveUnit;
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Up)
			{
				foreach (NodeElement node in selectedNodes)
					node.Y -= moveUnit;
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Down)
			{
				foreach (NodeElement node in selectedNodes)
					node.Y += moveUnit;
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
				Rectangle bounds = GraphBounds;
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
			//prepare list of used items & list of used recipes (with their inputs & outputs)
			HashSet<string> includedItems = new HashSet<string>();
			HashSet<Recipe> includedRecipes = new HashSet<Recipe>();
			List<Recipe> includedMissingRecipes = new List<Recipe>(); //why wont this work with a hashset! I want a hashset!
			foreach(ProductionNode node in Graph.Nodes)
            {
				if (node is RecipeNode rnode)
				{
					if(rnode.BaseRecipe.IsMissingRecipe)
						includedMissingRecipes.Add(rnode.BaseRecipe);
					else
						includedRecipes.Add(rnode.BaseRecipe);
				}

				foreach (Item input in node.Inputs)
					includedItems.Add(input.Name);
				foreach (Item output in node.Outputs)
					includedItems.Add(output.Name);
            }
			List<RecipeShort> includedRecipeShorts = includedRecipes.Select(recipe => new RecipeShort(recipe)).ToList();
			includedRecipeShorts.AddRange(includedMissingRecipes.Select(recipe => new RecipeShort(recipe))); //add the missing after the regular, since when we compare saves to preset we will only check 1st recipe of its name (the non-missing kind then)
			
			//write
			info.AddValue("AmountType", Graph.SelectedAmountType);
			info.AddValue("Unit", Graph.SelectedUnit);
			info.AddValue("SavedPresetName", Graph.DCache.PresetName);

			info.AddValue("IncludedItems", includedItems);
			info.AddValue("IncludedRecipes", includedRecipeShorts);
			info.AddValue("IncludedMods", Graph.DCache.IncludedMods.Select(m => m.Key + "|"+m.Value));

			info.AddValue("EnabledAssemblers", Graph.DCache.Assemblers.Values.Where(a => a.Enabled).Select(a => a.Name));
			info.AddValue("EnabledMiners", Graph.DCache.Miners.Values.Where(m => m.Enabled).Select(m => m.Name));
			info.AddValue("EnabledModules", Graph.DCache.Modules.Values.Where(m => m.Enabled).Select(m => m.Name));
			info.AddValue("HiddenRecipes", Graph.DCache.Recipes.Values.Where(r => r.Hidden).Select(r => r.Name));

			info.AddValue("Nodes", Graph.Nodes);
			info.AddValue("NodeLinks", Graph.GetAllNodeLinks());
			info.AddValue("ElementLocations", Graph.Nodes.Select(n => GetElementForNode(n).Location).ToList());
		}

		public void LoadFromOldJson(JObject json)
        {
			//need to convert it to the new format
			//then:
			//LoadFromJson(json);
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
			List<string> itemNames = json["IncludedItems"].Select(t => (string)t).ToList();
			Dictionary<string, List<RecipeShort>> recipeShorts = RecipeShort.GetSetFromJson(json["IncludedRecipes"]);

			//now - two options:
			// a) we are told to use the first preset (basically, the selected preset) - so that is the only one added to the possible Presets
			// b) we can choose preset - so go through each one and compare mod lists - ask to continue if
			// the preset list will then be checked for compatibility based on recipes, and the one with least errors will be used.
			// any errors will prompt a message box saying that 'incompatibility was found, but proceeding anyways'.
			List<Preset> allPresets = MainForm.GetValidPresetsList();
			Dictionary<Preset, PresetErrorPackage> presetErrors = new Dictionary<Preset, PresetErrorPackage>();
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
						presetErrors.Add(savedWPreset, errors);
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
							presetErrors.Add(preset, errors);
					}

					//show the menu to select the preferred preset
					using (PresetSelectionForm form = new PresetSelectionForm())
					{
						form.StartPosition = FormStartPosition.Manual;
						form.Left = this.Left + 150;
						form.Top = this.Top + 100;
						form.ShowDialog();

						chosenPreset = form.ChosenPreset;

						return;
					}
				}
			}

			//clear graph
			Graph.Nodes.Clear();
			Elements.Clear();

			//load new preset
			using (DataLoadForm form = new DataLoadForm(chosenPreset))
			{
				form.ShowDialog(); //LOAD FACTORIO DATA
				Graph = new ProductionGraph(form.GetDataCache());
				GC.Collect(); //loaded a new data cache - the old one should be collected (data caches can be over 1gb in size)
			}

			//set up graph options
			Graph.SelectedAmountType = (AmountType)(int)json["AmountType"];
			Graph.SelectedUnit = (RateUnit)(int)json["Unit"];

			if (!enableEverything)
			{
				//update enabled statuses
				foreach (Assembler assembler in Graph.DCache.Assemblers.Values)
					assembler.Enabled = false;
				foreach (string name in json["EnabledAssemblers"].Select(t => (string)t).ToList())
					if (Graph.DCache.Assemblers.ContainsKey(name))
						Graph.DCache.Assemblers[name].Enabled = true;

				foreach (Miner miner in Graph.DCache.Miners.Values)
					miner.Enabled = false;
				foreach (string name in json["EnabledMiners"].Select(t => (string)t).ToList())
					if (Graph.DCache.Miners.ContainsKey(name))
						Graph.DCache.Miners[name].Enabled = true;

				foreach (Module module in Graph.DCache.Modules.Values)
					module.Enabled = false;
				foreach (string name in json["EnabledModules"].Select(t => (string)t).ToList())
					if (Graph.DCache.Modules.ContainsKey(name))
						Graph.DCache.Modules[name].Enabled = true;

				foreach (string recipe in json["HiddenRecipes"].Select(t => (string)t).ToList())
					if (Graph.DCache.Recipes.ContainsKey(recipe))
						Graph.DCache.Recipes[recipe].Hidden = true;
			}
			//add all nodes
			InsertJsonObjects(json, false);
		}

		public void InsertJsonObjects(JObject json, bool relativePlacement = false)
		{
			//match rate types
			float multiplier = 1;
			if(Graph.SelectedUnit != (RateUnit)(int)json["Unit"])
            {
				if (Graph.SelectedUnit == RateUnit.PerMinute) //json unit is per second
					multiplier = 60;
				else if (Graph.SelectedUnit == RateUnit.PerSecond) //json unit is per minute
					multiplier = 1 / 60;
            }

			//check all items for existance
			foreach (string iItem in json["IncludedItems"].Select(t => (string)t).ToList())
				if (!Graph.DCache.Items.ContainsKey(iItem) && !Graph.DCache.MissingItems.ContainsKey(iItem)) //want to check for missing items too - in this case dont want duplicates
					new Item(Graph.DCache, iItem, iItem, false, Graph.DCache.MissingSubgroup, "", true); //datacache will take care of it.

			//check all recipes for compliance (and add the necessary recipes/items to the missing set)
			//at the same time set up the recipeLinks (recipe ID -> recipe). we will use them to get the correct recipe to the nodes
			Dictionary<string, List<RecipeShort>> includedRecipes = RecipeShort.GetSetFromJson(json["IncludedRecipes"]);
			Dictionary<long, Recipe> recipeLinks = new Dictionary<long, Recipe>();
			foreach (List<RecipeShort> recipeShortList in includedRecipes.Values)
			{
				foreach (RecipeShort iRecipe in recipeShortList)
				{
					Recipe recipe = null;

					//recipe check #1 : does its name exist in database (note: we dont quite care about extra missing recipes here - so what if we have a couple identical ones? they will combine during save/load anyway)
					bool recipeExists = Graph.DCache.Recipes.ContainsKey(iRecipe.Name);
					//recipe check #2 : do the number of ingredients & products match?
					recipeExists &= iRecipe.Ingredients.Count == Graph.DCache.Recipes[iRecipe.Name].IngredientList.Count;
					recipeExists &= iRecipe.Products.Count == Graph.DCache.Recipes[iRecipe.Name].ProductList.Count;
					//recipe check #3 : do the ingredients & products from the loaded data exist within the actual recipe?
					if (recipeExists)
					{
						recipe = Graph.DCache.Recipes[iRecipe.Name];
						//check #2 (from above) - dodnt care about amounts of each
						foreach (string ingredient in iRecipe.Ingredients.Keys)
							recipeExists &= Graph.DCache.Items.ContainsKey(ingredient) && recipe.IngredientSet.ContainsKey(Graph.DCache.Items[ingredient]);
						foreach (string product in iRecipe.Products.Keys)
							recipeExists &= Graph.DCache.Items.ContainsKey(product) && recipe.ProductSet.ContainsKey(Graph.DCache.Items[product]);
					}

					if (!recipeExists)
					{
						Recipe missingRecipe = new Recipe(Graph.DCache, iRecipe.Name, iRecipe.Name, Graph.DCache.MissingSubgroup, "", true);
						foreach (var ingredient in iRecipe.Ingredients)
						{
							if (Graph.DCache.Items.ContainsKey(ingredient.Key))
								missingRecipe.AddIngredient(Graph.DCache.Items[ingredient.Key], ingredient.Value);
							else
								missingRecipe.AddIngredient(Graph.DCache.MissingItems[ingredient.Key], ingredient.Value);
						}
						foreach (var product in iRecipe.Products)
						{
							if (Graph.DCache.Items.ContainsKey(product.Key))
								missingRecipe.AddProduct(Graph.DCache.Items[product.Key], product.Value);
							else
								missingRecipe.AddProduct(Graph.DCache.MissingItems[product.Key], product.Value);
						}
						recipe = missingRecipe;
					}

					recipeLinks.Add(iRecipe.RecipeID, recipe);
				}
			}

			//load in the graph nodes
			HashSet<int> failedNodes = new HashSet<int>(); //we will use these in the 'extreme' case of failure to create a node. Any links to such nodes will be ignored
			foreach (JToken node in json["Nodes"].ToList())
			{
				ProductionNode newNode = null;

				switch ((string)node["NodeType"])
				{
					case "Consumer":
						{
							string itemName = (string)node["ItemName"];
							if (Graph.DCache.MissingItems.ContainsKey(itemName))
								newNode = ConsumerNode.Create(Graph.DCache.MissingItems[itemName], Graph);
							else if (Graph.DCache.Items.ContainsKey(itemName))
								newNode = ConsumerNode.Create(Graph.DCache.Items[itemName], Graph);
							break;
						}
					case "Supply":
						{
							string itemName = (string)node["ItemName"];
							if (Graph.DCache.MissingItems.ContainsKey(itemName))
								newNode = SupplierNode.Create(Graph.DCache.MissingItems[itemName], Graph);
							else if (Graph.DCache.Items.ContainsKey(itemName))
								newNode = SupplierNode.Create(Graph.DCache.Items[itemName], Graph);
							break;
						}
					case "PassThrough":
						{
							string itemName = (string)node["ItemName"];
							if (Graph.DCache.MissingItems.ContainsKey(itemName))
                                newNode = PassthroughNode.Create(Graph.DCache.MissingItems[itemName], Graph);
                            else if (Graph.DCache.Items.ContainsKey(itemName))
                                newNode = PassthroughNode.Create(Graph.DCache.Items [itemName], Graph);
							break;
						}
					case "Recipe":
						{
							long recipeID = (long)node["RecipeID"];
							newNode = RecipeNode.Create(recipeLinks[recipeID], Graph);

							if (node["Assembler"] != null)
							{
								var assemblerKey = (string)node["Assembler"];
								if (Graph.DCache.Assemblers.ContainsKey(assemblerKey))
									(newNode as RecipeNode).Assembler = Graph.DCache.Assemblers[assemblerKey];
							}
							(newNode as RecipeNode).NodeModules = ModuleSelector.Load(node, Graph.DCache);
							break;
						}
                    default:
                        {
                            Trace.Fail("Unknown node type: " + node["NodeType"]);
                            break;
                        }
				}

				if (newNode != null)
				{
					newNode.rateType = (RateType)(int)node["RateType"];
					if (newNode.rateType == RateType.Manual)
						newNode.desiredRate = (float)node["DesiredRate"] * multiplier;
					if (node["SpeedBonus"] != null)
						newNode.SpeedBonus = Math.Round((float)node["SpeedBonus"], 4);
					if (node["ProductivityBonus"] != null)
						newNode.ProductivityBonus = Math.Round((float)node["ProductivityBonus"], 4);
				}
				else
                {
					Trace.Fail("ErrorNode created: ");
					newNode = ErrorNode.Create(Graph);
                }
			}

			//link the nodes
			foreach (JToken nodelink in json["NodeLinks"].ToList())
			{
				int supplierId = (int)nodelink["Supplier"];
				ProductionNode supplier = Graph.Nodes[supplierId];
				int consumerId = (int)nodelink["Consumer"];
				ProductionNode consumer = Graph.Nodes[consumerId];
				if (!failedNodes.Contains(supplierId) && !failedNodes.Contains(consumerId) && !(supplier is ErrorNode) && !(consumer is ErrorNode))
				{
					string itemName = (string)nodelink["Item"];
					Item item;
					if (Graph.DCache.Items.ContainsKey(itemName))
						item = Graph.DCache.Items[itemName];
					else if (Graph.DCache.MissingItems.ContainsKey(itemName))
						item = Graph.DCache.MissingItems[itemName];
					else
						continue;
					NodeLink.Create(supplier, consumer, item);
				}
			}

			//add in the graph elements based on the nodes added in above
			//Graph.UpdateNodeValues();
			AddRemoveElements();

			//place the nodes in their correct locations
			List<string> ElementLocations = json["ElementLocations"].Select(l => (string)l).ToList();
			for (int i = 0; i < ElementLocations.Count; i++)
			{
				int[] splitPoint = ElementLocations[i].Split(',').Select(s => Convert.ToInt32(s)).ToArray();
				GraphElement element = GetElementForNode(Graph.Nodes[i]);
				element.Location = new Point(splitPoint[0], splitPoint[1]);
			}

			//update the graph (both value optimization and nodes)
			UpdateGraphBounds();
			Graph.UpdateNodeValues();
			UpdateNodes();
			Invalidate();
		}

		//Stolen from the designer file
		protected override void Dispose(bool disposing)
		{
			stringFormat.Dispose();
			foreach (var element in Elements.ToList())
			{
				element.Dispose();
			}

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