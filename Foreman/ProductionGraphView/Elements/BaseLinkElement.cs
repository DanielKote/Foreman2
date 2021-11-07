using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Foreman
{
	public abstract class BaseLinkElement : GraphElement
	{
		private enum LineType { Simple, UShape, NShape }

		public BaseNodeElement SupplierElement { get; protected set; }
		public BaseNodeElement ConsumerElement { get; protected set; }
		public virtual Item Item { get; protected set; }

		private Point consumerOrigin, supplierOrigin;
		private Point consumerPull, supplierPull;
		private NodeDirection consumerDirection, supplierDirection;

		private LineType lineType;
		private Point pointMidA, pointMidAPull, pointMidB, pointMidBPull; //for the U and N shape links

		public float LinkWidth { get; set; }

		public Rectangle CalculatedBounds { get; private set; }

		private const int circlePull = 100;
		private static CustomLineCap arrowCap = new AdjustableArrowCap(4,3);

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
			Visible =
					 	CalculatedBounds.X + CalculatedBounds.Width > graph_zone.X - xborder &&
						CalculatedBounds.X < graph_zone.X + graph_zone.Width + xborder &&
						CalculatedBounds.Y + CalculatedBounds.Height > graph_zone.Y - yborder &&
						CalculatedBounds.Y < graph_zone.Y + graph_zone.Height + yborder;
		}

		protected abstract Tuple<Point, Point> GetCurveEndpoints(); //supplier,consumer
		protected abstract Tuple<NodeDirection, NodeDirection> GetEndpointDirections(); //supplier,consumer

		protected void UpdateCurve() //updates all points & boundaries (important for occluding objects outside view)
		{
			Tuple<Point,Point> endpoints = GetCurveEndpoints();
			Tuple<NodeDirection, NodeDirection> endpointDirections = GetEndpointDirections();

			if (endpoints == null || endpointDirections == null)
				return;

			if (supplierOrigin != endpoints.Item1|| consumerOrigin != endpoints.Item2 || supplierDirection != endpointDirections.Item1 || consumerDirection != endpointDirections.Item2)
			{
				supplierOrigin = endpoints.Item1;
				supplierDirection = endpointDirections.Item1;
				consumerOrigin = endpoints.Item2;
				consumerDirection = endpointDirections.Item2;

				lineType = (supplierDirection != consumerDirection) ? LineType.UShape :
					((supplierDirection == NodeDirection.Up && consumerOrigin.Y > supplierOrigin.Y) || (supplierDirection == NodeDirection.Down && consumerOrigin.Y < supplierOrigin.Y)) ? LineType.NShape : LineType.Simple;

				switch(lineType)
				{
					case LineType.Simple: //supplier and consumer directions are same, link direction is regular (consumer is below supplier if direction is up, and above supplier if direction is down)
						if (supplierDirection == NodeDirection.Up)
						{
							supplierPull = new Point(supplierOrigin.X, supplierOrigin.Y -  Math.Max((int)((supplierOrigin.Y - consumerOrigin.Y) / 2), 20));
							consumerPull = new Point(consumerOrigin.X, consumerOrigin.Y + Math.Max((int)((supplierOrigin.Y - consumerOrigin.Y) / 2), 20));
						}
						else
						{
							supplierPull = new Point(supplierOrigin.X, supplierOrigin.Y + Math.Max((int)((consumerOrigin.Y - supplierOrigin.Y) / 2), 20));
							consumerPull = new Point(consumerOrigin.X, consumerOrigin.Y - Math.Max((int)((consumerOrigin.Y - supplierOrigin.Y) / 2), 20));
						}

						CalculatedBounds = new Rectangle(
							Math.Min(supplierOrigin.X, consumerOrigin.X),
							Math.Min(supplierOrigin.Y, consumerOrigin.Y),
							Math.Abs(supplierOrigin.X - consumerOrigin.X),
							Math.Abs(supplierOrigin.Y - consumerOrigin.Y));

						break;
					case LineType.UShape: //supplier and consumer directions are different

						if(supplierDirection == NodeDirection.Up)
						{
							pointMidA = new Point(supplierOrigin.X, Math.Min(supplierOrigin.Y, consumerOrigin.Y));
							pointMidAPull = new Point(pointMidA.X, pointMidA.Y - circlePull);
							pointMidB = new Point(consumerOrigin.X, Math.Min(supplierOrigin.Y, consumerOrigin.Y));
							pointMidBPull = new Point(pointMidB.X, pointMidB.Y - circlePull);
						}
						else
						{
							pointMidA = new Point(supplierOrigin.X, Math.Max(supplierOrigin.Y, consumerOrigin.Y));
							pointMidAPull = new Point(pointMidA.X, pointMidA.Y + circlePull);
							pointMidB = new Point(consumerOrigin.X, Math.Max(supplierOrigin.Y, consumerOrigin.Y));
							pointMidBPull = new Point(pointMidB.X, pointMidB.Y + circlePull);
						}

						CalculatedBounds = new Rectangle(
							Math.Min(supplierOrigin.X, consumerOrigin.X),
							Math.Min(supplierOrigin.Y, consumerOrigin.Y) - (supplierDirection == NodeDirection.Up? circlePull : 0),
							Math.Abs(supplierOrigin.X - consumerOrigin.X),
							Math.Abs(supplierOrigin.Y - consumerOrigin.Y) + circlePull);
						break;
					case LineType.NShape: //supplier and consumer directions are same, but the link direction is wrong (consumer is above supplier if direction is up, and below supplier if direction is down)
						int midX = Math.Abs(supplierOrigin.X - consumerOrigin.X) > 200 ? (supplierOrigin.X + consumerOrigin.X) / 2 : supplierOrigin.X > consumerOrigin.X ? supplierOrigin.X + 150 : supplierOrigin.X - 150;
						pointMidA = new Point(midX, supplierOrigin.Y);
						pointMidB = new Point(midX, consumerOrigin.Y);

						if(supplierDirection == NodeDirection.Up)
						{
							supplierPull = new Point(supplierOrigin.X, supplierOrigin.Y - circlePull);
							pointMidAPull = new Point(pointMidA.X, pointMidA.Y - circlePull);
							pointMidBPull = new Point(pointMidB.X, pointMidB.Y + circlePull);
							consumerPull = new Point(consumerOrigin.X, consumerOrigin.Y + circlePull);
						}
						else
						{
							supplierPull = new Point(supplierOrigin.X, supplierOrigin.Y + circlePull);
							pointMidAPull = new Point(pointMidA.X, pointMidA.Y + circlePull);
							pointMidBPull = new Point(pointMidB.X, pointMidB.Y - circlePull);
							consumerPull = new Point(consumerOrigin.X, consumerOrigin.Y - circlePull);
						}

						CalculatedBounds = new Rectangle(
							Math.Min(supplierOrigin.X, consumerOrigin.X),
							Math.Min(supplierOrigin.Y, consumerOrigin.Y) - circlePull,
							Math.Abs(supplierOrigin.X - consumerOrigin.X),
							Math.Abs(supplierOrigin.Y - consumerOrigin.Y) + (2 * circlePull));
						break;
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

			using (Pen pen = new Pen(Item.AverageColor, LinkWidth) { EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round })
			{
				if (!graphViewer.DynamicLinkWidth)
					pen.CustomEndCap = arrowCap;

				switch(lineType)
				{
					case LineType.Simple:
						graphics.DrawBeziers(pen, new Point[]
						{
							supplierOrigin,
							supplierPull,
							consumerPull,
							consumerOrigin
						});
						break;
					case LineType.UShape:
						graphics.DrawBeziers(pen, new Point[]
						{
							supplierOrigin,
							supplierOrigin,
							pointMidA,
							pointMidA,
							pointMidAPull,
							pointMidBPull,
							pointMidB,
							pointMidB,
							consumerOrigin,
							consumerOrigin
						});
						break;
					case LineType.NShape:
						graphics.DrawBeziers(pen, new Point[]
						{
							supplierOrigin,
							supplierPull,
							pointMidAPull,
							pointMidA,
							pointMidA,
							pointMidB,
							pointMidB,
							pointMidBPull,
							consumerPull,
							consumerOrigin
						});
						break;
				}
			}
		}
	}
}
