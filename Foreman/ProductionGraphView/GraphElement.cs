using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman
{
	public abstract class GraphElement : IDisposable
	{
		public HashSet<GraphElement> SubElements { get; private set; }

		//bounds assumes 0,0 is the center of this element. X,Y (or location) is the difference between this origin and the parent element origin.
		//NOTE: any coordinate with "parent_" assumes parent coordinate system (this element's center is X,Y in parent coordinate system).
		//		anything with "local_" assumes current element's coordinate system (this element's center is 0,0 in local coordinate system).
		public Rectangle Bounds { get { return new Rectangle(-Width / 2, -Height / 2, Width, Height); } }
		public virtual int Width { get; set; }
		public virtual int Height { get; set; }
		public Size Size { get { return new Size(Width, Height); } set { Width = value.Width; Height = value.Height; } }
		public virtual int X { get; set; }
		public virtual int Y { get; set; }

		public virtual Point Location
		{
			get { return new Point(X, Y); }
			set { X = value.X; Y = value.Y; }
		}

		public virtual bool Visible { get; protected set; }
		protected readonly ProductionGraphViewer myGraphViewer;

		public GraphElement(ProductionGraphViewer parent)
		{
			myGraphViewer = parent;
			myGraphViewer.Elements.Add(this);
			SubElements = new HashSet<GraphElement>();
			Visible = true;
		}

		public bool IntersectsWithZone(Rectangle parent_zone, int xborder, int yborder)
        {
			return  X + (Width / 2) > parent_zone.X - xborder &&
					X - (Width / 2) < parent_zone.X + parent_zone.Width + xborder &&
					Y + (Height / 2) > parent_zone.Y - yborder &&
					Y - (Height / 2) < parent_zone.Y + parent_zone.Height + yborder;
		}

		public virtual void UpdateVisibility(Rectangle parent_zone, int xborder, int yborder)
        {
			Visible = IntersectsWithZone(parent_zone, xborder, yborder);
        }

		public virtual bool ContainsPoint(Point parent_point)
		{
			return Bounds.Contains(Point.Subtract(parent_point, (Size)Location));
		}

		public void Paint(Graphics graphics, Point parent_transform)
		{
			Point local_transform = Point.Add(parent_transform, (Size)Location); //update transform to this element's coordinate system.

			//call own draw operation
			Draw(graphics, local_transform);

			//call paint operations (this function) for each of the sub-elements owned by this element (who will call their own draw and further paint operations on their own sub elements)
			foreach (GraphElement element in SubElements)
				element.Paint(graphics, local_transform); //local_transform is then parent_transform for our sub-elements
		}

		protected abstract void Draw(Graphics graphics, Point local_transform);

		//location is local (0,0 is this element center)
		public virtual List<TooltipInfo> GetToolTips(Point local_point) { return new List<TooltipInfo>(); }
		public virtual void MouseMoved(Point local_point) { }
		public virtual void MouseDown(Point local_point, MouseButtons button) { }
		public virtual void MouseUp(Point local_point, MouseButtons button, bool wasDragged) { }
		public virtual void Dragged(Point local_point) { }

		public virtual void Dispose()
		{
			myGraphViewer.Elements.Remove(this);
			myGraphViewer.Invalidate();
		}
	}
}
