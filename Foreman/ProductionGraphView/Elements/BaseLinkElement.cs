using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Foreman {
    public abstract class BaseLinkElement : GraphElement {
        public enum LineType {
            Simple,
            UShape,
            NShape
        }

        public BaseNodeElement SupplierElement { get; protected set; }
        public BaseNodeElement ConsumerElement { get; protected set; }
        public virtual ItemQualityPair Item { get; protected set; }

        private Point _consumerOrigin, _supplierOrigin;
        private NodeDirection _consumerDirection, _supplierDirection;

        public LineType Type { get; private set; }

        // for basic links
        private Point _consumerPull, _supplierPull;
        // for U shape links
        private Point _midUa, _midUb, _midUc, _midUd, _pullU1, _pullU2, _pullU3, _pullU4;
        // for N shape links
        private Point _midNa, _midNb, _midNc, _midNd, _midNe, _midNf, _pullN1, _pullN2, _pullN3, _pullN4, _pullN5, _pullN6, _pullN7, _pullN8;
        //private Point pointMidA, pointMidAPull, pointMidB, pointMidBPull; //for the U and N shape links

        public float LinkWidth { get; set; }

        public Rectangle CalculatedBounds { get; private set; }

        protected bool IconOnlyDraw;

        private const int circlePull = 100;
        private static CustomLineCap _arrowCap = new AdjustableArrowCap(4, 3);

        // link elements are always considered to be located at 0,0 graph to simplify things,
        // with their connection points being in graph-coordinates (no need to do any local transforms)
        public override Point Location {
            get => new();
            set { }
        }

        public override int X {
            get => 0;
            set { }
        }

        public override int Y {
            get => 0;
            set { }
        }

        public BaseLinkElement(ProductionGraphViewer graphViewer) : base(graphViewer) {
            LinkWidth = 3f;
        }

        protected BaseLinkElement(ProductionGraphViewer graphViewer, BaseLinkElement masterLink) : base(graphViewer, masterLink) {
            LinkWidth = masterLink.Width;
        }

        // NOTE: link element works in graph coordinates throughout (since Location is 0,0 for it - and it is always owned directly by the graph viewer).
        // So we don't have to bother with graph to local conversions
        public override void UpdateVisibility(Rectangle graphZone, int xBorder = 0, int yBorder = 0) {
            UpdateCurve();
            Visible =
                CalculatedBounds.X + CalculatedBounds.Width > graphZone.X - xBorder &&
                CalculatedBounds.X < graphZone.X + graphZone.Width + xBorder &&
                CalculatedBounds.Y + CalculatedBounds.Height > graphZone.Y - yBorder &&
                CalculatedBounds.Y < graphZone.Y + graphZone.Height + yBorder;
        }

        // supplier, consumer
        protected abstract Tuple<Point, Point> GetCurveEndpoints();
        // supplier, consumer
        protected abstract Tuple<NodeDirection, NodeDirection> GetEndpointDirections();

        // updates all points & boundaries (important for occluding objects outside view)
        protected void UpdateCurve() {
            var endpoints = GetCurveEndpoints();
            var endpointDirections = GetEndpointDirections();

            if (endpoints == null || endpointDirections == null)
                return;

            if (_supplierOrigin != endpoints.Item1 || _consumerOrigin != endpoints.Item2 || _supplierDirection != endpointDirections.Item1 ||
                _consumerDirection != endpointDirections.Item2) {
                _supplierOrigin = endpoints.Item1;
                _supplierDirection = endpointDirections.Item1;
                _consumerOrigin = endpoints.Item2;
                _consumerDirection = endpointDirections.Item2;

                Type = _supplierDirection != _consumerDirection ? LineType.UShape :
                    (_supplierDirection == NodeDirection.Up && _consumerOrigin.Y > _supplierOrigin.Y) ||
                    (_supplierDirection == NodeDirection.Down && _consumerOrigin.Y < _supplierOrigin.Y) ? LineType.NShape : LineType.Simple;

                switch (Type) {
                    // supplier and consumer directions are same, link direction is regular
                    // (consumer is below supplier if direction is up, and above supplier if direction is down)
                    case LineType.Simple:
                        if (_supplierDirection == NodeDirection.Up) {
                            _supplierPull = new Point(_supplierOrigin.X, _supplierOrigin.Y - Math.Max((_supplierOrigin.Y - _consumerOrigin.Y) / 2, 20));
                            _consumerPull = new Point(_consumerOrigin.X, _consumerOrigin.Y + Math.Max((_supplierOrigin.Y - _consumerOrigin.Y) / 2, 20));
                        } else {
                            _supplierPull = new Point(_supplierOrigin.X, _supplierOrigin.Y + Math.Max((_consumerOrigin.Y - _supplierOrigin.Y) / 2, 20));
                            _consumerPull = new Point(_consumerOrigin.X, _consumerOrigin.Y - Math.Max((_consumerOrigin.Y - _supplierOrigin.Y) / 2, 20));
                        }

                        CalculatedBounds = new Rectangle(
                            Math.Min(_supplierOrigin.X, _consumerOrigin.X),
                            Math.Min(_supplierOrigin.Y, _consumerOrigin.Y),
                            Math.Abs(_supplierOrigin.X - _consumerOrigin.X),
                            Math.Abs(_supplierOrigin.Y - _consumerOrigin.Y));

                        break;

                    // supplier and consumer directions are different
                    case LineType.UShape:

                        var xOffset = Math.Min(circlePull * 2, Math.Abs(_consumerOrigin.X - _supplierOrigin.X)) *
                            Math.Sign(_consumerOrigin.X - _supplierOrigin.X) /
                            2;
                        if (_supplierDirection == NodeDirection.Up) {
                            _midUa = new Point(_supplierOrigin.X, Math.Min(_supplierOrigin.Y, _consumerOrigin.Y));
                            _midUb = new Point(_midUa.X + xOffset, _midUa.Y - circlePull);
                            _midUd = new Point(_consumerOrigin.X, _midUa.Y);
                            _midUc = new Point(_midUd.X - xOffset, _midUb.Y);

                            _pullU1 = new Point(_supplierOrigin.X, _midUa.Y - circlePull / 2);
                            _pullU2 = new Point(_supplierOrigin.X + xOffset / 2, _midUb.Y);
                            _pullU3 = new Point(_consumerOrigin.X - xOffset / 2, _midUb.Y);
                            _pullU4 = new Point(_consumerOrigin.X, _midUd.Y - circlePull / 2);
                        } else {
                            _midUa = new Point(_supplierOrigin.X, Math.Max(_supplierOrigin.Y, _consumerOrigin.Y));
                            _midUb = new Point(_midUa.X + xOffset, _midUa.Y + circlePull);
                            _midUd = new Point(_consumerOrigin.X, _midUa.Y);
                            _midUc = new Point(_midUd.X - xOffset, _midUb.Y);

                            _pullU1 = new Point(_supplierOrigin.X, _midUa.Y + circlePull / 2);
                            _pullU2 = new Point(_supplierOrigin.X + xOffset / 2, _midUb.Y);
                            _pullU3 = new Point(_consumerOrigin.X - xOffset / 2, _midUb.Y);
                            _pullU4 = new Point(_consumerOrigin.X, _midUd.Y + circlePull / 2);
                        }

                        CalculatedBounds = new Rectangle(
                            Math.Min(_supplierOrigin.X, _consumerOrigin.X),
                            Math.Min(_supplierOrigin.Y, _consumerOrigin.Y) - (_supplierDirection == NodeDirection.Up ? circlePull : 0),
                            Math.Abs(_supplierOrigin.X - _consumerOrigin.X),
                            Math.Abs(_supplierOrigin.Y - _consumerOrigin.Y) + circlePull);
                        break;

                    // supplier and consumer directions are same, but the link direction is wrong
                    // (consumer is above supplier if direction is up, and below supplier if direction is down)
                    case LineType.NShape:
                        var midX = Math.Abs(_supplierOrigin.X - _consumerOrigin.X) > 2 * circlePull ? (_supplierOrigin.X + _consumerOrigin.X) / 2 :
                            _supplierOrigin.X > _consumerOrigin.X ? _supplierOrigin.X + (int) (circlePull * 1.5) : _supplierOrigin.X - (int) (circlePull * 1.5);
                        var xOffsetA = Math.Min(circlePull * 2, Math.Abs(_supplierOrigin.X - midX)) * Math.Sign(midX - _supplierOrigin.X) / 2;
                        var xOffsetB = Math.Min(circlePull * 2, Math.Abs(midX - _consumerOrigin.X)) * Math.Sign(_consumerOrigin.X - midX) / 2;

                        _midNc = new Point(midX, _supplierOrigin.Y);
                        _midNd = new Point(midX, _consumerOrigin.Y);

                        if (_supplierDirection == NodeDirection.Up) {
                            _midNa = new Point(_supplierOrigin.X + xOffsetA, _supplierOrigin.Y - circlePull);
                            _midNb = new Point(_midNc.X - xOffsetA, _midNa.Y);

                            _midNe = new Point(_midNd.X + xOffsetB, _consumerOrigin.Y + circlePull);
                            _midNf = new Point(_consumerOrigin.X - xOffsetB, _midNe.Y);

                            _pullN1 = new Point(_supplierOrigin.X, _supplierOrigin.Y - circlePull / 2);
                            _pullN2 = new Point(_supplierOrigin.X + xOffsetA / 2, _midNa.Y);
                            _pullN3 = new Point(_midNc.X - xOffsetA / 2, _midNa.Y);
                            _pullN4 = new Point(_midNc.X, _pullN1.Y);
                            _pullN5 = new Point(_midNc.X, _consumerOrigin.Y + circlePull / 2);
                            _pullN6 = new Point(_midNc.X + xOffsetB / 2, _midNe.Y);
                            _pullN7 = new Point(_consumerOrigin.X - xOffsetB / 2, _midNe.Y);
                            _pullN8 = new Point(_consumerOrigin.X, _pullN5.Y);
                        } else {
                            _midNa = new Point(_supplierOrigin.X + xOffsetA, _supplierOrigin.Y + circlePull);
                            _midNb = new Point(_midNc.X - xOffsetA, _midNa.Y);

                            _midNe = new Point(_midNd.X + xOffsetB, _consumerOrigin.Y - circlePull);
                            _midNf = new Point(_consumerOrigin.X - xOffsetB, _midNe.Y);

                            _pullN1 = new Point(_supplierOrigin.X, _supplierOrigin.Y + circlePull / 2);
                            _pullN2 = new Point(_supplierOrigin.X + xOffsetA / 2, _midNa.Y);
                            _pullN3 = new Point(_midNc.X - xOffsetA / 2, _midNa.Y);
                            _pullN4 = new Point(_midNc.X, _pullN1.Y);
                            _pullN5 = new Point(_midNc.X, _consumerOrigin.Y - circlePull / 2);
                            _pullN6 = new Point(_midNc.X + xOffsetB / 2, _midNe.Y);
                            _pullN7 = new Point(_consumerOrigin.X - xOffsetB / 2, _midNe.Y);
                            _pullN8 = new Point(_consumerOrigin.X, _pullN5.Y);
                        }

                        CalculatedBounds = new Rectangle(
                            Math.Min(Math.Min(midX, _supplierOrigin.X), _consumerOrigin.X),
                            Math.Min(_supplierOrigin.Y, _consumerOrigin.Y) - circlePull,
                            Math.Max(Math.Max(midX, _supplierOrigin.X), _consumerOrigin.X) - Math.Min(Math.Min(midX, _supplierOrigin.X), _consumerOrigin.X),
                            Math.Abs(_supplierOrigin.Y - _consumerOrigin.Y) + 2 * circlePull);
                        break;
                }
            }
        }

        public override bool ContainsPoint(Point graphPoint) {
            return false;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            IconOnlyDraw = style == NodeDrawingStyle.IconsOnly;
            UpdateCurve();

            using var pen = new Pen(Item.Item.AverageColor, LinkWidth);

            pen.EndCap = LineCap.Round;
            pen.StartCap = LineCap.Round;
            if (GraphViewer.ArrowsOnLinks && !GraphViewer.DynamicLinkWidth && !IconOnlyDraw)
                pen.CustomEndCap = _arrowCap;

            switch (Type) {
                case LineType.Simple:
                    graphics.DrawBeziers(pen, [
                        _supplierOrigin,
                        _supplierPull,

                        _consumerPull,
                        _consumerOrigin
                    ]);
                    break;

                case LineType.UShape:
                    graphics.DrawBeziers(pen, [
                        _supplierOrigin,
                        _supplierOrigin,

                        _midUa,
                        _midUa,
                        _pullU1,

                        _pullU2,
                        _midUb,
                        _midUb,

                        _midUc,
                        _midUc,
                        _pullU3,

                        _pullU4,
                        _midUd,
                        _midUd,

                        _consumerOrigin,
                        _consumerOrigin
                    ]);
                    break;

                case LineType.NShape:
                    graphics.DrawBeziers(pen, [
                        _supplierOrigin,
                        _pullN1,

                        _pullN2,
                        _midNa,
                        _midNa,

                        _midNb,
                        _midNb,
                        _pullN3,

                        _pullN4,
                        _midNc,
                        _midNc,

                        _midNd,
                        _midNd,
                        _pullN5,

                        _pullN6,
                        _midNe,
                        _midNe,

                        _midNf,
                        _midNf,
                        _pullN7,

                        _pullN8,
                        _consumerOrigin
                    ]);
                    break;
            }
        }
    }
}