using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Diagnostics;

namespace Foreman
{
	public class LinkElement : BaseLinkElement
	{
		public NodeLink DisplayedLink { get; private set; }
		public override Item Item { get { return DisplayedLink.Item; } protected set { } }

		public ItemTabElement SupplierTab { get; protected set; }
		public ItemTabElement ConsumerTab { get; protected set; }

		public LinkElement(ProductionGraphViewer graphViewer, NodeLink displayedLink, NodeElement supplierElement, NodeElement consumerElement) : base(graphViewer)
		{
			if (supplierElement == null || consumerElement == null)
				Trace.Fail("Link element being created with one of the connected elements being null!");

			DisplayedLink = displayedLink;
			SupplierElement = supplierElement;
			ConsumerElement = consumerElement;
			SupplierTab = supplierElement.GetOutputLineItemTab(Item);
			ConsumerTab = consumerElement.GetInputLineItemTab(Item);

			if (SupplierTab == null || ConsumerTab == null)
				Trace.Fail(string.Format("Link element being created with one of the elements ({0}, {1}) not having the required item ({2})!", supplierElement, consumerElement, Item));

			LinkWidth = 3f;
			UpdateCurve();
		}

		protected override Point[] GetCurveEndpoints()
		{
			Point pointMupdate = ConsumerTab.GetConnectionPoint();
			Point pointNupdate = SupplierTab.GetConnectionPoint();
			return new Point[] { pointMupdate, pointNupdate };
		}
	}
}
