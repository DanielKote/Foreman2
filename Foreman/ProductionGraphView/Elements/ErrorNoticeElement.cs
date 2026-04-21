using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Foreman {
    public class ErrorNoticeElement : GraphElement {
        private const int ErrorIconSize = 24;
        private static readonly Bitmap ErrorIcon = IconCache.GetIcon(Path.Combine("Graphics", "ErrorIcon.png"), 64);

        private readonly ReadOnlyBaseNode _displayedNode;

        public ErrorNoticeElement(ProductionGraphViewer graphViewer, BaseNodeElement parent) : base(graphViewer, parent) {
            _displayedNode = parent.DisplayedNode;
            Width = ErrorIconSize;
            Height = ErrorIconSize;
        }

        public void SetVisibility(bool visible) {
            Visible = visible;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (style == NodeDrawingStyle.IconsOnly)
                return;

            var trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
            graphics.DrawImage(ErrorIcon, trans.X, trans.Y, ErrorIconSize, ErrorIconSize);
        }

        public override List<TooltipInfo> GetToolTips(Point graphPoint) {
            if (!Visible)
                return null;

            List<string> text;
            var nodeController = GraphViewer.Graph.RequestNodeController(_displayedNode);
            switch (_displayedNode.State) {
                case NodeState.Error:
                    text = _displayedNode.GetErrors();
                    break;
                case NodeState.Warning:
                    text = _displayedNode.GetWarnings();
                    break;
                case NodeState.Clean:
                default:
                    return null;
            }

            if (text == null || text.Count == 0)
                return null;

            var tooltips = new List<TooltipInfo>();
            var tti = new TooltipInfo {
                Direction = Direction.Up,
                ScreenLocation = GraphViewer.GraphToScreen(LocalToGraph(new Point(0, Height / 2))),
                Text = ""
            };
            var solutionsAvailable = false;
            foreach (var s in text) {
                tti.Text += s + "\n";
                // we use > as the start of something solvable, and ?> as the start of 'no solution'
                solutionsAvailable |= s.StartsWith(">");
            }

            if (solutionsAvailable)
                tti.Text += "\nLeft click to auto-resolve.\nRight click for options.";
            tooltips.Add(tti);

            return tooltips;
        }

        public override void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) {
            if (!Visible)
                return;

            Dictionary<string, Action> resolutions = null;
            var nodeController = GraphViewer.Graph.RequestNodeController(_displayedNode);
            switch (((BaseNodeElement) MyParent).DisplayedNode.State) {
                case NodeState.Error:
                    resolutions = nodeController.GetErrorResolutions();
                    break;
                case NodeState.Warning:
                    resolutions = nodeController.GetWarningResolutions();
                    break;
                case NodeState.Clean:
                default:
                    return;
            }

            if (button == MouseButtons.Left) {
                foreach (var resolution in resolutions.Values)
                    resolution.Invoke();
                GraphViewer.Graph.UpdateNodeValues();
            } else if (button == MouseButtons.Right) {
                RightClickMenu.Items.Clear();
                if (resolutions.Count <= 0)
                    return;

                foreach (var kvp in resolutions)
                    RightClickMenu.Items.Add(new ToolStripMenuItem(kvp.Key, null, (o, e) => {
                        RightClickMenu.Close();
                        kvp.Value.Invoke();
                        GraphViewer.Graph.UpdateNodeValues();
                    }));

                RightClickMenu.Show(GraphViewer, GraphViewer.GraphToScreen(graphPoint));
            }
        }
    }
}