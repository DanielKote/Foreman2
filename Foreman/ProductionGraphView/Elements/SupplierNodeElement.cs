using Foreman.Graph;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    public class SupplierNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return supplierBgBrush; } }
        private static Brush supplierBgBrush = new SolidBrush(Color.FromArgb(231, 214, 224));

        private ISupplierNodeViewModel SupplierViewModel => (ISupplierNodeViewModel)ViewModel;
        private string ItemName => SupplierViewModel.SuppliedItem.FriendlyName ?? "";

        public SupplierNodeElement(ProductionGraphViewer graphViewer, ISupplierNodeViewModel viewModel) : base(graphViewer, viewModel) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
        }

        protected override Bitmap? NodeIcon() => SupplierViewModel.SuppliedItem.Icon;

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            int yoffset = SupplierViewModel.NodeDirection == NodeDirection.Up ? 32 : 5;
            Rectangle titleSlot = new Rectangle(trans.X - (Width / 2) + 5, trans.Y - (Height / 2) + yoffset, Width - 10, 20);
            Rectangle textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 20, titleSlot.Width, (Height / 2) - 5);

            graphics.DrawString(SupplierViewModel.RateType == RateType.Auto ? "Infinite Source:" : "Exact Input:", TitleFont, TextBrush, titleSlot, TitleFormat);
            GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, ItemName, BaseFont, textSlot);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive) =>
            ExclusiveHelpTooltip(string.Format("Left click on this node to edit quantity of {0} produced.\nRight click for options.", ItemName), exclusive);
    }
}