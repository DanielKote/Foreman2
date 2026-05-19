using Foreman.Controls;
using Foreman.Graph;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman.ProductionGraphView.Elements {
    public class ConsumerNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return consumerBgBrush; } }
        private static readonly Brush consumerBgBrush = new SolidBrush(Color.FromArgb(249, 237, 195));

        private IConsumerNodeViewModel ConsumerViewModel => (IConsumerNodeViewModel)ViewModel;
        private string ItemName => ConsumerViewModel.ConsumedItem.FriendlyName ?? "";

        public ConsumerNodeElement(ProductionGraphViewer graphViewer, IConsumerNodeViewModel viewModel) : base(graphViewer, viewModel) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
        }

        protected override Bitmap? NodeIcon() => ConsumerViewModel.ConsumedItem.Icon;

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            int yoffset = ConsumerViewModel.NodeDirection == NodeDirection.Up ? 5 : 28;
            var titleSlot = new Rectangle(trans.X - (Width / 2) + 5, trans.Y - (Height / 2) + yoffset, Width - 10, 20);
            var textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 20, titleSlot.Width, (Height / 2) - 5);

            graphics.DrawString(ConsumerViewModel.RateType == RateType.Auto ? "Infinite Sink:" : "Required Output:", TitleFont, TextBrush, titleSlot, TitleFormat);
            GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, ItemName, BaseFont, textSlot);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) =>
            ExclusiveHelpTooltip(string.Format(DisplayCulture.Format, "Left click on this node to edit quantity of {0} required.\nRight click for options.", ItemName), exclusive);
    }
}
