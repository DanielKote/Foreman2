using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class SupplierNodeElement : BaseNodeElement
	{
		protected override Brush CleanBgBrush { get { return supplierBgBrush; } }
		private static Brush supplierBgBrush = new SolidBrush(Color.FromArgb(231, 214, 224));

		private static StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };

		public SupplierNodeElement(ProductionGraphViewer graphViewer, BaseNode node) : base(graphViewer, node)
		{
			Width = MinWidth;
			Height = BaseSimpleHeight;
		}

		public override void Update() { base.Update(); }

		protected override void DetailsDraw(Graphics graphics, Point trans)
		{
			string text = DisplayedNode.DisplayName;
			Rectangle textSlot = new Rectangle(trans.X - (Width / 2) + 5, trans.Y, (Width - 10), (Height / 2) - 5);
			//graphics.DrawRectangle(devPen, textSlot);

			GraphicsStuff.DrawText(graphics, TextBrush, textFormat, text, BaseFont, textSlot);
		}

		protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive)
		{
			List<TooltipInfo> tooltips = new List<TooltipInfo>();

			if (exclusive)
			{
				TooltipInfo helpToolTipInfo = new TooltipInfo();
				helpToolTipInfo.Text = string.Format("Left click on this node to edit quantity of {0} produced.\nRight click for options.", DisplayedNode.DisplayName);
				helpToolTipInfo.Direction = Direction.None;
				helpToolTipInfo.ScreenLocation = new Point(10, 10);
				tooltips.Add(helpToolTipInfo);
			}

			return tooltips;
		}
		protected override void MouseUpAction(Point graph_point, MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				DevNodeOptionsPanel newPanel = new DevNodeOptionsPanel(DisplayedNode, graphViewer);
				new FloatingTooltipControl(newPanel, Direction.Right, new Point(Location.X - (Width / 2), Location.Y), graphViewer);
			}
			else
				base.MouseUpAction(graph_point, button); //the standard menu
		}
	}
}
