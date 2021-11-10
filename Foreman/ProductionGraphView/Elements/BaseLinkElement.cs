using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Foreman
{
	public abstract class BaseLinkElement : GraphElement
	{
		public enum LineType { Simple, UShape, NShape }

		public BaseNodeElement SupplierElement { get; protected set; }
		public BaseNodeElement ConsumerElement { get; protected set; }
		public virtual Item Item { get; protected set; }

		private Point consumerOrigin, supplierOrigin;
		private NodeDirection consumerDirection, supplierDirection;

		public LineType Type { get; private set; }

		private Point consumerPull, supplierPull; //for basic links
		private Point midUA, midUB, midUC, midUD, pullU1, pullU2, pullU3, pullU4; //for U shape links
		private Point midNA, midNB, midNC, midND, midNE, midNF, pullN1, pullN2, pullN3, pullN4, pullN5, pullN6, pullN7, pullN8; //for N shape links
		//private Point pointMidA, pointMidAPull, pointMidB, pointMidBPull; //for the U and N shape links

		public float LinkWidth { get; set; }

		public Rectangle CalculatedBounds { get; private set; }

		protected bool iconOnlyDraw;

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

				Type = (supplierDirection != consumerDirection) ? LineType.UShape :
					((supplierDirection == NodeDirection.Up && consumerOrigin.Y > supplierOrigin.Y) || (supplierDirection == NodeDirection.Down && consumerOrigin.Y < supplierOrigin.Y)) ? LineType.NShape : LineType.Simple;

				switch(Type)
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

						int xOffset = Math.Min(circlePull * 2, Math.Abs(consumerOrigin.X - supplierOrigin.X)) * Math.Sign(consumerOrigin.X - supplierOrigin.X) / 2;
						if(supplierDirection == NodeDirection.Up)
						{
							midUA = new Point(supplierOrigin.X, Math.Min(supplierOrigin.Y, consumerOrigin.Y));
							midUB = new Point(midUA.X + xOffset, midUA.Y - circlePull);
							midUD = new Point(consumerOrigin.X, midUA.Y);
							midUC = new Point(midUD.X - xOffset, midUB.Y);

							pullU1 = new Point(supplierOrigin.X, midUA.Y - (circlePull / 2));
							pullU2 = new Point(supplierOrigin.X + (xOffset / 2), midUB.Y);
							pullU3 = new Point(consumerOrigin.X - (xOffset / 2), midUB.Y);
							pullU4 = new Point(consumerOrigin.X, midUD.Y - (circlePull / 2));
						}
						else
						{
							midUA = new Point(supplierOrigin.X, Math.Max(supplierOrigin.Y, consumerOrigin.Y));
							midUB = new Point(midUA.X + xOffset, midUA.Y + circlePull);
							midUD = new Point(consumerOrigin.X, midUA.Y);
							midUC = new Point(midUD.X - xOffset, midUB.Y);

							pullU1 = new Point(supplierOrigin.X, midUA.Y + (circlePull / 2));
							pullU2 = new Point(supplierOrigin.X + (xOffset / 2), midUB.Y);
							pullU3 = new Point(consumerOrigin.X - (xOffset / 2), midUB.Y);
							pullU4 = new Point(consumerOrigin.X, midUD.Y + (circlePull / 2));
						}

						CalculatedBounds = new Rectangle(
							Math.Min(supplierOrigin.X, consumerOrigin.X),
							Math.Min(supplierOrigin.Y, consumerOrigin.Y) - (supplierDirection == NodeDirection.Up? circlePull : 0),
							Math.Abs(supplierOrigin.X - consumerOrigin.X),
							Math.Abs(supplierOrigin.Y - consumerOrigin.Y) + circlePull);
						break;
					case LineType.NShape: //supplier and consumer directions are same, but the link direction is wrong (consumer is above supplier if direction is up, and below supplier if direction is down)
						int midX = Math.Abs(supplierOrigin.X - consumerOrigin.X) > 2 * circlePull ? (supplierOrigin.X + consumerOrigin.X) / 2 : supplierOrigin.X > consumerOrigin.X ? supplierOrigin.X + (int)(circlePull * 1.5) : supplierOrigin.X - (int)(circlePull * 1.5);
						int xOffsetA = Math.Min(circlePull * 2, Math.Abs(supplierOrigin.X - midX)) * Math.Sign(midX - supplierOrigin.X) / 2;
						int xOffsetB = Math.Min(circlePull * 2, Math.Abs(midX - consumerOrigin.X)) * Math.Sign(consumerOrigin.X - midX) / 2;

						midNC = new Point(midX, supplierOrigin.Y);
						midND = new Point(midX, consumerOrigin.Y);

						if(supplierDirection == NodeDirection.Up)
						{
							midNA = new Point(supplierOrigin.X + xOffsetA, supplierOrigin.Y - circlePull);
							midNB = new Point(midNC.X - xOffsetA, midNA.Y);

							midNE = new Point(midND.X + xOffsetB, consumerOrigin.Y + circlePull);
							midNF = new Point(consumerOrigin.X - xOffsetB, midNE.Y);

							pullN1 = new Point(supplierOrigin.X, supplierOrigin.Y - (circlePull / 2));
							pullN2 = new Point(supplierOrigin.X + (xOffsetA / 2), midNA.Y);
							pullN3 = new Point(midNC.X - (xOffsetA / 2), midNA.Y);
							pullN4 = new Point(midNC.X, pullN1.Y);
							pullN5 = new Point(midNC.X, consumerOrigin.Y + (circlePull / 2));
							pullN6 = new Point(midNC.X + (xOffsetB / 2), midNE.Y);
							pullN7 = new Point(consumerOrigin.X - (xOffsetB / 2), midNE.Y);
							pullN8 = new Point(consumerOrigin.X, pullN5.Y);
						}
						else
						{
							midNA = new Point(supplierOrigin.X + xOffsetA, supplierOrigin.Y + circlePull);
							midNB = new Point(midNC.X - xOffsetA, midNA.Y);

							midNE = new Point(midND.X + xOffsetB, consumerOrigin.Y - circlePull);
							midNF = new Point(consumerOrigin.X - xOffsetB, midNE.Y);

							pullN1 = new Point(supplierOrigin.X, supplierOrigin.Y + (circlePull / 2));
							pullN2 = new Point(supplierOrigin.X + (xOffsetA / 2), midNA.Y);
							pullN3 = new Point(midNC.X - (xOffsetA / 2), midNA.Y);
							pullN4 = new Point(midNC.X, pullN1.Y);
							pullN5 = new Point(midNC.X, consumerOrigin.Y - (circlePull / 2));
							pullN6 = new Point(midNC.X + (xOffsetB / 2), midNE.Y);
							pullN7 = new Point(consumerOrigin.X - (xOffsetB / 2), midNE.Y);
							pullN8 = new Point(consumerOrigin.X, pullN5.Y);
						}

						CalculatedBounds = new Rectangle(
							Math.Min(Math.Min(midX, supplierOrigin.X), consumerOrigin.X),
							Math.Min(supplierOrigin.Y, consumerOrigin.Y) - circlePull,
							Math.Max(Math.Max(midX, supplierOrigin.X), consumerOrigin.X) - Math.Min(Math.Min(midX, supplierOrigin.X), consumerOrigin.X),
							Math.Abs(supplierOrigin.Y - consumerOrigin.Y) + (2 * circlePull));
						break;
				}
			}
		}
		public override bool ContainsPoint(Point graph_point)
		{
			return false;
		}

		protected override void Draw(Graphics graphics, NodeDrawingStyle style)
		{
			iconOnlyDraw = (style == NodeDrawingStyle.IconsOnly);
			UpdateCurve();

			using (Pen pen = new Pen(Item.AverageColor, LinkWidth) { EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round })
			{
				if (graphViewer.ArrowsOnLinks && !graphViewer.DynamicLinkWidth && !iconOnlyDraw)
					pen.CustomEndCap = arrowCap;

				switch(Type)
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

							midUA,
							midUA,
							pullU1,

							pullU2,
							midUB,
							midUB,

							midUC,
							midUC,
							pullU3,

							pullU4,
							midUD,
							midUD,

							consumerOrigin,
							consumerOrigin
						});
						break;
					case LineType.NShape:
						graphics.DrawBeziers(pen, new Point[]
						{
							supplierOrigin,
							pullN1,

							pullN2,
							midNA,
							midNA,

							midNB,
							midNB,
							pullN3,

							pullN4,
							midNC,
							midNC,
							
							midND,
							midND,
							pullN5,

							pullN6,
							midNE,
							midNE,

							midNF,
							midNF,
							pullN7,

							pullN8,
							consumerOrigin
						}); ;
						break;
				}
			}
		}
	}
}
