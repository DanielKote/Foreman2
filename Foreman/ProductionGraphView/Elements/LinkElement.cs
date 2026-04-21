using System;
using System.Diagnostics;
using System.Drawing;

namespace Foreman {
    public class LinkElement : BaseLinkElement {
        public ReadOnlyNodeLink DisplayedLink { get; private set; }

        public override ItemQualityPair Item {
            get => DisplayedLink.Item;
            protected set { }
        }

        public ItemTabElement SupplierTab { get; protected set; }
        public ItemTabElement ConsumerTab { get; protected set; }

        public LinkElement(ProductionGraphViewer graphViewer, ReadOnlyNodeLink displayedLink, BaseNodeElement supplierElement,
            BaseNodeElement consumerElement) : base(graphViewer) {
            if (supplierElement == null || consumerElement == null)
                Trace.Fail("Link element being created with one of the connected elements being null!");

            DisplayedLink = displayedLink;
            SupplierElement = supplierElement;
            ConsumerElement = consumerElement;
            SupplierTab = supplierElement.GetOutputLineItemTab(Item);
            ConsumerTab = consumerElement.GetInputLineItemTab(Item);

            if (SupplierTab == null || ConsumerTab == null)
                Trace.Fail(
                    $"Link element being created with one of the elements ({supplierElement}, {consumerElement}) not having the required item ({Item})!");

            LinkWidth = 3f;
            UpdateCurve();
        }

        protected override Tuple<Point, Point> GetCurveEndpoints() {
            return new Tuple<Point, Point>(IconOnlyDraw ? SupplierElement.Location : SupplierTab.GetConnectionPoint(),
                IconOnlyDraw ? ConsumerElement.Location : ConsumerTab.GetConnectionPoint());
        }

        protected override Tuple<NodeDirection, NodeDirection> GetEndpointDirections() {
            return new Tuple<NodeDirection, NodeDirection>(SupplierElement.DisplayedNode.NodeDirection, ConsumerElement.DisplayedNode.NodeDirection);
        }
    }
}