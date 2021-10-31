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

namespace Foreman
{
	public enum Direction { Up, Down, Left, Right, None }

	public struct TooltipInfo
	{
		public TooltipInfo(Point screenLocation, Size screenSize, Direction direction, String text)
		{
			ScreenLocation = screenLocation;
			ScreenSize = screenSize;
			Direction = direction;
			Text = text;
		}

		public Point ScreenLocation;
		public Size ScreenSize;
		public Direction Direction;
		public String Text;
	}

	public class FloatingTooltipControl : IDisposable
	{
		public Control Control { get; private set; }
		public Direction Direction { get; private set; }
		public Point GraphLocation { get; private set; }
		public ProductionGraphViewer GraphViewer { get; private set; }
		public event EventHandler Closing;

		public FloatingTooltipControl(Control control, Direction direction, Point graphLocation, ProductionGraphViewer parent)
		{
			Control = control;
			Direction = direction;
			GraphLocation = graphLocation;
			GraphViewer = parent;

			parent.floatingTooltipControls.Add(this);
			parent.Controls.Add(control);
			Rectangle ttRect = parent.getTooltipScreenBounds(parent.GraphToScreen(graphLocation), control.Size, direction);
			control.Location = ttRect.Location;
			control.Focus();
		}

		public void Dispose()
		{
			Control.Dispose();
			GraphViewer.floatingTooltipControls.Remove(this);
			if (Closing != null)
			{
				Closing.Invoke(this, null);
			}
		}
	}

	[Serializable]
	public partial class ProductionGraphViewer : UserControl, ISerializable
	{
		private enum DragOperation { None, Item, Selection, Processed }

		public HashSet<GraphElement> Elements = new HashSet<GraphElement>();
		public ProductionGraph Graph = new ProductionGraph();
		private List<Item> Demands = new List<Item>();
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

		public int CurrentGridUnit = 0;
		public int CurrentMajorGridUnit = 0;
		public bool ShowGrid = false;
		public bool LockDragToAxis = false;
		public Rectangle SelectionZone;
		public Point SelectionZoneOriginPoint;

		private HashSet<NodeElement> SelectedNodes; //main list of selected nodes
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
		public bool ShowInserters = true;
		StringFormat stringFormat = new StringFormat();
		public GhostNodeElement GhostDragElement = null;
		public HashSet<FloatingTooltipControl> floatingTooltipControls = new HashSet<FloatingTooltipControl>();

		private const int minDragDiff = 30;

		private Rectangle visibleGraphBounds;

		private ContextMenu rightClickMenu = new ContextMenu();

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

		public ProductionGraphViewer()
		{
			InitializeComponent();
			MouseWheel += new MouseEventHandler(ProductionGraphViewer_MouseWheel);
			DragOver += new DragEventHandler(HandleItemDragging);
			DragDrop += new DragEventHandler(HandleItemDropping);
			DragEnter += new DragEventHandler(HandleDragEntering);
			DragLeave += new EventHandler(HandleDragLeaving);
			Resize += new EventHandler(ProductionGraphViewer_Resized);
			ViewOffset = new Point(Width / -2, Height / -2);

			SelectedNodes = new HashSet<NodeElement>();
			CurrentSelectionNodes = new HashSet<NodeElement>();

			UpdateGraphBounds();
			Invalidate();
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
			Elements.RemoveWhere(e => e is LinkElement && !Graph.GetAllNodeLinks().Contains((e as LinkElement).DisplayedLink));
			Elements.RemoveWhere(e => e is NodeElement && !Graph.Nodes.Contains((e as NodeElement).DisplayedNode));
			SelectedNodes.RemoveWhere(e => !Graph.Nodes.Contains(e.DisplayedNode));

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
				var removedLinks = Elements.Where(le => le is LinkElement && (le as LinkElement).ConsumerTab == tab).ToList();
				foreach (LinkElement removedLink in removedLinks)
				{
					removedLink.DisplayedLink.Destroy();
					Elements.Remove(removedLink);
				}
			}
			else
			{
				var removedLinks = Elements.Where(le => le is LinkElement && (le as LinkElement).SupplierTab == tab).ToList();
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

		public void PositionNodes()
		{
			if (!Elements.Any())
			{
				return;
			}
			var nodeOrder = Graph.GetTopologicalSort();
			nodeOrder.Reverse();

			if (nodeOrder.Any())
			{
				List<ProductionNode>[] nodePositions = new List<ProductionNode>[nodeOrder.Count()];
				for (int i = 0; i < nodePositions.Count(); i++)
				{
					nodePositions[i] = new List<ProductionNode>();
				}

				nodePositions.First().AddRange(nodeOrder.OfType<ConsumerNode>());
				foreach (RecipeNode node in nodeOrder.OfType<RecipeNode>())
				{
					bool PositionFound = false;

					for (int i = nodePositions.Count() - 1; i >= 0 && !PositionFound; i--)
					{
						foreach (ProductionNode listNode in nodePositions[i])
						{
							if (listNode.CanUltimatelyTakeFrom(node))
							{
								nodePositions[i + 1].Add(node);
								PositionFound = true;
								break;
							}
						}
					}

					if (!PositionFound)
					{
						nodePositions.First().Add(node);
					}
				}
				nodePositions.Last().AddRange(nodeOrder.OfType<SupplyNode>());

				int marginX = 100;
				int marginY = 200;
				int y = marginY;
				int[] tierWidths = new int[nodePositions.Count()];
				for (int i = 0; i < nodePositions.Count(); i++)
				{
					var list = nodePositions[i];
					int maxHeight = 0;
					int x = marginX;

					foreach (var node in list)
					{
						NodeElement control = GetElementForNode(node);
						control.X = x;
						control.Y = y;

						x += control.Width + marginX;
						maxHeight = Math.Max(control.Height, maxHeight);
					}

					if (maxHeight > 0) // Don't add any height for empty tiers
					{
						y += maxHeight + marginY;
					}

					tierWidths[i] = x;
				}

				int centrePoint = tierWidths.Last(i => i > marginX) / 2;
				for (int i = tierWidths.Count() - 1; i >= 0; i--)
				{
					int offset = centrePoint - tierWidths[i] / 2;

					foreach (var node in nodePositions[i])
					{
						NodeElement element = GetElementForNode(node);
						element.X = element.X + offset;
					}
				}
			}

			UpdateNodes();
			UpdateGraphBounds();
			Invalidate(true);
		}

		public IEnumerable<GraphElement> GetPaintingOrder()
		{
			foreach (LinkElement element in Elements.OfType<LinkElement>())
			{
				yield return element;
			}
			foreach (NodeElement element in Elements.OfType<NodeElement>())
			{
				yield return element;
			}
			foreach (DraggedLinkElement element in Elements.OfType<DraggedLinkElement>())
			{
				yield return element;
			}
			foreach (GhostNodeElement element in Elements.OfType<GhostNodeElement>())
			{
				yield return element;
			}
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
                int xaxis = draggedNode.DragOrigin.X + draggedNode.Width / 2;
                int yaxis = draggedNode.DragOrigin.Y + draggedNode.Height / 2;
                xaxis = AlignToGrid(xaxis);
                yaxis = AlignToGrid(yaxis);

				graphics.DrawLine(lockedAxisPen, xaxis, visibleGraphBounds.Y, xaxis, visibleGraphBounds.Y + visibleGraphBounds.Height);
				graphics.DrawLine(lockedAxisPen, visibleGraphBounds.X, yaxis, visibleGraphBounds.X + visibleGraphBounds.Width, yaxis);
			}

			//all elements (nodes & lines)
			foreach (GraphElement element in GetPaintingOrder())
			{
				element.Paint(graphics, new Point(element.X, element.Y));
			}

			//selection zone
			if(currentDragOperation == DragOperation.Selection)
            {
				graphics.DrawRectangle(selectionPen, SelectionZone);
            }

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
			if (Graph.PauseUpdates)
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

		public IEnumerable<GraphElement> GetElementsAtPoint(Point point)
		{
			foreach (GraphElement element in GetPaintingOrder().Reverse<GraphElement>())
			{
				if (element.ContainsPoint(Point.Add(point, new Size(-element.X, -element.Y))))
				{
					yield return element;
				}
			}
		}

		private void ProductionGraphViewer_LostFocus(object sender, EventArgs e)
        {
			Invalidate();
		}

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
						foreach (NodeElement ne in SelectedNodes)
							ne.Selected = false;
						SelectedNodes.Clear();
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
								SelectedNodes.Remove(newlySelectedNode);
						}
						else
						{
							if ((Control.ModifierKeys & Keys.Control) == 0) //if we arent using control, then we are just selecting
								SelectedNodes.Clear();

							foreach (NodeElement newlySelectedNode in CurrentSelectionNodes)
								SelectedNodes.Add(newlySelectedNode);
						}
						CurrentSelectionNodes.Clear();
					}
					//this is a release of a left click (non-drag operation) -> modify selection if clicking on node & using modifier keys
					else if (currentDragOperation == DragOperation.None && MouseDownElement is NodeElement clickedNode)
					{
						if ((Control.ModifierKeys & Keys.Alt) != 0) //remove
						{
							SelectedNodes.Remove(clickedNode);
							clickedNode.Selected = false;
							MouseDownElement = null;
							Invalidate();
						}
						else if ((Control.ModifierKeys & Keys.Control) != 0) //add if unselected, remove if selected
						{
							if (clickedNode.Selected)
								SelectedNodes.Remove(clickedNode);
							else
								SelectedNodes.Add(clickedNode);

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
					if (SelectedNodes.Contains(MouseDownElement)) //dragging a group
					{
						Point startPoint = MouseDownElement.Location;
						MouseDownElement.Dragged(Point.Add(ScreenToGraph(e.Location), new Size(-MouseDownElement.X, -MouseDownElement.Y)));
						Point endPoint = MouseDownElement.Location;
						if (startPoint != endPoint)
						{
							foreach(NodeElement node in SelectedNodes)
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

		private void UpdateSelection()
		{
			if ((Control.ModifierKeys & Keys.Alt) != 0) //remove zone
			{
				foreach (NodeElement selectedNode in SelectedNodes)
					selectedNode.Selected = true;
				foreach (NodeElement newlySelectedNode in CurrentSelectionNodes)
					newlySelectedNode.Selected = false;
			}
			else if ((Control.ModifierKeys & Keys.Control) != 0)  //add zone
			{
				foreach (NodeElement selectedNode in SelectedNodes)
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
			foreach(NodeElement ne in SelectedNodes)
            {
				int x = ne.X + ne.Width / 2;
				ne.X = AlignToGrid(x) - ne.Width / 2;
				int y = ne.Y + NodeElement.DeltaZ;
				ne.Y = AlignToGrid(y) - NodeElement.DeltaZ;
            }
			Invalidate();
        }

		void ProductionGraphViewer_MouseWheel(object sender, MouseEventArgs e)
		{
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

				foreach (NodeElement node in SelectedNodes)
					node.X -= moveUnit;
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Right)
			{
				foreach (NodeElement node in SelectedNodes)
					node.X += moveUnit;
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Up)
			{
				foreach (NodeElement node in SelectedNodes)
					node.Y -= moveUnit;
				processed = true;
			}
			else if ((keyData & Keys.KeyCode) == Keys.Down)
			{
				foreach (NodeElement node in SelectedNodes)
					node.Y += moveUnit;
				processed = true;
			}

			if (processed)
				return true;
			return base.ProcessCmdKey(ref msg, keyData);
		}

		void ProductionGraphViewer_Resized(object sender, EventArgs e)
        {
			UpdateGraphBounds();
			Invalidate();
        }

		public void ClearFloatingControls()
		{
			foreach (var control in floatingTooltipControls.ToArray())
			{
				control.Dispose();
			}
		}

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

		public void DeleteNode(NodeElement node)
		{
			if (node != null)
			{
				foreach (NodeLink link in node.DisplayedNode.InputLinks.ToList().Union(node.DisplayedNode.OutputLinks.ToList()))
				{
					Elements.RemoveWhere(le => le is LinkElement && (le as LinkElement).DisplayedLink == link);
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
			if (SelectedNodes.Count > 10)
				proceed = (MessageBox.Show("You are deleting " + SelectedNodes.Count + " nodes. \nAre you sure?", "Confirm delete.", MessageBoxButtons.YesNo) == DialogResult.Yes);
			if (proceed)
			{
				foreach (NodeElement node in SelectedNodes)
					DeleteNode(node);
				SelectedNodes.Clear();
			}
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

		void HandleDragEntering(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(HashSet<Item>)) || e.Data.GetDataPresent(typeof(HashSet<Recipe>)))
			{
				e.Effect = DragDropEffects.All;
			}
		}

		void HandleItemDragging(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(HashSet<Item>)))
			{
				if (GhostDragElement == null)
				{
					GhostDragElement = new GhostNodeElement(this);
					GhostDragElement.Items = e.Data.GetData(typeof(HashSet<Item>)) as HashSet<Item>;
				}

				GhostDragElement.Location = DesktopToGraph(e.X, e.Y);
			} else if (e.Data.GetDataPresent(typeof(HashSet<Recipe>)))
			{
				if (GhostDragElement == null)
				{
					GhostDragElement = new GhostNodeElement(this);
					GhostDragElement.Recipes = e.Data.GetData(typeof(HashSet<Recipe>)) as HashSet<Recipe>;
				}

				GhostDragElement.Location = DesktopToGraph(e.X, e.Y);
			}

			Invalidate();
		}

		void HandleItemDropping(object sender, DragEventArgs e)
		{
			if (GhostDragElement != null)
			{
				if (e.Data.GetDataPresent(typeof(HashSet<Item>)))
				{
					foreach (Item item in GhostDragElement.Items)
					{
						NodeElement newElement = null;

						var itemSupplyOption = new ItemChooserControl(item, "Create infinite supply node", item.FriendlyName);
						var itemOutputOption = new ItemChooserControl(item, "Create output node", item.FriendlyName);
						var itemPassthroughOption = new ItemChooserControl(item, "Create pass-through node", item.FriendlyName);

						var optionList = new List<ChooserControl>();
						optionList.Add(itemPassthroughOption);
						optionList.Add(itemOutputOption);
						foreach (Recipe recipe in item.ProductionRecipes)
							if (recipe.Enabled)
								optionList.Add(new RecipeChooserControl(recipe, String.Format("Create '{0}' recipe node", recipe.FriendlyName), recipe.FriendlyName));
						optionList.Add(itemSupplyOption);

						var chooserPanel = new ChooserPanel(optionList, this, ChooserPanel.RecipeIconSize);

						Point location = GhostDragElement.Location;
						if (ShowGrid)
						{
							location.X = AlignToGrid(location.X);
							location.Y = AlignToGrid(location.Y);
						}

						chooserPanel.Show(c =>
						{
							if (c != null)
							{
								if (c == itemSupplyOption)
								{
									newElement = new NodeElement(SupplyNode.Create(item, this.Graph), this);
								}
								else if (c is RecipeChooserControl)
								{
									newElement = new NodeElement(RecipeNode.Create((c as RecipeChooserControl).DisplayedRecipe, this.Graph), this);
								}
                                else if (c == itemPassthroughOption)
                                {
                                    newElement = new NodeElement(PassthroughNode.Create(item, this.Graph), this);
                                }
								else if (c == itemOutputOption)
								{
									newElement = new NodeElement(ConsumerNode.Create(item, this.Graph), this);
								} else
                                {
                                    Trace.Fail("No handler for selected item");
                                }

								//Graph.UpdateNodeValues(); // no need - its a disconnected node!
								newElement.Update();
								newElement.Location = Point.Add(location, new Size(-newElement.Width / 2, -newElement.Height / 2));
							}
						});
					}

				}
				else if (e.Data.GetDataPresent(typeof(HashSet<Recipe>)))
				{
					foreach (Recipe recipe in GhostDragElement.Recipes)
					{
						NodeElement newElement = new NodeElement(RecipeNode.Create(recipe, Graph), this);
						Graph.UpdateNodeValues();
						newElement.Update();
						newElement.Location = Point.Add(GhostDragElement.Location, new Size(-newElement.Width / 2, -newElement.Height / 2));
					}
				}

				GhostDragElement.Dispose();
			}
		}

		void HandleDragLeaving(object sender, EventArgs e)
		{
			if (GhostDragElement != null)
			{
				GhostDragElement.Dispose();
			}
		}

		public int AlignToGrid(int original)
		{
			if (CurrentGridUnit < 1)
				return original;

			original += Math.Sign(original) * CurrentGridUnit / 2;
			original -= original % CurrentGridUnit;
			return original;
		}

		public void OpenNodeMenu(NodeElement node)
		{
			rightClickMenu.MenuItems.Clear();
			rightClickMenu.MenuItems.Add(new MenuItem("Delete node",
				new EventHandler((o, e) =>
				{
					DeleteNode(node);
				})));
			if (SelectedNodes.Count > 2 && SelectedNodes.Contains(node))
			{
				rightClickMenu.MenuItems.Add(new MenuItem("Delete selected nodes",
				new EventHandler((o, e) =>
				{
					TryDeleteSelectedNodes();
				})));
			}
			rightClickMenu.Show(Parent, Point.Add(Control.MousePosition, new Size(0, -20)));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("AmountType", Graph.SelectedAmountType);
			info.AddValue("Unit", Graph.SelectedUnit);
			info.AddValue("Nodes", Graph.Nodes);
			info.AddValue("NodeLinks", Graph.GetAllNodeLinks());
			info.AddValue("EnabledAssemblers", DataCache.Assemblers.Values.Where(a => a.Enabled).Select<Assembler, String>(a => a.Name));
			info.AddValue("EnabledMiners", DataCache.Miners.Values.Where(m => m.Enabled).Select<Miner, String>(m => m.Name));
			info.AddValue("EnabledModules", DataCache.Modules.Values.Where(m => m.Enabled).Select<Module, String>(m => m.Name));
			info.AddValue("EnabledMods", DataCache.Mods.Where(m => m.Enabled).Select<Mod, String>(m => m.Name));
			info.AddValue("EnabledRecipes", DataCache.Recipes.Values.Where(r => r.Enabled).Select<Recipe, String>(r => r.Name));
			List<Point> elementLocations = new List<Point>();
			foreach (ProductionNode node in Graph.Nodes)
			{
				elementLocations.Add(GetElementForNode(node).Location);
			}
			info.AddValue("ElementLocations", elementLocations);
		}

		public void LoadFromJson(JObject json, bool resetEnabledStates = false)
		{
			Graph.Nodes.Clear();
			Elements.Clear();

			//Has to go first, as all other data depends on which mods are loaded
			List<String> EnabledMods = json["EnabledMods"].Select(t => (String)t).ToList();
			foreach (Mod mod in DataCache.Mods)
			{
				mod.Enabled = EnabledMods.Contains(mod.Name);
			}
			List<String> enabledMods = DataCache.Mods.Where(m => m.Enabled).Select(m => m.Name).ToList();


            using (DataReloadForm form = new DataReloadForm(enabledMods, (DataCache.GenerationType)(Properties.Settings.Default.GenerationType)))
            {
                form.ShowDialog();
            }

			Graph.SelectedAmountType = (AmountType)(int)json["AmountType"];
			Graph.SelectedUnit = (RateUnit)(int)json["Unit"];

			List<JToken> nodes = json["Nodes"].ToList<JToken>();
			foreach (var node in nodes)
			{
				ProductionNode newNode = null;

				switch ((String)node["NodeType"])
				{
					case "Consumer":
						{
							String itemName = (String)node["ItemName"];
							if (DataCache.Items.ContainsKey(itemName))
							{
								Item item = DataCache.Items[itemName];
								newNode = ConsumerNode.Create(item, Graph);
							}
							else
							{
								Item missingItem = new Item(itemName);
								missingItem.IsMissingItem = true;
								newNode = ConsumerNode.Create(missingItem, Graph);
							}
							break;
						}
					case "Supply":
						{
							String itemName = (String)node["ItemName"];
							if (DataCache.Items.ContainsKey(itemName))
							{
								Item item = DataCache.Items[itemName];
								newNode = SupplyNode.Create(item, Graph);
							}
							else
							{
								Item missingItem = new Item(itemName);
								missingItem.IsMissingItem = true;
								DataCache.Items.Add(itemName, missingItem);
								newNode = SupplyNode.Create(missingItem, Graph);
							}
							break;
						}
					case "PassThrough":
						{
							String itemName = (String)node["ItemName"];
							if (DataCache.Items.ContainsKey(itemName))
							{
								Item item = DataCache.Items[itemName];
								newNode = PassthroughNode.Create(item, Graph);
							}
							else
							{
								Item missingItem = new Item(itemName);
								missingItem.IsMissingItem = true;
								DataCache.Items.Add(itemName, missingItem);
								newNode = PassthroughNode.Create(missingItem, Graph);
							}
							break;
						}
					case "Recipe":
						{
							String recipeName = (String)node["RecipeName"];
							if (DataCache.Recipes.ContainsKey(recipeName))
							{
								Recipe recipe = DataCache.Recipes[recipeName];
								newNode = RecipeNode.Create(recipe, Graph);
							}
							else
							{
								Recipe missingRecipe = new Recipe(recipeName);
								missingRecipe.IsMissingRecipe = true;
								DataCache.Recipes.Add(recipeName, missingRecipe);
								newNode = RecipeNode.Create(missingRecipe, Graph);
							}

							if (node["Assembler"] != null)
							{
								var assemblerKey = (String)node["Assembler"];
								if (DataCache.Assemblers.ContainsKey(assemblerKey))
								{
									(newNode as RecipeNode).Assembler = DataCache.Assemblers[assemblerKey];
								}
							}

							(newNode as RecipeNode).NodeModules = ModuleSelector.Load(node);
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
                    {
                        if (node["DesiredRate"] != null)
                        {
                            newNode.desiredRate = (float)node["DesiredRate"];
                        } else
                        {
                            // Legacy data format stored desired rate in actual
                            newNode.desiredRate = (float)node["ActualRate"];
                        }
                    }
                    if (node["SpeedBonus"] != null)
                        newNode.SpeedBonus = Math.Round((float)node["SpeedBonus"], 4);
                    if (node["ProductivityBonus"] != null)
                        newNode.ProductivityBonus = Math.Round((float)node["ProductivityBonus"], 4);
				}
			}

			List<JToken> nodeLinks = json["NodeLinks"].ToList<JToken>();
			foreach (var nodelink in nodeLinks)
			{
				ProductionNode supplier = Graph.Nodes[(int)nodelink["Supplier"]];
				ProductionNode consumer = Graph.Nodes[(int)nodelink["Consumer"]];

				String itemName = (String)nodelink["Item"];
				if (!DataCache.Items.ContainsKey(itemName))
				{
					Item missingItem = new Item(itemName);
					missingItem.IsMissingItem = true;
					DataCache.Items.Add(itemName, missingItem);
				}
				Item item = DataCache.Items[itemName];
				NodeLink.Create(supplier, consumer, item);
			}

			IEnumerable<String> EnabledAssemblers = json["EnabledAssemblers"].Select(t => (String)t);
			foreach (Assembler assembler in DataCache.Assemblers.Values)
			{
				assembler.Enabled = EnabledAssemblers.Contains(assembler.Name);
			}

			IEnumerable<String> EnabledMiners = json["EnabledMiners"].Select(t => (String)t);
			foreach (Miner miner in DataCache.Miners.Values)
			{
				miner.Enabled = EnabledMiners.Contains(miner.Name);
			}

			IEnumerable<String> EnabledModules = json["EnabledModules"].Select(t => (String)t);
			foreach (Module module in DataCache.Modules.Values)
			{
				module.Enabled = EnabledModules.Contains(module.Name);
			}

			JToken enabledRecipesToken;
			if (json.TryGetValue("EnabledRecipes", out enabledRecipesToken))
			{
				IEnumerable<String> EnabledRecipes = enabledRecipesToken.Select(t => (String)t);
				foreach (Recipe recipe in DataCache.Recipes.Values)
				{
					recipe.Enabled = EnabledRecipes.Contains(recipe.Name);
				}
			}

			Graph.UpdateNodeValues();
			AddRemoveElements();

			List<String> ElementLocations = json["ElementLocations"].Select(l => (String)l).ToList();
			for (int i = 0; i < ElementLocations.Count; i++)
			{
				int[] splitPoint = ElementLocations[i].Split(',').Select(s => Convert.ToInt32(s)).ToArray();
				GraphElement element = GetElementForNode(Graph.Nodes[i]);
				element.Location = new Point(splitPoint[0], splitPoint[1]);
			}

			UpdateGraphBounds();

			Graph.UpdateNodeValues();
			UpdateNodes();
			Invalidate();
		}
	}
}