using Foreman.Controls;
using Foreman.DataCaching.DataTypes;
using Foreman.Graph;
using Foreman.Models.Nodes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    public class PassthroughNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return passthroughBGBrush; } }
        private static readonly Brush passthroughBGBrush = new SolidBrush(Color.FromArgb(200, 200, 200));

        private IPassthroughNodeViewModel PassthroughViewModel => (IPassthroughNodeViewModel)ViewModel;
        private string ItemName => PassthroughViewModel.PassthroughItem.FriendlyName ?? "";

        public PassthroughNodeElement(ProductionGraphViewer graphViewer, IPassthroughNodeViewModel viewModel) : base(graphViewer, viewModel) {
            Width = PassthroughNodeWidth;
            Height = BaseSimpleHeight;
        }

        protected override Bitmap? NodeIcon() => null;

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (style != NodeDrawingStyle.IconsOnly && PassthroughViewModel.SimpleDraw && PassthroughViewModel.RateType == RateType.Auto && !PassthroughViewModel.KeyNode && !PassthroughViewModel.IsOverproducing() && !PassthroughViewModel.ManualRateNotMet() && PassthroughViewModel.InputLinks.Any() && PassthroughViewModel.OutputLinks.Any()) {
                InputTabs[0].HideItemTab = true;
                OutputTabs[0].HideItemTab = true;

                float maxLineWidth = PassthroughViewModel.InputLinks.Concat(PassthroughViewModel.OutputLinks).Select(l => graphViewer.GetLinkElement(l.Id)?.LinkWidth ?? 0).Max();
                Point inputPoint = InputTabs[0].GetConnectionPoint();
                Point outputPoint = OutputTabs[0].GetConnectionPoint();
                if (PassthroughViewModel.PassthroughItem.Item is not IItem passthroughItem)
                    return;
                using (var pen = new Pen(passthroughItem.AverageColor, maxLineWidth) { EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round })
                    graphics.DrawLine(pen, inputPoint, outputPoint);
                if (style == NodeDrawingStyle.Regular) {
                    using (Brush brush = new SolidBrush(passthroughItem.AverageColor)) {
                        graphics.FillEllipse(brush, inputPoint.X - 6, Math.Min(outputPoint.Y, inputPoint.Y) - 6 + (ItemTabElement.TabWidth / 2), 12, 12);
                        graphics.FillEllipse(brush, inputPoint.X - 6, Math.Max(outputPoint.Y, inputPoint.Y) - 6 - (ItemTabElement.TabWidth / 2), 12, 12);
                    }
                    if (Highlighted)
                        using (var pen = new Pen(selectionOverlayBrush, Math.Max(30, maxLineWidth + 10)) { EndCap = System.Drawing.Drawing2D.LineCap.Round, StartCap = System.Drawing.Drawing2D.LineCap.Round })
                            graphics.DrawLine(pen, inputPoint, outputPoint);
                }
            } else {
                InputTabs[0].HideItemTab = false;
                OutputTabs[0].HideItemTab = false;
                base.Draw(graphics, style);
            }
        }

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            if (PassthroughViewModel.RateType == RateType.Manual) {
                int yoffset = PassthroughViewModel.NodeDirection == NodeDirection.Up ? 28 : 32;
                var titleSlot = new Rectangle(trans.X - (Width / 2) + 5, trans.Y - (Height / 2) + yoffset, Width - 10, 18);
                var textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 18, titleSlot.Width, 20);
                //graphics.DrawRectangle(devPen, textSlot);
                //graphics.DrawRectangle(devPen, titleSlot);

                graphics.DrawString("-Limit-", TitleFont, TextBrush, titleSlot, TitleFormat);
                GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, GraphicsStuff.DoubleToString(PassthroughViewModel.DesiredRate), BaseFont, textSlot);
            }
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) =>
            ExclusiveHelpTooltip(string.Format(DisplayCulture.Format, "Left click on this node to edit the throughput of {0}.\nRight click for options.", ItemName), exclusive);

        protected override void AddRClickMenuOptions(bool nodeInSelection) {
            RightClickMenu.Items.Add(new ToolStripSeparator());
            if (PassthroughViewModel.SimpleDraw) {
                RightClickMenu.Items.Add(new ToolStripMenuItem("Dont simple-draw node", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        if (graphViewer.Session.Editor.RequestNodeController(PassthroughViewModel.Id) is PassthroughNodeController passthroughController)
                            passthroughController.SetSimpleDraw(false);
                        graphViewer.Invalidate();
                    })));
                if (graphViewer.SelectedNodes.Count > 1 && graphViewer.SelectedNodes.Contains(this)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Dont simple-draw selected nodes", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            graphViewer.SetSelectedPassthroughNodesSimpleDraw(false);
                            graphViewer.Invalidate();
                        })));
                }
            } else {
                RightClickMenu.Items.Add(new ToolStripMenuItem("Simple-draw node", null,
                    new EventHandler((o, e) => {
                        RightClickMenu.Close();
                        if (graphViewer.Session.Editor.RequestNodeController(PassthroughViewModel.Id) is PassthroughNodeController passthroughController)
                            passthroughController.SetSimpleDraw(true);
                        graphViewer.Invalidate();
                    })));
                if (graphViewer.SelectedNodes.Count > 1 && graphViewer.SelectedNodes.Contains(this)) {
                    RightClickMenu.Items.Add(new ToolStripMenuItem("Simple-draw selected nodes", null,
                        new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            graphViewer.SetSelectedPassthroughNodesSimpleDraw(true);
                            graphViewer.Invalidate();
                        })));
                }
            }
        }
    }
}
