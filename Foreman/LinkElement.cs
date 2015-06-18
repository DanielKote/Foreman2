using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Foreman
{
	class LinkElement : GraphElement
	{
		public NodeLink DisplayedLink { get; private set; }
		public ProductionNode Supplier { get { return DisplayedLink.Supplier; } }
		public ProductionNode Consumer { get { return DisplayedLink.Consumer; } }
		public NodeElement SupplierElement { get { return Parent.GetElementForNode(Supplier); } }
		public NodeElement ConsumerElement { get { return Parent.GetElementForNode(Consumer); } }
		public Item Item { get { return DisplayedLink.Item; } }

		public override Point Location
		{
			get { return new Point(); }
			set { }
		}
		public override int X
		{
			get { return 0; }
			set { }
		}
		public override int Y
		{
			get { return 0; }
			set { }
		}
		public override Point Size
		{
			get { return new Point(); }
			set { }
		}
		public override int Width
		{
			get { return 0; }
			set { }
		}
		public override int Height
		{
			get { return 0; }
			set { }
		}

		public LinkElement(ProductionGraphViewer parent, NodeLink displayedLink)
			: base(parent)
		{
			DisplayedLink = displayedLink;
		}

		public override void Paint(Graphics graphics)
		{
			Point pointN = SupplierElement.GetOutputLineConnectionPoint(Item);
			Point pointM = ConsumerElement.GetInputLineConnectionPoint(Item);
			Point pointN2 = new Point(pointN.X, pointN.Y - Math.Max((int)((pointN.Y - pointM.Y) / 2), 40));
			Point pointM2 = new Point(pointM.X, pointM.Y + Math.Max((int)((pointN.Y - pointM.Y) / 2), 40));

			using (Pen pen = new Pen(DataCache.IconAverageColour(Item.Icon), 3f))
			{
				graphics.DrawBezier(pen, pointN, pointN2, pointM2, pointM);
			}
		}
	}
}
