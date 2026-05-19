using Foreman.Controls;
using Foreman.DataCaching;
using Foreman.Graph;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman.ProductionGraphView.Elements {
    public class SpoilNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return spoilBGBrush; } }
        private static readonly Brush spoilBGBrush = new SolidBrush(Color.FromArgb(190, 217, 212));

        private static readonly StringFormat textFormat = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        private ISpoilNodeViewModel SpoilViewModel => (ISpoilNodeViewModel)ViewModel;
        private string InputName => SpoilViewModel.InputItem.FriendlyName ?? "";

        public SpoilNodeElement(ProductionGraphViewer graphViewer, ISpoilNodeViewModel viewModel) : base(graphViewer, viewModel) {
            Width = MinWidth;
            Height = BaseSimpleHeight;

            UpdateState();
        }

        protected override void UpdateState() {
            //check for and update the output tab in the case that the spoil item has changed
            //we are guaranteed to have just 1 item in the output, so we just need to check if it needs to be changed, if so delete it and make a new one
            ItemTabElement oldTab = OutputTabs[0];
            if (oldTab.Item != SpoilViewModel.OutputItem) {
                OutputTabs.Clear();
                oldTab.Dispose();

                OutputTabs.Add(new ItemTabElement(SpoilViewModel.OutputItem, LinkType.Output, graphViewer, this));
            }

            base.UpdateState();
        }

        protected override Bitmap? NodeIcon() => IconCache.SpoilageIcon;

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            //text
            bool overproducing = SpoilViewModel.IsOverproducing();
            var textSlot = new Rectangle(trans.X - (Width / 2) + 40, trans.Y - (Height / 2) + (overproducing ? 32 : 27), (Width - 10 - 40), Height - (overproducing ? 64 : 54));
            //graphics.DrawRectangle(devPen, textSlot);

            var textLength = graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low
                ? GraphicsStuff.DrawText(graphics, TextBrush, textFormat, InputName + " Spoilage", BaseFont, textSlot)
                : GraphicsStuff.DrawText(graphics, TextBrush, textFormat, BuildingQuantityToText(SpoilViewModel.ActualSetValue) + " stacks", CounterBaseFont, textSlot);

            //spoilage icon
            graphics.DrawImage(IconCache.SpoilageIcon, trans.X - Math.Min((Width / 2) - 10, (textLength / 2) + 32), trans.Y - 16, 32, 32);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) =>
            ExclusiveHelpTooltip(string.Format(DisplayCulture.Format, "Left click on this node to edit the throughput of {0} Spoilage.\nxN quantity lists number of slots required for throughput.\nRight click for options.", InputName), exclusive);
    }
}
