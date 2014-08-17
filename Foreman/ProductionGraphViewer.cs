using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Foreman
{
	public partial class ProductionGraphViewer : UserControl
	{
		public Dictionary<ProductionNode, ProductionNodeViewer> nodeControls = new Dictionary<ProductionNode, ProductionNodeViewer>();
		public ProductionGraph graph = new ProductionGraph();
		private List<Item> Demands = new List<Item>();
		public bool IsBeingDragged { get; private set; }
		private Point lastMouseDragPoint;
		public Point viewOffset;

		private Rectangle graphBounds
		{
			get
			{
				int width = 0;
				int height = 0;
				foreach (ProductionNodeViewer viewer in nodeControls.Values)
				{
					height = Math.Max(viewer.Y + viewer.Height, height);
					width = Math.Max(viewer.X + viewer.Width, width);
				}
				return new Rectangle(viewOffset.X, viewOffset.Y, width + 20, height + 20);
			}
		}
		
		public ProductionGraphViewer()
		{
			InitializeComponent();

			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		}

		public void AddDemand(Item item)
		{	
			graph.Nodes.Add(new ConsumerNode(item, 1f, graph));
			while (!graph.Complete)
			{
				graph.IterateNodeDemands();
			}
			CreateMissingControls();

			foreach (ProductionNodeViewer node in nodeControls.Values)
			{
				node.Update();
			}
			PositionControls();
			Invalidate(true);
		}

		private void CreateMissingControls()
		{
			foreach (ProductionNode node in graph.Nodes)
			{
				if (!nodeControls.ContainsKey(node))
				{
					ProductionNodeViewer nodeViewer = new ProductionNodeViewer(node);
					nodeViewer.Parent = this;
					nodeControls.Add(node, nodeViewer);
				}
			}
		}

		private void DrawConnections(Graphics graphics)
		{
			foreach (var n in nodeControls.Keys)
			{
				foreach (var m in nodeControls.Keys)
				{
					if (m.CanTakeFrom(n))
					{
						foreach (Item item in m.Inputs.Keys.Intersect(n.Outputs.Keys))
						{
							Point pointN = nodeControls[n].getOutputLineConnectionPoint(item);
							Point pointM = nodeControls[m].getInputLineConnectionPoint(item);
							Point pointN2 = new Point(pointN.X, pointN.Y - Math.Max((int)((pointN.Y - pointM.Y) / 2), 40));
							Point pointM2 = new Point(pointM.X, pointM.Y + Math.Max((int)((pointN.Y - pointM.Y) / 2), 40));

							using (Pen pen = new Pen(DataCache.IconAverageColour(item.Icon), 3f))
							{
								graphics.DrawBezier(pen, pointN, pointN2, pointM2, pointM);
							}
						}
					}
				}
			}
		}

		private void PositionControls()
		{
			if (!nodeControls.Any())
			{
				return;
			}
			var nodeOrder = graph.GetTopologicalSort();
			nodeOrder.Reverse();
			var pathMatrix = graph.PathMatrix;

			List<ProductionNode>[] nodePositions = new List<ProductionNode>[nodeOrder.Count()];
			for (int i = 0; i < nodePositions.Count(); i++)
			{
				nodePositions[i] = new List<ProductionNode>();
			}

			nodePositions.First().AddRange(nodeOrder.OfType<ConsumerNode>());
			foreach (RecipeNode node in nodeOrder.OfType<RecipeNode>())
			{
				bool PositionFound = false;

				for (int i = nodePositions.Count() - 1; i >= 0 && !PositionFound; i--)
				{
					foreach (ProductionNode listNode in nodePositions[i])
					{
						if (listNode.CanUltimatelyTakeFrom(node))
						{
							nodePositions[i + 1].Add(node);
							PositionFound = true;
							break;
						}
					}
				}

				if (!PositionFound)
				{
					nodePositions.First().Add(node);
				}
			}
			nodePositions.Last().AddRange(nodeOrder.OfType<SupplyNode>());

			int margin = 60;
			int y = margin;
			int[] tierWidths = new int[nodePositions.Count()];
			for (int i = 0; i < nodePositions.Count(); i++)
			{
				var list = nodePositions[i];
				int maxHeight = 0;
				int x = margin;

				foreach (var node in list)
				{
					if (!nodeControls.ContainsKey(node))
					{
						continue;
					}
					ProductionNodeViewer control = nodeControls[node];
					control.X = x;
					control.Y = y;

					x += control.Width + margin;
					maxHeight = Math.Max(control.Height, maxHeight);
				}

				if (maxHeight > 0) // Don't add any height for empty tiers
				{
					y += maxHeight + margin;
				}

				tierWidths[i] = x;
			}

			int centrePoint = tierWidths.Last(i => i > margin) / 2;
			for (int i = tierWidths.Count() - 1; i >= 0; i--)
			{
				int offset = centrePoint - tierWidths[i] / 2;

				foreach (var node in nodePositions[i])
				{
					nodeControls[node].X = nodeControls[node].X + offset;
				}
			}

			Invalidate(true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.Clear(this.BackColor);
			e.Graphics.TranslateTransform(viewOffset.X, viewOffset.Y);
			DrawConnections(e.Graphics);

			foreach (ProductionNodeViewer viewer in nodeControls.Values)
			{
				e.Graphics.TranslateTransform(viewer.X, viewer.Y);
				viewer.Paint(e.Graphics);
				e.Graphics.TranslateTransform(-viewer.X, -viewer.Y);
			}
		}

		private void ProductionGraphViewer_MouseDown(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Left:
					foreach (ProductionNodeViewer viewer in nodeControls.Values)
					{
						if (viewer.screenBounds.Contains(e.Location))
						{
							viewer.IsBeingDragged = true;
							viewer.DragOffsetX = e.X - viewer.X;
							viewer.DragOffsetY = e.Y - viewer.Y;
							break;
						}
					}
					break;

				case MouseButtons.Middle:
					IsBeingDragged = true;
					lastMouseDragPoint = new Point(e.X, e.Y);
					break;
			}
		}

		private void ProductionGraphViewer_MouseUp(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Left:
					foreach (ProductionNodeViewer viewer in nodeControls.Values)
					{
						viewer.IsBeingDragged = false;
					}
					break;

				case MouseButtons.Middle:
					IsBeingDragged = false;
					break;
			}
		}

		private void ProductionGraphViewer_MouseMove(object sender, MouseEventArgs e)
		{
			if (IsBeingDragged)
			{
				viewOffset = new Point(viewOffset.X + e.X - lastMouseDragPoint.X, viewOffset.Y + e.Y - lastMouseDragPoint.Y);
				lastMouseDragPoint = e.Location;
				Invalidate();
			}
			foreach (ProductionNodeViewer viewer in nodeControls.Values)
			{
				if (viewer.IsBeingDragged)
				{
					viewer.X = e.X - viewer.DragOffsetX;
					viewer.Y = e.Y - viewer.DragOffsetY;
					Invalidate();
					break;
				}
			}
		}

		private Point screenToGraph(int X, int Y)
		{
			return new Point(X + viewOffset.X, Y + viewOffset.Y);
		}
	}
}