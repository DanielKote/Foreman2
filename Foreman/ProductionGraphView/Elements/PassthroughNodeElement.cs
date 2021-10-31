using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class PassthroughNodeElement : BaseNodeElement
	{
		protected override Brush CleanBgBrush { get { return passthroughBGBrush; } }
		private static Brush passthroughBGBrush = new SolidBrush(Color.FromArgb(190, 217, 212));
		private PassthroughNode passthroughNode { get { return (PassthroughNode)DisplayedNode; } }


		public PassthroughNodeElement(ProductionGraphViewer graphViewer, BaseNode node) : base(graphViewer, node)
		{
			Width = PassthroughNodeWidth;
			Height = BaseSimpleHeight;
		}

		public override void Update() { base.Update(); }

		protected override void DetailsDraw(Graphics graphics, Point trans)
		{
			if (DisplayedNode.RateType == RateType.Manual)
			{
				Rectangle titleSlot = new Rectangle(trans.X - (Width / 2) + 5, trans.Y - (Height / 2) + 26, Width - 10, 18);
				Rectangle textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 18, titleSlot.Width, 20);
				//graphics.DrawRectangle(devPen, textSlot);
				//graphics.DrawRectangle(devPen, titleSlot);

				graphics.DrawString("-Limit-", TitleFont, TextBrush, titleSlot, TitleFormat);
				GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, GraphicsStuff.FloatToString(DisplayedNode.DesiredRate), BaseFont, textSlot);
			}
		}

		protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive)
		{
			List<TooltipInfo> tooltips = new List<TooltipInfo>();

			if (exclusive)
			{
				TooltipInfo helpToolTipInfo = new TooltipInfo();
				helpToolTipInfo.Text = string.Format("Left click on this node to edit the throughput of {0}.\nRight click for options.", passthroughNode.PassthroughItem.FriendlyName);
				helpToolTipInfo.Direction = Direction.None;
				helpToolTipInfo.ScreenLocation = new Point(10, 10);
				tooltips.Add(helpToolTipInfo);
			}

			return tooltips;
		}
	}
}
