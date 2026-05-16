using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman {
    public class PlantNodeElement : BaseNodeElement {
        protected override Brush CleanBgBrush { get { return plantBGBrush; } }
        private static readonly Brush plantBGBrush = new SolidBrush(Color.FromArgb(190, 217, 212));

        private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        private string InputName => DisplayedNode.Seed.FriendlyName ?? "";

        private new readonly ReadOnlyPlantNode DisplayedNode;

        public PlantNodeElement(ProductionGraphViewer graphViewer, ReadOnlyPlantNode node) : base(graphViewer, node) {
            Width = MinWidth;
            Height = BaseSimpleHeight;
            DisplayedNode = node;

            UpdateState();
        }

        protected override void UpdateState() {
            //check for and update the output tabs in the case that the plant result items have changed
            //we can have multiple output items here, so go through all of them, delete any that arent part of the correct outputs, then add any that are missing.
            foreach (ItemTabElement oldTab in OutputTabs.Where(tab => !DisplayedNode.Outputs.Contains(tab.Item)).ToList()) {
                OutputTabs.Remove(oldTab);
                oldTab.Dispose();
            }
            foreach (ItemQualityPair item in DisplayedNode.Outputs)
                if (!OutputTabs.Any(tab => tab.Item == item))
                    OutputTabs.Add(new ItemTabElement(item, LinkType.Output, graphViewer, this));

            //update width based on number of output tabs
            Width = Math.Max(MinWidth, GetIconWidths(OutputTabs) + 10);
            if (Width % WidthD != 0) {
                Width += WidthD;
                Width -= Width % WidthD;
            }

            base.UpdateState();
        }

        protected override Bitmap? NodeIcon() => IconCache.PlantingIcon;

        protected override void DetailsDraw(Graphics graphics, Point trans) {
            //text
            bool overproducing = DisplayedNode.IsOverproducing();
            Rectangle textSlot = new Rectangle(trans.X - (Width / 2) + 40, trans.Y - (Height / 2) + (overproducing ? 32 : 27), (Width - 10 - 40), Height - (overproducing ? 64 : 54));
            //graphics.DrawRectangle(devPen, textSlot);

            int textLength;

            if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low)
                textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, InputName + " Planting", BaseFont, textSlot);
            else
                textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, BuildingQuantityToText(DisplayedNode.ActualSetValue) + " tiles", CounterBaseFont, textSlot);

            //spoilage icon
            graphics.DrawImage(IconCache.PlantingIcon, trans.X - Math.Min((Width / 2) - 10, (textLength / 2) + 32), trans.Y - 16, 32, 32);
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive) {
            List<TooltipInfo> tooltips = new List<TooltipInfo>();

            if (exclusive) {
                TooltipInfo helpToolTipInfo = new TooltipInfo();
                helpToolTipInfo.Text = string.Format("Left click on this node to edit the throughput of {0} Growth.\nxN quantity lists number of tiles required for throughput.\nRight click for options.", InputName);
                helpToolTipInfo.Direction = Direction.None;
                helpToolTipInfo.ScreenLocation = new Point(10, 10);
                tooltips.Add(helpToolTipInfo);
            }

            return tooltips;
        }
    }
}