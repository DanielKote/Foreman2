using Foreman.DataCaching;
using Foreman.Graph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Foreman.ProductionGraphView.Elements {
    public class ErrorNoticeElement : GraphElement {
        private const int ErrorIconSize = 24;
        private static readonly Bitmap errorIcon = IconCache.GetIcon(Path.Combine("Graphics", "ErrorIcon.png"), 64);

        private readonly INodeViewModel NodeViewModel;

        public ErrorNoticeElement(ProductionGraphViewer graphViewer, BaseNodeElement parent) : base(graphViewer, parent) {
            NodeViewModel = parent.ViewModel;
            Width = ErrorIconSize;
            Height = ErrorIconSize;
        }

        public void SetVisibility(bool visible) {
            Visible = visible;
        }

        protected override void Draw(Graphics graphics, NodeDrawingStyle style) {
            if (style == NodeDrawingStyle.IconsOnly)
                return;

            Point trans = LocalToGraph(new Point(-Width / 2, -Height / 2));
            graphics.DrawImage(errorIcon, trans.X, trans.Y, ErrorIconSize, ErrorIconSize);
        }

        public override List<TooltipInfo>? GetToolTips(Point graphPoint) {
            if (!Visible)
                return null;

            List<string>? text;
            switch (NodeViewModel.State) {
                case NodeState.Error:
                    text = NodeViewModel.GetErrors();
                    break;
                case NodeState.Warning:
                    text = NodeViewModel.GetWarnings();
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
                ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(0, Height / 2))),
                Text = ""
            };
            bool solutionsAvailable = false;
            for (int i = 0; i < text.Count; i++) {
                tti.Text += text[i] + "\n";
                solutionsAvailable |= text[i].StartsWith('>'); //we use > as the start of something solvable, and ?> as the start of 'no solution'
            }
            if (solutionsAvailable)
                tti.Text += "\nLeft click to autoresolve.\nRight click for options.";
            tooltips.Add(tti);

            return tooltips;
        }

        public override void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) {
            if (!Visible)
                return;

            Dictionary<string, Action>? resolutions;
            if (myParent is not BaseNodeElement parentNode)
                return;
            if (graphViewer.Session.Editor.RequestNodeController(NodeViewModel.Id) is not BaseNodeController nodeController)
                return;
            switch (parentNode.ViewModel.State) {
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

            if (resolutions == null)
                return;

            if (button == MouseButtons.Left) {
                foreach (Action resolution in resolutions.Values)
                    resolution.Invoke();
                graphViewer.Graph.UpdateNodeValues();
            } else if (button == MouseButtons.Right) {
                RightClickMenu.Items.Clear();
                if (resolutions.Count > 0) {
                    foreach (KeyValuePair<string, Action> kvp in resolutions)
                        RightClickMenu.Items.Add(new ToolStripMenuItem(kvp.Key, null, new EventHandler((o, e) => {
                            RightClickMenu.Close();
                            kvp.Value.Invoke();
                            graphViewer.Graph.UpdateNodeValues();
                        })));

                    RightClickMenu.Show(graphViewer, graphViewer.GraphToScreen(graphPoint));
                }
            }
        }
    }
}
