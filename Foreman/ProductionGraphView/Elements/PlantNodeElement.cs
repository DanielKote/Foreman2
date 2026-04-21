using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Foreman {
    public class PlantNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush => PlantBgBrush;

        private static readonly Brush PlantBgBrush = new SolidBrush(Color.FromArgb(190, 217, 212));

        private static readonly StringFormat OwnTextFormat = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        private string InputName => _displayedNode.Seed.FriendlyName;

        private readonly ReadOnlyPlantNode _displayedNode;

        public PlantNodeElement(ProductionGraphViewer graphViewer, ReadOnlyPlantNode node) : base(graphViewer, node) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
            _displayedNode = node;

            UpdateState();
        }

        protected override void UpdateState() {
            // check for and update the output tabs in the case that the plant result items have changed
            // we can have multiple output items here, so go through all of them, delete any that aren't part of the correct outputs,
            // then add any that are missing.

            foreach (var oldTab in OutputTabs.Where(tab => !_displayedNode.Outputs.Contains(tab.Item)).ToList()) {
                OutputTabs.Remove(oldTab);
                oldTab.Dispose();
            }

            foreach (var item in _displayedNode.Outputs)
                if (OutputTabs.All(tab => tab.Item != item))
                    OutputTabs.Add(new ItemTabElement(item, LinkType.Output, GraphViewer, this));

            // update width based on number of output tabs

            Width = Math.Max(MinWidth, GetIconWidths(OutputTabs) + 10);
            if (Width % WidthD != 0) {
                Width += WidthD;
                Width -= Width % WidthD;
            }

            base.UpdateState();
        }

        protected override Bitmap NodeIcon() {
            return IconCache.GetPlantingIcon();
        }

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            // text

            var overproducing = _displayedNode.IsOverproducing();
            var textSlot = new Rectangle(trans.X - Width / 2 + 40, trans.Y - Height / 2 + (overproducing ? 32 : 27), Width - 10 - 40,
                Height - (overproducing ? 64 : 54));
            //graphics.DrawRectangle(devPen, textSlot);

            int textLength;

            if (GraphViewer.LevelOfDetail == ProductionGraphViewer.Lod.Low)
                textLength = GraphicsStuff.DrawText(graphics, TextBrush, OwnTextFormat, InputName + " Planting", BaseFont, textSlot);
            else
                textLength = GraphicsStuff.DrawText(graphics, TextBrush, OwnTextFormat, BuildingQuantityToText(_displayedNode.ActualSetValue) + " tiles",
                    CounterBaseFont, textSlot);

            // spoilage icon

            graphics.DrawImage(IconCache.GetPlantingIcon(), trans.X - Math.Min(Width / 2 - 10, textLength / 2 + 32), trans.Y - 16, 32, 32);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) {
            var tooltips = new List<TooltipInfo>();

            if (!exclusive)
                return tooltips;

            var helpToolTipInfo = new TooltipInfo {
                Text =
                    $"Left click on this node to edit the throughput of {InputName} Growth.\nxN quantity lists number of tiles required for throughput.\nRight click for options.",
                Direction = Direction.None,
                ScreenLocation = new Point(10, 10)
            };
            tooltips.Add(helpToolTipInfo);

            return tooltips;
        }
    }
}