using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman {
    public class PassthroughNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush => _passthroughBgBrush;

        private static Brush _passthroughBgBrush = new SolidBrush(Color.FromArgb(200, 200, 200));

        private string ItemName => _displayedNode.PassthroughItem.FriendlyName;

        private readonly ReadOnlyPassthroughNode _displayedNode;

        public PassthroughNodeElement(ProductionGraphViewer graphViewer, ReadOnlyPassthroughNode node) : base(graphViewer, node) {
            Width = PassthroughNodeWidth;
            Height = BaseSimpleHeight;
            _displayedNode = node;
        }

        protected override Bitmap NodeIcon() {
            return null;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (style != NodeDrawingStyle.IconsOnly && _displayedNode.SimpleDraw && _displayedNode.RateType == RateType.Auto && !_displayedNode.KeyNode &&
                !_displayedNode.IsOverproducing() && !_displayedNode.ManualRateNotMet() && _displayedNode.InputLinks.Any() &&
                _displayedNode.OutputLinks.Any()) {
                InputTabs[0].HideItemTab = true;
                OutputTabs[0].HideItemTab = true;

                var maxLineWidth = _displayedNode.InputLinks.Concat(_displayedNode.OutputLinks).Select(l => GraphViewer.LinkElementDictionary[l].LinkWidth)
                    .Max();
                var inputPoint = InputTabs[0].GetConnectionPoint();
                var outputPoint = OutputTabs[0].GetConnectionPoint();
                using (var pen = new Pen(_displayedNode.PassthroughItem.Item.AverageColor, maxLineWidth)) {
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    graphics.DrawLine(pen, inputPoint, outputPoint);
                }

                if (style != NodeDrawingStyle.Regular) return;

                using (Brush brush = new SolidBrush(_displayedNode.PassthroughItem.Item.AverageColor)) {
                    graphics.FillEllipse(brush, inputPoint.X - 6, Math.Min(outputPoint.Y, inputPoint.Y) - 6 + ItemTabElement.TabWidth / 2, 12, 12);
                    graphics.FillEllipse(brush, inputPoint.X - 6, Math.Max(outputPoint.Y, inputPoint.Y) - 6 - ItemTabElement.TabWidth / 2, 12, 12);
                }

                if (!Highlighted)
                    return;

                using (var pen = new Pen(SelectionOverlayBrush, Math.Max(30, maxLineWidth + 10))) {
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    graphics.DrawLine(pen, inputPoint, outputPoint);
                }
            } else {
                InputTabs[0].HideItemTab = false;
                OutputTabs[0].HideItemTab = false;
                base.Draw(graphics, style);
            }
        }

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            if (_displayedNode.RateType != RateType.Manual)
                return;

            var yOffset = _displayedNode.NodeDirection == NodeDirection.Up ? 28 : 32;
            var titleSlot = new Rectangle(trans.X - Width / 2 + 5, trans.Y - Height / 2 + yOffset, Width - 10, 18);
            var textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 18, titleSlot.Width, 20);
            //graphics.DrawRectangle(devPen, textSlot);
            //graphics.DrawRectangle(devPen, titleSlot);

            graphics.DrawString("-Limit-", TitleFont, TextBrush, titleSlot, TitleFormat);
            GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, GraphicsStuff.DoubleToString(_displayedNode.DesiredRate), BaseFont, textSlot);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) {
            var tooltips = new List<TooltipInfo>();

            if (!exclusive)
                return tooltips;

            var helpToolTipInfo = new TooltipInfo {
                Text = $"Left click on this node to edit the throughput of {ItemName}.\nRight click for options.",
                Direction = Direction.None,
                ScreenLocation = new Point(10, 10)
            };
            tooltips.Add(helpToolTipInfo);

            return tooltips;
        }

        protected override void AddRClickMenuOptions(bool nodeInSelection) {
            RightClickMenu.Items.Add(new ToolStripSeparator());
            if (_displayedNode.SimpleDraw) {
                RightClickMenu.Items.Add(new ToolStripMenuItem("Don't simple-draw node", null,
                    (o, e) => {
                        RightClickMenu.Close();
                        ((PassthroughNodeController) GraphViewer.Graph.RequestNodeController(_displayedNode)).SetSimpleDraw(false);
                        GraphViewer.Invalidate();
                    }));
                if (GraphViewer.SelectedNodes.Count > 1 && GraphViewer.SelectedNodes.Contains(this)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Don't simple-draw selected nodes", null,
                        (o, e) => {
                            RightClickMenu.Close();
                            GraphViewer.SetSelectedPassthroughNodesSimpleDraw(false);
                            GraphViewer.Invalidate();
                        }));
                }
            } else {
                RightClickMenu.Items.Add(new ToolStripMenuItem("Simple-draw node", null,
                    (o, e) => {
                        RightClickMenu.Close();
                        ((PassthroughNodeController) GraphViewer.Graph.RequestNodeController(_displayedNode)).SetSimpleDraw(true);
                        GraphViewer.Invalidate();
                    }));
                if (GraphViewer.SelectedNodes.Count > 1 && GraphViewer.SelectedNodes.Contains(this)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Simple-draw selected nodes", null,
                        (o, e) => {
                            RightClickMenu.Close();
                            GraphViewer.SetSelectedPassthroughNodesSimpleDraw(true);
                            GraphViewer.Invalidate();
                        }));
                }
            }
        }
    }
}