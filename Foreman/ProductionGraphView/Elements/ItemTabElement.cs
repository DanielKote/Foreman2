using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    public class ItemTabElement : GraphElement {
        public static int TabWidth { get { return iconSize + border * 3; } } //I just use these two to get a decent aproximation as to how far to space new nodes when bulk-added
        public static int TabBorder { get { return border; } }

        public LinkType LinkType { get; set; }
        public ItemQualityPair Item { get; private set; }
        public IEnumerable<INodeLinkViewModel> Links { get { return LinkType == LinkType.Input ? NodeViewModel.InputLinks.Where(l => l.Item == Item) : NodeViewModel.OutputLinks.Where(l => l.Item == Item); } }

        public bool HideItemTab { get; set; }

        private const int iconSize = 32;
        private const int border = 3;
        private readonly int textHeight = 11;

        private static readonly StringFormat bottomFormat = new() { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Center };
        private static readonly StringFormat topFormat = new() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };

        private static readonly Brush directionBrush = new SolidBrush(Color.FromArgb(40, Color.Black));

        private static readonly Pen regularBorderPen = new(Color.DimGray, 3);
        private static readonly Pen overproducedBorderPen = new(Color.DarkGoldenrod, 3);
        private static readonly Pen disconnectedBorderPen = new(Color.DarkRed, 3);

        private static readonly Brush textBrush = Brushes.Black;
        private static readonly Brush fillBrush = Brushes.White;

        private static readonly Font textFont = new(FontFamily.GenericSansSerif, 6);

        private Pen borderPen;
        private string text = "";

        private readonly INodeViewModel NodeViewModel;

        public ItemTabElement(ItemQualityPair item, LinkType type, ProductionGraphViewer graphViewer, BaseNodeElement node) : base(graphViewer, node) {
            NodeViewModel = node.ViewModel;
            Item = item;
            LinkType = type;
            HideItemTab = false;

            borderPen = regularBorderPen;
            int textHeight = (int)base.graphViewer.CreateGraphics().MeasureString("a", textFont).Height;
            Width = TabWidth;
            Height = iconSize + textHeight + border + 3;
            X = 0;
            Y = 0;
        }

        public Point GetConnectionPoint() //in graph coordinates
        {
            return (LinkType == LinkType.Input && NodeViewModel.NodeDirection == NodeDirection.Up) || (LinkType == LinkType.Output && NodeViewModel.NodeDirection == NodeDirection.Down)
                ? LocalToGraph(new Point(0, Height / 2))
                : LocalToGraph(new Point(0, -Height / 2));
        }

        public void UpdateValues(double recipeRate, double outputRate, bool isOverproduced) //if input then: recipe rate = consume rate; if output then recipe rate = production rate
        {
            borderPen = regularBorderPen;
            text = GraphicsStuff.DoubleToString(recipeRate);
            int textHeight = 10;
            if (isOverproduced) {
                borderPen = overproducedBorderPen;
                text = GraphicsStuff.DoubleToString(outputRate) + "\n" + text;
                textHeight += 10;
            } else if (!Links.Any())
                borderPen = disconnectedBorderPen;

            Height = iconSize + textHeight + border + 3;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (style == NodeDrawingStyle.IconsOnly || HideItemTab)
                return;

            Point trans = LocalToGraph(new Point(0, 0));

            //background
            GraphicsStuff.FillRoundRect(trans.X - (Bounds.Width / 2), trans.Y - (Bounds.Height / 2), Bounds.Width, Bounds.Height, border, graphics, fillBrush);

            //direction signs (only if using dynamic link width or not using arrows on links)
            if (graphViewer.DynamicLinkWidth || !graphViewer.ArrowsOnLinks) {
                if (NodeViewModel.NodeDirection == NodeDirection.Up)
                    graphics.FillPolygon(directionBrush, [new(trans.X - (Bounds.Width / 2), trans.Y + (Bounds.Height / 2)), new(trans.X + (Bounds.Width / 2), trans.Y + (Bounds.Height / 2)), new(trans.X, trans.Y - (Bounds.Height / 2))]);
                else
                    graphics.FillPolygon(directionBrush, [new(trans.X - (Bounds.Width / 2), trans.Y - (Bounds.Height / 2)), new(trans.X + (Bounds.Width / 2), trans.Y - (Bounds.Height / 2)), new(trans.X, trans.Y + (Bounds.Height / 2))]);
            }

            //border
            GraphicsStuff.DrawRoundRect(trans.X - (Bounds.Width / 2), trans.Y - (Bounds.Height / 2), Bounds.Width, Bounds.Height, border, graphics, borderPen);

            //text & icon
            if (style == NodeDrawingStyle.Regular || style == NodeDrawingStyle.PrintStyle) {
                if (LinkType == LinkType.Output) {
                    graphics.DrawString(text, textFont, textBrush, new PointF(trans.X, trans.Y + ((textHeight + border - Bounds.Height - 10) / 2)), topFormat);
                    graphics.DrawImage(Item.Icon ?? DataCache.UnknownIcon, trans.X - (Bounds.Width / 2) + (int)(border * 1.5), trans.Y + (Bounds.Height / 2) - border - iconSize, iconSize, iconSize);
                } else {
                    graphics.DrawString(text, textFont, textBrush, new PointF(trans.X, trans.Y - ((textHeight + border - Bounds.Height - 10) / 2)), bottomFormat);
                    graphics.DrawImage(Item.Icon ?? DataCache.UnknownIcon, trans.X - (Bounds.Width / 2) + (int)(border * 1.5), trans.Y - (Bounds.Height / 2) + border, iconSize, iconSize);
                }
            }
        }

        public override List<TooltipInfo> GetToolTips(Point graphPoint) {
            var toolTips = new List<TooltipInfo>();
            var tti = new TooltipInfo();
            if (myParent is not BaseNodeElement parentNode)
                return toolTips;

            if (parentNode.ViewModel is RecipeNodeViewModel rNode && rNode.BaseRecipe.Recipe is IRecipe recipe) {
                if (LinkType == LinkType.Input)
                    tti.Text = Item.Item is IFluid fluid ? recipe.GetIngredientFriendlyName(fluid) : Item.FriendlyName ?? "";
                else //if(LinkType == LinkType.Output)
                    tti.Text = Item.Item is IFluid fluid ? recipe.GetProductFriendlyName(fluid) : Item.FriendlyName ?? "";
            } else if ((Item.Item is IFluid fluid) && fluid.IsTemperatureDependent) {
                FRange tempRange = LinkChecker.GetTemperatureRange(
                    fluid,
                    parentNode.ViewModel,
                    (LinkType == LinkType.Input) ? LinkType.Output : LinkType.Input,
                    true,
                    graphViewer.Session); //input type tab means output of connection link and vice versa
                if (tempRange.Ignore && NodeViewModel is IPassthroughNodeViewModel)
                    tempRange = LinkChecker.GetTemperatureRange(fluid, parentNode.ViewModel, LinkType, true, graphViewer.Session); //if there was no temp range on this side of this throughput node, try to just copy the other side
                tti.Text = fluid.GetTemperatureRangeFriendlyName(tempRange);
            } else
                tti.Text = Item.FriendlyName ?? "";

            tti.Direction = ((LinkType == LinkType.Input && NodeViewModel.NodeDirection == NodeDirection.Up) || (LinkType == LinkType.Output && NodeViewModel.NodeDirection == NodeDirection.Down)) ? Direction.Up : Direction.Down;
            tti.ScreenLocation = graphViewer.GraphToScreen(GetConnectionPoint());
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
            if (button == MouseButtons.Right) {
                var connections = new List<LinkId>();
                if (LinkType == LinkType.Input)
                    connections.AddRange(NodeViewModel.InputLinks.Where(l => l.Item == Item).Select(l => l.Id));
                else //if (LinkType == LinkType.Output)
                    connections.AddRange(NodeViewModel.OutputLinks.Where(l => l.Item == Item).Select(l => l.Id));

                RightClickMenu.Items.Add(new ToolStripMenuItem("Delete connections", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        foreach (LinkId linkId in connections)
                            graphViewer.Session.Editor.DeleteLink(linkId);
                        graphViewer.Graph.UpdateNodeValues();
                    })) { Enabled = connections.Count > 0 });

                RightClickMenu.Show(graphViewer, graphViewer.GraphToScreen(graphPoint));
            }
        }
    }
}
