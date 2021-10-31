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
		public virtual Point Location { get; set; }
		public virtual int X { get { return Location.X; } set { Location = new Point(value, Location.Y); } }
		public virtual int Y { get { return Location.Y; } set { Location = new Point(Location.X, value); } }
		public virtual Point Size { get; set; }
		public virtual int Width { get { return Size.X; } set { Size = new Point(value, Size.Y); } }
		public virtual int Height { get { return Size.Y; } set { Size = new Point(Size.X, value); } }
		public bool Visible { get; protected set; }
		public Rectangle bounds
		{
			get
			{
				return new Rectangle(X, Y, Width, Height);
			}
			set
			{
				X = value.X;
				Y = value.Y;
				Width = value.Width;
				Height = value.Height;
			}
		}
		public readonly ProductionGraphViewer Parent;

		public GraphElement(ProductionGraphViewer parent)
		{
			Parent = parent;
			Parent.Elements.Add(this);
			SubElements = new HashSet<GraphElement>();
			Visible = true;
		}

		public bool IntersectsWithZone(Rectangle zone, int xborder, int yborder)
        {
			return X + (Width / 2) > zone.X - xborder &&
						X - (Width / 2) < zone.X + zone.Width + xborder &&
						Y + (Height / 2) > zone.Y - yborder &&
						Y - (Height / 2) < zone.Y + zone.Height + yborder;
		}

		public virtual void UpdateVisibility(Rectangle zone, int xborder, int yborder)
        {
			Visible = IntersectsWithZone(zone, xborder, yborder);
        }

		public Point PT(Point original, int xTransform, int yTransform)
		{
			return new Point(original.X + xTransform, original.Y + yTransform);
		}
		public Point PT(Point original, Point transform)
		{
			return new Point(original.X + transform.X, original.Y + transform.Y);
		}

		public virtual bool ContainsPoint(Point point) { return false; }
		public virtual void Paint(Graphics graphics, Point transform)
		{
			foreach (GraphElement element in SubElements)
			{
				element.Paint(graphics, PT(transform, element.X, element.Y));
			}
		}

		//location is relative to this element
		public virtual List<TooltipInfo> GetToolTips(Point location) { return new List<TooltipInfo>(); }

		public virtual void MouseMoved(Point location) { }
		public virtual void MouseDown(Point location, MouseButtons button) { }
		public virtual void MouseUp(Point location, MouseButtons button, bool wasDragged) { }
		public virtual void Dragged(Point location) { }
		public virtual void Dispose()
		{
			Parent.Elements.Remove(this);
			Parent.Invalidate();
		}
	}
}
