using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class ConsumerNodeElement : BaseNodeElement
	{
		protected override Brush CleanBgBrush { get { return consumerBgBrush; } }
		private static Brush consumerBgBrush = new SolidBrush(Color.FromArgb(249, 237, 195));

		public ConsumerNodeElement(ProductionGraphViewer graphViewer, BaseNode node) : base(graphViewer, node)
		{
			Width = MinWidth;
			Height = BaseSimpleHeight;
		}

		public override void Update() { base.Update(); }

		protected override void DetailsDraw(Graphics graphics, Point trans, bool simple)
		{
			if (simple)
				return;

			string text = DisplayedNode.DisplayName;
			Rectangle titleSlot = new Rectangle(trans.X - (Width / 2) + 5, trans.Y - (Height / 2) + 5, Width - 10, 20);
			Rectangle textSlot = new Rectangle(titleSlot.X, titleSlot.Y + 20, titleSlot.Width, (Height / 2) - 5);
			//graphics.DrawRectangle(devPen, textSlot);
			//graphics.DrawRectangle(devPen, titleSlot);

			graphics.DrawString(DisplayedNode.RateType == RateType.Auto ? "Infinite Sink:" : "Required Output:", TitleFont, TextBrush, titleSlot, TitleFormat);
			GraphicsStuff.DrawText(graphics, TextBrush, TextFormat, text, BaseFont, textSlot);
		}

		protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive)
		{
			List<TooltipInfo> tooltips = new List<TooltipInfo>();

			if (exclusive)
			{
				TooltipInfo helpToolTipInfo = new TooltipInfo();
				helpToolTipInfo.Text = string.Format("Left click on this node to edit quantity of {0} required.\nRight click for options.", DisplayedNode.DisplayName);
				helpToolTipInfo.Direction = Direction.None;
				helpToolTipInfo.ScreenLocation = new Point(10, 10);
				tooltips.Add(helpToolTipInfo);
			}

			return tooltips;
		}
	}
}
