using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public class DraggedLinkElement : BaseLinkElement {
        public override ItemQualityPair Item { get; protected set; }
        public LinkType StartConnectionType { get; private set; }
        public Point EndpointLocation { get; set; }

        private bool _dragEnded;
        private BaseNodeElement _originElement;

        public DraggedLinkElement(ProductionGraphViewer graphViewer, BaseNodeElement startNode, LinkType startConnectionType, ItemQualityPair item) :
            base(graphViewer) {
            Init(graphViewer, startNode, startConnectionType, item);
        }

        protected DraggedLinkElement(ProductionGraphViewer graphViewer, BaseNodeElement startNode, LinkType startConnectionType, ItemQualityPair item,
            DraggedLinkElement masterLink) : base(graphViewer, masterLink) {
            Init(graphViewer, startNode, startConnectionType, item);
        }

        protected void Init(ProductionGraphViewer graphViewer, BaseNodeElement startNode, LinkType startConnectionType, ItemQualityPair item) {
            if (startNode == null)
                Trace.Fail("Cant create a dragged link element with a null startNode!");

            _originElement = startNode;
            if (startConnectionType == LinkType.Input)
                ConsumerElement = startNode;
            else
                SupplierElement = startNode;

            StartConnectionType = startConnectionType;
            Item = item;

            _dragEnded = false;
        }


        public override void UpdateVisibility(Rectangle graphZone, int xBorder, int yBorder) {
            Visible = true;
        } //always visible.

        public override void PrePaint() {
            UpdateSlaveLinks();
            foreach (DraggedLinkElement slaveLink in SubElements.Where(e => e is DraggedLinkElement))
                slaveLink.LinkWidth = LinkWidth;
        }

        protected override Tuple<Point, Point> GetCurveEndpoints() {
            if (_dragEnded)
                return null; //no update

            var supplierPoint = EndpointLocation;
            var consumerPoint = EndpointLocation;
            if (SupplierElement != null)
                supplierPoint = IconOnlyDraw ? SupplierElement.Location : SupplierElement.GetOutputLineItemTab(Item).GetConnectionPoint();
            if (ConsumerElement != null)
                consumerPoint = IconOnlyDraw ? ConsumerElement.Location : ConsumerElement.GetInputLineItemTab(Item).GetConnectionPoint();

            return new Tuple<Point, Point>(supplierPoint, consumerPoint);
        }

        protected override Tuple<NodeDirection, NodeDirection> GetEndpointDirections() {
            if (SupplierElement == null) {
                if (MyParent is DraggedLinkElement masterLinkElement) {
                    var masterDirections = masterLinkElement.GetEndpointDirections();
                    if (masterDirections.Item2 == ConsumerElement.DisplayedNode.NodeDirection)
                        return masterDirections;
                    return new Tuple<NodeDirection, NodeDirection>(
                        masterLinkElement.GetEndpointDirections().Item1 == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up,
                        ConsumerElement.DisplayedNode.NodeDirection);
                }

                if (!GraphViewer.SmartNodeDirection)
                    return new Tuple<NodeDirection, NodeDirection>(GraphViewer.Graph.DefaultNodeDirection, ConsumerElement.DisplayedNode.NodeDirection);

                var consumerPoint = IconOnlyDraw ? ConsumerElement.Location : ConsumerElement.GetInputLineItemTab(Item).GetConnectionPoint();
                if ((ConsumerElement.DisplayedNode.NodeDirection == NodeDirection.Up && consumerPoint.Y > EndpointLocation.Y) ||
                    (ConsumerElement.DisplayedNode.NodeDirection == NodeDirection.Down && consumerPoint.Y < EndpointLocation.Y))
                    return new Tuple<NodeDirection, NodeDirection>(
                        ConsumerElement.DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up,
                        ConsumerElement.DisplayedNode.NodeDirection);
                return new Tuple<NodeDirection, NodeDirection>(ConsumerElement.DisplayedNode.NodeDirection, ConsumerElement.DisplayedNode.NodeDirection);
            }

            if (ConsumerElement == null) {
                if (MyParent is DraggedLinkElement masterLinkElement) {
                    var masterDirections = masterLinkElement.GetEndpointDirections();
                    if (masterDirections.Item1 == SupplierElement.DisplayedNode.NodeDirection)
                        return masterDirections;
                    return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection,
                        masterLinkElement.GetEndpointDirections().Item2 == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
                }

                if (!GraphViewer.SmartNodeDirection)
                    return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, GraphViewer.Graph.DefaultNodeDirection);

                var supplierPoint = IconOnlyDraw ? SupplierElement.Location : SupplierElement.GetOutputLineItemTab(Item).GetConnectionPoint();
                if ((SupplierElement.DisplayedNode.NodeDirection == NodeDirection.Up && supplierPoint.Y < EndpointLocation.Y) ||
                    (SupplierElement.DisplayedNode.NodeDirection == NodeDirection.Down && supplierPoint.Y > EndpointLocation.Y))
                    return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection,
                        SupplierElement.DisplayedNode.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
                return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, SupplierElement.DisplayedNode.NodeDirection);
            }

            return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, ConsumerElement.DisplayedNode.NodeDirection);
        }

        private void EndDrag(Point graphPoint) {
            _dragEnded = true;

            if (SupplierElement != null && ConsumerElement != null) { // no nulls -> this is a 'link 2 nodes' operation
                GraphViewer.Graph.CreateLink(SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode, Item);

                GraphViewer.Graph.UpdateNodeValues();
                GraphViewer.UpdateGraphBounds();
                GraphViewer.Invalidate();
                GraphViewer.DisposeLinkDrag();
            } else if (SubElements.Any(e => e is DraggedLinkElement)) { // at least one null + sub-link -> this is an 'add new passthrough nodes operation
                GraphViewer.AddPassthroughNodesFromSelection(StartConnectionType, (Size) Point.Subtract(EndpointLocation, (Size) _originElement.Location));
            } else { // at least one null -> this is an 'add new recipe' operation
                var screenPoint = new Point(GraphViewer.GraphToScreen(graphPoint).X - 150, 15);

                // want to position the recipe selector such that it is well visible.
                screenPoint.X = Math.Max(15, Math.Min(GraphViewer.Width - 650, screenPoint.X));

                if (StartConnectionType == LinkType.Input && SupplierElement == null)
                    GraphViewer.AddNewNode(screenPoint, Item, EndpointLocation, NewNodeType.Supplier, ConsumerElement, true);
                else if (StartConnectionType == LinkType.Output && ConsumerElement == null)
                    GraphViewer.AddNewNode(screenPoint, Item, EndpointLocation, NewNodeType.Consumer, SupplierElement, true);
                else
                    Trace.Fail("Both null dragged link!");
            }
        }

        public override void MouseDown(Point graphPoint, MouseButtons button) {
            if (button == MouseButtons.Left)
                EndDrag(graphPoint);
            else if (button == MouseButtons.Right) // cancel drag-link
                GraphViewer.DisposeLinkDrag();
        }

        public override void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) {
            if (button == MouseButtons.Left)
                EndDrag(graphPoint);
        }

        public override void MouseMoved(Point graphPoint) {
            if (_dragEnded)
                return;

            var mousedElement = GraphViewer.GetNodeAtPoint(graphPoint);
            if (mousedElement != null) {
                if (StartConnectionType == LinkType.Input && mousedElement.DisplayedNode.Outputs.Contains(Item))
                    SupplierElement = mousedElement;
                else if (StartConnectionType == LinkType.Output && mousedElement.DisplayedNode.Inputs.Contains(Item))
                    ConsumerElement = mousedElement;

                // if we have found a possible connection above (both supplier & consumer are no longer null),
                // but the item temperature check fails, break connection

                if (SupplierElement != null &&
                    ConsumerElement != null &&
                    !LinkChecker.IsPossibleConnection(Item, SupplierElement.DisplayedNode, ConsumerElement.DisplayedNode)) {
                    if (StartConnectionType == LinkType.Input)
                        SupplierElement = null;
                    else //if(StartConnectionType == LinkType.Output)
                        ConsumerElement = null;
                }

                if (SupplierElement != null && ConsumerElement != null && SubElements.Any(e => e is DraggedLinkElement))
                    foreach (DraggedLinkElement link in SubElements.Where(e => e is DraggedLinkElement).ToList())
                        link.Dispose();
            } else { // no node under mouse, break any previously established connections (ex:when mouse drag leaves a possible connection)
                if (StartConnectionType == LinkType.Input)
                    SupplierElement = null;
                else //if(StartConnectionType == LinkType.Output)
                    ConsumerElement = null;
            }

            UpdateEndpoint();
        }

        private void UpdateSlaveLinks() {
            if (SupplierElement != null && ConsumerElement != null)
                return;

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control && !SubElements.Any(e => e is DraggedLinkElement) &&
                _originElement is PassthroughNodeElement && GraphViewer.SelectedNodes.Count > 1 && GraphViewer.SelectedNodes.Contains(_originElement) &&
                !GraphViewer.SelectedNodes.Any(e => e is not PassthroughNodeElement))
                foreach (PassthroughNodeElement node in GraphViewer.SelectedNodes.Where(e => e != _originElement))
                    new DraggedLinkElement(GraphViewer, node, StartConnectionType, ((ReadOnlyPassthroughNode) node.DisplayedNode).PassthroughItem, this);
            else if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
                foreach (DraggedLinkElement link in SubElements.Where(e => e is DraggedLinkElement).ToList())
                    link.Dispose();
            UpdateEndpoint();
        }

        private void UpdateEndpoint() {
            EndpointLocation = GraphViewer.ScreenToGraph(GraphViewer.PointToClient(Cursor.Position));
            if (GraphViewer.Grid.ShowGrid && GraphViewer.Grid.CurrentGridUnit > 0)
                EndpointLocation = GraphViewer.Grid.AlignToGrid(EndpointLocation);

            if (!SubElements.Any(e => e is DraggedLinkElement))
                return;

            foreach (DraggedLinkElement slaveLink in SubElements.Where(e => e is DraggedLinkElement)) {
                slaveLink.EndpointLocation = Point.Add(
                    (StartConnectionType == LinkType.Input
                        ? slaveLink.ConsumerElement
                        : slaveLink.SupplierElement).Location,
                    (Size) Point.Subtract(EndpointLocation,
                        (Size) _originElement.Location));
            }
        }
    }
}