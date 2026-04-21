using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public class ItemTabElement : GraphElement {
        // I just use these two to get a decent approximation as to how far to space new nodes when bulk-added
        public static int TabWidth => iconSize + border * 3;

        public static int TabBorder => border;

        public LinkType LinkType;
        public ItemQualityPair Item { get; private set; }

        public IEnumerable<ReadOnlyNodeLink> Links {
            get {
                return LinkType == LinkType.Input
                    ? _displayedNode.InputLinks.Where(l => l.Item == Item)
                    : _displayedNode.OutputLinks.Where(l => l.Item == Item);
            }
        }

        public bool HideItemTab { get; set; }

        private const int iconSize = 32;
        private const int border = 3;
        private int _textHeight = 11;

        private static StringFormat _bottomFormat = new() { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Center };
        private static StringFormat _topFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };

        private static Brush _directionBrush = new SolidBrush(Color.FromArgb(40, Color.Black));

        private static Pen _regularBorderPen = new(Color.DimGray, 3);
        private static Pen _overproducedBorderPen = new(Color.DarkGoldenrod, 3);
        private static Pen _disconnectedBorderPen = new(Color.DarkRed, 3);

        private static Brush _textBrush = Brushes.Black;
        private static Brush _fillBrush = Brushes.White;

        private static Font _textFont = new(FontFamily.GenericSansSerif, 6);

        private Pen _borderPen;
        private string _text = "";

        private readonly ReadOnlyBaseNode _displayedNode;

        public ItemTabElement(ItemQualityPair item, LinkType type, ProductionGraphViewer graphViewer, BaseNodeElement node) : base(graphViewer, node) {
            _displayedNode = node.DisplayedNode;
            Item = item;
            LinkType = type;
            HideItemTab = false;

            _borderPen = _regularBorderPen;
            var textHeight = (int) GraphViewer.CreateGraphics().MeasureString("a", _textFont).Height;
            Width = TabWidth;
            Height = iconSize + textHeight + border + 3;
            X = 0;
            Y = 0;
        }

        // in graph coordinates
        public Point GetConnectionPoint() {
            if ((LinkType == LinkType.Input && _displayedNode.NodeDirection == NodeDirection.Up) ||
                (LinkType == LinkType.Output && _displayedNode.NodeDirection == NodeDirection.Down))
                return LocalToGraph(new Point(0, Height / 2));
            else //if ((LinkType == LinkType.Input && DisplayedNode.NodeDirection == NodeDirection.down) || (LinkType == LinkType.Output && DisplayedNode.NodeDirection == NodeDirection.Up))
                return LocalToGraph(new Point(0, -Height / 2));
        }

        // if input then: recipe rate = consume rate; if output then recipe rate = production rate
        public void UpdateValues(double recipeRate, double outputRate, bool isOverproduced) {
            _borderPen = _regularBorderPen;
            _text = GraphicsStuff.DoubleToString(recipeRate);
            var textHeight = 10;
            if (isOverproduced) {
                _borderPen = _overproducedBorderPen;
                _text = GraphicsStuff.DoubleToString(outputRate) + "\n" + _text;
                textHeight += 10;
            } else if (!Links.Any())
                _borderPen = _disconnectedBorderPen;

            Height = iconSize + textHeight + border + 3;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (style == NodeDrawingStyle.IconsOnly || HideItemTab)
                return;

            var trans = LocalToGraph(new Point(0, 0));

            // background

            GraphicsStuff.FillRoundRect(trans.X - Bounds.Width / 2, trans.Y - Bounds.Height / 2, Bounds.Width, Bounds.Height, border, graphics, _fillBrush);

            // direction signs (only if using dynamic link width or not using arrows on links)

            if (GraphViewer.DynamicLinkWidth || !GraphViewer.ArrowsOnLinks) {
                if (_displayedNode.NodeDirection == NodeDirection.Up)
                    graphics.FillPolygon(_directionBrush,
                    [
                        new Point(trans.X - Bounds.Width / 2, trans.Y + Bounds.Height / 2),
                        new Point(trans.X + Bounds.Width / 2, trans.Y + Bounds.Height / 2), new Point(trans.X, trans.Y - Bounds.Height / 2)
                    ]);
                else
                    graphics.FillPolygon(_directionBrush,
                    [
                        new Point(trans.X - Bounds.Width / 2, trans.Y - Bounds.Height / 2),
                        new Point(trans.X + Bounds.Width / 2, trans.Y - Bounds.Height / 2), new Point(trans.X, trans.Y + Bounds.Height / 2)
                    ]);
            }

            // border

            GraphicsStuff.DrawRoundRect(trans.X - Bounds.Width / 2, trans.Y - Bounds.Height / 2, Bounds.Width, Bounds.Height, border, graphics, _borderPen);

            // text & icon

            if (style is NodeDrawingStyle.Regular or NodeDrawingStyle.PrintStyle) {
                if (LinkType == LinkType.Output) {
                    graphics.DrawString(_text, _textFont, _textBrush, new PointF(trans.X, trans.Y + (_textHeight + border - Bounds.Height - 10) / 2),
                        _topFormat);
                    graphics.DrawImage(Item.Icon ?? DataCache.UnknownIcon, trans.X - Bounds.Width / 2 + (int) (border * 1.5),
                        trans.Y + Bounds.Height / 2 - border - iconSize, iconSize, iconSize);
                } else {
                    graphics.DrawString(_text, _textFont, _textBrush, new PointF(trans.X, trans.Y - (_textHeight + border - Bounds.Height - 10) / 2),
                        _bottomFormat);
                    graphics.DrawImage(Item.Icon ?? DataCache.UnknownIcon, trans.X - Bounds.Width / 2 + (int) (border * 1.5),
                        trans.Y - Bounds.Height / 2 + border, iconSize, iconSize);
                }
            }
        }

        public override List<TooltipInfo> GetToolTips(Point graphPoint) {
            var toolTips = new List<TooltipInfo>();
            var tti = new TooltipInfo();
            var parentNode = (BaseNodeElement) MyParent;

            if (parentNode.DisplayedNode is ReadOnlyRecipeNode rNode) {
                if (LinkType == LinkType.Input)
                    tti.Text = Item.Item is Fluid ? rNode.BaseRecipe.Recipe.GetIngredientFriendlyName(Item.Item) : Item.FriendlyName;
                else //if(LinkType == LinkType.Output)
                    tti.Text = Item.Item is Fluid ? rNode.BaseRecipe.Recipe.GetProductFriendlyName(Item.Item) : Item.FriendlyName;
            } else if (Item.Item is Fluid { IsTemperatureDependent: true } fluid) {
                // input type tab means output of connection link and vice versa
                var tempRange = LinkChecker.GetTemperatureRange(
                    fluid,
                    parentNode.DisplayedNode,
                    LinkType == LinkType.Input
                        ? LinkType.Output
                        : LinkType.Input,
                    true
                );

                // if there was no temp range on this side of this throughput node, try to just copy the other side
                if (tempRange.Ignore && _displayedNode is ReadOnlyPassthroughNode)
                    tempRange = LinkChecker.GetTemperatureRange(fluid, parentNode.DisplayedNode, LinkType, true);
                tti.Text = fluid.GetTemperatureRangeFriendlyName(tempRange);
            } else
                tti.Text = Item.FriendlyName;

            tti.Direction = (LinkType == LinkType.Input && _displayedNode.NodeDirection == NodeDirection.Up) ||
                (LinkType == LinkType.Output && _displayedNode.NodeDirection == NodeDirection.Down)
                    ? Direction.Up
                    : Direction.Down;
            tti.ScreenLocation = GraphViewer.GraphToScreen(GetConnectionPoint());
            toolTips.Add(tti);

            var helpToolTipInfo = new TooltipInfo {
                Text = "Drag to create a new connection.\nRight click for options.",
                Direction = Direction.None,
                ScreenLocation = new Point(10, 10)
            };
            toolTips.Add(helpToolTipInfo);

            return toolTips;
        }

        public override void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) {
            if (button != MouseButtons.Right)
                return;

            var connections = new List<ReadOnlyNodeLink>();
            connections.AddRange(LinkType == LinkType.Input
                ? _displayedNode.InputLinks.Where(l => l.Item == Item)
                : _displayedNode.OutputLinks.Where(l => l.Item == Item));

            RightClickMenu.Items.Add(new ToolStripMenuItem("Delete connections", null,
                    (o, e) => {
                        RightClickMenu.Close();
                        foreach (var link in connections)
                            GraphViewer.Graph.DeleteLink(link);
                        GraphViewer.Graph.UpdateNodeValues();
                    })
                { Enabled = connections.Count > 0 });

            RightClickMenu.Show(GraphViewer, GraphViewer.GraphToScreen(graphPoint));
        }
    }
}