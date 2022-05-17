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

        private Font labelFont; 

        protected override Brush CleanBgBrush { get { return labelBgBrush; } }
        private static Brush labelBgBrush = new SolidBrush(Color.FromArgb(249, 237, 195));
        protected override Bitmap NodeIcon() { return null; }

        private readonly ProductionGraphViewer PGV;
        public LabelNodeElement(ProductionGraphViewer graphViewer, ReadOnlyLabelNode parent) : base(graphViewer, parent)
        {
            DisplayedNode = parent;
            PGV = graphViewer;
            labelFont = new Font(FontFamily.GenericSansSerif, parent.MyNode.LabelSize, FontStyle.Bold);
            CalculateSize();
        }

        public void ChangeFontSize()
        {
            labelFont = new Font(FontFamily.GenericSansSerif, DisplayedNode.MyNode.LabelSize, FontStyle.Bold);
        }
        public void CalculateSize()
        {
            SizeF stringSize = PGV.CreateGraphics().MeasureString(DisplayedNode.MyNode.LabelText, labelFont);
            Width = (int)stringSize.Width;
            Height = (int)stringSize.Height;
        }
        protected override void DetailsDraw(Graphics graphics, Point trans)
        {
            ChangeFontSize();
            CalculateSize();
            graphics.DrawString(DisplayedNode.MyNode.LabelText, labelFont, Brushes.Black, trans.X - (int)(Width/2), trans.Y - (int)(Height/2));
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
