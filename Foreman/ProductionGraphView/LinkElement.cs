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
		public NodeElement SupplierElement { get; private set; }
		public NodeElement ConsumerElement { get; private set; }
		public ItemTab SupplierTab { get; private set; }
		public ItemTab ConsumerTab { get; private set; }
		public Item Item { get { return DisplayedLink.Item; } }

		private Point pointM, pointM2;
		private Point pointN, pointN2;
		private Point pointMidM, pointMidA, pointMidB, pointMidN;
		private bool linkingUp;
		public float LinkWidth { get; set; }

		public Rectangle CalculatedBounds { get; private set; }

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

		public LinkElement(ProductionGraphViewer parent, NodeLink displayedLink, NodeElement supplierElement, NodeElement consumerElement)
			: base(parent)
		{
			DisplayedLink = displayedLink;
			SupplierElement = supplierElement;
			ConsumerElement = consumerElement;
			SupplierTab = supplierElement.GetOutputLineItemTab(Item);
			ConsumerTab = consumerElement.GetInputLineItemTab(Item);
			LinkWidth = 3f;
			UpdateCurve();
		}

		public override void UpdateVisibility(Rectangle bounds, int xborder, int yborder)
		{
			UpdateCurve();
			Visible =
					 	CalculatedBounds.X + CalculatedBounds.Width > bounds.X - xborder &&
						CalculatedBounds.X < bounds.X + bounds.Width + xborder &&
						CalculatedBounds.Y + CalculatedBounds.Height > bounds.Y - yborder &&
						CalculatedBounds.Y < bounds.Y + bounds.Height + yborder;
		}

		public void UpdateCurve() //updates all points & boundaries (important for occluding objects outside view)
        {
			Point pointNupdate = (SupplierTab is null) ? new Point(SupplierElement.X + Width / 2, Y) : new Point(SupplierElement.X + SupplierTab.X + SupplierTab.Width / 2, SupplierElement.Y + SupplierTab.Y);
			Point pointMupdate = (ConsumerTab is null) ? new Point(ConsumerElement.X + Width / 2, Y + ConsumerElement.Height) : new Point(ConsumerElement.X + ConsumerTab.X + ConsumerTab.Width / 2, ConsumerElement.Y + ConsumerTab.Y + ConsumerTab.Height);

			if (pointNupdate != pointN || pointMupdate != pointM)
			{
				pointN = pointNupdate;
				pointM = pointMupdate;

				linkingUp = (pointN.Y > pointM.Y);
				if (linkingUp)
				{
					//directLine = (pointN.X == pointM.X || (Math.Abs(pointN.X - pointM.X) + Math.Abs(pointN.Y - pointM.Y) < 40)); //just in case we want to draw it as a straight
					pointN2 = new Point(pointN.X, pointN.Y - Math.Max((int)((pointN.Y - pointM.Y) / 2), 20));
					pointM2 = new Point(pointM.X, pointM.Y + Math.Max((int)((pointN.Y - pointM.Y) / 2), 20));

					CalculatedBounds = new Rectangle(
						Math.Min(pointM.X, pointN.X),
						pointM.Y,
						Math.Abs(pointM.X - pointN.X),
						pointN.Y - pointM.Y
						);
				}
				else
				{
					int midX = Math.Abs(pointN.X - pointM.X) > 200 ? (pointN.X + pointM.X) / 2 : pointN.X > pointM.X ? pointN.X + 150 : pointN.X - 150;

					pointMidA = new Point(midX, pointN.Y);
					pointMidB = new Point(midX, pointM.Y);


					pointN2 = new Point(pointN.X, pointN.Y - 120);
					pointMidN = new Point(midX, pointN.Y - 120);

					pointM2 = new Point(pointM.X, pointM.Y + 120);
					pointMidM = new Point(midX, pointM.Y + 120);

					CalculatedBounds = new Rectangle(
						Math.Min(pointM.X, pointN.X),
						pointN2.Y,
						Math.Abs(pointM.X - pointN.X),
						pointM2.Y - pointN2.Y
						);
				}
			}
		}

		public override void Paint(Graphics graphics, Point trans)
		{
			if (Visible)
			{
				UpdateCurve();

				using (Pen pen = new Pen(Item.AverageColor, LinkWidth))
				{
					if (linkingUp)
						graphics.DrawBeziers(pen, new Point[] {
							PT(PT(pointN,trans), 0, 10),
							PT(PT(pointN,trans), 0, 10),
							PT(pointN,trans),
							PT(pointN,trans),
							PT(pointN2,trans),
							PT(pointM2, trans),
							PT(pointM,trans),
							PT(pointM,trans),
							PT(PT(pointM,trans), 0, -10),
							PT(PT(pointM,trans), 0, -10) });
					else
						graphics.DrawBeziers(pen, new Point[]{
							PT(pointN, trans),
							PT(pointN2, trans),
							PT(pointMidN, trans),
							PT(pointMidA, trans),
							PT(pointMidA, trans),
							PT(pointMidB, trans),
							PT(pointMidB, trans),
							PT(pointMidM, trans),
							PT(pointM2, trans),
							PT(pointM, trans) });
				}
			}
		}
	}
}
