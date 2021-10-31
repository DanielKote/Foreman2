using SvgNet.SvgGdi;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman
{
	public abstract class GraphElement : IDisposable
	{
		public List<GraphElement> SubElements { get; private set; }

		//bounds assumes 0,0 is the center of this element. X,Y (or location) is the difference between this origin and the parent element origin (NOT! the graph origin! -> this means that moving the parent element will not change the x,y of the child elements)
		//to simplify things, most point transformations make use of ConvertToLocal funtion which goes through the entire element-subelement ownership to convert a graph point to local coordinates (for in-class use).
		//so any calls OUTSIDE a GraphElement assumes graph origin (0,0 is the center of the graph)
		//and any work inside is usually done with local origin (0,0 is the center of the element)
		//ex: mouse clicked at 10,10 on the graph. the first node (offset -5,-5) will get a call mouseClick(10,10) which will internally be converted to local coordinate (15,15).
		//		a pass of that mouse click to its sub node (offset -5,-5 further from the parent node) will get a call mouseClick(10,10), which will internally be converted to local coordinate (20,20)
		public Rectangle Bounds { get { return new Rectangle(-Width / 2, -Height / 2, Width, Height); } }
		public virtual int Width { get; set; }
		public virtual int Height { get; set; }
		public Size Size { get { return new Size(Width, Height); } set { Width = value.Width; Height = value.Height; } }

		public virtual int X { get; set; }
		public virtual int Y { get; set; }
		public virtual Point Location //relative to parent element
		{
			get { return new Point(X, Y); }
			set { X = value.X; Y = value.Y; }
		}

		public virtual bool Visible { get; protected set; }
		protected readonly ProductionGraphViewer graphViewer;
		protected readonly GraphElement myParent;

		protected ContextMenuStrip RightClickMenu;

		protected static readonly Pen devPen = new Pen(new SolidBrush(Color.OrangeRed), 1);

		public GraphElement(ProductionGraphViewer graphViewer, GraphElement parent = null)
		{
			this.graphViewer = graphViewer;
			myParent = parent;
			if (myParent != null)
				parent.SubElements.Add(this);

			RightClickMenu = new ContextMenuStrip();
			RightClickMenu.ShowItemToolTips = false;
			RightClickMenu.ShowImageMargin = false;
			RightClickMenu.Closing += (o, e) =>
			{
				if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
					e.Cancel = true; //we will handle closing from item clicking within the items themselves
				else
				{
					RightClickMenu.Items.Clear();
					RightClickMenu.ShowCheckMargin = false;
				}
			};

			SubElements = new List<GraphElement>();
			Visible = true;
		}

		public Point GraphToLocal(Point graph_point) //converts the point (in graph coordinates) to the local (0,0 is the center of this element's bound) point
		{
			if (myParent == null) //owned by graphViewer
				return Point.Subtract(graph_point, (Size)Location);
			else //subelement of some element
				return Point.Subtract(myParent.GraphToLocal(graph_point), (Size)Location);
		}

		public Point LocalToGraph(Point local_point)
		{
			if (myParent == null) //owned by graphViewer
				return Point.Add(local_point, (Size)Location);
			else //subelement of some element
				return Point.Add(myParent.LocalToGraph(local_point), (Size)Location);
		}

		public bool IntersectsWithZone(Rectangle graph_zone, int xborder, int yborder)
		{
			Point local_graph_zone_origin = GraphToLocal(graph_zone.Location);
			return (Width / 2) > local_graph_zone_origin.X - xborder &&
					-(Width / 2) < local_graph_zone_origin.X + graph_zone.Width + xborder &&
					 (Height / 2) > local_graph_zone_origin.Y - yborder &&
					-(Height / 2) < local_graph_zone_origin.Y + graph_zone.Height + yborder;
		}

		public virtual void UpdateVisibility(Rectangle graph_zone, int xborder = 0, int yborder = 0)
		{
			Visible = IntersectsWithZone(graph_zone, xborder, yborder);
		}

		public virtual bool ContainsPoint(Point graph_point)
		{
			if (!Visible)
				return false;
			return Bounds.Contains(GraphToLocal(graph_point));
		}

		public void Paint(IGraphics graphics, bool simple)
		{
			if (Visible)
			{
				//call own draw operation
				Draw(graphics, simple);

				//call paint operations (this function) for each of the sub-elements owned by this element (who will call their own draw and further paint operations on their own sub elements)
				foreach (GraphElement element in SubElements)
					element.Paint(graphics, simple);
			}
		}

		protected abstract void Draw(IGraphics graphics, bool simple);

		public virtual List<TooltipInfo> GetToolTips(Point graph_point) { return new List<TooltipInfo>(); }
		public virtual void MouseMoved(Point graph_point) { }
		public virtual void MouseDown(Point graph_point, MouseButtons button) { }
		public virtual void MouseUp(Point graph_point, MouseButtons button, bool wasDragged) { }
		public virtual void Dragged(Point graph_point) { }

		public virtual void Dispose()
		{
			foreach (GraphElement element in SubElements.ToArray())
				element.Dispose();
			SubElements.Clear();
			if (myParent != null)
				myParent.SubElements.Remove(this);

			RightClickMenu.Dispose();

			graphViewer.Invalidate();
		}
	}
}
