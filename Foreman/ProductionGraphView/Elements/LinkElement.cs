using Foreman.Graph;
using Foreman.Models;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Foreman.ProductionGraphView.Elements {
    public class LinkElement : BaseLinkElement {
        public INodeLinkViewModel ViewModel { get; private set; }
        public override ItemQualityPair Item { get { return ViewModel.Item; } protected set { } }

        public ItemTabElement SupplierTab { get; protected set; }
        public ItemTabElement ConsumerTab { get; protected set; }

        public LinkElement(ProductionGraphViewer graphViewer, INodeLinkViewModel viewModel, BaseNodeElement supplierElement, BaseNodeElement consumerElement) : base(graphViewer) {
            if (supplierElement == null || consumerElement == null)
                Trace.Fail("Link element being created with one of the connected elements being null!");

            ViewModel = viewModel;
            SupplierElement = supplierElement;
            ConsumerElement = consumerElement;
            ItemTabElement? supplierTab = supplierElement.GetOutputLineItemTab(Item);
            ItemTabElement? consumerTab = consumerElement.GetInputLineItemTab(Item);
            if (supplierTab == null || consumerTab == null)
                throw new InvalidOperationException(string.Format(DisplayCulture.Format, "Link element being created with one of the elements ({0}, {1}) not having the required item ({2})!", supplierElement, consumerElement, Item));
            SupplierTab = supplierTab;
            ConsumerTab = consumerTab;

            LinkWidth = 3f;
            UpdateCurve();
        }

        protected override Tuple<Point, Point> GetCurveEndpoints() {
            return SupplierElement is null || ConsumerElement is null || SupplierTab is null || ConsumerTab is null
                ? throw new InvalidOperationException("Link element is missing a connected node or item tab.")
                : new Tuple<Point, Point>(iconOnlyDraw ? SupplierElement.Location : SupplierTab.GetConnectionPoint(), iconOnlyDraw ? ConsumerElement.Location : ConsumerTab.GetConnectionPoint());
        }
        protected override Tuple<NodeDirection, NodeDirection> GetEndpointDirections() {
            return SupplierElement is null || ConsumerElement is null
                ? throw new InvalidOperationException("Link element is missing a connected node.")
                : new Tuple<NodeDirection, NodeDirection>(SupplierElement.ViewModel.NodeDirection, ConsumerElement.ViewModel.NodeDirection);
        }
    }
}
