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
		Dictionary<ProductionNode, ProductionNodeViewer> nodeControls = new Dictionary<ProductionNode, ProductionNodeViewer>();
		public ProductionGraph graph = new ProductionGraph();
		private List<Item> Demands = new List<Item>();
		
		public ProductionGraphViewer()
		{
			InitializeComponent();
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
				node.UpdateText();
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
					ProductionNodeViewer control = new ProductionNodeViewer(node);
					control.parentTreeViewer = this;
					Controls.Add(control);
					nodeControls.Add(node, control);
				}
			}
		}

		private void DrawConnections(Graphics graphics)
		{
			Pen pen = new Pen(Color.DarkRed, 3f);
			graphics.Clear(this.BackColor);

			foreach (var n in nodeControls.Keys)
			{
				foreach (var m in nodeControls.Keys)
				{
					if (m.CanTakeFrom(n))
					{
						Point pointN = Point.Add(nodeControls[n].Location, new Size(nodeControls[n].Width / 2, 0));
						Point pointM = Point.Add(nodeControls[m].Location, new Size(nodeControls[m].Width / 2, nodeControls[m].Height));
						graphics.DrawLine(pen, pointN, pointM);
					}
				}
			}

			pen.Dispose();
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

			foreach (ProductionNode node in nodeOrder)
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

			int y = 20;
			foreach (var list in nodePositions)
			{
				int maxHeight = 0;
				int x = 20;

				foreach (var node in list)
				{
					if (!nodeControls.ContainsKey(node))
					{
						continue;
					}
					UserControl control = nodeControls[node];
					control.Location = new Point(x, y);

					x += control.Width + 20;
					maxHeight = Math.Max(control.Height, maxHeight);
				}

				y += maxHeight + 20;
			}

			Invalidate(true);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			DrawConnections(e.Graphics);
		}
	}
}