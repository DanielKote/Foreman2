using Foreman.Graph;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    public class ConsumerNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return consumerBgBrush; } }
        private static Brush consumerBgBrush = new SolidBrush(Color.FromArgb(249, 237, 195));

        private IConsumerNodeViewModel ConsumerViewModel => (IConsumerNodeViewModel)ViewModel;
        private string ItemName => ConsumerViewModel.ConsumedItem.FriendlyName ?? "";

        public ConsumerNodeElement(ProductionGraphViewer graphViewer, IConsumerNodeViewModel viewModel) : base(graphViewer, viewModel) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
        }

        protected override Bitmap? NodeIcon() => ConsumerViewModel.ConsumedItem.Icon;

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            int yoffset = ConsumerViewModel.NodeDirection == NodeDirection.Up ? 5 : 28;
            Rectangle titleSlot = new Rectangle(trans.X - (Width / 2) + 5, trans.Y - (Height / 2) + yoffset, Width - 10, 20);
            Rectangle textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 20, titleSlot.Width, (Height / 2) - 5);

            graphics.DrawString(ConsumerViewModel.RateType == RateType.Auto ? "Infinite Sink:" : "Required Output:", TitleFont, TextBrush, titleSlot, TitleFormat);
            GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, ItemName, BaseFont, textSlot);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive) =>
            ExclusiveHelpTooltip(string.Format("Left click on this node to edit quantity of {0} required.\nRight click for options.", ItemName), exclusive);
    }
}