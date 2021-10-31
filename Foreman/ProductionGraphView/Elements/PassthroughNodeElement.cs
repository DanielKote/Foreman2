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

		protected override void DetailsDraw(Graphics graphics, Point trans) { }

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
