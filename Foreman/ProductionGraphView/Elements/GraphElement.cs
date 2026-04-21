using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Foreman {
    public abstract class GraphElement : IDisposable {
        public List<GraphElement> SubElements { get; private set; }

        // bounds assumes 0,0 is the center of this element.
        // X,Y (or location) is the difference between this origin and the parent element origin
        // (NOT! the graph origin! -> this means that moving the parent element will not change the x,y of the child elements)
        // to simplify things, most point transformations make use of ConvertToLocal function
        // which goes through the entire element-subelement ownership to convert a graph point to local coordinates (for in-class use).
        // so any calls OUTSIDE a GraphElement assumes graph origin (0,0 is the center of the graph)
        // and any work inside is usually done with local origin (0,0 is the center of the element)
        // ex: mouse clicked at 10,10 on the graph. the first node (offset -5,-5) will get a call mouseClick(10,10)
        // which will internally be converted to local coordinate (15,15).
        // a pass of that mouse click to its sub node (offset -5,-5 further from the parent node)
        // will get a call mouseClick(10,10), which will internally be converted to local coordinate (20,20)
        public Rectangle Bounds => new(-Width / 2, -Height / 2, Width, Height);

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public Size Size {
            get => new(Width, Height);
            set {
                Width = value.Width;
                Height = value.Height;
            }
        }

        public virtual int X { get; set; }
        public virtual int Y { get; set; }

        // relative to parent element
        public virtual Point Location {
            get => new(X, Y);
            set {
                X = value.X;
                Y = value.Y;
            }
        }

        public bool Visible { get; protected set; }
        protected readonly ProductionGraphViewer GraphViewer;
        protected readonly GraphElement MyParent;

        protected ContextMenuStrip RightClickMenu;

        protected static readonly Pen DevPen = new(new SolidBrush(Color.OrangeRed), 1);

        public GraphElement(ProductionGraphViewer graphViewer, GraphElement parent = null) {
            GraphViewer = graphViewer;
            MyParent = parent;
            if (MyParent != null)
                parent.SubElements.Add(this);

            RightClickMenu = new ContextMenuStrip();
            RightClickMenu.ShowItemToolTips = false;
            RightClickMenu.ShowImageMargin = false;
            RightClickMenu.Closing += (o, e) => {
                // we will handle closing from item clicking within the items themselves

                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked) {
                    e.Cancel = true;
                } else {
                    RightClickMenu.Items.Clear();
                    RightClickMenu.ShowCheckMargin = false;
                }
            };

            SubElements = [];
            Visible = true;
        }

        // converts the point (in graph coordinates) to the local (0,0 is the center of this element's bound) point
        public Point GraphToLocal(Point graphPoint) {
            return Point.Subtract(MyParent?.GraphToLocal(graphPoint) ?? graphPoint, (Size) Location);
        }

        public Point LocalToGraph(Point localPoint) {
            return Point.Add(MyParent?.LocalToGraph(localPoint) ?? localPoint, (Size) Location);
        }

        public bool IntersectsWithZone(Rectangle graphZone, int xBorder, int yBorder) {
            var localGraphZoneOrigin = GraphToLocal(graphZone.Location);
            return Width / 2 > localGraphZoneOrigin.X - xBorder &&
                -(Width / 2) < localGraphZoneOrigin.X + graphZone.Width + xBorder &&
                Height / 2 > localGraphZoneOrigin.Y - yBorder &&
                -(Height / 2) < localGraphZoneOrigin.Y + graphZone.Height + yBorder;
        }

        public virtual void UpdateVisibility(Rectangle graphZone, int xBorder = 0, int yBorder = 0) {
            Visible = IntersectsWithZone(graphZone, xBorder, yBorder);
        }

        public virtual bool ContainsPoint(Point graphPoint) {
            return Visible && Bounds.Contains(GraphToLocal(graphPoint));
        }

        public virtual void PrePaint() { }

        public void Paint(Graphics graphics, NodeDrawingStyle style) {
            if (!Visible)
                return;

            // call own draw operation

            Draw(graphics, style);

            // call paint operations (this function) for each of the sub-elements owned by this element
            // (who will call their own draw and further paint operations on their own sub elements)

            foreach (var element in SubElements)
                element.Paint(graphics, style);
        }

        protected abstract void Draw(Graphics graphics, NodeDrawingStyle style);

        public virtual List<TooltipInfo> GetToolTips(Point graphPoint) {
            return [];
        }

        public virtual void MouseMoved(Point graphPoint) { }
        public virtual void MouseDown(Point graphPoint, MouseButtons button) { }
        public virtual void MouseUp(Point graphPoint, MouseButtons button, bool wasDragged) { }
        public virtual void Dragged(Point graphPoint) { }

        public virtual void Dispose() {
            foreach (var element in SubElements.ToArray())
                element.Dispose();
            SubElements.Clear();
            MyParent?.SubElements.Remove(this);

            RightClickMenu.Dispose();

            GraphViewer.Invalidate();
        }

        internal string BuildingQuantityToText(double quantity) {
            var text = "";
            if (quantity >= 10000)
                text += quantity.ToString("0.##e0");
            else if (Properties.Settings.Default.RoundAssemblerCount)
                text += Math.Ceiling(quantity).ToString("0");
            else if (quantity >= 0.1)
                text += quantity.ToString("0.#");
            else if (quantity != 0)
                text += "<0.1";
            else
                text += "0";
            return text;
        }
    }
}