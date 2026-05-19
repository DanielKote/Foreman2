using Foreman.Controls;
using Foreman.Graph;
using System.Collections.Generic;
using System.Drawing;

namespace Foreman.ProductionGraphView.Elements {
    public class SupplierNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return supplierBgBrush; } }
        private static readonly Brush supplierBgBrush = new SolidBrush(Color.FromArgb(231, 214, 224));

        private ISupplierNodeViewModel SupplierViewModel => (ISupplierNodeViewModel)ViewModel;
        private string ItemName => SupplierViewModel.SuppliedItem.FriendlyName ?? "";

        public SupplierNodeElement(ProductionGraphViewer graphViewer, ISupplierNodeViewModel viewModel) : base(graphViewer, viewModel) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
        }

        protected override Bitmap? NodeIcon() => SupplierViewModel.SuppliedItem.Icon;

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            int yoffset = SupplierViewModel.NodeDirection == NodeDirection.Up ? 32 : 5;
            var titleSlot = new Rectangle(trans.X - (Width / 2) + 5, trans.Y - (Height / 2) + yoffset, Width - 10, 20);
            var textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 20, titleSlot.Width, (Height / 2) - 5);

            graphics.DrawString(SupplierViewModel.RateType == RateType.Auto ? "Infinite Source:" : "Exact Input:", TitleFont, TextBrush, titleSlot, TitleFormat);
            GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, ItemName, BaseFont, textSlot);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) =>
            ExclusiveHelpTooltip(string.Format(DisplayCulture.Format, "Left click on this node to edit quantity of {0} produced.\nRight click for options.", ItemName), exclusive);
    }
}
