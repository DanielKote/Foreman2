using System.Collections.Generic;
using System.Drawing;

namespace Foreman {
    public class SupplierNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush => _supplierBgBrush;

        private static Brush _supplierBgBrush = new SolidBrush(Color.FromArgb(231, 214, 224));

        private string ItemName => _displayedNode.SuppliedItem.FriendlyName;

        private readonly ReadOnlySupplierNode _displayedNode;

        public SupplierNodeElement(ProductionGraphViewer graphViewer, ReadOnlySupplierNode node) : base(graphViewer, node) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
            _displayedNode = node;
        }

        protected override Bitmap NodeIcon() {
            return _displayedNode.SuppliedItem.Icon;
        }

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            var yoffset = _displayedNode.NodeDirection == NodeDirection.Up ? 32 : 5;
            var titleSlot = new Rectangle(trans.X - Width / 2 + 5, trans.Y - Height / 2 + yoffset, Width - 10, 20);
            var textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 20, titleSlot.Width, Height / 2 - 5);
            //graphics.DrawRectangle(devPen, textSlot);
            //graphics.DrawRectangle(devPen, titleSlot);

            graphics.DrawString(_displayedNode.RateType == RateType.Auto ? "Infinite Source:" : "Exact Input:", TitleFont, TextBrush, titleSlot, TitleFormat);
            GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, ItemName, BaseFont, textSlot);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graphPoint, bool exclusive) {
            var tooltips = new List<TooltipInfo>();

            if (exclusive) {
                var helpToolTipInfo = new TooltipInfo {
                    Text = $"Left click on this node to edit quantity of {ItemName} produced.\nRight click for options.",
                    Direction = Direction.None,
                    ScreenLocation = new Point(10, 10)
                };
                tooltips.Add(helpToolTipInfo);
            }

            return tooltips;
        }
    }
}