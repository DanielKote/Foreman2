using System.Collections.Generic;
using System.Drawing;

namespace Foreman {
    public class ConsumerNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush => _consumerBgBrush;

        private static Brush _consumerBgBrush = new SolidBrush(Color.FromArgb(249, 237, 195));

        private string ItemName => _displayedNode.ConsumedItem.FriendlyName;

        private readonly ReadOnlyConsumerNode _displayedNode;

        public ConsumerNodeElement(ProductionGraphViewer graphViewer, ReadOnlyConsumerNode node) : base(graphViewer, node) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
            _displayedNode = node;
        }

        protected override Bitmap NodeIcon() {
            return _displayedNode.ConsumedItem.Icon;
        }

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            var yOffset = _displayedNode.NodeDirection == NodeDirection.Up ? 5 : 28;
            var titleSlot = new Rectangle(trans.X - Width / 2 + 5, trans.Y - Height / 2 + yOffset, Width - 10, 20);
            var textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 20, titleSlot.Width, Height / 2 - 5);
            //graphics.DrawRectangle(devPen, textSlot);
            //graphics.DrawRectangle(devPen, titleSlot);

            graphics.DrawString(_displayedNode.RateType == RateType.Auto ? "Infinite Sink:" : "Required Output:", TitleFont, TextBrush, titleSlot, TitleFormat);
            GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, ItemName, BaseFont, textSlot);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) {
            var tooltips = new List<TooltipInfo>();

            if (!exclusive)
                return tooltips;

            var helpToolTipInfo = new TooltipInfo {
                Text = $"Left click on this node to edit quantity of {ItemName} required.\nRight click for options.",
                Direction = Direction.None,
                ScreenLocation = new Point(10, 10)
            };
            tooltips.Add(helpToolTipInfo);

            return tooltips;
        }
    }
}