using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public class SpoilNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return spoilBGBrush; } }
        private static Brush spoilBGBrush = new SolidBrush(Color.FromArgb(190, 217, 212));

        private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        private string InputName => DisplayedNode.InputItem.FriendlyName ?? "";

        private new readonly ReadOnlySpoilNode DisplayedNode;

        public SpoilNodeElement(ProductionGraphViewer graphViewer, ReadOnlySpoilNode node) : base(graphViewer, node) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
            DisplayedNode = node;

            UpdateState();
        }

        protected override void UpdateState() {
            //check for and update the output tab in the case that the spoil item has changed
            //we are guaranteed to have just 1 item in the output, so we just need to check if it needs to be changed, if so delete it and make a new one
            ItemTabElement oldTab = OutputTabs[0];
            if (oldTab.Item != DisplayedNode.OutputItem) {
                OutputTabs.Clear();
                oldTab.Dispose();

                OutputTabs.Add(new ItemTabElement(DisplayedNode.OutputItem, LinkType.Output, graphViewer, this));
            }

            base.UpdateState();
        }

        protected override Bitmap? NodeIcon() => IconCache.SpoilageIcon;

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            //text
            bool overproducing = DisplayedNode.IsOverproducing();
            Rectangle textSlot = new Rectangle(trans.X - (Width / 2) + 40, trans.Y - (Height / 2) + (overproducing ? 32 : 27), (Width - 10 - 40), Height - (overproducing ? 64 : 54));
            //graphics.DrawRectangle(devPen, textSlot);

            int textLength;

            if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low)
                textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, InputName + " Spoilage", BaseFont, textSlot);
            else
                textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, BuildingQuantityToText(DisplayedNode.ActualSetValue) + " stacks", CounterBaseFont, textSlot);

            //spoilage icon
            graphics.DrawImage(IconCache.SpoilageIcon, trans.X - Math.Min((Width / 2) - 10, (textLength / 2) + 32), trans.Y - 16, 32, 32);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive) {
            List<TooltipInfo> tooltips = new List<TooltipInfo>();

            if (exclusive) {
                TooltipInfo helpToolTipInfo = new TooltipInfo();
                helpToolTipInfo.Text = string.Format("Left click on this node to edit the throughput of {0} Spoilage.\nxN quantity lists number of slots required for throughput.\nRight click for options.", InputName);
                helpToolTipInfo.Direction = Direction.None;
                helpToolTipInfo.ScreenLocation = new Point(10, 10);
                tooltips.Add(helpToolTipInfo);
            }

            return tooltips;
        }
    }
}