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
	public enum Direction { Up, Down, Left, Right }

	public partial class ProductionGraphViewer : UserControl
	{
		public Dictionary<ProductionNode, ProductionNodeViewer> nodeControls = new Dictionary<ProductionNode, ProductionNodeViewer>();
		public ProductionGraph graph = new ProductionGraph();
		private List<Item> Demands = new List<Item>();
		public bool IsBeingDragged { get; private set; }
		private Point lastMouseDragPoint;
		private Point mousePosition;
		public Point ViewOffset;
		public float ViewScale = 1f;
		public ProductionNodeViewer SelectedNode = null;
		public ProductionNodeViewer MousedNode = null;
		public ProductionNodeViewer ClickedNode = null;

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
				return new Rectangle(0, 0, width + 20, height + 20);
			}
		}
		
		public ProductionGraphViewer()
		{
			InitializeComponent();
			MouseWheel += new MouseEventHandler(ProductionGraphViewer_MouseWheel);
		}

		public void AddDemands(IEnumerable<Item> list)
		{
			foreach (Item item in list)
			{
				ConsumerNode node = ConsumerNode.Create(item, graph);
			}
			graph.SatisfyAllItemDemands();
			CreateMissingControls();

			foreach (ProductionNodeViewer node in nodeControls.Values)
			{
				node.Update();
			}
			PositionControls();
			Invalidate(true);
		}

		public void AddDemand(Item item)
		{
			AddDemands(new List<Item> { item });
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
				foreach (NodeLink link in n.InputLinks)
				{
					Point pointN = nodeControls[link.Supplier].getOutputLineConnectionPoint(link.Item);
					Point pointM = nodeControls[link.Consumer].getInputLineConnectionPoint(link.Item);
					Point pointN2 = new Point(pointN.X, pointN.Y - Math.Max((int)((pointN.Y - pointM.Y) / 2), 40));
					Point pointM2 = new Point(pointM.X, pointM.Y + Math.Max((int)((pointN.Y - pointM.Y) / 2), 40));

					using (Pen pen = new Pen(DataCache.IconAverageColour(link.Item.Icon), 3f))
					{
						graphics.DrawBezier(pen, pointN, pointN2, pointM2, pointM);
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

			int marginX = 100;
			int marginY = 200;
			int y = marginY;
			int[] tierWidths = new int[nodePositions.Count()];
			for (int i = 0; i < nodePositions.Count(); i++)
			{
				var list = nodePositions[i];
				int maxHeight = 0;
				int x = marginX;

				foreach (var node in list)
				{
					if (!nodeControls.ContainsKey(node))
					{
						continue;
					}
					ProductionNodeViewer control = nodeControls[node];
					control.X = x;
					control.Y = y;

					x += control.Width + marginX;
					maxHeight = Math.Max(control.Height, maxHeight);
				}

				if (maxHeight > 0) // Don't add any height for empty tiers
				{
					y += maxHeight + marginY;
				}

				tierWidths[i] = x;
			}

			int centrePoint = tierWidths.Last(i => i > marginX) / 2;
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
			e.Graphics.ResetTransform();
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			e.Graphics.Clear(this.BackColor);
			e.Graphics.TranslateTransform(ViewOffset.X, ViewOffset.Y);
			e.Graphics.ScaleTransform(ViewScale, ViewScale);

			DrawConnections(e.Graphics);

			foreach (ProductionNodeViewer viewer in nodeControls.Values)
			{
				e.Graphics.TranslateTransform(viewer.X, viewer.Y);
				viewer.Paint(e.Graphics);
				e.Graphics.TranslateTransform(-viewer.X, -viewer.Y);
			}
			
			if (MousedNode != null)
			{
				Point offsetMousePosition = Point.Add(screenToGraph(mousePosition.X, mousePosition.Y), new Size(-MousedNode.X, -MousedNode.Y));
				foreach (Item item in MousedNode.DisplayedNode.Inputs)
				{
					if (MousedNode.GetIconBounds(item, LinkType.Input).Contains(offsetMousePosition))
					{
						DrawTooltip(MousedNode.getInputLineConnectionPoint(item), item.Name, Direction.Up, e.Graphics);
					}
				}
				foreach (Item item in MousedNode.DisplayedNode.Outputs)
				{
					if (MousedNode.GetIconBounds(item, LinkType.Output).Contains(offsetMousePosition))
					{
						DrawTooltip(MousedNode.getOutputLineConnectionPoint(item), item.Name, Direction.Down, e.Graphics);
					}
				}
			}
		}

		private void DrawTooltip(Point point, String text, Direction direction, Graphics graphics)
		{
			int border = 2;
			Font font = new Font(FontFamily.GenericSansSerif, 10);
			int arrowSize = 10;
			SizeF stringSize = graphics.MeasureString(text, font);
			Point centreOffset = new Point();
			Point arrowPoint1 = new Point();
			Point arrowPoint2 = new Point();

			switch (direction){
				case Direction.Down:
					centreOffset = new Point(0, -arrowSize - (int)stringSize.Height / 2);
					arrowPoint1 = new Point(point.X - arrowSize / 2, point.Y - arrowSize);
					arrowPoint2 = new Point(point.X + arrowSize / 2, point.Y - arrowSize);
					break;
				case Direction.Left:
					centreOffset = new Point(arrowSize + (int)stringSize.Width / 2, 0);
					arrowPoint1 = new Point(point.X - arrowSize, point.Y - arrowSize / 2);
					arrowPoint1 = new Point(point.X - arrowSize, point.Y + arrowSize / 2);
					break;
				case Direction.Up:
					centreOffset = new Point(0, arrowSize + (int)stringSize.Height / 2);
					arrowPoint1 = new Point(point.X - arrowSize / 2, point.Y + arrowSize);
					arrowPoint2 = new Point(point.X + arrowSize / 2, point.Y + arrowSize);
					break;
				case Direction.Right:
					centreOffset = new Point(-arrowSize - (int)stringSize.Width / 2, 0);
					arrowPoint1 = new Point(point.X + arrowSize, point.Y - arrowSize / 2);
					arrowPoint1 = new Point(point.X + arrowSize, point.Y + arrowSize / 2);
					break;
			}

			int X = (int)(point.X + centreOffset.X - stringSize.Width / 2) - border;
			int Y = (int)(point.Y + centreOffset.Y - stringSize.Height / 2) - border;
			int Width = (int)stringSize.Width + border * 2;
			int Height = (int)stringSize.Height + border * 2;

			Point[] points = new Point[]{point, arrowPoint1, arrowPoint2};
			graphics.FillPolygon(Brushes.DarkGray, points); 
			GraphicsStuff.FillRoundRect(X, Y, Width, Height, 3, graphics, Brushes.DarkGray);

			StringFormat centreFormat = new StringFormat();
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;
			graphics.DrawString(text, font, Brushes.White, Point.Add(point, new Size(centreOffset)), centreFormat);
		}

		private ProductionNodeViewer getNodeAtScreenPoint(Point point)
		{
			foreach (ProductionNodeViewer node in nodeControls.Values)
			{
				if (node.bounds.Contains(screenToGraph(point.X, point.Y)))
				{
					return node;
				}
				foreach (Item item in node.DisplayedNode.Inputs)
				{
					Rectangle iconBounds = node.GetIconBounds(item, LinkType.Input);
					iconBounds.Offset(node.X, node.Y);
					if (iconBounds.Contains(screenToGraph(point.X, point.Y)))
					{
						return node;
					}
				}
				foreach (Item item in node.DisplayedNode.Outputs)
				{
					Rectangle iconBounds = node.GetIconBounds(item, LinkType.Output);
					iconBounds.Offset(node.X, node.Y);
					if (iconBounds.Contains(screenToGraph(point.X, point.Y)))
					{
						return node;
					}
				}
			}
			return null;
		}		
		
		private void ProductionGraphViewer_MouseDown(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Left:
					var node = getNodeAtScreenPoint(e.Location);
					if (node != null)
					{
						node.IsBeingDragged = true;
						node.DragOffsetX = screenToGraph(e.X, 0).X - node.X;
						node.DragOffsetY = screenToGraph(0, e.Y).Y - node.Y;
						ClickedNode = node;
					}
					break;

				case MouseButtons.Middle:
					IsBeingDragged = true;
					lastMouseDragPoint = new Point(e.X, e.Y);
					break;
			}

			Invalidate();
		}

		private void ProductionGraphViewer_MouseUp(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Left:
					SelectedNode = getNodeAtScreenPoint(e.Location);
					ClickedNode = null;
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
			MousedNode = getNodeAtScreenPoint(e.Location);
			mousePosition = e.Location;

			if (IsBeingDragged)
			{
				SelectedNode = getNodeAtScreenPoint(e.Location);
				ViewOffset = new Point(ViewOffset.X + e.X - lastMouseDragPoint.X, ViewOffset.Y + e.Y - lastMouseDragPoint.Y);
				lastMouseDragPoint = e.Location;
				Invalidate();
			}
			foreach (ProductionNodeViewer node in nodeControls.Values)
			{
				if (node.IsBeingDragged)
				{
					node.X = screenToGraph(e.X, 0).X - node.DragOffsetX;
					node.Y = screenToGraph(0, e.Y).Y - node.DragOffsetY;
					Invalidate();
					break;
				}
			}
			
			Invalidate();
		}

		void ProductionGraphViewer_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta > 0)
			{
				ViewScale *= 1.1f;
			}
			else
			{
				ViewScale /= 1.1f;
			}
			Invalidate();
		}

		private Point screenToGraph(int X, int Y)
		{
			return new Point(Convert.ToInt32((X - ViewOffset.X) / ViewScale), Convert.ToInt32((Y - ViewOffset.Y) / ViewScale));
		}

	}
}