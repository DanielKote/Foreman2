using Foreman.Graph;
using Foreman.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    public class DraggedLinkElement : BaseLinkElement {
        public override ItemQualityPair Item { get; protected set; }
        public LinkType StartConnectionType { get; private set; }
        public Point EndpointLocation { get; set; }

        private bool dragEnded;
        private readonly BaseNodeElement originElement;

        public DraggedLinkElement(ProductionGraphViewer graphViewer, BaseNodeElement startNode, LinkType startConnectionType, ItemQualityPair item) : base(graphViewer) {
            if (startNode == null)
                throw new ArgumentNullException(nameof(startNode), "Cant create a dragged link element with a null startNode!");
            originElement = startNode;
            Init(startConnectionType, item);
        }
        protected DraggedLinkElement(ProductionGraphViewer graphViewer, BaseNodeElement startNode, LinkType startConnectionType, ItemQualityPair item, DraggedLinkElement masterLink) : base(graphViewer, masterLink) {
            if (startNode == null)
                throw new ArgumentNullException(nameof(startNode), "Cant create a dragged link element with a null startNode!");
            originElement = startNode;
            Init(startConnectionType, item);
        }

        private void Init(LinkType startConnectionType, ItemQualityPair item) {
            if (startConnectionType == LinkType.Input)
                ConsumerElement = originElement;
            else
                SupplierElement = originElement;

            StartConnectionType = startConnectionType;
            Item = item;

            dragEnded = false;
        }


        public override void UpdateVisibility(Rectangle graphZone, int xborder, int yborder) { Visible = true; } //always visible.

        public override void PrePaint() {
            UpdateSlaveLinks();
            foreach (DraggedLinkElement slaveLink in SubElements.Where(e => e is DraggedLinkElement))
                slaveLink.LinkWidth = this.LinkWidth;
        }

        protected override Tuple<Point, Point>? GetCurveEndpoints() {
            if (dragEnded)
                return null; //no update

            Point supplierPoint = EndpointLocation;
            Point consumerPoint = EndpointLocation;
            if (SupplierElement != null)
                supplierPoint = iconOnlyDraw ? SupplierElement.Location : SupplierElement.GetOutputLineItemTab(Item).GetConnectionPoint();
            if (ConsumerElement != null)
                consumerPoint = iconOnlyDraw ? ConsumerElement.Location : ConsumerElement.GetInputLineItemTab(Item).GetConnectionPoint();

            return new Tuple<Point, Point>(supplierPoint, consumerPoint);
        }

        protected override Tuple<NodeDirection, NodeDirection> GetEndpointDirections() {

            if (SupplierElement == null) {
                if (ConsumerElement == null)
                    return new Tuple<NodeDirection, NodeDirection>(graphViewer.Graph.DefaultNodeDirection, graphViewer.Graph.DefaultNodeDirection);

                if (myParent is DraggedLinkElement masterLinkElement) {
                    Tuple<NodeDirection, NodeDirection> masterDirections = masterLinkElement.GetEndpointDirections();
                    return masterDirections.Item2 == ConsumerElement.ViewModel.NodeDirection
                        ? masterDirections
                        : new Tuple<NodeDirection, NodeDirection>(masterLinkElement.GetEndpointDirections().Item1 == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up, ConsumerElement.ViewModel.NodeDirection);
                }

                if (!graphViewer.SmartNodeDirection)
                    return new Tuple<NodeDirection, NodeDirection>(graphViewer.Graph.DefaultNodeDirection, ConsumerElement.ViewModel.NodeDirection);

                Point consumerPoint = iconOnlyDraw ? ConsumerElement.Location : ConsumerElement.GetInputLineItemTab(Item).GetConnectionPoint();
                return (ConsumerElement.ViewModel.NodeDirection == NodeDirection.Up && consumerPoint.Y > EndpointLocation.Y) || (ConsumerElement.ViewModel.NodeDirection == NodeDirection.Down && consumerPoint.Y < EndpointLocation.Y)
                    ? new Tuple<NodeDirection, NodeDirection>(ConsumerElement.ViewModel.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up, ConsumerElement.ViewModel.NodeDirection)
                    : new Tuple<NodeDirection, NodeDirection>(ConsumerElement.ViewModel.NodeDirection, ConsumerElement.ViewModel.NodeDirection);
            }
            if (ConsumerElement == null) {
                if (SupplierElement == null)
                    return new Tuple<NodeDirection, NodeDirection>(graphViewer.Graph.DefaultNodeDirection, graphViewer.Graph.DefaultNodeDirection);

                if (myParent is DraggedLinkElement masterLinkElement) {
                    Tuple<NodeDirection, NodeDirection> masterDirections = masterLinkElement.GetEndpointDirections();
                    return masterDirections.Item1 == SupplierElement.ViewModel.NodeDirection
                        ? masterDirections
                        : new Tuple<NodeDirection, NodeDirection>(SupplierElement.ViewModel.NodeDirection, masterLinkElement.GetEndpointDirections().Item2 == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up);
                }

                if (!graphViewer.SmartNodeDirection)
                    return new Tuple<NodeDirection, NodeDirection>(SupplierElement.ViewModel.NodeDirection, graphViewer.Graph.DefaultNodeDirection);

                Point supplierPoint = iconOnlyDraw ? SupplierElement.Location : SupplierElement.GetOutputLineItemTab(Item).GetConnectionPoint();
                return (SupplierElement.ViewModel.NodeDirection == NodeDirection.Up && supplierPoint.Y < EndpointLocation.Y) || (SupplierElement.ViewModel.NodeDirection == NodeDirection.Down && supplierPoint.Y > EndpointLocation.Y)
                    ? new Tuple<NodeDirection, NodeDirection>(SupplierElement.ViewModel.NodeDirection, SupplierElement.ViewModel.NodeDirection == NodeDirection.Up ? NodeDirection.Down : NodeDirection.Up)
                    : new Tuple<NodeDirection, NodeDirection>(SupplierElement.ViewModel.NodeDirection, SupplierElement.ViewModel.NodeDirection);
            }

            return new Tuple<NodeDirection, NodeDirection>(SupplierElement.ViewModel.NodeDirection, ConsumerElement.ViewModel.NodeDirection);
        }

        private void EndDrag(Point graphPoint) {
            dragEnded = true;

            if (SupplierElement != null && ConsumerElement != null) //no nulls -> this is a 'link 2 nodes' operation
            {
                graphViewer.Session.Editor.CreateLink(SupplierElement.ViewModel.Id, ConsumerElement.ViewModel.Id, this.Item);

                graphViewer.Graph.UpdateNodeValues();
                graphViewer.UpdateGraphBounds();
                graphViewer.Invalidate();
                graphViewer.DisposeLinkDrag();
            } else if (SubElements.Any(e => e is DraggedLinkElement)) //at least one null + sub-link -> this is an 'add new passthrough nodes operation
              {
                graphViewer.AddPassthroughNodesFromSelection(StartConnectionType, (Size)Point.Subtract(EndpointLocation, (Size)originElement.Location));
            } else //at least one null -> this is an 'add new recipe' operation
              {
                Point screenPoint = graphViewer.GraphToScreen(graphPoint);

                if (StartConnectionType == LinkType.Input && SupplierElement == null)
                    graphViewer.AddNewNode(screenPoint, Item, EndpointLocation, NewNodeType.Supplier, ConsumerElement, true);
                else if (StartConnectionType == LinkType.Output && ConsumerElement == null)
                    graphViewer.AddNewNode(screenPoint, Item, EndpointLocation, NewNodeType.Consumer, SupplierElement, true);
                else
                    Trace.Fail("Both null dragged link!");
            }
        }

        public override void MouseDown(Point graphPoint, MouseButtons button) {
            if (button == MouseButtons.Left)
                EndDrag(graphPoint);
            else if (button == MouseButtons.Right) //cancel drag-link
                graphViewer.DisposeLinkDrag();
        }

        public override void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) {
            if (button == MouseButtons.Left)
                EndDrag(graphPoint);
        }

        public override void MouseMoved(Point graphPoint) {
            if (dragEnded)
                return;

            BaseNodeElement? mousedElement = graphViewer.GetNodeAtPoint(graphPoint);
            if (mousedElement != null) {
                if (StartConnectionType == LinkType.Input && mousedElement.ViewModel.Outputs.Contains(Item))
                    SupplierElement = mousedElement;
                else if (StartConnectionType == LinkType.Output && mousedElement.ViewModel.Inputs.Contains(Item))
                    ConsumerElement = mousedElement;

                //if we have found a possible connection above (both supplier & consumer are no longer null), but the item temperature check fails, break connection
                if (SupplierElement != null &&
                    ConsumerElement != null &&
                    !LinkChecker.IsPossibleConnection(Item, SupplierElement.ViewModel, ConsumerElement.ViewModel, graphViewer.Session)) {
                    if (StartConnectionType == LinkType.Input)
                        SupplierElement = null;
                    else  //if(StartConnectionType == LinkType.Output)
                        ConsumerElement = null;
                }

                if (SupplierElement != null && ConsumerElement != null && SubElements.Any(e => e is DraggedLinkElement))
                    foreach (DraggedLinkElement link in SubElements.Where(e => e is DraggedLinkElement).ToList())
                        link.Dispose();
            } else //no node under mouse, break any previously established connections (ex:when mouse drag leaves a possible connection)
              {
                if (StartConnectionType == LinkType.Input)
                    SupplierElement = null;
                else  //if(StartConnectionType == LinkType.Output)
                    ConsumerElement = null;
            }
            UpdateEndpoint();
        }

        private void UpdateSlaveLinks() {
            if (SupplierElement == null || ConsumerElement == null) {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control && !SubElements.Any(e => e is DraggedLinkElement) && originElement is PassthroughNodeElement && graphViewer.SelectedNodes.Count > 1 && graphViewer.SelectedNodes.Contains(originElement) && !graphViewer.SelectedNodes.Any(e => !(e is PassthroughNodeElement)))
                    foreach (PassthroughNodeElement node in graphViewer.SelectedNodes.Where(e => e != originElement)) {
                        var dle = new DraggedLinkElement(graphViewer, node, StartConnectionType, ((IPassthroughNodeViewModel)node.ViewModel).PassthroughItem, this);
                        try {
                            if (SubElements.Contains(dle))
                                dle = null;
                        } finally {
                            dle?.Dispose();
                        }
                    }
                else if ((Control.ModifierKeys & Keys.Control) != Keys.Control)
                    foreach (DraggedLinkElement link in SubElements.Where(e => e is DraggedLinkElement).ToList())
                        link.Dispose();
                UpdateEndpoint();
            }
        }

        private void UpdateEndpoint() {
            EndpointLocation = graphViewer.ScreenToGraph(graphViewer.PointToClient(Cursor.Position));
            if (graphViewer.Grid.ShowGrid && graphViewer.Grid.CurrentGridUnit > 0)
                EndpointLocation = graphViewer.Grid.AlignToGrid(EndpointLocation);

            if (SubElements.Any(e => e is DraggedLinkElement)) {
                foreach (DraggedLinkElement slaveLink in SubElements.Where(e => e is DraggedLinkElement)) {
                    BaseNodeElement? anchor = StartConnectionType == LinkType.Input ? slaveLink.ConsumerElement : slaveLink.SupplierElement;
                    if (anchor != null)
                        slaveLink.EndpointLocation = Point.Add(anchor.Location, (Size)Point.Subtract(EndpointLocation, (Size)originElement.Location));
                }
            }
        }
    }
}
