using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Foreman
{
	public class DraggedLinkElement : BaseLinkElement
	{
		public override Item Item { get; protected set; }
		public LinkType StartConnectionType { get; private set; }
		private Point newObjectLocation;

		private bool dragEnded;

		public DraggedLinkElement(ProductionGraphViewer graphViewer, BaseNodeElement startNode, LinkType startConnectionType, Item item) : base(graphViewer)
		{
			if (startNode == null)
				Trace.Fail("Cant create a dragged link element with a null startNode!");

			if (startConnectionType == LinkType.Input)
				ConsumerElement = startNode;
			else
				SupplierElement = startNode;

			StartConnectionType = startConnectionType;
			Item = item;

			dragEnded = false;
		}

		public override void UpdateVisibility(Rectangle graph_zone, int xborder, int yborder) { Visible = true; } //always visible.

		protected override Tuple<Point,Point> GetCurveEndpoints()
		{
			if (dragEnded)
				return null; //no update

			Point pointMUpdate = graphViewer.ScreenToGraph(graphViewer.PointToClient(Cursor.Position));
			Point pointNUpdate = pointMUpdate;
			newObjectLocation = pointMUpdate;

			//only snap to grid if grid exists and its a free link (not linking 2 existing objects)
			if ((SupplierElement == null || ConsumerElement == null) && graphViewer.Grid.ShowGrid && graphViewer.Grid.CurrentGridUnit > 0)
			{
				pointMUpdate = graphViewer.Grid.AlignToGrid(pointMUpdate);
				pointNUpdate = pointMUpdate;
				newObjectLocation = pointMUpdate;
			}

			if (SupplierElement != null)
				pointMUpdate = SupplierElement.GetOutputLineItemTab(Item).GetConnectionPoint();
			if (ConsumerElement != null)
				pointNUpdate = ConsumerElement.GetInputLineItemTab(Item).GetConnectionPoint();

			return new Tuple<Point,Point>(pointMUpdate, pointNUpdate);
		}
		protected override Tuple<NodeDirection, NodeDirection> GetEndpointDirections()
		{
			if (SupplierElement == null)
				return new Tuple<NodeDirection, NodeDirection>(graphViewer.Graph.DefaultNodeDirection, ConsumerElement.DisplayedNode.NodeDirection);
			if (ConsumerElement == null)
				return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, graphViewer.Graph.DefaultNodeDirection);

			return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, ConsumerElement.DisplayedNode.NodeDirection);
		}

		private void EndDrag(Point graph_point)
		{
			dragEnded = true;

			if (SupplierElement != null && ConsumerElement != null) //no nulls -> this is a 'link 2 nodes' operation
			{
				graphViewer.Graph.CreateLink(SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode, this.Item);

				graphViewer.Graph.UpdateNodeValues();
				graphViewer.UpdateGraphBounds();
				graphViewer.Invalidate();
				graphViewer.DisposeLinkDrag();
			}
			else //at least one null -> this is an 'add new recipe' operation
			{
				Point screenPoint = new Point(graphViewer.GraphToScreen(graph_point).X - 150, 15);
				screenPoint.X = Math.Max(15, Math.Min(graphViewer.Width - 650, screenPoint.X)); //want to position the recipe selector such that it is well visible.

				if (StartConnectionType == LinkType.Input && SupplierElement == null)
					graphViewer.AddRecipe(screenPoint, Item, newObjectLocation, NewNodeType.Supplier, ConsumerElement, true);
				else if (StartConnectionType == LinkType.Output && ConsumerElement == null)
					graphViewer.AddRecipe(screenPoint, Item, newObjectLocation, NewNodeType.Consumer, SupplierElement, true);
				else
					Trace.Fail("Both null dragged link!");
			}
		}

		public override void MouseDown(Point graph_point, MouseButtons button)
		{
			if (button == MouseButtons.Left)
				EndDrag(graph_point);
			else if (button == MouseButtons.Right) //cancel drag-link
				graphViewer.DisposeLinkDrag();
		}

		public override void MouseUp(Point graph_point, MouseButtons button, bool wasDragged)
		{
			if (button == MouseButtons.Left)
				EndDrag(graph_point);
		}

		public override void MouseMoved(Point graph_point)
		{
			if (dragEnded)
				return;

			BaseNodeElement mousedElement = graphViewer.GetNodeAtPoint(graph_point);
			if (mousedElement != null)
			{
				if (StartConnectionType == LinkType.Input && mousedElement.DisplayedNode.Outputs.Contains(Item))
					SupplierElement = mousedElement;
				else if (StartConnectionType == LinkType.Output && mousedElement.DisplayedNode.Inputs.Contains(Item))
					ConsumerElement = mousedElement;

				//if we have found a possible connection above (both supplier & consumer are no longer null), but the item temperature check fails, break connection
				if (SupplierElement != null &&
					ConsumerElement != null &&
					!LinkChecker.IsPossibleConnection(Item, SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode))
				{
					if (StartConnectionType == LinkType.Input)
						SupplierElement = null;
					else  //if(StartConnectionType == LinkType.Output)
						ConsumerElement = null;
				}
			}
			else //no node under mouse, break any previously established connections (ex:when mouse drag leaves a possible connection)
			{
				if (StartConnectionType == LinkType.Input)
					SupplierElement = null;
				else  //if(StartConnectionType == LinkType.Output)
					ConsumerElement = null;
			}
		}
	}
}
