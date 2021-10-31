using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class ErrorNoticeElement : GraphElement
	{
		private const int ErrorIconSize = 24;
		private static readonly Bitmap errorIcon = IconCache.GetIcon(Path.Combine("Graphics", "ErrorIcon.png"), 32);

		public ErrorNoticeElement(ProductionGraphViewer graphViewer, BaseNodeElement parent) : base(graphViewer, parent)
		{
			Width = ErrorIconSize;
			Height = ErrorIconSize;
		}

		public void SetVisibility(bool visible)
		{
			Visible = visible;
		}

		protected override void Draw(Graphics graphics)
		{
			Point trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
			graphics.DrawImage(errorIcon, trans.X, trans.Y, ErrorIconSize, ErrorIconSize);
		}

		public override List<TooltipInfo> GetToolTips(Point graph_point)
		{
			if (!Visible)
				return null;

			List<TooltipInfo> tooltips = new List<TooltipInfo>();
			TooltipInfo tti = new TooltipInfo();
			tti.Direction = Direction.Up;
			tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(0, Height/2)));
			string errors = ((BaseNodeElement)myParent).DisplayedNode.GetErrors();

			if (errors.EndsWith("\n"))
				errors = errors.Substring(0, errors.Length - 1);
			errors += "\nClick to auto-resolve.";
			tti.Text += errors;
			tooltips.Add(tti);

			return tooltips;
		}

		public override void MouseUp(Point graph_point, MouseButtons button, bool wasDragged)
		{
			((BaseNodeElement)myParent).DisplayedNode.AutoResolveErrors();
		}

	}
}
