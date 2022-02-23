using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Foreman
{
    public class LabelNodeElement : BaseNodeElement
    {
        private new readonly ReadOnlyLabelNode DisplayedNode;

        protected override Brush CleanBgBrush { get { return labelBgBrush; } }
        private static Brush labelBgBrush = new SolidBrush(Color.FromArgb(249, 237, 195));
        protected override Bitmap NodeIcon() { return null; }

        public LabelNodeElement(ProductionGraphViewer graphViewer, ReadOnlyLabelNode parent) : base(graphViewer, parent)
        {
            Width = 20;
            Height = 20;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style)
        {
            Point trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
            graphics.DrawRectangle(devPen, trans.X, trans.Y, Width, Height);
        }

        protected override void DetailsDraw(Graphics graphics, Point trans)
        {
            //MR:
        }

        protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive)
        {
            List<TooltipInfo> tooltips = new List<TooltipInfo>();

            if (exclusive)
            {
                TooltipInfo helpToolTipInfo = new TooltipInfo();
                helpToolTipInfo.Text = string.Format("Label", "Label");
                helpToolTipInfo.Direction = Direction.None;
                helpToolTipInfo.ScreenLocation = new Point(10, 10);
                tooltips.Add(helpToolTipInfo);
            }

            return tooltips;
        }
    }
}
