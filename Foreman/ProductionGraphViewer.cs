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
	public struct TooltipInfo
	{
		public TooltipInfo(Point screenLocation, Point screenSize, Direction direction, String text)
		{
			ScreenLocation = screenLocation;
			ScreenSize = screenSize;
			Direction = direction;
			Text = text;
		}

		public Point ScreenLocation;
		public Point ScreenSize;
		public Direction Direction;
		public String Text;
	}

	public partial class ProductionGraphViewer : UserControl
	{
		public Dictionary<ProductionNode, ProductionNodeViewer> nodeControls = new Dictionary<ProductionNode, ProductionNodeViewer>();
		public ProductionGraph graph = new ProductionGraph();
		private List<Item> Demands = new List<Item>();
		public bool IsBeingDragged { get; private set; }
		private Point lastMouseDragPoint;
		public Point ViewOffset;
		public float ViewScale = 1f;
		public ProductionNodeViewer SelectedNode = null;
		public ProductionNodeViewer MousedNode = null;
		public ProductionNodeViewer ClickedNode = null;
		public Queue<TooltipInfo> toolTipsToDraw = new Queue<TooltipInfo>();

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

			CreateMissingControls();
		}

		public void AddDemand(Item item)
		{
			AddDemands(new List<Item> { item });
		}

		public void CreateMissingControls()
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

			foreach (ProductionNodeViewer node in nodeControls.Values)
			{
				node.Update();
			}
			PositionControls();
			Invalidate();
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
			
			e.Graphics.ResetTransform();
			while (toolTipsToDraw.Any())
			{
				var tt = toolTipsToDraw.Dequeue();

				if (tt.Text != null)
				{
					DrawTooltip(tt.ScreenLocation, tt.Text, tt.Direction, e.Graphics);
				}
				else
				{
					DrawTooltip(tt.ScreenLocation, tt.ScreenSize, tt.Direction, e.Graphics);
				}
			}
		}

		private void DrawTooltip(Point point, String text, Direction direction, Graphics graphics)
		{
			Font font = new Font(FontFamily.GenericSansSerif, 10);
			SizeF stringSize = graphics.MeasureString(text, font);
			DrawTooltip(point, new Point((int)stringSize.Width, (int)stringSize.Height), direction, graphics, text);
		}
		
		private void DrawTooltip(Point screenArrowPoint, Point screenSize, Direction direction, Graphics graphics, String text = "")
		{
			Font font = new Font(FontFamily.GenericSansSerif, 10);
			int border = 2;
			int arrowSize = 10;
			Point arrowPoint1 = new Point();
			Point arrowPoint2 = new Point();

			switch (direction){
				case Direction.Down:
					arrowPoint1 = new Point(screenArrowPoint.X - arrowSize / 2, screenArrowPoint.Y - arrowSize);
					arrowPoint2 = new Point(screenArrowPoint.X + arrowSize / 2, screenArrowPoint.Y - arrowSize);
					break;
				case Direction.Left:
					arrowPoint1 = new Point(screenArrowPoint.X - arrowSize, screenArrowPoint.Y - arrowSize / 2);
					arrowPoint1 = new Point(screenArrowPoint.X - arrowSize, screenArrowPoint.Y + arrowSize / 2);
					break;
				case Direction.Up:
					arrowPoint1 = new Point(screenArrowPoint.X - arrowSize / 2, screenArrowPoint.Y + arrowSize);
					arrowPoint2 = new Point(screenArrowPoint.X + arrowSize / 2, screenArrowPoint.Y + arrowSize);
					break;
				case Direction.Right:
					arrowPoint1 = new Point(screenArrowPoint.X + arrowSize, screenArrowPoint.Y - arrowSize / 2);
					arrowPoint1 = new Point(screenArrowPoint.X + arrowSize, screenArrowPoint.Y + arrowSize / 2);
					break;
			}

			Rectangle rect = getTooltipScreenBounds(screenArrowPoint, screenSize, direction);

			Point[] points = new Point[]{screenArrowPoint, arrowPoint1, arrowPoint2};
			graphics.FillPolygon(Brushes.DarkGray, points); 
			GraphicsStuff.FillRoundRect(rect.X - border, rect.Y - border, rect.Width + border * 2, rect.Height + border * 2, 3, graphics, Brushes.DarkGray);

			StringFormat centreFormat = new StringFormat();
			centreFormat.Alignment = centreFormat.LineAlignment = StringAlignment.Center;
			graphics.DrawString(text, font, Brushes.White, new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2), centreFormat);
		}

		public Rectangle getTooltipScreenBounds(Point screenArrowPoint, Point screenSize, Direction direction)
		{
			Point centreOffset = new Point();
			int arrowSize = 10;

			switch (direction)
			{
				case Direction.Down:
					centreOffset = new Point(0, -arrowSize - screenSize.Y / 2);
					break;
				case Direction.Left:
					centreOffset = new Point(arrowSize + screenSize.X / 2, 0);
					break;
				case Direction.Up:
					centreOffset = new Point(0, arrowSize + screenSize.Y / 2);
					break;
				case Direction.Right:
					centreOffset = new Point(-arrowSize - screenSize.X / 2, 0);
					break;
			}
			int X = (screenArrowPoint.X + centreOffset.X - screenSize.X / 2);
			int Y = (screenArrowPoint.Y + centreOffset.Y - screenSize.Y / 2);
			int Width = screenSize.X;
			int Height = screenSize.Y;

			return new Rectangle(X, Y, Width, Height);
		}

		//Takes a graph point as an argument
		private ProductionNodeViewer getNodeAtPoint(Point point)
		{
			foreach (ProductionNodeViewer node in nodeControls.Values)
			{
				if (node.bounds.Contains(point.X, point.Y))
				{
					return node;
				}
				foreach (Item item in node.DisplayedNode.Inputs)
				{
					Rectangle iconBounds = node.GetIconBounds(item, LinkType.Input);
					iconBounds.Offset(node.X, node.Y);
					if (iconBounds.Contains(point.X, point.Y))
					{
						return node;
					}
				}
				foreach (Item item in node.DisplayedNode.Outputs)
				{
					Rectangle iconBounds = node.GetIconBounds(item, LinkType.Output);
					iconBounds.Offset(node.X, node.Y);
					if (iconBounds.Contains(point.X, point.Y))
					{
						return node;
					}
				}
			}
			return null;
		}		
		
		private void ProductionGraphViewer_MouseDown(object sender, MouseEventArgs e)
		{
			var node = getNodeAtPoint(screenToGraph(e.Location));
			if (node != null)
			{
				NodeMouseDown(node, screenToGraph(e.Location), e.Button);
			}

			switch (e.Button)
			{
				case MouseButtons.Middle:
					IsBeingDragged = true;
					lastMouseDragPoint = new Point(e.X, e.Y);
					break;
			}

			Invalidate();
		}

		//Takes a location on the graph, not the screen
		private void NodeMouseDown(ProductionNodeViewer node, Point location, MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				node.IsBeingDragged = true;
				node.DragOffsetX = location.X - node.X;
				node.DragOffsetY = location.Y - node.Y;
				ClickedNode = node;
			}

			node.MouseDown(Point.Add(location, new Size(-node.X, -node.Y)), button);
		}
		private void NodeMouseUp(ProductionNodeViewer node, Point location, MouseButtons button)
		{
			if (button == MouseButtons.Left)
			{
				SelectedNode = node;
				ClickedNode = null;
				foreach (ProductionNodeViewer otherNode in nodeControls.Values)
				{
					otherNode.IsBeingDragged = false;
				}
			}

			node.MouseUp(Point.Add(location, new Size(-node.X, -node.Y)), button);
		}

		private void ProductionGraphViewer_MouseUp(object sender, MouseEventArgs e)
		{
			var node = getNodeAtPoint(screenToGraph(e.Location));
			if (node != null)
			{
				NodeMouseUp(node, screenToGraph(e.Location), e.Button);
			}

			switch (e.Button)
			{
				case MouseButtons.Middle:
					IsBeingDragged = false;
					break;
			}
		}

		private void ProductionGraphViewer_MouseMove(object sender, MouseEventArgs e)
		{
			MousedNode = getNodeAtPoint(screenToGraph(e.Location));

			if (MousedNode != null)
			{
				MousedNode.MouseMoved(Point.Add(screenToGraph(e.Location), new Size(-MousedNode.X, -MousedNode.Y)));
			}

			if (IsBeingDragged)
			{
				ViewOffset = new Point(ViewOffset.X + e.X - lastMouseDragPoint.X, ViewOffset.Y + e.Y - lastMouseDragPoint.Y);
				foreach (ProductionNodeViewer node in nodeControls.Values)
				{
					node.GraphViewMoved();
				}

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

			foreach (var node in nodeControls.Values)
			{
				node.GraphViewMoved();
			}

			Invalidate();
		}

		public Point screenToGraph(Point point)
		{
			return screenToGraph(point.X, point.Y);
		}

		public Point screenToGraph(int X, int Y)
		{
			return new Point(Convert.ToInt32((X - ViewOffset.X) / ViewScale), Convert.ToInt32((Y - ViewOffset.Y) / ViewScale));
		}

		public Point graphToScreen(Point point)
		{
			return graphToScreen(point.X, point.Y);
		}

		public Point graphToScreen(int X, int Y)
		{
			return new Point(Convert.ToInt32((X * ViewScale) + ViewOffset.X), Convert.ToInt32((Y * ViewScale) + ViewOffset.Y));
		}

		//Tooltips added with this method will be drawn the next time the graph is repainted.
		public void AddTooltip(TooltipInfo info)
		{
			toolTipsToDraw.Enqueue(info);
		}

		private void ProductionGraphViewer_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				DeleteNode(SelectedNode);
			}
		}

		public void DeleteNode(ProductionNodeViewer node)
		{
			if (node != null)
			{
				node.DisplayedNode.Destroy();
				nodeControls.Remove(node.DisplayedNode);
				graph.UpdateNodeAmounts();
				Invalidate();
			}
		}
	}
}