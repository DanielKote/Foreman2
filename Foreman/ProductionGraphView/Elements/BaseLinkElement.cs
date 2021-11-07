using System;
using System.Drawing;

namespace Foreman
{
	public abstract class BaseLinkElement : GraphElement
	{
		public BaseNodeElement SupplierElement { get; protected set; }
		public BaseNodeElement ConsumerElement { get; protected set; }
		public virtual Item Item { get; protected set; }

		private Point pointM, pointN;
		private Point pointM2, pointN2;
		private Point pointMidM, pointMidA, pointMidB, pointMidN;
		private bool linkingUp;
		public float LinkWidth { get; set; }

		public Rectangle CalculatedBounds { get; private set; }

		public override Point Location //link elements are always considered to be located at 0,0 graph to simplify things, with their connection points being in graph-coordinates (no need to do any local transforms)
		{
			get { return new Point(); }
			set { }
		}
		public override int X { get { return 0; } set { } }
		public override int Y { get { return 0; } set { } }

		public BaseLinkElement(ProductionGraphViewer graphViewer) : base(graphViewer)
		{
			LinkWidth = 3f;
		}

		public override void UpdateVisibility(Rectangle graph_zone, int xborder, int yborder)
		{
			//NOTE: link element works in graph coordinates throughout (since Location is 0,0 for it - and it is always owned directly by the graph viewer). So we dont have to bother with graph to local conversions
			UpdateCurve();

			if (!linkingUp)
			{
				xborder += 200;
				yborder += 200;
			}

			Visible =
						 CalculatedBounds.X + CalculatedBounds.Width > graph_zone.X - xborder &&
						CalculatedBounds.X < graph_zone.X + graph_zone.Width + xborder &&
						CalculatedBounds.Y + CalculatedBounds.Height > graph_zone.Y - yborder &&
						CalculatedBounds.Y < graph_zone.Y + graph_zone.Height + yborder;
		}

		protected abstract Point[] GetCurveEndpoints();

		protected void UpdateCurve() //updates all points & boundaries (important for occluding objects outside view)
		{
			Point[] endpoints = GetCurveEndpoints();
			if (endpoints == null || endpoints.Length != 2)
				return;

			if (endpoints[0] != pointM || endpoints[1] != pointN)
			{
				pointM = endpoints[0];
				pointN = endpoints[1];

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
		public override bool ContainsPoint(Point graph_point)
		{
			return false;
		}

		protected override void Draw(Graphics graphics, bool simple)
		{
			UpdateCurve();

			using (Pen pen = new Pen(Item.AverageColor, LinkWidth))
			{

				if (SupplierElement?.DisplayedNode.IsFlipped == true || ConsumerElement?.DisplayedNode.IsFlipped == true)
				{
					pen.CustomStartCap = new System.Drawing.Drawing2D.AdjustableArrowCap(LinkWidth, LinkWidth);
					pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
				}
				else
				{
					pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
					pen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(LinkWidth, LinkWidth);
				}

				if (linkingUp)
					graphics.DrawBeziers(pen, new Point[] {
						pointN,
						pointN2,
						pointM2,
						pointM,
					});
				else
					graphics.DrawBeziers(pen, new Point[]{
						pointN,
						pointN2,
						pointMidN,
						pointMidA,
						pointMidA,
						pointMidB,
						pointMidB,
						pointMidM,
						pointM2,
						pointM
					});
			}
		}
	}
}
