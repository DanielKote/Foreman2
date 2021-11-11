using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace Foreman
{
	public class DraggedLinkElement : BaseLinkElement
	{
		public override Item Item { get; protected set; }
		public LinkType StartConnectionType { get; private set; }
		public Point EndpointLocation { get; set; }

		private bool dragEnded;
		private BaseNodeElement originElement;

		public DraggedLinkElement(ProductionGraphViewer graphViewer, BaseNodeElement startNode, LinkType startConnectionType, Item item) : base(graphViewer) { Init(graphViewer, startNode, startConnectionType, item); }
		protected DraggedLinkElement(ProductionGraphViewer graphViewer, BaseNodeElement startNode, LinkType startConnectionType, Item item, DraggedLinkElement masterLink) : base(graphViewer, masterLink) { Init(graphViewer, startNode, startConnectionType, item); }

		protected void Init(ProductionGraphViewer graphViewer, BaseNodeElement startNode, LinkType startConnectionType, Item item)
		{
			if (startNode == null)
				Trace.Fail("Cant create a dragged link element with a null startNode!");

			originElement = startNode;
			if (startConnectionType == LinkType.Input)
				ConsumerElement = startNode;
			else
				SupplierElement = startNode;

			StartConnectionType = startConnectionType;
			Item = item;

			dragEnded = false;
		}


		public override void UpdateVisibility(Rectangle graph_zone, int xborder, int yborder) { Visible = true; } //always visible.

		public override void PrePaint()
		{
			UpdateSlaveLinks();
			foreach (DraggedLinkElement slaveLink in SubElements.Where(e => e is DraggedLinkElement))
				slaveLink.LinkWidth = this.LinkWidth;
		}

		protected override Tuple<Point,Point> GetCurveEndpoints()
		{
			if (dragEnded)
				return null; //no update

			Point supplierPoint = EndpointLocation;
			Point consumerPoint = EndpointLocation;
			if(SupplierElement != null)
				supplierPoint = iconOnlyDraw ? SupplierElement.Location : SupplierElement.GetOutputLineItemTab(Item).GetConnectionPoint();
			if(ConsumerElement != null)
				consumerPoint = iconOnlyDraw ? ConsumerElement.Location : ConsumerElement.GetInputLineItemTab(Item).GetConnectionPoint();

			return new Tuple<Point, Point>(supplierPoint, consumerPoint);
		}

		protected override Tuple<NodeDirection, NodeDirection> GetEndpointDirections()
		{

			if (SupplierElement == null)
			{
				if (myParent is DraggedLinkElement masterLinkElement)
				{
					Tuple<NodeDirection, NodeDirection> masterDirections = masterLinkElement.GetEndpointDirections();
					if (masterDirections.Item2 == ConsumerElement.DisplayedNode.NodeDirection)
						return masterDirections;
					return new Tuple<NodeDirection, NodeDirection>(masterLinkElement.GetEndpointDirections().Item1 == NodeDirection.Up? NodeDirection.Down : NodeDirection.Up, ConsumerElement.DisplayedNode.NodeDirection);
				}

				if (!graphViewer.SmartNodeDirection)
					return new Tuple<NodeDirection, NodeDirection>(graphViewer.Graph.DefaultNodeDirection, ConsumerElement.DisplayedNode.NodeDirection);

				Point consumerPoint = iconOnlyDraw ? ConsumerElement.Location : ConsumerElement.GetInputLineItemTab(Item).GetConnectionPoint();
				if ((ConsumerElement.DisplayedNode.NodeDirection == NodeDirection.Up && consumerPoint.Y > EndpointLocation.Y) || (ConsumerElement.DisplayedNode.NodeDirection == NodeDirection.Down && consumerPoint.Y < EndpointLocation.Y))
					return new Tuple<NodeDirection, NodeDirection>(ConsumerElement.DisplayedNode.NodeDirection == NodeDirection.Up? NodeDirection.Down : NodeDirection.Up, ConsumerElement.DisplayedNode.NodeDirection);
				return new Tuple<NodeDirection, NodeDirection>(ConsumerElement.DisplayedNode.NodeDirection, ConsumerElement.DisplayedNode.NodeDirection);
			}
			if (ConsumerElement == null)
			{
				if (myParent is DraggedLinkElement masterLinkElement)
				{
					Tuple<NodeDirection, NodeDirection> masterDirections = masterLinkElement.GetEndpointDirections();
					if (masterDirections.Item1 == SupplierElement.DisplayedNode.NodeDirection)
						return masterDirections;
					return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, masterLinkElement.GetEndpointDirections().Item2 == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
				}

				if (!graphViewer.SmartNodeDirection)
					return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, graphViewer.Graph.DefaultNodeDirection);

				Point supplierPoint = iconOnlyDraw ? SupplierElement.Location : SupplierElement.GetOutputLineItemTab(Item).GetConnectionPoint();
				if ((SupplierElement.DisplayedNode.NodeDirection == NodeDirection.Up && supplierPoint.Y < EndpointLocation.Y) || (SupplierElement.DisplayedNode.NodeDirection == NodeDirection.Down && supplierPoint.Y > EndpointLocation.Y))
					return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, SupplierElement.DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
				return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, SupplierElement.DisplayedNode.NodeDirection);
			}

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
			else if(SubElements.Any(e => e is DraggedLinkElement)) //at least one null + sub-link -> this is an 'add new passthrough nodes operation
			{
				graphViewer.AddPassthroughNodesFromSelection(StartConnectionType, (Size)Point.Subtract(EndpointLocation, (Size)originElement.Location));
			}
			else //at least one null -> this is an 'add new recipe' operation
			{
				Point screenPoint = new Point(graphViewer.GraphToScreen(graph_point).X - 150, 15);
				screenPoint.X = Math.Max(15, Math.Min(graphViewer.Width - 650, screenPoint.X)); //want to position the recipe selector such that it is well visible.

				if (StartConnectionType == LinkType.Input && SupplierElement == null)
					graphViewer.AddRecipe(screenPoint, Item, EndpointLocation, NewNodeType.Supplier, ConsumerElement, true);
				else if (StartConnectionType == LinkType.Output && ConsumerElement == null)
					graphViewer.AddRecipe(screenPoint, Item, EndpointLocation, NewNodeType.Consumer, SupplierElement, true);
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

				if(SupplierElement != null && ConsumerElement != null && SubElements.Any(e => e is DraggedLinkElement))
					foreach (DraggedLinkElement link in SubElements.Where(e => e is DraggedLinkElement).ToList())
						link.Dispose();
			}
			else //no node under mouse, break any previously established connections (ex:when mouse drag leaves a possible connection)
			{
				if (StartConnectionType == LinkType.Input)
					SupplierElement = null;
				else  //if(StartConnectionType == LinkType.Output)
					ConsumerElement = null;
			}
			UpdateEndpoint();
		}

		private void UpdateSlaveLinks()
		{
			if (SupplierElement == null || ConsumerElement == null)
			{
				if ((Control.ModifierKeys & Keys.Control) == Keys.Control && !SubElements.Any(e => e is DraggedLinkElement) && originElement is PassthroughNodeElement && graphViewer.SelectedNodes.Count > 1 && graphViewer.SelectedNodes.Contains(originElement) && !graphViewer.SelectedNodes.Any(e => !(e is PassthroughNodeElement)))
					foreach (PassthroughNodeElement node in graphViewer.SelectedNodes.Where(e => e != originElement))
						new DraggedLinkElement(graphViewer, node, StartConnectionType, ((ReadOnlyPassthroughNode)node.DisplayedNode).PassthroughItem, this);
				else if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
					foreach (DraggedLinkElement link in SubElements.Where(e => e is DraggedLinkElement).ToList())
						link.Dispose();
				UpdateEndpoint();
			}
		}

		private void UpdateEndpoint()
		{
			EndpointLocation = graphViewer.ScreenToGraph(graphViewer.PointToClient(Cursor.Position));
			if (graphViewer.Grid.ShowGrid && graphViewer.Grid.CurrentGridUnit > 0)
				EndpointLocation = graphViewer.Grid.AlignToGrid(EndpointLocation);

			if (SubElements.Any(e => e is DraggedLinkElement))
			{
				foreach (DraggedLinkElement slaveLink in SubElements.Where(e => e is DraggedLinkElement))
					slaveLink.EndpointLocation = Point.Add((StartConnectionType == LinkType.Input ? slaveLink.ConsumerElement : slaveLink.SupplierElement).Location, (Size)Point.Subtract(EndpointLocation, (Size)originElement.Location));
			}
		}
	}
}
