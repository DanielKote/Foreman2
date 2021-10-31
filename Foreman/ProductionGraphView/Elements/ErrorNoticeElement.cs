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
		private static readonly Bitmap errorIcon = IconCache.GetIcon(Path.Combine("Graphics", "ErrorIcon.png"), 64);

		public ErrorNoticeElement(ProductionGraphViewer graphViewer, BaseNodeElement parent) : base(graphViewer, parent)
		{
			Width = ErrorIconSize;
			Height = ErrorIconSize;
		}

		public void SetVisibility(bool visible)
		{
			Visible = visible;
		}

		protected override void Draw(Graphics graphics, bool simple)
		{
			if (simple)
				return;

			Point trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
			graphics.DrawImage(errorIcon, trans.X, trans.Y, ErrorIconSize, ErrorIconSize);
		}

		public override List<TooltipInfo> GetToolTips(Point graph_point)
		{
			if (!Visible)
				return null;

			List<string> text = null;
			switch (((BaseNodeElement)myParent).DisplayedNode.State)
			{
				case NodeState.Error:
					text = ((BaseNodeElement)myParent).DisplayedNode.GetErrors();
					break;
				case NodeState.Warning:
					text = ((BaseNodeElement)myParent).DisplayedNode.GetWarnings();
					break;
				case NodeState.Clean:
				default:
					return null;
			}
			if (text == null || text.Count == 0)
				return null;

			List<TooltipInfo> tooltips = new List<TooltipInfo>();
			TooltipInfo tti = new TooltipInfo();
			tti.Direction = Direction.Up;
			tti.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(0, Height / 2)));
			tti.Text = "";
			bool solutionsAvailable = false;
			for (int i = 0; i < text.Count; i++)
			{
				tti.Text += text[i] + "\n";
				solutionsAvailable |= text[i].StartsWith(">"); //we use > as the start of something solvable, and ?> as the start of 'no solution'
			}
			if (solutionsAvailable)
				tti.Text += "\nLeft click to autoresolve.\nRight click for options.";
			tooltips.Add(tti);

			return tooltips;
		}

		public override void MouseUp(Point graph_point, MouseButtons button, bool wasDragged)
		{
			if (!Visible)
				return;

			Dictionary<string, Action> resolutions = null;
			switch (((BaseNodeElement)myParent).DisplayedNode.State)
			{
				case NodeState.Error:
					resolutions = ((BaseNodeElement)myParent).DisplayedNode.GetErrorResolutions();
					break;
				case NodeState.Warning:
					resolutions = ((BaseNodeElement)myParent).DisplayedNode.GetWarningResolutions();
					break;
				case NodeState.Clean:
				default:
					return;
			}

			if (button == MouseButtons.Left)
			{
				foreach (Action resolution in resolutions.Values)
					resolution.Invoke();
				((BaseNodeElement)myParent).Update();
			}
			else if (button == MouseButtons.Right)
			{
				RightClickMenu.MenuItems.Clear();
				if (resolutions.Count > 0)
				{
					foreach (KeyValuePair<string, Action> kvp in resolutions)
						RightClickMenu.MenuItems.Add(new MenuItem(kvp.Key, new EventHandler((o, e) =>
						{
							kvp.Value.Invoke();
							((BaseNodeElement)myParent).Update();
						})));

					RightClickMenu.Show(graphViewer, graphViewer.GraphToScreen(graph_point));
				}
			}
		}
	}
}
