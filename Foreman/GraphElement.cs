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
		}

		public virtual bool ContainsPoint(Point point) { return false; }
		public virtual void Paint(Graphics graphics)
		{
			foreach (GraphElement element in SubElements)
			{
				graphics.TranslateTransform(element.X, element.Y);
				element.Paint(graphics);
				graphics.TranslateTransform(-element.X, -element.Y);
			}
		}
		public virtual void MouseMoved(Point location) { }
		public virtual void MouseDown(Point location, MouseButtons button) { }
		public virtual void MouseUp(Point location, MouseButtons button) { }
		public virtual void Dragged(Point location) { }
		public virtual void Dispose()
		{
			Parent.Elements.Remove(this);
			Parent.Invalidate();
		}
	}
}
