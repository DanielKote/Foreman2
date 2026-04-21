using System;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman {
    public class SpoilNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush => _spoilBgBrush;

        private static Brush _spoilBgBrush = new SolidBrush(Color.FromArgb(190, 217, 212));

        private static readonly StringFormat OwnTextFormat = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        private string InputName => _displayedNode.InputItem.FriendlyName;

        private readonly ReadOnlySpoilNode _displayedNode;

        public SpoilNodeElement(ProductionGraphViewer graphViewer, ReadOnlySpoilNode node) : base(graphViewer, node) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
            _displayedNode = node;

            UpdateState();
        }

        protected override void UpdateState() {
            // check for and update the output tab in the case that the spoil item has changed
            // we are guaranteed to have just 1 item in the output, so we just need to check if it needs to be changed,
            // if so delete it and make a new one

            var oldTab = OutputTabs[0];
            if (oldTab.Item != _displayedNode.OutputItem) {
                OutputTabs.Clear();
                oldTab.Dispose();

                OutputTabs.Add(new ItemTabElement(_displayedNode.OutputItem, LinkType.Output, GraphViewer, this));
            }

            base.UpdateState();
        }

        protected override Bitmap NodeIcon() {
            return IconCache.GetSpoilageIcon();
        }

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            // text

            var overproducing = _displayedNode.IsOverproducing();
            var textSlot = new Rectangle(trans.X - Width / 2 + 40, trans.Y - Height / 2 + (overproducing ? 32 : 27), Width - 10 - 40,
                Height - (overproducing ? 64 : 54));
            //graphics.DrawRectangle(devPen, textSlot);

            int textLength;

            if (GraphViewer.LevelOfDetail == ProductionGraphViewer.Lod.Low)
                textLength = GraphicsStuff.DrawText(graphics, TextBrush, OwnTextFormat, InputName + " Spoilage", BaseFont, textSlot);
            else
                textLength = GraphicsStuff.DrawText(graphics, TextBrush, OwnTextFormat, BuildingQuantityToText(_displayedNode.ActualSetValue) + " stacks",
                    CounterBaseFont, textSlot);

            // spoilage icon

            graphics.DrawImage(IconCache.GetSpoilageIcon(), trans.X - Math.Min(Width / 2 - 10, textLength / 2 + 32), trans.Y - 16, 32, 32);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) {
            var tooltips = new List<TooltipInfo>();

            if (!exclusive)
                return tooltips;

            var helpToolTipInfo = new TooltipInfo {
                Text =
                    $"Left click on this node to edit the throughput of {InputName} Spoilage.\nxN quantity lists number of slots required for throughput.\nRight click for options.",
                Direction = Direction.None,
                ScreenLocation = new Point(10, 10)
            };
            tooltips.Add(helpToolTipInfo);

            return tooltips;
        }
    }
}